using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;

public class PedidoCriacaoRequisicao
{
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
    public List<PedidoItemCriacaoRequisicao> Itens { get; set; } = new();
    public List<PedidoBebidaCriacaoRequisicao> Bebidas { get; set; } = new();
}

public class PedidoItemCriacaoRequisicao
{
    public Guid PratoId { get; set; }
    public TamanhoRefeicao Tamanho { get; set; }
    public List<Guid> AcompanhamentoIds { get; set; } = new();
    public string? Observacao { get; set; }
}

public class PedidoBebidaCriacaoRequisicao
{
    public Guid BebidaId { get; set; }
    public int Quantidade { get; set; }
    public decimal? ValorUnitario { get; set; }
}
