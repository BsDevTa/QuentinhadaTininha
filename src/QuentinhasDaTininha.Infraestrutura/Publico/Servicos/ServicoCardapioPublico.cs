using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

public class ServicoCardapioPublico : IServicoCardapioPublico
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoCardapioPublico(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CardapioPublicoResposta> ObterAsync(
        DateOnly? data,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.Now;
        var dataConsulta = data ?? DateOnly.FromDateTime(agora.DateTime);
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

        var fechamento = await _dbContext.FechamentosExcepcionais
            .AsNoTracking()
            .Where(fechamento =>
                fechamento.DataFechamento == dataConsulta &&
                fechamento.EstaAtivo)
            .OrderBy(fechamento => fechamento.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        var resposta = new CardapioPublicoResposta
        {
            Restaurante = MapearRestaurante(configuracao),
            Data = dataConsulta,
            DiaSemana = diaSemana,
            Horarios = horarios
        };

        if (fechamento is not null)
        {
            resposta.Aberto = false;
            resposta.MotivoFechamento = fechamento.MensagemCliente ?? fechamento.Motivo;
            resposta.Mensagem = fechamento.MensagemCliente ??
                configuracao?.MensagemFechado ??
                fechamento.Motivo;

            return resposta;
        }

        resposta.Aberto = EstaAberto(configuracao, horarios, dataConsulta, agora);
        resposta.Mensagem = resposta.Aberto
            ? configuracao?.MensagemAberto
            : configuracao?.MensagemFechado;

        resposta.Categorias = await ObterCategoriasAsync(diaSemana, cancellationToken);

        return resposta;
    }

    private async Task<IReadOnlyList<CategoriaCardapioPublicoResposta>> ObterCategoriasAsync(
        DiaSemana diaSemana,
        CancellationToken cancellationToken)
    {
        var cardapio = await _dbContext.CardapiosDia
            .AsNoTracking()
            .Include(cardapio => cardapio.CardapiosDiaPratos)
                .ThenInclude(cardapioPrato => cardapioPrato.Prato)
                    .ThenInclude(prato => prato.Categoria)
            .Include(cardapio => cardapio.CardapiosDiaPratos)
                .ThenInclude(cardapioPrato => cardapioPrato.Prato)
                    .ThenInclude(prato => prato.PratoAcompanhamentos)
                        .ThenInclude(pratoAcompanhamento => pratoAcompanhamento.Acompanhamento)
            .FirstOrDefaultAsync(
                cardapio => cardapio.DiaSemana == diaSemana && cardapio.EstaAtivo,
                cancellationToken);

        if (cardapio is null)
        {
            return new List<CategoriaCardapioPublicoResposta>();
        }

        return cardapio.CardapiosDiaPratos
            .Where(cardapioPrato =>
                cardapioPrato.EstaDisponivel &&
                cardapioPrato.Prato.EstaAtivo &&
                cardapioPrato.Prato.EstaDisponivel &&
                cardapioPrato.Prato.Categoria.EstaAtiva)
            .GroupBy(cardapioPrato => cardapioPrato.Prato.Categoria)
            .OrderBy(grupo => grupo.Key.Nome)
            .Select(grupo => new CategoriaCardapioPublicoResposta
            {
                Id = grupo.Key.Id,
                Nome = grupo.Key.Nome,
                Descricao = grupo.Key.Descricao,
                Pratos = grupo
                    .OrderBy(cardapioPrato => cardapioPrato.Prato.Nome)
                    .Select(cardapioPrato => new PratoCardapioPublicoResposta
                    {
                        Id = cardapioPrato.Prato.Id,
                        Nome = cardapioPrato.Prato.Nome,
                        Descricao = cardapioPrato.Prato.Descricao,
                        Preco = cardapioPrato.Prato.Preco,
                        ImagemUrl = cardapioPrato.Prato.UrlImagem,
                        Acompanhamentos = cardapioPrato.Prato.PratoAcompanhamentos
                            .Where(pratoAcompanhamento =>
                                pratoAcompanhamento.Acompanhamento.EstaAtivo)
                            .OrderBy(pratoAcompanhamento =>
                                pratoAcompanhamento.Acompanhamento.Nome)
                            .Select(pratoAcompanhamento =>
                                new AcompanhamentoCardapioPublicoResposta
                                {
                                    Id = pratoAcompanhamento.Acompanhamento.Id,
                                    Nome = pratoAcompanhamento.Acompanhamento.Nome,
                                    Descricao = pratoAcompanhamento.Acompanhamento.Descricao,
                                    PrecoAdicional =
                                        pratoAcompanhamento.Acompanhamento.PrecoAdicional
                                })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();
    }

    private static bool EstaAberto(
        ConfiguracaoRestaurante? configuracao,
        IReadOnlyCollection<HorarioFuncionamentoPublicoResposta> horarios,
        DateOnly dataConsulta,
        DateTimeOffset agora)
    {
        if (configuracao is { EstaAtivo: false } ||
            configuracao is { AceitaPedidos: false })
        {
            return false;
        }

        return configuracao?.ModoFuncionamento switch
        {
            ModoFuncionamento.AbertoManualmente => true,
            ModoFuncionamento.FechadoManualmente => false,
            _ => EstaAbertoPorHorario(horarios, dataConsulta, agora)
        };
    }

    private static bool EstaAbertoPorHorario(
        IReadOnlyCollection<HorarioFuncionamentoPublicoResposta> horarios,
        DateOnly dataConsulta,
        DateTimeOffset agora)
    {
        if (horarios.Count == 0)
        {
            return false;
        }

        var dataAtual = DateOnly.FromDateTime(agora.DateTime);
        if (dataConsulta != dataAtual)
        {
            return true;
        }

        var horaAtual = TimeOnly.FromDateTime(agora.DateTime);
        return horarios.Any(horario =>
            horaAtual >= horario.HoraAbertura &&
            horaAtual <= horario.HoraFechamento);
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
}
