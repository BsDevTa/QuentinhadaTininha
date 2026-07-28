namespace QuentinhasDaTininha.Aplicacao.Acompanhamentos.DTOs;

public class AcompanhamentoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoAdicional { get; set; }
    public bool Ativo { get; set; }
    public bool Disponivel { get; set; }
}
