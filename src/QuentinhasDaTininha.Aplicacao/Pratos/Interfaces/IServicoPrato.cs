using QuentinhasDaTininha.Aplicacao.Pratos.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;

public interface IServicoPrato
{
    Task<IReadOnlyList<PratoResumoResposta>> ListarAsync(
        string? busca,
        Guid? categoriaId,
        bool? ativo,
        bool? disponivel,
        CancellationToken cancellationToken = default);

    Task<PratoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PratoResposta> CriarAsync(
        PratoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<PratoResposta?> AtualizarAsync(
        Guid id,
        PratoAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
