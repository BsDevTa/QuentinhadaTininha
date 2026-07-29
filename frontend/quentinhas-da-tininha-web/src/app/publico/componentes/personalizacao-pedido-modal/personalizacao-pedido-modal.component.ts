import { CurrencyPipe } from '@angular/common';
import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
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
import { PedidoService } from '../../../compartilhado/servicos/pedido.service';
import { WhatsappService } from '../../../compartilhado/servicos/whatsapp.service';

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
                <span>2</span>
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
                <span>3</span>
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
                <span>4</span>
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
                <div class="pedido-campos">
                  <label class="campo-pedido">
                    Endereço
                    <input type="text" placeholder="Rua, número e complemento" [ngModel]="enderecoEntrega()" (ngModelChange)="enderecoEntrega.set($event)" />
                  </label>
                  <label class="campo-pedido">
                    Bairro
                    <input type="text" placeholder="Informe o bairro" [ngModel]="bairro()" (ngModelChange)="bairro.set($event)" />
                  </label>
                  <label class="campo-pedido campo-pedido--largo">
                    Referência
                    <input type="text" placeholder="Ponto de referência" [ngModel]="referencia()" (ngModelChange)="referencia.set($event)" />
                  </label>
                </div>
                @if (entregaInvalida()) {
                  <small class="aviso-pedido">Informe endereço, bairro e referência para entrega.</small>
                }
              }
            </section>

            <section class="resumo-pedido" aria-label="Resumo do pedido">
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
export class PersonalizacaoPedidoModalComponent implements AfterViewInit {
  private readonly pedidoService = inject(PedidoService);
  private readonly whatsappService = inject(WhatsappService);

  @ViewChild('painel') private painel?: ElementRef<HTMLElement>;

  @Input({ required: true }) prato!: Prato;
  @Input({ required: true }) whatsappRestaurante = '';
  @Input({ required: true }) restauranteAberto = true;
  @Output() readonly fechar = new EventEmitter<void>();

  protected readonly imagemFalhou = signal(false);
  protected readonly tamanho = signal<TamanhoRefeicao>('P');
  protected readonly formaPagamento = signal<FormaPagamento>('pix');
  protected readonly precisaTroco = signal(false);
  protected readonly valorTrocoTexto = signal('');
  protected readonly tipoEntrega = signal<TipoEntrega>('retirada');
  protected readonly enderecoEntrega = signal('');
  protected readonly bairro = signal('');
  protected readonly referencia = signal('');
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
  protected readonly entregaInvalida = computed(() =>
    this.tipoEntrega() === 'entrega' &&
    (!this.normalizarTexto(this.enderecoEntrega()) ||
      !this.normalizarTexto(this.bairro()) ||
      !this.normalizarTexto(this.referencia()))
  );
  protected readonly pedidoValido = computed(() =>
    !this.erroTroco() &&
    !this.entregaInvalida()
  );
  protected readonly personalizacao = computed<PersonalizacaoPedido>(() => ({
    pratoId: this.prato.id,
    tamanho: this.tamanho(),
    formaPagamento: this.formaPagamento(),
    acompanhamentoIds: this.acompanhamentoIds(),
    tipoFeijaoId: this.tipoFeijaoId(),
    precisaTroco: this.formaPagamento() === 'dinheiro' && this.precisaTroco(),
    valorTroco: this.formaPagamento() === 'dinheiro' && this.precisaTroco() ? this.valorTroco() : null,
    tipoEntrega: this.tipoEntrega(),
    enderecoEntrega: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.enderecoEntrega()) : null,
    bairro: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.bairro()) : null,
    referencia: this.tipoEntrega() === 'entrega' ? this.normalizarTexto(this.referencia()) : null
  }));
  protected readonly acompanhamentosSelecionados = computed(() =>
    this.pedidoService.listarAcompanhamentosSelecionados(this.personalizacao(), this.grupo())
  );
  protected readonly total = computed(() =>
    this.pedidoService.calcularPreco(this.prato, this.tamanho(), this.formaPagamento())
  );
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
        precisaTroco: this.personalizacao().precisaTroco,
        valorTroco: this.personalizacao().valorTroco,
        tipoEntrega: this.personalizacao().tipoEntrega,
        enderecoEntrega: this.personalizacao().enderecoEntrega,
        bairro: this.personalizacao().bairro,
        referencia: this.personalizacao().referencia
      }
    );
  });

  ngAfterViewInit(): void {
    setTimeout(() => this.painel?.nativeElement.focus());
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

  protected get resumoEntrega(): string {
    if (this.tipoEntrega() === 'retirada') {
      return 'Retirada no local';
    }

    const endereco = this.normalizarTexto(this.enderecoEntrega());
    const bairro = this.normalizarTexto(this.bairro());
    return endereco && bairro ? `${endereco} - ${bairro}` : 'aguardando endereço';
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
      this.enderecoEntrega.set('');
      this.bairro.set('');
      this.referencia.set('');
    }
  }

  protected atualizarValorTroco(valor: string | number | null): void {
    this.valorTrocoTexto.set(valor === null ? '' : String(valor));
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

  private normalizarTexto(texto: string): string | null {
    const valor = texto.trim();
    return valor ? valor : null;
  }
}
