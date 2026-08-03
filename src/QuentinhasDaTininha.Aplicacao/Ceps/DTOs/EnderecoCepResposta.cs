namespace QuentinhasDaTininha.Aplicacao.Ceps.DTOs;

public class EnderecoCepResposta
{
    public string Cep { get; set; } = string.Empty;
    public string? Logradouro { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
