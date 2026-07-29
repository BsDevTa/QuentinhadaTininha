import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Restaurante } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-status-restaurante',
  standalone: true,
  template: `
    <section class="status-restaurante">
      <div class="container">
        <div class="cartao">
          <div>
            <span class="tag">Funcionamento</span>
            <h2 [class.fechado]="!restaurante.estaAberto" class="selo-status">
              {{ restaurante.estaAberto ? 'Estamos abertos' : 'Estamos fechados hoje' }}
            </h2>
            <p class="texto-suave">{{ restaurante.mensagemStatus }}</p>
          </div>
          <strong>{{ restaurante.horarioFuncionamento }}</strong>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatusRestauranteComponent {
  @Input({ required: true }) restaurante!: Restaurante;
}
