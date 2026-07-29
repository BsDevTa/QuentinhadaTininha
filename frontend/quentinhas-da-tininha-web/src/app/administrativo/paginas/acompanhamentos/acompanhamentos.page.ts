import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  AcompanhamentoAdmin,
  AcompanhamentoAdminSalvar,
  GrupoAcompanhamentoAdmin,
  GrupoAcompanhamentoVinculoAdmin,
  TipoSelecaoAcompanhamentoAdmin
} from '../../modelos/admin-cardapio.model';
import { AcompanhamentosAdministrativoService } from '../../servicos/acompanhamentos-administrativo.service';
import { PratosAdministrativoService } from '../../servicos/pratos-administrativo.service';

@Component({
  selector: 'app-acompanhamentos-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho admin-crud__topo">
        <div>
          <span class="admin-tag">Complementos</span>
          <h1>Acompanhamentos</h1>
          <p>Controle acompanhamentos, grupos, disponibilidade e tipo de selecao.</p>
        </div>
        <button class="botao" type="button" (click)="abrirNovo()">Novo acompanhamento</button>
      </header>

      <form class="admin-bloco admin-filtros" [formGroup]="filtrosForm" (ngSubmit)="carregar()">
        <label class="admin-campo">Nome <input type="search" formControlName="nome"></label>
        <label class="admin-campo">
          Grupo
          <select formControlName="grupoAcompanhamentoId">
            <option value="">Todos</option>
            <option *ngFor="let grupo of grupos()" [value]="grupo.id">{{ grupo.nome }}</option>
          </select>
        </label>
        <label class="admin-campo">
          Disponibilidade
          <select formControlName="estaDisponivel">
            <option value="">Todos</option>
            <option value="true">Disponivel</option>
            <option value="false">Indisponivel</option>
          </select>
        </label>
        <label class="admin-campo">
          Cadastro
          <select formControlName="estaAtivo">
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

      <article class="admin-bloco" *ngIf="carregando()">Carregando acompanhamentos...</article>
      <article class="admin-bloco admin-vazio" *ngIf="!carregando() && acompanhamentos().length === 0">Nenhum acompanhamento encontrado.</article>

      <div class="admin-lista" *ngIf="!carregando() && acompanhamentos().length > 0">
        <article class="admin-item admin-item--sem-imagem" *ngFor="let item of acompanhamentos()">
          <div>
            <strong>{{ item.nome }}</strong>
            <span>{{ textoTipo(item.tipoSelecao) }}</span>
            <small>Grupos: {{ nomesGrupos(item.grupos) }}</small>
          </div>
          <div class="admin-badges">
            <span [class.admin-badge--off]="!item.estaDisponivel">{{ item.estaDisponivel ? 'Disponivel' : 'Indisponivel' }}</span>
            <span [class.admin-badge--off]="!item.estaAtivo">{{ item.estaAtivo ? 'Ativo' : 'Inativo' }}</span>
          </div>
          <div class="admin-acoes">
            <button type="button" (click)="editar(item.id)">Editar</button>
            <button type="button" (click)="alterarDisponibilidade(item)">{{ item.estaDisponivel ? 'Marcar indisponivel' : 'Marcar disponivel' }}</button>
            <button type="button" (click)="alterarAtivacao(item)">{{ item.estaAtivo ? 'Desativar' : 'Ativar' }}</button>
          </div>
        </article>
      </div>

      <dialog class="admin-modal" [open]="modalAberto()" aria-labelledby="titulo-acompanhamento">
        <form [formGroup]="form" (ngSubmit)="salvar()">
          <header>
            <h2 id="titulo-acompanhamento">{{ editandoId() ? 'Editar acompanhamento' : 'Novo acompanhamento' }}</h2>
            <button type="button" aria-label="Fechar formulario" (click)="fecharModal()">×</button>
          </header>

          <section class="admin-form-grid">
            <label class="admin-campo">
              Nome
              <input formControlName="nome">
              <small *ngIf="campoInvalido('nome')">Informe entre 2 e 120 caracteres.</small>
            </label>
            <label class="admin-campo">
              Tipo de selecao
              <select formControlName="tipoSelecao">
                <option value="MULTIPLA">Selecao multipla</option>
                <option value="EXCLUSIVA">Selecao exclusiva</option>
              </select>
            </label>
            <label class="admin-campo" *ngIf="form.controls['tipoSelecao'].value === 'EXCLUSIVA'">
              Grupo exclusivo
              <input formControlName="grupoExclusivo" placeholder="Ex: Feijao">
            </label>
          </section>

          <p class="admin-ajuda">{{ form.controls['tipoSelecao'].value === 'EXCLUSIVA' ? 'O cliente deve escolher apenas uma opcao deste grupo.' : 'O cliente pode escolher este item junto com outros.' }}</p>

          <h3>Grupos vinculados</h3>
          <section class="admin-dias">
            <label *ngFor="let grupo of grupos(); let indice = index">
              <input type="checkbox" [formControlName]="'grupo' + indice">
              {{ grupo.nome }}
              <input type="number" min="0" [formControlName]="'ordem' + indice" aria-label="Ordem no grupo">
              <span><input type="checkbox" [formControlName]="'obrigatorio' + indice"> obrigatorio</span>
            </label>
          </section>

          <section class="admin-switches">
            <label><input type="checkbox" formControlName="estaDisponivel"> Disponivel para venda</label>
            <label><input type="checkbox" formControlName="estaAtivo"> Cadastro ativo</label>
          </section>

          <footer>
            <button class="botao secundario" type="button" (click)="fecharModal()">Cancelar</button>
            <button class="botao" type="submit" [disabled]="salvando()">{{ salvando() ? 'Salvando...' : 'Salvar acompanhamento' }}</button>
          </footer>
        </form>
      </dialog>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AcompanhamentosPage {
  private readonly service = inject(AcompanhamentosAdministrativoService);
  private readonly pratosService = inject(PratosAdministrativoService);
  private readonly fb = inject(UntypedFormBuilder);

  readonly acompanhamentos = signal<AcompanhamentoAdmin[]>([]);
  readonly grupos = signal<GrupoAcompanhamentoAdmin[]>([]);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly modalAberto = signal(false);
  readonly editandoId = signal<string | null>(null);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly filtrosForm = this.fb.group({
    nome: [''],
    grupoAcompanhamentoId: [''],
    estaDisponivel: [''],
    estaAtivo: ['']
  });

  readonly form = this.fb.group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    tipoSelecao: ['MULTIPLA' as TipoSelecaoAcompanhamentoAdmin],
    grupoExclusivo: [''],
    estaDisponivel: [true],
    estaAtivo: [true],
    grupo0: [false],
    grupo1: [false],
    grupo2: [false],
    grupo3: [false],
    grupo4: [false],
    grupo5: [false],
    ordem0: [0],
    ordem1: [0],
    ordem2: [0],
    ordem3: [0],
    ordem4: [0],
    ordem5: [0],
    obrigatorio0: [false],
    obrigatorio1: [false],
    obrigatorio2: [false],
    obrigatorio3: [false],
    obrigatorio4: [false],
    obrigatorio5: [false]
  });

  constructor() {
    this.carregarGrupos();
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    const filtros = this.filtrosForm.getRawValue();
    this.service.listar({
      nome: filtros.nome,
      grupoAcompanhamentoId: filtros.grupoAcompanhamentoId,
      estaDisponivel: this.valorBooleano(filtros.estaDisponivel),
      estaAtivo: this.valorBooleano(filtros.estaAtivo)
    }).pipe(finalize(() => this.carregando.set(false))).subscribe({
      next: (itens) => this.acompanhamentos.set(itens),
      error: () => this.erro.set('Nao foi possivel carregar os acompanhamentos.')
    });
  }

  limparFiltros(): void {
    this.filtrosForm.reset();
    this.carregar();
  }

  abrirNovo(): void {
    this.editandoId.set(null);
    this.form.reset({
      nome: '',
      tipoSelecao: 'MULTIPLA',
      grupoExclusivo: '',
      estaDisponivel: true,
      estaAtivo: true
    });
    this.limparGruposFormulario();
    this.modalAberto.set(true);
  }

  editar(id: string): void {
    this.editandoId.set(id);
    this.modalAberto.set(true);
    this.service.obterPorId(id).subscribe({
      next: (item) => this.preencherFormulario(item),
      error: () => this.erro.set('Nao foi possivel carregar o acompanhamento.')
    });
  }

  salvar(): void {
    if (this.form.invalid || this.gruposSelecionados().length === 0) {
      this.form.markAllAsTouched();
      this.erro.set('Verifique os campos e selecione ao menos um grupo.');
      return;
    }

    const id = this.editandoId();
    const operacao = id ? this.service.atualizar(id, this.montarRequest()) : this.service.criar(this.montarRequest());
    this.salvando.set(true);
    operacao.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: () => {
        this.mensagem.set('Acompanhamento salvo com sucesso.');
        this.fecharModal();
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  alterarDisponibilidade(item: AcompanhamentoAdmin): void {
    this.service.alterarDisponibilidade(item.id, !item.estaDisponivel).subscribe({
      next: () => {
        this.mensagem.set('Disponibilidade atualizada.');
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  alterarAtivacao(item: AcompanhamentoAdmin): void {
    if (item.estaAtivo && !window.confirm(`Desativar o acompanhamento ${item.nome}?`)) {
      return;
    }
    this.service.alterarAtivacao(item.id, !item.estaAtivo).subscribe({
      next: () => {
        this.mensagem.set('Cadastro atualizado.');
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  fecharModal(): void {
    this.modalAberto.set(false);
  }

  nomesGrupos(grupos: GrupoAcompanhamentoVinculoAdmin[]): string {
    return grupos.map((grupo) => grupo.nome ?? grupo.codigo ?? 'Grupo').join(', ') || 'Sem grupo';
  }

  textoTipo(tipo: TipoSelecaoAcompanhamentoAdmin): string {
    return tipo === 'EXCLUSIVA' ? 'Selecao exclusiva' : 'Selecao multipla';
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  private carregarGrupos(): void {
    this.pratosService.listarGruposAcompanhamento().subscribe({
      next: (grupos) => this.grupos.set(grupos.slice(0, 6)),
      error: () => this.erro.set('Nao foi possivel carregar os grupos.')
    });
  }

  private preencherFormulario(item: AcompanhamentoAdmin): void {
    this.form.patchValue({
      nome: item.nome,
      tipoSelecao: item.tipoSelecao,
      grupoExclusivo: item.grupoExclusivo ?? '',
      estaDisponivel: item.estaDisponivel,
      estaAtivo: item.estaAtivo
    });
    this.limparGruposFormulario();
    this.grupos().forEach((grupo, indice) => {
      const vinculo = item.grupos.find((valor) => valor.grupoAcompanhamentoId === grupo.id);
      if (vinculo) {
        this.form.get(`grupo${indice}`)?.setValue(true);
        this.form.get(`ordem${indice}`)?.setValue(vinculo.ordemExibicao);
        this.form.get(`obrigatorio${indice}`)?.setValue(vinculo.obrigatorio);
      }
    });
  }

  private montarRequest(): AcompanhamentoAdminSalvar {
    const valor = this.form.getRawValue();
    return {
      nome: valor.nome.trim(),
      tipoSelecao: valor.tipoSelecao,
      grupoExclusivo: valor.grupoExclusivo.trim() || null,
      estaDisponivel: valor.estaDisponivel,
      estaAtivo: valor.estaAtivo,
      grupos: this.gruposSelecionados()
    };
  }

  private gruposSelecionados(): GrupoAcompanhamentoVinculoAdmin[] {
    return this.grupos()
      .map((grupo, indice) => ({ grupo, indice }))
      .filter((item) => this.form.get(`grupo${item.indice}`)?.value === true)
      .map((item) => ({
        grupoAcompanhamentoId: item.grupo.id,
        obrigatorio: this.form.get(`obrigatorio${item.indice}`)?.value === true,
        ordemExibicao: Number(this.form.get(`ordem${item.indice}`)?.value ?? 0)
      }));
  }

  private limparGruposFormulario(): void {
    for (let indice = 0; indice < 6; indice++) {
      this.form.get(`grupo${indice}`)?.setValue(false);
      this.form.get(`ordem${indice}`)?.setValue(0);
      this.form.get(`obrigatorio${indice}`)?.setValue(false);
    }
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
}
