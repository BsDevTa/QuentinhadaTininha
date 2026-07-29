using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;

public interface IServicoDisponibilidadePedido
{
    Task<IReadOnlyList<DisponibilidadeDataResposta>> ListarAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default);

    Task<DisponibilidadeDataResposta> ObterPorDataAsync(
        DateOnly data,
        CancellationToken cancellationToken = default);

    Task<DisponibilidadeDataResposta> LiberarDataAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default);

    Task<DisponibilidadeDataResposta> BloquearDataAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default);

    Task<DisponibilidadeDataResposta> AlterarMotivoAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default);

    Task<DisponibilidadePublicaResposta> ListarPublicaAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default);

    Task<ValidacaoDisponibilidadePedidoResposta> ValidarPedidoAsync(
        DateOnly data,
        CancellationToken cancellationToken = default);
}
