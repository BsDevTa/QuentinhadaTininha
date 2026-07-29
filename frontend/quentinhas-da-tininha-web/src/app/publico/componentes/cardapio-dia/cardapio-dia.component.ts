import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges, signal } from '@angular/core';
import { CardapioDia, DiaSemana } from '../../../compartilhado/modelos/cardapio.model';
import { DiaSemanaPipe } from '../../../compartilhado/utilitarios/dia-semana.pipe';
import { CardMascoteComponent } from '../card-mascote/card-mascote.component';
import { CartaoPratoComponent } from '../cartao-prato/cartao-prato.component';
import { SeletorDiaComponent } from '../seletor-dia/seletor-dia.component';

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
          (selecionar)="selecionarDia($event)"
        />
        <app-card-mascote />
      </div>

      <div class="cardapio-ref__lista">
        <header class="cardapio-ref__cabecalho">
          <div>
            <h2>{{ titulo }}</h2>
            <span></span>
          </div>
          <strong><span aria-hidden="true">▣</span> Cardapio do dia</strong>
        </header>

        @if (cardapio.pratos.length > 0) {
          <div class="lista-pratos">
            @for (prato of cardapio.pratos; track prato.id) {
              <app-cartao-prato
                [prato]="prato"
                [aberto]="pratoAbertoId() === prato.id"
                [whatsappRestaurante]="whatsappRestaurante"
                [restauranteAberto]="restauranteAberto"
                (alternar)="alternarPrato($event)"
              />
            }
          </div>
        } @else {
          <div class="estado-cardapio">
            <h3>{{ diaSelecionado === 0 ? 'Domingo sem cardapio' : 'Nao ha cardapio disponivel para este dia.' }}</h3>
            <p>{{ mensagemStatus || (diaSelecionado === 0 ? 'Hoje nao temos atendimento. Consulte o cardapio dos outros dias.' : 'Volte em breve para conferir as opcoes da Tininha.') }}</p>
          </div>
        }
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardapioDiaComponent implements OnChanges {
  @Input({ required: true }) cardapio!: CardapioDia;
  @Input({ required: true }) diaSelecionado!: DiaSemana;
  @Input({ required: true }) diaAtual!: DiaSemana;
  @Input({ required: true }) selecionarDia!: (dia: DiaSemana) => void;
  @Input({ required: true }) whatsappRestaurante!: string;
  @Input({ required: true }) restauranteAberto!: boolean;
  @Input() mensagemStatus = '';

  private readonly diaSemanaPipe = new DiaSemanaPipe();
  protected readonly pratoAbertoId = signal<string | null>(null);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['diaSelecionado']) {
      this.pratoAbertoId.set(null);
    }
  }

  protected get titulo(): string {
    const nomeDia = this.diaSemanaPipe.transform(this.diaSelecionado);
    return this.diaSelecionado === 0 ? 'Cardapio da semana' : `Cardapio de ${nomeDia}`;
  }

  protected alternarPrato(pratoId: string): void {
    const prato = this.cardapio.pratos.find((item) => item.id === pratoId);
    if (!prato?.estaDisponivel) {
      return;
    }

    this.pratoAbertoId.set(this.pratoAbertoId() === pratoId ? null : pratoId);
  }
}
