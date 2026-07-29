import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, signal } from '@angular/core';
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
  selector: 'app-cartao-prato',
  standalone: true,
  imports: [CurrencyPipe, FormsModule],
  template: `
    <article class="item-prato" [class.item-prato--indisponivel]="!prato.estaDisponivel" [class.item-prato--aberto]="aberto">
      <button
        class="item-prato__linha"
        type="button"
        [disabled]="!prato.estaDisponivel"
        [attr.aria-expanded]="aberto"
        [attr.aria-controls]="'personalizacao-' + prato.id"
        (click)="alternar.emit(prato.id)"
      >
        <span class="item-prato__imagem">
          @if (deveMostrarImagem()) {
            <img [src]="prato.urlImagem" [alt]="prato.nome" loading="lazy" (error)="imagemFalhou.set(true)" />
          } @else {
            <img src="/assets/prato-hero-real.png" [alt]="prato.nome" loading="lazy" />
          }
        </span>

        <span class="item-prato__texto">
          <span>
            <h3>{{ prato.nome }}</h3>
            <p>{{ prato.descricao }}</p>
          </span>
          <small>A partir de {{ prato.precos.pequenaDinheiroPix | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</small>
        </span>

        <span class="item-prato__acao">
          @if (prato.estaDisponivel) {
            <i aria-hidden="true">{{ aberto ? '^' : 'v' }}</i>
            <span aria-hidden="true">♥</span>
          } @else {
            <em>Indisponivel hoje</em>
          }
        </span>
      </button>

      @if (aberto) {
        <div class="personalizacao-prato" [id]="'personalizacao-' + prato.id">
          <section>
            <h4>1. Escolha o tamanho</h4>
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

          <section>
            <h4>2. Escolha a forma de pagamento</h4>
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
                  <label [class.ativo]="!precisaTroco()">
                    <input type="radio" [name]="'troco-' + prato.id" [checked]="!precisaTroco()" (change)="precisaTroco.set(false); valorTrocoTexto.set('')" />
                    Não
                  </label>
                  <label [class.ativo]="precisaTroco()">
                    <input type="radio" [name]="'troco-' + prato.id" [checked]="precisaTroco()" (change)="precisaTroco.set(true)" />
                    Sim
                  </label>
                </div>

                @if (precisaTroco()) {
                  <label class="campo-pedido">
                    Troco para quanto?
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      inputmode="decimal"
                      placeholder="Ex.: 50,00"
                      [ngModel]="valorTrocoTexto()"
                      (ngModelChange)="atualizarValorTroco($event)"
                    />
                  </label>
                  @if (erroTroco(); as erro) {
                    <small class="aviso-pedido">{{ erro }}</small>
                  }
                }
              </div>
            }
          </section>

          <section>
            <h4>3. Escolha os acompanhamentos</h4>

            @if (grupo().tipoFeijao.length > 0) {
              <div class="grupo-opcoes">
                <strong>Tipo de feijao</strong>
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
                      {{ feijao.nome }} @if (!feijao.estaDisponivel) { <em>Indisponivel</em> }
                    </label>
                  }
                  <label>
                    <input type="radio" [name]="'feijao-' + prato.id" [checked]="tipoFeijaoId() === null" (change)="tipoFeijaoId.set(null)" />
                    Sem feijao
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
                    {{ acompanhamento.nome }} @if (!acompanhamento.estaDisponivel) { <em>Indisponivel</em> }
                  </label>
                }
              </div>
            </div>
          </section>

          <section>
            <h4>4. Escolha retirada ou entrega</h4>
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

          <section class="resumo-pedido">
            <h4>Seu pedido</h4>
            <p><strong>Prato:</strong> {{ prato.nome }}</p>
            <p><strong>Tamanho:</strong> {{ tamanho() }}</p>
            <p><strong>Pagamento:</strong> {{ rotuloPagamento }}</p>
            @if (formaPagamento() === 'dinheiro') {
              <p><strong>Troco:</strong> {{ resumoTroco }}</p>
            }
            <p><strong>Entrega:</strong> {{ resumoEntrega }}</p>
            <p><strong>Acompanhamentos:</strong> {{ resumoAcompanhamentos }}</p>
            <strong class="total-pedido">Total: {{ total() | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
          </section>

          @if (linkWhatsapp(); as link) {
            <a class="botao botao-pedido-expandido" [href]="link" target="_blank" rel="noopener" aria-label="Pedir {{ prato.nome }} pelo WhatsApp">
              Pedir pelo WhatsApp
            </a>
          } @else {
            <button class="botao botao-pedido-expandido" type="button" disabled>
              {{ textoBotaoIndisponivel }}
            </button>
          }
        </div>
      }
    </article>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CartaoPratoComponent {
  private readonly pedidoService = inject(PedidoService);
  private readonly whatsappService = inject(WhatsappService);

  @Input({ required: true }) prato!: Prato;
  @Input({ required: true }) aberto = false;
  @Input({ required: true }) whatsappRestaurante = '';
  @Input({ required: true }) restauranteAberto = true;
  @Output() readonly alternar = new EventEmitter<string>();

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

  protected get textoBotaoIndisponivel(): string {
    if (!this.restauranteAberto) {
      return 'Pedido indisponivel no momento';
    }

    return 'Complete os dados do pedido';
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
