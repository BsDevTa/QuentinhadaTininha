import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-sobre',
  standalone: true,
  template: `
    <section id="sobre" class="secao">
      <div class="container cartao contato-card">
        <span class="tag">Sobre nós</span>
        <h2 class="titulo-secao">Quentinha com jeito de casa</h2>
        <p class="texto-suave">A Quentinhas da Tininha nasceu para servir comida simples, gostosa e caprichada, com atendimento próximo e aquele tempero de família.</p>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SobreComponent {}
