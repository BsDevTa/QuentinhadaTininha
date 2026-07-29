import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-beneficios',
  standalone: true,
  template: `
    <section class="beneficios" aria-label="Benefícios da Quentinhas da Tininha">
      @for (beneficio of beneficios; track beneficio.titulo) {
        <article class="beneficio-card">
          <span aria-hidden="true">{{ beneficio.icone }}</span>
          <div>
            <h3>{{ beneficio.titulo }}</h3>
            <p>{{ beneficio.descricao }}</p>
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BeneficiosComponent {
  protected readonly beneficios = [
    { icone: '🍲', titulo: 'Comida 100% caseira', descricao: 'Feita com ingredientes frescos e selecionados' },
    { icone: '♥', titulo: 'Feito com amor', descricao: 'Cada quentinha preparada com carinho para você' },
    { icone: '🏷', titulo: 'Melhor preço', descricao: 'Qualidade que cabe no seu bolso' },
    { icone: '🛵', titulo: 'Entrega rápida', descricao: 'Receba sua quentinha quente e fresquinha' }
  ];
}
