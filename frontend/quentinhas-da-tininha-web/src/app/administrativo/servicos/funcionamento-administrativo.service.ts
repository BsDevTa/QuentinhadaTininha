import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FuncionamentoAdmin, FuncionamentoAdminSalvar } from '../modelos/admin-cardapio.model';

@Injectable({ providedIn: 'root' })
export class FuncionamentoAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  obter(): Observable<FuncionamentoAdmin> {
    return this.httpClient.get<FuncionamentoAdmin>(`${this.apiUrl}/admin/funcionamento`);
  }

  atualizar(request: FuncionamentoAdminSalvar): Observable<FuncionamentoAdmin> {
    return this.httpClient.put<FuncionamentoAdmin>(`${this.apiUrl}/admin/funcionamento`, request);
  }
}
