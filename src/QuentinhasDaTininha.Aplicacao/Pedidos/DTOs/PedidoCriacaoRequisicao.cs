using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;

public class PedidoCriacaoRequisicao
{
    public DateOnly DataPedido { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? TelefoneCliente { get; set; }
    public decimal ValorTotal { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public bool PrecisaTroco { get; set; }
    public decimal? ValorTroco { get; set; }
    public TipoEntrega TipoEntrega { get; set; }
    public string? EnderecoEntrega { get; set; }
    public string? Bairro { get; set; }
    public string? Referencia { get; set; }
    public string? Observacao { get; set; }
}
