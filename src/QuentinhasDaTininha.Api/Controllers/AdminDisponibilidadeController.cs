using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/disponibilidade")]
public class AdminDisponibilidadeController : ControllerBase
{
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public AdminDisponibilidadeController(
        IServicoDisponibilidadePedido servicoDisponibilidadePedido)
    {
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DisponibilidadeDataResposta>>> Listar(
        [FromQuery] DateOnly? dataInicial,
        [FromQuery] DateOnly? dataFinal,
        CancellationToken cancellationToken)
    {
        try
        {
            var disponibilidade = await _servicoDisponibilidadePedido.ListarAsync(
                dataInicial,
                dataFinal,
                cancellationToken);

            return Ok(disponibilidade);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }

    [HttpGet("{data}")]
    public async Task<ActionResult<DisponibilidadeDataResposta>> ObterPorData(
        DateOnly data,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _servicoDisponibilidadePedido.ObterPorDataAsync(
                data,
                cancellationToken));
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }

    [HttpPost("{data}/liberar")]
    public async Task<ActionResult<DisponibilidadeDataResposta>> LiberarData(
        DateOnly data,
        [FromBody] DisponibilidadeDataMotivoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var disponibilidade = await _servicoDisponibilidadePedido.LiberarDataAsync(
                data,
                requisicao?.Motivo,
                cancellationToken);

            return Ok(disponibilidade);
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

    [HttpPost("{data}/bloquear")]
    public async Task<ActionResult<DisponibilidadeDataResposta>> BloquearData(
        DateOnly data,
        [FromBody] DisponibilidadeDataMotivoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        try
        {
            var disponibilidade = await _servicoDisponibilidadePedido.BloquearDataAsync(
                data,
                requisicao?.Motivo,
                cancellationToken);

            return Ok(disponibilidade);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
    }

    [HttpPut("{data}/motivo")]
    public async Task<ActionResult<DisponibilidadeDataResposta>> AlterarMotivo(
        DateOnly data,
        [FromBody] DisponibilidadeDataMotivoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var disponibilidade = await _servicoDisponibilidadePedido.AlterarMotivoAsync(
                data,
                requisicao.Motivo,
                cancellationToken);

            return Ok(disponibilidade);
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
