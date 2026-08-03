namespace QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;

public class ConsultaFreteCepResposta
{
    public string Cep { get; set; } = string.Empty;
    public string? Logradouro { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Atendido { get; set; }
    public decimal? ValorFrete { get; set; }
    public string? Mensagem { get; set; }
}
