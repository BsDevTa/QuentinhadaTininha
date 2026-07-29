import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Acompanhamento } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-lista-acompanhamentos',
  standalone: true,
  template: `
    <section id="acompanhamentos">
      <h2>Acompanhamentos</h2>
      <div class="grade-admin">
        @for (acompanhamento of acompanhamentos; track acompanhamento.id) {
          <article class="cartao admin-card">
            <h3>{{ acompanhamento.nome }}</h3>
            <p class="texto-suave">{{ acompanhamento.estaDisponivel ? 'Disponível para os pratos do dia.' : 'Indisponível no momento.' }}</p>
            <button class="botao" type="button" (click)="disponibilidade.emit({ id: acompanhamento.id, disponivel: !acompanhamento.estaDisponivel })">
              {{ acompanhamento.estaDisponivel ? 'Marcar indisponível' : 'Marcar disponível' }}
            </button>
          </article>
        }
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListaAcompanhamentosComponent {
  @Input({ required: true }) acompanhamentos: Acompanhamento[] = [];
  @Output() readonly disponibilidade = new EventEmitter<{ id: string; disponivel: boolean }>();
}
