import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-logo-marca',
  standalone: true,
  template: `
    <img
      class="logo-marca-imagem"
      [class.logo-marca-imagem--grande]="grande"
      src="/assets/logo-tininha-nova.png"
      alt="Quentinhas da Tininha"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogoMarcaComponent {
  @Input() grande = false;
}
