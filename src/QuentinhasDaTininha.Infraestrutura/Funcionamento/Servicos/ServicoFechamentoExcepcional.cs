using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Funcionamento.Servicos;

public class ServicoFechamentoExcepcional : IServicoFechamentoExcepcional
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoFechamentoExcepcional(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FechamentoExcepcionalResposta>> ListarAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FechamentosExcepcionais.AsNoTracking();

        if (dataInicial.HasValue)
        {
            query = query.Where(fechamento => fechamento.DataFechamento >= dataInicial.Value);
        }

        if (dataFinal.HasValue)
        {
            query = query.Where(fechamento => fechamento.DataFechamento <= dataFinal.Value);
        }

        return await query
            .OrderBy(fechamento => fechamento.DataFechamento)
            .Select(fechamento => MapearResposta(fechamento))
            .ToListAsync(cancellationToken);
    }

    public async Task<FechamentoExcepcionalResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FechamentosExcepcionais
            .AsNoTracking()
            .Where(fechamento => fechamento.Id == id)
            .Select(fechamento => MapearResposta(fechamento))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FechamentoExcepcionalResposta> CriarAsync(
        FechamentoExcepcionalCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        ValidarData(requisicao.DataFechamento);
        await ValidarFechamentoAtivoDuplicadoAsync(
            requisicao.DataFechamento,
            null,
            requisicao.Ativo,
            cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        var fechamento = new FechamentoExcepcional
        {
            DataFechamento = requisicao.DataFechamento,
            Motivo = NormalizarTextoOpcional(requisicao.Motivo),
            MensagemCliente = NormalizarTextoOpcional(requisicao.MensagemCliente),
            DiaInteiro = requisicao.DiaInteiro,
            HoraInicio = requisicao.HoraInicio,
            HoraFim = requisicao.HoraFim,
            EstaAtivo = requisicao.Ativo,
            PermitirPedidos = requisicao.PermitirPedidos,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.FechamentosExcepcionais.AddAsync(fechamento, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(fechamento);
    }

    public async Task<FechamentoExcepcionalResposta?> AtualizarAsync(
        Guid id,
        FechamentoExcepcionalAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var fechamento = await _dbContext.FechamentosExcepcionais
            .FirstOrDefaultAsync(fechamento => fechamento.Id == id, cancellationToken);

        if (fechamento is null)
        {
            return null;
        }

        ValidarData(requisicao.DataFechamento);
        await ValidarFechamentoAtivoDuplicadoAsync(
            requisicao.DataFechamento,
            id,
            requisicao.Ativo,
            cancellationToken);

        fechamento.DataFechamento = requisicao.DataFechamento;
        fechamento.Motivo = NormalizarTextoOpcional(requisicao.Motivo);
        fechamento.MensagemCliente = NormalizarTextoOpcional(requisicao.MensagemCliente);
        fechamento.DiaInteiro = requisicao.DiaInteiro;
        fechamento.HoraInicio = requisicao.HoraInicio;
        fechamento.HoraFim = requisicao.HoraFim;
        fechamento.EstaAtivo = requisicao.Ativo;
        fechamento.PermitirPedidos = requisicao.PermitirPedidos;
        fechamento.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(fechamento);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var fechamento = await _dbContext.FechamentosExcepcionais
            .FirstOrDefaultAsync(fechamento => fechamento.Id == id, cancellationToken);

        if (fechamento is null)
        {
            return false;
        }

        _dbContext.FechamentosExcepcionais.Remove(fechamento);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task ValidarFechamentoAtivoDuplicadoAsync(
        DateOnly dataFechamento,
        Guid? idIgnorado,
        bool fechamentoAtivo,
        CancellationToken cancellationToken)
    {
        if (!fechamentoAtivo)
        {
            return;
        }

        var existeFechamentoAtivo = await _dbContext.FechamentosExcepcionais
            .AsNoTracking()
            .AnyAsync(
                fechamento =>
                    fechamento.DataFechamento == dataFechamento &&
                    fechamento.EstaAtivo &&
                    (!idIgnorado.HasValue || fechamento.Id != idIgnorado.Value),
                cancellationToken);

        if (existeFechamentoAtivo)
        {
            throw new InvalidOperationException(
                "Já existe um fechamento excepcional para essa data.");
        }
    }

    private static void ValidarData(DateOnly data)
    {
        if (data == default)
        {
            throw new ArgumentException("Data é obrigatória.");
        }
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static FechamentoExcepcionalResposta MapearResposta(
        FechamentoExcepcional fechamento)
    {
        return new FechamentoExcepcionalResposta
        {
            Id = fechamento.Id,
            DataFechamento = fechamento.DataFechamento,
            Motivo = fechamento.Motivo,
            MensagemCliente = fechamento.MensagemCliente,
            DiaInteiro = fechamento.DiaInteiro,
            HoraInicio = fechamento.HoraInicio,
            HoraFim = fechamento.HoraFim,
            Ativo = fechamento.EstaAtivo,
            PermitirPedidos = fechamento.PermitirPedidos
        };
    }
}
