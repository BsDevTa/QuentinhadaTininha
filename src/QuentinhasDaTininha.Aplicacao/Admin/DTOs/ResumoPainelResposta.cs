namespace QuentinhasDaTininha.Aplicacao.Admin.DTOs;

public class ResumoPainelResposta
{
    public bool RestauranteAberto { get; set; }
    public string MensagemStatus { get; set; } = string.Empty;
    public int QuantidadePratosHoje { get; set; }
    public int QuantidadePratosDisponiveis { get; set; }
    public int QuantidadePratosIndisponiveis { get; set; }
    public int QuantidadeAcompanhamentosIndisponiveis { get; set; }
    public int DiaSemana { get; set; }
    public string NomeDiaSemana { get; set; } = string.Empty;
}
