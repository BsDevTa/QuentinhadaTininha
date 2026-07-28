using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Pratos.DTOs;
using QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;
using ArquivoArmazenamentoUploadRequisicao =
    QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs.ArquivoUploadRequisicao;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pratos")]
public class PratosController : ControllerBase
{
    private readonly IServicoPrato _servicoPrato;
    private readonly IServicoImagemPrato _servicoImagemPrato;

    public PratosController(
        IServicoPrato servicoPrato,
        IServicoImagemPrato servicoImagemPrato)
    {
        _servicoPrato = servicoPrato;
        _servicoImagemPrato = servicoImagemPrato;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PratoResumoResposta>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] Guid? categoriaId,
        [FromQuery] bool? ativo,
        [FromQuery] bool? disponivel,
        CancellationToken cancellationToken)
    {
        var pratos = await _servicoPrato.ListarAsync(
            busca,
            categoriaId,
            ativo,
            disponivel,
            cancellationToken);

        return Ok(pratos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PratoResposta>> ObterPorId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var prato = await _servicoPrato.ObterPorIdAsync(id, cancellationToken);

        if (prato is null)
        {
            return NotFound();
        }

        return Ok(prato);
    }

    [HttpPost]
    public async Task<ActionResult<PratoResposta>> Criar(
        [FromBody] PratoCriacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var prato = await _servicoPrato.CriarAsync(requisicao, cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = prato.Id },
                prato);
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
    public async Task<ActionResult<PratoResposta>> Atualizar(
        Guid id,
        [FromBody] PratoAtualizacaoRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisição é obrigatória." });
        }

        try
        {
            var prato = await _servicoPrato.AtualizarAsync(id, requisicao, cancellationToken);

            if (prato is null)
            {
                return NotFound();
            }

            return Ok(prato);
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
            var excluido = await _servicoPrato.ExcluirAsync(id, cancellationToken);

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

    [HttpPut("{id}/imagem")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> AtualizarImagem(
        Guid id,
        [FromForm] ArquivoUploadRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao?.Arquivo is null)
        {
            return BadRequest(new { mensagem = "A imagem é obrigatória." });
        }

        try
        {
            await using var conteudo = requisicao.Arquivo.OpenReadStream();
            var urlImagem = await _servicoImagemPrato.AtualizarImagemAsync(
                id,
                new ArquivoArmazenamentoUploadRequisicao
                {
                    Conteudo = conteudo,
                    NomeArquivo = requisicao.Arquivo.FileName,
                    TipoConteudo = requisicao.Arquivo.ContentType,
                    Tamanho = requisicao.Arquivo.Length
                },
                cancellationToken);

            if (urlImagem is null)
            {
                return NotFound();
            }

            return Ok(new { imagemUrl = urlImagem });
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return Conflict(new { mensagem = excecao.Message });
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensagem = "Não foi possível processar a imagem." });
        }
    }

    [HttpDelete("{id:guid}/imagem")]
    public async Task<IActionResult> RemoverImagem(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var removido = await _servicoImagemPrato.RemoverImagemAsync(
                id,
                cancellationToken);

            if (!removido)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException excecao)
        {
            return Conflict(new { mensagem = excecao.Message });
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensagem = "Não foi possível processar a imagem." });
        }
    }
}

public class ArquivoUploadRequisicao
{
    public IFormFile Arquivo { get; set; } = null!;
}
