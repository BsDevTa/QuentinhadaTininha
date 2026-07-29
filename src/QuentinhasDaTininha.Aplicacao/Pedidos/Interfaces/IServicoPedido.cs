using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Pedidos.Interfaces;

public interface IServicoPedido
{
    Task<PedidoResposta> CriarAsync(
        PedidoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<PedidoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
