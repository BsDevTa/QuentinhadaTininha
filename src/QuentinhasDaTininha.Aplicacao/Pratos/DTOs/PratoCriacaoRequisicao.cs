namespace QuentinhasDaTininha.Aplicacao.Pratos.DTOs;

public class PratoCriacaoRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public Guid CategoriaId { get; set; }
    public bool Disponivel { get; set; } = true;
    public string? ImagemUrl { get; set; }
    public ICollection<Guid> AcompanhamentoIds { get; set; } = new List<Guid>();
}
