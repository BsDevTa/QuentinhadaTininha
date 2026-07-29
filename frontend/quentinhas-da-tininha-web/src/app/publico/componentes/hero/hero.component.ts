import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { LogoMarcaComponent } from '../../../compartilhado/componentes/logo-marca/logo-marca.component';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [LogoMarcaComponent],
  template: `
    <section id="inicio" class="hero-publico">
      <div class="hero-publico__texto">
        <app-logo-marca [grande]="true" />

        <h1>
          <span>Comida 100% caseira</span>
          <strong>com o melhor sabor e o menor pre&ccedil;o!</strong>
        </h1>

        <p>Card&aacute;pio variado todos os dias da semana para deixar sua refei&ccedil;&atilde;o mais pr&aacute;tica e deliciosa!</p>

        <div class="acoes">
          <a class="botao" href="#cardapio"><span aria-hidden="true">&#9633;</span> Ver card&aacute;pio de hoje</a>
        </div>
      </div>

      <div class="hero-publico__prato" aria-label="Prato de comida caseira em destaque" role="img">
        <img src="/assets/prato-hero-transparente.png" alt="Prato real de comida caseira com arroz, feijao, farofa, salada, macarrao e frango assado" />
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeroComponent {
  @Input({ required: true }) linkWhatsapp = '';
}
