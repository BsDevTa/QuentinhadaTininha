export type StatusImpressaoPedido = 'aguardando' | 'imprimindo' | 'impresso' | 'erro';

export interface PedidoImpressao {
  id: string;
  dataPedido?: string | null;
  nomeCliente: string;
  telefoneCliente?: string | null;
  valorSubtotal: number;
  valorFrete?: number | null;
  valorTotal: number;
  formaPagamento: number | string;
  precisaTroco?: boolean;
  valorTroco?: number | null;
  tipoEntrega: number | string;
  cep?: string | null;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  enderecoEntrega?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  estado?: string | null;
  referencia?: string | null;
  observacao?: string | null;
  itens: PedidoItemImpressao[];
  criadoEm: string;
}

export interface PedidoItemImpressao {
  id: string;
  nomePrato: string;
  quantidade?: number | null;
  tamanho?: number | string | null;
  acompanhamentos?: string | string[] | null;
  valorUnitario: number;
  observacao?: string | null;
}
