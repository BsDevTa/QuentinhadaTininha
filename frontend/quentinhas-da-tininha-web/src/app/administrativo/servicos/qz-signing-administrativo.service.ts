import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface QzAssinaturaResposta {
  assinatura: string;
}

@Injectable({ providedIn: 'root' })
export class QzSigningAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  obterCertificado(): Observable<string> {
    return this.httpClient.get(`${this.apiUrl}/admin/qz/certificado`, {
      responseType: 'text'
    });
  }

  assinar(dados: string): Observable<QzAssinaturaResposta> {
    return this.httpClient.post<QzAssinaturaResposta>(
      `${this.apiUrl}/admin/qz/assinar`,
      { dados }
    );
  }
}
