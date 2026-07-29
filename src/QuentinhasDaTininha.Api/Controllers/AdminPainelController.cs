using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Admin.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/admin/painel")]
public class AdminPainelController : ControllerBase
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoDataLocal _servicoDataLocal;

    public AdminPainelController(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoDataLocal servicoDataLocal)
    {
        _dbContext = dbContext;
        _servicoDataLocal = servicoDataLocal;
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoPainelResposta>> ObterResumo(
        CancellationToken cancellationToken)
    {
        var dataLocal = _servicoDataLocal.ObterDataAtual();
        var diaDominio = MapearDia(dataLocal.DayOfWeek);
        var diaResposta = diaDominio == DiaSemana.Domingo ? 7 : (int)diaDominio;

        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        var cardapioHoje = _dbContext.CardapiosDiaPratos
            .AsNoTracking()
            .Where(cardapioPrato =>
                cardapioPrato.CardapioDia.DiaSemana == diaDominio &&
                cardapioPrato.CardapioDia.EstaAtivo &&
                cardapioPrato.Prato.EstaAtivo);

        var quantidadePratosHoje = await cardapioHoje.CountAsync(cancellationToken);
        var quantidadePratosDisponiveis = await cardapioHoje
            .CountAsync(cardapioPrato =>
                cardapioPrato.EstaDisponivel &&
                cardapioPrato.Prato.EstaDisponivel,
                cancellationToken);
        var quantidadePratosIndisponiveis =
            quantidadePratosHoje - quantidadePratosDisponiveis;

        var quantidadeAcompanhamentosIndisponiveis = await _dbContext.Acompanhamentos
            .AsNoTracking()
            .CountAsync(acompanhamento =>
                acompanhamento.EstaAtivo &&
                !acompanhamento.EstaDisponivel,
                cancellationToken);

        var restauranteAberto = diaDominio != DiaSemana.Domingo &&
            configuracao?.EstaAtivo == true &&
            configuracao.AceitaPedidos &&
            configuracao.ModoFuncionamento != ModoFuncionamento.FechadoManualmente;

        return Ok(new ResumoPainelResposta
        {
            RestauranteAberto = restauranteAberto,
            MensagemStatus = restauranteAberto
                ? configuracao?.MensagemAberto ?? "Estamos atendendo normalmente."
                : configuracao?.MensagemFechado ?? "Restaurante fechado no momento.",
            QuantidadePratosHoje = quantidadePratosHoje,
            QuantidadePratosDisponiveis = quantidadePratosDisponiveis,
            QuantidadePratosIndisponiveis = quantidadePratosIndisponiveis,
            QuantidadeAcompanhamentosIndisponiveis = quantidadeAcompanhamentosIndisponiveis,
            DiaSemana = diaResposta,
            NomeDiaSemana = ObterNomeDia(diaDominio)
        });
    }

    private static DiaSemana MapearDia(DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Monday => DiaSemana.SegundaFeira,
            DayOfWeek.Tuesday => DiaSemana.TercaFeira,
            DayOfWeek.Wednesday => DiaSemana.QuartaFeira,
            DayOfWeek.Thursday => DiaSemana.QuintaFeira,
            DayOfWeek.Friday => DiaSemana.SextaFeira,
            DayOfWeek.Saturday => DiaSemana.Sabado,
            _ => DiaSemana.Domingo
        };
    }

    private static string ObterNomeDia(DiaSemana dia)
    {
        return dia switch
        {
            DiaSemana.SegundaFeira => "Segunda-feira",
            DiaSemana.TercaFeira => "Terca-feira",
            DiaSemana.QuartaFeira => "Quarta-feira",
            DiaSemana.QuintaFeira => "Quinta-feira",
            DiaSemana.SextaFeira => "Sexta-feira",
            DiaSemana.Sabado => "Sabado",
            _ => "Domingo"
        };
    }
}
