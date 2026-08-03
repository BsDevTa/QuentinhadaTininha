namespace QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;

public class FreteBairroSalvarRequisicao
{
    public string Bairro { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public bool Ativo { get; set; } = true;
}
