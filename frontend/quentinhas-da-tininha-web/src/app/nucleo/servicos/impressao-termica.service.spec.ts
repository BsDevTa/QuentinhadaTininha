import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { ImpressaoTermicaService } from './impressao-termica.service';

const qzMock = vi.hoisted(() => {
  let ativo = false;

  return {
    resetar: () => {
      ativo = false;
    },
    websocket: {
      isActive: vi.fn(() => ativo),
      connect: vi.fn(async () => {
        ativo = true;
      }),
      getConnectionInfo: vi.fn(() => ({ host: 'localhost', port: 8182 })),
      setClosedCallbacks: vi.fn(),
      setErrorCallbacks: vi.fn()
    },
    printers: {
      find: vi.fn()
    },
    configs: {
      create: vi.fn()
    },
    print: vi.fn(),
    security: {
      setCertificatePromise: vi.fn(),
      setSignatureAlgorithm: vi.fn(),
      setSignaturePromise: vi.fn()
    }
  };
});

vi.mock('qz-tray', () => ({ default: qzMock }));

describe('ImpressaoTermicaService QZ security', () => {
  let http: HttpTestingController;
  let service: ImpressaoTermicaService;

  beforeEach(() => {
    qzMock.resetar();
    qzMock.websocket.isActive.mockClear();
    qzMock.websocket.connect.mockClear();
    qzMock.websocket.getConnectionInfo.mockClear();
    qzMock.websocket.setClosedCallbacks.mockClear();
    qzMock.websocket.setErrorCallbacks.mockClear();
    qzMock.security.setCertificatePromise.mockClear();
    qzMock.security.setSignatureAlgorithm.mockClear();
    qzMock.security.setSignaturePromise.mockClear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(ImpressaoTermicaService);
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
  });

  it('configura seguranca uma vez antes de reutilizar conexao', async () => {
    await service.conectar();
    await service.conectar();

    expect(qzMock.security.setCertificatePromise).toHaveBeenCalledTimes(1);
    expect(qzMock.security.setSignatureAlgorithm).toHaveBeenCalledTimes(1);
    expect(qzMock.security.setSignatureAlgorithm).toHaveBeenCalledWith('SHA512');
    expect(qzMock.security.setSignaturePromise).toHaveBeenCalledTimes(1);
    expect(qzMock.websocket.connect).toHaveBeenCalledTimes(1);
  });

  it('usa certificate promise e signature promise chamando a API admin', async () => {
    await service.conectar();

    const certificateHandler = qzMock.security.setCertificatePromise.mock.calls[0][0] as () => Promise<string>;
    const certificatePromise = certificateHandler();
    http.expectOne(`${environment.apiUrl}/admin/qz/certificado`).flush('CERTIFICADO');
    await expect(certificatePromise).resolves.toBe('CERTIFICADO');

    const signatureHandler = qzMock.security.setSignaturePromise.mock.calls[0][0] as (dados: string) => Promise<string>;
    const signaturePromise = signatureHandler('dados qz');
    const req = http.expectOne(`${environment.apiUrl}/admin/qz/assinar`);
    expect(req.request.body).toEqual({ dados: 'dados qz' });
    req.flush({ assinatura: 'assinatura-base64' });

    await expect(signaturePromise).resolves.toBe('assinatura-base64');
  });
});
