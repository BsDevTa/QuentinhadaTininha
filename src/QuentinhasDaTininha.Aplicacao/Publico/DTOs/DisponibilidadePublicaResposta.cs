namespace QuentinhasDaTininha.Aplicacao.Publico.DTOs;

public class DisponibilidadePublicaResposta
{
    public IReadOnlyList<DateOnly> DatasDisponiveis { get; set; } = new List<DateOnly>();
    public IReadOnlyList<DisponibilidadeDataPublicaResposta> DatasBloqueadas { get; set; } =
        new List<DisponibilidadeDataPublicaResposta>();
    public IReadOnlyList<DisponibilidadeDataPublicaResposta> Datas { get; set; } =
        new List<DisponibilidadeDataPublicaResposta>();
}

public class DisponibilidadeDataPublicaResposta
{
    public DateOnly Data { get; set; }
    public bool Disponivel { get; set; }
    public bool PermitirPedidos { get; set; }
    public string? Motivo { get; set; }
    public string? MotivoBloqueio { get; set; }
}
