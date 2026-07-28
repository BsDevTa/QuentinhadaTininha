using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class HorarioFuncionamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DiaSemana DiaSemana { get; set; }
    public TimeOnly HoraAbertura { get; set; }
    public TimeOnly HoraFechamento { get; set; }
    public bool EstaAtivo { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
