import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CardapioService } from './cardapio.service';

describe('CardapioService', () => {
  let http: HttpTestingController;
  let service: CardapioService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(CardapioService);
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('compartilha a mesma requisicao do cardapio de hoje enquanto o cache esta valido', () => {
    const respostas: string[] = [];

    service.obterCardapioHoje().subscribe((cardapio) => respostas.push(cardapio.nomeDiaSemana ?? ''));
    service.obterCardapioHoje().subscribe((cardapio) => respostas.push(cardapio.nomeDiaSemana ?? ''));

    const req = http.expectOne(`${environment.apiUrl}/cardapio/hoje`);
    req.flush(criarRespostaCardapio());

    http.expectNone(`${environment.apiUrl}/cardapio/hoje`);
    expect(respostas).toEqual(['Segunda-feira', 'Segunda-feira']);
  });

  it('remove o cache quando a requisicao falha', () => {
    service.obterCardapioHoje().subscribe({
      error: () => undefined
    });

    http.expectOne(`${environment.apiUrl}/cardapio/hoje`).flush(
      { mensagem: 'erro' },
      { status: 500, statusText: 'Server Error' }
    );

    service.obterCardapioHoje().subscribe({
      error: () => undefined
    });

    http.expectOne(`${environment.apiUrl}/cardapio/hoje`).flush(
      { mensagem: 'erro' },
      { status: 500, statusText: 'Server Error' }
    );
  });
});

function criarRespostaCardapio() {
  return {
    diaSemana: 1,
    nomeDiaSemana: 'Segunda-feira',
    restaurante: {
      nome: 'Quentinhas da Tininha',
      estaAberto: true,
      permitirPedidos: true,
      motivoBloqueio: null,
      mensagemStatus: 'Estamos atendendo hoje.',
      whatsapp: '5571982189319',
      instagram: '@quentinhasdatininha',
      endereco: 'Rua Apolinario de Santana',
      horarioFuncionamento: 'Segunda a sabado, das 10h as 14h',
      urlLogo: '/assets/logo-tininha.svg'
    },
    pratos: [
      {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'Bife ao molho',
        descricao: 'Bife com molho caseiro.',
        urlImagem: null,
        estaDisponivel: true,
        ordemExibicao: 1,
        precos: {
          pequenaDinheiroPix: 17,
          pequenaCartao: 18,
          grandeDinheiroPix: 21,
          grandeCartao: 22
        },
        grupoAcompanhamento: {
          codigo: 'PADRAO',
          nome: 'Acompanhamentos',
          acompanhamentos: []
        }
      }
    ]
  };
}
