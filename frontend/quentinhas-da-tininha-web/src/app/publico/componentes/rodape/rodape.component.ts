import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { LogoMarcaComponent } from '../../../compartilhado/componentes/logo-marca/logo-marca.component';
import { Restaurante } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-rodape',
  standalone: true,
  imports: [LogoMarcaComponent],
  template: `
    <footer id="contato" class="rodape-publico">
      <div class="rodape-publico__grade">
        <div class="rodape-publico__logo">
          <app-logo-marca [grande]="true" />
        </div>

        <nav aria-label="Navegação do rodapé">
          <h3>Navegação</h3>
          <a href="#cardapio">Cardápio</a>
          <a href="#sobre">Sobre nós</a>
          <a href="#como-funciona">Como funciona</a>
          <a href="#contato">Fale conosco</a>
        </nav>

        <div class="rodape-publico__contato">
          <h3>Contato</h3>
          <p><span aria-hidden="true">☏</span> {{ restaurante.whatsapp }}</p>
          <p><span aria-hidden="true">◎</span> {{ restaurante.instagram }}</p>
          <p><span aria-hidden="true">●</span> {{ restaurante.endereco }}</p>
        </div>

        <div>
          <h3>Formas de pagamento</h3>
          <div class="pagamentos">
            <img src="/assets/pagamentos/pix.svg" alt="Pix" />
            <img src="/assets/pagamentos/visa.svg" alt="Visa" />
            <img src="/assets/pagamentos/mastercard.svg" alt="Mastercard" />
            <img src="/assets/pagamentos/hipercard.svg" alt="Hipercard" />
          </div>
        </div>
      </div>
      <p class="copyright">© 2026 Quentinhas da Tininha - Todos os direitos reservados.</p>
    </footer>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RodapeComponent {
  @Input({ required: true }) restaurante!: Restaurante;
}
