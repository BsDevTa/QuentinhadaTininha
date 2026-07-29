using QuentinhasDaTininha.Aplicacao.Publico.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

public interface IServicoCardapioDiaPublico
{
    Task<CardapioDiaPublicoResposta> ObterHojeAsync(CancellationToken cancellationToken = default);
    Task<CardapioDiaPublicoResposta> ObterPorDiaAsync(int diaSemana, CancellationToken cancellationToken = default);
    Task<RestauranteStatusPublicoResposta> ObterStatusRestauranteAsync(CancellationToken cancellationToken = default);
}
