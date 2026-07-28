namespace QuentinhasDaTininha.Aplicacao.Pratos.DTOs;

public class PratoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public bool Ativo { get; set; }
    public bool Disponivel { get; set; }
    public string? ImagemUrl { get; set; }
    public PratoCategoriaResposta Categoria { get; set; } = null!;
    public IReadOnlyList<PratoAcompanhamentoResposta> Acompanhamentos { get; set; } =
        new List<PratoAcompanhamentoResposta>();
}

public class PratoCategoriaResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class PratoAcompanhamentoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Disponivel { get; set; }
}
