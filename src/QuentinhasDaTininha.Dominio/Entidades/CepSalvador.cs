namespace QuentinhasDaTininha.Dominio.Entidades;

public class CepSalvador
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string BairroNormalizado { get; set; } = string.Empty;
    public string Cidade { get; set; } = "Salvador";
    public string Uf { get; set; } = "BA";
    public bool Ativo { get; set; } = true;
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
