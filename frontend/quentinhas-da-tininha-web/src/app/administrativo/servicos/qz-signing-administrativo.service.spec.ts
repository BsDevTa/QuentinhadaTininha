import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { QzSigningAdministrativoService } from './qz-signing-administrativo.service';

describe('QzSigningAdministrativoService', () => {
  let http: HttpTestingController;
  let service: QzSigningAdministrativoService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(QzSigningAdministrativoService);
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('obtem certificado como texto puro', () => {
    const resultados: string[] = [];

    service.obterCertificado().subscribe((certificado) => resultados.push(certificado));

    const req = http.expectOne(`${environment.apiUrl}/admin/qz/certificado`);
    expect(req.request.responseType).toBe('text');
    req.flush('CERTIFICADO PUBLICO');

    expect(resultados).toEqual(['CERTIFICADO PUBLICO']);
  });

  it('envia dados exatos para assinatura e retorna base64', () => {
    const resultados: string[] = [];
    const dados = 'dados com espacos  \n  e quebras';

    service.assinar(dados).subscribe((resposta) => resultados.push(resposta.assinatura));

    const req = http.expectOne(`${environment.apiUrl}/admin/qz/assinar`);
    expect(req.request.body).toEqual({ dados });
    req.flush({ assinatura: 'base64' });

    expect(resultados).toEqual(['base64']);
  });
});
