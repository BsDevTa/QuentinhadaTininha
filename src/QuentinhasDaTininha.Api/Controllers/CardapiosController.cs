using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;
using QuentinhasDaTininha.Aplicacao.Cardapios.Interfaces;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cardapios")]
public class CardapiosController : ControllerBase
{
    private readonly IServicoCardapio _servicoCardapio;

    public CardapiosController(IServicoCardapio servicoCardapio)
    {
        _servicoCardapio = servicoCardapio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CardapioDiaResposta>>> ListarTodos(
        CancellationToken cancellationToken)
    {
        var cardapios = await _servicoCardapio.ListarTodosAsync(cancellationToken);

        return Ok(cardapios);
    }

    [HttpGet("{diaSemana}")]
    public async Task<ActionResult<CardapioDiaResposta>> ObterPorDia(
        DiaSemana diaSemana,
        CancellationToken cancellationToken)
    {
        var cardapio = await _servicoCardapio.ObterPorDiaAsync(
            diaSemana,
            cancellationToken);

        if (cardapio is null)
        {
            return NotFound();
        }

        return Ok(cardapio);
    }

    [HttpPut("{diaSemana}")]
    public async Task<ActionResult<CardapioDiaResposta>> Atualizar(
        DiaSemana diaSemana,
        [FromBody] CardapioDiaAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var cardapio = await _servicoCardapio.AtualizarAsync(
                diaSemana,
                requisicao,
                cancellationToken);

            return Ok(cardapio);
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

    [HttpPatch("{diaSemana}/pratos/{pratoId:guid}/disponibilidade")]
    public async Task<ActionResult> AlterarDisponibilidadePrato(
        DiaSemana diaSemana,
        Guid pratoId,
        [FromBody] CardapioDiaPratoDisponibilidadeRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var alterado = await _servicoCardapio.AlterarDisponibilidadePratoAsync(
                diaSemana,
                pratoId,
                requisicao.Disponivel,
                cancellationToken);

            if (!alterado)
            {
                return NotFound();
            }

            return Ok();
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
