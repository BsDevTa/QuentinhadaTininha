namespace QuentinhasDaTininha.Aplicacao.Publico.DTOs;

public class CardapioDiaPublicoResposta
{
    public int DiaSemana { get; set; }
    public string NomeDiaSemana { get; set; } = string.Empty;
    public RestauranteStatusPublicoResposta Restaurante { get; set; } = new();
    public IReadOnlyList<PratoPublicoResposta> Pratos { get; set; } = new List<PratoPublicoResposta>();
}

public class RestauranteStatusPublicoResposta
{
    public string Nome { get; set; } = string.Empty;
    public bool EstaAberto { get; set; }
    public string? MensagemStatus { get; set; }
    public string? Whatsapp { get; set; }
    public string? Instagram { get; set; }
    public string? Endereco { get; set; }
    public string? HorarioFuncionamento { get; set; }
    public string? UrlLogo { get; set; }
}

public class PratoPublicoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlImagem { get; set; }
    public bool EstaDisponivel { get; set; }
    public int OrdemExibicao { get; set; }
    public PrecosPratoPublicoResposta Precos { get; set; } = new();
    public GrupoAcompanhamentoPublicoResposta GrupoAcompanhamento { get; set; } = new();
}

public class PrecosPratoPublicoResposta
{
    public decimal PequenaDinheiroPix { get; set; }
    public decimal PequenaCartao { get; set; }
    public decimal GrandeDinheiroPix { get; set; }
    public decimal GrandeCartao { get; set; }
}

public class GrupoAcompanhamentoPublicoResposta
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public IReadOnlyList<AcompanhamentoPublicoResposta> Acompanhamentos { get; set; } =
        new List<AcompanhamentoPublicoResposta>();
}

public class AcompanhamentoPublicoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool EstaDisponivel { get; set; }
    public string TipoSelecao { get; set; } = string.Empty;
    public string? GrupoExclusivo { get; set; }
    public bool Obrigatorio { get; set; }
    public int OrdemExibicao { get; set; }
}
