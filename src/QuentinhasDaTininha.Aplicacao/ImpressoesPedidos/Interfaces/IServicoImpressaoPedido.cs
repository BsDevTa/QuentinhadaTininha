using QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.DTOs;

namespace QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.Interfaces;

public interface IServicoImpressaoPedido
{
    Task<IReadOnlyList<ImpressaoPedidoResposta>> ListarPendentesAsync(
        int limite = 10,
        CancellationToken cancellationToken = default);

    Task<ImpressaoPedidoResposta?> IniciarAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ConcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> RegistrarErroAsync(
        Guid id,
        string? erro,
        CancellationToken cancellationToken = default);

    Task<ImpressaoPedidoResposta?> CriarReimpressaoAsync(
        Guid pedidoId,
        CancellationToken cancellationToken = default);
}
