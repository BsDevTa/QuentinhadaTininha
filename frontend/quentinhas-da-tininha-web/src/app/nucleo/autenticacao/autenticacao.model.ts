export interface CredenciaisLogin {
  email: string;
  senha: string;
}

export interface UsuarioAutenticado {
  id: string;
  nome: string;
  email: string;
}

export interface RespostaAutenticacao {
  token: string;
  tipoToken: string;
  expiraEm: string;
  usuario: UsuarioAutenticado;
}

export interface SessaoUsuario {
  autenticado: boolean;
  usuario: UsuarioAutenticado;
}

export interface ResumoPainel {
  restauranteAberto: boolean;
  mensagemStatus: string;
  quantidadePratosHoje: number;
  quantidadePratosDisponiveis: number;
  quantidadePratosIndisponiveis: number;
  quantidadeAcompanhamentosIndisponiveis: number;
  diaSemana: number;
  nomeDiaSemana: string;
}
