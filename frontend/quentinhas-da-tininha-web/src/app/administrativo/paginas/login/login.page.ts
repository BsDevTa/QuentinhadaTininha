import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TimeoutError, finalize } from 'rxjs';
import { LogoMarcaComponent } from '../../../compartilhado/componentes/logo-marca/logo-marca.component';
import { AutenticacaoService } from '../../../nucleo/autenticacao/autenticacao.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LogoMarcaComponent],
  template: `
    <main class="admin-login">
      <form class="admin-login__card" [formGroup]="formulario" (ngSubmit)="entrar()" novalidate>
        <app-logo-marca />
        <div>
          <h1>Acesso administrativo</h1>
          <p>Entre para cuidar do cardapio, disponibilidade e funcionamento da Tininha.</p>
        </div>

        <label class="admin-campo">
          <span>E-mail</span>
          <input type="email" formControlName="email" autocomplete="email" />
          @if (campoInvalido('email')) {
            <small>{{ formulario.controls.email.hasError('required') ? 'Informe seu e-mail.' : 'Digite um e-mail valido.' }}</small>
          }
        </label>

        <label class="admin-campo admin-campo--senha">
          <span>Senha</span>
          <input [type]="senhaVisivel() ? 'text' : 'password'" formControlName="senha" autocomplete="current-password" />
          <button type="button" [attr.aria-label]="senhaVisivel() ? 'Ocultar senha' : 'Mostrar senha'" (click)="senhaVisivel.update((valor) => !valor)">
            {{ senhaVisivel() ? 'Ocultar' : 'Mostrar' }}
          </button>
          @if (campoInvalido('senha')) {
            <small>Informe sua senha.</small>
          }
        </label>

        @if (mensagemErro()) {
          <p class="admin-login__erro" aria-live="polite">{{ mensagemErro() }}</p>
        }

        <button class="botao" type="submit" [disabled]="formulario.invalid || carregando()">
          {{ carregando() ? 'Entrando...' : 'Entrar' }}
        </button>
        <a class="admin-login__voltar" routerLink="/">Voltar para o cardapio</a>
      </form>
    </main>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly autenticacaoService = inject(AutenticacaoService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly carregando = signal(false);
  protected readonly mensagemErro = signal('');
  protected readonly senhaVisivel = signal(false);
  protected readonly formulario = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required]]
  });

  protected entrar(): void {
    this.formulario.markAllAsTouched();

    if (this.formulario.invalid || this.carregando()) {
      return;
    }

    this.carregando.set(true);
    this.mensagemErro.set('');

    this.autenticacaoService.entrar(this.formulario.getRawValue())
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/admin/painel';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (erro: unknown) => {
        this.mensagemErro.set(this.criarMensagemErroLogin(erro));
      }
    });
  }

  protected campoInvalido(campo: 'email' | 'senha'): boolean {
    const controle = this.formulario.controls[campo];
    return controle.invalid && (controle.dirty || controle.touched);
  }

  private criarMensagemErroLogin(erro: unknown): string {
    if (erro instanceof TimeoutError) {
      return 'A entrada demorou mais que o esperado. Tente novamente.';
    }

    if (erro instanceof HttpErrorResponse) {
      if (erro.status === 401) {
        return 'E-mail ou senha invalidos.';
      }

      if (erro.status === 0) {
        return 'Nao foi possivel conectar com a API agora.';
      }

      if (erro.status >= 500) {
        return 'A API encontrou um erro. Tente novamente em instantes.';
      }
    }

    return 'Nao foi possivel entrar agora. Tente novamente.';
  }
}
