using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Categorias.DTOs;
using QuentinhasDaTininha.Aplicacao.Categorias.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly IServicoCategoria _servicoCategoria;

    public CategoriasController(IServicoCategoria servicoCategoria)
    {
        _servicoCategoria = servicoCategoria;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaResposta>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        CancellationToken cancellationToken)
    {
        var categorias = await _servicoCategoria.ListarAsync(busca, ativo, cancellationToken);

        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoriaResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var categoria = await _servicoCategoria.ObterPorIdAsync(id, cancellationToken);

        if (categoria is null)
        {
            return NotFound();
        }

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaResposta>> Criar(
        [FromBody] CategoriaCriacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var categoria = await _servicoCategoria.CriarAsync(requisicao, cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = categoria.Id },
                categoria);
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

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoriaResposta>> Atualizar(
        Guid id,
        [FromBody] CategoriaAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var categoria = await _servicoCategoria.AtualizarAsync(id, requisicao, cancellationToken);

            if (categoria is null)
            {
                return NotFound();
            }

            return Ok(categoria);
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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var excluido = await _servicoCategoria.ExcluirAsync(id, cancellationToken);

            if (!excluido)
            {
                return NotFound();
            }

            return NoContent();
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
