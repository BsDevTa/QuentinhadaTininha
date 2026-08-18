import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PedidoCriacaoRequisicao {
  dataPedido: string;
  nomeCliente: string;
  telefoneCliente?: string | null;
  valorSubtotal: number;
  valorFrete?: number | null;
  valorTotal: number;
  formaPagamento: 1 | 2 | 3;
  precisaTroco: boolean;
  valorTroco?: number | null;
  tipoEntrega: 1 | 2;
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
  itens: PedidoItemCriacaoRequisicao[];
  bebidas?: PedidoBebidaCriacaoRequisicao[];
}

export interface PedidoItemCriacaoRequisicao {
  pratoId: string;
  tamanho: 1 | 2;
  acompanhamentoIds: string[];
  observacao?: string | null;
}

export interface PedidoBebidaCriacaoRequisicao {
  bebidaId: string;
  quantidade: number;
}

export interface PedidoResposta {
  id: string;
  dataPedido: string;
  nomeCliente: string;
  telefoneCliente?: string | null;
  valorSubtotal: number;
  valorFrete?: number | null;
  valorTotal: number;
  formaPagamento: string | number;
  precisaTroco: boolean;
  valorTroco?: number | null;
  tipoEntrega: string | number;
  criadoEm: string;
}

@Injectable({ providedIn: 'root' })
export class PedidoApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  criar(requisicao: PedidoCriacaoRequisicao): Observable<PedidoResposta> {
    return this.httpClient.post<PedidoResposta>(`${this.apiUrl}/pedidos`, requisicao);
  }
}
