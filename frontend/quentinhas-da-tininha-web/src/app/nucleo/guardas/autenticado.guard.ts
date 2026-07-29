import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AutenticacaoService } from '../autenticacao/autenticacao.service';

export const autenticadoGuard: CanActivateFn = (_route, state) => {
  const autenticacaoService = inject(AutenticacaoService);
  const router = inject(Router);
  const loginUrl = router.createUrlTree(['/admin/login'], {
    queryParams: { returnUrl: state.url }
  });

  if (autenticacaoService.estaAutenticado()) {
    return true;
  }

  if (!autenticacaoService.tokenValido()) {
    autenticacaoService.limparSessao();
    return loginUrl;
  }

  return autenticacaoService.restaurarSessao().pipe(
    map((autenticado) => autenticado ? true : loginUrl)
  );
};
