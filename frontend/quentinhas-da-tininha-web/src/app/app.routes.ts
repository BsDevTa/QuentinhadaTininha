import { Routes } from '@angular/router';
import { autenticadoGuard } from './nucleo/guardas/autenticado.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./publico/paginas/inicio/inicio.page').then(
        (m) => m.InicioPage
      )
  },
  {
    path: 'cardapio',
    redirectTo: ''
  },
  {
    path: 'admin/login',
    loadComponent: () =>
      import('./administrativo/paginas/login/login.page').then(
        (m) => m.LoginPage
      )
  },
  {
    path: 'admin',
    canActivate: [autenticadoGuard],
    loadComponent: () =>
      import('./administrativo/layout/layout-administrativo.component').then(
        (m) => m.LayoutAdministrativoComponent
      ),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'painel'
      },
      {
        path: 'painel',
        loadComponent: () =>
          import('./administrativo/paginas/painel/painel.page').then(
            (m) => m.PainelPage
          )
      },
      {
        path: 'pratos',
        loadComponent: () =>
          import('./administrativo/paginas/pratos/pratos.page').then(
            (m) => m.PratosPage
          )
      },
      {
        path: 'acompanhamentos',
        loadComponent: () =>
          import('./administrativo/paginas/acompanhamentos/acompanhamentos.page').then(
            (m) => m.AcompanhamentosPage
          )
      },
      {
        path: 'funcionamento',
        loadComponent: () =>
          import('./administrativo/paginas/funcionamento/funcionamento.page').then(
            (m) => m.FuncionamentoPage
          )
      },
      {
        path: 'fretes-bairros',
        loadComponent: () =>
          import('./administrativo/paginas/fretes-bairros/fretes-bairros.page').then(
            (m) => m.FretesBairrosPage
          )
      },
      {
        path: 'configuracoes',
        loadComponent: () =>
          import('./administrativo/paginas/configuracoes/configuracoes.page').then(
            (m) => m.ConfiguracoesPage
          )
      },
      {
        path: '**',
        redirectTo: 'painel'
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
