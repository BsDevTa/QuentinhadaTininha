namespace QuentinhasDaTininha.Aplicacao.Pratos.DTOs;

public class PratoResumoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public bool Ativo { get; set; }
    public bool Disponivel { get; set; }
    public string? ImagemUrl { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
}
