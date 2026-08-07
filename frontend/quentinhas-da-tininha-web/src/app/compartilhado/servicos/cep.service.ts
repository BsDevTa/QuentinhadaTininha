import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ConsultaFreteCep {
  cep: string;
  logradouro: string | null;
  bairro: string;
  cidade: string;
  estado: string;
  bairroFrete: string | null;
  atendido: boolean;
  valorFrete: number | null;
  mensagem: string | null;
}

@Injectable({ providedIn: 'root' })
export class CepService {
  private readonly httpClient = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl.replace(/\/$/, '');

  consultarFretePorCep(cep: string): Observable<ConsultaFreteCep> {
    const cepNumerico = cep.replace(/\D/g, '');
    const params = new HttpParams().set('cep', cepNumerico);

    return this.httpClient.get<ConsultaFreteCep>(
      `${this.apiUrl}/publico/fretes-bairros/consultar-por-cep`,
      { params }
    );
  }
}
