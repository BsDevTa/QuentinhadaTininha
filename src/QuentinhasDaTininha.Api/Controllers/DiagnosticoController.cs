using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/diagnostico")]
public class DiagnosticoController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public DiagnosticoController(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet("deploy")]
    public ActionResult ObterDeploy()
    {
        var assembly = typeof(DiagnosticoController).Assembly;
        var versaoAssembly = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        return Ok(new
        {
            Aplicacao = "QuentinhasDaTininha.Api",
            Ambiente = _environment.EnvironmentName,
            Versao = ObterPrimeiroValorConfigurado(
                "APP_VERSION",
                "RENDER_GIT_COMMIT",
                "COMMIT_SHA") ?? versaoAssembly ?? "desconhecida",
            SeedCardapioPublico = "somente-se-vazio",
            GeradoEmUtc = DateTimeOffset.UtcNow
        });
    }

    private string? ObterPrimeiroValorConfigurado(params string[] chaves)
    {
        return chaves
            .Select(chave => _configuration[chave])
            .FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor));
    }
}
