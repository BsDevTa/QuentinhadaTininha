import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { DisponibilidadeDataAdmin, FuncionamentoAdmin } from '../../modelos/admin-cardapio.model';
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

      <article class="admin-bloco admin-disponibilidade">
        <header class="admin-disponibilidade__topo">
          <div>
            <span class="admin-tag">Datas</span>
            <h2>Outros dias de funcionamento</h2>
            <p>Libere ou bloqueie datas especificas para pedidos.</p>
          </div>
        </header>

        <form class="admin-form-grid admin-form-grid--datas" [formGroup]="periodoForm" (ngSubmit)="carregarDisponibilidade()">
          <label class="admin-campo">
            Data inicial
            <input type="date" formControlName="dataInicial">
            <small *ngIf="campoPeriodoInvalido('dataInicial')">Informe a data inicial.</small>
          </label>
          <label class="admin-campo">
            Data final
            <input type="date" formControlName="dataFinal">
            <small *ngIf="campoPeriodoInvalido('dataFinal')">Informe a data final.</small>
          </label>
          <div class="admin-filtros__acoes">
            <button class="botao" type="submit" [disabled]="carregandoDisponibilidade() || periodoForm.invalid">
              {{ carregandoDisponibilidade() ? 'Carregando...' : 'Filtrar dias' }}
            </button>
          </div>
        </form>
      </article>

      <article class="admin-bloco" *ngIf="carregandoDisponibilidade()">Carregando dias...</article>
      <article class="admin-bloco admin-vazio" *ngIf="!carregandoDisponibilidade() && disponibilidade().length === 0">
        Nenhuma data encontrada no periodo selecionado.
      </article>

      <div class="admin-lista admin-disponibilidade-lista" *ngIf="!carregandoDisponibilidade() && disponibilidade().length > 0">
        <article
          class="admin-dia"
          *ngFor="let dia of disponibilidade(); trackBy: rastrearData"
          [class.admin-dia--bloqueado]="!dia.permitirPedidos">
          <div class="admin-dia__info">
            <span>{{ formatarDataLonga(dia.data) }}</span>
            <strong>{{ dia.permitirPedidos ? 'Liberado para pedidos' : 'Bloqueado para pedidos' }}</strong>
            <small>{{ dia.motivo || textoMotivoPadrao(dia) }}</small>
          </div>

          <div class="admin-badges">
            <span [class.admin-badge--off]="!dia.permitirPedidos">{{ dia.status }}</span>
          </div>

          <div class="admin-dia__acoes">
            <button type="button" (click)="liberarData(dia)" [disabled]="salvandoData() === dia.data || dia.permitirPedidos">
              Liberar
            </button>
            <button type="button" (click)="bloquearData(dia)" [disabled]="salvandoData() === dia.data || !dia.permitirPedidos">
              Bloquear
            </button>
            <button type="button" (click)="editarMotivo(dia)" [disabled]="salvandoData() === dia.data">
              Editar motivo
            </button>
          </div>

          <section class="admin-dia__motivo" *ngIf="dataEmEdicao() === dia.data">
            <label class="admin-campo">
              Motivo exibido para o cliente
              <textarea rows="2" [value]="motivos()[dia.data] || ''" (input)="atualizarMotivo(dia.data, $any($event.target).value)"></textarea>
            </label>
            <div class="admin-dia__edicao">
              <button class="botao" type="button" (click)="salvarMotivo(dia)" [disabled]="salvandoData() === dia.data">
                {{ salvandoData() === dia.data ? 'Salvando...' : 'Salvar motivo' }}
              </button>
              <button class="botao secundario" type="button" (click)="cancelarMotivo(dia)" [disabled]="salvandoData() === dia.data">
                Cancelar
              </button>
            </div>
          </section>
        </article>
      </div>
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
  readonly carregandoDisponibilidade = signal(false);
  readonly disponibilidade = signal<DisponibilidadeDataAdmin[]>([]);
  readonly dataEmEdicao = signal<string | null>(null);
  readonly salvandoData = signal<string | null>(null);
  readonly motivos = signal<Record<string, string>>({});

  readonly form = this.fb.nonNullable.group({
    estaAberto: [true],
    mensagemStatus: ['', [Validators.required, Validators.maxLength(180)]],
    horarioFuncionamento: ['', [Validators.required, Validators.maxLength(160)]]
  });

  readonly periodoForm = this.fb.nonNullable.group({
    dataInicial: [this.formatarData(new Date()), Validators.required],
    dataFinal: [this.formatarData(this.adicionarDias(new Date(), 14)), Validators.required]
  });

  constructor() {
    this.carregar();
    this.carregarDisponibilidade();
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

  carregarDisponibilidade(): void {
    if (this.periodoForm.invalid) {
      this.periodoForm.markAllAsTouched();
      return;
    }

    const periodo = this.periodoForm.getRawValue();
    if (periodo.dataFinal < periodo.dataInicial) {
      this.erro.set('A data final deve ser maior ou igual a data inicial.');
      return;
    }

    this.erro.set('');
    this.carregandoDisponibilidade.set(true);
    this.service.listarDisponibilidade(periodo.dataInicial, periodo.dataFinal)
      .pipe(finalize(() => this.carregandoDisponibilidade.set(false)))
      .subscribe({
        next: (datas) => {
          this.disponibilidade.set(datas);
          this.sincronizarMotivos(datas);
        },
        error: () => this.erro.set('Nao foi possivel carregar os outros dias de funcionamento.')
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
    this.erro.set('');
    this.mensagem.set('');
    this.salvando.set(true);
    this.service.atualizar(this.form.getRawValue()).pipe(finalize(() => this.salvando.set(false))).subscribe({
      next: (status) => {
        this.status.set(status);
        this.mensagem.set('Funcionamento atualizado.');
      },
      error: () => this.erro.set('Nao foi possivel concluir a operacao.')
    });
  }

  liberarData(dia: DisponibilidadeDataAdmin): void {
    this.salvarStatusData(dia, true);
  }

  bloquearData(dia: DisponibilidadeDataAdmin): void {
    if (!window.confirm(`Bloquear pedidos em ${this.formatarDataLonga(dia.data)}?`)) {
      return;
    }
    this.salvarStatusData(dia, false);
  }

  editarMotivo(dia: DisponibilidadeDataAdmin): void {
    this.atualizarMotivo(dia.data, dia.motivo ?? '');
    this.dataEmEdicao.set(dia.data);
  }

  salvarMotivo(dia: DisponibilidadeDataAdmin): void {
    this.salvarStatusData(dia, dia.permitirPedidos);
  }

  cancelarMotivo(dia: DisponibilidadeDataAdmin): void {
    this.atualizarMotivo(dia.data, dia.motivo ?? '');
    this.dataEmEdicao.set(null);
  }

  atualizarMotivo(data: string, valor: string): void {
    this.motivos.update((motivos) => ({ ...motivos, [data]: valor }));
  }

  campoInvalido(campo: string): boolean {
    const controle = this.form.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  campoPeriodoInvalido(campo: string): boolean {
    const controle = this.periodoForm.get(campo);
    return !!controle && controle.invalid && (controle.dirty || controle.touched);
  }

  rastrearData(_indice: number, dia: DisponibilidadeDataAdmin): string {
    return dia.data;
  }

  textoMotivoPadrao(dia: DisponibilidadeDataAdmin): string {
    return dia.permitirPedidos ? 'Pedidos liberados para esta data.' : 'Pedidos bloqueados para esta data.';
  }

  formatarDataLonga(data: string): string {
    return new Intl.DateTimeFormat('pt-BR', {
      weekday: 'long',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(this.criarDataLocal(data));
  }

  private salvarStatusData(dia: DisponibilidadeDataAdmin, permitirPedidos: boolean): void {
    const motivo = this.normalizarMotivo(this.motivos()[dia.data]);
    const requisicao = permitirPedidos
      ? this.service.liberarData(dia.data, motivo)
      : this.service.bloquearData(dia.data, motivo);

    this.erro.set('');
    this.mensagem.set('');
    this.salvandoData.set(dia.data);
    requisicao.pipe(finalize(() => this.salvandoData.set(null))).subscribe({
      next: (atualizado) => {
        this.substituirDisponibilidade(atualizado);
        this.dataEmEdicao.set(null);
        this.mensagem.set(`Data ${permitirPedidos ? 'liberada' : 'bloqueada'} com sucesso.`);
      },
      error: () => this.erro.set('Nao foi possivel atualizar a data selecionada.')
    });
  }

  private substituirDisponibilidade(atualizado: DisponibilidadeDataAdmin): void {
    this.disponibilidade.update((datas) =>
      datas.map((dia) => dia.data === atualizado.data ? atualizado : dia)
    );
    this.atualizarMotivo(atualizado.data, atualizado.motivo ?? '');
  }

  private sincronizarMotivos(datas: DisponibilidadeDataAdmin[]): void {
    this.motivos.set(datas.reduce<Record<string, string>>((motivos, dia) => {
      motivos[dia.data] = dia.motivo ?? '';
      return motivos;
    }, {}));
  }

  private normalizarMotivo(motivo: string | undefined): string | null {
    const motivoNormalizado = motivo?.trim();
    return motivoNormalizado ? motivoNormalizado : null;
  }

  private adicionarDias(data: Date, quantidade: number): Date {
    const novaData = new Date(data);
    novaData.setDate(novaData.getDate() + quantidade);
    return novaData;
  }

  private formatarData(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }

  private criarDataLocal(data: string): Date {
    return new Date(`${data}T00:00:00`);
  }
}
