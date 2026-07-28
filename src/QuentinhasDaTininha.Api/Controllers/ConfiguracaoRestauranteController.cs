using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.DTOs;
using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/configuracao-restaurante")]
public class ConfiguracaoRestauranteController : ControllerBase
{
    private readonly IServicoConfiguracaoRestaurante _servicoConfiguracaoRestaurante;

    public ConfiguracaoRestauranteController(
        IServicoConfiguracaoRestaurante servicoConfiguracaoRestaurante)
    {
        _servicoConfiguracaoRestaurante = servicoConfiguracaoRestaurante;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracaoRestauranteResposta>> Obter(
        CancellationToken cancellationToken)
    {
        var configuracao = await _servicoConfiguracaoRestaurante.ObterAsync(
            cancellationToken);

        if (configuracao is null)
        {
            return NotFound();
        }

        return Ok(configuracao);
    }

    [HttpPut]
    public async Task<ActionResult<ConfiguracaoRestauranteResposta>> Atualizar(
        [FromBody] ConfiguracaoRestauranteAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var configuracao = await _servicoConfiguracaoRestaurante.AtualizarAsync(
                requisicao,
                cancellationToken);

            return Ok(configuracao);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return Conflict(new { mensagem = excecao.Message });
        }
    }
}
