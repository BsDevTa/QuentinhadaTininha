using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;

public interface IServicoFechamentoExcepcional
{
    Task<IReadOnlyList<FechamentoExcepcionalResposta>> ListarAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default);

    Task<FechamentoExcepcionalResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FechamentoExcepcionalResposta> CriarAsync(
        FechamentoExcepcionalCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<FechamentoExcepcionalResposta?> AtualizarAsync(
        Guid id,
        FechamentoExcepcionalAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
