import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ResumoPainel } from '../../../nucleo/autenticacao/autenticacao.model';
import { PainelAdministrativoService } from '../../servicos/painel-administrativo.service';

interface CardResumo {
  icone: string;
  titulo: string;
  valor: string;
  descricao: string;
  status: 'neutro' | 'sucesso' | 'alerta';
}

@Component({
  selector: 'app-painel-page',
  standalone: true,
  imports: [RouterLink],
  template: `
    <section class="admin-pagina">
      <header class="admin-pagina__cabecalho">
        <span class="admin-tag">Administracao</span>
        <h1>Painel administrativo</h1>
        <p>Gerencie o cardapio e o funcionamento do restaurante.</p>
      </header>

      @if (carregando()) {
        <div class="admin-skeleton-grid" aria-label="Carregando dados do painel">
          <span></span><span></span><span></span><span></span><span></span>
        </div>
      } @else if (mensagemErro()) {
        <div class="admin-estado">
          <h2>Nao foi possivel carregar os dados do painel.</h2>
          <p>Confira se a API esta disponivel e tente novamente.</p>
          <button class="botao" type="button" (click)="carregarResumo()">Tentar novamente</button>
        </div>
      } @else if (resumo(); as dados) {
        <div class="admin-resumo-grid">
          @for (card of cardsResumo(); track card.titulo) {
            <article class="admin-resumo-card" [class.admin-resumo-card--sucesso]="card.status === 'sucesso'" [class.admin-resumo-card--alerta]="card.status === 'alerta'">
              <span aria-hidden="true">{{ card.icone }}</span>
              <div>
                <small>{{ card.titulo }}</small>
                <strong>{{ card.valor }}</strong>
                <p>{{ card.descricao }}</p>
              </div>
            </article>
          }
        </div>

        <section class="admin-bloco">
          <div>
            <span class="admin-tag">Acoes rapidas</span>
            <h2>O que voce quer ajustar agora?</h2>
          </div>
          <div class="admin-acoes-rapidas">
            <a routerLink="/admin/pratos">Gerenciar pratos</a>
            <a routerLink="/admin/acompanhamentos">Gerenciar acompanhamentos</a>
            <a routerLink="/admin/bebidas">Gerenciar bebidas</a>
            <a routerLink="/admin/funcionamento">Alterar funcionamento</a>
            <a href="/" target="_blank" rel="noopener">Ver pagina publica</a>
          </div>
        </section>

        <section class="admin-bloco admin-bloco--duplo">
          <div>
            <span class="admin-tag">Cardapio de hoje</span>
            <h2>{{ dados.nomeDiaSemana }}</h2>
            <p>Total de {{ dados.quantidadePratosHoje }} pratos no cardapio, com {{ dados.quantidadePratosDisponiveis }} disponiveis e {{ dados.quantidadePratosIndisponiveis }} indisponiveis.</p>
          </div>
          <a class="botao secundario" routerLink="/admin/pratos">Gerenciar cardapio</a>
        </section>
      } @else {
        <div class="admin-estado">
          <h2>Nenhum dado para exibir ainda.</h2>
          <p>Assim que a API responder, o resumo do restaurante aparecera aqui.</p>
        </div>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PainelPage implements OnInit {
  private readonly painelService = inject(PainelAdministrativoService);

  protected readonly resumo = signal<ResumoPainel | null>(null);
  protected readonly carregando = signal(false);
  protected readonly mensagemErro = signal('');
  protected readonly cardsResumo = signal<CardResumo[]>([]);

  ngOnInit(): void {
    this.carregarResumo();
  }

  protected carregarResumo(): void {
    this.carregando.set(true);
    this.mensagemErro.set('');

    this.painelService.obterResumo()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
      next: (resumo) => {
        this.resumo.set(resumo);
        this.cardsResumo.set(this.montarCards(resumo));
      },
      error: () => {
        this.mensagemErro.set('Nao foi possivel carregar os dados do painel.');
      }
    });
  }

  private montarCards(resumo: ResumoPainel): CardResumo[] {
    return [
      {
        icone: resumo.restauranteAberto ? '●' : '○',
        titulo: 'Status do restaurante',
        valor: resumo.restauranteAberto ? 'Aberto' : 'Fechado',
        descricao: resumo.mensagemStatus,
        status: resumo.restauranteAberto ? 'sucesso' : 'alerta'
      },
      {
        icone: '▦',
        titulo: 'Pratos no cardapio de hoje',
        valor: String(resumo.quantidadePratosHoje),
        descricao: resumo.nomeDiaSemana,
        status: 'neutro'
      },
      {
        icone: '✓',
        titulo: 'Pratos disponiveis',
        valor: String(resumo.quantidadePratosDisponiveis),
        descricao: 'Podem ser pedidos hoje.',
        status: 'sucesso'
      },
      {
        icone: '!',
        titulo: 'Pratos indisponiveis',
        valor: String(resumo.quantidadePratosIndisponiveis),
        descricao: 'Aparecem com aviso para o cliente.',
        status: resumo.quantidadePratosIndisponiveis > 0 ? 'alerta' : 'neutro'
      },
      {
        icone: '☑',
        titulo: 'Acompanhamentos indisponiveis',
        valor: String(resumo.quantidadeAcompanhamentosIndisponiveis),
        descricao: 'Continuam visiveis, mas bloqueados.',
        status: resumo.quantidadeAcompanhamentosIndisponiveis > 0 ? 'alerta' : 'neutro'
      },
      {
        icone: '◍',
        titulo: 'Atalho bebidas',
        valor: 'Admin',
        descricao: 'Cadastre e mantenha bebidas opcionais.',
        status: 'neutro'
      }
    ];
  }
}
