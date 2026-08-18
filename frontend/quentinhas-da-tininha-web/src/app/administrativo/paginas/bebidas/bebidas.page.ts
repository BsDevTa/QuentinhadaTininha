import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { BebidaAdmin, BebidasAdministrativoService } from '../../servicos/bebidas-administrativo.service';

@Component({
  selector: 'app-bebidas-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho">
        <span class="admin-tag">Catalogo</span>
        <h1>Bebidas</h1>
        <p>Cadastre bebidas opcionais para aparecerem no cardapio publico.</p>
      </header>

      <p class="admin-feedback" *ngIf="mensagem()" aria-live="polite">{{ mensagem() }}</p>
      <p class="admin-feedback admin-feedback--erro" *ngIf="erro()" aria-live="assertive">{{ erro() }}</p>

      <form class="admin-bloco" [formGroup]="form" (ngSubmit)="salvar()">
        <section class="admin-form-grid">
          <label class="admin-campo">
            Nome
            <input formControlName="nome">
          </label>
          <label class="admin-campo">
            Preco
            <input type="number" step="0.01" min="0" formControlName="preco">
          </label>
          <label class="admin-campo admin-campo--largo">
            Imagem
            <input formControlName="imagemUrl" placeholder="/assets/bebidas/pepsi.png">
          </label>
          <label class="admin-campo admin-campo--largo">
            Descricao
            <textarea rows="2" formControlName="descricao"></textarea>
          </label>
          <label class="admin-check">
            <input type="checkbox" formControlName="ativa">
            Bebida ativa
          </label>
        </section>

        <footer class="admin-form-footer">
          <button class="botao" type="submit" [disabled]="salvando() || form.invalid">
            {{ salvando() ? 'Salvando...' : bebidaEmEdicao() ? 'Salvar bebida' : 'Cadastrar bebida' }}
          </button>
          <button class="botao secundario" type="button" (click)="limparFormulario()" [disabled]="salvando()">Limpar</button>
        </footer>
      </form>

      <article class="admin-bloco" *ngIf="carregando()">Carregando bebidas...</article>
      <article class="admin-bloco admin-vazio" *ngIf="!carregando() && bebidas().length === 0">
        Nenhuma bebida cadastrada.
      </article>

      <div class="admin-lista" *ngIf="!carregando() && bebidas().length > 0">
        <article class="admin-card-item" *ngFor="let bebida of bebidas(); trackBy: rastrearBebida">
          <div>
            <span>{{ bebida.ativa ? 'Ativa' : 'Inativa' }}</span>
            <strong>{{ bebida.nome }}</strong>
            <small>{{ bebida.preco | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}</small>
          </div>
          <div class="admin-dia__acoes">
            <button type="button" (click)="editar(bebida)">Editar</button>
            <button type="button" (click)="alternar(bebida)">{{ bebida.ativa ? 'Desativar' : 'Ativar' }}</button>
            <button type="button" (click)="excluir(bebida)">Excluir</button>
          </div>
        </article>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BebidasPage {
  private readonly service = inject(BebidasAdministrativoService);
  private readonly fb = inject(FormBuilder);

  readonly bebidas = signal<BebidaAdmin[]>([]);
  readonly bebidaEmEdicao = signal<BebidaAdmin | null>(null);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly form = this.fb.nonNullable.group({
    nome: ['', [Validators.required, Validators.maxLength(120)]],
    descricao: [''],
    preco: [0, [Validators.required, Validators.min(0.01)]],
    ativa: [true],
    imagemUrl: ['']
  });

  constructor() {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.service.listar()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({
        next: (bebidas) => this.bebidas.set(bebidas),
        error: () => this.erro.set('Nao foi possivel carregar as bebidas.')
      });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const valor = this.form.getRawValue();
    const requisicao = {
      nome: valor.nome.trim(),
      descricao: this.normalizar(valor.descricao),
      preco: Number(valor.preco),
      ativa: valor.ativa,
      imagemUrl: this.normalizar(valor.imagemUrl)
    };
    const edicao = this.bebidaEmEdicao();
    const operacao = edicao
      ? this.service.atualizar(edicao.id, requisicao)
      : this.service.criar(requisicao);

    this.salvando.set(true);
    this.erro.set('');
    this.mensagem.set('');
    operacao.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: () => {
        this.mensagem.set('Bebida salva.');
        this.limparFormulario();
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel salvar a bebida.')
    });
  }

  editar(bebida: BebidaAdmin): void {
    this.bebidaEmEdicao.set(bebida);
    this.form.patchValue({
      nome: bebida.nome,
      descricao: bebida.descricao ?? '',
      preco: bebida.preco,
      ativa: bebida.ativa,
      imagemUrl: bebida.imagemUrl ?? ''
    });
  }

  alternar(bebida: BebidaAdmin): void {
    this.service.atualizar(bebida.id, {
      nome: bebida.nome,
      descricao: bebida.descricao,
      preco: bebida.preco,
      ativa: !bebida.ativa,
      imagemUrl: bebida.imagemUrl
    }).subscribe({
      next: () => this.carregar(),
      error: () => this.erro.set('Nao foi possivel alterar a bebida.')
    });
  }

  excluir(bebida: BebidaAdmin): void {
    if (!window.confirm(`Excluir ${bebida.nome}?`)) {
      return;
    }

    this.service.excluir(bebida.id).subscribe({
      next: () => this.carregar(),
      error: () => this.erro.set('Nao foi possivel excluir a bebida.')
    });
  }

  limparFormulario(): void {
    this.bebidaEmEdicao.set(null);
    this.form.reset({
      nome: '',
      descricao: '',
      preco: 0,
      ativa: true,
      imagemUrl: ''
    });
  }

  rastrearBebida(_indice: number, bebida: BebidaAdmin): string {
    return bebida.id;
  }

  private normalizar(valor: string | null): string | null {
    const texto = valor?.trim();
    return texto ? texto : null;
  }
}
