using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly IServicoAutenticacao _servicoAutenticacao;

    public AutenticacaoController(IServicoAutenticacao servicoAutenticacao)
    {
        _servicoAutenticacao = servicoAutenticacao;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResposta>> Login(
        [FromBody] LoginRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null ||
            string.IsNullOrWhiteSpace(requisicao.Email) ||
            string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return BadRequest("Email e senha são obrigatórios.");
        }

        var resposta = await _servicoAutenticacao.AutenticarAsync(requisicao, cancellationToken);

        if (resposta is null)
        {
            return Unauthorized("Email ou senha inválidos.");
        }

        return Ok(resposta);
    }

    [HttpGet("validar-token")]
    [Authorize]
    public IActionResult ValidarToken()
    {
        return Ok(new
        {
            autenticado = true,
            usuarioId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue(ClaimTypes.NameIdentifier),
            nome = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ??
                User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
                User.FindFirstValue(ClaimTypes.Email),
            perfil = User.FindFirstValue(ClaimTypes.Role) ??
                User.FindFirstValue("role")
        });
    }
}
