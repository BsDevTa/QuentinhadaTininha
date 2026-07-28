using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;

public class HorarioFuncionamentoResposta
{
    public Guid Id { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public TimeOnly HoraAbertura { get; set; }
    public TimeOnly HoraFechamento { get; set; }
    public bool Ativo { get; set; }
}
