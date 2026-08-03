using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;

public interface IServicoCep
{
    Task<EnderecoCepResposta?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default);
}
