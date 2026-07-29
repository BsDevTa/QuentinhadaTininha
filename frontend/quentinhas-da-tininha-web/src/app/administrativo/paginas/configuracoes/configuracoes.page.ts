import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ConfiguracoesPublicasAdmin } from '../../modelos/admin-cardapio.model';
import { ConfiguracoesAdministrativoService } from '../../servicos/configuracoes-administrativo.service';

@Component({
  selector: 'app-configuracoes-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho">
        <span class="admin-tag">Restaurante</span>
        <h1>Configuracoes</h1>
        <p>Atualize os dados publicos exibidos na pagina e usados no WhatsApp.</p>
      </header>

      <p class="admin-feedback" *ngIf="mensagem()" aria-live="polite">{{ mensagem() }}</p>
      <p class="admin-feedback admin-feedback--erro" *ngIf="erro()" aria-live="assertive">{{ erro() }}</p>

      <form class="admin-bloco" [formGroup]="form" (ngSubmit)="salvar()">
        <section class="admin-form-grid">
          <label class="admin-campo">
            Nome do restaurante
            <input formControlName="nomeRestaurante">
          </label>
          <label class="admin-campo">
            WhatsApp
            <input formControlName="whatsapp" placeholder="(71) 99999-9999">
            <small *ngIf="campoInvalido('whatsapp')">Informe DDD e numero validos.</small>
          </label>
          <label class="admin-campo">
            Instagram
            <input formControlName="instagram" placeholder="@quentinhasdatininha">
          </label>
          <label class="admin-campo">
            Endereco
            <input formControlName="endereco">
          </label>
          <label class="admin-campo admin-campo--largo">
            URL da logo
            <input formControlName="urlLogo" placeholder="Opcional">
          </label>
          <label class="admin-campo admin-campo--largo">
            Texto do rodape
            <input formControlName="textoRodape" placeholder="Opcional">
          </label>
        </section>

        <article class="admin-preview">
          <span>Previa publica</span>
          <strong>{{ form.controls.nomeRestaurante.value }}</strong>
          <p>WhatsApp: {{ whatsappNormalizado() }}</p>
          <p>Instagram: {{ form.controls.instagram.value || '@quentinhasdatininha' }}</p>
          <p>{{ form.controls.endereco.value || 'Endereco nao informado' }}</p>
        </article>

        <footer class="admin-form-footer">
          <button class="botao" type="submit" [disabled]="salvando() || form.invalid">{{ salvando() ? 'Salvando...' : 'Salvar configuracoes' }}</button>
        </footer>
      </form>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfiguracoesPage {
  private readonly service = inject(ConfiguracoesAdministrativoService);
  private readonly fb = inject(FormBuilder);

  readonly configuracoes = signal<ConfiguracoesPublicasAdmin | null>(null);
  readonly salvando = signal(false);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly form = this.fb.nonNullable.group({
    nomeRestaurante: ['', [Validators.required, Validators.maxLength(120)]],
    whatsapp: ['', [Validators.required, Validators.minLength(10)]],
    instagram: [''],
    endereco: [''],
    urlLogo: [''],
    textoRodape: ['']
  });

  constructor() {
    this.carregar();
  }

  carregar(): void {
    this.service.obter().subscribe({
      next: (configuracoes) => {
        this.configuracoes.set(configuracoes);
        this.form.patchValue({
          nomeRestaurante: configuracoes.nomeRestaurante,
          whatsapp: configuracoes.whatsapp,
          instagram: configuracoes.instagram ?? '',
          endereco: configuracoes.endereco ?? '',
          urlLogo: configuracoes.urlLogo ?? '',
          textoRodape: configuracoes.textoRodape ?? ''
        });
      },
      error: () => this.erro.set('Nao foi possivel carregar as configuracoes.')
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const valor = this.form.getRawValue();
    this.salvando.set(true);
    this.service.atualizar({
      nomeRestaurante: valor.nomeRestaurante.trim(),
      whatsapp: this.whatsappNormalizado(),
      instagram: this.opcional(valor.instagram),
      endereco: this.opcional(valor.endereco),
      urlLogo: this.opcional(valor.urlLogo),
      textoRodape: this.opcional(valor.textoRodape)
    }).pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (configuracoes) => {
        this.configuracoes.set(configuracoes);
        this.mensagem.set('Configuracoes salvas.');
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  whatsappNormalizado(): string {
    const digitos = this.form.controls.whatsapp.value.replace(/\D/g, '');
    return digitos.length === 11 ? `55${digitos}` : digitos;
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  private opcional(valor: string): string | null {
    const texto = valor.trim();
    return texto ? texto : null;
  }
}
