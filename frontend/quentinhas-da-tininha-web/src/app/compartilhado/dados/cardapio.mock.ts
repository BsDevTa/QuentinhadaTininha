import {
  Acompanhamento,
  DiaSemana,
  PrecosPrato,
  Prato,
  Restaurante,
  TipoGrupoAcompanhamento
} from '../modelos/cardapio.model';

export const restauranteMock: Restaurante = {
  nome: 'Quentinhas da Tininha',
  whatsapp: '5571982189319',
  instagram: '@quentinhasdatininha',
  endereco: 'Rua Apolinario de Santana, 129 - Engenho Velho da Federacao',
  horarioFuncionamento: 'Segunda a sábado, das 10h às 14h',
  estaAberto: true,
  permitirPedidos: true,
  motivoBloqueio: null,
  mensagemStatus: 'Atendimento aberto para pedidos de hoje. Chama no WhatsApp e combina sua quentinha.',
  urlLogo: '/assets/logo-tininha.svg',
  formasPagamento: ['Dinheiro', 'PIX', 'Cartão']
};

export const acompanhamentosMock: Acompanhamento[] = [
  { id: 'feijao-caldo', nome: 'Feijão de caldo', estaDisponivel: true },
  { id: 'feijao-tropeiro', nome: 'Feijão tropeiro', estaDisponivel: true },
  { id: 'arroz', nome: 'Arroz', estaDisponivel: true },
  { id: 'macarrao', nome: 'Macarrão', estaDisponivel: true },
  { id: 'salada', nome: 'Salada', estaDisponivel: true },
  { id: 'feijao-fradinho', nome: 'Feijão fradinho', estaDisponivel: true },
  { id: 'caruru', nome: 'Caruru', estaDisponivel: true },
  { id: 'vatapa', nome: 'Vatapá', estaDisponivel: true },
  { id: 'farofa', nome: 'Farofa', estaDisponivel: true },
  { id: 'pirao', nome: 'Pirão', estaDisponivel: true },
  { id: 'salada-vinagrete', nome: 'Salada vinagrete', estaDisponivel: true }
];

export const precosPadrao: PrecosPrato = {
  pequenaDinheiroPix: 17,
  pequenaCartao: 18,
  grandeDinheiroPix: 21,
  grandeCartao: 22
};

export const precosIntermediario: PrecosPrato = {
  pequenaDinheiroPix: 19,
  pequenaCartao: 20,
  grandeDinheiroPix: 23,
  grandeCartao: 24
};

const precosParmegiana: PrecosPrato = {
  pequenaDinheiroPix: 20,
  pequenaCartao: 21,
  grandeDinheiroPix: 24,
  grandeCartao: 25
};

const precosPeixe: PrecosPrato = {
  pequenaDinheiroPix: 25,
  pequenaCartao: 26,
  grandeDinheiroPix: 45,
  grandeCartao: 46
};

const precosCozido: PrecosPrato = {
  pequenaDinheiroPix: 20,
  pequenaCartao: 21,
  grandeDinheiroPix: 36,
  grandeCartao: 37
};

const precosBaianaXinxim: PrecosPrato = {
  pequenaDinheiroPix: 20,
  pequenaCartao: 21,
  grandeDinheiroPix: 37,
  grandeCartao: 38
};

const imagemPorPrato: Record<'bisteca' | 'frango' | 'omelete' | 'carne' | 'peixe' | 'feijoada', string> = {
  bisteca: '/assets/pratos/bisteca.svg',
  frango: '/assets/pratos/frango-assado.svg',
  omelete: '/assets/pratos/omelete-frango.svg',
  carne: '/assets/pratos/carne-sol.svg',
  peixe: '/assets/pratos/peixe-frito.svg',
  feijoada: '/assets/pratos/feijoada.svg'
};

function prato(
  id: string,
  nome: string,
  descricao: string,
  diasSemana: DiaSemana[],
  tipoGrupoAcompanhamento: TipoGrupoAcompanhamento,
  precos: PrecosPrato,
  imagem: string,
  estaDisponivel = true
): Prato {
  return {
    id,
    nome,
    descricao,
    preco: precos.pequenaDinheiroPix,
    urlImagem: imagem,
    estaDisponivel,
    diasSemana,
    tipoGrupoAcompanhamento,
    precos
  };
}

export const pratosMock: Prato[] = [
  prato('omelete-frango', 'Omelete de frango', 'Omelete recheado com frango desfiado e tempero caseiro.', [1], 'padrao', precosPadrao, imagemPorPrato.omelete),
  prato('bisteca', 'Bisteca', 'Bisteca suína grelhada, suculenta e bem temperada.', [1, 2, 3, 4, 5, 6], 'padrao', precosPadrao, imagemPorPrato.bisteca),
  prato('frango-milanesa', 'Frango à milanesa', 'Filé de frango empanado, crocante por fora e macio por dentro.', [1, 2, 3, 4, 5, 6], 'padrao', precosPadrao, imagemPorPrato.frango),
  prato('ensopado-boi', 'Ensopado de boi', 'Carne bovina cozida lentamente com molho caseiro encorpado.', [1], 'padrao', precosPadrao, imagemPorPrato.carne),
  prato('frango-grelhado', 'Frango grelhado', 'Filé de frango grelhado, leve e bem temperado.', [1, 3, 5, 6], 'padrao', precosPadrao, imagemPorPrato.peixe),
  prato('coxinha-asa-toscana', 'Coxinha da asa + Toscana', 'Coxinha da asa assada com linguiça toscana saborosa.', [1], 'padrao', precosIntermediario, imagemPorPrato.feijoada),

  prato('ensopado-frango', 'Ensopado de frango', 'Frango cozido em molho caseiro com tempero marcante.', [2], 'padrao', precosPadrao, imagemPorPrato.frango),
  prato('peixe-frito', 'Peixe frito', 'Peixe sequinho, crocante e temperado no ponto.', [2], 'padrao', precosPeixe, imagemPorPrato.peixe),
  prato('figado-pure', 'Fígado ao molho com purê de batata', 'Fígado macio ao molho, servido com purê cremoso.', [2], 'padrao', precosPadrao, imagemPorPrato.carne),
  prato('frango-parmegiana', 'Frango à parmegiana', 'Filé de frango empanado com molho e queijo derretido.', [2, 4], 'padrao', precosParmegiana, imagemPorPrato.frango),
  prato('carne-panela', 'Carne de panela', 'Carne cozida lentamente, macia e cheia de sabor.', [2], 'padrao', precosIntermediario, imagemPorPrato.carne),

  prato('bife-molho', 'Bife ao molho', 'Bife macio com molho caseiro bem temperado.', [3], 'padrao', precosIntermediario, imagemPorPrato.carne),
  prato('estrogonofe-frango', 'Estrogonofe de frango', 'Frango cremoso com molho suave e gostinho caseiro.', [3], 'padrao', precosIntermediario, imagemPorPrato.frango),
  prato('quiabada', 'Quiabada', 'Quiabada tradicional com tempero baiano e sabor marcante.', [3], 'padrao', precosPadrao, imagemPorPrato.feijoada),

  prato('cozido', 'Cozido', 'Cozido completo, farto e preparado com caldo encorpado.', [4], 'cozido', precosCozido, imagemPorPrato.carne),
  prato('bife-acebolado', 'Bife acebolado', 'Bife grelhado com cebola dourada e tempero caseiro.', [4], 'padrao', precosIntermediario, imagemPorPrato.carne),
  prato('isca-carne-molho', 'Isca de carne ao molho', 'Iscas de carne macias em molho caseiro.', [4], 'padrao', precosIntermediario, imagemPorPrato.carne),

  prato('baiana-peixe-frito', 'Comida baiana com peixe frito', 'Prato baiano com peixe frito e temperos tradicionais.', [5], 'comida_baiana', precosPeixe, imagemPorPrato.peixe),
  prato('baiana-xinxim-frango', 'Comida baiana com xinxim de frango', 'Xinxim de frango cremoso com sabor baiano.', [5], 'comida_baiana', precosBaianaXinxim, imagemPorPrato.frango),
  prato('baiana-moqueca-peixe', 'Comida baiana com moqueca de peixe', 'Moqueca de peixe com molho aromático e tempero baiano.', [5], 'comida_baiana', precosPeixe, imagemPorPrato.peixe),
  prato('isca-figado-acebolado', 'Isca de fígado acebolado', 'Iscas de fígado aceboladas e bem temperadas.', [5], 'padrao', precosPadrao, imagemPorPrato.carne),

  prato('arrumadinho-misto', 'Arrumadinho misto', 'Arrumadinho caprichado com mistura saborosa e tempero caseiro.', [6], 'arrumadinho', precosPadrao, imagemPorPrato.feijoada),
  prato('xinxim-bofe', 'Xinxim de bofe', 'Xinxim de bofe tradicional, temperado e marcante.', [6], 'sarapatel_xinxim', precosIntermediario, imagemPorPrato.feijoada),
  prato('sarapatel', 'Sarapatel', 'Sarapatel tradicional com tempero forte e caseiro.', [6], 'sarapatel_xinxim', precosIntermediario, imagemPorPrato.feijoada, false)
];
