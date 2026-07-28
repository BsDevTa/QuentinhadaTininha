using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/publico/cardapio")]
public class CardapioPublicoController : ControllerBase
{
    private readonly IServicoCardapioPublico _servicoCardapioPublico;

    public CardapioPublicoController(IServicoCardapioPublico servicoCardapioPublico)
    {
        _servicoCardapioPublico = servicoCardapioPublico;
    }

    [HttpGet]
    public async Task<ActionResult<CardapioPublicoResposta>> Obter(
        [FromQuery] DateOnly? data,
        CancellationToken cancellationToken)
    {
        var cardapio = await _servicoCardapioPublico.ObterAsync(data, cancellationToken);

        return Ok(cardapio);
    }
}
