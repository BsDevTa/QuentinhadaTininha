namespace QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;

public class CardapioDiaAtualizacaoRequisicao
{
    public bool Ativo { get; set; } = true;
    public string? Observacao { get; set; }
    public ICollection<CardapioDiaPratoRequisicao> Pratos { get; set; } =
        new List<CardapioDiaPratoRequisicao>();
}
