using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.DTOs;

namespace QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.Interfaces;

public interface IServicoConfiguracaoRestaurante
{
    Task<ConfiguracaoRestauranteResposta?> ObterAsync(
        CancellationToken cancellationToken = default);

    Task<ConfiguracaoRestauranteResposta> AtualizarAsync(
        ConfiguracaoRestauranteAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);
}
