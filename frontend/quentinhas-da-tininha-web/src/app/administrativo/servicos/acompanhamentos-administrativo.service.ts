import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AcompanhamentoAdmin,
  AcompanhamentoAdminSalvar,
  FiltrosAcompanhamentosAdmin,
  StatusAdmin
} from '../modelos/admin-cardapio.model';

@Injectable({ providedIn: 'root' })
export class AcompanhamentosAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  listar(filtros: FiltrosAcompanhamentosAdmin): Observable<AcompanhamentoAdmin[]> {
    return this.httpClient.get<AcompanhamentoAdmin[]>(`${this.apiUrl}/admin/acompanhamentos`, {
      params: this.criarParametros(filtros)
    });
  }

  obterPorId(id: string): Observable<AcompanhamentoAdmin> {
    return this.httpClient.get<AcompanhamentoAdmin>(`${this.apiUrl}/admin/acompanhamentos/${id}`);
  }

  criar(request: AcompanhamentoAdminSalvar): Observable<AcompanhamentoAdmin> {
    return this.httpClient.post<AcompanhamentoAdmin>(`${this.apiUrl}/admin/acompanhamentos`, request);
  }

  atualizar(id: string, request: AcompanhamentoAdminSalvar): Observable<AcompanhamentoAdmin> {
    return this.httpClient.put<AcompanhamentoAdmin>(`${this.apiUrl}/admin/acompanhamentos/${id}`, request);
  }

  alterarDisponibilidade(id: string, estaDisponivel: boolean): Observable<StatusAdmin> {
    return this.httpClient.patch<StatusAdmin>(`${this.apiUrl}/admin/acompanhamentos/${id}/disponibilidade`, { estaDisponivel });
  }

  alterarAtivacao(id: string, estaAtivo: boolean): Observable<StatusAdmin> {
    return this.httpClient.patch<StatusAdmin>(`${this.apiUrl}/admin/acompanhamentos/${id}/ativacao`, { estaAtivo });
  }

  private criarParametros(filtros: FiltrosAcompanhamentosAdmin): HttpParams {
    let params = new HttpParams();
    Object.entries(filtros).forEach(([chave, valor]) => {
      if (valor !== undefined && valor !== null && valor !== '') {
        params = params.set(chave, String(valor));
      }
    });
    return params;
  }
}
