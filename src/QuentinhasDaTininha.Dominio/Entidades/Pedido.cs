using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly DataPedido { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? TelefoneCliente { get; set; }
    public decimal ValorSubtotal { get; set; }
    public decimal? ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public bool PrecisaTroco { get; set; }
    public decimal? ValorTroco { get; set; }
    public TipoEntrega TipoEntrega { get; set; }
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Referencia { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();
    public ICollection<PedidoBebida> Bebidas { get; set; } = new List<PedidoBebida>();
}
