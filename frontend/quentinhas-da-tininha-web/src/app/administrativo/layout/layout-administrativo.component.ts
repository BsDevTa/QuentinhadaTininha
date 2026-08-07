import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { LogoMarcaComponent } from '../../compartilhado/componentes/logo-marca/logo-marca.component';
import { AutenticacaoService } from '../../nucleo/autenticacao/autenticacao.service';

interface ItemMenuAdmin {
  rotulo: string;
  icone: string;
  rota: string;
}

@Component({
  selector: 'app-layout-administrativo',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LogoMarcaComponent],
  template: `
    <div class="admin-shell" [class.admin-shell--menu-aberto]="menuAberto()">
      <button class="admin-overlay" type="button" aria-label="Fechar menu" (click)="fecharMenu()"></button>

      <aside class="admin-sidebar" aria-label="Menu administrativo">
        <div class="admin-sidebar__marca">
          <app-logo-marca />
          <button class="admin-sidebar__fechar" type="button" aria-label="Fechar menu" (click)="fecharMenu()">×</button>
        </div>

        <div class="admin-sidebar__usuario">
          <span>{{ inicialUsuario() }}</span>
          <div>
            <strong>{{ usuario()?.nome || 'Administrador' }}</strong>
            <small>{{ usuario()?.email || 'Sessao administrativa' }}</small>
          </div>
        </div>

        <nav class="admin-menu">
          @for (item of itensMenu; track item.rota) {
            <a
              [routerLink]="item.rota"
              routerLinkActive="ativo"
              [routerLinkActiveOptions]="{ exact: item.rota === '/admin/painel' }"
              (click)="fecharMenu()"
            >
              <span aria-hidden="true">{{ item.icone }}</span>
              {{ item.rotulo }}
            </a>
          }
        </nav>

        <button class="admin-sair" type="button" (click)="sair()">
          <span aria-hidden="true">↩</span>
          Sair
        </button>
      </aside>

      <div class="admin-corpo">
        <header class="admin-topbar">
          <button class="admin-menu-botao" type="button" aria-label="Abrir menu" (click)="abrirMenu()">☰</button>
          <div>
            <strong>{{ tituloPagina() }}</strong>
            <span>{{ subtituloPagina() }}</span>
          </div>
          <div class="admin-topbar__acoes">
            <span class="admin-status" [class.admin-status--fechado]="!restauranteAberto()">
              {{ restauranteAberto() ? 'Aberto' : 'Fechado' }}
            </span>
            <a class="botao secundario admin-link-publico" href="/" target="_blank" rel="noopener">Ver pagina publica</a>
            <span class="admin-avatar" aria-hidden="true">{{ inicialUsuario() }}</span>
          </div>
        </header>

        <main class="admin-main">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutAdministrativoComponent {
  private readonly autenticacaoService = inject(AutenticacaoService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly menuAberto = signal(false);
  protected readonly restauranteAberto = signal(true);
  protected readonly usuario = this.autenticacaoService.usuarioAtual;
  protected readonly itensMenu: ItemMenuAdmin[] = [
    { rotulo: 'Painel', icone: '▦', rota: '/admin/painel' },
    { rotulo: 'Pratos', icone: '◉', rota: '/admin/pratos' },
    { rotulo: 'Acompanhamentos', icone: '☑', rota: '/admin/acompanhamentos' },
    { rotulo: 'Funcionamento', icone: '◷', rota: '/admin/funcionamento' },
    { rotulo: 'Fretes por bairro', icone: '⌂', rota: '/admin/fretes-bairros' },
    { rotulo: 'Configuracoes', icone: '⚙', rota: '/admin/configuracoes' }
  ];

  protected readonly inicialUsuario = computed(() =>
    (this.usuario()?.nome || 'A').trim().charAt(0).toUpperCase()
  );
  protected readonly tituloPagina = signal('Painel administrativo');
  protected readonly subtituloPagina = signal('Gerencie o cardapio e o funcionamento do restaurante.');

  constructor() {
    this.atualizarTitulo(this.router.url);
    this.router.events
      .pipe(
        filter((evento): evento is NavigationEnd => evento instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((evento) => this.atualizarTitulo(evento.urlAfterRedirects));
  }

  protected abrirMenu(): void {
    this.menuAberto.set(true);
    document.body.classList.add('admin-menu-travado');
  }

  protected fecharMenu(): void {
    this.menuAberto.set(false);
    document.body.classList.remove('admin-menu-travado');
  }

  protected sair(): void {
    this.autenticacaoService.sair();
    this.fecharMenu();
    void this.router.navigateByUrl('/admin/login');
  }

  private atualizarTitulo(url: string): void {
    const titulos: Record<string, { titulo: string; subtitulo: string }> = {
      '/admin/painel': {
        titulo: 'Painel administrativo',
        subtitulo: 'Gerencie o cardapio e o funcionamento do restaurante.'
      },
      '/admin/pratos': {
        titulo: 'Pratos',
        subtitulo: 'Cadastre, edite e organize os pratos do cardapio.'
      },
      '/admin/acompanhamentos': {
        titulo: 'Acompanhamentos',
        subtitulo: 'Organize as opcoes que acompanham cada quentinha.'
      },
      '/admin/funcionamento': {
        titulo: 'Funcionamento',
        subtitulo: 'Acompanhe horarios, abertura e mensagens de atendimento.'
      },
      '/admin/fretes-bairros': {
        titulo: 'Fretes por bairro',
        subtitulo: 'Defina bairros atendidos e valores de entrega.'
      },
      '/admin/configuracoes': {
        titulo: 'Configuracoes',
        subtitulo: 'Prepare os dados publicos e preferencias do restaurante.'
      }
    };
    const dados = titulos[url] ?? titulos['/admin/painel'];
    this.tituloPagina.set(dados.titulo);
    this.subtituloPagina.set(dados.subtitulo);
  }
}
