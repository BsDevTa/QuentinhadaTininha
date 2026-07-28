namespace QuentinhasDaTininha.Aplicacao.Categorias.DTOs;

public class CategoriaAtualizacaoRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; }
}
