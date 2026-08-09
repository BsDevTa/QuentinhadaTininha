using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Qz.DTOs;
using QuentinhasDaTininha.Aplicacao.Qz.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/qz")]
public class AdminQzController : ControllerBase
{
    private readonly IServicoQzSigning _servicoQzSigning;

    public AdminQzController(IServicoQzSigning servicoQzSigning)
    {
        _servicoQzSigning = servicoQzSigning;
    }

    [AllowAnonymous]
    [HttpGet("certificado")]
    public ActionResult ObterCertificado()
    {
        try
        {
            return Content(
                _servicoQzSigning.ObterCertificado(),
                "text/plain",
                System.Text.Encoding.UTF8);
        }
        catch (InvalidOperationException excecao)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { mensagem = excecao.Message });
        }
    }

    [HttpPost("assinar")]
    public ActionResult<QzAssinaturaResposta> Assinar(
        [FromBody] QzAssinaturaRequisicao? requisicao)
    {
        if (requisicao is null)
        {
            return BadRequest(new { mensagem = "A requisicao de assinatura QZ e obrigatoria." });
        }

        try
        {
            return Ok(new QzAssinaturaResposta
            {
                Assinatura = _servicoQzSigning.Assinar(requisicao.Dados)
            });
        }
        catch (ArgumentException excecao)
        {
            return BadRequest(new { mensagem = excecao.Message });
        }
        catch (InvalidOperationException excecao)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { mensagem = excecao.Message });
        }
    }
}
