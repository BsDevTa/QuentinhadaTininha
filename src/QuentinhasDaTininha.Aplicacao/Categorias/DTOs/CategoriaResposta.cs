namespace QuentinhasDaTininha.Aplicacao.Categorias.DTOs;

public class CategoriaResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
