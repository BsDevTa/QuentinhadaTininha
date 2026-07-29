using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/acompanhamentos")]
public class AdminAcompanhamentosController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public AdminAcompanhamentosController(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AcompanhamentoAdminResposta>>> Listar(
        [FromQuery] string? nome,
        [FromQuery] bool? estaDisponivel,
        [FromQuery] bool? estaAtivo,
        [FromQuery] Guid? grupoAcompanhamentoId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Acompanhamentos
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var busca = nome.Trim().ToLowerInvariant();
            query = query.Where(acompanhamento => acompanhamento.Nome.ToLower().Contains(busca));
        }

        if (estaDisponivel.HasValue)
        {
            query = query.Where(acompanhamento => acompanhamento.EstaDisponivel == estaDisponivel.Value);
        }

        if (estaAtivo.HasValue)
        {
            query = query.Where(acompanhamento => acompanhamento.EstaAtivo == estaAtivo.Value);
        }

        if (grupoAcompanhamentoId.HasValue)
        {
            query = query.Where(acompanhamento =>
                acompanhamento.GruposAcompanhamentoItens.Any(item =>
                    item.GrupoAcompanhamentoId == grupoAcompanhamentoId.Value));
        }

        var acompanhamentos = await query
            .OrderBy(acompanhamento => acompanhamento.Nome)
            .Select(acompanhamento => new AcompanhamentoAdminResposta
            {
                Id = acompanhamento.Id,
                Nome = acompanhamento.Nome,
                EstaAtivo = acompanhamento.EstaAtivo,
                EstaDisponivel = acompanhamento.EstaDisponivel,
                TipoSelecao = acompanhamento.TipoSelecao.ToString().ToUpperInvariant(),
                GrupoExclusivo = acompanhamento.GrupoExclusivo,
                Grupos = acompanhamento.GruposAcompanhamentoItens
                    .OrderBy(item => item.GrupoAcompanhamento.Nome)
                    .Select(item => new GrupoAcompanhamentoVinculoAdminResposta
                    {
                        GrupoAcompanhamentoId = item.GrupoAcompanhamentoId,
                        Nome = item.GrupoAcompanhamento.Nome,
                        Codigo = item.GrupoAcompanhamento.Codigo,
                        Obrigatorio = item.Obrigatorio,
                        OrdemExibicao = item.OrdemExibicao
                    })
                    .ToList(),
                DataAtualizacao = acompanhamento.AtualizadoEm
            })
            .ToListAsync(cancellationToken);

        return Ok(acompanhamentos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AcompanhamentoAdminResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await ObterRespostaAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AcompanhamentoAdminResposta>> Criar(
        [FromBody] AcompanhamentoAdminSalvarRequisicao? requisicao,
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
        var acompanhamento = new Acompanhamento
        {
            Nome = requisicao.Nome.Trim(),
            EstaAtivo = requisicao.EstaAtivo,
            EstaDisponivel = requisicao.EstaDisponivel,
            TipoSelecao = ConverterTipo(requisicao.TipoSelecao),
            GrupoExclusivo = NormalizarOpcional(requisicao.GrupoExclusivo),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Acompanhamentos.AddAsync(acompanhamento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await ConfigurarGruposAsync(acompanhamento, requisicao.Grupos, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);

        var resposta = await ObterRespostaAsync(acompanhamento.Id, cancellationToken) ??
            throw new InvalidOperationException("Nao foi possivel carregar o acompanhamento criado.");
        return CreatedAtAction(nameof(ObterPorId), new { id = acompanhamento.Id }, resposta);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AcompanhamentoAdminResposta>> Atualizar(
        Guid id,
        [FromBody] AcompanhamentoAdminSalvarRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(CriarErro("requisicao", "A requisicao e obrigatoria."));
        }

        var acompanhamento = await _dbContext.Acompanhamentos
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (acompanhamento is null)
        {
            return NotFound();
        }

        var erro = await ValidarRequisicaoAsync(requisicao, id, cancellationToken);
        if (erro is not null)
        {
            return BadRequest(erro);
        }

        await using var transacao = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        acompanhamento.Nome = requisicao.Nome.Trim();
        acompanhamento.EstaAtivo = requisicao.EstaAtivo;
        acompanhamento.EstaDisponivel = requisicao.EstaDisponivel;
        acompanhamento.TipoSelecao = ConverterTipo(requisicao.TipoSelecao);
        acompanhamento.GrupoExclusivo = NormalizarOpcional(requisicao.GrupoExclusivo);
        acompanhamento.AtualizadoEm = DateTimeOffset.UtcNow;
        await ConfigurarGruposAsync(acompanhamento, requisicao.Grupos, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);

        return Ok(await ObterRespostaAsync(id, cancellationToken));
    }

    [HttpPatch("{id:guid}/disponibilidade")]
    public async Task<ActionResult<StatusAcompanhamentoAdminResposta>> AlterarDisponibilidade(
        Guid id,
        [FromBody] DisponibilidadeAdminRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var acompanhamento = await _dbContext.Acompanhamentos
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (acompanhamento is null)
        {
            return NotFound();
        }

        acompanhamento.EstaDisponivel = requisicao.EstaDisponivel;
        acompanhamento.AtualizadoEm = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new StatusAcompanhamentoAdminResposta
        {
            Id = acompanhamento.Id,
            EstaDisponivel = acompanhamento.EstaDisponivel,
            EstaAtivo = acompanhamento.EstaAtivo
        });
    }

    [HttpPatch("{id:guid}/ativacao")]
    public async Task<ActionResult<StatusAcompanhamentoAdminResposta>> AlterarAtivacao(
        Guid id,
        [FromBody] AtivacaoAdminRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var acompanhamento = await _dbContext.Acompanhamentos
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (acompanhamento is null)
        {
            return NotFound();
        }

        acompanhamento.EstaAtivo = requisicao.EstaAtivo;
        acompanhamento.AtualizadoEm = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new StatusAcompanhamentoAdminResposta
        {
            Id = acompanhamento.Id,
            EstaDisponivel = acompanhamento.EstaDisponivel,
            EstaAtivo = acompanhamento.EstaAtivo
        });
    }

    private async Task<AcompanhamentoAdminResposta?> ObterRespostaAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Acompanhamentos
            .AsNoTracking()
            .Where(acompanhamento => acompanhamento.Id == id)
            .Select(acompanhamento => new AcompanhamentoAdminResposta
            {
                Id = acompanhamento.Id,
                Nome = acompanhamento.Nome,
                EstaAtivo = acompanhamento.EstaAtivo,
                EstaDisponivel = acompanhamento.EstaDisponivel,
                TipoSelecao = acompanhamento.TipoSelecao.ToString().ToUpperInvariant(),
                GrupoExclusivo = acompanhamento.GrupoExclusivo,
                Grupos = acompanhamento.GruposAcompanhamentoItens
                    .OrderBy(item => item.GrupoAcompanhamento.Nome)
                    .Select(item => new GrupoAcompanhamentoVinculoAdminResposta
                    {
                        GrupoAcompanhamentoId = item.GrupoAcompanhamentoId,
                        Nome = item.GrupoAcompanhamento.Nome,
                        Codigo = item.GrupoAcompanhamento.Codigo,
                        Obrigatorio = item.Obrigatorio,
                        OrdemExibicao = item.OrdemExibicao
                    })
                    .ToList(),
                DataAtualizacao = acompanhamento.AtualizadoEm
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<object?> ValidarRequisicaoAsync(
        AcompanhamentoAdminSalvarRequisicao requisicao,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(requisicao.Nome) ||
            requisicao.Nome.Trim().Length is < 2 or > 120)
        {
            erros["nome"] = new[] { "O nome deve ter entre 2 e 120 caracteres." };
        }

        if (!TipoValido(requisicao.TipoSelecao))
        {
            erros["tipoSelecao"] = new[] { "Tipo de selecao invalido." };
        }

        if (requisicao.Grupos.Select(grupo => grupo.GrupoAcompanhamentoId).Distinct().Count() !=
            requisicao.Grupos.Count)
        {
            erros["grupos"] = new[] { "Nao repita o mesmo grupo." };
        }

        if (requisicao.Grupos.Any(grupo => grupo.OrdemExibicao < 0))
        {
            erros["ordemExibicao"] = new[] { "A ordem deve ser maior ou igual a zero." };
        }

        var grupoIds = requisicao.Grupos.Select(grupo => grupo.GrupoAcompanhamentoId).ToList();
        var gruposValidos = await _dbContext.GruposAcompanhamento
            .AsNoTracking()
            .CountAsync(grupo => grupoIds.Contains(grupo.Id) && grupo.EstaAtivo, cancellationToken);

        if (gruposValidos != grupoIds.Distinct().Count())
        {
            erros["grupos"] = new[] { "Informe apenas grupos ativos e existentes." };
        }

        if (!string.IsNullOrWhiteSpace(requisicao.Nome))
        {
            var nome = requisicao.Nome.Trim().ToLowerInvariant();
            var duplicado = await _dbContext.Acompanhamentos
                .AsNoTracking()
                .AnyAsync(acompanhamento =>
                    acompanhamento.Nome.ToLower() == nome &&
                    (!idIgnorado.HasValue || acompanhamento.Id != idIgnorado.Value),
                    cancellationToken);

            if (duplicado)
            {
                erros["nome"] = new[] { "Ja existe um acompanhamento com esse nome." };
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

    private async Task ConfigurarGruposAsync(
        Acompanhamento acompanhamento,
        IReadOnlyCollection<GrupoAcompanhamentoVinculoAdminRequisicao> grupos,
        CancellationToken cancellationToken)
    {
        var existentes = await _dbContext.GruposAcompanhamentoItens
            .Where(item => item.AcompanhamentoId == acompanhamento.Id)
            .ToListAsync(cancellationToken);

        _dbContext.GruposAcompanhamentoItens.RemoveRange(existentes);

        foreach (var grupo in grupos)
        {
            _dbContext.GruposAcompanhamentoItens.Add(new GrupoAcompanhamentoItem
            {
                AcompanhamentoId = acompanhamento.Id,
                GrupoAcompanhamentoId = grupo.GrupoAcompanhamentoId,
                Obrigatorio = grupo.Obrigatorio,
                OrdemExibicao = grupo.OrdemExibicao
            });
        }
    }

    private static TipoSelecaoAcompanhamento ConverterTipo(string tipo)
    {
        return Enum.Parse<TipoSelecaoAcompanhamento>(NormalizarTipo(tipo), true);
    }

    private static string NormalizarTipo(string tipo)
    {
        return tipo?.Trim().ToUpperInvariant() switch
        {
            "EXCLUSIVA" => nameof(TipoSelecaoAcompanhamento.Exclusiva),
            _ => nameof(TipoSelecaoAcompanhamento.Multipla)
        };
    }

    private static bool TipoValido(string tipo)
    {
        return tipo?.Trim().ToUpperInvariant() is "MULTIPLA" or "EXCLUSIVA";
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

public class AcompanhamentoAdminResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool EstaAtivo { get; set; }
    public bool EstaDisponivel { get; set; }
    public string TipoSelecao { get; set; } = "MULTIPLA";
    public string? GrupoExclusivo { get; set; }
    public IReadOnlyList<GrupoAcompanhamentoVinculoAdminResposta> Grupos { get; set; } = Array.Empty<GrupoAcompanhamentoVinculoAdminResposta>();
    public DateTimeOffset DataAtualizacao { get; set; }
}

public class AcompanhamentoAdminSalvarRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public bool EstaAtivo { get; set; } = true;
    public bool EstaDisponivel { get; set; } = true;
    public string TipoSelecao { get; set; } = "MULTIPLA";
    public string? GrupoExclusivo { get; set; }
    public List<GrupoAcompanhamentoVinculoAdminRequisicao> Grupos { get; set; } = new();
}

public class GrupoAcompanhamentoVinculoAdminRequisicao
{
    public Guid GrupoAcompanhamentoId { get; set; }
    public bool Obrigatorio { get; set; }
    public int OrdemExibicao { get; set; }
}

public class GrupoAcompanhamentoVinculoAdminResposta : GrupoAcompanhamentoVinculoAdminRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
}

public class StatusAcompanhamentoAdminResposta
{
    public Guid Id { get; set; }
    public bool EstaDisponivel { get; set; }
    public bool EstaAtivo { get; set; }
}
