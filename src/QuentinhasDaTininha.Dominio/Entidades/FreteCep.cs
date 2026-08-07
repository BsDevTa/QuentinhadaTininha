namespace QuentinhasDaTininha.Dominio.Entidades;

public class FreteCep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FreteBairroId { get; set; }
    public string Cep { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public FreteBairro FreteBairro { get; set; } = null!;
}
