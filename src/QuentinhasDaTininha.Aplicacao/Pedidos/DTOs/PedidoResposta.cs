using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;

public class PedidoResposta
{
    public Guid Id { get; set; }
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
    public IReadOnlyList<PedidoItemResposta> Itens { get; set; } = Array.Empty<PedidoItemResposta>();
    public IReadOnlyList<PedidoBebidaResposta> Bebidas { get; set; } = Array.Empty<PedidoBebidaResposta>();
    public DateTimeOffset CriadoEm { get; set; }
}

public class PedidoItemResposta
{
    public Guid Id { get; set; }
    public Guid PratoId { get; set; }
    public string NomePrato { get; set; } = string.Empty;
    public TamanhoRefeicao Tamanho { get; set; }
    public string? Acompanhamentos { get; set; }
    public decimal ValorUnitario { get; set; }
    public string? Observacao { get; set; }
}

public class PedidoBebidaResposta
{
    public Guid Id { get; set; }
    public Guid BebidaId { get; set; }
    public string NomeBebida { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}
