using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.Pedidos.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IServicoPedido _servicoPedido;

    public PedidosController(IServicoPedido servicoPedido)
    {
        _servicoPedido = servicoPedido;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoResposta>> Criar(
        [FromBody] PedidoCriacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var pedido = await _servicoPedido.CriarAsync(requisicao, cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = pedido.Id },
                pedido);
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PedidoResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pedido = await _servicoPedido.ObterPorIdAsync(id, cancellationToken);
        if (pedido is null)
        {
            return NotFound();
        }

        return Ok(pedido);
    }
}
