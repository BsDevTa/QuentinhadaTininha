using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Pratos.DTOs;
using QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Pratos.Servicos;

public class ServicoPrato : IServicoPrato
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoPrato(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PratoResumoResposta>> ListarAsync(
        string? busca,
        Guid? categoriaId,
        bool? ativo,
        bool? disponivel,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Pratos
            .AsNoTracking()
            .Include(prato => prato.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaNormalizada = busca.Trim().ToLowerInvariant();
            query = query.Where(prato => prato.Nome.ToLower().Contains(buscaNormalizada));
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(prato => prato.CategoriaId == categoriaId.Value);
        }

        if (ativo.HasValue)
        {
            query = query.Where(prato => prato.EstaAtivo == ativo.Value);
        }

        if (disponivel.HasValue)
        {
            query = query.Where(prato => prato.EstaDisponivel == disponivel.Value);
        }

        return await query
            .OrderBy(prato => prato.Nome)
            .Select(prato => new PratoResumoResposta
            {
                Id = prato.Id,
                Nome = prato.Nome,
                Descricao = prato.Descricao,
                Preco = prato.Preco,
                Ativo = prato.EstaAtivo,
                Disponivel = prato.EstaDisponivel,
                ImagemUrl = prato.UrlImagem,
                CategoriaId = prato.CategoriaId,
                CategoriaNome = prato.Categoria.Nome
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PratoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var prato = await _dbContext.Pratos
            .AsNoTracking()
            .Include(prato => prato.Categoria)
            .Include(prato => prato.PratoAcompanhamentos)
                .ThenInclude(pratoAcompanhamento => pratoAcompanhamento.Acompanhamento)
            .FirstOrDefaultAsync(prato => prato.Id == id, cancellationToken);

        return prato is null ? null : MapearResposta(prato);
    }

    public async Task<PratoResposta> CriarAsync(
        PratoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var nome = NormalizarNome(requisicao.Nome);
        ValidarPreco(requisicao.Preco);

        await ValidarCategoriaAtivaAsync(requisicao.CategoriaId, cancellationToken);
        var acompanhamentoIds = await ValidarAcompanhamentosAsync(
            requisicao.AcompanhamentoIds,
            cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        var prato = new Prato
        {
            Nome = nome,
            Descricao = NormalizarTextoOpcional(requisicao.Descricao),
            Preco = requisicao.Preco,
            CategoriaId = requisicao.CategoriaId,
            EstaAtivo = true,
            EstaDisponivel = requisicao.Disponivel,
            UrlImagem = NormalizarTextoOpcional(requisicao.ImagemUrl),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        foreach (var acompanhamentoId in acompanhamentoIds)
        {
            prato.PratoAcompanhamentos.Add(new PratoAcompanhamento
            {
                PratoId = prato.Id,
                AcompanhamentoId = acompanhamentoId,
                EstaIncluido = true
            });
        }

        await _dbContext.Pratos.AddAsync(prato, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ObterPorIdAsync(prato.Id, cancellationToken) ??
            throw new InvalidOperationException("Não foi possível carregar o prato criado.");
    }

    public async Task<PratoResposta?> AtualizarAsync(
        Guid id,
        PratoAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var prato = await _dbContext.Pratos
            .Include(prato => prato.PratoAcompanhamentos)
            .FirstOrDefaultAsync(prato => prato.Id == id, cancellationToken);

        if (prato is null)
        {
            return null;
        }

        var nome = NormalizarNome(requisicao.Nome);
        ValidarPreco(requisicao.Preco);

        await ValidarCategoriaAtivaAsync(requisicao.CategoriaId, cancellationToken);
        var acompanhamentoIds = await ValidarAcompanhamentosAsync(
            requisicao.AcompanhamentoIds,
            cancellationToken);

        prato.Nome = nome;
        prato.Descricao = NormalizarTextoOpcional(requisicao.Descricao);
        prato.Preco = requisicao.Preco;
        prato.CategoriaId = requisicao.CategoriaId;
        prato.EstaAtivo = requisicao.Ativo;
        prato.EstaDisponivel = requisicao.Disponivel;
        prato.UrlImagem = NormalizarTextoOpcional(requisicao.ImagemUrl);
        prato.AtualizadoEm = DateTimeOffset.UtcNow;

        AtualizarAcompanhamentos(prato, acompanhamentoIds);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ObterPorIdAsync(id, cancellationToken);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var prato = await _dbContext.Pratos
            .FirstOrDefaultAsync(prato => prato.Id == id, cancellationToken);

        if (prato is null)
        {
            return false;
        }

        var associadoAoCardapio = await _dbContext.CardapiosDiaPratos
            .AsNoTracking()
            .AnyAsync(cardapioDiaPrato => cardapioDiaPrato.PratoId == id, cancellationToken);

        if (associadoAoCardapio)
        {
            throw new InvalidOperationException(
                "O prato não pode ser excluído porque está associado a um cardápio.");
        }

        _dbContext.Pratos.Remove(prato);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidarCategoriaAtivaAsync(
        Guid categoriaId,
        CancellationToken cancellationToken)
    {
        var categoria = await _dbContext.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == categoriaId, cancellationToken);

        if (categoria is null)
        {
            throw new InvalidOperationException("A categoria informada não existe.");
        }

        if (!categoria.EstaAtiva)
        {
            throw new InvalidOperationException("A categoria informada está inativa.");
        }
    }

    private async Task<IReadOnlyList<Guid>> ValidarAcompanhamentosAsync(
        ICollection<Guid>? acompanhamentoIds,
        CancellationToken cancellationToken)
    {
        var ids = (acompanhamentoIds ?? new List<Guid>()).ToList();

        if (ids.Count != ids.Distinct().Count())
        {
            throw new ArgumentException("Não é permitido informar acompanhamentos duplicados.");
        }

        if (ids.Count == 0)
        {
            return ids;
        }

        var acompanhamentos = await _dbContext.Acompanhamentos
            .AsNoTracking()
            .Where(acompanhamento => ids.Contains(acompanhamento.Id))
            .ToListAsync(cancellationToken);

        if (acompanhamentos.Count != ids.Count)
        {
            throw new InvalidOperationException(
                "Um ou mais acompanhamentos informados não existem.");
        }

        if (acompanhamentos.Any(acompanhamento => !acompanhamento.EstaAtivo))
        {
            throw new InvalidOperationException(
                "Um ou mais acompanhamentos informados estão inativos.");
        }

        return ids;
    }

    private static void AtualizarAcompanhamentos(
        Prato prato,
        IReadOnlyList<Guid> acompanhamentoIds)
    {
        var idsSelecionados = acompanhamentoIds.ToHashSet();

        var relacoesRemovidas = prato.PratoAcompanhamentos
            .Where(pratoAcompanhamento =>
                !idsSelecionados.Contains(pratoAcompanhamento.AcompanhamentoId))
            .ToList();

        foreach (var relacao in relacoesRemovidas)
        {
            prato.PratoAcompanhamentos.Remove(relacao);
        }

        var idsExistentes = prato.PratoAcompanhamentos
            .Select(pratoAcompanhamento => pratoAcompanhamento.AcompanhamentoId)
            .ToHashSet();

        foreach (var acompanhamentoId in idsSelecionados.Except(idsExistentes))
        {
            prato.PratoAcompanhamentos.Add(new PratoAcompanhamento
            {
                PratoId = prato.Id,
                AcompanhamentoId = acompanhamentoId,
                EstaIncluido = true
            });
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

    private static void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
        {
            throw new ArgumentException("O preço deve ser maior que zero.");
        }
    }

    private static PratoResposta MapearResposta(Prato prato)
    {
        return new PratoResposta
        {
            Id = prato.Id,
            Nome = prato.Nome,
            Descricao = prato.Descricao,
            Preco = prato.Preco,
            Ativo = prato.EstaAtivo,
            Disponivel = prato.EstaDisponivel,
            ImagemUrl = prato.UrlImagem,
            Categoria = new PratoCategoriaResposta
            {
                Id = prato.Categoria.Id,
                Nome = prato.Categoria.Nome
            },
            Acompanhamentos = prato.PratoAcompanhamentos
                .Select(pratoAcompanhamento => new PratoAcompanhamentoResposta
                {
                    Id = pratoAcompanhamento.Acompanhamento.Id,
                    Nome = pratoAcompanhamento.Acompanhamento.Nome,
                    Ativo = pratoAcompanhamento.Acompanhamento.EstaAtivo,
                    Disponivel = pratoAcompanhamento.Acompanhamento.EstaDisponivel
                })
                .OrderBy(acompanhamento => acompanhamento.Nome)
                .ToList()
        };
    }
}
