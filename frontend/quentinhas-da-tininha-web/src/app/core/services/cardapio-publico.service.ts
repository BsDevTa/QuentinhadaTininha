import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CardapioPublicoResposta } from '../models/cardapio-publico.model';

@Injectable({
  providedIn: 'root'
})
export class CardapioPublicoService {
  private readonly endpoint = `${environment.apiUrl}/api/publico/cardapio`;

  constructor(private readonly httpClient: HttpClient) {}

  obterCardapio(): Observable<CardapioPublicoResposta> {
    return this.httpClient.get<CardapioPublicoResposta>(this.endpoint);
  }
}
