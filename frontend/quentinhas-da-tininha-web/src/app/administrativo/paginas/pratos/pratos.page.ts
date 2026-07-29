import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import {
  DiaPratoAdmin,
  GrupoAcompanhamentoAdmin,
  PratoAdminDetalhe,
  PratoAdminResumo,
  PratoAdminSalvar
} from '../../modelos/admin-cardapio.model';
import { PratosAdministrativoService } from '../../servicos/pratos-administrativo.service';

@Component({
  selector: 'app-pratos-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="admin-pagina admin-crud">
      <header class="admin-pagina__cabecalho admin-crud__topo">
        <div>
          <span class="admin-tag">Cardapio</span>
          <h1>Pratos</h1>
          <p>Cadastre, edite e organize pratos, precos, dias e disponibilidade.</p>
        </div>
        <button class="botao" type="button" (click)="abrirNovo()">Novo prato</button>
      </header>

      <form class="admin-bloco admin-filtros" [formGroup]="filtrosForm" (ngSubmit)="carregar()">
        <label class="admin-campo">
          Nome
          <input type="search" formControlName="nome" placeholder="Buscar prato">
        </label>
        <label class="admin-campo">
          Dia
          <select formControlName="diaSemana">
            <option value="">Todos</option>
            <option *ngFor="let dia of dias" [value]="dia.valor">{{ dia.nome }}</option>
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
          <button class="botao" type="submit" [disabled]="carregando()">Filtrar</button>
          <button class="botao secundario" type="button" (click)="limparFiltros()">Limpar</button>
        </div>
      </form>

      <p class="admin-feedback" *ngIf="mensagem()" aria-live="polite">{{ mensagem() }}</p>
      <p class="admin-feedback admin-feedback--erro" *ngIf="erro()" aria-live="assertive">{{ erro() }}</p>

      <article class="admin-bloco" *ngIf="carregando()">Carregando pratos...</article>
      <article class="admin-bloco admin-vazio" *ngIf="!carregando() && pratos().length === 0">Nenhum prato encontrado.</article>

      <div class="admin-lista" *ngIf="!carregando() && pratos().length > 0">
        <article class="admin-item" *ngFor="let prato of pratos()">
          <img [src]="prato.urlImagem || '/assets/prato-principal.png'" [alt]="prato.nome">
          <div>
            <strong>{{ prato.nome }}</strong>
            <span>{{ prato.grupoAcompanhamento?.nome || 'Sem grupo' }}</span>
            <small>Dias: {{ nomesDias(prato.diasSemana) }} | a partir de {{ prato.precos.pequenaDinheiroPix | currency:'BRL' }}</small>
          </div>
          <div class="admin-badges">
            <span [class.admin-badge--off]="!prato.estaDisponivel">{{ prato.estaDisponivel ? 'Disponivel' : 'Indisponivel' }}</span>
            <span [class.admin-badge--off]="!prato.estaAtivo">{{ prato.estaAtivo ? 'Ativo' : 'Inativo' }}</span>
          </div>
          <div class="admin-acoes">
            <button type="button" (click)="editar(prato.id)">Editar</button>
            <button type="button" (click)="alterarDisponibilidade(prato)">{{ prato.estaDisponivel ? 'Marcar indisponivel' : 'Marcar disponivel' }}</button>
            <button type="button" (click)="alterarAtivacao(prato)">{{ prato.estaAtivo ? 'Desativar' : 'Ativar' }}</button>
          </div>
        </article>
      </div>

      <dialog class="admin-modal" [open]="modalAberto()" aria-labelledby="titulo-prato">
        <form [formGroup]="form" (ngSubmit)="salvar()">
          <header>
            <h2 id="titulo-prato">{{ pratoEditandoId() ? 'Editar prato' : 'Novo prato' }}</h2>
            <button type="button" aria-label="Fechar formulario" (click)="fecharModal()">×</button>
          </header>

          <section class="admin-form-grid">
            <label class="admin-campo">
              Nome
              <input formControlName="nome">
              <small *ngIf="campoInvalido('nome')">Informe entre 2 e 120 caracteres.</small>
            </label>
            <label class="admin-campo">
              Grupo de acompanhamento
              <select formControlName="grupoAcompanhamentoId">
                <option value="">Selecione</option>
                <option *ngFor="let grupo of grupos()" [value]="grupo.id">{{ grupo.nome }}</option>
              </select>
              <small *ngIf="campoInvalido('grupoAcompanhamentoId')">Escolha um grupo ativo.</small>
            </label>
            <label class="admin-campo admin-campo--largo">
              Descricao
              <textarea formControlName="descricao" rows="3"></textarea>
            </label>
            <label class="admin-campo admin-campo--largo">
              URL da imagem
              <input formControlName="urlImagem" placeholder="Opcional">
            </label>
          </section>

          <h3>Precos</h3>
          <section class="admin-form-grid">
            <label class="admin-campo">P Dinheiro/Pix <input type="number" min="0.01" step="0.01" formControlName="pequenaDinheiroPix"></label>
            <label class="admin-campo">P Cartao <input type="number" min="0.01" step="0.01" formControlName="pequenaCartao"></label>
            <label class="admin-campo">G Dinheiro/Pix <input type="number" min="0.01" step="0.01" formControlName="grandeDinheiroPix"></label>
            <label class="admin-campo">G Cartao <input type="number" min="0.01" step="0.01" formControlName="grandeCartao"></label>
          </section>

          <h3>Dias do cardapio</h3>
          <section class="admin-dias">
            <label *ngFor="let dia of dias">
              <input type="checkbox" [formControlName]="'dia' + dia.valor">
              {{ dia.nome }}
              <input type="number" min="0" [formControlName]="'ordem' + dia.valor" aria-label="Ordem de exibicao">
            </label>
          </section>

          <section class="admin-switches">
            <label><input type="checkbox" formControlName="estaDisponivel"> Disponivel para venda</label>
            <label><input type="checkbox" formControlName="estaAtivo"> Cadastro ativo</label>
          </section>

          <footer>
            <button class="botao secundario" type="button" (click)="fecharModal()">Cancelar</button>
            <button class="botao" type="submit" [disabled]="salvando()">{{ salvando() ? 'Salvando...' : 'Salvar prato' }}</button>
          </footer>
        </form>
      </dialog>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PratosPage {
  private readonly service = inject(PratosAdministrativoService);
  private readonly fb = inject(UntypedFormBuilder);

  readonly dias = [
    { valor: 1, nome: 'Segunda-feira' },
    { valor: 2, nome: 'Terca-feira' },
    { valor: 3, nome: 'Quarta-feira' },
    { valor: 4, nome: 'Quinta-feira' },
    { valor: 5, nome: 'Sexta-feira' },
    { valor: 6, nome: 'Sabado' },
    { valor: 7, nome: 'Domingo' }
  ];

  readonly pratos = signal<PratoAdminResumo[]>([]);
  readonly grupos = signal<GrupoAcompanhamentoAdmin[]>([]);
  readonly carregando = signal(false);
  readonly salvando = signal(false);
  readonly modalAberto = signal(false);
  readonly pratoEditandoId = signal<string | null>(null);
  readonly mensagem = signal('');
  readonly erro = signal('');

  readonly filtrosForm = this.fb.group({
    nome: [''],
    diaSemana: [''],
    estaDisponivel: [''],
    estaAtivo: ['']
  });

  readonly form = this.fb.group({
    nome: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]],
    descricao: [''],
    urlImagem: [''],
    grupoAcompanhamentoId: ['', Validators.required],
    pequenaDinheiroPix: [0, [Validators.required, Validators.min(0.01)]],
    pequenaCartao: [0, [Validators.required, Validators.min(0.01)]],
    grandeDinheiroPix: [0, [Validators.required, Validators.min(0.01)]],
    grandeCartao: [0, [Validators.required, Validators.min(0.01)]],
    dia1: [true],
    dia2: [false],
    dia3: [false],
    dia4: [false],
    dia5: [false],
    dia6: [false],
    dia7: [false],
    ordem1: [0],
    ordem2: [0],
    ordem3: [0],
    ordem4: [0],
    ordem5: [0],
    ordem6: [0],
    ordem7: [0],
    estaDisponivel: [true],
    estaAtivo: [true]
  });

  constructor() {
    this.carregarGrupos();
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set('');
    const filtros = this.filtrosForm.getRawValue();
    this.service.listar({
      nome: filtros.nome,
      diaSemana: filtros.diaSemana === '' ? '' : Number(filtros.diaSemana),
      estaDisponivel: this.valorBooleano(filtros.estaDisponivel),
      estaAtivo: this.valorBooleano(filtros.estaAtivo)
    }).pipe(finalize(() => this.carregando.set(false))).subscribe({
      next: (pratos) => this.pratos.set(pratos),
      error: () => this.erro.set('Nao foi possivel carregar os pratos.')
    });
  }

  limparFiltros(): void {
    this.filtrosForm.reset();
    this.carregar();
  }

  abrirNovo(): void {
    this.pratoEditandoId.set(null);
    this.form.reset({
      nome: '',
      descricao: '',
      urlImagem: '',
      grupoAcompanhamentoId: this.grupos()[0]?.id ?? '',
      pequenaDinheiroPix: 0,
      pequenaCartao: 0,
      grandeDinheiroPix: 0,
      grandeCartao: 0,
      dia1: true,
      dia2: false,
      dia3: false,
      dia4: false,
      dia5: false,
      dia6: false,
      dia7: false,
      ordem1: 0,
      ordem2: 0,
      ordem3: 0,
      ordem4: 0,
      ordem5: 0,
      ordem6: 0,
      ordem7: 0,
      estaDisponivel: true,
      estaAtivo: true
    });
    this.modalAberto.set(true);
  }

  editar(id: string): void {
    this.erro.set('');
    this.modalAberto.set(true);
    this.pratoEditandoId.set(id);
    this.service.obterPorId(id).subscribe({
      next: (prato) => this.preencherFormulario(prato),
      error: () => this.erro.set('Nao foi possivel carregar o prato.')
    });
  }

  salvar(): void {
    if (this.form.invalid || this.diasSelecionados().length === 0) {
      this.form.markAllAsTouched();
      this.erro.set('Verifique os campos do prato e selecione ao menos um dia.');
      return;
    }

    const id = this.pratoEditandoId();
    const request = this.montarRequest();
    const operacao = id ? this.service.atualizar(id, request) : this.service.criar(request);
    this.salvando.set(true);
    this.erro.set('');
    operacao.pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: () => {
        this.mensagem.set('Prato salvo com sucesso.');
        this.fecharModal();
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  alterarDisponibilidade(prato: PratoAdminResumo): void {
    this.service.alterarDisponibilidade(prato.id, !prato.estaDisponivel).subscribe({
      next: () => {
        this.mensagem.set('Disponibilidade atualizada.');
        this.carregar();
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  alterarAtivacao(prato: PratoAdminResumo): void {
    if (prato.estaAtivo && !window.confirm(`Desativar o prato ${prato.nome}?`)) {
      return;
    }
    this.service.alterarAtivacao(prato.id, !prato.estaAtivo).subscribe({
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

  nomesDias(dias: number[]): string {
    return dias.map((dia) => this.dias.find((item) => item.valor === dia)?.nome.replace('-feira', '') ?? dia).join(', ');
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  private carregarGrupos(): void {
    this.service.listarGruposAcompanhamento().subscribe({
      next: (grupos) => this.grupos.set(grupos),
      error: () => this.erro.set('Nao foi possivel carregar os grupos de acompanhamento.')
    });
  }

  private preencherFormulario(prato: PratoAdminDetalhe): void {
    const dias = new Map(prato.diasSemana.map((dia) => [dia.diaSemana, dia]));
    this.form.patchValue({
      nome: prato.nome,
      descricao: prato.descricao ?? '',
      urlImagem: prato.urlImagem ?? '',
      grupoAcompanhamentoId: prato.grupoAcompanhamentoId ?? '',
      pequenaDinheiroPix: prato.precos.pequenaDinheiroPix,
      pequenaCartao: prato.precos.pequenaCartao,
      grandeDinheiroPix: prato.precos.grandeDinheiroPix,
      grandeCartao: prato.precos.grandeCartao,
      estaDisponivel: prato.estaDisponivel,
      estaAtivo: prato.estaAtivo
    });
    this.dias.forEach((dia) => {
      this.form.get(`dia${dia.valor}`)?.setValue(dias.has(dia.valor));
      this.form.get(`ordem${dia.valor}`)?.setValue(dias.get(dia.valor)?.ordemExibicao ?? 0);
    });
  }

  private montarRequest(): PratoAdminSalvar {
    const valor = this.form.getRawValue();
    return {
      nome: valor.nome.trim(),
      descricao: this.opcional(valor.descricao),
      urlImagem: this.opcional(valor.urlImagem),
      grupoAcompanhamentoId: valor.grupoAcompanhamentoId,
      estaAtivo: valor.estaAtivo,
      estaDisponivel: valor.estaDisponivel,
      precos: {
        pequenaDinheiroPix: Number(valor.pequenaDinheiroPix),
        pequenaCartao: Number(valor.pequenaCartao),
        grandeDinheiroPix: Number(valor.grandeDinheiroPix),
        grandeCartao: Number(valor.grandeCartao)
      },
      diasSemana: this.diasSelecionados()
    };
  }

  private diasSelecionados(): DiaPratoAdmin[] {
    return this.dias
      .filter((dia) => this.form.get(`dia${dia.valor}`)?.value === true)
      .map((dia) => ({
        diaSemana: dia.valor,
        ordemExibicao: Number(this.form.get(`ordem${dia.valor}`)?.value ?? 0),
        estaAtivo: true
      }));
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

  private opcional(valor: string): string | null {
    const texto = valor.trim();
    return texto ? texto : null;
  }
}
