using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class ImpressaoPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public StatusImpressaoPedido Status { get; set; } = StatusImpressaoPedido.Pendente;
    public int Tentativas { get; set; }
    public bool Reimpressao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ImpressoEm { get; set; }
    public string? UltimoErro { get; set; }
    public Pedido Pedido { get; set; } = null!;
}
