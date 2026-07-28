using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Acompanhamentos.DTOs;
using QuentinhasDaTininha.Aplicacao.Acompanhamentos.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Acompanhamentos.Servicos;

public class ServicoAcompanhamento : IServicoAcompanhamento
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoAcompanhamento(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AcompanhamentoResposta>> ListarAsync(
        string? busca,
        bool? ativo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Acompanhamentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaNormalizada = busca.Trim().ToLowerInvariant();
            query = query.Where(acompanhamento =>
                acompanhamento.Nome.ToLower().Contains(buscaNormalizada));
        }

        if (ativo.HasValue)
        {
            query = query.Where(acompanhamento => acompanhamento.EstaAtivo == ativo.Value);
        }

        return await query
            .OrderBy(acompanhamento => acompanhamento.Nome)
            .Select(acompanhamento => MapearResposta(acompanhamento))
            .ToListAsync(cancellationToken);
    }

    public async Task<AcompanhamentoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Acompanhamentos
            .AsNoTracking()
            .Where(acompanhamento => acompanhamento.Id == id)
            .Select(acompanhamento => MapearResposta(acompanhamento))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AcompanhamentoResposta> CriarAsync(
        AcompanhamentoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var nome = NormalizarNome(requisicao.Nome);
        await ValidarNomeDuplicadoAsync(nome, null, cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        var acompanhamento = new Acompanhamento
        {
            Nome = nome,
            Descricao = NormalizarTextoOpcional(requisicao.Descricao),
            PrecoAdicional = requisicao.PrecoAdicional,
            EstaAtivo = true,
            EstaDisponivel = requisicao.Disponivel,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Acompanhamentos.AddAsync(acompanhamento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(acompanhamento);
    }

    public async Task<AcompanhamentoResposta?> AtualizarAsync(
        Guid id,
        AcompanhamentoAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var acompanhamento = await _dbContext.Acompanhamentos
            .FirstOrDefaultAsync(acompanhamento => acompanhamento.Id == id, cancellationToken);

        if (acompanhamento is null)
        {
            return null;
        }

        var nome = NormalizarNome(requisicao.Nome);
        await ValidarNomeDuplicadoAsync(nome, id, cancellationToken);

        acompanhamento.Nome = nome;
        acompanhamento.Descricao = NormalizarTextoOpcional(requisicao.Descricao);
        acompanhamento.PrecoAdicional = requisicao.PrecoAdicional;
        acompanhamento.EstaAtivo = requisicao.Ativo;
        acompanhamento.EstaDisponivel = requisicao.Disponivel;
        acompanhamento.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(acompanhamento);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var acompanhamento = await _dbContext.Acompanhamentos
            .FirstOrDefaultAsync(acompanhamento => acompanhamento.Id == id, cancellationToken);

        if (acompanhamento is null)
        {
            return false;
        }

        var possuiPratos = await _dbContext.PratosAcompanhamentos
            .AsNoTracking()
            .AnyAsync(
                pratoAcompanhamento => pratoAcompanhamento.AcompanhamentoId == id,
                cancellationToken);

        if (possuiPratos)
        {
            throw new InvalidOperationException(
                "O acompanhamento não pode ser excluído porque está associado a pratos.");
        }

        _dbContext.Acompanhamentos.Remove(acompanhamento);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidarNomeDuplicadoAsync(
        string nome,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        var nomeComparacao = nome.ToLowerInvariant();

        var nomeJaExiste = await _dbContext.Acompanhamentos
            .AsNoTracking()
            .AnyAsync(
                acompanhamento =>
                    acompanhamento.Nome.ToLower() == nomeComparacao &&
                    (!idIgnorado.HasValue || acompanhamento.Id != idIgnorado.Value),
                cancellationToken);

        if (nomeJaExiste)
        {
            throw new InvalidOperationException("Já existe um acompanhamento com esse nome.");
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

    private static AcompanhamentoResposta MapearResposta(Acompanhamento acompanhamento)
    {
        return new AcompanhamentoResposta
        {
            Id = acompanhamento.Id,
            Nome = acompanhamento.Nome,
            Descricao = acompanhamento.Descricao,
            PrecoAdicional = acompanhamento.PrecoAdicional,
            Ativo = acompanhamento.EstaAtivo,
            Disponivel = acompanhamento.EstaDisponivel
        };
    }
}
