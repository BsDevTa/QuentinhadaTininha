namespace QuentinhasDaTininha.Dominio.Entidades;

public class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int OrdemExibicao { get; set; }
    public bool EstaAtiva { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Prato> Pratos { get; set; } = new List<Prato>();
}
