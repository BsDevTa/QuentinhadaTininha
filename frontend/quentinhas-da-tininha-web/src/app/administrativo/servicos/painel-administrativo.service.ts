import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ResumoPainel } from '../../nucleo/autenticacao/autenticacao.model';

@Injectable({ providedIn: 'root' })
export class PainelAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  obterResumo(): Observable<ResumoPainel> {
    return this.httpClient.get<ResumoPainel>(`${this.apiUrl}/admin/painel/resumo`);
  }
}
