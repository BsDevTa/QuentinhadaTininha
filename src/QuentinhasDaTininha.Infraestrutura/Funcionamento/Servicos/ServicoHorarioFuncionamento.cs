using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Funcionamento.Servicos;

public class ServicoHorarioFuncionamento : IServicoHorarioFuncionamento
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoHorarioFuncionamento(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<HorarioFuncionamentoResposta>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HorariosFuncionamento
            .AsNoTracking()
            .OrderBy(horario => horario.DiaSemana)
            .ThenBy(horario => horario.HoraAbertura)
            .Select(horario => MapearResposta(horario))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HorarioFuncionamentoResposta>> SubstituirDiaAsync(
        DiaSemana diaSemana,
        IReadOnlyCollection<HorarioFuncionamentoRequisicao> horarios,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(horarios);

        var horariosOrdenados = horarios
            .OrderBy(horario => horario.HoraAbertura)
            .ThenBy(horario => horario.HoraFechamento)
            .ToList();

        ValidarHorarios(horariosOrdenados);

        var horariosAntigos = await _dbContext.HorariosFuncionamento
            .Where(horario => horario.DiaSemana == diaSemana)
            .ToListAsync(cancellationToken);

        _dbContext.HorariosFuncionamento.RemoveRange(horariosAntigos);

        var agora = DateTimeOffset.UtcNow;
        foreach (var horario in horariosOrdenados)
        {
            await _dbContext.HorariosFuncionamento.AddAsync(
                new HorarioFuncionamento
                {
                    DiaSemana = diaSemana,
                    HoraAbertura = horario.HoraAbertura,
                    HoraFechamento = horario.HoraFechamento,
                    EstaAtivo = horario.Ativo,
                    CriadoEm = agora,
                    AtualizadoEm = agora
                },
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.HorariosFuncionamento
            .AsNoTracking()
            .Where(horario => horario.DiaSemana == diaSemana)
            .OrderBy(horario => horario.HoraAbertura)
            .Select(horario => MapearResposta(horario))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var horario = await _dbContext.HorariosFuncionamento
            .FirstOrDefaultAsync(horario => horario.Id == id, cancellationToken);

        if (horario is null)
        {
            return false;
        }

        _dbContext.HorariosFuncionamento.Remove(horario);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidarHorarios(
        IReadOnlyList<HorarioFuncionamentoRequisicao> horarios)
    {
        foreach (var horario in horarios)
        {
            if (horario.HoraAbertura >= horario.HoraFechamento)
            {
                throw new ArgumentException(
                    "O horário de abertura deve ser anterior ao horário de fechamento.");
            }
        }

        for (var indice = 1; indice < horarios.Count; indice++)
        {
            var horarioAnterior = horarios[indice - 1];
            var horarioAtual = horarios[indice];

            if (horarioAtual.HoraAbertura < horarioAnterior.HoraFechamento)
            {
                throw new InvalidOperationException(
                    "Existem horários sobrepostos para o mesmo dia.");
            }
        }
    }

    private static HorarioFuncionamentoResposta MapearResposta(
        HorarioFuncionamento horario)
    {
        return new HorarioFuncionamentoResposta
        {
            Id = horario.Id,
            DiaSemana = horario.DiaSemana,
            HoraAbertura = horario.HoraAbertura,
            HoraFechamento = horario.HoraFechamento,
            Ativo = horario.EstaAtivo
        };
    }
}
