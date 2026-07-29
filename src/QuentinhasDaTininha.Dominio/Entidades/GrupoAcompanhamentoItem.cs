namespace QuentinhasDaTininha.Dominio.Entidades;

public class GrupoAcompanhamentoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GrupoAcompanhamentoId { get; set; }
    public Guid AcompanhamentoId { get; set; }
    public bool Obrigatorio { get; set; }
    public int OrdemExibicao { get; set; }
    public GrupoAcompanhamento GrupoAcompanhamento { get; set; } = null!;
    public Acompanhamento Acompanhamento { get; set; } = null!;
}
