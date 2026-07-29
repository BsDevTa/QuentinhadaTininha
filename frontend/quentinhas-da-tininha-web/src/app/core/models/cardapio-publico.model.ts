export interface CardapioPublicoResposta {
  restaurante: RestaurantePublicoResposta;
  data: string;
  diaSemana: DiaSemana;
  aberto: boolean;
  motivoFechamento: string | null;
  mensagem: string | null;
  horarios: HorarioFuncionamentoPublicoResposta[];
  categorias: CategoriaCardapioPublicoResposta[];
}

export interface RestaurantePublicoResposta {
  nome: string;
  descricao: string | null;
  urlLogotipo: string | null;
  urlImagemCapa: string | null;
  telefone: string | null;
  whatsapp: string | null;
  endereco: string | null;
  cidade: string | null;
  estado: string | null;
  cep: string | null;
}

export interface HorarioFuncionamentoPublicoResposta {
  horaAbertura: string;
  horaFechamento: string;
}

export interface CategoriaCardapioPublicoResposta {
  id: string;
  nome: string;
  descricao: string | null;
  pratos: PratoCardapioPublicoResposta[];
}

export interface PratoCardapioPublicoResposta {
  id: string;
  nome: string;
  descricao: string | null;
  preco: number;
  imagemUrl: string | null;
  acompanhamentos: AcompanhamentoCardapioPublicoResposta[];
}

export interface AcompanhamentoCardapioPublicoResposta {
  id: string;
  nome: string;
  descricao: string | null;
  precoAdicional: number;
}

export type DiaSemana = 0 | 1 | 2 | 3 | 4 | 5 | 6;
