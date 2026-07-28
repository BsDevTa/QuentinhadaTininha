namespace QuentinhasDaTininha.Dominio.Entidades;

public class Prato
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public string? UrlImagem { get; set; }
    public bool EstaAtivo { get; set; }
    public bool EstaDisponivel { get; set; }
    public string? MotivoIndisponibilidade { get; set; }
    public bool EhDestaque { get; set; }
    public int OrdemExibicao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public Categoria Categoria { get; set; } = null!;
    public ICollection<PratoAcompanhamento> PratoAcompanhamentos { get; set; } = new List<PratoAcompanhamento>();
    public ICollection<CardapioDiaPrato> CardapiosDiaPratos { get; set; } = new List<CardapioDiaPrato>();
}
