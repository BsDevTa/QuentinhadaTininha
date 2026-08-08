using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.DTOs;

public class ImpressaoPedidoResposta
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public StatusImpressaoPedido Status { get; set; }
    public int Tentativas { get; set; }
    public bool Reimpressao { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
    public DateTimeOffset? ImpressoEm { get; set; }
    public string? UltimoErro { get; set; }
    public PedidoResposta Pedido { get; set; } = new();
}
