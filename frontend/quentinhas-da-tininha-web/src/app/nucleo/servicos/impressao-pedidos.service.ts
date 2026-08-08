import { Injectable, signal } from '@angular/core';
import { PedidoImpressao, StatusImpressaoPedido } from '../modelos/pedido-impressao.model';
import { ImpressaoTermicaService } from './impressao-termica.service';

const CHAVE_PEDIDOS_IMPRESSOS = 'quentinhas.pedidos-impressos';

interface CachePedidosImpressos {
  data: string;
  pedidos: Record<string, string>;
}

interface TrabalhoImpressao {
  pedido: PedidoImpressao;
  resolve: () => void;
  reject: (erro: unknown) => void;
}

@Injectable({ providedIn: 'root' })
export class ImpressaoPedidosService {
  private readonly pedidosImpressos = new Set<string>();
  private readonly pedidosBaseline = new Set<string>();
  private readonly pedidosNaFila = new Set<string>();
  private readonly fila: TrabalhoImpressao[] = [];
  private baselineInicializado = false;
  private processando = false;

  readonly statuses = signal<Record<string, StatusImpressaoPedido>>({});
  readonly ultimoErro = signal<string | null>(null);

  constructor(private readonly impressaoTermicaService: ImpressaoTermicaService) {
    this.carregarCacheDiario();
  }

  registrarPedidosRecebidos(pedidos: PedidoImpressao[]): void {
    this.garantirCacheDoDia();
    const pedidosOrdenados = this.ordenarPorCriacao(pedidos);

    if (!this.baselineInicializado) {
      pedidosOrdenados.forEach((pedido) => this.pedidosBaseline.add(pedido.id));
      this.baselineInicializado = true;
      return;
    }

    pedidosOrdenados.forEach((pedido) => {
      if (this.deveImprimirAutomaticamente(pedido)) {
        void this.enfileirar(pedido, false).catch(() => undefined);
      }
    });
  }

  reimprimir(pedido: PedidoImpressao): Promise<void> {
    return this.enfileirar(pedido, true);
  }

  tentarPendentes(pedidos: PedidoImpressao[]): void {
    this.ordenarPorCriacao(pedidos).forEach((pedido) => {
      const status = this.statuses()[pedido.id];
      if ((status === 'aguardando' || status === 'erro') && !this.pedidosImpressos.has(pedido.id)) {
        void this.enfileirar(pedido, false).catch(() => undefined);
      }
    });
  }

  obterStatus(pedidoId: string): StatusImpressaoPedido | null {
    return this.statuses()[pedidoId] ?? (this.pedidosImpressos.has(pedidoId) ? 'impresso' : null);
  }

  foiImpressoAutomaticamente(pedidoId: string): boolean {
    return this.pedidosImpressos.has(pedidoId);
  }

  private deveImprimirAutomaticamente(pedido: PedidoImpressao): boolean {
    if (!pedido.id || this.pedidosBaseline.has(pedido.id) || this.pedidosImpressos.has(pedido.id)) {
      return false;
    }

    const status = this.statuses()[pedido.id];
    return !this.pedidosNaFila.has(pedido.id) && status !== 'imprimindo' && status !== 'erro';
  }

  private enfileirar(pedido: PedidoImpressao, manual: boolean): Promise<void> {
    if (!manual && !this.deveImprimirAutomaticamente(pedido)) {
      return Promise.resolve();
    }

    if (this.pedidosNaFila.has(pedido.id)) {
      return Promise.resolve();
    }

    this.atualizarStatus(pedido.id, 'aguardando');
    this.pedidosNaFila.add(pedido.id);

    const promessa = new Promise<void>((resolve, reject) => {
      this.fila.push({ pedido, resolve, reject });
    });

    void this.processarFila();
    return promessa;
  }

  private async processarFila(): Promise<void> {
    if (this.processando) {
      return;
    }

    this.processando = true;

    while (this.fila.length) {
      const trabalho = this.fila.shift()!;
      const { pedido, resolve, reject } = trabalho;

      try {
        this.atualizarStatus(pedido.id, 'imprimindo');
        await this.impressaoTermicaService.imprimirPedido(pedido);
        this.marcarImpresso(pedido.id);
        this.atualizarStatus(pedido.id, 'impresso');
        this.ultimoErro.set(null);
        resolve();
      } catch (erro: unknown) {
        this.atualizarStatus(pedido.id, 'erro');
        this.ultimoErro.set(erro instanceof Error ? erro.message : 'Nao foi possivel imprimir o pedido.');
        reject(erro);
      } finally {
        this.pedidosNaFila.delete(pedido.id);
      }
    }

    this.processando = false;
  }

  private marcarImpresso(pedidoId: string): void {
    this.pedidosImpressos.add(pedidoId);
    this.salvarCache();
  }

  private atualizarStatus(pedidoId: string, status: StatusImpressaoPedido): void {
    this.statuses.update((statuses) => ({
      ...statuses,
      [pedidoId]: status
    }));
  }

  private carregarCacheDiario(): void {
    const hoje = this.dataLocal();
    const cache = this.lerCache();

    if (!cache || cache.data !== hoje) {
      this.salvarCache({ data: hoje, pedidos: {} });
      return;
    }

    Object.keys(cache.pedidos).forEach((pedidoId) => this.pedidosImpressos.add(pedidoId));
  }

  private garantirCacheDoDia(): void {
    const hoje = this.dataLocal();
    const cache = this.lerCache();

    if (!cache || cache.data !== hoje) {
      this.pedidosImpressos.clear();
      this.salvarCache({ data: hoje, pedidos: {} });
    }
  }

  private lerCache(): CachePedidosImpressos | null {
    try {
      const valor = localStorage.getItem(CHAVE_PEDIDOS_IMPRESSOS);
      return valor ? JSON.parse(valor) as CachePedidosImpressos : null;
    } catch {
      return null;
    }
  }

  private salvarCache(cache?: CachePedidosImpressos): void {
    const payload = cache ?? {
      data: this.dataLocal(),
      pedidos: Object.fromEntries(
        Array.from(this.pedidosImpressos).map((pedidoId) => [pedidoId, new Date().toISOString()])
      )
    };

    localStorage.setItem(CHAVE_PEDIDOS_IMPRESSOS, JSON.stringify(payload));
  }

  private ordenarPorCriacao(pedidos: PedidoImpressao[]): PedidoImpressao[] {
    return [...pedidos].sort((a, b) => Date.parse(a.criadoEm) - Date.parse(b.criadoEm));
  }

  private dataLocal(): string {
    const agora = new Date();
    const ano = agora.getFullYear();
    const mes = String(agora.getMonth() + 1).padStart(2, '0');
    const dia = String(agora.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }
}
