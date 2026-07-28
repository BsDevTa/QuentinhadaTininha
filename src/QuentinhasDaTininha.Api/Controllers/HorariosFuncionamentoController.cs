using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/horarios-funcionamento")]
public class HorariosFuncionamentoController : ControllerBase
{
    private readonly IServicoHorarioFuncionamento _servicoHorarioFuncionamento;

    public HorariosFuncionamentoController(
        IServicoHorarioFuncionamento servicoHorarioFuncionamento)
    {
        _servicoHorarioFuncionamento = servicoHorarioFuncionamento;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HorarioFuncionamentoResposta>>> Listar(
        CancellationToken cancellationToken)
    {
        var horarios = await _servicoHorarioFuncionamento.ListarAsync(cancellationToken);

        return Ok(horarios);
    }

    [HttpPut("{diaSemana}")]
    public async Task<ActionResult<IReadOnlyList<HorarioFuncionamentoResposta>>> SubstituirDia(
        DiaSemana diaSemana,
        [FromBody] IReadOnlyCollection<HorarioFuncionamentoRequisicao>? horarios,
        CancellationToken cancellationToken)
    {
        if (horarios is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var horariosAtualizados =
                await _servicoHorarioFuncionamento.SubstituirDiaAsync(
                    diaSemana,
                    horarios,
                    cancellationToken);

            return Ok(horariosAtualizados);
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
            var excluido = await _servicoHorarioFuncionamento.ExcluirAsync(
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
