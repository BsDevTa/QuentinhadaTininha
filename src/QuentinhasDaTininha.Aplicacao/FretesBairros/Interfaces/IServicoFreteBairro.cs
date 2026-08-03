using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;

namespace QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;

public interface IServicoFreteBairro
{
    Task<IReadOnlyList<FreteBairroResposta>> ListarAsync(
        string? bairro,
        bool? ativo,
        CancellationToken cancellationToken = default);

    Task<FreteBairroResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FreteBairroResposta> CriarAsync(
        FreteBairroSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<FreteBairroResposta?> AtualizarAsync(
        Guid id,
        FreteBairroSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<FreteBairroResposta?> AlterarStatusAsync(
        Guid id,
        bool ativo,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ConsultaFreteBairroResposta> ConsultarPorBairroAsync(
        string bairro,
        CancellationToken cancellationToken = default);

    Task<ConsultaFreteCepResposta> ConsultarPorCepAsync(
        string cep,
        CancellationToken cancellationToken = default);
}
