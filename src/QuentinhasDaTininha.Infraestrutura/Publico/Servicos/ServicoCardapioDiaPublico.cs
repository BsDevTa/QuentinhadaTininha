using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

public class ServicoCardapioDiaPublico : IServicoCardapioDiaPublico
{
    private static readonly TimeSpan DuracaoCache = TimeSpan.FromSeconds(30);

    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IMemoryCache _memoryCache;
    private readonly IControleCacheCardapioPublico _controleCache;
    private readonly IServicoDataLocal _servicoDataLocal;
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public ServicoCardapioDiaPublico(
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

    public Task<CardapioDiaPublicoResposta> ObterHojeAsync(
        CancellationToken cancellationToken = default)
    {
        var dataAtual = _servicoDataLocal.ObterDataAtual();
        var diaSemana = ConverterParaDiaPublico(dataAtual.DayOfWeek);
        return ObterPorDiaAsync(diaSemana, cancellationToken);
    }

    public async Task<CardapioDiaPublicoResposta> ObterPorDiaAsync(
        int diaSemana,
        CancellationToken cancellationToken = default)
    {
        if (diaSemana is < 1 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(diaSemana));
        }

        var dataAtual = _servicoDataLocal.ObterDataAtual();
        var chaveCache =
            $"cardapio-dia-publico:{_controleCache.Versao}:{dataAtual:yyyyMMdd}:{diaSemana}";

        return await _memoryCache.GetOrCreateAsync(chaveCache, entrada =>
        {
            entrada.AbsoluteExpirationRelativeToNow = DuracaoCache;
            entrada.SetPriority(CacheItemPriority.Normal);

            return ObterPorDiaSemCacheAsync(diaSemana, dataAtual, cancellationToken);
        }) ?? new CardapioDiaPublicoResposta
        {
            DiaSemana = diaSemana,
            NomeDiaSemana = ObterNomeDia(diaSemana)
        };
    }

    private async Task<CardapioDiaPublicoResposta> ObterPorDiaSemCacheAsync(
        int diaSemana,
        DateOnly dataAtual,
        CancellationToken cancellationToken)
    {
        var restaurante = await ObterStatusRestauranteAsync(cancellationToken);
        var diaAtual = ConverterParaDiaPublico(dataAtual.DayOfWeek);

        if (diaSemana != diaAtual)
        {
            restaurante.EstaAberto = false;
            restaurante.PermitirPedidos = false;
            restaurante.MotivoBloqueio =
                "Somente o dia atual está disponível para pedidos.";
            restaurante.MensagemStatus = restaurante.MotivoBloqueio;
        }

        if (diaSemana == 7)
        {
            restaurante.EstaAberto = false;
            restaurante.PermitirPedidos = false;
            restaurante.MotivoBloqueio =
                "Hoje não temos atendimento. Consulte o cardápio dos outros dias.";
            restaurante.MensagemStatus = "Hoje não temos atendimento. Consulte o cardápio dos outros dias.";
        }

        var diaDominio = ConverterParaDiaDominio(diaSemana);
        var pratos = await _dbContext.CardapiosDiaPratos
            .AsNoTracking()
            .Where(cardapioPrato =>
                cardapioPrato.CardapioDia.DiaSemana == diaDominio &&
                cardapioPrato.CardapioDia.EstaAtivo &&
                cardapioPrato.Prato.EstaAtivo)
            .OrderBy(cardapioPrato => cardapioPrato.OrdemExibicao)
            .Select(cardapioPrato => new PratoPublicoResposta
            {
                Id = cardapioPrato.Prato.Id,
                Nome = cardapioPrato.Prato.Nome,
                Descricao = cardapioPrato.Prato.Descricao,
                UrlImagem = cardapioPrato.Prato.UrlImagem,
                EstaDisponivel = cardapioPrato.EstaDisponivel &&
                    cardapioPrato.Prato.EstaDisponivel,
                OrdemExibicao = cardapioPrato.OrdemExibicao,
                Precos = new PrecosPratoPublicoResposta
                {
                    PequenaDinheiroPix = cardapioPrato.Prato.Precos
                        .Where(preco => preco.Tamanho == TamanhoRefeicao.P &&
                            preco.FormaPagamento == TipoPrecoPagamento.DinheiroPix)
                        .Select(preco => preco.Valor)
                        .FirstOrDefault(),
                    PequenaCartao = cardapioPrato.Prato.Precos
                        .Where(preco => preco.Tamanho == TamanhoRefeicao.P &&
                            preco.FormaPagamento == TipoPrecoPagamento.Cartao)
                        .Select(preco => preco.Valor)
                        .FirstOrDefault(),
                    GrandeDinheiroPix = cardapioPrato.Prato.Precos
                        .Where(preco => preco.Tamanho == TamanhoRefeicao.G &&
                            preco.FormaPagamento == TipoPrecoPagamento.DinheiroPix)
                        .Select(preco => preco.Valor)
                        .FirstOrDefault(),
                    GrandeCartao = cardapioPrato.Prato.Precos
                        .Where(preco => preco.Tamanho == TamanhoRefeicao.G &&
                            preco.FormaPagamento == TipoPrecoPagamento.Cartao)
                        .Select(preco => preco.Valor)
                        .FirstOrDefault()
                },
                GrupoAcompanhamento = cardapioPrato.Prato.GrupoAcompanhamento == null
                    ? new GrupoAcompanhamentoPublicoResposta()
                    : new GrupoAcompanhamentoPublicoResposta
                    {
                        Codigo = cardapioPrato.Prato.GrupoAcompanhamento.Codigo,
                        Nome = cardapioPrato.Prato.GrupoAcompanhamento.Nome,
                        Acompanhamentos = cardapioPrato.Prato.GrupoAcompanhamento.Itens
                            .Where(item =>
                                item.GrupoAcompanhamento.EstaAtivo &&
                                item.Acompanhamento.EstaAtivo)
                            .OrderBy(item => item.OrdemExibicao)
                            .Select(item => new AcompanhamentoPublicoResposta
                            {
                                Id = item.Acompanhamento.Id,
                                Nome = item.Acompanhamento.Nome,
                                EstaDisponivel = item.Acompanhamento.EstaDisponivel,
                                TipoSelecao = item.Acompanhamento.TipoSelecao.ToString().ToUpperInvariant(),
                                GrupoExclusivo = item.Acompanhamento.GrupoExclusivo,
                                Obrigatorio = item.Obrigatorio,
                                OrdemExibicao = item.OrdemExibicao
                            })
                            .ToList()
                    }
            })
            .ToListAsync(cancellationToken);

        return new CardapioDiaPublicoResposta
        {
            DiaSemana = diaSemana,
            NomeDiaSemana = ObterNomeDia(diaSemana),
            Restaurante = restaurante,
            Pratos = pratos
        };
    }

    public async Task<RestauranteStatusPublicoResposta> ObterStatusRestauranteAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuracao is null)
        {
            return new RestauranteStatusPublicoResposta
            {
                Nome = "Quentinhas da Tininha",
                EstaAberto = false,
                PermitirPedidos = false,
                MotivoBloqueio = "Não conseguimos carregar o status do restaurante.",
                MensagemStatus = "Não conseguimos carregar o status do restaurante."
            };
        }

        var dataAtual = _servicoDataLocal.ObterDataAtual();
        var disponibilidade = await _servicoDisponibilidadePedido.ValidarPedidoAsync(
            dataAtual,
            cancellationToken);

        return new RestauranteStatusPublicoResposta
        {
            Nome = configuracao.Nome,
            EstaAberto = disponibilidade.PermitirPedidos,
            PermitirPedidos = disponibilidade.PermitirPedidos,
            MotivoBloqueio = disponibilidade.MotivoBloqueio,
            MensagemStatus = disponibilidade.PermitirPedidos
                ? configuracao.MensagemAberto
                : disponibilidade.MotivoBloqueio ?? configuracao.MensagemFechado,
            Whatsapp = configuracao.Whatsapp,
            Instagram = configuracao.Instagram,
            Endereco = string.Join(", ", new[]
            {
                configuracao.Endereco,
                configuracao.Cidade,
                configuracao.Estado
            }.Where(valor => !string.IsNullOrWhiteSpace(valor))),
            HorarioFuncionamento = configuracao.HorarioFuncionamento,
            UrlLogo = configuracao.UrlLogotipo
        };
    }

    private static int ConverterParaDiaPublico(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
    }

    private static DiaSemana ConverterParaDiaDominio(int diaSemana)
    {
        return diaSemana == 7 ? DiaSemana.Domingo : (DiaSemana)diaSemana;
    }

    private static string ObterNomeDia(int diaSemana)
    {
        return diaSemana switch
        {
            1 => "Segunda-feira",
            2 => "Terça-feira",
            3 => "Quarta-feira",
            4 => "Quinta-feira",
            5 => "Sexta-feira",
            6 => "Sábado",
            7 => "Domingo",
            _ => string.Empty
        };
    }
}
