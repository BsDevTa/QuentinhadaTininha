export type DiaSemana = 0 | 1 | 2 | 3 | 4 | 5 | 6;
export type TamanhoRefeicao = 'P' | 'G';
export type FormaPagamento = 'dinheiro' | 'pix' | 'cartao';
export type TipoEntrega = 'retirada' | 'entrega';
export type TipoSelecaoAcompanhamento = 'MULTIPLA' | 'EXCLUSIVA';
export type TipoGrupoAcompanhamento =
  | 'padrao'
  | 'comida_baiana'
  | 'cozido'
  | 'sarapatel_xinxim'
  | 'arrumadinho';

export interface PrecosPrato {
  pequenaDinheiroPix: number;
  pequenaCartao: number;
  grandeDinheiroPix: number;
  grandeCartao: number;
}

export interface Acompanhamento {
  id: string;
  nome: string;
  estaDisponivel: boolean;
  tipoSelecao?: TipoSelecaoAcompanhamento;
  grupoExclusivo?: string | null;
  obrigatorio?: boolean;
  ordemExibicao?: number;
}

export interface Prato {
  id: string;
  nome: string;
  descricao: string;
  preco: number;
  urlImagem: string;
  estaDisponivel: boolean;
  ordemExibicao?: number;
  diasSemana?: DiaSemana[];
  tipoGrupoAcompanhamento?: TipoGrupoAcompanhamento;
  precos: PrecosPrato;
  grupoAcompanhamento?: GrupoAcompanhamentoApi | null;
}

export interface GrupoAcompanhamento {
  tipo?: TipoGrupoAcompanhamento;
  titulo: string;
  tipoFeijao: Acompanhamento[];
  itens: Acompanhamento[];
}

export interface GrupoAcompanhamentoApi {
  codigo: string;
  nome: string;
  acompanhamentos: Acompanhamento[];
}

export interface PersonalizacaoPedido {
  pratoId: string;
  tamanho: TamanhoRefeicao;
  formaPagamento: FormaPagamento;
  acompanhamentoIds: string[];
  tipoFeijaoId: string | null;
  observacao: string | null;
  precisaTroco: boolean;
  valorTroco: number | null;
  tipoEntrega: TipoEntrega;
  cep: string | null;
  logradouro: string | null;
  numero: string | null;
  complemento: string | null;
  enderecoEntrega: string | null;
  bairro: string | null;
  cidade: string | null;
  estado: string | null;
  referencia: string | null;
  valorFrete: number | null;
}

export interface CardapioDia {
  diaSemana: DiaSemana;
  nomeDia: string;
  nomeDiaSemana?: string;
  restaurante?: Restaurante;
  pratos: Prato[];
}

export interface Restaurante {
  nome: string;
  whatsapp: string;
  instagram: string;
  endereco: string;
  horarioFuncionamento: string;
  estaAberto: boolean;
  permitirPedidos: boolean;
  motivoBloqueio?: string | null;
  mensagemStatus: string;
  urlLogo: string;
  formasPagamento: string[];
}

export type RestauranteStatus = Restaurante;

export interface UsuarioLogin {
  email: string;
  senha: string;
}

export interface RespostaAutenticacao {
  token: string;
  nome: string;
  email: string;
}

export interface AtualizacaoPrato {
  nome: string;
  descricao: string;
  preco: number;
}
