import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ImpressaoPedidoPendente } from '../../nucleo/modelos/pedido-impressao.model';

@Injectable({ providedIn: 'root' })
export class ImpressoesPedidosAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  listarPendentes(limite = 10): Observable<ImpressaoPedidoPendente[]> {
    return this.httpClient.get<ImpressaoPedidoPendente[]>(
      `${this.apiUrl}/admin/impressoes-pedidos/pendentes`,
      { params: { limite } }
    );
  }

  iniciar(id: string): Observable<ImpressaoPedidoPendente> {
    return this.httpClient.post<ImpressaoPedidoPendente>(
      `${this.apiUrl}/admin/impressoes-pedidos/${id}/iniciar`,
      {}
    );
  }

  concluir(id: string): Observable<void> {
    return this.httpClient.post<void>(
      `${this.apiUrl}/admin/impressoes-pedidos/${id}/concluir`,
      {}
    );
  }

  registrarErro(id: string, erro: string): Observable<void> {
    return this.httpClient.post<void>(
      `${this.apiUrl}/admin/impressoes-pedidos/${id}/erro`,
      { erro }
    );
  }

  reimprimir(pedidoId: string): Observable<ImpressaoPedidoPendente> {
    return this.httpClient.post<ImpressaoPedidoPendente>(
      `${this.apiUrl}/admin/impressoes-pedidos/pedidos/${pedidoId}/reimprimir`,
      {}
    );
  }
}
