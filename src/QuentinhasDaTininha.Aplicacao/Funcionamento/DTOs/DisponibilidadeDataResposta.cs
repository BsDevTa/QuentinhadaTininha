namespace QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;

public class DisponibilidadeDataResposta
{
    public DateOnly Data { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Liberado { get; set; }
    public bool Bloqueado { get; set; }
    public bool PermitirPedidos { get; set; }
    public string? Motivo { get; set; }
}

public class DisponibilidadeDataMotivoRequisicao
{
    public string? Motivo { get; set; }
}

public class ValidacaoDisponibilidadePedidoResposta
{
    public DateOnly Data { get; set; }
    public bool PermitirPedidos { get; set; }
    public string? MotivoBloqueio { get; set; }
}
