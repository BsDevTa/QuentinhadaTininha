using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;
using QuentinhasDaTininha.Aplicacao.Cardapios.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Cardapios.Servicos;

public class ServicoCardapio : IServicoCardapio
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoCardapio(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CardapioDiaResposta>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var cardapios = await ConsultaCardapios()
            .OrderBy(cardapio => cardapio.DiaSemana)
            .ToListAsync(cancellationToken);

        return cardapios.Select(MapearResposta).ToList();
    }

    public async Task<CardapioDiaResposta?> ObterPorDiaAsync(
        DiaSemana diaSemana,
        CancellationToken cancellationToken = default)
    {
        var cardapio = await ConsultaCardapios()
            .FirstOrDefaultAsync(cardapio => cardapio.DiaSemana == diaSemana, cancellationToken);

        return cardapio is null ? null : MapearResposta(cardapio);
    }

    public async Task<CardapioDiaResposta> AtualizarAsync(
        DiaSemana diaSemana,
        CardapioDiaAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var pratosRequisicao = requisicao.Pratos.ToList();
        var pratoIds = pratosRequisicao
            .Select(prato => prato.PratoId)
            .ToList();

        if (pratoIds.Count != pratoIds.Distinct().Count())
        {
            throw new InvalidOperationException("Existem pratos duplicados na solicitação.");
        }

        var pratos = await _dbContext.Pratos
            .Where(prato => pratoIds.Contains(prato.Id))
            .ToListAsync(cancellationToken);

        if (pratos.Count != pratoIds.Count)
        {
            throw new InvalidOperationException("O prato informado não existe.");
        }

        if (pratos.Any(prato => !prato.EstaAtivo))
        {
            throw new InvalidOperationException("O prato informado está inativo.");
        }

        var cardapio = await _dbContext.CardapiosDia
            .Include(cardapio => cardapio.CardapiosDiaPratos)
            .FirstOrDefaultAsync(cardapio => cardapio.DiaSemana == diaSemana, cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        if (cardapio is null)
        {
            cardapio = new CardapioDia
            {
                DiaSemana = diaSemana,
                CriadoEm = agora
            };

            await _dbContext.CardapiosDia.AddAsync(cardapio, cancellationToken);
        }

        cardapio.EstaAtivo = requisicao.Ativo;
        cardapio.Observacao = NormalizarTextoOpcional(requisicao.Observacao);
        cardapio.AtualizadoEm = agora;

        SincronizarPratos(cardapio, pratosRequisicao);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ObterPorDiaAsync(diaSemana, cancellationToken) ??
            throw new InvalidOperationException("Não foi possível carregar o cardápio atualizado.");
    }

    public async Task<bool> AlterarDisponibilidadePratoAsync(
        DiaSemana diaSemana,
        Guid pratoId,
        bool disponivel,
        CancellationToken cancellationToken = default)
    {
        var cardapio = await _dbContext.CardapiosDia
            .Include(cardapio => cardapio.CardapiosDiaPratos)
            .FirstOrDefaultAsync(cardapio => cardapio.DiaSemana == diaSemana, cancellationToken);

        if (cardapio is null)
        {
            return false;
        }

        var cardapioPrato = cardapio.CardapiosDiaPratos
            .FirstOrDefault(prato => prato.PratoId == pratoId);

        if (cardapioPrato is null)
        {
            throw new InvalidOperationException("O prato não está associado ao cardápio informado.");
        }

        cardapioPrato.EstaDisponivel = disponivel;
        cardapio.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private IQueryable<CardapioDia> ConsultaCardapios()
    {
        return _dbContext.CardapiosDia
            .AsNoTracking()
            .Include(cardapio => cardapio.CardapiosDiaPratos)
                .ThenInclude(cardapioPrato => cardapioPrato.Prato)
                    .ThenInclude(prato => prato.Categoria);
    }

    private void SincronizarPratos(
        CardapioDia cardapio,
        IReadOnlyList<CardapioDiaPratoRequisicao> pratosRequisicao)
    {
        var idsSolicitados = pratosRequisicao
            .Select(prato => prato.PratoId)
            .ToHashSet();

        var relacoesRemovidas = cardapio.CardapiosDiaPratos
            .Where(cardapioPrato => !idsSolicitados.Contains(cardapioPrato.PratoId))
            .ToList();

        foreach (var relacao in relacoesRemovidas)
        {
            cardapio.CardapiosDiaPratos.Remove(relacao);
        }

        _dbContext.CardapiosDiaPratos.RemoveRange(relacoesRemovidas);

        for (var indice = 0; indice < pratosRequisicao.Count; indice++)
        {
            var pratoRequisicao = pratosRequisicao[indice];
            var ordemExibicao = indice + 1;
            var relacaoExistente = cardapio.CardapiosDiaPratos
                .FirstOrDefault(cardapioPrato => cardapioPrato.PratoId == pratoRequisicao.PratoId);

            if (relacaoExistente is null)
            {
                var novaRelacao = new CardapioDiaPrato
                {
                    CardapioDiaId = cardapio.Id,
                    PratoId = pratoRequisicao.PratoId,
                    EstaDisponivel = pratoRequisicao.Disponivel,
                    OrdemExibicao = ordemExibicao
                };

                cardapio.CardapiosDiaPratos.Add(novaRelacao);
                _dbContext.Entry(novaRelacao).State = EntityState.Added;

                continue;
            }

            relacaoExistente.EstaDisponivel = pratoRequisicao.Disponivel;
            relacaoExistente.OrdemExibicao = ordemExibicao;
        }
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static CardapioDiaResposta MapearResposta(CardapioDia cardapio)
    {
        return new CardapioDiaResposta
        {
            Id = cardapio.Id,
            DiaSemana = cardapio.DiaSemana,
            Ativo = cardapio.EstaAtivo,
            Observacao = cardapio.Observacao,
            Pratos = cardapio.CardapiosDiaPratos
                .OrderBy(cardapioPrato => cardapioPrato.Prato.Categoria.Nome)
                .ThenBy(cardapioPrato => cardapioPrato.Prato.Nome)
                .Select(cardapioPrato => new CardapioDiaPratoResposta
                {
                    PratoId = cardapioPrato.PratoId,
                    Nome = cardapioPrato.Prato.Nome,
                    Descricao = cardapioPrato.Prato.Descricao,
                    Preco = cardapioPrato.Prato.Preco,
                    ImagemUrl = cardapioPrato.Prato.UrlImagem,
                    CategoriaId = cardapioPrato.Prato.CategoriaId,
                    CategoriaNome = cardapioPrato.Prato.Categoria.Nome,
                    PratoAtivo = cardapioPrato.Prato.EstaAtivo,
                    PratoDisponivel = cardapioPrato.Prato.EstaDisponivel,
                    DisponivelNoCardapio = cardapioPrato.EstaDisponivel,
                    OrdemExibicao = cardapioPrato.OrdemExibicao
                })
                .ToList()
        };
    }
}
