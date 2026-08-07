using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/pratos")]
public class AdminPratosController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IControleCacheCardapioPublico _controleCacheCardapioPublico;

    public AdminPratosController(
        QuentinhasDaTininhaDbContext dbContext,
        IControleCacheCardapioPublico controleCacheCardapioPublico)
    {
        _dbContext = dbContext;
        _controleCacheCardapioPublico = controleCacheCardapioPublico;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PratoAdminResumoResposta>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] int? diaSemana,
        [FromQuery] bool? estaDisponivel,
        [FromQuery] bool? estaAtivo,
        [FromQuery] string? grupoAcompanhamentoCodigo,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Pratos
            .AsNoTracking()
            .Include(prato => prato.Precos)
            .Include(prato => prato.GrupoAcompanhamento)
            .Include(prato => prato.CardapiosDiaPratos)
                .ThenInclude(cardapioPrato => cardapioPrato.CardapioDia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var nomeBusca = nome.Trim().ToLowerInvariant();
            query = query.Where(prato => prato.Nome.ToLower().Contains(nomeBusca));
        }

        if (estaDisponivel.HasValue)
        {
            query = query.Where(prato => prato.EstaDisponivel == estaDisponivel.Value);
        }

        if (estaAtivo.HasValue)
        {
            query = query.Where(prato => prato.EstaAtivo == estaAtivo.Value);
        }

        if (!string.IsNullOrWhiteSpace(grupoAcompanhamentoCodigo))
        {
            var codigo = grupoAcompanhamentoCodigo.Trim().ToUpperInvariant();
            query = query.Where(prato =>
                prato.GrupoAcompanhamento != null &&
                prato.GrupoAcompanhamento.Codigo == codigo);
        }

        DiaSemana? diaDominio = null;
        if (diaSemana.HasValue)
        {
            if (diaSemana.Value is < 1 or > 7)
            {
                return BadRequest(CriarErro("diaSemana", "Dia da semana deve ficar entre 1 e 7."));
            }

            diaDominio = ConverterDia(diaSemana.Value);
            query = query.Where(prato =>
                prato.CardapiosDiaPratos.Any(item =>
                    item.CardapioDia.DiaSemana == diaDominio &&
                    item.CardapioDia.EstaAtivo));
        }

        var entidades = await query.ToListAsync(cancellationToken);

        var pratos = entidades
            .Select(prato => new PratoAdminResumoResposta
            {
                Id = prato.Id,
                Nome = prato.Nome,
                Descricao = prato.Descricao,
                UrlImagem = prato.UrlImagem,
                EstaAtivo = prato.EstaAtivo,
                EstaDisponivel = prato.EstaDisponivel,
                OrdemExibicao = diaDominio.HasValue
                    ? prato.CardapiosDiaPratos
                        .Where(item => item.CardapioDia.DiaSemana == diaDominio)
                        .Select(item => item.OrdemExibicao)
                        .FirstOrDefault()
                    : prato.OrdemExibicao,
                GrupoAcompanhamento = prato.GrupoAcompanhamento is null
                    ? null
                    : new GrupoAcompanhamentoPratoAdminResposta
                    {
                        Id = prato.GrupoAcompanhamento.Id,
                        Nome = prato.GrupoAcompanhamento.Nome,
                        Codigo = prato.GrupoAcompanhamento.Codigo
                    },
                DiasSemana = prato.CardapiosDiaPratos
                    .OrderBy(item => item.CardapioDia.DiaSemana)
                    .Select(item => item.CardapioDia.DiaSemana == DiaSemana.Domingo ? 7 : (int)item.CardapioDia.DiaSemana)
                    .ToList(),
                Precos = MapearPrecos(prato.Precos),
                DataAtualizacao = prato.AtualizadoEm
            })
            .ToList();

        pratos = diaDominio.HasValue
            ? pratos.OrderBy(prato => prato.OrdemExibicao).ThenBy(prato => prato.Nome).ToList()
            : pratos.OrderBy(prato => prato.Nome).ToList();

        return Ok(pratos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PratoAdminDetalheResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var prato = await ObterDetalheAsync(id, cancellationToken);
        return prato is null ? NotFound() : Ok(prato);
    }

    [HttpPost]
    public async Task<ActionResult<PratoAdminDetalheResposta>> Criar(
        [FromBody] PratoAdminSalvarRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(CriarErro("requisicao", "A requisicao e obrigatoria."));
        }

        var erro = await ValidarRequisicaoAsync(requisicao, null, cancellationToken);
        if (erro is not null)
        {
            return BadRequest(erro);
        }

        await using var transacao = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var agora = DateTimeOffset.UtcNow;
        var categoriaId = await ObterCategoriaPadraoAsync(cancellationToken);
        var prato = new Prato
        {
            Nome = requisicao.Nome.Trim(),
            Descricao = NormalizarOpcional(requisicao.Descricao),
            UrlImagem = NormalizarOpcional(requisicao.UrlImagem),
            CategoriaId = categoriaId,
            GrupoAcompanhamentoId = requisicao.GrupoAcompanhamentoId,
            Preco = requisicao.Precos.PequenaDinheiroPix,
            EstaAtivo = requisicao.EstaAtivo,
            EstaDisponivel = requisicao.EstaDisponivel,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Pratos.AddAsync(prato, cancellationToken);
        ConfigurarPrecos(prato, requisicao.Precos);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await ConfigurarDiasAsync(prato, requisicao.DiasSemana, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
        _controleCacheCardapioPublico.Invalidar();

        var resposta = await ObterDetalheAsync(prato.Id, cancellationToken) ??
            throw new InvalidOperationException("Nao foi possivel carregar o prato criado.");

        return CreatedAtAction(nameof(ObterPorId), new { id = prato.Id }, resposta);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PratoAdminDetalheResposta>> Atualizar(
        Guid id,
        [FromBody] PratoAdminSalvarRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(CriarErro("requisicao", "A requisicao e obrigatoria."));
        }

        var prato = await _dbContext.Pratos
            .Include(item => item.Precos)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (prato is null)
        {
            return NotFound();
        }

        var erro = await ValidarRequisicaoAsync(requisicao, id, cancellationToken);
        if (erro is not null)
        {
            return BadRequest(erro);
        }

        await using var transacao = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        prato.Nome = requisicao.Nome.Trim();
        prato.Descricao = NormalizarOpcional(requisicao.Descricao);
        prato.UrlImagem = NormalizarOpcional(requisicao.UrlImagem);
        prato.GrupoAcompanhamentoId = requisicao.GrupoAcompanhamentoId;
        prato.Preco = requisicao.Precos.PequenaDinheiroPix;
        prato.EstaAtivo = requisicao.EstaAtivo;
        prato.EstaDisponivel = requisicao.EstaDisponivel;
        prato.AtualizadoEm = DateTimeOffset.UtcNow;
        ConfigurarPrecos(prato, requisicao.Precos);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await ConfigurarDiasAsync(prato, requisicao.DiasSemana, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
        _controleCacheCardapioPublico.Invalidar();

        return Ok(await ObterDetalheAsync(id, cancellationToken));
    }

    [HttpPatch("{id:guid}/disponibilidade")]
    public async Task<ActionResult<StatusPratoAdminResposta>> AlterarDisponibilidade(
        Guid id,
        [FromBody] DisponibilidadeAdminRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var prato = await _dbContext.Pratos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (prato is null)
        {
            return NotFound();
        }

        prato.EstaDisponivel = requisicao.EstaDisponivel;
        prato.AtualizadoEm = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCacheCardapioPublico.Invalidar();

        return Ok(new StatusPratoAdminResposta
        {
            Id = prato.Id,
            EstaDisponivel = prato.EstaDisponivel,
            EstaAtivo = prato.EstaAtivo
        });
    }

    [HttpPatch("{id:guid}/ativacao")]
    public async Task<ActionResult<StatusPratoAdminResposta>> AlterarAtivacao(
        Guid id,
        [FromBody] AtivacaoAdminRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var prato = await _dbContext.Pratos.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (prato is null)
        {
            return NotFound();
        }

        prato.EstaAtivo = requisicao.EstaAtivo;
        prato.AtualizadoEm = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCacheCardapioPublico.Invalidar();

        return Ok(new StatusPratoAdminResposta
        {
            Id = prato.Id,
            EstaDisponivel = prato.EstaDisponivel,
            EstaAtivo = prato.EstaAtivo
        });
    }

    private async Task<PratoAdminDetalheResposta?> ObterDetalheAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Pratos
            .AsNoTracking()
            .Include(prato => prato.Precos)
            .Include(prato => prato.CardapiosDiaPratos)
                .ThenInclude(cardapioPrato => cardapioPrato.CardapioDia)
            .Where(prato => prato.Id == id)
            .Select(prato => prato)
            .FirstOrDefaultAsync(cancellationToken) is { } prato
            ? new PratoAdminDetalheResposta
            {
                Id = prato.Id,
                Nome = prato.Nome,
                Descricao = prato.Descricao,
                UrlImagem = prato.UrlImagem,
                EstaAtivo = prato.EstaAtivo,
                EstaDisponivel = prato.EstaDisponivel,
                GrupoAcompanhamentoId = prato.GrupoAcompanhamentoId,
                DiasSemana = prato.CardapiosDiaPratos
                        .OrderBy(item => item.CardapioDia.DiaSemana)
                        .Select(item => new DiaPratoAdminResposta
                        {
                            DiaSemana = item.CardapioDia.DiaSemana == DiaSemana.Domingo ? 7 : (int)item.CardapioDia.DiaSemana,
                            OrdemExibicao = item.OrdemExibicao,
                            EstaAtivo = true
                        })
                        .ToList(),
                Precos = MapearPrecos(prato.Precos)
            }
            : null;
    }

    private async Task<object?> ValidarRequisicaoAsync(
        PratoAdminSalvarRequisicao requisicao,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(requisicao.Nome) ||
            requisicao.Nome.Trim().Length is < 2 or > 120)
        {
            erros["nome"] = new[] { "O nome deve ter entre 2 e 120 caracteres." };
        }

        if (!string.IsNullOrWhiteSpace(requisicao.Descricao) && requisicao.Descricao.Length > 500)
        {
            erros["descricao"] = new[] { "A descricao deve ter no maximo 500 caracteres." };
        }

        if (requisicao.Precos is null || new[]
            {
                requisicao.Precos?.PequenaDinheiroPix ?? 0,
                requisicao.Precos?.PequenaCartao ?? 0,
                requisicao.Precos?.GrandeDinheiroPix ?? 0,
                requisicao.Precos?.GrandeCartao ?? 0
            }.Any(valor => valor <= 0))
        {
            erros["precos"] = new[] { "Informe os quatro precos maiores que zero." };
        }

        if (requisicao.DiasSemana.Any(dia => dia.DiaSemana is < 1 or > 7))
        {
            erros["diasSemana"] = new[] { "Dia da semana deve ficar entre 1 e 7." };
        }

        if (requisicao.DiasSemana.Select(dia => dia.DiaSemana).Distinct().Count() !=
            requisicao.DiasSemana.Count)
        {
            erros["diasSemana"] = new[] { "Nao repita o mesmo dia no prato." };
        }

        if (requisicao.DiasSemana.Any(dia => dia.OrdemExibicao < 0))
        {
            erros["ordemExibicao"] = new[] { "A ordem deve ser maior ou igual a zero." };
        }

        var grupoValido = await _dbContext.GruposAcompanhamento
            .AsNoTracking()
            .AnyAsync(grupo =>
                grupo.Id == requisicao.GrupoAcompanhamentoId &&
                grupo.EstaAtivo,
                cancellationToken);

        if (!grupoValido)
        {
            erros["grupoAcompanhamentoId"] = new[] { "Informe um grupo de acompanhamento ativo." };
        }

        if (!string.IsNullOrWhiteSpace(requisicao.Nome))
        {
            var nome = requisicao.Nome.Trim().ToLowerInvariant();
            var duplicado = await _dbContext.Pratos
                .AsNoTracking()
                .AnyAsync(prato =>
                    prato.Nome.ToLower() == nome &&
                    (!idIgnorado.HasValue || prato.Id != idIgnorado.Value),
                    cancellationToken);

            if (duplicado)
            {
                erros["nome"] = new[] { "Ja existe um prato com esse nome." };
            }
        }

        return erros.Count == 0
            ? null
            : new
            {
                titulo = "Dados invalidos",
                mensagem = "Verifique os campos informados.",
                erros
            };
    }

    private async Task<Guid> ObterCategoriaPadraoAsync(CancellationToken cancellationToken)
    {
        var categoria = await _dbContext.Categorias
            .FirstOrDefaultAsync(item => item.Nome == "Quentinhas", cancellationToken);

        if (categoria is not null)
        {
            return categoria.Id;
        }

        categoria = new Categoria
        {
            Nome = "Quentinhas",
            EstaAtiva = true,
            CriadoEm = DateTimeOffset.UtcNow,
            AtualizadoEm = DateTimeOffset.UtcNow
        };
        await _dbContext.Categorias.AddAsync(categoria, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return categoria.Id;
    }

    private async Task ConfigurarDiasAsync(
        Prato prato,
        IReadOnlyCollection<DiaPratoAdminRequisicao> dias,
        CancellationToken cancellationToken)
    {
        var existentes = await _dbContext.CardapiosDiaPratos
            .Where(item => item.PratoId == prato.Id)
            .ToListAsync(cancellationToken);

        _dbContext.CardapiosDiaPratos.RemoveRange(existentes);

        foreach (var dia in dias.Where(item => item.EstaAtivo))
        {
            var diaDominio = ConverterDia(dia.DiaSemana);
            var cardapio = await _dbContext.CardapiosDia
                .FirstOrDefaultAsync(item => item.DiaSemana == diaDominio, cancellationToken);

            if (cardapio is null)
            {
                cardapio = new CardapioDia
                {
                    DiaSemana = diaDominio,
                    EstaAtivo = true,
                    CriadoEm = DateTimeOffset.UtcNow,
                    AtualizadoEm = DateTimeOffset.UtcNow
                };
                await _dbContext.CardapiosDia.AddAsync(cardapio, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            cardapio.EstaAtivo = true;
            _dbContext.CardapiosDiaPratos.Add(new CardapioDiaPrato
            {
                CardapioDiaId = cardapio.Id,
                PratoId = prato.Id,
                OrdemExibicao = dia.OrdemExibicao,
                EstaDisponivel = true
            });
        }
    }

    private static void ConfigurarPrecos(Prato prato, PrecosPratoAdminDto precos)
    {
        ConfigurarPreco(prato, TamanhoRefeicao.P, TipoPrecoPagamento.DinheiroPix, precos.PequenaDinheiroPix);
        ConfigurarPreco(prato, TamanhoRefeicao.P, TipoPrecoPagamento.Cartao, precos.PequenaCartao);
        ConfigurarPreco(prato, TamanhoRefeicao.G, TipoPrecoPagamento.DinheiroPix, precos.GrandeDinheiroPix);
        ConfigurarPreco(prato, TamanhoRefeicao.G, TipoPrecoPagamento.Cartao, precos.GrandeCartao);
    }

    private static void ConfigurarPreco(
        Prato prato,
        TamanhoRefeicao tamanho,
        TipoPrecoPagamento formaPagamento,
        decimal valor)
    {
        var preco = prato.Precos.FirstOrDefault(item =>
            item.Tamanho == tamanho &&
            item.FormaPagamento == formaPagamento);

        if (preco is null)
        {
            preco = new PrecoPrato
            {
                Tamanho = tamanho,
                FormaPagamento = formaPagamento
            };
            prato.Precos.Add(preco);
        }

        preco.Valor = valor;
    }

    private static PrecosPratoAdminDto MapearPrecos(IEnumerable<PrecoPrato> precos)
    {
        return new PrecosPratoAdminDto
        {
            PequenaDinheiroPix = ObterPreco(precos, TamanhoRefeicao.P, TipoPrecoPagamento.DinheiroPix),
            PequenaCartao = ObterPreco(precos, TamanhoRefeicao.P, TipoPrecoPagamento.Cartao),
            GrandeDinheiroPix = ObterPreco(precos, TamanhoRefeicao.G, TipoPrecoPagamento.DinheiroPix),
            GrandeCartao = ObterPreco(precos, TamanhoRefeicao.G, TipoPrecoPagamento.Cartao)
        };
    }

    private static decimal ObterPreco(
        IEnumerable<PrecoPrato> precos,
        TamanhoRefeicao tamanho,
        TipoPrecoPagamento formaPagamento)
    {
        return precos
            .Where(preco => preco.Tamanho == tamanho && preco.FormaPagamento == formaPagamento)
            .Select(preco => preco.Valor)
            .FirstOrDefault();
    }

    private static DiaSemana ConverterDia(int dia)
    {
        return dia == 7 ? DiaSemana.Domingo : (DiaSemana)dia;
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static object CriarErro(string campo, string mensagem)
    {
        return new
        {
            titulo = "Dados invalidos",
            mensagem = "Verifique os campos informados.",
            erros = new Dictionary<string, string[]> { [campo] = new[] { mensagem } }
        };
    }
}

public class PratoAdminResumoResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlImagem { get; set; }
    public bool EstaAtivo { get; set; }
    public bool EstaDisponivel { get; set; }
    public int OrdemExibicao { get; set; }
    public GrupoAcompanhamentoPratoAdminResposta? GrupoAcompanhamento { get; set; }
    public IReadOnlyList<int> DiasSemana { get; set; } = Array.Empty<int>();
    public PrecosPratoAdminDto Precos { get; set; } = new();
    public DateTimeOffset DataAtualizacao { get; set; }
}

public class PratoAdminDetalheResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlImagem { get; set; }
    public bool EstaAtivo { get; set; }
    public bool EstaDisponivel { get; set; }
    public Guid? GrupoAcompanhamentoId { get; set; }
    public IReadOnlyList<DiaPratoAdminResposta> DiasSemana { get; set; } = Array.Empty<DiaPratoAdminResposta>();
    public PrecosPratoAdminDto Precos { get; set; } = new();
}

public class GrupoAcompanhamentoPratoAdminResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class PratoAdminSalvarRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlImagem { get; set; }
    public bool EstaAtivo { get; set; } = true;
    public bool EstaDisponivel { get; set; } = true;
    public Guid GrupoAcompanhamentoId { get; set; }
    public PrecosPratoAdminDto Precos { get; set; } = new();
    public List<DiaPratoAdminRequisicao> DiasSemana { get; set; } = new();
}

public class PrecosPratoAdminDto
{
    public decimal PequenaDinheiroPix { get; set; }
    public decimal PequenaCartao { get; set; }
    public decimal GrandeDinheiroPix { get; set; }
    public decimal GrandeCartao { get; set; }
}

public class DiaPratoAdminRequisicao
{
    public int DiaSemana { get; set; }
    public int OrdemExibicao { get; set; }
    public bool EstaAtivo { get; set; } = true;
}

public class DiaPratoAdminResposta : DiaPratoAdminRequisicao
{
}

public class DisponibilidadeAdminRequisicao
{
    public bool EstaDisponivel { get; set; }
}

public class AtivacaoAdminRequisicao
{
    public bool EstaAtivo { get; set; }
}

public class StatusPratoAdminResposta
{
    public Guid Id { get; set; }
    public bool EstaDisponivel { get; set; }
    public bool EstaAtivo { get; set; }
}
