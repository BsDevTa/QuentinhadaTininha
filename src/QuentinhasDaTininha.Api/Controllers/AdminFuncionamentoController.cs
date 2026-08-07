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
[Route("api/admin/funcionamento")]
public class AdminFuncionamentoController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IControleCacheCardapioPublico _controleCacheCardapioPublico;

    public AdminFuncionamentoController(
        QuentinhasDaTininhaDbContext dbContext,
        IControleCacheCardapioPublico controleCacheCardapioPublico)
    {
        _dbContext = dbContext;
        _controleCacheCardapioPublico = controleCacheCardapioPublico;
    }

    [HttpGet]
    public async Task<ActionResult<FuncionamentoAdminResposta>> Obter(
        CancellationToken cancellationToken)
    {
        var configuracao = await ObterOuCriarConfiguracaoAsync(cancellationToken);
        return Ok(Mapear(configuracao));
    }

    [HttpPut]
    public async Task<ActionResult<FuncionamentoAdminResposta>> Atualizar(
        [FromBody] FuncionamentoAdminAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(CriarErro("requisicao", "A requisicao e obrigatoria."));
        }

        if ((requisicao.MensagemStatus?.Length ?? 0) > 250 ||
            (requisicao.HorarioFuncionamento?.Length ?? 0) > 160)
        {
            return BadRequest(CriarErro("mensagemStatus", "Mensagem ou horario muito longos."));
        }

        var configuracao = await ObterOuCriarConfiguracaoAsync(cancellationToken);
        configuracao.ModoFuncionamento = requisicao.EstaAberto
            ? ModoFuncionamento.AbertoManualmente
            : ModoFuncionamento.FechadoManualmente;
        configuracao.AceitaPedidos = requisicao.EstaAberto;
        configuracao.MensagemAberto = requisicao.EstaAberto
            ? NormalizarOpcional(requisicao.MensagemStatus)
            : configuracao.MensagemAberto;
        configuracao.MensagemFechado = requisicao.EstaAberto
            ? configuracao.MensagemFechado
            : NormalizarOpcional(requisicao.MensagemStatus);
        configuracao.HorarioFuncionamento = NormalizarOpcional(requisicao.HorarioFuncionamento);
        configuracao.EstaAtivo = true;
        configuracao.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCacheCardapioPublico.Invalidar();
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
            MensagemAberto = "Estamos atendendo normalmente.",
            MensagemFechado = "Restaurante fechado no momento.",
            HorarioFuncionamento = "Segunda a sabado, das 10h as 14h",
            CriadoEm = DateTimeOffset.UtcNow,
            AtualizadoEm = DateTimeOffset.UtcNow
        };
        await _dbContext.ConfiguracoesRestaurante.AddAsync(configuracao, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return configuracao;
    }

    private static FuncionamentoAdminResposta Mapear(ConfiguracaoRestaurante configuracao)
    {
        var estaAberto = configuracao.EstaAtivo &&
            configuracao.AceitaPedidos &&
            configuracao.ModoFuncionamento != ModoFuncionamento.FechadoManualmente;

        return new FuncionamentoAdminResposta
        {
            EstaAberto = estaAberto,
            MensagemStatus = estaAberto
                ? configuracao.MensagemAberto ?? "Estamos atendendo normalmente."
                : configuracao.MensagemFechado ?? "Restaurante fechado no momento.",
            HorarioFuncionamento = configuracao.HorarioFuncionamento ?? "Segunda a sabado, das 10h as 14h",
            AberturaManual = configuracao.ModoFuncionamento is ModoFuncionamento.AbertoManualmente or ModoFuncionamento.FechadoManualmente,
            DataUltimaAlteracao = configuracao.AtualizadoEm
        };
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

public class FuncionamentoAdminResposta
{
    public bool EstaAberto { get; set; }
    public string MensagemStatus { get; set; } = string.Empty;
    public string HorarioFuncionamento { get; set; } = string.Empty;
    public bool AberturaManual { get; set; }
    public DateTimeOffset DataUltimaAlteracao { get; set; }
}

public class FuncionamentoAdminAtualizacaoRequisicao
{
    public bool EstaAberto { get; set; }
    public string MensagemStatus { get; set; } = string.Empty;
    public string HorarioFuncionamento { get; set; } = string.Empty;
}
