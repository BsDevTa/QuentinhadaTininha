import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AtualizacaoPrato, Prato } from '../../../compartilhado/modelos/cardapio.model';
import { MoedaBrPipe } from '../../../compartilhado/utilitarios/moeda-br.pipe';

@Component({
  selector: 'app-lista-pratos',
  standalone: true,
  imports: [FormsModule, MoedaBrPipe],
  template: `
    <section id="pratos">
      <h2>Pratos do dia</h2>
      <div class="grade-admin">
        @for (prato of pratos; track prato.id) {
          <article class="cartao admin-card">
            <h3>{{ prato.nome }}</h3>
            <p class="texto-suave">{{ prato.descricao }}</p>
            <strong class="preco">{{ prato.preco | moedaBr }}</strong>
            <div class="admin-acoes">
              <button class="botao secundario" type="button" (click)="iniciarEdicao(prato)">Editar prato</button>
              <label class="botao secundario">
                Trocar imagem
                <input type="file" accept="image/*" hidden (change)="selecionarImagem(prato.id, $event)" />
              </label>
              <button class="botao" type="button" (click)="disponibilidade.emit({ id: prato.id, disponivel: !prato.estaDisponivel })">
                {{ prato.estaDisponivel ? 'Marcar indisponível' : 'Marcar disponível' }}
              </button>
            </div>
            @if (pratoEditando() === prato.id) {
              <div class="campo">
                <label>Nome<input [ngModel]="obterEdicao(prato).nome" (ngModelChange)="alterarCampo(prato, 'nome', $event)" /></label>
                <label>Descrição<textarea [ngModel]="obterEdicao(prato).descricao" (ngModelChange)="alterarCampo(prato, 'descricao', $event)"></textarea></label>
                <label>Preço<input type="number" min="0" step="0.01" [ngModel]="obterEdicao(prato).preco" (ngModelChange)="alterarCampo(prato, 'preco', $event)" /></label>
                <button class="botao" type="button" (click)="salvar(prato)">Salvar alterações</button>
              </div>
            }
          </article>
        }
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListaPratosComponent {
  @Input({ required: true }) pratos: Prato[] = [];
  @Output() readonly disponibilidade = new EventEmitter<{ id: string; disponivel: boolean }>();
  @Output() readonly editar = new EventEmitter<{ id: string; dados: AtualizacaoPrato }>();
  @Output() readonly trocarImagem = new EventEmitter<{ id: string; arquivo: File }>();
  protected readonly pratoEditando = signal<string | null>(null);
  private readonly edicoes = signal<Record<string, AtualizacaoPrato>>({});

  protected iniciarEdicao(prato: Prato): void {
    this.pratoEditando.set(prato.id);
    this.edicoes.update((edicoes) => ({
      ...edicoes,
      [prato.id]: {
        nome: prato.nome,
        descricao: prato.descricao,
        preco: prato.preco
      }
    }));
  }

  protected obterEdicao(prato: Prato): AtualizacaoPrato {
    return this.edicoes()[prato.id] ?? {
      nome: prato.nome,
      descricao: prato.descricao,
      preco: prato.preco
    };
  }

  protected alterarCampo(prato: Prato, campo: keyof AtualizacaoPrato, valor: string | number): void {
    const edicaoAtual = this.obterEdicao(prato);
    this.edicoes.update((edicoes) => ({
      ...edicoes,
      [prato.id]: {
        ...edicaoAtual,
        [campo]: campo === 'preco' ? Number(valor) : String(valor)
      }
    }));
  }

  protected salvar(prato: Prato): void {
    this.editar.emit({ id: prato.id, dados: this.obterEdicao(prato) });
    this.pratoEditando.set(null);
  }

  protected selecionarImagem(id: string, evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const arquivo = input.files?.[0];
    if (arquivo) {
      this.trocarImagem.emit({ id, arquivo });
    }
    input.value = '';
  }
}
