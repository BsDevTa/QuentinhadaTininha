using QuentinhasDaTininha.Aplicacao.Categorias.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Categorias.Interfaces;

public interface IServicoCategoria
{
    Task<IReadOnlyList<CategoriaResposta>> ListarAsync(
        string? busca,
        bool? ativo,
        CancellationToken cancellationToken = default);

    Task<CategoriaResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CategoriaResposta> CriarAsync(
        CategoriaCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<CategoriaResposta?> AtualizarAsync(
        Guid id,
        CategoriaAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
