namespace QuentinhasDaTininha.Aplicacao.Bebidas.DTOs;

public class BebidaResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public bool Ativa { get; set; }
    public string? ImagemUrl { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
