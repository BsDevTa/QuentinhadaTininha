import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ConfiguracoesPublicasAdmin,
  ConfiguracoesPublicasAdminSalvar
} from '../modelos/admin-cardapio.model';

@Injectable({ providedIn: 'root' })
export class ConfiguracoesAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  obter(): Observable<ConfiguracoesPublicasAdmin> {
    return this.httpClient.get<ConfiguracoesPublicasAdmin>(`${this.apiUrl}/admin/configuracoes`);
  }

  atualizar(request: ConfiguracoesPublicasAdminSalvar): Observable<ConfiguracoesPublicasAdmin> {
    return this.httpClient.put<ConfiguracoesPublicasAdmin>(`${this.apiUrl}/admin/configuracoes`, request);
  }
}
