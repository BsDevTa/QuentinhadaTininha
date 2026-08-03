import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { FreteBairroAdmin, FreteBairroAdminSalvar } from '../../modelos/admin-cardapio.model';
import { FretesBairrosAdministrativoService } from '../../servicos/fretes-bairros-administrativo.service';

@Component({
  selector: 'app-fretes-bairros-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho admin-crud__topo">
        <div>
          <span class="admin-tag">Entrega</span>
          <h1>Fretes por bairro</h1>
          <p>Configure os bairros atendidos e o valor cobrado em cada entrega.</p>
        </div>
        <button class="botao" type="button" (click)="abrirNovo()">Novo bairro</button>
      </header>

      <form class="admin-bloco admin-filtros" [formGroup]="filtrosForm" (ngSubmit)="carregar()">
        <label class="admin-campo">
          Bairro
          <input type="search" formControlName="bairro" placeholder="Buscar bairro">
        </label>
        <label class="admin-campo">
          Status
          <select formControlName="ativo">
            <option value="">Todos</option>
            <option value="true">Ativo</option>
            <option value="false">Inativo</option>
          </select>
        </label>
        <div class="admin-filtros__acoes">
          <button class="botao" type="submit">Filtrar</button>
          <button class="botao secundario" type="button" (click)="limparFiltros()">Limpar</button>
        </div>
      </form>

      <p class="admin-feedback" *ngIf="mensagem()" aria-live="polite">{{ mensagem() }}</p>
      <p class="admin-feedback admin-feedback--erro" *ngIf="erro()" aria-live="assertive">{{ erro() }}</p>

      <article class="admin-bloco" *ngIf="carregando()">Carregando fretes...</article>
      <article class="admin-bloco admin-vazio" *ngIf="!carregando() && fretes().length === 0">
        Nenhum frete por bairro encontrado.
      </article>

      <div class="admin-lista" *ngIf="!carregando() && fretes().length > 0">
        <article class="admin-item admin-item--sem-imagem" *ngFor="let item of fretes(); trackBy: rastrearFrete">
          <div>
            <strong>{{ item.bairro }}</strong>
            <span>{{ item.valor | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</span>
            <small>Atualizado em {{ item.atualizadoEm | date:'short' }}</small>
          </div>
          <div class="admin-badges">
            <span [class.admin-badge--off]="!item.ativo">{{ item.ativo ? 'Ativo' : 'Inativo' }}</span>
          </div>
          <div class="admin-acoes">
            <button type="button" (click)="editar(item.id)">Editar</button>
            <button type="button" (click)="alterarStatus(item)">{{ item.ativo ? 'Desativar' : 'Ativar' }}</button>
            <button type="button" (click)="excluir(item)">Excluir</button>
          </div>
        </article>
      </div>

      <dialog class="admin-modal" [open]="modalAberto()" aria-labelledby="titulo-frete-bairro">
        <form [formGroup]="form" (ngSubmit)="salvar()">
          <header>
            <h2 id="titulo-frete-bairro">{{ editandoId() ? 'Editar frete' : 'Novo frete' }}</h2>
            <button type="button" aria-label="Fechar formulario" (click)="fecharModal()">×</button>
          </header>

          <section class="admin-form-grid">
            <label class="admin-campo">
              Bairro
              <input formControlName="bairro" placeholder="Ex.: Pituba">
              <small *ngIf="campoInvalido('bairro')">Informe um bairro válido.</small>
            </label>
            <label class="admin-campo">
              Valor do frete
              <input type="number" min="0" step="0.01" formControlName="valor" placeholder="8,00">
              <small *ngIf="campoInvalido('valor')">Informe um valor maior ou igual a zero.</small>
            </label>
          </section>

          <section class="admin-switches">
            <label><input type="checkbox" formControlName="ativo"> Bairro ativo para entrega</label>
          </section>

          <footer>
            <button class="botao secundario" type="button" (click)="fecharModal()">Cancelar</button>
            <button class="botao" type="submit" [disabled]="salvando()">{{ salvando() ? 'Salvando...' : 'Salvar frete' }}</button>
          </footer>
        </form>
      </dialog>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FretesBairrosPage {
  private readonly service = inject(FretesBairrosAdministrativoService);
  private readonly fb = inject(FormBuilder);

  readonly fretes = signal<FreteBairroAdmin[]>([]);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly modalAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly filtrosForm = this.fb.nonNullable.group({
    bairro: [''],
    ativo: ['']
  });

  readonly form = this.fb.nonNullable.group({
    bairro: ['', [Validators.required, Validators.maxLength(120), this.bairroValido]],
    valor: [0, [Validators.required, Validators.min(0)]],
    ativo: [true]
  });

  constructor() {
    this.carregar();
  }

  carregar(): void {
    this.erro.set('');
    this.carregando.set(true);
    const filtros = this.filtrosForm.getRawValue();
    this.service.listar({
      bairro: filtros.bairro,
      ativo: this.valorBooleano(filtros.ativo)
    }).pipe(finalize(() => this.carregando.set(false))).subscribe({
      next: (fretes) => this.fretes.set(fretes),
      error: () => this.erro.set('Nao foi possivel carregar os fretes por bairro.')
    });
  }

  limparFiltros(): void {
    this.filtrosForm.reset();
    this.carregar();
  }

  abrirNovo(): void {
    this.editandoId.set(null);
    this.form.reset({
      bairro: '',
      valor: 0,
      ativo: true
    });
    this.modalAberto.set(true);
  }

  editar(id: string): void {
    this.editandoId.set(id);
    this.modalAberto.set(true);
    this.service.obterPorId(id).subscribe({
      next: (item) => this.form.reset({
        bairro: item.bairro,
        valor: item.valor,
        ativo: item.ativo
      }),
      error: () => this.erro.set('Nao foi possivel carregar o frete selecionado.')
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.erro.set('');
    this.mensagem.set('');
    const id = this.editandoId();
    const operacao = id
      ? this.service.atualizar(id, this.montarRequest())
      : this.service.criar(this.montarRequest());

    this.salvando.set(true);
    operacao.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: () => {
        this.mensagem.set('Frete salvo com sucesso.');
        this.fecharModal();
        this.carregar();
      },
      error: (erro: { error?: { mensagem?: string } }) =>
        this.erro.set(erro.error?.mensagem ?? 'Nao foi possivel salvar o frete.')
    });
  }

  alterarStatus(item: FreteBairroAdmin): void {
    if (item.ativo && !window.confirm(`Desativar entregas para ${item.bairro}?`)) {
      return;
    }

    this.service.alterarStatus(item.id, !item.ativo).subscribe({
      next: () => {
        this.mensagem.set('Status do frete atualizado.');
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel atualizar o status.')
    });
  }

  excluir(item: FreteBairroAdmin): void {
    if (!window.confirm(`Excluir o frete de ${item.bairro}?`)) {
      return;
    }

    this.service.excluir(item.id).subscribe({
      next: () => {
        this.mensagem.set('Frete excluido com sucesso.');
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel excluir o frete.')
    });
  }

  fecharModal(): void {
    this.modalAberto.set(false);
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  rastrearFrete(_indice: number, item: FreteBairroAdmin): string {
    return item.id;
  }

  private montarRequest(): FreteBairroAdminSalvar {
    const valor = this.form.getRawValue();
    return {
      bairro: this.limparEspacos(valor.bairro),
      valor: Number(valor.valor),
      ativo: valor.ativo
    };
  }

  private valorBooleano(valor: string): boolean | '' {
    if (valor === 'true') {
      return true;
    }
    if (valor === 'false') {
      return false;
    }
    return '';
  }

  private limparEspacos(valor: string): string {
    return valor.trim().replace(/\s+/g, ' ');
  }

  private bairroValido(controle: AbstractControl<string>): ValidationErrors | null {
    const valor = String(controle.value ?? '').trim();
    if (!valor) {
      return null;
    }

    return /^\d+$/.test(valor.replace(/\s+/g, '')) ? { somenteNumeros: true } : null;
  }
}
