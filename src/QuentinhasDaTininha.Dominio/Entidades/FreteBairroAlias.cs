namespace QuentinhasDaTininha.Dominio.Entidades;

public class FreteBairroAlias
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FreteBairroId { get; set; }
    public string AliasNormalizado { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public bool GeradoAutomaticamente { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
    public FreteBairro FreteBairro { get; set; } = null!;
}
