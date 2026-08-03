namespace QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;

public class ConsultaFreteBairroResposta
{
    public bool Atendido { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public decimal? ValorFrete { get; set; }
    public string? Mensagem { get; set; }
}
