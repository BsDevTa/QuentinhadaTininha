namespace QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;

public class CardapioDiaPratoResposta
{
    public Guid PratoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public string? ImagemUrl { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public bool PratoAtivo { get; set; }
    public bool PratoDisponivel { get; set; }
    public bool DisponivelNoCardapio { get; set; }
    public int OrdemExibicao { get; set; }
}
