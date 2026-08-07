import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CredenciaisLogin,
  RespostaAutenticacao,
  SessaoUsuario,
  UsuarioAutenticado
} from './autenticacao.model';

@Injectable({ providedIn: 'root' })
export class AutenticacaoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');
  private readonly chaveToken = 'quentinhas_admin_token';
  private readonly chaveUsuario = 'quentinhas_admin_usuario';
  private readonly chaveExpiracao = 'quentinhas_admin_expiracao';
  private restauracaoSessao$?: Observable<boolean>;

  readonly usuarioAtual = signal<UsuarioAutenticado | null>(this.lerUsuarioArmazenado());
  readonly carregandoSessao = signal(false);
  readonly autenticado = computed(() => Boolean(this.usuarioAtual()) && this.tokenValido());

  entrar(credenciais: CredenciaisLogin): Observable<RespostaAutenticacao> {
    return this.httpClient
      .post<RespostaAutenticacao>(`${this.apiUrl}/autenticacao/entrar`, credenciais)
      .pipe(tap((resposta) => this.salvarSessao(resposta)));
  }

  sair(): void {
    this.limparSessao();
  }

  restaurarSessao(): Observable<boolean> {
    if (!this.tokenValido()) {
      this.limparSessao();
      return of(false);
    }

    if (this.usuarioAtual()) {
      return of(true);
    }

    if (this.restauracaoSessao$) {
      return this.restauracaoSessao$;
    }

    this.carregandoSessao.set(true);

    this.restauracaoSessao$ = this.httpClient.get<SessaoUsuario>(`${this.apiUrl}/autenticacao/sessao`).pipe(
      tap((sessao) => this.usuarioAtual.set(sessao.usuario)),
      map((sessao) => sessao.autenticado),
      catchError(() => {
        this.limparSessao();
        return of(false);
      }),
      finalize(() => {
        this.carregandoSessao.set(false);
        this.restauracaoSessao$ = undefined;
      }),
      shareReplay({ bufferSize: 1, refCount: false })
    );

    return this.restauracaoSessao$;
  }

  estaAutenticado(): boolean {
    return this.autenticado();
  }

  obterToken(): string | null {
    return localStorage.getItem(this.chaveToken);
  }

  obterUsuarioAtual(): UsuarioAutenticado | null {
    return this.usuarioAtual();
  }

  limparSessao(): void {
    localStorage.removeItem(this.chaveToken);
    localStorage.removeItem(this.chaveUsuario);
    localStorage.removeItem(this.chaveExpiracao);
    this.usuarioAtual.set(null);
    this.carregandoSessao.set(false);
  }

  tokenValido(): boolean {
    const token = this.obterToken();
    const expiracao = localStorage.getItem(this.chaveExpiracao);

    if (!token || !expiracao) {
      return false;
    }

    return new Date(expiracao).getTime() > Date.now();
  }

  private salvarSessao(resposta: RespostaAutenticacao): void {
    localStorage.setItem(this.chaveToken, resposta.token);
    localStorage.setItem(this.chaveUsuario, JSON.stringify(resposta.usuario));
    localStorage.setItem(this.chaveExpiracao, resposta.expiraEm);
    this.usuarioAtual.set(resposta.usuario);
  }

  private lerUsuarioArmazenado(): UsuarioAutenticado | null {
    const usuario = localStorage.getItem(this.chaveUsuario);
    if (!usuario || !this.tokenValido()) {
      return null;
    }

    try {
      return JSON.parse(usuario) as UsuarioAutenticado;
    } catch {
      return null;
    }
  }
}
