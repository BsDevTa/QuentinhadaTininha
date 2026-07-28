namespace QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;

public class HorarioFuncionamentoRequisicao
{
    public TimeOnly HoraAbertura { get; set; }
    public TimeOnly HoraFechamento { get; set; }
    public bool Ativo { get; set; } = true;
}
