using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/ceps-salvador")]
public class AdminCepsSalvadorController : ControllerBase
{
    private readonly IServicoCepSalvador _servicoCepSalvador;

    public AdminCepsSalvadorController(IServicoCepSalvador servicoCepSalvador)
    {
        _servicoCepSalvador = servicoCepSalvador;
    }

    [HttpPost("importar")]
    public async Task<ActionResult<CepSalvadorImportacaoResposta>> Importar(
        [FromBody] List<CepSalvadorImportacaoItem>? itens,
        CancellationToken cancellationToken)
    {
        if (itens is null)
        {
            return BadRequest(new { mensagem = "Informe os CEPs para importacao." });
        }

        var resposta = await _servicoCepSalvador.ImportarAsync(
            itens,
            cancellationToken);

        return Ok(resposta);
    }
}
