using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;
using QuentinhasDaTininha.Dominio.Enumeracoes;

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

    [HttpPost("entrar")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResposta>> Entrar(
        [FromBody] LoginRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null ||
            string.IsNullOrWhiteSpace(requisicao.Email) ||
            string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return BadRequest("Email e senha sao obrigatorios.");
        }

        var resposta = await _servicoAutenticacao.AutenticarAsync(requisicao, cancellationToken);

        if (resposta is null)
        {
            return Unauthorized("E-mail ou senha invalidos.");
        }

        return Ok(resposta);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public Task<ActionResult<LoginResposta>> Login(
        [FromBody] LoginRequisicao? requisicao,
        CancellationToken cancellationToken)
    {
        return Entrar(requisicao, cancellationToken);
    }

    [HttpGet("sessao")]
    [Authorize]
    public ActionResult<SessaoUsuarioResposta> Sessao()
    {
        var usuarioId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(usuarioId, out var id))
        {
            return Unauthorized();
        }

        var perfilTexto = User.FindFirstValue(ClaimTypes.Role) ??
            User.FindFirstValue("role");
        var perfil = Enum.TryParse<PerfilUsuario>(perfilTexto, out var perfilUsuario)
            ? perfilUsuario
            : PerfilUsuario.Funcionario;

        return Ok(new SessaoUsuarioResposta
        {
            Autenticado = true,
            Usuario = new UsuarioAutenticadoDto
            {
                Id = id,
                Nome = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName) ??
                    User.FindFirstValue(ClaimTypes.Name) ??
                    string.Empty,
                Email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
                    User.FindFirstValue(ClaimTypes.Email) ??
                    string.Empty,
                Perfil = perfil
            }
        });
    }

    [HttpGet("validar-token")]
    [Authorize]
    public ActionResult<SessaoUsuarioResposta> ValidarToken()
    {
        return Sessao();
    }
}
