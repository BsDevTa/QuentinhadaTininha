import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, timeout } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AutenticacaoService } from '../autenticacao/autenticacao.service';

let redirecionamento401EmAndamento = false;

export const jwtInterceptor: HttpInterceptorFn = (requisicao, next) => {
  const autenticacaoService = inject(AutenticacaoService);
  const router = inject(Router);
  const apiUrl = environment.apiUrl.replace(/\/$/, '');
  const ehApi = requisicao.url.startsWith(apiUrl);
  const deveAnexarToken = ehApi &&
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

  const requisicaoApi = next(requisicaoAutenticada);
  const requisicaoComTimeout = ehApi
    ? requisicaoApi.pipe(timeout({ each: 10000 }))
    : requisicaoApi;

  return requisicaoComTimeout.pipe(
    catchError((erro: unknown) => {
      if (
        erro instanceof HttpErrorResponse &&
        erro.status === 401 &&
        deveAnexarToken
      ) {
        autenticacaoService.limparSessao();

        if (!redirecionamento401EmAndamento && !router.url.startsWith('/admin/login')) {
          redirecionamento401EmAndamento = true;
          void router.navigate(['/admin/login'], {
            queryParams: { returnUrl: router.url }
          }).finally(() => {
            redirecionamento401EmAndamento = false;
          });
        }
      }

      return throwError(() => erro);
    })
  );
};
