using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

public class ServicoCardapioPublico : IServicoCardapioPublico
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoDataLocal _servicoDataLocal;
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public ServicoCardapioPublico(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoDataLocal servicoDataLocal,
        IServicoDisponibilidadePedido servicoDisponibilidadePedido)
    {
        _dbContext = dbContext;
        _servicoDataLocal = servicoDataLocal;
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
    }

    public async Task<CardapioPublicoResposta> ObterAsync(
        DateOnly? data,
        CancellationToken cancellationToken = default)
    {
        var dataConsulta = data ?? _servicoDataLocal.ObterDataAtual();
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
