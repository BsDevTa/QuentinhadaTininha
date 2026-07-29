import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, computed, inject, signal } from '@angular/core';
import {
  FormaPagamento,
  PersonalizacaoPedido,
  Prato,
  TamanhoRefeicao
} from '../../../compartilhado/modelos/cardapio.model';
import { PedidoService } from '../../../compartilhado/servicos/pedido.service';
import { WhatsappService } from '../../../compartilhado/servicos/whatsapp.service';

@Component({
  selector: 'app-cartao-prato',
  standalone: true,
  imports: [CurrencyPipe],
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
                  <input type="radio" [name]="'pagamento-' + prato.id" [checked]="formaPagamento() === opcao.valor" (change)="formaPagamento.set(opcao.valor)" />
                  <span>{{ opcao.rotulo }}</span>
                </label>
              }
            </div>
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

          <section class="resumo-pedido">
            <h4>Seu pedido</h4>
            <p><strong>Prato:</strong> {{ prato.nome }}</p>
            <p><strong>Tamanho:</strong> {{ tamanho() }}</p>
            <p><strong>Pagamento:</strong> {{ rotuloPagamento }}</p>
            <p><strong>Acompanhamentos:</strong> {{ resumoAcompanhamentos }}</p>
            <strong class="total-pedido">Total: {{ total() | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
          </section>

          @if (linkWhatsapp(); as link) {
            <a class="botao botao-pedido-expandido" [href]="link" target="_blank" rel="noopener" aria-label="Pedir {{ prato.nome }} pelo WhatsApp">
              Pedir pelo WhatsApp
            </a>
          } @else {
            <button class="botao botao-pedido-expandido" type="button" disabled>
              Pedido indisponivel no momento
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
  protected readonly formaPagamento = signal<FormaPagamento>('dinheiro_pix');
  protected readonly acompanhamentoIds = signal<string[]>([]);
  protected readonly tipoFeijaoId = signal<string | null>(null);

  protected readonly tamanhos: { valor: TamanhoRefeicao; rotulo: string }[] = [
    { valor: 'P', rotulo: 'Pequena' },
    { valor: 'G', rotulo: 'Grande' }
  ];
  protected readonly pagamentos: { valor: FormaPagamento; rotulo: string }[] = [
    { valor: 'dinheiro_pix', rotulo: 'Dinheiro ou Pix' },
    { valor: 'cartao', rotulo: 'Cartao' }
  ];

  protected readonly grupo = computed(() => this.pedidoService.obterGrupo(this.prato));
  protected readonly personalizacao = computed<PersonalizacaoPedido>(() => ({
    pratoId: this.prato.id,
    tamanho: this.tamanho(),
    formaPagamento: this.formaPagamento(),
    acompanhamentoIds: this.acompanhamentoIds(),
    tipoFeijaoId: this.tipoFeijaoId()
  }));
  protected readonly acompanhamentosSelecionados = computed(() =>
    this.pedidoService.listarAcompanhamentosSelecionados(this.personalizacao(), this.grupo())
  );
  protected readonly total = computed(() =>
    this.pedidoService.calcularPreco(this.prato, this.tamanho(), this.formaPagamento())
  );
  protected readonly linkWhatsapp = computed(() => {
    if (!this.restauranteAberto || !this.whatsappRestaurante.trim()) {
      return null;
    }

    return this.whatsappService.criarLinkPedido(
      this.prato,
      this.tamanho(),
      this.formaPagamento(),
      this.acompanhamentosSelecionados(),
      this.total(),
      this.whatsappRestaurante
    );
  });

  protected get resumoAcompanhamentos(): string {
    const nomes = this.acompanhamentosSelecionados().map((acompanhamento) => acompanhamento.nome);
    return nomes.length > 0 ? nomes.join(', ') : 'sem acompanhamentos selecionados';
  }

  protected get rotuloPagamento(): string {
    return this.pedidoService.rotuloPagamento(this.formaPagamento());
  }

  protected deveMostrarImagem(): boolean {
    return Boolean(this.prato.urlImagem.trim()) && !this.imagemFalhou();
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
}
