import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Subscription, TimeoutError, catchError, finalize, of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CardapioDia, DiaSemana, Prato, Restaurante } from '../../../compartilhado/modelos/cardapio.model';
import { CardapioService } from '../../../compartilhado/servicos/cardapio.service';
import { OverlayHandle, OverlayService } from '../../../compartilhado/servicos/overlay.service';
import { PedidoService } from '../../../compartilhado/servicos/pedido.service';
import { WhatsappService } from '../../../compartilhado/servicos/whatsapp.service';
import { CabecalhoComponent } from '../../componentes/cabecalho/cabecalho.component';
import { CardapioDiaComponent } from '../../componentes/cardapio-dia/cardapio-dia.component';
import { BeneficiosComponent } from '../../componentes/beneficios/beneficios.component';
import { ComoFuncionaComponent } from '../../componentes/como-funciona/como-funciona.component';
import { ContatoComponent } from '../../componentes/contato/contato.component';
import { DiaBloqueadoModalComponent } from '../../componentes/dia-bloqueado-modal/dia-bloqueado-modal.component';
import { HeroComponent } from '../../componentes/hero/hero.component';
import { PersonalizacaoPedidoModalComponent } from '../../componentes/personalizacao-pedido-modal/personalizacao-pedido-modal.component';
import { RodapeComponent } from '../../componentes/rodape/rodape.component';
import { DiaBloqueadoSelecionado, EstadoDiaSeletor } from '../../componentes/seletor-dia/seletor-dia.component';
import { SobreComponent } from '../../componentes/sobre/sobre.component';
import { StatusRestauranteComponent } from '../../componentes/status-restaurante/status-restaurante.component';

@Component({
  selector: 'app-inicio-page',
  standalone: true,
  imports: [CabecalhoComponent, HeroComponent, StatusRestauranteComponent, CardapioDiaComponent, BeneficiosComponent, SobreComponent, ComoFuncionaComponent, ContatoComponent, RodapeComponent],
  template: `
    <main class="pagina-referencia">
      <app-cabecalho [linkPedido]="linkPedidoGeral()" />
      <app-hero [linkWhatsapp]="linkPedidoGeral()" />

      @if (restaurante(); as dadosRestaurante) {
        <app-status-restaurante [restaurante]="dadosRestaurante" />
      }

      @if (mensagemErro()) {
        <section class="estado-cardapio">
          <h3>Nao conseguimos carregar o cardapio agora.</h3>
          <p>Tente novamente em instantes.</p>
          <button class="botao" type="button" (click)="tentarNovamente()">Tentar novamente</button>
        </section>
      }

      @if (carregando() && !cardapio()) {
        <section class="cardapio-skeleton" aria-label="Carregando cardapio">
          @for (item of skeletonCards; track item) {
            <article>
              <span></span>
              <div>
                <strong></strong>
                <small></small>
                <small></small>
              </div>
            </article>
          }
          @if (demorando()) {
            <div class="estado-cardapio estado-cardapio--compacto">
              <h3>Esta demorando mais que o esperado.</h3>
              <p>Voce pode tentar novamente agora.</p>
              <button class="botao" type="button" (click)="tentarNovamente()">Tentar novamente</button>
            </div>
          }
        </section>
      } @else if (carregando() && demorando()) {
        <section class="estado-cardapio estado-cardapio--compacto">
          <h3>Esta demorando mais que o esperado.</h3>
          <button class="botao secundario" type="button" (click)="tentarNovamente()">Tentar novamente</button>
        </section>
      }

      @if (cardapio(); as cardapioDia) {
        <app-cardapio-dia
          [cardapio]="cardapioDia"
          [diaSelecionado]="diaSelecionado()"
          [diaAtual]="diaAtual"
          [selecionarDia]="selecionarDia"
          [statusDias]="statusDias()"
          [whatsappRestaurante]="restaurante()?.whatsapp ?? ''"
          [restauranteAberto]="restaurante()?.permitirPedidos ?? false"
          [mensagemStatus]="restaurante()?.mensagemStatus ?? ''"
          (personalizarPrato)="abrirPersonalizacao($event)"
          (diaBloqueado)="abrirModalDiaBloqueado($event)"
        />
      }

      <app-beneficios />
      <app-sobre />
      <app-como-funciona />
      @if (restaurante(); as dadosRestaurante) {
        <app-contato [restaurante]="dadosRestaurante" />
        <app-rodape [restaurante]="dadosRestaurante" />
      }
    </main>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InicioPage implements OnInit, OnDestroy {
  private readonly httpClient = inject(HttpClient);
  private readonly cardapioService = inject(CardapioService);
  private readonly overlayService = inject(OverlayService);
  private readonly pedidoService = inject(PedidoService);
  private readonly whatsappService = inject(WhatsappService);

  private personalizacaoOverlay?: OverlayHandle<PersonalizacaoPedidoModalComponent>;
  private modalDiaOverlay?: OverlayHandle<DiaBloqueadoModalComponent>;
  private fecharPersonalizacaoSubscription?: Subscription;
  private fecharModalDiaSubscription?: Subscription;
  private cardapioSubscription?: Subscription;
  private disponibilidadeSubscription?: Subscription;
  private avisoDemora?: ReturnType<typeof setTimeout>;
  private requisicaoCardapioAtual = 0;
  private readonly dataExcecaoPedidosTeste = '2026-08-09';

  protected readonly diaAtual = this.cardapioService.obterDiaAtual();
  protected readonly restaurante = signal<Restaurante | null>(null);
  protected readonly cardapio = signal<CardapioDia | null>(null);
  protected readonly diaSelecionado = signal<DiaSemana>(this.diaAtual);
  protected readonly carregando = signal(false);
  protected readonly demorando = signal(false);
  protected readonly mensagemErro = signal('');
  protected readonly statusDias = signal<Partial<Record<DiaSemana, EstadoDiaSeletor>>>({});
  protected readonly skeletonCards = [1, 2, 3];

  ngOnInit(): void {
    this.carregarDisponibilidadeDias();
    this.carregarCardapioHoje();
  }

  ngOnDestroy(): void {
    this.cardapioSubscription?.unsubscribe();
    this.disponibilidadeSubscription?.unsubscribe();
    this.limparAvisoDemora();
    this.fecharPersonalizacao();
    this.fecharModalDiaBloqueado();
  }

  protected readonly selecionarDia = (dia: DiaSemana): void => {
    this.fecharPersonalizacao();
    this.fecharModalDiaBloqueado();

    if (this.diaSelecionado() === dia && this.cardapio()) {
      return;
    }

    this.carregarCardapioPorDia(dia);
  };

  protected tentarNovamente(): void {
    this.carregarCardapioPorDia(this.diaSelecionado());
  }

  protected abrirPersonalizacao(prato: Prato): void {
    this.fecharModalDiaBloqueado();
    this.fecharPersonalizacao();

    const dataPedido = this.obterDataPedidoParaFinalizacao();
    this.personalizacaoOverlay = this.overlayService.open(PersonalizacaoPedidoModalComponent, {
      prato,
      whatsappRestaurante: this.restaurante()?.whatsapp ?? '',
      restauranteAberto: this.permitirPedidoParaData(dataPedido),
      dataPedido
    });

    this.fecharPersonalizacaoSubscription = this.personalizacaoOverlay.componentRef.instance.fechar
      .subscribe(() => this.fecharPersonalizacao());
  }

  protected fecharPersonalizacao(): void {
    this.fecharPersonalizacaoSubscription?.unsubscribe();
    this.fecharPersonalizacaoSubscription = undefined;
    this.personalizacaoOverlay?.close();
    this.personalizacaoOverlay = undefined;
  }

  protected abrirModalDiaBloqueado(dia: DiaBloqueadoSelecionado): void {
    this.fecharPersonalizacao();
    this.fecharModalDiaBloqueado();

    this.modalDiaOverlay = this.overlayService.open(DiaBloqueadoModalComponent, {
      nome: dia.nome,
      motivo: dia.motivo,
      data: dia.data
    });

    this.fecharModalDiaSubscription = this.modalDiaOverlay.componentRef.instance.fechar
      .subscribe(() => this.fecharModalDiaBloqueado());
  }

  protected fecharModalDiaBloqueado(): void {
    this.fecharModalDiaSubscription?.unsubscribe();
    this.fecharModalDiaSubscription = undefined;
    this.modalDiaOverlay?.close();
    this.modalDiaOverlay = undefined;
  }

  protected readonly criarLinkPedido = (prato: Prato): string => {
    const whatsapp = this.restaurante()?.whatsapp ?? '';
    if (!this.permitirPedidoParaData(this.obterDataPedidoParaFinalizacao()) || !whatsapp.trim()) {
      return '#cardapio';
    }

    const valor = this.pedidoService.calcularPreco(prato, 'P', 'pix');
    return this.whatsappService.criarLinkPedido(prato, 'P', 'pix', [], valor, whatsapp);
  };

  protected linkPedidoGeral(): string {
    return this.cardapio()?.pratos[0]
      ? this.criarLinkPedido(this.cardapio()!.pratos[0])
      : '#cardapio';
  }

  private carregarCardapioHoje(): void {
    const requisicaoId = this.iniciarCarregamento();

    this.cardapioSubscription = this.cardapioService.obterCardapioHoje()
      .pipe(
        catchError((erro: unknown) => {
          this.tratarErro(erro);
          return of(null);
        }),
        finalize(() => this.finalizarCarregamento(requisicaoId))
      )
      .subscribe((cardapio) => {
        if (cardapio && requisicaoId === this.requisicaoCardapioAtual) {
          this.aplicarCardapio(cardapio);
        }
      });
  }

  private carregarCardapioPorDia(dia: DiaSemana): void {
    this.diaSelecionado.set(dia);
    const requisicaoId = this.iniciarCarregamento();

    this.cardapioSubscription = this.cardapioService.obterCardapioPorDia(dia)
      .pipe(
        catchError((erro: unknown) => {
          this.tratarErro(erro);
          return of(null);
        }),
        finalize(() => this.finalizarCarregamento(requisicaoId))
      )
      .subscribe((cardapio) => {
        if (cardapio && requisicaoId === this.requisicaoCardapioAtual) {
          this.aplicarCardapio(cardapio);
        }
      });
  }

  private carregarDisponibilidadeDias(): void {
    const dataInicial = this.criarDataLocalHoje();
    const dataFinal = new Date(dataInicial);
    dataFinal.setDate(dataInicial.getDate() + 6);
    const apiUrl = environment.apiUrl.replace(/\/$/, '');
    const url = `${apiUrl}/publico/disponibilidade?dataInicial=${this.formatarDataIso(dataInicial)}&dataFinal=${this.formatarDataIso(dataFinal)}`;

    this.disponibilidadeSubscription?.unsubscribe();
    this.disponibilidadeSubscription = this.httpClient
      .get<DisponibilidadePublicaResposta>(url)
      .pipe(
        catchError((erro: unknown) => {
          if (!environment.production) {
            console.error('Erro ao carregar disponibilidade publica', erro);
          }

          return of({ datas: [] } as DisponibilidadePublicaResposta);
        })
      )
      .subscribe((disponibilidade) =>
        this.statusDias.set(this.mapearDisponibilidadeDias(disponibilidade.datas ?? []))
      );
  }

  private aplicarCardapio(cardapio: CardapioDia): void {
    this.diaSelecionado.set(cardapio.diaSemana);

    if (cardapio.restaurante) {
      const dataPedido = this.obterDataPedidoParaFinalizacao();
      const restaurante = this.deveAplicarExcecaoPedidosTeste(dataPedido)
        ? {
            ...cardapio.restaurante,
            estaAberto: true,
            permitirPedidos: true,
            motivoBloqueio: null
          }
        : cardapio.restaurante;

      this.restaurante.set(restaurante);
      this.cardapio.set({ ...cardapio, restaurante });
      return;
    }

    this.cardapio.set(cardapio);
  }

  private iniciarCarregamento(): number {
    const requisicaoId = ++this.requisicaoCardapioAtual;
    this.cardapioSubscription?.unsubscribe();
    this.limparAvisoDemora();
    this.carregando.set(true);
    this.demorando.set(false);
    this.mensagemErro.set('');

    this.avisoDemora = setTimeout(() => {
      if (requisicaoId === this.requisicaoCardapioAtual && this.carregando()) {
        this.demorando.set(true);
      }
    }, 3500);

    return requisicaoId;
  }

  private finalizarCarregamento(requisicaoId: number): void {
    if (requisicaoId !== this.requisicaoCardapioAtual) {
      return;
    }

    this.carregando.set(false);
    this.demorando.set(false);
    this.limparAvisoDemora();
  }

  private tratarErro(erro: unknown): void {
    if (!environment.production) {
      console.error('Erro ao carregar cardapio publico', erro);
    }

    this.mensagemErro.set(this.criarMensagemErroCardapio(erro));
    this.restaurante.set(this.restaurante() ?? this.restauranteIndisponivel());
    this.cardapio.set(this.cardapio() ?? {
      diaSemana: this.diaSelecionado(),
      nomeDia: '',
      pratos: []
    });
  }

  private criarMensagemErroCardapio(erro: unknown): string {
    if (erro instanceof TimeoutError) {
      return 'O cardapio demorou mais que o esperado. Tente novamente.';
    }

    if (erro instanceof HttpErrorResponse) {
      if (erro.status === 0) {
        return 'Nao foi possivel conectar com a API agora. Tente novamente.';
      }

      if (erro.status === 404) {
        return 'Cardapio ainda nao disponivel para este dia.';
      }

      if (erro.status >= 500) {
        return 'A API encontrou um erro ao carregar o cardapio. Tente novamente.';
      }
    }

    return 'Nao conseguimos carregar o cardapio agora. Tente novamente em instantes.';
  }

  private mapearDisponibilidadeDias(
    datas: DisponibilidadeDataPublicaResposta[]
  ): Partial<Record<DiaSemana, EstadoDiaSeletor>> {
    const statusDias: Partial<Record<DiaSemana, EstadoDiaSeletor>> = {};

    for (const data of datas) {
      const diaSemana = this.obterDiaSemanaData(data.data);
      statusDias[diaSemana] = {
        data: data.data,
        permitirPedidos: this.deveAplicarExcecaoHoje()
          ? true
          : data.permitirPedidos ?? data.disponivel,
        motivo: this.deveAplicarExcecaoHoje() ? null : data.motivo,
        motivoBloqueio: this.deveAplicarExcecaoHoje() ? null : data.motivoBloqueio
      };
    }

    return statusDias;
  }

  private permitirPedidoParaData(dataPedido: string): boolean {
    return this.deveAplicarExcecaoPedidosTeste(dataPedido) ||
      (this.restaurante()?.permitirPedidos ?? false);
  }

  private criarDataLocalHoje(): Date {
    const hoje = new Date();
    return new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate());
  }

  private obterDiaSemanaData(dataIso: string): DiaSemana {
    const [ano, mes, dia] = dataIso.split('-').map(Number);
    return new Date(ano, mes - 1, dia).getDay() as DiaSemana;
  }

  private formatarDataIso(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');

    return `${ano}-${mes}-${dia}`;
  }

  private obterDataPedidoSelecionada(): string {
    const dataDisponibilidade = this.statusDias()[this.diaSelecionado()]?.data;
    if (dataDisponibilidade) {
      return dataDisponibilidade;
    }

    const hoje = this.criarDataLocalHoje();
    const diferencaDias = (this.diaSelecionado() - hoje.getDay() + 7) % 7;
    hoje.setDate(hoje.getDate() + diferencaDias);
    return this.formatarDataIso(hoje);
  }

  private obterDataPedidoParaFinalizacao(): string {
    return this.deveAplicarExcecaoHoje()
      ? this.dataExcecaoPedidosTeste
      : this.obterDataPedidoSelecionada();
  }

  private deveAplicarExcecaoPedidosTeste(dataPedido: string): boolean {
    return dataPedido === this.dataExcecaoPedidosTeste && this.deveAplicarExcecaoHoje();
  }

  private deveAplicarExcecaoHoje(): boolean {
    return this.criarDataLocalSalvadorHojeIso() === this.dataExcecaoPedidosTeste;
  }

  private criarDataLocalSalvadorHojeIso(): string {
    const partes = new Intl.DateTimeFormat('en-CA', {
      timeZone: 'America/Bahia',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).formatToParts(new Date());

    const ano = partes.find((parte) => parte.type === 'year')?.value;
    const mes = partes.find((parte) => parte.type === 'month')?.value;
    const dia = partes.find((parte) => parte.type === 'day')?.value;

    return ano && mes && dia ? `${ano}-${mes}-${dia}` : this.formatarDataIso(this.criarDataLocalHoje());
  }

  private limparAvisoDemora(): void {
    if (this.avisoDemora) {
      clearTimeout(this.avisoDemora);
      this.avisoDemora = undefined;
    }
  }

  private restauranteIndisponivel(): Restaurante {
    return {
      nome: 'Quentinhas da Tininha',
      whatsapp: environment.whatsappRestaurante,
      instagram: '@quentinhasdatininha',
      endereco: 'Rua Apolinario de Santana, 129 - Engenho Velho da Federacao',
      horarioFuncionamento: 'Segunda a sabado, das 10h as 14h',
      estaAberto: false,
      permitirPedidos: false,
      motivoBloqueio: 'Status indisponivel',
      mensagemStatus: 'Nao conseguimos carregar o status do restaurante agora.',
      urlLogo: '/assets/logo-tininha.svg',
      formasPagamento: ['Dinheiro', 'PIX', 'Cartão']
    };
  }
}

interface DisponibilidadePublicaResposta {
  datas: DisponibilidadeDataPublicaResposta[];
}

interface DisponibilidadeDataPublicaResposta {
  data: string;
  disponivel: boolean;
  permitirPedidos: boolean;
  motivo?: string | null;
  motivoBloqueio?: string | null;
}
