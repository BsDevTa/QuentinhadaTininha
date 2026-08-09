import { Injectable, inject, signal } from '@angular/core';
import qz from 'qz-tray';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { QzSigningAdministrativoService } from '../../administrativo/servicos/qz-signing-administrativo.service';
import { PedidoImpressao, PedidoItemImpressao } from '../modelos/pedido-impressao.model';

const NOME_IMPRESSORA_PADRAO = 'HPRT MPT-II';
const LARGURA_CUPOM_58MM = 32;

type EstadoQz = 'desconectado' | 'conectando' | 'conectado';

@Injectable({ providedIn: 'root' })
export class ImpressaoTermicaService {
  private readonly qzSigningService = inject(QzSigningAdministrativoService);
  private readonly qzConectadoInterno = signal(false);
  private estado: EstadoQz = 'desconectado';
  private conexaoPromise?: Promise<void>;
  private callbacksRegistrados = false;
  private segurancaConfigurada = false;
  private segurancaPromise?: Promise<void>;

  readonly qzConectado = this.qzConectadoInterno.asReadonly();

  constructor() {
    this.registrarCallbacksQz();
  }

  estaConectado(): boolean {
    const conectado = this.estado === 'conectado' && qz.websocket.isActive();
    this.qzConectadoInterno.set(conectado);
    return conectado;
  }

  async conectar(): Promise<void> {
    console.info(`[QZ] estado antes: ${this.estado}; isActive antes: ${qz.websocket.isActive()}`);
    await this.configurarSeguranca();

    if (this.estado === 'conectado' && qz.websocket.isActive()) {
      console.info('[QZ] conexao ja ativa');
      return;
    }

    if (this.conexaoPromise) {
      console.info('[QZ] conexao em andamento; reutilizando Promise');
      return this.conexaoPromise;
    }

    this.estado = 'conectando';

    this.conexaoPromise = (async () => {
      console.info('[QZ] conectando...');

      try {
        await qz.websocket.connect();

        if (!qz.websocket.isActive()) {
          throw new Error('QZ connect resolveu, mas websocket nao esta ativo.');
        }

        const info = qz.websocket.getConnectionInfo();
        this.estado = 'conectado';
        this.qzConectadoInterno.set(true);
        console.info('[QZ] conectado', info);
      } catch (erro: unknown) {
        this.estado = 'desconectado';
        this.qzConectadoInterno.set(false);
        console.warn(`[QZ] falha ao conectar: ${this.mensagemErro(erro)}`);
        throw this.normalizarErro(erro, 'conexao');
      } finally {
        this.conexaoPromise = undefined;
      }
    })();

    return this.conexaoPromise;
  }

  async conectarQzTeste(): Promise<boolean> {
    await this.conectar();

    const info = qz.websocket.getConnectionInfo();
    console.info('[QZ TESTE]', {
      conectado: true,
      host: info.host,
      port: info.port,
      socket: info.socket
    });

    return true;
  }

  async buscarImpressora(nome = NOME_IMPRESSORA_PADRAO): Promise<string> {
    await this.conectar();
    await this.configurarSeguranca();

    try {
      const impressoraExata = await qz.printers.find(nome);
      if (typeof impressoraExata === 'string' && impressoraExata.trim()) {
        return impressoraExata;
      }
    } catch {
      // A busca exata pode falhar mesmo com a impressora instalada; tentamos a lista completa abaixo.
    }

    const impressoras = await this.listarImpressoras();
    const impressoraEncontrada = this.localizarHprt(impressoras);

    if (!impressoraEncontrada) {
      throw new Error('Impressora HPRT MPT-II nao encontrada. Verifique se ela esta ligada e instalada no Windows.');
    }

    return impressoraEncontrada;
  }

  async imprimirTeste(): Promise<void> {
    try {
      const nomeImpressora = await this.buscarImpressora();
      const config = qz.configs.create(nomeImpressora);
      const dados = this.montarCupomTeste(nomeImpressora);

      await this.configurarSeguranca();
      await qz.print(config, [
        {
          type: 'raw',
          format: 'command',
          data: dados
        }
      ]);
    } catch (erro: unknown) {
      throw this.normalizarErro(erro, 'impressao');
    }
  }

  async imprimirPedido(
    pedido: PedidoImpressao,
    opcoes: { reimpressao?: boolean } = {}
  ): Promise<void> {
    try {
      const nomeImpressora = await this.buscarImpressora();
      const config = qz.configs.create(nomeImpressora);
      const dados = this.montarCupomPedido(pedido, opcoes);

      await this.configurarSeguranca();
      await qz.print(config, [
        {
          type: 'raw',
          format: 'command',
          data: dados
        }
      ]);
    } catch (erro: unknown) {
      throw this.normalizarErro(erro, 'impressao');
    }
  }

  private async listarImpressoras(): Promise<string[]> {
    const resultado = await qz.printers.find();

    if (Array.isArray(resultado)) {
      return resultado;
    }

    return resultado ? [resultado] : [];
  }

  private localizarHprt(impressoras: string[]): string | null {
    const termos = ['hprt mpt-ii', 'mpt-ii', 'hprt'];

    return impressoras.find((impressora) => {
      const nome = impressora.toLowerCase();
      return termos.some((termo) => nome.includes(termo));
    }) ?? null;
  }

  private montarCupomTeste(nomeImpressora: string): string {
    const inicializar = '\x1B\x40';
    const paginaCodigo = '\x1B\x74\x02';
    const alinharCentro = '\x1B\x61\x01';
    const alinharEsquerda = '\x1B\x61\x00';
    const negritoLigado = '\x1B\x45\x01';
    const negritoDesligado = '\x1B\x45\x00';
    const linha = '--------------------------------\n';

    return [
      inicializar,
      paginaCodigo,
      alinharCentro,
      negritoLigado,
      'QUENTINHAS DA TININHA\n',
      negritoDesligado,
      alinharEsquerda,
      linha,
      'TESTE DE IMPRESSAO\n\n',
      'Sistema conectado com sucesso.\n\n',
      'Impressora:\n',
      `${nomeImpressora}\n\n`,
      linha,
      alinharCentro,
      'Angular + QZ Tray + ESC/POS\n\n',
      negritoLigado,
      'TESTE CONCLUIDO\n',
      negritoDesligado,
      '\n\n\n'
    ].join('');
  }

  private montarCupomPedido(
    pedido: PedidoImpressao,
    opcoes: { reimpressao?: boolean }
  ): string {
    const inicializar = '\x1B\x40';
    const paginaCodigo = '\x1B\x74\x02';
    const alinharCentro = '\x1B\x61\x01';
    const alinharEsquerda = '\x1B\x61\x00';
    const negritoLigado = '\x1B\x45\x01';
    const negritoDesligado = '\x1B\x45\x00';
    const tamanhoDuplo = '\x1D\x21\x11';
    const tamanhoNormal = '\x1D\x21\x00';
    const cortarParcial = '\x1D\x56\x42\x00';
    const linhas: string[] = [];

    linhas.push(
      inicializar,
      paginaCodigo,
      alinharCentro,
      negritoLigado,
      tamanhoDuplo,
      'QUENTINHAS\n',
      tamanhoNormal,
      'DA TININHA\n',
      negritoDesligado,
      alinharEsquerda,
      this.linha(),
      ...(opcoes.reimpressao
        ? [
            alinharCentro,
            negritoLigado,
            '*** REIMPRESSAO ***\n',
            negritoDesligado,
            alinharEsquerda,
            this.linha()
          ]
        : []),
      `${negritoLigado}PEDIDO${negritoDesligado} ${this.sufixoPedido(pedido.id)}\n`,
      `Criado: ${this.formatarDataHora(pedido.criadoEm)}\n`,
      this.linha(),
      `${negritoLigado}CLIENTE${negritoDesligado}\n`,
      `${this.limparTexto(pedido.nomeCliente)}\n`
    );

    if (pedido.telefoneCliente) {
      linhas.push(`Tel: ${this.limparTexto(pedido.telefoneCliente)}\n`);
    }

    linhas.push(
      this.linha(),
      `${negritoLigado}ITENS${negritoDesligado}\n`
    );

    pedido.itens.forEach((item) => linhas.push(this.formatarItem(item)));

    linhas.push(
      this.linha(),
      this.linhaValor('Subtotal', pedido.valorSubtotal)
    );

    if (typeof pedido.valorFrete === 'number' && pedido.valorFrete > 0) {
      linhas.push(this.linhaValor('Frete', pedido.valorFrete));
    }

    linhas.push(
      `${negritoLigado}${this.linhaValor('TOTAL', pedido.valorTotal)}${negritoDesligado}`,
      this.linha(),
      `${negritoLigado}PAGAMENTO${negritoDesligado}\n`,
      `${this.rotuloFormaPagamento(pedido.formaPagamento)}\n`
    );

    if (pedido.precisaTroco) {
      linhas.push(`Troco para: ${this.formatarMoeda(pedido.valorTroco ?? 0)}\n`);
    }

    linhas.push(
      this.linha(),
      `${negritoLigado}${this.rotuloTipoEntrega(pedido.tipoEntrega).toUpperCase()}${negritoDesligado}\n`
    );

    linhas.push(...this.formatarEntrega(pedido));

    if (pedido.observacao) {
      linhas.push(
        this.linha(),
        alinharCentro,
        negritoLigado,
        '*** OBSERVACAO ***\n',
        negritoDesligado,
        alinharEsquerda,
        ...this.quebrarLinhas(this.limparTexto(pedido.observacao).toUpperCase()).map((linha) => `${linha}\n`),
        alinharCentro,
        '******************\n',
        alinharEsquerda
      );
    }

    linhas.push(
      this.linha(),
      alinharCentro,
      'Obrigado pela preferencia!\n',
      alinharEsquerda,
      '\n\n\n',
      cortarParcial
    );

    return linhas.join('');
  }

  private formatarItem(item: PedidoItemImpressao): string {
    const quantidade = item.quantidade && item.quantidade > 0 ? item.quantidade : 1;
    const tamanho = this.rotuloTamanho(item.tamanho);
    const cabecalho = `${quantidade}x ${this.limparTexto(item.nomePrato)}${tamanho ? ` ${tamanho}` : ''}`;
    const linhas = this.quebrarLinhas(cabecalho).map((linha) => `${linha}\n`);

    linhas.push(`  ${this.formatarMoeda(item.valorUnitario)} cada\n`);

    const acompanhamentos = this.normalizarAcompanhamentos(item.acompanhamentos);
    acompanhamentos.forEach((acompanhamento) => {
      this.quebrarLinhas(`+ ${acompanhamento}`, LARGURA_CUPOM_58MM - 2)
        .forEach((linha) => linhas.push(`  ${linha}\n`));
    });

    if (item.observacao) {
      this.quebrarLinhas(`OBS: ${this.limparTexto(item.observacao).toUpperCase()}`, LARGURA_CUPOM_58MM - 2)
        .forEach((linha) => linhas.push(`  ${linha}\n`));
    }

    return linhas.join('');
  }

  private formatarEntrega(pedido: PedidoImpressao): string[] {
    if (this.ehRetirada(pedido.tipoEntrega)) {
      return ['Retirada no local\n'];
    }

    const endereco = pedido.enderecoEntrega || [
      pedido.logradouro,
      pedido.numero ? `n ${pedido.numero}` : null,
      pedido.complemento
    ].filter(Boolean).join(', ');

    const linhas: string[] = [];

    if (endereco) {
      linhas.push(...this.quebrarLinhas(this.limparTexto(endereco)).map((linha) => `${linha}\n`));
    }

    const bairroCidade = [
      pedido.bairro,
      pedido.cidade,
      pedido.estado
    ].filter(Boolean).join(' - ');

    if (bairroCidade) {
      linhas.push(...this.quebrarLinhas(this.limparTexto(bairroCidade)).map((linha) => `${linha}\n`));
    }

    if (pedido.cep) {
      linhas.push(`CEP: ${this.limparTexto(pedido.cep)}\n`);
    }

    if (pedido.referencia) {
      linhas.push(...this.quebrarLinhas(`Ref: ${this.limparTexto(pedido.referencia)}`).map((linha) => `${linha}\n`));
    }

    return linhas.length ? linhas : ['Endereco nao informado\n'];
  }

  private linhaValor(rotulo: string, valor: number): string {
    const moeda = this.formatarMoeda(valor);
    const texto = `${rotulo}:`;
    const espacos = Math.max(1, LARGURA_CUPOM_58MM - texto.length - moeda.length);
    return `${texto}${' '.repeat(espacos)}${moeda}\n`;
  }

  private linha(): string {
    return `${'-'.repeat(LARGURA_CUPOM_58MM)}\n`;
  }

  private quebrarLinhas(texto: string, largura = LARGURA_CUPOM_58MM): string[] {
    const palavras = texto.split(/\s+/).filter(Boolean);
    const linhas: string[] = [];
    let linhaAtual = '';

    palavras.forEach((palavra) => {
      if (palavra.length > largura) {
        if (linhaAtual) {
          linhas.push(linhaAtual);
          linhaAtual = '';
        }
        for (let indice = 0; indice < palavra.length; indice += largura) {
          linhas.push(palavra.slice(indice, indice + largura));
        }
        return;
      }

      const candidata = linhaAtual ? `${linhaAtual} ${palavra}` : palavra;
      if (candidata.length > largura) {
        linhas.push(linhaAtual);
        linhaAtual = palavra;
      } else {
        linhaAtual = candidata;
      }
    });

    if (linhaAtual) {
      linhas.push(linhaAtual);
    }

    return linhas.length ? linhas : [''];
  }

  private limparTexto(texto: string): string {
    return texto
      .replace(/[^\S\r\n]+/g, ' ')
      .replace(/[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]/g, '')
      .trim();
  }

  private formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor || 0);
  }

  private formatarDataHora(valor: string): string {
    const data = new Date(valor);

    if (Number.isNaN(data.getTime())) {
      return valor;
    }

    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(data);
  }

  private sufixoPedido(id: string): string {
    return `#${id.replace(/-/g, '').slice(-6).toUpperCase()}`;
  }

  private rotuloFormaPagamento(valor: number | string): string {
    const normalizado = this.normalizarChave(valor);

    if (normalizado === '1' || normalizado.includes('dinheiro')) {
      return 'Dinheiro';
    }

    if (normalizado === '2' || normalizado.includes('pix')) {
      return 'Pix';
    }

    if (normalizado === '3' || normalizado.includes('cartao')) {
      return 'Cartao';
    }

    return this.limparTexto(String(valor));
  }

  private rotuloTipoEntrega(valor: number | string): string {
    const normalizado = this.normalizarChave(valor);
    return normalizado === '1' || normalizado.includes('retirada') ? 'Retirada' : 'Entrega';
  }

  private ehRetirada(valor: number | string): boolean {
    return this.rotuloTipoEntrega(valor) === 'Retirada';
  }

  private rotuloTamanho(valor: number | string | null | undefined): string {
    if (valor === null || valor === undefined || valor === '') {
      return '';
    }

    const normalizado = this.normalizarChave(valor);

    if (normalizado === '1' || normalizado === 'p' || normalizado.includes('pequena')) {
      return '(P)';
    }

    if (normalizado === '2' || normalizado === 'g' || normalizado.includes('grande')) {
      return '(G)';
    }

    return `(${this.limparTexto(String(valor))})`;
  }

  private normalizarAcompanhamentos(valor: string | string[] | null | undefined): string[] {
    if (!valor) {
      return [];
    }

    const acompanhamentos = Array.isArray(valor)
      ? valor
      : valor.split(/[;,\n]/);

    return acompanhamentos
      .map((acompanhamento) => this.limparTexto(acompanhamento))
      .filter(Boolean);
  }

  private normalizarChave(valor: number | string): string {
    return String(valor)
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase();
  }

  private normalizarErro(erro: unknown, contexto: 'conexao' | 'impressao'): Error {
    if (!environment.production) {
      console.error('Falha na impressao termica via QZ Tray.', erro);
    }

    if (erro instanceof Error) {
      const mensagem = erro.message.toLowerCase();

      if (mensagem.includes('hprt mpt-ii nao encontrada')) {
        return erro;
      }

      if (
        mensagem.includes('connection') ||
        mensagem.includes('websocket') ||
        mensagem.includes('connect') ||
        mensagem.includes('qz tray')
      ) {
        return new Error('QZ Tray nao esta conectado. Abra o QZ Tray e tente novamente.');
      }
    }

    return new Error(
      contexto === 'conexao'
        ? 'QZ Tray nao esta conectado. Abra o QZ Tray e tente novamente.'
        : 'Nao foi possivel enviar o teste para a impressora.'
    );
  }

  private mensagemErro(erro: unknown): string {
    return erro instanceof Error
      ? erro.message
      : 'Falha desconhecida ao conectar ao QZ Tray.';
  }

  private async configurarSeguranca(): Promise<void> {
    if (this.segurancaConfigurada) {
      return;
    }

    if (this.segurancaPromise) {
      return this.segurancaPromise;
    }

    this.segurancaPromise = (async () => {
      console.log('[QZ SECURITY] iniciando');

      if (!qz.security) {
        throw new Error('QZ Tray nao possui API de assinatura disponivel.');
      }

      qz.security.setCertificatePromise(async () => {
        try {
          console.log('[QZ SECURITY] solicitando certificado');
          const certificado = await firstValueFrom(this.qzSigningService.obterCertificado());

          if (!certificado.trim()) {
            throw new Error('Certificado QZ vazio.');
          }

          console.log('[QZ SECURITY] certificado recebido');
          return certificado;
        } catch (erro: unknown) {
          console.warn(`[QZ SECURITY] falha ao obter certificado: ${this.mensagemErro(erro)}`);
          throw erro;
        }
      }, { rejectOnFailure: true });

      qz.security.setSignatureAlgorithm('SHA512');

      qz.security.setSignaturePromise(async (toSign: string) => {
        try {
          console.log('[QZ SECURITY] solicitando assinatura');
          const resposta = await firstValueFrom(this.qzSigningService.assinar(toSign));

          if (!resposta.assinatura?.trim()) {
            throw new Error('Assinatura QZ vazia.');
          }

          console.log('[QZ SECURITY] assinatura recebida');
          return resposta.assinatura;
        } catch (erro: unknown) {
          console.warn(`[QZ SECURITY] falha ao obter assinatura: ${this.mensagemErro(erro)}`);
          throw erro;
        }
      });

      this.segurancaConfigurada = true;
      console.log('[QZ SECURITY] configurado');
    })();

    try {
      await this.segurancaPromise;
    } finally {
      this.segurancaPromise = undefined;
    }
  }

  private registrarCallbacksQz(): void {
    if (this.callbacksRegistrados) {
      return;
    }

    qz.websocket.setClosedCallbacks((evento: unknown) => {
      console.info('[QZ] websocket fechado', evento);
      this.estado = 'desconectado';
      this.qzConectadoInterno.set(false);
      this.conexaoPromise = undefined;
    });

    qz.websocket.setErrorCallbacks((evento: unknown) => {
      console.warn('[QZ] websocket erro', evento);
    });

    this.callbacksRegistrados = true;
  }
}
