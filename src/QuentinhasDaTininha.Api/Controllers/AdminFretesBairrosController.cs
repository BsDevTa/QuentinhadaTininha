using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/fretes-bairros")]
public class AdminFretesBairrosController : ControllerBase
{
    private readonly IServicoFreteBairro _servicoFreteBairro;

    public AdminFretesBairrosController(IServicoFreteBairro servicoFreteBairro)
    {
        _servicoFreteBairro = servicoFreteBairro;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FreteBairroResposta>>> Listar(
        [FromQuery] string? bairro,
        [FromQuery] bool? ativo,
        CancellationToken cancellationToken)
    {
        var fretes = await _servicoFreteBairro.ListarAsync(
            bairro,
            ativo,
            cancellationToken);

        return Ok(fretes);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FreteBairroResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var frete = await _servicoFreteBairro.ObterPorIdAsync(id, cancellationToken);
        return frete is null ? NotFound() : Ok(frete);
    }

    [HttpPost]
    public async Task<ActionResult<FreteBairroResposta>> Criar(
        [FromBody] FreteBairroSalvarRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var frete = await _servicoFreteBairro.CriarAsync(
                requisicao,
                cancellationToken);

            return CreatedAtAction(nameof(ObterPorId), new { id = frete.Id }, frete);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return Conflict(new { mensagem = excecao.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Nao foi possivel salvar o frete por conflito nos dados cadastrados." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FreteBairroResposta>> Atualizar(
        Guid id,
        [FromBody] FreteBairroSalvarRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var frete = await _servicoFreteBairro.AtualizarAsync(
                id,
                requisicao,
                cancellationToken);

            return frete is null ? NotFound() : Ok(frete);
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return Conflict(new { mensagem = excecao.Message });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Nao foi possivel salvar o frete por conflito nos dados cadastrados." });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<FreteBairroResposta>> AlterarStatus(
        Guid id,
        [FromBody] FreteBairroStatusRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var frete = await _servicoFreteBairro.AlterarStatusAsync(
            id,
            requisicao.Ativo,
            cancellationToken);

        return frete is null ? NotFound() : Ok(frete);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        var excluido = await _servicoFreteBairro.ExcluirAsync(id, cancellationToken);
        return excluido ? NoContent() : NotFound();
    }
}
