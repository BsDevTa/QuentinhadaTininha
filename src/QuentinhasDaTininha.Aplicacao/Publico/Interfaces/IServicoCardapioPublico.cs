using QuentinhasDaTininha.Aplicacao.Publico.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

public interface IServicoCardapioPublico
{
    Task<CardapioPublicoResposta> ObterAsync(
        DateOnly? data,
        CancellationToken cancellationToken = default);
}
