using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/publico/disponibilidade")]
public class DisponibilidadePublicaController : ControllerBase
{
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public DisponibilidadePublicaController(
        IServicoDisponibilidadePedido servicoDisponibilidadePedido)
    {
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
    }

    [HttpGet]
    public async Task<ActionResult<DisponibilidadePublicaResposta>> Listar(
        [FromQuery] DateOnly? dataInicial,
        [FromQuery] DateOnly? dataFinal,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _servicoDisponibilidadePedido.ListarPublicaAsync(
                dataInicial,
                dataFinal,
                cancellationToken));
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }
}
