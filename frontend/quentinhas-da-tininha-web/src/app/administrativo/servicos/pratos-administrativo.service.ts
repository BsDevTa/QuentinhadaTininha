import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FiltrosPratosAdmin,
  GrupoAcompanhamentoAdmin,
  PratoAdminDetalhe,
  PratoAdminSalvar,
  PratoAdminResumo,
  StatusAdmin
} from '../modelos/admin-cardapio.model';

@Injectable({ providedIn: 'root' })
export class PratosAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  listar(filtros: FiltrosPratosAdmin): Observable<PratoAdminResumo[]> {
    return this.httpClient.get<PratoAdminResumo[]>(`${this.apiUrl}/admin/pratos`, {
      params: this.criarParametros(filtros)
    });
  }

  obterPorId(id: string): Observable<PratoAdminDetalhe> {
    return this.httpClient.get<PratoAdminDetalhe>(`${this.apiUrl}/admin/pratos/${id}`);
  }

  criar(request: PratoAdminSalvar): Observable<PratoAdminDetalhe> {
    return this.httpClient.post<PratoAdminDetalhe>(`${this.apiUrl}/admin/pratos`, request);
  }

  atualizar(id: string, request: PratoAdminSalvar): Observable<PratoAdminDetalhe> {
    return this.httpClient.put<PratoAdminDetalhe>(`${this.apiUrl}/admin/pratos/${id}`, request);
  }

  alterarDisponibilidade(id: string, estaDisponivel: boolean): Observable<StatusAdmin> {
    return this.httpClient.patch<StatusAdmin>(`${this.apiUrl}/admin/pratos/${id}/disponibilidade`, { estaDisponivel });
  }

  alterarAtivacao(id: string, estaAtivo: boolean): Observable<StatusAdmin> {
    return this.httpClient.patch<StatusAdmin>(`${this.apiUrl}/admin/pratos/${id}/ativacao`, { estaAtivo });
  }

  listarGruposAcompanhamento(): Observable<GrupoAcompanhamentoAdmin[]> {
    return this.httpClient.get<GrupoAcompanhamentoAdmin[]>(`${this.apiUrl}/admin/grupos-acompanhamento`);
  }

  private criarParametros(filtros: FiltrosPratosAdmin): HttpParams {
    let params = new HttpParams();
    Object.entries(filtros).forEach(([chave, valor]) => {
      if (valor !== undefined && valor !== null && valor !== '') {
        params = params.set(chave, String(valor));
      }
    });
    return params;
  }
}
