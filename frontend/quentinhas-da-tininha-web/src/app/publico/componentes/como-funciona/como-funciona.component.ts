import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-como-funciona',
  standalone: true,
  template: `
    <section id="como-funciona" class="secao">
      <div class="container">
        <span class="tag">Como funciona</span>
        <h2 class="titulo-secao">Pedido simples, sem complicação</h2>
        <div class="passos">
          @for (passo of passos; track passo.titulo; let indice = $index) {
            <div class="cartao passo">
              <strong>{{ indice + 1 }}</strong>
              <h3>{{ passo.titulo }}</h3>
              <p class="texto-suave">{{ passo.texto }}</p>
            </div>
          }
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ComoFuncionaComponent {
  protected readonly passos = [
    { titulo: 'Escolha sua quentinha', texto: 'Veja as opções disponíveis no cardápio do dia.' },
    { titulo: 'Faça o pedido pelo WhatsApp', texto: 'Envie a mensagem pronta e combine os detalhes.' },
    { titulo: 'Receba ou retire seu pedido', texto: 'A Tininha prepara tudo com carinho para você.' }
  ];
}
