import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { LogoMarcaComponent } from '../../../compartilhado/componentes/logo-marca/logo-marca.component';
import { AutenticacaoService } from '../../../nucleo/autenticacao/autenticacao.service';

@Component({
  selector: 'app-menu-lateral',
  standalone: true,
  imports: [LogoMarcaComponent],
  template: `
    <aside class="menu-lateral">
      <app-logo-marca />
      <nav>
        <a href="#status">Funcionamento</a>
        <a href="#pratos">Pratos do dia</a>
        <a href="#acompanhamentos">Acompanhamentos</a>
      </nav>
      <button class="botao secundario" type="button" (click)="sair()">Sair</button>
    </aside>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MenuLateralComponent {
  private readonly autenticacaoService = inject(AutenticacaoService);
  private readonly router = inject(Router);

  protected sair(): void {
    this.autenticacaoService.sair();
    void this.router.navigateByUrl('/admin/login');
  }
}
