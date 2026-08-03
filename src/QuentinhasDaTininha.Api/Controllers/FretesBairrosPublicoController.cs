using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/publico/fretes-bairros")]
public class FretesBairrosPublicoController : ControllerBase
{
    private readonly IServicoFreteBairro _servicoFreteBairro;

    public FretesBairrosPublicoController(IServicoFreteBairro servicoFreteBairro)
    {
        _servicoFreteBairro = servicoFreteBairro;
    }

    [HttpGet("consultar")]
    public async Task<ActionResult<ConsultaFreteBairroResposta>> ConsultarPorBairro(
        [FromQuery] string? bairro,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bairro))
        {
            return BadRequest(new { mensagem = "Bairro é obrigatório." });
        }

        try
        {
            var resposta = await _servicoFreteBairro.ConsultarPorBairroAsync(
                bairro,
                cancellationToken);

            return Ok(resposta);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }

    [HttpGet("consultar-por-cep")]
    public async Task<ActionResult<ConsultaFreteCepResposta>> ConsultarPorCep(
        [FromQuery] string? cep,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return BadRequest(new { mensagem = "Informe um CEP com 8 números." });
        }

        try
        {
            var resposta = await _servicoFreteBairro.ConsultarPorCepAsync(
                cep,
                cancellationToken);

            return Ok(resposta);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (KeyNotFoundException excecao)
        {
            return NotFound(new { mensagem = excecao.Message });
        }
        catch (TimeoutException excecao)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { mensagem = excecao.Message });
        }
    }
}
