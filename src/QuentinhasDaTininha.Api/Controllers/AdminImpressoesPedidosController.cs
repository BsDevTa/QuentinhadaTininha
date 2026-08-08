using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/impressoes-pedidos")]
public class AdminImpressoesPedidosController : ControllerBase
{
    private readonly IServicoImpressaoPedido _servicoImpressaoPedido;

    public AdminImpressoesPedidosController(IServicoImpressaoPedido servicoImpressaoPedido)
    {
        _servicoImpressaoPedido = servicoImpressaoPedido;
    }

    [HttpGet("pendentes")]
    public async Task<ActionResult<IReadOnlyList<ImpressaoPedidoResposta>>> ListarPendentes(
        [FromQuery] int limite,
        CancellationToken cancellationToken)
    {
        var impressoes = await _servicoImpressaoPedido.ListarPendentesAsync(
            limite <= 0 ? 10 : limite,
            cancellationToken);

        return Ok(impressoes);
    }

    [HttpPost("{id:guid}/iniciar")]
    public async Task<ActionResult<ImpressaoPedidoResposta>> Iniciar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var impressao = await _servicoImpressaoPedido.IniciarAsync(id, cancellationToken);
        return impressao is null ? Conflict(new { mensagem = "Impressao nao esta disponivel para processamento." }) : Ok(impressao);
    }

    [HttpPost("{id:guid}/concluir")]
    public async Task<IActionResult> Concluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        var concluida = await _servicoImpressaoPedido.ConcluirAsync(id, cancellationToken);
        return concluida ? NoContent() : Conflict(new { mensagem = "Impressao nao esta em processamento." });
    }

    [HttpPost("{id:guid}/erro")]
    public async Task<IActionResult> RegistrarErro(
        Guid id,
        [FromBody] ImpressaoPedidoErroRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        var registrado = await _servicoImpressaoPedido.RegistrarErroAsync(
            id,
            requisicao?.Erro,
            cancellationToken);

        return registrado ? NoContent() : Conflict(new { mensagem = "Impressao nao esta em processamento." });
    }

    [HttpPost("pedidos/{pedidoId:guid}/reimprimir")]
    public async Task<ActionResult<ImpressaoPedidoResposta>> Reimprimir(
        Guid pedidoId,
        CancellationToken cancellationToken)
    {
        var impressao = await _servicoImpressaoPedido.CriarReimpressaoAsync(
            pedidoId,
            cancellationToken);

        return impressao is null ? NotFound() : CreatedAtAction(nameof(Iniciar), new { id = impressao.Id }, impressao);
    }
}
