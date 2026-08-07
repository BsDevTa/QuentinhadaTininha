import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AutenticacaoService } from './autenticacao.service';

describe('AutenticacaoService', () => {
  let http: HttpTestingController;
  let service: AutenticacaoService;

  beforeEach(() => {
    localStorage.clear();
    localStorage.setItem('quentinhas_admin_token', 'token-valido');
    localStorage.setItem(
      'quentinhas_admin_expiracao',
      new Date(Date.now() + 60000).toISOString()
    );

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(AutenticacaoService);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
    TestBed.resetTestingModule();
  });

  it('deduplica restauracoes simultaneas de sessao', () => {
    const resultados: boolean[] = [];

    service.restaurarSessao().subscribe((resultado) => resultados.push(resultado));
    service.restaurarSessao().subscribe((resultado) => resultados.push(resultado));

    expect(service.carregandoSessao()).toBe(true);

    const req = http.expectOne(`${environment.apiUrl}/autenticacao/sessao`);
    req.flush({
      autenticado: true,
      usuario: {
        id: '11111111-1111-1111-1111-111111111111',
        nome: 'Tininha',
        email: 'admin@tininha.test'
      }
    });

    expect(resultados).toEqual([true, true]);
    expect(service.carregandoSessao()).toBe(false);
    expect(service.usuarioAtual()?.email).toBe('admin@tininha.test');
  });

  it('encerra o loading e limpa sessao quando a restauracao falha', () => {
    const resultados: boolean[] = [];

    service.restaurarSessao().subscribe((resultado) => resultados.push(resultado));

    http.expectOne(`${environment.apiUrl}/autenticacao/sessao`).flush(
      { mensagem: 'nao autorizado' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(resultados).toEqual([false]);
    expect(service.carregandoSessao()).toBe(false);
    expect(service.obterToken()).toBeNull();
  });
});
