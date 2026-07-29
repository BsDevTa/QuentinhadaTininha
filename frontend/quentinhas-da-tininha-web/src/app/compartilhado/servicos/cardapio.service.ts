import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, map, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Acompanhamento,
  AtualizacaoPrato,
  CardapioDia,
  DiaSemana,
  GrupoAcompanhamentoApi,
  Prato,
  Restaurante
} from '../modelos/cardapio.model';
import { acompanhamentosMock, pratosMock, restauranteMock } from '../dados/cardapio.mock';

@Injectable({ providedIn: 'root' })
export class CardapioService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');
  private readonly pratos = signal(pratosMock);
  private readonly restaurante = signal(restauranteMock);
  private readonly cacheRestaurante = signal<Restaurante | null>(null);

  obterCardapioHoje(): Observable<CardapioDia> {
    if (environment.usarDadosMockados) {
      return of(this.obterMockPorDia(this.obterDiaAtual()));
    }

    return this.httpClient
      .get<CardapioDiaApi>(`${this.apiUrl}/cardapio/hoje`)
      .pipe(map((resposta) => this.mapearCardapio(resposta)));
  }

  obterCardapioPorDia(diaSemana: DiaSemana): Observable<CardapioDia> {
    if (environment.usarDadosMockados) {
      return of(this.obterMockPorDia(diaSemana));
    }

    return this.httpClient
      .get<CardapioDiaApi>(`${this.apiUrl}/cardapio/dia/${this.mapearDiaParaApi(diaSemana)}`)
      .pipe(map((resposta) => this.mapearCardapio(resposta)));
  }

  obterRestaurante(): Observable<Restaurante> {
    if (environment.usarDadosMockados) {
      return of(this.obterRestauranteMock());
    }

    const cacheado = this.cacheRestaurante();
    if (cacheado) {
      return of(cacheado);
    }

    return this.obterStatusRestaurante().pipe(tap((restaurante) => this.cacheRestaurante.set(restaurante)));
  }

  obterStatusRestaurante(): Observable<Restaurante> {
    if (environment.usarDadosMockados) {
      return of(this.obterRestauranteMock());
    }

    return this.httpClient
      .get<RestauranteApi>(`${this.apiUrl}/restaurante/status`)
      .pipe(map((resposta) => this.mapearRestaurante(resposta)));
  }

  listarPratosHoje(): Observable<CardapioDia> {
    return this.obterCardapioHoje();
  }

  listarAcompanhamentos(): Observable<Acompanhamento[]> {
    return of(acompanhamentosMock);
  }

  atualizarStatus(estaAberto: boolean, mensagemStatus: string): Observable<Restaurante> {
    const restaurante = { ...this.restaurante(), estaAberto, mensagemStatus };
    this.restaurante.set(restaurante);
    return of(restaurante);
  }

  alternarDisponibilidadePrato(id: string, estaDisponivel: boolean): Observable<void> {
    this.pratos.update((pratos) =>
      pratos.map((prato) => prato.id === id ? { ...prato, estaDisponivel } : prato)
    );
    return of(void 0);
  }

  atualizarPrato(id: string, dados: AtualizacaoPrato): Observable<void> {
    this.pratos.update((pratos) =>
      pratos.map((prato) => prato.id === id ? { ...prato, ...dados } : prato)
    );
    return of(void 0);
  }

  trocarImagemPrato(id: string, arquivo: File): Observable<void> {
    const urlImagem = URL.createObjectURL(arquivo);
    this.pratos.update((pratos) =>
      pratos.map((prato) => prato.id === id ? { ...prato, urlImagem } : prato)
    );
    return of(void 0);
  }

  alternarDisponibilidadeAcompanhamento(id: string, estaDisponivel: boolean): Observable<void> {
    const acompanhamento = acompanhamentosMock.find((item) => item.id === id);
    if (acompanhamento) {
      acompanhamento.estaDisponivel = estaDisponivel;
    }
    return of(void 0);
  }

  obterDiaAtual(): DiaSemana {
    return new Date().getDay() as DiaSemana;
  }

  private obterMockPorDia(diaSemana: DiaSemana): CardapioDia {
    const nomesDia = ['Domingo', 'Segunda', 'Terca', 'Quarta', 'Quinta', 'Sexta', 'Sabado'];
    return {
      diaSemana,
      nomeDia: nomesDia[diaSemana],
      pratos: diaSemana === 0
        ? []
        : this.pratos().filter((prato) => prato.diasSemana?.includes(diaSemana))
    };
  }

  private obterRestauranteMock(): Restaurante {
    if (this.obterDiaAtual() === 0) {
      return {
        ...this.restaurante(),
        estaAberto: false,
        permitirPedidos: false,
        motivoBloqueio: 'Hoje nao temos atendimento. Consulte o cardapio dos outros dias.',
        mensagemStatus: 'Hoje nao temos atendimento. Consulte o cardapio dos outros dias.'
      };
    }

    return this.restaurante();
  }

  private mapearCardapio(resposta: CardapioDiaApi): CardapioDia {
    return {
      diaSemana: this.mapearDiaDaApi(resposta.diaSemana),
      nomeDia: resposta.nomeDiaSemana,
      nomeDiaSemana: resposta.nomeDiaSemana,
      restaurante: this.mapearRestaurante(resposta.restaurante),
      pratos: resposta.pratos.map((prato) => ({
        id: prato.id,
        nome: prato.nome,
        descricao: prato.descricao,
        preco: prato.precos.pequenaDinheiroPix,
        urlImagem: prato.urlImagem ?? '',
        estaDisponivel: prato.estaDisponivel,
        ordemExibicao: prato.ordemExibicao,
        precos: prato.precos,
        grupoAcompanhamento: prato.grupoAcompanhamento
      }))
    };
  }

  private mapearRestaurante(restaurante: RestauranteApi): Restaurante {
    const whatsappConfigurado = environment.whatsappRestaurante.trim();

    return {
      nome: restaurante.nome,
      whatsapp: whatsappConfigurado || restaurante.whatsapp || '',
      instagram: restaurante.instagram ?? '@quentinhasdatininha',
      endereco: restaurante.endereco ?? 'Rua Apolinario de Santana, 129 - Engenho Velho da Federacao',
      horarioFuncionamento: restaurante.horarioFuncionamento ?? 'Segunda a sabado, das 10h as 14h',
      estaAberto: restaurante.estaAberto,
      permitirPedidos: restaurante.permitirPedidos ?? restaurante.estaAberto,
      motivoBloqueio: restaurante.motivoBloqueio,
      mensagemStatus: restaurante.mensagemStatus,
      urlLogo: restaurante.urlLogo ?? '/assets/logo-tininha.svg',
      formasPagamento: ['Dinheiro', 'PIX', 'Cartão']
    };
  }

  private mapearDiaParaApi(diaSemana: DiaSemana): number {
    return diaSemana === 0 ? 7 : diaSemana;
  }

  private mapearDiaDaApi(diaSemana: number): DiaSemana {
    return (diaSemana === 7 ? 0 : diaSemana) as DiaSemana;
  }
}

interface CardapioDiaApi {
  diaSemana: number;
  nomeDiaSemana: string;
  restaurante: RestauranteApi;
  pratos: PratoApi[];
}

interface RestauranteApi {
  nome: string;
  estaAberto: boolean;
  permitirPedidos?: boolean;
  motivoBloqueio?: string | null;
  mensagemStatus: string;
  whatsapp: string | null;
  instagram: string | null;
  endereco: string | null;
  horarioFuncionamento: string | null;
  urlLogo: string | null;
}

interface PratoApi {
  id: string;
  nome: string;
  descricao: string;
  urlImagem: string | null;
  estaDisponivel: boolean;
  ordemExibicao: number;
  precos: Prato['precos'];
  grupoAcompanhamento: GrupoAcompanhamentoApi | null;
}
