export interface GrupoAcompanhamentoAdmin {
  id: string;
  nome: string;
  codigo: string;
  estaAtivo?: boolean;
  quantidadeAcompanhamentos?: number;
}

export interface PrecosPratoAdmin {
  pequenaDinheiroPix: number;
  pequenaCartao: number;
  grandeDinheiroPix: number;
  grandeCartao: number;
}

export interface DiaPratoAdmin {
  diaSemana: number;
  ordemExibicao: number;
  estaAtivo: boolean;
}

export interface PratoAdminResumo {
  id: string;
  nome: string;
  descricao: string | null;
  urlImagem: string | null;
  estaAtivo: boolean;
  estaDisponivel: boolean;
  ordemExibicao: number;
  grupoAcompanhamento: GrupoAcompanhamentoAdmin | null;
  diasSemana: number[];
  precos: PrecosPratoAdmin;
  dataAtualizacao: string;
}

export interface PratoAdminDetalhe extends Omit<PratoAdminResumo, 'diasSemana' | 'grupoAcompanhamento'> {
  grupoAcompanhamentoId: string | null;
  diasSemana: DiaPratoAdmin[];
}

export interface PratoAdminSalvar {
  nome: string;
  descricao: string | null;
  urlImagem: string | null;
  estaAtivo: boolean;
  estaDisponivel: boolean;
  grupoAcompanhamentoId: string;
  precos: PrecosPratoAdmin;
  diasSemana: DiaPratoAdmin[];
}

export interface FiltrosPratosAdmin {
  nome?: string;
  diaSemana?: number | '';
  estaDisponivel?: boolean | '';
  estaAtivo?: boolean | '';
  grupoAcompanhamentoCodigo?: string;
}

export interface GrupoAcompanhamentoVinculoAdmin {
  grupoAcompanhamentoId: string;
  nome?: string;
  codigo?: string;
  obrigatorio: boolean;
  ordemExibicao: number;
}

export interface AcompanhamentoAdmin {
  id: string;
  nome: string;
  estaAtivo: boolean;
  estaDisponivel: boolean;
  tipoSelecao: TipoSelecaoAcompanhamentoAdmin;
  grupoExclusivo: string | null;
  grupos: GrupoAcompanhamentoVinculoAdmin[];
  dataAtualizacao: string;
}

export interface AcompanhamentoAdminSalvar {
  nome: string;
  estaAtivo: boolean;
  estaDisponivel: boolean;
  tipoSelecao: TipoSelecaoAcompanhamentoAdmin;
  grupoExclusivo: string | null;
  grupos: GrupoAcompanhamentoVinculoAdmin[];
}

export interface FiltrosAcompanhamentosAdmin {
  nome?: string;
  estaDisponivel?: boolean | '';
  estaAtivo?: boolean | '';
  grupoAcompanhamentoId?: string;
}

export type TipoSelecaoAcompanhamentoAdmin = 'MULTIPLA' | 'EXCLUSIVA';

export interface FuncionamentoAdmin {
  estaAberto: boolean;
  mensagemStatus: string;
  horarioFuncionamento: string;
  aberturaManual: boolean;
  horarioAutomatico: string;
  proximaAbertura: string;
  fechamentoAutomatico: string;
  dataUltimaAlteracao: string;
}

export interface FuncionamentoAdminSalvar {
  estaAberto: boolean;
  mensagemStatus: string;
  horarioFuncionamento: string;
}

export interface DisponibilidadeDataAdmin {
  data: string;
  status: string;
  liberado: boolean;
  bloqueado: boolean;
  permitirPedidos: boolean;
  motivo: string | null;
}

export interface DisponibilidadeDataMotivoAdmin {
  motivo: string | null;
}

export interface ConfiguracoesPublicasAdmin {
  nomeRestaurante: string;
  whatsapp: string;
  instagram: string | null;
  endereco: string | null;
  urlLogo: string | null;
  textoRodape: string | null;
  dataUltimaAlteracao: string;
}

export interface ConfiguracoesPublicasAdminSalvar {
  nomeRestaurante: string;
  whatsapp: string;
  instagram: string | null;
  endereco: string | null;
  urlLogo: string | null;
  textoRodape: string | null;
}

export interface StatusAdmin {
  id: string;
  estaDisponivel: boolean;
  estaAtivo: boolean;
}

export interface FreteBairroAdmin {
  id: string;
  bairro: string;
  valor: number;
  ativo: boolean;
  criadoEm: string;
  atualizadoEm: string;
}

export interface FreteBairroAdminSalvar {
  bairro: string;
  valor: number;
  ativo: boolean;
}

export interface FiltrosFretesBairrosAdmin {
  bairro?: string;
  ativo?: boolean | '';
}
