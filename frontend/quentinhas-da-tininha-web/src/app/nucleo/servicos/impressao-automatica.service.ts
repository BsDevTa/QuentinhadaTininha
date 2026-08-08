import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ImpressoesPedidosAdministrativoService } from '../../administrativo/servicos/impressoes-pedidos-administrativo.service';
import { ImpressaoPedidoPendente } from '../modelos/pedido-impressao.model';
import { ImpressaoTermicaService } from './impressao-termica.service';

const INTERVALO_POLLING_MS = 5000;
const LIMITE_PENDENTES = 10;

@Injectable({ providedIn: 'root' })
export class ImpressaoAutomaticaService {
  private readonly api = inject(ImpressoesPedidosAdministrativoService);
  private readonly impressaoTermicaService = inject(ImpressaoTermicaService);
  private readonly fila: ImpressaoPedidoPendente[] = [];
  private readonly idsNaFila = new Set<string>();
  private timer: ReturnType<typeof setInterval> | null = null;
  private pollingEmAndamento = false;
  private processandoFila = false;

  readonly ativo = signal(false);
  readonly qzDisponivel = signal(false);
  readonly imprimindo = signal(false);
  readonly aguardando = signal(0);
  readonly ultimoErro = signal<string | null>(null);
  readonly descricaoStatus = computed(() => {
    if (!this.ativo()) {
      return 'Impressao automatica parada';
    }

    if (!this.qzDisponivel()) {
      return 'Impressao automatica indisponivel - QZ Tray desconectado';
    }

    if (this.imprimindo()) {
      return 'Imprimindo pedido';
    }

    return this.aguardando() > 0
      ? `${this.aguardando()} pedidos aguardando impressao`
      : 'Impressora online';
  });

  iniciar(): void {
    if (this.timer) {
      return;
    }

    this.ativo.set(true);
    void this.executarCiclo();
    this.timer = setInterval(() => {
      void this.executarCiclo();
    }, INTERVALO_POLLING_MS);
  }

  parar(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }

    this.ativo.set(false);
    this.qzDisponivel.set(false);
    this.imprimindo.set(false);
    this.aguardando.set(0);
    this.fila.length = 0;
    this.idsNaFila.clear();
  }

  private async executarCiclo(): Promise<void> {
    if (!this.ativo() || this.pollingEmAndamento) {
      return;
    }

    this.pollingEmAndamento = true;

    try {
      await this.impressaoTermicaService.conectar();
      this.qzDisponivel.set(this.impressaoTermicaService.estaConectado());
    } catch (erro: unknown) {
      this.qzDisponivel.set(false);
      this.ultimoErro.set(this.normalizarErro(erro));
      this.pollingEmAndamento = false;
      return;
    }

    try {
      const pendentes = await firstValueFrom(this.api.listarPendentes(LIMITE_PENDENTES));
      pendentes.forEach((impressao) => this.enfileirar(impressao));
      void this.processarFila();
    } catch (erro: unknown) {
      this.ultimoErro.set(this.normalizarErro(erro));
    } finally {
      this.pollingEmAndamento = false;
    }
  }

  private enfileirar(impressao: ImpressaoPedidoPendente): void {
    if (this.idsNaFila.has(impressao.id)) {
      return;
    }

    this.idsNaFila.add(impressao.id);
    this.fila.push(impressao);
    this.aguardando.set(this.fila.length);
  }

  private async processarFila(): Promise<void> {
    if (this.processandoFila) {
      return;
    }

    this.processandoFila = true;

    while (this.ativo() && this.fila.length) {
      const impressao = this.fila.shift()!;
      this.aguardando.set(this.fila.length);

      try {
        const reservada = await this.reservar(impressao);
        if (!reservada) {
          continue;
        }

        this.imprimindo.set(true);
        await this.impressaoTermicaService.imprimirPedido(
          reservada.pedido,
          { reimpressao: reservada.reimpressao }
        );
        await firstValueFrom(this.api.concluir(reservada.id));
        this.ultimoErro.set(null);
      } catch (erro: unknown) {
        await this.registrarFalha(impressao.id, erro);
      } finally {
        this.imprimindo.set(false);
        this.idsNaFila.delete(impressao.id);
      }
    }

    this.processandoFila = false;
  }

  private async reservar(impressao: ImpressaoPedidoPendente): Promise<ImpressaoPedidoPendente | null> {
    try {
      return await firstValueFrom(this.api.iniciar(impressao.id));
    } catch (erro: unknown) {
      if (erro instanceof HttpErrorResponse && erro.status === 409) {
        return null;
      }

      throw erro;
    }
  }

  private async registrarFalha(impressaoId: string, erro: unknown): Promise<void> {
    const mensagem = this.normalizarErro(erro);
    this.ultimoErro.set(mensagem);

    try {
      await firstValueFrom(this.api.registrarErro(impressaoId, mensagem));
    } catch {
      // Se o claim nao aconteceu ou outro processo assumiu, o backend rejeita e seguimos para o proximo polling.
    }
  }

  private normalizarErro(erro: unknown): string {
    if (erro instanceof Error) {
      return erro.message.slice(0, 500);
    }

    return 'Nao foi possivel processar a impressao automatica.';
  }
}
