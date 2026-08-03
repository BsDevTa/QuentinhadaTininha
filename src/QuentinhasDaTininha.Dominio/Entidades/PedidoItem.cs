using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class PedidoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Guid PratoId { get; set; }
    public string NomePrato { get; set; } = string.Empty;
    public TamanhoRefeicao Tamanho { get; set; }
    public string? Acompanhamentos { get; set; }
    public decimal ValorUnitario { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Pedido Pedido { get; set; } = null!;
    public Prato Prato { get; set; } = null!;
}
