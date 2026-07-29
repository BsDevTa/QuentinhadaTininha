import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  DisponibilidadeDataAdmin,
  DisponibilidadeDataMotivoAdmin,
  FuncionamentoAdmin,
  FuncionamentoAdminSalvar
} from '../modelos/admin-cardapio.model';

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

  listarDisponibilidade(dataInicial: string, dataFinal: string): Observable<DisponibilidadeDataAdmin[]> {
    const params = new HttpParams()
      .set('dataInicial', dataInicial)
      .set('dataFinal', dataFinal);

    return this.httpClient.get<DisponibilidadeDataAdmin[]>(`${this.apiUrl}/admin/disponibilidade`, { params });
  }

  liberarData(data: string, motivo: string | null): Observable<DisponibilidadeDataAdmin> {
    return this.httpClient.post<DisponibilidadeDataAdmin>(
      `${this.apiUrl}/admin/disponibilidade/${data}/liberar`,
      this.criarMotivoRequest(motivo)
    );
  }

  bloquearData(data: string, motivo: string | null): Observable<DisponibilidadeDataAdmin> {
    return this.httpClient.post<DisponibilidadeDataAdmin>(
      `${this.apiUrl}/admin/disponibilidade/${data}/bloquear`,
      this.criarMotivoRequest(motivo)
    );
  }

  alterarMotivoData(data: string, motivo: string | null): Observable<DisponibilidadeDataAdmin> {
    return this.httpClient.put<DisponibilidadeDataAdmin>(
      `${this.apiUrl}/admin/disponibilidade/${data}/motivo`,
      this.criarMotivoRequest(motivo)
    );
  }

  private criarMotivoRequest(motivo: string | null): DisponibilidadeDataMotivoAdmin {
    const motivoNormalizado = motivo?.trim();
    return { motivo: motivoNormalizado ? motivoNormalizado : null };
  }
}
