using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Bebidas.DTOs;
using QuentinhasDaTininha.Aplicacao.Bebidas.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Bebidas.Servicos;

public class ServicoBebida : IServicoBebida
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IControleCacheCardapioPublico _controleCache;

    public ServicoBebida(
        QuentinhasDaTininhaDbContext dbContext,
        IControleCacheCardapioPublico controleCache)
    {
        _dbContext = dbContext;
        _controleCache = controleCache;
    }

    public async Task<IReadOnlyList<BebidaResposta>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bebidas
            .AsNoTracking()
            .OrderBy(bebida => bebida.Nome)
            .Select(bebida => Mapear(bebida))
            .ToListAsync(cancellationToken);
    }

    public async Task<BebidaResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var bebida = await _dbContext.Bebidas
            .AsNoTracking()
            .FirstOrDefaultAsync(bebida => bebida.Id == id, cancellationToken);

        return bebida is null ? null : Mapear(bebida);
    }

    public async Task<BebidaResposta> CriarAsync(
        BebidaSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);
        Validar(requisicao);

        var agora = DateTimeOffset.UtcNow;
        var bebida = new Bebida
        {
            Nome = requisicao.Nome.Trim(),
            Descricao = NormalizarOpcional(requisicao.Descricao),
            Preco = requisicao.Preco,
            Ativa = requisicao.Ativa,
            ImagemUrl = NormalizarOpcional(requisicao.ImagemUrl),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Bebidas.AddAsync(bebida, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCache.Invalidar();

        return Mapear(bebida);
    }

    public async Task<BebidaResposta?> AtualizarAsync(
        Guid id,
        BebidaSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);
        Validar(requisicao);

        var bebida = await _dbContext.Bebidas
            .FirstOrDefaultAsync(bebida => bebida.Id == id, cancellationToken);

        if (bebida is null)
        {
            return null;
        }

        bebida.Nome = requisicao.Nome.Trim();
        bebida.Descricao = NormalizarOpcional(requisicao.Descricao);
        bebida.Preco = requisicao.Preco;
        bebida.Ativa = requisicao.Ativa;
        bebida.ImagemUrl = NormalizarOpcional(requisicao.ImagemUrl);
        bebida.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCache.Invalidar();

        return Mapear(bebida);
    }

    public async Task<bool> ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bebida = await _dbContext.Bebidas
            .FirstOrDefaultAsync(bebida => bebida.Id == id, cancellationToken);

        if (bebida is null)
        {
            return false;
        }

        var usadaEmPedido = await _dbContext.PedidosBebidas
            .AsNoTracking()
            .AnyAsync(item => item.BebidaId == id, cancellationToken);

        if (usadaEmPedido)
        {
            bebida.Ativa = false;
            bebida.AtualizadoEm = DateTimeOffset.UtcNow;
        }
        else
        {
            _dbContext.Bebidas.Remove(bebida);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _controleCache.Invalidar();
        return true;
    }

    private static void Validar(BebidaSalvarRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Nome))
        {
            throw new ArgumentException("Nome da bebida e obrigatorio.");
        }

        if (requisicao.Nome.Trim().Length > 120)
        {
            throw new ArgumentException("Nome da bebida deve ter no maximo 120 caracteres.");
        }

        if (requisicao.Preco <= 0)
        {
            throw new ArgumentException("Preco da bebida deve ser maior que zero.");
        }
    }

    private static string? NormalizarOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    private static BebidaResposta Mapear(Bebida bebida)
    {
        return new BebidaResposta
        {
            Id = bebida.Id,
            Nome = bebida.Nome,
            Descricao = bebida.Descricao,
            Preco = bebida.Preco,
            Ativa = bebida.Ativa,
            ImagemUrl = bebida.ImagemUrl,
            AtualizadoEm = bebida.AtualizadoEm
        };
    }
}
