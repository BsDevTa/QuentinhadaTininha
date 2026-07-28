using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fechamentos-excepcionais")]
public class FechamentosExcepcionaisController : ControllerBase
{
    private readonly IServicoFechamentoExcepcional _servicoFechamentoExcepcional;

    public FechamentosExcepcionaisController(
        IServicoFechamentoExcepcional servicoFechamentoExcepcional)
    {
        _servicoFechamentoExcepcional = servicoFechamentoExcepcional;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FechamentoExcepcionalResposta>>> Listar(
        [FromQuery] DateOnly? dataInicial,
        [FromQuery] DateOnly? dataFinal,
        CancellationToken cancellationToken)
    {
        var fechamentos = await _servicoFechamentoExcepcional.ListarAsync(
            dataInicial,
            dataFinal,
            cancellationToken);

        return Ok(fechamentos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FechamentoExcepcionalResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var fechamento = await _servicoFechamentoExcepcional.ObterPorIdAsync(
            id,
            cancellationToken);

        if (fechamento is null)
        {
            return NotFound();
        }

        return Ok(fechamento);
    }

    [HttpPost]
    public async Task<ActionResult<FechamentoExcepcionalResposta>> Criar(
        [FromBody] FechamentoExcepcionalCriacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var fechamento = await _servicoFechamentoExcepcional.CriarAsync(
                requisicao,
                cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = fechamento.Id },
                fechamento);
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
    public async Task<ActionResult<FechamentoExcepcionalResposta>> Atualizar(
        Guid id,
        [FromBody] FechamentoExcepcionalAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var fechamento = await _servicoFechamentoExcepcional.AtualizarAsync(
                id,
                requisicao,
                cancellationToken);

            if (fechamento is null)
            {
                return NotFound();
            }

            return Ok(fechamento);
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
            var excluido = await _servicoFechamentoExcepcional.ExcluirAsync(
                id,
                cancellationToken);

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
