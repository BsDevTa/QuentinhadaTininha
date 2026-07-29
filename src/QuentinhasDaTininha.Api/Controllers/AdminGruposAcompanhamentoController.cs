using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/grupos-acompanhamento")]
public class AdminGruposAcompanhamentoController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public AdminGruposAcompanhamentoController(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GrupoAcompanhamentoAdminResposta>>> Listar(
        CancellationToken cancellationToken)
    {
        var grupos = await _dbContext.GruposAcompanhamento
            .AsNoTracking()
            .OrderBy(grupo => grupo.Nome)
            .Select(grupo => new GrupoAcompanhamentoAdminResposta
            {
                Id = grupo.Id,
                Nome = grupo.Nome,
                Codigo = grupo.Codigo,
                EstaAtivo = grupo.EstaAtivo,
                QuantidadeAcompanhamentos = grupo.Itens.Count(item => item.Acompanhamento.EstaAtivo)
            })
            .ToListAsync(cancellationToken);

        return Ok(grupos);
    }
}

public class GrupoAcompanhamentoAdminResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool EstaAtivo { get; set; }
    public int QuantidadeAcompanhamentos { get; set; }
}
