import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RestauranteStatus } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-resumo-status',
  standalone: true,
  template: `
    <section id="status" class="cartao admin-card">
      <span class="tag">Status</span>
      <h2>{{ status.estaAberto ? 'Restaurante aberto' : 'Restaurante fechado' }}</h2>
      <p class="texto-suave">{{ status.mensagemStatus }}</p>
      <button class="botao" type="button" (click)="alternar.emit(!status.estaAberto)">
        {{ status.estaAberto ? 'Fechar restaurante' : 'Abrir restaurante' }}
      </button>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResumoStatusComponent {
  @Input({ required: true }) status!: RestauranteStatus;
  @Output() readonly alternar = new EventEmitter<boolean>();
}
