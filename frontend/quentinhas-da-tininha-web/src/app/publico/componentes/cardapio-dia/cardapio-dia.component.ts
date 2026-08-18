import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Bebida, CardapioDia, DiaSemana, Prato } from '../../../compartilhado/modelos/cardapio.model';
import { DiaSemanaPipe } from '../../../compartilhado/utilitarios/dia-semana.pipe';
import { CardMascoteComponent } from '../card-mascote/card-mascote.component';
import { CartaoPratoComponent } from '../cartao-prato/cartao-prato.component';
import { DiaBloqueadoSelecionado, EstadoDiaSeletor, SeletorDiaComponent } from '../seletor-dia/seletor-dia.component';

@Component({
  selector: 'app-cardapio-dia',
  standalone: true,
  imports: [CardMascoteComponent, CartaoPratoComponent, SeletorDiaComponent],
  template: `
    <section id="cardapio" class="cardapio-ref">
      <div class="cardapio-ref__lateral">
        <app-seletor-dia
          [diaSelecionado]="diaSelecionado"
          [diaAtual]="diaAtual"
          [statusDias]="statusDias"
          (selecionar)="selecionarDia($event)"
          (bloqueado)="diaBloqueado.emit($event)"
        />
        <app-card-mascote />
      </div>

      <div class="cardapio-ref__lista">
        <header class="cardapio-ref__cabecalho">
          <div>
            <span class="cardapio-ref__selo">Cardápio do dia</span>
            <h2>{{ titulo }}</h2>
          </div>
          @if (cardapio.pratos.length > 0) {
            <strong>{{ cardapio.pratos.length }} opções</strong>
          }
        </header>

        @if (cardapio.pratos.length > 0) {
          <div class="lista-pratos">
            @for (prato of cardapio.pratos; track prato.id) {
              <app-cartao-prato
                [prato]="prato"
                (personalizar)="abrirPersonalizacao($event)"
              />
            }
          </div>
        } @else {
          <div class="estado-cardapio">
            <h3>{{ diaSelecionado === 0 ? 'Domingo sem cardapio' : 'Nao ha cardapio disponivel para este dia.' }}</h3>
            <p>{{ textoEstadoVazio }}</p>
          </div>
        }

        @if ((cardapio.bebidas?.length ?? 0) > 0) {
          <section class="bebidas-opcionais">
            <header class="cardapio-ref__cabecalho cardapio-ref__cabecalho--compacto">
              <div>
                <span class="cardapio-ref__selo">Bebidas</span>
                <h2>Bebidas (opcional)</h2>
              </div>
            </header>
            <div class="bebidas-opcionais__lista">
              @for (bebida of cardapio.bebidas; track bebida.id) {
                <article>
                  <strong>{{ bebida.nome }}</strong>
                  @if (bebida.descricao) {
                    <small>{{ bebida.descricao }}</small>
                  }
                  <span>{{ formatarMoeda(bebida.preco) }}</span>
                </article>
              }
            </div>
          </section>
        }
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardapioDiaComponent {
  @Input({ required: true }) cardapio!: CardapioDia;
  @Input({ required: true }) diaSelecionado!: DiaSemana;
  @Input({ required: true }) diaAtual!: DiaSemana;
  @Input({ required: true }) selecionarDia!: (dia: DiaSemana) => void;
  @Input({ required: true }) whatsappRestaurante!: string;
  @Input({ required: true }) restauranteAberto!: boolean;
  @Input() statusDias: Partial<Record<DiaSemana, EstadoDiaSeletor>> = {};
  @Input() mensagemStatus = '';
  @Output() readonly personalizarPrato = new EventEmitter<{ prato: Prato; bebidas: Bebida[] }>();
  @Output() readonly diaBloqueado = new EventEmitter<DiaBloqueadoSelecionado>();

  private readonly diaSemanaPipe = new DiaSemanaPipe();

  protected get titulo(): string {
    const nomeDia = this.diaSemanaPipe.transform(this.diaSelecionado);
    return this.diaSelecionado === 0 ? 'Cardapio da semana' : `Cardapio de ${nomeDia}`;
  }

  protected get textoEstadoVazio(): string {
    if (!this.restauranteAberto && this.mensagemStatus) {
      return this.mensagemStatus;
    }

    return 'Volte em breve para conferir as opcoes da Tininha.';
  }

  protected abrirPersonalizacao(pratoId: string): void {
    const prato = this.cardapio.pratos.find((item) => item.id === pratoId);
    if (!prato?.estaDisponivel) {
      return;
    }

    this.personalizarPrato.emit({ prato, bebidas: this.cardapio.bebidas ?? [] });
  }

  protected formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  }
}
