import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-card-mascote',
  standalone: true,
  template: `
    <aside class="card-mascote" aria-label="Mensagem da Tininha">
      <img src="/assets/mascote-tininha.svg" alt="Mascote da Quentinhas da Tininha" />
      <p><strong>Feito com amor</strong><br />para você! <span aria-hidden="true">♥</span></p>
    </aside>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CardMascoteComponent {}
