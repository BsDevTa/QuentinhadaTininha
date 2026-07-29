namespace QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;

public class FechamentoExcepcionalCriacaoRequisicao
{
    public DateOnly DataFechamento { get; set; }
    public string? Motivo { get; set; }
    public string? MensagemCliente { get; set; }
    public bool DiaInteiro { get; set; } = true;
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFim { get; set; }
    public bool Ativo { get; set; } = true;
    public bool PermitirPedidos { get; set; }
}
