using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Categorias.DTOs;
using QuentinhasDaTininha.Aplicacao.Categorias.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Categorias.Servicos;

public class ServicoCategoria : IServicoCategoria
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoCategoria(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CategoriaResposta>> ListarAsync(
        string? busca,
        bool? ativo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaNormalizada = busca.Trim().ToLowerInvariant();
            query = query.Where(categoria => categoria.Nome.ToLower().Contains(buscaNormalizada));
        }

        if (ativo.HasValue)
        {
            query = query.Where(categoria => categoria.EstaAtiva == ativo.Value);
        }

        return await query
            .OrderBy(categoria => categoria.Nome)
            .Select(categoria => MapearResposta(categoria))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Id == id)
            .Select(categoria => MapearResposta(categoria))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoriaResposta> CriarAsync(
        CategoriaCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var nome = NormalizarNome(requisicao.Nome);
        await ValidarNomeDuplicadoAsync(nome, null, cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = NormalizarTextoOpcional(requisicao.Descricao),
            EstaAtiva = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Categorias.AddAsync(categoria, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(categoria);
    }

    public async Task<CategoriaResposta?> AtualizarAsync(
        Guid id,
        CategoriaAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var categoria = await _dbContext.Categorias
            .FirstOrDefaultAsync(categoria => categoria.Id == id, cancellationToken);

        if (categoria is null)
        {
            return null;
        }

        var nome = NormalizarNome(requisicao.Nome);
        await ValidarNomeDuplicadoAsync(nome, id, cancellationToken);

        categoria.Nome = nome;
        categoria.Descricao = NormalizarTextoOpcional(requisicao.Descricao);
        categoria.EstaAtiva = requisicao.Ativo;
        categoria.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(categoria);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var categoria = await _dbContext.Categorias
            .FirstOrDefaultAsync(categoria => categoria.Id == id, cancellationToken);

        if (categoria is null)
        {
            return false;
        }

        var possuiPratos = await _dbContext.Pratos
            .AsNoTracking()
            .AnyAsync(prato => prato.CategoriaId == id, cancellationToken);

        if (possuiPratos)
        {
            throw new InvalidOperationException(
                "A categoria não pode ser excluída porque possui pratos associados.");
        }

        _dbContext.Categorias.Remove(categoria);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidarNomeDuplicadoAsync(
        string nome,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        var nomeComparacao = nome.ToLowerInvariant();

        var nomeJaExiste = await _dbContext.Categorias
            .AsNoTracking()
            .AnyAsync(
                categoria =>
                    categoria.Nome.ToLower() == nomeComparacao &&
                    (!idIgnorado.HasValue || categoria.Id != idIgnorado.Value),
                cancellationToken);

        if (nomeJaExiste)
        {
            throw new InvalidOperationException("Já existe uma categoria com esse nome.");
        }
    }

    private static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.");
        }

        return nome.Trim();
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static CategoriaResposta MapearResposta(Categoria categoria)
    {
        return new CategoriaResposta
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Ativo = categoria.EstaAtiva
        };
    }
}
