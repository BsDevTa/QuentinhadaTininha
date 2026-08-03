namespace QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;

public class FreteBairroResposta
{
    public Guid Id { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string BairroNormalizado { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
