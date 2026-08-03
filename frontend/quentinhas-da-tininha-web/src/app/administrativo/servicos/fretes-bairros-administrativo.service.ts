import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FiltrosFretesBairrosAdmin,
  FreteBairroAdmin,
  FreteBairroAdminSalvar
} from '../modelos/admin-cardapio.model';

@Injectable({ providedIn: 'root' })
export class FretesBairrosAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  listar(filtros: FiltrosFretesBairrosAdmin): Observable<FreteBairroAdmin[]> {
    return this.httpClient.get<FreteBairroAdmin[]>(`${this.apiUrl}/admin/fretes-bairros`, {
      params: this.criarParametros(filtros)
    });
  }

  obterPorId(id: string): Observable<FreteBairroAdmin> {
    return this.httpClient.get<FreteBairroAdmin>(`${this.apiUrl}/admin/fretes-bairros/${id}`);
  }

  criar(request: FreteBairroAdminSalvar): Observable<FreteBairroAdmin> {
    return this.httpClient.post<FreteBairroAdmin>(`${this.apiUrl}/admin/fretes-bairros`, request);
  }

  atualizar(id: string, request: FreteBairroAdminSalvar): Observable<FreteBairroAdmin> {
    return this.httpClient.put<FreteBairroAdmin>(`${this.apiUrl}/admin/fretes-bairros/${id}`, request);
  }

  alterarStatus(id: string, ativo: boolean): Observable<FreteBairroAdmin> {
    return this.httpClient.patch<FreteBairroAdmin>(`${this.apiUrl}/admin/fretes-bairros/${id}/status`, { ativo });
  }

  excluir(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.apiUrl}/admin/fretes-bairros/${id}`);
  }

  private criarParametros(filtros: FiltrosFretesBairrosAdmin): HttpParams {
    let params = new HttpParams();
    Object.entries(filtros).forEach(([chave, valor]) => {
      if (valor !== undefined && valor !== null && valor !== '') {
        params = params.set(chave, String(valor));
      }
    });
    return params;
  }
}
