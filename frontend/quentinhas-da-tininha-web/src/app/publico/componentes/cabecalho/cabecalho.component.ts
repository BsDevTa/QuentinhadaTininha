import { ChangeDetectionStrategy, Component, Input, signal } from '@angular/core';
import { LogoMarcaComponent } from '../../../compartilhado/componentes/logo-marca/logo-marca.component';

@Component({
  selector: 'app-cabecalho',
  standalone: true,
  imports: [LogoMarcaComponent],
  template: `
    <header class="cabecalho-publico">
      <div class="cabecalho-publico__conteudo">
        <a class="cabecalho-publico__logo" href="#inicio" aria-label="Ir para o início">
          <app-logo-marca />
        </a>

        <button class="menu-mobile" type="button" aria-label="Abrir menu" (click)="menuAberto.set(!menuAberto())">
          ☰
        </button>

        <nav [class.aberto]="menuAberto()" aria-label="Menu principal">
          <a class="ativo" href="#cardapio">Cardápio</a>
          <a href="#sobre">Sobre nós</a>
          <a href="#como-funciona">Como funciona</a>
          <a href="#contato">Fale conosco</a>
        </nav>

        <a class="botao botao-whatsapp" [href]="linkPedido" target="_blank" rel="noopener" aria-label="Faça seu pedido pelo WhatsApp">
          <span aria-hidden="true">☏</span>
          Faça seu pedido
        </a>
      </div>
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CabecalhoComponent {
  @Input({ required: true }) linkPedido = '#cardapio';
  protected readonly menuAberto = signal(false);
}
