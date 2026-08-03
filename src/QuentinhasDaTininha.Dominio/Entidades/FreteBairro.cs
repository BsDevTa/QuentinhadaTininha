namespace QuentinhasDaTininha.Dominio.Entidades;

public class FreteBairro
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Bairro { get; set; } = string.Empty;
    public string BairroNormalizado { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
