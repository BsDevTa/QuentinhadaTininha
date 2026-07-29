import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-formulario-prato',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: '<div class="cartao admin-card"><h2>Formulário de prato preparado</h2><p class="texto-suave">A edição será conectada à API administrativa.</p></div>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormularioPratoComponent {}
