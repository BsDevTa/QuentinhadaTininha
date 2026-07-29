import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Restaurante } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-contato',
  standalone: true,
  template: `
    <section id="contato" class="secao">
      <div class="container contato-card cartao">
        <span class="tag">Contato</span>
        <h2 class="titulo-secao">Fale com a Tininha</h2>
        <p><strong>WhatsApp:</strong> {{ restaurante.whatsapp }}</p>
        <p><strong>Instagram:</strong> {{ restaurante.instagram }}</p>
        <p><strong>Endereço:</strong> {{ restaurante.endereco }}</p>
        <p><strong>Horário:</strong> {{ restaurante.horarioFuncionamento }}</p>
        <p><strong>Pagamento:</strong> {{ restaurante.formasPagamento.join(', ') }}</p>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContatoComponent {
  @Input({ required: true }) restaurante!: Restaurante;
}
