namespace QuentinhasDaTininha.Dominio.Entidades;

public class FechamentoExcepcional
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly DataFechamento { get; set; }
    public string? Motivo { get; set; }
    public string? MensagemCliente { get; set; }
    public bool DiaInteiro { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public bool EstaAtivo { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
