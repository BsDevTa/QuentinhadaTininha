import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { FuncionamentoAdmin } from '../../modelos/admin-cardapio.model';
import { FuncionamentoAdministrativoService } from '../../servicos/funcionamento-administrativo.service';

@Component({
  selector: 'app-funcionamento-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho">
        <span class="admin-tag">Atendimento</span>
        <h1>Funcionamento</h1>
        <p>Abra ou feche o restaurante e ajuste a mensagem exibida ao cliente.</p>
      </header>

      <p class="admin-feedback" *ngIf="mensagem()" aria-live="polite">{{ mensagem() }}</p>
      <p class="admin-feedback admin-feedback--erro" *ngIf="erro()" aria-live="assertive">{{ erro() }}</p>

      <article class="admin-bloco admin-funcionamento" *ngIf="status() as atual">
        <div [class.admin-funcionamento__status--fechado]="!atual.estaAberto" class="admin-funcionamento__status">
          <span>{{ atual.estaAberto ? 'Aberto' : 'Fechado' }}</span>
          <strong>{{ atual.estaAberto ? 'Restaurante aberto' : 'Restaurante fechado' }}</strong>
          <small>Ultima alteracao: {{ atual.dataUltimaAlteracao | date:'short' }}</small>
        </div>
        <div class="admin-funcionamento__acoes">
          <button class="botao" type="button" (click)="definirAberto(true)" [disabled]="salvando()">Abrir restaurante</button>
          <button class="botao secundario" type="button" (click)="definirAberto(false)" [disabled]="salvando()">Fechar restaurante</button>
        </div>
      </article>

      <form class="admin-bloco" [formGroup]="form" (ngSubmit)="salvar()">
        <section class="admin-form-grid">
          <label class="admin-campo admin-campo--largo">
            Mensagem publica
            <textarea rows="3" formControlName="mensagemStatus"></textarea>
            <small *ngIf="campoInvalido('mensagemStatus')">Limite de 180 caracteres.</small>
          </label>
          <label class="admin-campo admin-campo--largo">
            Horario de funcionamento
            <input formControlName="horarioFuncionamento">
            <small *ngIf="campoInvalido('horarioFuncionamento')">Limite de 160 caracteres.</small>
          </label>
        </section>

        <article class="admin-preview">
          <span>Como aparecera para o cliente</span>
          <strong>{{ form.controls.estaAberto.value ? 'Estamos abertos' : 'Estamos fechados' }}</strong>
          <p>{{ form.controls.horarioFuncionamento.value }}</p>
          <p>{{ form.controls.mensagemStatus.value }}</p>
        </article>

        <footer class="admin-form-footer">
          <button class="botao" type="submit" [disabled]="salvando() || form.invalid">{{ salvando() ? 'Salvando...' : 'Salvar funcionamento' }}</button>
        </footer>
      </form>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FuncionamentoPage {
  private readonly service = inject(FuncionamentoAdministrativoService);
  private readonly fb = inject(FormBuilder);

  readonly status = signal<FuncionamentoAdmin | null>(null);
  readonly salvando = signal(false);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly form = this.fb.nonNullable.group({
    estaAberto: [true],
    mensagemStatus: ['', [Validators.required, Validators.maxLength(180)]],
    horarioFuncionamento: ['', [Validators.required, Validators.maxLength(160)]]
  });

  constructor() {
    this.carregar();
  }

  carregar(): void {
    this.service.obter().subscribe({
      next: (status) => {
        this.status.set(status);
        this.form.patchValue(status);
      },
      error: () => this.erro.set('Nao foi possivel carregar o funcionamento.')
    });
  }

  definirAberto(estaAberto: boolean): void {
    if (!estaAberto && !window.confirm('Fechar o restaurante agora?')) {
      return;
    }
    this.form.controls.estaAberto.setValue(estaAberto);
    this.salvar();
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.salvando.set(true);
    this.service.atualizar(this.form.getRawValue()).pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (status) => {
        this.status.set(status);
        this.mensagem.set('Funcionamento atualizado.');
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }
}
