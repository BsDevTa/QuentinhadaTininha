import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { DiaSemana } from '../../../compartilhado/modelos/cardapio.model';

export interface EstadoDiaSeletor {
  data: string;
  permitirPedidos: boolean;
  motivoBloqueio?: string | null;
  motivo?: string | null;
}

export interface DiaBloqueadoSelecionado {
  dia: DiaSemana;
  nome: string;
  motivo: string;
  data: string;
}

@Component({
  selector: 'app-seletor-dia',
  standalone: true,
  template: `
    <aside class="seletor-dia-card">
      <div class="seletor-dia-card__topo">
        <strong>Dias da semana</strong>
      </div>

      <div class="seletor-dia" aria-label="Selecionar dia da semana">
        @for (dia of dias; track dia.valor) {
          <button
            type="button"
            [class.ativo]="dia.valor === diaSelecionado"
            [class.dia-atual]="dia.valor === diaAtual"
            [class.bloqueado]="diaBloqueado(dia.valor)"
            [attr.aria-disabled]="diaBloqueado(dia.valor)"
            [attr.title]="tituloBotao(dia.valor)"
            (click)="selecionarDia(dia.valor)"
          >
            <span>{{ dia.nome }}</span>
            @if (dia.valor === diaAtual) {
              <small>Hoje</small>
            }
            @if (diaBloqueado(dia.valor)) {
              <i aria-hidden="true">🔒</i>
            }
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
  @Input() statusDias: Partial<Record<DiaSemana, EstadoDiaSeletor>> = {};
  @Output() readonly selecionar = new EventEmitter<DiaSemana>();
  @Output() readonly bloqueado = new EventEmitter<DiaBloqueadoSelecionado>();

  protected readonly dias: { valor: DiaSemana; nome: string; nomeCompleto: string }[] = [
    { valor: 1, nome: 'Seg', nomeCompleto: 'Segunda-feira' },
    { valor: 2, nome: 'Ter', nomeCompleto: 'Terça-feira' },
    { valor: 3, nome: 'Qua', nomeCompleto: 'Quarta-feira' },
    { valor: 4, nome: 'Qui', nomeCompleto: 'Quinta-feira' },
    { valor: 5, nome: 'Sex', nomeCompleto: 'Sexta-feira' },
    { valor: 6, nome: 'Sáb', nomeCompleto: 'Sábado' },
    { valor: 0, nome: 'Dom', nomeCompleto: 'Domingo' }
  ];

  protected selecionarDia(dia: DiaSemana): void {
    if (this.diaBloqueado(dia)) {
      this.abrirDiaBloqueado(dia);
      return;
    }

    this.selecionar.emit(dia);
  }

  protected diaBloqueado(dia: DiaSemana): boolean {
    return this.statusDias[dia]?.permitirPedidos === false;
  }

  protected tituloBotao(dia: DiaSemana): string {
    if (!this.diaBloqueado(dia)) {
      return `Ver cardápio de ${this.nomeCompleto(dia)}`;
    }

    return this.motivoDia(dia);
  }

  private abrirDiaBloqueado(dia: DiaSemana): void {
    const status = this.statusDias[dia];
    this.bloqueado.emit({
      dia,
      nome: this.nomeCompleto(dia),
      motivo: this.motivoDia(dia),
      data: status?.data ? this.formatarData(status.data) : ''
    });
  }

  private motivoDia(dia: DiaSemana): string {
    const status = this.statusDias[dia];
    return status?.motivoBloqueio || status?.motivo || 'Esse dia não está liberado para pedidos.';
  }

  private nomeCompleto(dia: DiaSemana): string {
    return this.dias.find((item) => item.valor === dia)?.nomeCompleto ?? 'dia selecionado';
  }

  private formatarData(dataIso: string): string {
    const [ano, mes, dia] = dataIso.split('-').map(Number);
    if (!ano || !mes || !dia) {
      return dataIso;
    }

    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(new Date(ano, mes - 1, dia));
  }
}
