using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

public class ServicoCardapioPublico : IServicoCardapioPublico
{
    private static readonly TimeSpan DuracaoCache = TimeSpan.FromSeconds(30);

    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IControleCacheCardapioPublico _controleCache;
    private readonly IServicoDataLocal _servicoDataLocal;
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public ServicoCardapioPublico(
        QuentinhasDaTininhaDbContext dbContext,
        IMemoryCache memoryCache,
        IControleCacheCardapioPublico controleCache,
        IServicoDataLocal servicoDataLocal,
        IServicoDisponibilidadePedido servicoDisponibilidadePedido)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
        _controleCache = controleCache;
        _servicoDataLocal = servicoDataLocal;
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
    }

    public async Task<CardapioPublicoResposta> ObterAsync(
        DateOnly? data,
        CancellationToken cancellationToken = default)
    {
        var dataConsulta = data ?? _servicoDataLocal.ObterDataAtual();
        var chaveCache = $"cardapio-publico:{_controleCache.Versao}:{dataConsulta:yyyyMMdd}";

        return await _memoryCache.GetOrCreateAsync(chaveCache, entrada =>
        {
            entrada.AbsoluteExpirationRelativeToNow = DuracaoCache;
            entrada.SetPriority(CacheItemPriority.Normal);

            return ObterSemCacheAsync(dataConsulta, cancellationToken);
        }) ?? new CardapioPublicoResposta
        {
            Data = dataConsulta,
            DiaSemana = ConverterDiaSemana(dataConsulta)
        };
    }

    private async Task<CardapioPublicoResposta> ObterSemCacheAsync(
        DateOnly dataConsulta,
        CancellationToken cancellationToken)
    {
        var diaSemana = ConverterDiaSemana(dataConsulta);

        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        var horarios = await _dbContext.HorariosFuncionamento
            .AsNoTracking()
            .Where(horario => horario.DiaSemana == diaSemana && horario.EstaAtivo)
            .OrderBy(horario => horario.HoraAbertura)
            .Select(horario => new HorarioFuncionamentoPublicoResposta
            {
                HoraAbertura = horario.HoraAbertura,
                HoraFechamento = horario.HoraFechamento
            })
            .ToListAsync(cancellationToken);

        var resposta = new CardapioPublicoResposta
        {
            Restaurante = MapearRestaurante(configuracao),
            Data = dataConsulta,
            DiaSemana = diaSemana,
            Horarios = horarios
        };

        var disponibilidadeData = await _servicoDisponibilidadePedido.ObterPorDataAsync(
            dataConsulta,
            cancellationToken);
        var disponibilidade = await _servicoDisponibilidadePedido.ValidarPedidoAsync(
            dataConsulta,
            cancellationToken);

        resposta.PermitirPedidos = disponibilidade.PermitirPedidos;
        resposta.MotivoBloqueio = disponibilidade.MotivoBloqueio;
        resposta.DatasDisponiveis = disponibilidade.PermitirPedidos
            ? new List<DateOnly> { dataConsulta }
            : new List<DateOnly>();
        resposta.DatasBloqueadas = disponibilidade.PermitirPedidos
            ? new List<DisponibilidadeDataPublicaResposta>()
            : new List<DisponibilidadeDataPublicaResposta>
            {
                new()
                {
                    Data = dataConsulta,
                    Disponivel = false,
                    PermitirPedidos = false,
                    Motivo = disponibilidade.MotivoBloqueio,
                    MotivoBloqueio = disponibilidade.MotivoBloqueio
                }
            };

        resposta.Aberto = disponibilidade.PermitirPedidos;
        resposta.Mensagem = disponibilidade.PermitirPedidos
            ? configuracao?.MensagemAberto
            : disponibilidade.MotivoBloqueio ?? configuracao?.MensagemFechado;

        if (!disponibilidadeData.PermitirPedidos)
        {
            resposta.MotivoFechamento = disponibilidade.MotivoBloqueio;

            return resposta;
        }

        resposta.Categorias = await ObterCategoriasAsync(diaSemana, cancellationToken);
        resposta.Bebidas = await ObterBebidasAsync(cancellationToken);

        return resposta;
    }

    private async Task<IReadOnlyList<BebidaCardapioPublicoResposta>> ObterBebidasAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Bebidas
            .AsNoTracking()
            .Where(bebida => bebida.Ativa)
            .OrderBy(bebida => bebida.Nome)
            .Select(bebida => new BebidaCardapioPublicoResposta
            {
                Id = bebida.Id,
                Nome = bebida.Nome,
                Descricao = bebida.Descricao,
                Preco = bebida.Preco,
                ImagemUrl = bebida.ImagemUrl
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<CategoriaCardapioPublicoResposta>> ObterCategoriasAsync(
        DiaSemana diaSemana,
        CancellationToken cancellationToken)
    {
        var pratos = await _dbContext.CardapiosDiaPratos
            .AsNoTracking()
            .Where(cardapioPrato =>
                cardapioPrato.CardapioDia.DiaSemana == diaSemana &&
                cardapioPrato.CardapioDia.EstaAtivo &&
                cardapioPrato.EstaDisponivel &&
                cardapioPrato.Prato.EstaAtivo &&
                cardapioPrato.Prato.EstaDisponivel &&
                cardapioPrato.Prato.Categoria.EstaAtiva)
            .OrderBy(cardapioPrato => cardapioPrato.Prato.Categoria.Nome)
            .ThenBy(cardapioPrato => cardapioPrato.Prato.Nome)
            .Select(cardapioPrato => new LinhaPratoCardapioPublico(
                cardapioPrato.Prato.Categoria.Id,
                cardapioPrato.Prato.Categoria.Nome,
                cardapioPrato.Prato.Categoria.Descricao,
                cardapioPrato.Prato.Id,
                cardapioPrato.Prato.Nome,
                cardapioPrato.Prato.Descricao,
                cardapioPrato.Prato.Preco,
                cardapioPrato.Prato.UrlImagem))
            .ToListAsync(cancellationToken);

        if (pratos.Count == 0)
        {
            return new List<CategoriaCardapioPublicoResposta>();
        }

        var pratoIds = pratos.Select(prato => prato.PratoId).Distinct().ToList();
        var acompanhamentos = await _dbContext.PratosAcompanhamentos
            .AsNoTracking()
            .Where(pratoAcompanhamento =>
                pratoIds.Contains(pratoAcompanhamento.PratoId) &&
                pratoAcompanhamento.Acompanhamento.EstaAtivo)
            .OrderBy(pratoAcompanhamento => pratoAcompanhamento.Acompanhamento.Nome)
            .Select(pratoAcompanhamento => new LinhaAcompanhamentoCardapioPublico(
                pratoAcompanhamento.PratoId,
                pratoAcompanhamento.Acompanhamento.Id,
                pratoAcompanhamento.Acompanhamento.Nome,
                pratoAcompanhamento.Acompanhamento.Descricao,
                pratoAcompanhamento.Acompanhamento.PrecoAdicional))
            .ToListAsync(cancellationToken);

        var acompanhamentosPorPrato = acompanhamentos
            .GroupBy(acompanhamento => acompanhamento.PratoId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo
                    .Select(acompanhamento => new AcompanhamentoCardapioPublicoResposta
                    {
                        Id = acompanhamento.Id,
                        Nome = acompanhamento.Nome,
                        Descricao = acompanhamento.Descricao,
                        PrecoAdicional = acompanhamento.PrecoAdicional
                    })
                    .ToList());

        return pratos
            .GroupBy(prato => new
            {
                prato.CategoriaId,
                prato.CategoriaNome,
                prato.CategoriaDescricao
            })
            .OrderBy(grupo => grupo.Key.CategoriaNome)
            .Select(grupo => new CategoriaCardapioPublicoResposta
            {
                Id = grupo.Key.CategoriaId,
                Nome = grupo.Key.CategoriaNome,
                Descricao = grupo.Key.CategoriaDescricao,
                Pratos = grupo
                    .OrderBy(prato => prato.PratoNome)
                    .Select(prato => new PratoCardapioPublicoResposta
                    {
                        Id = prato.PratoId,
                        Nome = prato.PratoNome,
                        Descricao = prato.PratoDescricao,
                        Preco = prato.Preco,
                        ImagemUrl = prato.ImagemUrl,
                        Acompanhamentos = acompanhamentosPorPrato.GetValueOrDefault(
                            prato.PratoId,
                            new List<AcompanhamentoCardapioPublicoResposta>())
                    })
                    .ToList()
            })
            .ToList();
    }

    private static DiaSemana ConverterDiaSemana(DateOnly data)
    {
        return (DiaSemana)(int)data.ToDateTime(TimeOnly.MinValue).DayOfWeek;
    }

    private static RestaurantePublicoResposta MapearRestaurante(
        ConfiguracaoRestaurante? configuracao)
    {
        if (configuracao is null)
        {
            return new RestaurantePublicoResposta();
        }

        return new RestaurantePublicoResposta
        {
            Nome = configuracao.Nome,
            Descricao = configuracao.Descricao,
            UrlLogotipo = configuracao.UrlLogotipo,
            UrlImagemCapa = configuracao.UrlImagemCapa,
            Telefone = configuracao.Telefone,
            Whatsapp = configuracao.Whatsapp,
            Endereco = configuracao.Endereco,
            Cidade = configuracao.Cidade,
            Estado = configuracao.Estado,
            Cep = configuracao.Cep
        };
    }

    private sealed record LinhaPratoCardapioPublico(
        Guid CategoriaId,
        string CategoriaNome,
        string? CategoriaDescricao,
        Guid PratoId,
        string PratoNome,
        string? PratoDescricao,
        decimal Preco,
        string? ImagemUrl);

    private sealed record LinhaAcompanhamentoCardapioPublico(
        Guid PratoId,
        Guid Id,
        string Nome,
        string? Descricao,
        decimal PrecoAdicional);
}
