namespace QuentinhasDaTininha.Aplicacao.Bebidas.DTOs;

public class BebidaSalvarRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public bool Ativa { get; set; } = true;
    public string? ImagemUrl { get; set; }
}
