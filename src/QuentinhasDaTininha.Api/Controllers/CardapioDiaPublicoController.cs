using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/cardapio")]
public class CardapioDiaPublicoController : ControllerBase
{
    private readonly IServicoCardapioDiaPublico _servicoCardapioDiaPublico;

    public CardapioDiaPublicoController(
        IServicoCardapioDiaPublico servicoCardapioDiaPublico)
    {
        _servicoCardapioDiaPublico = servicoCardapioDiaPublico;
    }

    [HttpGet("hoje")]
    public async Task<ActionResult<CardapioDiaPublicoResposta>> ObterHoje(
        CancellationToken cancellationToken)
    {
        return Ok(await _servicoCardapioDiaPublico.ObterHojeAsync(cancellationToken));
    }

    [HttpGet("dia/{diaSemana:int}")]
    public async Task<ActionResult<CardapioDiaPublicoResposta>> ObterPorDia(
        int diaSemana,
        CancellationToken cancellationToken)
    {
        if (diaSemana is < 1 or > 7)
        {
            return BadRequest(new { mensagem = "O dia da semana deve estar entre 1 e 7." });
        }

        return Ok(await _servicoCardapioDiaPublico.ObterPorDiaAsync(
            diaSemana,
            cancellationToken));
    }
}
