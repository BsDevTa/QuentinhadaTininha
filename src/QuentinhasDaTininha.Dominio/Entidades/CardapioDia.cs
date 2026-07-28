using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class CardapioDia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DiaSemana DiaSemana { get; set; }
    public bool EstaAtivo { get; set; }
    public string? Observacao { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<CardapioDiaPrato> CardapiosDiaPratos { get; set; } = new List<CardapioDiaPrato>();
}
