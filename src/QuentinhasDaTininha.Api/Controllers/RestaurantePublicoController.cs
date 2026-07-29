using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/restaurante")]
public class RestaurantePublicoController : ControllerBase
{
    private readonly IServicoCardapioDiaPublico _servicoCardapioDiaPublico;

    public RestaurantePublicoController(
        IServicoCardapioDiaPublico servicoCardapioDiaPublico)
    {
        _servicoCardapioDiaPublico = servicoCardapioDiaPublico;
    }

    [HttpGet("status")]
    public async Task<ActionResult<RestauranteStatusPublicoResposta>> ObterStatus(
        CancellationToken cancellationToken)
    {
        return Ok(await _servicoCardapioDiaPublico.ObterStatusRestauranteAsync(
            cancellationToken));
    }
}
