import { CurrencyPipe } from '@angular/common';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  Output,
  ViewChild,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  FormaPagamento,
  PersonalizacaoPedido,
  Prato,
  TamanhoRefeicao,
  TipoEntrega
} from '../../../compartilhado/modelos/cardapio.model';
import { CepService } from '../../../compartilhado/servicos/cep.service';
import { PedidoService } from '../../../compartilhado/servicos/pedido.service';
import { WhatsappService } from '../../../compartilhado/servicos/whatsapp.service';
import { Subscription, finalize } from 'rxjs';

@Component({
  selector: 'app-personalizacao-pedido-modal',
  standalone: true,
  imports: [CurrencyPipe, FormsModule],
  template: `
    <div class="pedido-modal" role="presentation">
      <button class="pedido-modal__backdrop" type="button" aria-label="Fechar personalização" (click)="fechar.emit()"></button>

      <section
        #painel
        class="pedido-modal__painel"
        role="dialog"
        aria-modal="true"
        [attr.aria-labelledby]="tituloId"
        tabindex="-1"
      >
        <span class="pedido-modal__alca" aria-hidden="true"></span>

        <header class="pedido-modal__topo">
          <span class="pedido-modal__imagem">
            @if (deveMostrarImagem()) {
              <img [src]="prato.urlImagem" [alt]="prato.nome" loading="lazy" (error)="imagemFalhou.set(true)" />
            } @else {
              <img src="/assets/prato-hero-real.png" [alt]="prato.nome" loading="lazy" />
            }
          </span>

          <div>
            <h3 [id]="tituloId">{{ prato.nome }}</h3>
            <p>{{ prato.descricao }}</p>
            <strong>A partir de {{ prato.precos.pequenaDinheiroPix | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
          </div>

          <button class="pedido-modal__fechar" type="button" aria-label="Fechar personalização" (click)="fechar.emit()">×</button>
        </header>

        <div class="pedido-modal__conteudo">
          <div class="personalizacao-prato pedido-modal__formulario" [id]="'personalizacao-' + prato.id">
            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>1</span>
                <h4>Cliente</h4>
              </header>
              <label class="campo-pedido campo-pedido--largo">
                Nome
                <input type="text" placeholder="Seu nome" [ngModel]="nomeCliente()" (ngModelChange)="nomeCliente.set($event)" />
              </label>
              @if (nomeClienteInvalido()) {
                <small class="aviso-pedido">Informe seu nome para finalizar o pedido.</small>
              }
            </section>

            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>2</span>
                <h4>Tamanho</h4>
              </header>
              <div class="radio-cards">
                @for (opcao of tamanhos; track opcao.valor) {
                  <label [class.ativo]="tamanho() === opcao.valor">
                    <input type="radio" [name]="'tamanho-' + prato.id" [checked]="tamanho() === opcao.valor" (change)="tamanho.set(opcao.valor)" />
                    <strong>{{ opcao.valor }}</strong>
                    <span>{{ opcao.rotulo }}</span>
                  </label>
                }
              </div>
            </section>

            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>3</span>
                <h4>Pagamento</h4>
              </header>
              <div class="radio-cards">
                @for (opcao of pagamentos; track opcao.valor) {
                  <label [class.ativo]="formaPagamento() === opcao.valor">
                    <input type="radio" [name]="'pagamento-' + prato.id" [checked]="formaPagamento() === opcao.valor" (change)="selecionarFormaPagamento(opcao.valor)" />
                    <span>{{ opcao.rotulo }}</span>
                  </label>
                }
              </div>

              @if (formaPagamento() === 'dinheiro') {
                <div class="grupo-opcoes">
                  <strong>Precisa de troco?</strong>
                  <div class="opcoes-inline">
                    <label [class.ativo]="precisaTroco()">
                      <input type="radio" [name]="'troco-' + prato.id" [checked]="precisaTroco()" (change)="precisaTroco.set(true)" />
                      Sim
                    </label>
                    <label [class.ativo]="!precisaTroco()">
                      <input type="radio" [name]="'troco-' + prato.id" [checked]="!precisaTroco()" (change)="precisaTroco.set(false); valorTrocoTexto.set('')" />
                      Não
                    </label>
                  </div>

                  @if (precisaTroco()) {
                    <label class="campo-pedido campo-pedido--moeda">
                      <span>Troco para:</span>
                      <span class="entrada-moeda">
                        <strong>R$</strong>
                        <input
                          type="text"
                          inputmode="decimal"
                          placeholder="50,00"
                          [ngModel]="valorTrocoTexto()"
                          (ngModelChange)="atualizarValorTroco($event)"
                        />
                      </span>
                    </label>
                    @if (erroTroco(); as erro) {
                      <small class="aviso-pedido">{{ erro }}</small>
                    }
                  }
                </div>
              }
            </section>

            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>4</span>
                <h4>Acompanhamentos</h4>
              </header>

              @if (grupo().tipoFeijao.length > 0) {
                <div class="grupo-opcoes">
                  <strong>Tipo de feijão</strong>
                  <div class="opcoes-inline">
                    @for (feijao of grupo().tipoFeijao; track feijao.id) {
                      <label [class.opcao-indisponivel]="!feijao.estaDisponivel">
                        <input
                          type="radio"
                          [name]="'feijao-' + prato.id"
                          [checked]="tipoFeijaoId() === feijao.id"
                          [disabled]="!feijao.estaDisponivel"
                          (change)="selecionarTipoFeijao(feijao.id)"
                        />
                        {{ feijao.nome }} @if (!feijao.estaDisponivel) { <em>Indisponível</em> }
                      </label>
                    }
                    <label>
                      <input type="radio" [name]="'feijao-' + prato.id" [checked]="tipoFeijaoId() === null" (change)="tipoFeijaoId.set(null)" />
                      Sem feijão
                    </label>
                  </div>
                </div>
              }

              <div class="grupo-opcoes">
                <strong>{{ grupo().tipoFeijao.length > 0 ? 'Outros acompanhamentos' : grupo().titulo }}</strong>
                <div class="opcoes-inline">
                  @for (acompanhamento of grupo().itens; track acompanhamento.id) {
                    <label [class.opcao-indisponivel]="!acompanhamento.estaDisponivel">
                      <input
                        type="checkbox"
                        [checked]="acompanhamentoSelecionado(acompanhamento.id)"
                        [disabled]="!acompanhamento.estaDisponivel"
                        (change)="alternarAcompanhamento(acompanhamento.id)"
                      />
                      {{ acompanhamento.nome }} @if (!acompanhamento.estaDisponivel) { <em>Indisponível</em> }
                    </label>
                  }
                </div>
              </div>
            </section>

            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>5</span>
                <h4>Observação</h4>
              </header>

              <label class="campo-pedido campo-pedido--largo">
                Observação do pedido
                <textarea
                  rows="3"
                  maxlength="250"
                  placeholder="Ex.: sem cebola, pouco sal, separar a salada..."
                  [ngModel]="observacao()"
                  (ngModelChange)="atualizarObservacao($event)"
                ></textarea>
                <small class="contador-caracteres">{{ observacao().length }}/250</small>
              </label>
            </section>

            <section class="etapa-pedido">
              <header class="etapa-pedido__cabecalho">
                <span>6</span>
                <h4>Entrega</h4>
              </header>
              <div class="radio-cards">
                @for (opcao of tiposEntrega; track opcao.valor) {
                  <label [class.ativo]="tipoEntrega() === opcao.valor">
                    <input type="radio" [name]="'entrega-' + prato.id" [checked]="tipoEntrega() === opcao.valor" (change)="selecionarTipoEntrega(opcao.valor)" />
                    <span>{{ opcao.rotulo }}</span>
                  </label>
                }
              </div>

              @if (tipoEntrega() === 'entrega') {
                <div class="pedido-campos pedido-campos--entrega">
                  <label class="campo-pedido">
                    CEP
                    <input
                      type="text"
                      inputmode="numeric"
                      placeholder="00000-000"
                      [ngModel]="cep()"
                      (ngModelChange)="atualizarCep($event)"
                      (blur)="validarCepIncompleto()"
                    />
                  </label>
                  <label class="campo-pedido">
                    Rua/logradouro
                    <input type="text" placeholder="Rua ou avenida" [ngModel]="logradouro()" (ngModelChange)="logradouro.set($event)" />
                  </label>
                  <label class="campo-pedido">
                    Número
                    <input type="text" placeholder="Número" [ngModel]="numero()" (ngModelChange)="numero.set($event)" />
                  </label>
                  <label class="campo-pedido">
                    Complemento
                    <input type="text" placeholder="Apto, casa, bloco" [ngModel]="complemento()" (ngModelChange)="complemento.set($event)" />
                  </label>
                  <label class="campo-pedido">
                    Bairro
                    <input type="text" placeholder="Bairro pelo CEP" [ngModel]="bairro()" readonly />
                  </label>
                  <label class="campo-pedido">
                    Cidade
                    <input type="text" placeholder="Cidade" [ngModel]="cidade()" readonly />
                  </label>
                  <label class="campo-pedido">
                    Estado
                    <input type="text" placeholder="UF" [ngModel]="estado()" readonly />
                  </label>
                  <label class="campo-pedido campo-pedido--largo">
                    Ponto de referência
                    <input type="text" placeholder="Ponto de referência" [ngModel]="referencia()" (ngModelChange)="referencia.set($event)" />
                  </label>
                </div>
                @if (consultandoCep()) {
                  <small class="info-pedido">Consultando CEP...</small>
                }
                @if (cepMensagem(); as mensagemCep) {
                  <small
                    class="aviso-pedido"
                    [class.aviso-pedido--sucesso]="cepMensagemTipo() === 'sucesso'"
                  >
                    {{ mensagemCep }}
                  </small>
                }
                @if (freteAtendido() && valorFrete() !== null) {
                  <small class="info-pedido info-pedido--sucesso">
                    Frete: {{ valorFrete() | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}
                  </small>
                }
                @if (bairro() && !freteAtendido() && !consultandoCep() && cepNumerico().length === 8) {
                  <button class="botao secundario botao-retirada" type="button" (click)="selecionarTipoEntrega('retirada')">
                    Mudar para retirada
                  </button>
                }
                @if (entregaInvalida()) {
                  <small class="aviso-pedido">Valide o CEP e informe rua e número para entrega.</small>
                }
              }
            </section>

            <section class="resumo-pedido" aria-label="Resumo do pedido">
              <div class="resumo-pedido__linha">
                <span>Subtotal</span>
                <strong>{{ subtotal() | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
              </div>
              @if (tipoEntrega() === 'entrega') {
                <div class="resumo-pedido__linha">
                  <span>Frete</span>
                  <strong>{{ freteResumo }}</strong>
                </div>
              }
              <div class="resumo-pedido__linha resumo-pedido__linha--total">
                <span>Total</span>
                <strong>{{ total() | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
              </div>
              <div class="resumo-pedido__linha">
                <span>Pagamento</span>
                <strong>{{ rotuloPagamento }}</strong>
              </div>
              @if (formaPagamento() === 'dinheiro') {
                <div class="resumo-pedido__linha">
                  <span>Troco</span>
                  <strong>{{ resumoTroco }}</strong>
                </div>
              }
              <div class="resumo-pedido__linha">
                <span>Entrega</span>
                <strong>{{ resumoEntrega }}</strong>
              </div>
              <small>Acompanhamentos: {{ resumoAcompanhamentos }}</small>
            </section>
          </div>
        </div>

        <footer class="pedido-modal__rodape">
          @if (linkWhatsapp(); as link) {
            <a class="botao botao-pedido-expandido" [href]="link" target="_blank" rel="noopener" aria-label="Finalizar pedido de {{ prato.nome }} pelo WhatsApp">
              Finalizar Pedido
            </a>
          } @else {
            <button class="botao botao-pedido-expandido" type="button" disabled>
              Finalizar Pedido
            </button>
          }
        </footer>
      </section>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PersonalizacaoPedidoModalComponent implements AfterViewInit, OnDestroy {
  private readonly pedidoService = inject(PedidoService);
  private readonly whatsappService = inject(WhatsappService);
  private readonly cepService = inject(CepService);

  @ViewChild('painel') private painel?: ElementRef<HTMLElement>;
  private consultaCepSubscription?: Subscription;

  @Input({ required: true }) prato!: Prato;
  @Input({ required: true }) whatsappRestaurante = '';
  @Input({ required: true }) restauranteAberto = true;
  @Output() readonly fechar = new EventEmitter<void>();

  protected readonly imagemFalhou = signal(false);
  protected readonly tamanho = signal<TamanhoRefeicao>('P');
  protected readonly nomeCliente = signal('');
  protected readonly formaPagamento = signal<FormaPagamento>('pix');
  protected readonly precisaTroco = signal(false);
  protected readonly valorTrocoTexto = signal('');
  protected readonly tipoEntrega = signal<TipoEntrega>('retirada');
  protected readonly cep = signal('');
  protected readonly logradouro = signal('');
  protected readonly numero = signal('');
  protected readonly complemento = signal('');
  protected readonly bairro = signal('');
  protected readonly cidade = signal('');
  protected readonly estado = signal('');
  protected readonly referencia = signal('');
  protected readonly observacao = signal('');
  protected readonly valorFrete = signal<number | null>(null);
  protected readonly freteAtendido = signal(false);
  protected readonly consultandoCep = signal(false);
  protected readonly cepMensagem = signal('');
  protected readonly cepMensagemTipo = signal<'erro' | 'sucesso' | ''>('');
  private readonly ultimoCepConsultado = signal('');
  protected readonly acompanhamentoIds = signal<string[]>([]);
  protected readonly tipoFeijaoId = signal<string | null>(null);

  protected readonly tamanhos: { valor: TamanhoRefeicao; rotulo: string }[] = [
    { valor: 'P', rotulo: 'Pequena' },
    { valor: 'G', rotulo: 'Grande' }
  ];
  protected readonly pagamentos: { valor: FormaPagamento; rotulo: string }[] = [
    { valor: 'dinheiro', rotulo: 'Dinheiro' },
    { valor: 'pix', rotulo: 'PIX' },
    { valor: 'cartao', rotulo: 'Cartão' }
  ];
  protected readonly tiposEntrega: { valor: TipoEntrega; rotulo: string }[] = [
    { valor: 'retirada', rotulo: 'Retirada' },
    { valor: 'entrega', rotulo: 'Entrega' }
  ];

  protected get tituloId(): string {
    return `pedido-modal-titulo-${this.prato.id}`;
  }

  protected readonly grupo = computed(() => this.pedidoService.obterGrupo(this.prato));
  protected readonly cepNumerico = computed(() => this.cep().replace(/\D/g, ''));
  protected readonly valorTroco = computed(() => {
    const valor = Number(this.valorTrocoTexto().replace(',', '.'));
    return Number.isFinite(valor) && valor > 0 ? valor : null;
  });
  protected readonly erroTroco = computed(() => {
    if (this.formaPagamento() !== 'dinheiro' || !this.precisaTroco()) {
      return null;
    }

    const valorTroco = this.valorTroco();
    if (valorTroco === null) {
      return 'Informe o valor para troco.';
    }

    if (valorTroco <= this.total()) {
      return 'O valor para troco deve ser maior que o total do pedido.';
    }

    return null;
  });
  protected readonly nomeClienteInvalido = computed(() => !this.normalizarTexto(this.nomeCliente()));
  protected readonly entregaInvalida = computed(() =>
    this.tipoEntrega() === 'entrega' &&
    (this.cepNumerico().length !== 8 ||
      this.consultandoCep() ||
      !this.freteAtendido() ||
      !this.normalizarTexto(this.logradouro()) ||
      !this.normalizarTexto(this.numero()) ||
      !this.normalizarTexto(this.bairro()) ||
      !this.normalizarTexto(this.cidade()) ||
      !this.normalizarTexto(this.estado()))
  );
  protected readonly pedidoValido = computed(() =>
    !this.nomeClienteInvalido() &&
    !this.erroTroco() &&
    !this.entregaInvalida() &&
    this.observacao().length <= 250
  );
  protected readonly personalizacao = computed<PersonalizacaoPedido>(() => ({
    pratoId: this.prato.id,
    tamanho: this.tamanho(),
    formaPagamento: this.formaPagamento(),
    acompanhamentoIds: this.acompanhamentoIds(),
    tipoFeijaoId: this.tipoFeijaoId(),
    observacao: this.normalizarTexto(this.observacao()),
    precisaTroco: this.formaPagamento() === 'dinheiro' && this.precisaTroco(),
    valorTroco: this.formaPagamento() === 'dinheiro' && this.precisaTroco() ? this.valorTroco() : null,
    tipoEntrega: this.tipoEntrega(),
    cep: this.tipoEntrega() === 'entrega' ? this.cepNumerico() : null,
    logradouro: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.logradouro()) : null,
    numero: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.numero()) : null,
    complemento: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.complemento()) : null,
    enderecoEntrega: this.tipoEntrega() === 'entrega' ? this.montarEnderecoEntrega() : null,
    bairro: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.bairro()) : null,
    cidade: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.cidade()) : null,
    estado: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.estado()) : null,
    referencia: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.referencia()) : null,
    valorFrete: this.tipoEntrega() === 'entrega' && this.freteAtendido() ? this.valorFrete() : null
  }));
  protected readonly acompanhamentosSelecionados = computed(() =>
    this.pedidoService.listarAcompanhamentosSelecionados(this.personalizacao(), this.grupo())
  );
  protected readonly subtotal = computed(() =>
    this.pedidoService.calcularPreco(this.prato, this.tamanho(), this.formaPagamento())
  );
  protected readonly freteAplicado = computed(() =>
    this.tipoEntrega() === 'entrega' && this.freteAtendido()
      ? this.valorFrete() ?? 0
      : 0
  );
  protected readonly total = computed(() => this.subtotal() + this.freteAplicado());
  protected readonly linkWhatsapp = computed(() => {
    if (!this.restauranteAberto || !this.whatsappRestaurante.trim() || !this.pedidoValido()) {
      return null;
    }

    return this.whatsappService.criarLinkPedido(
      this.prato,
      this.tamanho(),
      this.formaPagamento(),
      this.acompanhamentosSelecionados(),
      this.total(),
      this.whatsappRestaurante,
      {
        nomeCliente: this.normalizarTexto(this.nomeCliente()),
        precisaTroco: this.personalizacao().precisaTroco,
        valorTroco: this.personalizacao().valorTroco,
        tipoEntrega: this.personalizacao().tipoEntrega,
        observacaoItem: this.personalizacao().observacao,
        subtotal: this.subtotal(),
        valorFrete: this.personalizacao().valorFrete,
        logradouro: this.personalizacao().logradouro,
        numero: this.personalizacao().numero,
        complemento: this.personalizacao().complemento,
        enderecoEntrega: this.personalizacao().enderecoEntrega,
        bairro: this.personalizacao().bairro,
        cidade: this.personalizacao().cidade,
        estado: this.personalizacao().estado,
        referencia: this.personalizacao().referencia
      }
    );
  });

  ngAfterViewInit(): void {
    setTimeout(() => this.painel?.nativeElement.focus());
  }

  ngOnDestroy(): void {
    this.consultaCepSubscription?.unsubscribe();
  }

  @HostListener('document:keydown.escape')
  protected fecharPorEsc(): void {
    this.fechar.emit();
  }

  protected get resumoAcompanhamentos(): string {
    const nomes = this.acompanhamentosSelecionados().map((acompanhamento) => acompanhamento.nome);
    return nomes.length > 0 ? nomes.join(', ') : 'sem acompanhamentos selecionados';
  }

  protected get rotuloPagamento(): string {
    return this.pedidoService.rotuloPagamento(this.formaPagamento());
  }

  protected get resumoTroco(): string {
    if (!this.precisaTroco()) {
      return 'Não precisa';
    }

    const valorTroco = this.valorTroco();
    return valorTroco === null
      ? 'aguardando valor'
      : `para ${new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valorTroco)}`;
  }

  protected get freteResumo(): string {
    if (this.consultandoCep()) {
      return 'consultando';
    }

    if (this.freteAtendido() && this.valorFrete() !== null) {
      return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
        .format(this.valorFrete() ?? 0);
    }

    return 'aguardando CEP';
  }

  protected get resumoEntrega(): string {
    if (this.tipoEntrega() === 'retirada') {
      return 'Retirada no local';
    }

    if (this.consultandoCep()) {
      return 'consultando CEP';
    }

    const endereco = this.montarEnderecoEntrega();
    const bairro = this.normalizarTexto(this.bairro());
    return endereco && bairro ? `${endereco} - ${bairro}` : 'aguardando CEP';
  }

  protected deveMostrarImagem(): boolean {
    return Boolean(this.prato.urlImagem.trim()) && !this.imagemFalhou();
  }

  protected selecionarFormaPagamento(formaPagamento: FormaPagamento): void {
    this.formaPagamento.set(formaPagamento);

    if (formaPagamento !== 'dinheiro') {
      this.precisaTroco.set(false);
      this.valorTrocoTexto.set('');
    }
  }

  protected selecionarTipoEntrega(tipoEntrega: TipoEntrega): void {
    this.tipoEntrega.set(tipoEntrega);

    if (tipoEntrega === 'retirada') {
      this.consultaCepSubscription?.unsubscribe();
      this.consultandoCep.set(false);
      this.freteAtendido.set(false);
      this.valorFrete.set(null);
      this.cepMensagem.set('');
      this.cepMensagemTipo.set('');
      return;
    }

    if (this.cepNumerico().length === 8) {
      this.consultarCep(true);
    } else {
      this.validarCepIncompleto();
    }
  }

  protected atualizarValorTroco(valor: string | number | null): void {
    this.valorTrocoTexto.set(valor === null ? '' : String(valor));
  }

  protected atualizarObservacao(valor: string | number | null): void {
    this.observacao.set(String(valor ?? '').slice(0, 250));
  }

  protected atualizarCep(valor: string | number | null): void {
    const cepFormatado = this.formatarCep(String(valor ?? ''));
    const cepAnterior = this.cepNumerico();
    this.cep.set(cepFormatado);
    const cepAtual = this.cepNumerico();

    if (cepAtual !== cepAnterior) {
      this.limparResultadoCep();
    }

    if (this.tipoEntrega() !== 'entrega') {
      return;
    }

    if (cepAtual.length === 8) {
      this.consultarCep();
      return;
    }

    this.validarCepIncompleto();
  }

  protected validarCepIncompleto(): void {
    if (this.tipoEntrega() !== 'entrega') {
      return;
    }

    const quantidade = this.cepNumerico().length;
    if (quantidade > 0 && quantidade < 8) {
      this.definirMensagemCep('Informe um CEP com 8 números.', 'erro');
      return;
    }

    if (quantidade === 0) {
      this.cepMensagem.set('');
      this.cepMensagemTipo.set('');
    }
  }

  protected acompanhamentoSelecionado(id: string): boolean {
    return this.acompanhamentoIds().includes(id);
  }

  protected selecionarTipoFeijao(id: string): void {
    const feijao = this.grupo().tipoFeijao.find((item) => item.id === id);
    if (feijao?.estaDisponivel) {
      this.tipoFeijaoId.set(id);
    }
  }

  protected alternarAcompanhamento(id: string): void {
    const acompanhamento = this.grupo().itens.find((item) => item.id === id);
    if (!acompanhamento?.estaDisponivel) {
      return;
    }

    this.acompanhamentoIds.update((ids) =>
      ids.includes(id) ? ids.filter((item) => item !== id) : [...ids, id]
    );
  }

  private consultarCep(forcar = false): void {
    if (this.tipoEntrega() !== 'entrega') {
      return;
    }

    const cep = this.cepNumerico();
    if (cep.length !== 8) {
      this.definirMensagemCep('Informe um CEP com 8 números.', 'erro');
      return;
    }

    if (!forcar && cep === this.ultimoCepConsultado() && (this.freteAtendido() || this.cepMensagem())) {
      return;
    }

    this.consultaCepSubscription?.unsubscribe();
    this.consultandoCep.set(true);
    this.ultimoCepConsultado.set(cep);
    this.cepMensagem.set('');
    this.cepMensagemTipo.set('');

    this.consultaCepSubscription = this.cepService.consultarFretePorCep(cep)
      .pipe(finalize(() => this.consultandoCep.set(false)))
      .subscribe({
        next: (resposta) => this.aplicarConsultaCep(resposta),
        error: (erro: { status?: number; error?: { mensagem?: string } }) => {
          this.freteAtendido.set(false);
          this.valorFrete.set(null);

          if (erro.status === 404) {
            this.definirMensagemCep('CEP não encontrado. Verifique os números informados.', 'erro');
            return;
          }

          this.definirMensagemCep(
            erro.error?.mensagem ?? 'Não foi possível consultar o CEP agora. Tente novamente em alguns instantes.',
            'erro'
          );
        }
      });
  }

  private aplicarConsultaCep(resposta: {
    logradouro: string | null;
    bairro: string;
    cidade: string;
    estado: string;
    atendido: boolean;
    valorFrete: number | null;
    mensagem: string | null;
  }): void {
    this.logradouro.set(resposta.logradouro ?? '');
    this.bairro.set(resposta.bairro ?? '');
    this.cidade.set(resposta.cidade ?? '');
    this.estado.set(resposta.estado ?? '');

    if (resposta.atendido && resposta.valorFrete !== null) {
      this.freteAtendido.set(true);
      this.valorFrete.set(resposta.valorFrete);
      this.definirMensagemCep('Entrega disponível para este bairro.', 'sucesso');
      return;
    }

    this.freteAtendido.set(false);
    this.valorFrete.set(null);
    this.definirMensagemCep(
      resposta.mensagem ??
      `No momento, ainda não realizamos entregas para o bairro ${resposta.bairro}. Você pode selecionar a opção de retirada no local.`,
      'erro'
    );
  }

  private limparResultadoCep(): void {
    this.consultaCepSubscription?.unsubscribe();
    this.consultandoCep.set(false);
    this.ultimoCepConsultado.set('');
    this.freteAtendido.set(false);
    this.valorFrete.set(null);
    this.logradouro.set('');
    this.bairro.set('');
    this.cidade.set('');
    this.estado.set('');
    this.cepMensagem.set('');
    this.cepMensagemTipo.set('');
  }

  private definirMensagemCep(mensagem: string, tipo: 'erro' | 'sucesso'): void {
    this.cepMensagem.set(mensagem);
    this.cepMensagemTipo.set(tipo);
  }

  private montarEnderecoEntrega(): string | null {
    const logradouro = this.normalizarTexto(this.logradouro());
    const numero = this.normalizarTexto(this.numero());
    const complemento = this.normalizarTexto(this.complemento());

    if (!logradouro || !numero) {
      return null;
    }

    return complemento ? `${logradouro}, ${numero} - ${complemento}` : `${logradouro}, ${numero}`;
  }

  private formatarCep(valor: string): string {
    const numeros = valor.replace(/\D/g, '').slice(0, 8);
    if (numeros.length <= 5) {
      return numeros;
    }

    return `${numeros.slice(0, 5)}-${numeros.slice(5)}`;
  }

  private normalizarTexto(texto?: string | null): string | null {
    const valor = texto?.trim() ?? '';
    return valor ? valor : null;
  }
}
