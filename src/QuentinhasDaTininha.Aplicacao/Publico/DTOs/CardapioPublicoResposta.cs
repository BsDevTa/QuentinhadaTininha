using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Publico.DTOs;

public class CardapioPublicoResposta
{
    public RestaurantePublicoResposta Restaurante { get; set; } = new();
    public DateOnly Data { get; set; }
    public DiaSemana DiaSemana { get; set; }
    public bool Aberto { get; set; }
    public string? MotivoFechamento { get; set; }
    public string? Mensagem { get; set; }
    public IReadOnlyList<HorarioFuncionamentoPublicoResposta> Horarios { get; set; } =
        new List<HorarioFuncionamentoPublicoResposta>();
    public IReadOnlyList<CategoriaCardapioPublicoResposta> Categorias { get; set; } =
        new List<CategoriaCardapioPublicoResposta>();
}

public class RestaurantePublicoResposta
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlLogotipo { get; set; }
    public string? UrlImagemCapa { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
}

public class HorarioFuncionamentoPublicoResposta
{
    public TimeOnly HoraAbertura { get; set; }
    public TimeOnly HoraFechamento { get; set; }
}

public class CategoriaCardapioPublicoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public IReadOnlyList<PratoCardapioPublicoResposta> Pratos { get; set; } =
        new List<PratoCardapioPublicoResposta>();
}

public class PratoCardapioPublicoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public string? ImagemUrl { get; set; }
    public IReadOnlyList<AcompanhamentoCardapioPublicoResposta> Acompanhamentos { get; set; } =
        new List<AcompanhamentoCardapioPublicoResposta>();
}

public class AcompanhamentoCardapioPublicoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal PrecoAdicional { get; set; }
}
