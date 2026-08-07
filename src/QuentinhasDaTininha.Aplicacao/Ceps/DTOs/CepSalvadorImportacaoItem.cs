namespace QuentinhasDaTininha.Aplicacao.Ceps.DTOs;

public class CepSalvadorImportacaoItem
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public int? LinhaOrigem { get; set; }
}
