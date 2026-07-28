using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;

public class CardapioDiaResposta
{
    public Guid Id { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public bool Ativo { get; set; }
    public string? Observacao { get; set; }
    public IReadOnlyList<CardapioDiaPratoResposta> Pratos { get; set; } =
        new List<CardapioDiaPratoResposta>();
}
