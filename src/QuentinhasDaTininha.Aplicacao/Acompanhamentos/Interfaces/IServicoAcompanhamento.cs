using QuentinhasDaTininha.Aplicacao.Acompanhamentos.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Acompanhamentos.Interfaces;

public interface IServicoAcompanhamento
{
    Task<IReadOnlyList<AcompanhamentoResposta>> ListarAsync(
        string? busca,
        bool? ativo,
        CancellationToken cancellationToken = default);

    Task<AcompanhamentoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AcompanhamentoResposta> CriarAsync(
        AcompanhamentoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<AcompanhamentoResposta?> AtualizarAsync(
        Guid id,
        AcompanhamentoAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
