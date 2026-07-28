namespace QuentinhasDaTininha.Dominio.Entidades;

public class CardapioDiaPrato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardapioDiaId { get; set; }
    public Guid PratoId { get; set; }
    public int OrdemExibicao { get; set; }
    public bool EstaDisponivel { get; set; }
    public CardapioDia CardapioDia { get; set; } = null!;
    public Prato Prato { get; set; } = null!;
}
