import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AutenticacaoService } from '../autenticacao/autenticacao.service';

export const jwtInterceptor: HttpInterceptorFn = (requisicao, next) => {
  const autenticacaoService = inject(AutenticacaoService);
  const router = inject(Router);
  const apiUrl = environment.apiUrl.replace(/\/$/, '');
  const deveAnexarToken = requisicao.url.startsWith(apiUrl) &&
    !requisicao.url.includes('/autenticacao/entrar') &&
    !requisicao.url.includes('/autenticacao/login');
  const token = autenticacaoService.obterToken();

  const requisicaoAutenticada = deveAnexarToken && token
    ? requisicao.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : requisicao;

  return next(requisicaoAutenticada).pipe(
    catchError((erro: unknown) => {
      if (erro instanceof HttpErrorResponse && erro.status === 401 && deveAnexarToken) {
        autenticacaoService.limparSessao();
        void router.navigate(['/admin/login'], {
          queryParams: { returnUrl: router.url }
        });
      }

      return throwError(() => erro);
    })
  );
};
