import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-controle-funcionamento',
  standalone: true,
  template: '<div class="cartao admin-card"><h2>Controle de funcionamento preparado</h2></div>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ControleFuncionamentoComponent {}
