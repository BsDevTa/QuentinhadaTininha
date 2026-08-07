import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Prato } from '../../../compartilhado/modelos/cardapio.model';

@Component({
  selector: 'app-cartao-prato',
  standalone: true,
  imports: [CurrencyPipe],
  template: `
    <article class="item-prato" [class.item-prato--indisponivel]="!prato.estaDisponivel">
      <div class="item-prato__linha">
        <span class="item-prato__imagem">
          @if (deveMostrarImagem()) {
            <img [src]="prato.urlImagem" [alt]="prato.nome" width="80" height="80" loading="lazy" decoding="async" (error)="imagemFalhou.set(true)" />
          } @else {
            <img src="/assets/prato-hero-real.png" [alt]="prato.nome" width="80" height="80" loading="lazy" decoding="async" />
          }
        </span>

        <div class="item-prato__texto">
          <h3>{{ prato.nome }}</h3>
          <p>{{ prato.descricao }}</p>
        </div>

        <div class="item-prato__acao">
          <small>A partir de</small>
          <strong>{{ prato.precos.pequenaDinheiroPix | currency: 'BRL' : 'symbol' : '1.2-2' : 'pt-BR' }}</strong>
          @if (prato.estaDisponivel) {
            <button
              class="item-prato__personalizar"
              type="button"
              [attr.aria-label]="'Personalizar ' + prato.nome"
              (click)="personalizar.emit(prato.id)"
            >
              Personalizar
            </button>
          } @else {
            <em>Indisponível hoje</em>
          }
        </div>
      </div>
    </article>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CartaoPratoComponent {
  @Input({ required: true }) prato!: Prato;
  @Output() readonly personalizar = new EventEmitter<string>();

  protected readonly imagemFalhou = signal(false);

  protected deveMostrarImagem(): boolean {
    return Boolean(this.prato.urlImagem.trim()) && !this.imagemFalhou();
  }
}
