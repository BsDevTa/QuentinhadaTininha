import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface BebidaAdmin {
  id: string;
  nome: string;
  descricao: string | null;
  preco: number;
  ativa: boolean;
  imagemUrl: string | null;
  atualizadoEm: string;
}

export interface BebidaAdminSalvar {
  nome: string;
  descricao: string | null;
  preco: number;
  ativa: boolean;
  imagemUrl: string | null;
}

@Injectable({ providedIn: 'root' })
export class BebidasAdministrativoService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  listar(): Observable<BebidaAdmin[]> {
    return this.httpClient.get<BebidaAdmin[]>(`${this.apiUrl}/admin/bebidas`);
  }

  criar(requisicao: BebidaAdminSalvar): Observable<BebidaAdmin> {
    return this.httpClient.post<BebidaAdmin>(`${this.apiUrl}/admin/bebidas`, requisicao);
  }

  atualizar(id: string, requisicao: BebidaAdminSalvar): Observable<BebidaAdmin> {
    return this.httpClient.put<BebidaAdmin>(`${this.apiUrl}/admin/bebidas/${id}`, requisicao);
  }

  excluir(id: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.apiUrl}/admin/bebidas/${id}`);
  }
}
