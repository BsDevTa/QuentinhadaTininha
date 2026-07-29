using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class PrecoPrato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PratoId { get; set; }
    public TamanhoRefeicao Tamanho { get; set; }
    public TipoPrecoPagamento FormaPagamento { get; set; }
    public decimal Valor { get; set; }
    public Prato Prato { get; set; } = null!;
}
