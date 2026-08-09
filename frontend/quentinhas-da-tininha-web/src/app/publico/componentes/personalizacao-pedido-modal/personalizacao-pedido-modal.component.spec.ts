import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { Prato } from '../../../compartilhado/modelos/cardapio.model';
import { CepService, ConsultaFreteCep } from '../../../compartilhado/servicos/cep.service';
import { PedidoApiService } from '../../../compartilhado/servicos/pedido-api.service';
import { PersonalizacaoPedidoModalComponent } from './personalizacao-pedido-modal.component';

describe('PersonalizacaoPedidoModalComponent entrega', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('remove o frete anterior quando o cliente troca o CEP e aplica a nova consulta', () => {
    const cepService = new CepServiceFake();
    const componente = criarComponente(cepService);

    componente.selecionarTipoEntrega('entrega');
    componente.atualizarCep('40221-005');
    cepService.responder('40221005', criarRespostaCep({
      cep: '40221-005',
      logradouro: 'Rua A',
      bairro: 'Alto das Pombas',
      bairroFrete: 'Calabar',
      valorFrete: 3
    }));

    expect(componente.freteAtendido()).toBe(true);
    expect(componente.valorFrete()).toBe(3);

    componente.atualizarCep('40222-000');

    expect(componente.freteAtendido()).toBe(false);
    expect(componente.valorFrete()).toBeNull();
    expect(componente.logradouro()).toBe('');
    expect(componente.bairro()).toBe('');
    expect(cepService.consultas).toEqual(['40221005', '40222000']);

    cepService.responder('40222000', criarRespostaCep({
      cep: '40222-000',
      logradouro: 'Rua B',
      bairro: 'Graça',
      bairroFrete: 'Graca',
      valorFrete: 5
    }));

    expect(componente.freteAtendido()).toBe(true);
    expect(componente.valorFrete()).toBe(5);
    expect(componente.bairro()).toBe('Graça');
  });

  it('remove frete e obrigacao de endereco quando muda para retirada', () => {
    const cepService = new CepServiceFake();
    const componente = criarComponente(cepService);

    componente.selecionarTipoEntrega('entrega');
    componente.atualizarCep('40221-005');
    cepService.responder('40221005', criarRespostaCep({
      cep: '40221-005',
      logradouro: 'Rua A',
      bairro: 'Alto das Pombas',
      bairroFrete: 'Calabar',
      valorFrete: 3
    }));

    expect(componente.total()).toBe(23);

    componente.selecionarTipoEntrega('retirada');

    expect(componente.freteAtendido()).toBe(false);
    expect(componente.valorFrete()).toBeNull();
    expect(componente.personalizacao().valorFrete).toBeNull();
    expect(componente.entregaInvalida()).toBe(false);
    expect(componente.total()).toBe(20);
  });

  it('permite finalizar quando restaurante esta liberado e dados obrigatorios estao preenchidos', () => {
    const cepService = new CepServiceFake();
    const componente = criarComponente(cepService);

    componente.restauranteAberto = true;
    componente.dataPedido = '2026-08-09';
    componente.nomeCliente.set('Cliente Teste');
    componente.telefoneCliente.set('(71) 99999-9999');

    expect(componente.podeFinalizarPedido()).toBe(true);
  });
});

function criarComponente(cepService: CepServiceFake): any {
  TestBed.configureTestingModule({
    providers: [
      { provide: CepService, useValue: cepService },
      { provide: PedidoApiService, useValue: {} }
    ]
  });

  const componente = TestBed.runInInjectionContext(() =>
    new PersonalizacaoPedidoModalComponent());

  componente.prato = criarPrato();
  componente.whatsappRestaurante = '5571982189319';
  componente.restauranteAberto = true;
  componente.dataPedido = '2026-08-08';

  return componente as any;
}

function criarPrato(): Prato {
  return {
    id: 'prato-1',
    nome: 'Bife ao molho',
    descricao: 'Bife com molho caseiro.',
    preco: 20,
    urlImagem: '',
    estaDisponivel: true,
    precos: {
      pequenaDinheiroPix: 20,
      pequenaCartao: 21,
      grandeDinheiroPix: 24,
      grandeCartao: 25
    },
    grupoAcompanhamento: {
      codigo: 'PADRAO',
      nome: 'Acompanhamentos',
      acompanhamentos: []
    }
  };
}

function criarRespostaCep(
  sobrescrita: Partial<ConsultaFreteCep>): ConsultaFreteCep {
  return {
    cep: '40221-005',
    logradouro: null,
    bairro: 'Alto das Pombas',
    cidade: 'Salvador',
    estado: 'BA',
    bairroFrete: null,
    atendido: true,
    valorFrete: 3,
    mensagem: null,
    ...sobrescrita
  };
}

class CepServiceFake {
  readonly consultas: string[] = [];
  private readonly pendentes = new Map<string, Subject<ConsultaFreteCep>>();

  consultarFretePorCep(cep: string) {
    this.consultas.push(cep);
    const resposta = new Subject<ConsultaFreteCep>();
    this.pendentes.set(cep, resposta);
    return resposta.asObservable();
  }

  responder(cep: string, resposta: ConsultaFreteCep): void {
    const pendente = this.pendentes.get(cep);
    if (!pendente) {
      throw new Error(`Consulta pendente nao encontrada para o CEP ${cep}.`);
    }

    pendente.next(resposta);
    pendente.complete();
  }
}
