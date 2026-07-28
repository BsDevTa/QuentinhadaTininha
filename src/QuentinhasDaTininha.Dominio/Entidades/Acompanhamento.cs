namespace QuentinhasDaTininha.Dominio.Entidades;

public class Acompanhamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoAdicional { get; set; }
    public bool EstaAtivo { get; set; }
    public bool EstaDisponivel { get; set; }
    public string? MotivoIndisponibilidade { get; set; }
    public int OrdemExibicao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<PratoAcompanhamento> PratoAcompanhamentos { get; set; } = new List<PratoAcompanhamento>();
}
