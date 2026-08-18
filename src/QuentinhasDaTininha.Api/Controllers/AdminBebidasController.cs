using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Bebidas.DTOs;
using QuentinhasDaTininha.Aplicacao.Bebidas.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/bebidas")]
public class AdminBebidasController : ControllerBase
{
    private readonly IServicoBebida _servicoBebida;

    public AdminBebidasController(IServicoBebida servicoBebida)
    {
        _servicoBebida = servicoBebida;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BebidaResposta>>> Listar(
        CancellationToken cancellationToken)
    {
        return Ok(await _servicoBebida.ListarAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<BebidaResposta>> Criar(
        [FromBody] BebidaSalvarRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var bebida = await _servicoBebida.CriarAsync(requisicao, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = bebida.Id }, bebida);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BebidaResposta>> Obter(
        Guid id,
        CancellationToken cancellationToken)
    {
        var bebida = await _servicoBebida.ObterPorIdAsync(id, cancellationToken);
        return bebida is null ? NotFound() : Ok(bebida);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BebidaResposta>> Atualizar(
        Guid id,
        [FromBody] BebidaSalvarRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var bebida = await _servicoBebida.AtualizarAsync(id, requisicao, cancellationToken);
        return bebida is null ? NotFound() : Ok(bebida);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        return await _servicoBebida.ExcluirAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
