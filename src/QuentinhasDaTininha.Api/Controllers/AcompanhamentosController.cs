using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Acompanhamentos.DTOs;
using QuentinhasDaTininha.Aplicacao.Acompanhamentos.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/acompanhamentos")]
public class AcompanhamentosController : ControllerBase
{
    private readonly IServicoAcompanhamento _servicoAcompanhamento;

    public AcompanhamentosController(IServicoAcompanhamento servicoAcompanhamento)
    {
        _servicoAcompanhamento = servicoAcompanhamento;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AcompanhamentoResposta>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        CancellationToken cancellationToken)
    {
        var acompanhamentos = await _servicoAcompanhamento.ListarAsync(
            busca,
            ativo,
            cancellationToken);

        return Ok(acompanhamentos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AcompanhamentoResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var acompanhamento = await _servicoAcompanhamento.ObterPorIdAsync(
            id,
            cancellationToken);

        if (acompanhamento is null)
        {
            return NotFound();
        }

        return Ok(acompanhamento);
    }

    [HttpPost]
    public async Task<ActionResult<AcompanhamentoResposta>> Criar(
        [FromBody] AcompanhamentoCriacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var acompanhamento = await _servicoAcompanhamento.CriarAsync(
                requisicao,
                cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = acompanhamento.Id },
                acompanhamento);
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
    public async Task<ActionResult<AcompanhamentoResposta>> Atualizar(
        Guid id,
        [FromBody] AcompanhamentoAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var acompanhamento = await _servicoAcompanhamento.AtualizarAsync(
                id,
                requisicao,
                cancellationToken);

            if (acompanhamento is null)
            {
                return NotFound();
            }

            return Ok(acompanhamento);
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
            var excluido = await _servicoAcompanhamento.ExcluirAsync(id, cancellationToken);

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
