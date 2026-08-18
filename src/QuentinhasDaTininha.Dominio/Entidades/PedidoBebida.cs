namespace QuentinhasDaTininha.Dominio.Entidades;

public class PedidoBebida
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Guid BebidaId { get; set; }
    public string NomeBebida { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Pedido Pedido { get; set; } = null!;
    public Bebida Bebida { get; set; } = null!;
}
