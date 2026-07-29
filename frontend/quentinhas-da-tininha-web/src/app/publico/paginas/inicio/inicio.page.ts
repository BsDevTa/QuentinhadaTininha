import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { catchError, finalize, of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CardapioDia, DiaSemana, Prato, Restaurante } from '../../../compartilhado/modelos/cardapio.model';
import { CardapioService } from '../../../compartilhado/servicos/cardapio.service';
import { PedidoService } from '../../../compartilhado/servicos/pedido.service';
import { WhatsappService } from '../../../compartilhado/servicos/whatsapp.service';
import { CabecalhoComponent } from '../../componentes/cabecalho/cabecalho.component';
import { CardapioDiaComponent } from '../../componentes/cardapio-dia/cardapio-dia.component';
import { BeneficiosComponent } from '../../componentes/beneficios/beneficios.component';
import { ComoFuncionaComponent } from '../../componentes/como-funciona/como-funciona.component';
import { ContatoComponent } from '../../componentes/contato/contato.component';
import { HeroComponent } from '../../componentes/hero/hero.component';
import { RodapeComponent } from '../../componentes/rodape/rodape.component';
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

      @if (carregando()) {
        <section class="estado-cardapio">
          <h3>Carregando cardapio...</h3>
          <p>Estamos buscando as opcoes fresquinhas da Tininha.</p>
        </section>
      }

      @if (cardapio(); as cardapioDia) {
        <app-cardapio-dia
          [cardapio]="cardapioDia"
          [diaSelecionado]="diaSelecionado()"
          [diaAtual]="diaAtual"
          [selecionarDia]="selecionarDia"
          [whatsappRestaurante]="restaurante()?.whatsapp ?? ''"
          [restauranteAberto]="restaurante()?.estaAberto ?? false"
          [mensagemStatus]="restaurante()?.mensagemStatus ?? ''"
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
export class InicioPage implements OnInit {
  private readonly cardapioService = inject(CardapioService);
  private readonly pedidoService = inject(PedidoService);
  private readonly whatsappService = inject(WhatsappService);

  protected readonly diaAtual = this.cardapioService.obterDiaAtual();
  protected readonly restaurante = signal<Restaurante | null>(null);
  protected readonly cardapio = signal<CardapioDia | null>(null);
  protected readonly diaSelecionado = signal<DiaSemana>(this.diaAtual);
  protected readonly carregando = signal(false);
  protected readonly mensagemErro = signal('');

  ngOnInit(): void {
    this.carregarCardapioHoje();
  }

  protected readonly selecionarDia = (dia: DiaSemana): void => {
    if (this.diaSelecionado() === dia && this.cardapio()) {
      return;
    }

    this.carregarCardapioPorDia(dia);
  };

  protected tentarNovamente(): void {
    this.carregarCardapioPorDia(this.diaSelecionado());
  }

  protected readonly criarLinkPedido = (prato: Prato): string => {
    const whatsapp = this.restaurante()?.whatsapp ?? '';
    if (!this.restaurante()?.estaAberto || !whatsapp.trim()) {
      return '#cardapio';
    }

    const valor = this.pedidoService.calcularPreco(prato, 'P', 'dinheiro_pix');
    return this.whatsappService.criarLinkPedido(prato, 'P', 'dinheiro_pix', [], valor, whatsapp);
  };

  protected linkPedidoGeral(): string {
    return this.cardapio()?.pratos[0]
      ? this.criarLinkPedido(this.cardapio()!.pratos[0])
      : '#cardapio';
  }

  private carregarCardapioHoje(): void {
    this.iniciarCarregamento();

    this.cardapioService.obterCardapioHoje()
      .pipe(
        catchError((erro: unknown) => this.tratarErro(erro)),
        finalize(() => this.carregando.set(false))
      )
      .subscribe((cardapio) => this.aplicarCardapio(cardapio));
  }

  private carregarCardapioPorDia(dia: DiaSemana): void {
    this.diaSelecionado.set(dia);
    this.iniciarCarregamento();

    this.cardapioService.obterCardapioPorDia(dia)
      .pipe(
        catchError((erro: unknown) => this.tratarErro(erro)),
        finalize(() => this.carregando.set(false))
      )
      .subscribe((cardapio) => this.aplicarCardapio(cardapio));
  }

  private aplicarCardapio(cardapio: CardapioDia): void {
    this.cardapio.set(cardapio);
    this.diaSelecionado.set(cardapio.diaSemana);

    if (cardapio.restaurante) {
      this.restaurante.set(cardapio.restaurante);
    }
  }

  private iniciarCarregamento(): void {
    this.carregando.set(true);
    this.mensagemErro.set('');
  }

  private tratarErro(erro: unknown) {
    if (!environment.production) {
      console.error('Erro ao carregar cardapio publico', erro);
    }

    this.mensagemErro.set('Nao conseguimos carregar o cardapio agora. Tente novamente em instantes.');
    this.restaurante.set(this.restaurante() ?? this.restauranteIndisponivel());
    this.cardapio.set(this.cardapio() ?? {
      diaSemana: this.diaSelecionado(),
      nomeDia: '',
      pratos: []
    });

    return of(this.cardapio()!);
  }

  private restauranteIndisponivel(): Restaurante {
    return {
      nome: 'Quentinhas da Tininha',
      whatsapp: '',
      instagram: '@quentinhasdatininha',
      endereco: 'Salvador - BA',
      horarioFuncionamento: 'Segunda a sabado, das 10h as 14h',
      estaAberto: false,
      mensagemStatus: 'Nao conseguimos carregar o status do restaurante agora.',
      urlLogo: '/assets/logo-tininha.svg',
      formasPagamento: ['Pix', 'Visa', 'Mastercard', 'Hipercard']
    };
  }
}
