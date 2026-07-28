namespace QuentinhasDaTininha.Dominio.Entidades;

public class PratoAcompanhamento
{
    public Guid PratoId { get; set; }
    public Guid AcompanhamentoId { get; set; }
    public bool EstaIncluido { get; set; }
    public bool EhObrigatorio { get; set; }
    public int? QuantidadeMaxima { get; set; }
    public Prato Prato { get; set; } = null!;
    public Acompanhamento Acompanhamento { get; set; } = null!;
}
