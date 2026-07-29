import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { DiaSemana } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-seletor-dia',
  standalone: true,
  template: `
    <aside class="seletor-dia-card">
      <div class="seletor-dia-card__topo">
        <strong>Escolha o dia</strong>
        <span aria-hidden="true">▣</span>
      </div>

      <div class="seletor-dia" aria-label="Selecionar dia da semana">
        @for (dia of dias; track dia.valor) {
          <button
            type="button"
            [class.ativo]="dia.valor === diaSelecionado"
            [class.dia-atual]="dia.valor === diaAtual"
            (click)="selecionar.emit(dia.valor)"
          >
            <span aria-hidden="true">▣</span>
            {{ dia.nome }}
            <i aria-hidden="true">›</i>
          </button>
        }
      </div>
    </aside>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SeletorDiaComponent {
  @Input({ required: true }) diaSelecionado!: DiaSemana;
  @Input({ required: true }) diaAtual!: DiaSemana;
  @Output() readonly selecionar = new EventEmitter<DiaSemana>();

  protected readonly dias: { valor: DiaSemana; nome: string }[] = [
    { valor: 1, nome: 'Segunda-feira' },
    { valor: 2, nome: 'Terca-feira' },
    { valor: 3, nome: 'Quarta-feira' },
    { valor: 4, nome: 'Quinta-feira' },
    { valor: 5, nome: 'Sexta-feira' },
    { valor: 6, nome: 'Sabado' },
    { valor: 0, nome: 'Domingo' }
  ];
}
