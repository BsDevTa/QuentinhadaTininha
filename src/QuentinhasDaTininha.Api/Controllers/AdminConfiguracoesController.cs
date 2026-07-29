using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/configuracoes")]
public class AdminConfiguracoesController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public AdminConfiguracoesController(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracoesPublicasAdminResposta>> Obter(
        CancellationToken cancellationToken)
    {
        var configuracao = await ObterOuCriarConfiguracaoAsync(cancellationToken);
        return Ok(Mapear(configuracao));
    }

    [HttpPut]
    public async Task<ActionResult<ConfiguracoesPublicasAdminResposta>> Atualizar(
        [FromBody] ConfiguracoesPublicasAdminRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(CriarErro("requisicao", "A requisicao e obrigatoria."));
        }

        var erros = Validar(requisicao);
        if (erros.Count > 0)
        {
            return BadRequest(new
            {
                titulo = "Dados invalidos",
                mensagem = "Verifique os campos informados.",
                erros
            });
        }

        var configuracao = await ObterOuCriarConfiguracaoAsync(cancellationToken);
        configuracao.Nome = requisicao.Nome.Trim();
        configuracao.Whatsapp = NormalizarWhatsapp(requisicao.Whatsapp);
        configuracao.Telefone = configuracao.Whatsapp;
        configuracao.Instagram = NormalizarInstagram(requisicao.Instagram);
        configuracao.Endereco = NormalizarOpcional(requisicao.Endereco);
        configuracao.UrlLogotipo = NormalizarOpcional(requisicao.UrlLogo);
        configuracao.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Mapear(configuracao));
    }

    private async Task<ConfiguracaoRestaurante> ObterOuCriarConfiguracaoAsync(
        CancellationToken cancellationToken)
    {
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .OrderBy(item => item.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuracao is not null)
        {
            return configuracao;
        }

        configuracao = new ConfiguracaoRestaurante
        {
            Nome = "Quentinhas da Tininha",
            EstaAtivo = true,
            AceitaPedidos = true,
            CriadoEm = DateTimeOffset.UtcNow,
            AtualizadoEm = DateTimeOffset.UtcNow
        };
        await _dbContext.ConfiguracoesRestaurante.AddAsync(configuracao, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return configuracao;
    }

    private static Dictionary<string, string[]> Validar(ConfiguracoesPublicasAdminRequisicao requisicao)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(requisicao.Nome) ||
            requisicao.Nome.Trim().Length is < 2 or > 120)
        {
            erros["nome"] = new[] { "Informe o nome do restaurante." };
        }

        var whatsapp = NormalizarWhatsapp(requisicao.Whatsapp);
        if (string.IsNullOrWhiteSpace(whatsapp) || whatsapp.Length < 12 || whatsapp.Length > 13)
        {
            erros["whatsapp"] = new[] { "Informe um WhatsApp valido com DDD." };
        }

        if ((requisicao.Endereco?.Length ?? 0) > 250)
        {
            erros["endereco"] = new[] { "Endereco muito longo." };
        }

        if ((requisicao.UrlLogo?.Length ?? 0) > 500)
        {
            erros["urlLogo"] = new[] { "URL da logo muito longa." };
        }

        return erros;
    }

    private static ConfiguracoesPublicasAdminResposta Mapear(ConfiguracaoRestaurante configuracao)
    {
        return new ConfiguracoesPublicasAdminResposta
        {
            Nome = configuracao.Nome,
            Whatsapp = configuracao.Whatsapp,
            Instagram = configuracao.Instagram,
            Endereco = configuracao.Endereco,
            UrlLogo = configuracao.UrlLogotipo
        };
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static string? NormalizarInstagram(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var instagram = valor.Trim();
        if (instagram.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return instagram;
        }

        return instagram.StartsWith('@') ? instagram : $"@{instagram}";
    }

    private static string NormalizarWhatsapp(string? valor)
    {
        var digitos = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length == 11)
        {
            return $"55{digitos}";
        }

        return digitos;
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

public class ConfiguracoesPublicasAdminResposta
{
    public string Nome { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public string? Instagram { get; set; }
    public string? Endereco { get; set; }
    public string? UrlLogo { get; set; }
}

public class ConfiguracoesPublicasAdminRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Whatsapp { get; set; }
    public string? Instagram { get; set; }
    public string? Endereco { get; set; }
    public string? UrlLogo { get; set; }
}
