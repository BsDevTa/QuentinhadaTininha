using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Funcionamento.Servicos;

public class ServicoDisponibilidadePedido : IServicoDisponibilidadePedido
{
    private static readonly DateOnly DataExcecaoPedidosTeste = new(2026, 8, 9);
    private const int QuantidadeDiasPadrao = 30;
    private const int QuantidadeMaximaDiasConsulta = 366;
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoDataLocal _servicoDataLocal;

    public ServicoDisponibilidadePedido(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoDataLocal servicoDataLocal)
    {
        _dbContext = dbContext;
        _servicoDataLocal = servicoDataLocal;
    }

    public async Task<IReadOnlyList<DisponibilidadeDataResposta>> ListarAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fim) = NormalizarPeriodo(
            dataInicial,
            dataFinal,
            _servicoDataLocal.ObterDataAtual());
        var registros = await ObterRegistrosAtivosAsync(inicio, fim, cancellationToken);
        var dataAtual = _servicoDataLocal.ObterDataAtual();

        return CriarPeriodo(inicio, fim)
            .Select(data => MapearResposta(data, dataAtual, registros.GetValueOrDefault(data)))
            .ToList();
    }

    public async Task<DisponibilidadeDataResposta> ObterPorDataAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        ValidarData(data);

        var registro = await ObterRegistroAtivoAsync(data, cancellationToken);
        return MapearResposta(data, _servicoDataLocal.ObterDataAtual(), registro);
    }

    public async Task<DisponibilidadeDataResposta> LiberarDataAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default)
    {
        ValidarData(data);

        if (data < _servicoDataLocal.ObterDataAtual())
        {
            throw new InvalidOperationException(
                "Não é possível liberar datas anteriores para pedidos.");
        }

        var registro = await ObterOuCriarRegistroAsync(data, cancellationToken);
        AtualizarRegistro(registro, permitirPedidos: true, motivo);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(data, _servicoDataLocal.ObterDataAtual(), registro);
    }

    public async Task<DisponibilidadeDataResposta> BloquearDataAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default)
    {
        ValidarData(data);

        var registro = await ObterOuCriarRegistroAsync(data, cancellationToken);
        AtualizarRegistro(registro, permitirPedidos: false, motivo);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(data, _servicoDataLocal.ObterDataAtual(), registro);
    }

    public async Task<DisponibilidadeDataResposta> AlterarMotivoAsync(
        DateOnly data,
        string? motivo,
        CancellationToken cancellationToken = default)
    {
        ValidarData(data);

        var registro = await ObterRegistroAtivoAsync(data, cancellationToken);
        if (registro is null)
        {
            throw new InvalidOperationException(
                "Não existe disponibilidade cadastrada para essa data.");
        }

        var motivoNormalizado = NormalizarTextoOpcional(motivo);
        registro.Motivo = motivoNormalizado;
        registro.MensagemCliente = motivoNormalizado;
        registro.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(data, _servicoDataLocal.ObterDataAtual(), registro);
    }

    public async Task<DisponibilidadePublicaResposta> ListarPublicaAsync(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        CancellationToken cancellationToken = default)
    {
        var (inicio, fim) = NormalizarPeriodo(
            dataInicial,
            dataFinal,
            _servicoDataLocal.ObterDataAtual());
        var dataAtual = _servicoDataLocal.ObterDataAtual();
        var periodo = CriarPeriodo(inicio, fim).ToList();
        var registros = await ObterRegistrosAtivosAsync(inicio, fim, cancellationToken);
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);
        var diasSemana = periodo
            .Select(ConverterDiaSemana)
            .Distinct()
            .ToList();
        var horarios = await _dbContext.HorariosFuncionamento
            .AsNoTracking()
            .Where(horario =>
                diasSemana.Contains(horario.DiaSemana) &&
                horario.EstaAtivo)
            .ToListAsync(cancellationToken);
        var horariosPorDia = horarios
            .GroupBy(horario => horario.DiaSemana)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.ToList());

        var datas = periodo
            .Select(data =>
            {
                var avaliacaoData = DeveAplicarExcecaoPedidosTeste(data, dataAtual)
                    ? Liberar(data)
                    : AvaliarData(
                        data,
                        dataAtual,
                        registros.GetValueOrDefault(data));
                var validacao = avaliacaoData.PermitirPedidos
                    ? DeveAplicarExcecaoPedidosTeste(data, dataAtual)
                        ? avaliacaoData
                        : AvaliarRestaurante(
                            data,
                            dataAtual,
                            configuracao,
                            horariosPorDia)
                    : avaliacaoData;

                return new DisponibilidadeDataPublicaResposta
                {
                    Data = data,
                    Disponivel = validacao.PermitirPedidos,
                    PermitirPedidos = validacao.PermitirPedidos,
                    Motivo = validacao.MotivoBloqueio,
                    MotivoBloqueio = validacao.MotivoBloqueio
                };
            })
            .ToList();

        return new DisponibilidadePublicaResposta
        {
            DatasDisponiveis = datas
                .Where(data => data.PermitirPedidos)
                .Select(data => data.Data)
                .ToList(),
            DatasBloqueadas = datas
                .Where(data => !data.PermitirPedidos)
                .ToList(),
            Datas = datas
        };
    }

    public async Task<ValidacaoDisponibilidadePedidoResposta> ValidarPedidoAsync(
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        ValidarData(data);

        var dataAtual = _servicoDataLocal.ObterDataAtual();
        if (DeveAplicarExcecaoPedidosTeste(data, dataAtual))
        {
            return Liberar(data);
        }

        var registro = await ObterRegistroAtivoAsync(data, cancellationToken);
        var avaliacaoData = AvaliarData(data, dataAtual, registro);
        if (!avaliacaoData.PermitirPedidos)
        {
            return new ValidacaoDisponibilidadePedidoResposta
            {
                Data = data,
                PermitirPedidos = false,
                MotivoBloqueio = avaliacaoData.MotivoBloqueio
            };
        }

        var avaliacaoRestaurante = await AvaliarRestauranteAsync(data, cancellationToken);
        return new ValidacaoDisponibilidadePedidoResposta
        {
            Data = data,
            PermitirPedidos = avaliacaoRestaurante.PermitirPedidos,
            MotivoBloqueio = avaliacaoRestaurante.MotivoBloqueio
        };
    }

    private async Task<Dictionary<DateOnly, FechamentoExcepcional>> ObterRegistrosAtivosAsync(
        DateOnly dataInicial,
        DateOnly dataFinal,
        CancellationToken cancellationToken)
    {
        var registros = await _dbContext.FechamentosExcepcionais
            .AsNoTracking()
            .Where(registro =>
                registro.DataFechamento >= dataInicial &&
                registro.DataFechamento <= dataFinal &&
                registro.EstaAtivo)
            .OrderByDescending(registro => registro.AtualizadoEm)
            .ToListAsync(cancellationToken);

        return registros
            .GroupBy(registro => registro.DataFechamento)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.First());
    }

    private async Task<FechamentoExcepcional?> ObterRegistroAtivoAsync(
        DateOnly data,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FechamentosExcepcionais
            .Where(registro => registro.DataFechamento == data && registro.EstaAtivo)
            .OrderByDescending(registro => registro.AtualizadoEm)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<FechamentoExcepcional> ObterOuCriarRegistroAsync(
        DateOnly data,
        CancellationToken cancellationToken)
    {
        var registro = await ObterRegistroAtivoAsync(data, cancellationToken);
        if (registro is not null)
        {
            return registro;
        }

        var agora = DateTimeOffset.UtcNow;
        registro = new FechamentoExcepcional
        {
            DataFechamento = data,
            DiaInteiro = true,
            EstaAtivo = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.FechamentosExcepcionais.AddAsync(registro, cancellationToken);

        return registro;
    }

    private async Task<ValidacaoDisponibilidadePedidoResposta> AvaliarRestauranteAsync(
        DateOnly data,
        CancellationToken cancellationToken)
    {
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        if (ConverterDiaSemana(data) == DiaSemana.Domingo)
        {
            return Bloquear(
                data,
                "Hoje não temos atendimento. Consulte o cardápio dos outros dias.");
        }

        if (configuracao is null)
        {
            return Bloquear(data, "Não conseguimos carregar o status do restaurante.");
        }

        if (!configuracao.EstaAtivo ||
            !configuracao.AceitaPedidos ||
            configuracao.ModoFuncionamento == ModoFuncionamento.FechadoManualmente)
        {
            return Bloquear(
                data,
                configuracao.MensagemFechado ?? "Restaurante fechado no momento.");
        }

        if (configuracao.ModoFuncionamento == ModoFuncionamento.AbertoManualmente)
        {
            return Liberar(data);
        }

        var horarios = await _dbContext.HorariosFuncionamento
            .AsNoTracking()
            .Where(horario =>
                horario.DiaSemana == ConverterDiaSemana(data) &&
                horario.EstaAtivo)
            .ToListAsync(cancellationToken);

        if (horarios.Count == 0)
        {
            return Bloquear(data, "Restaurante fechado nessa data.");
        }

        if (data != _servicoDataLocal.ObterDataAtual())
        {
            return Liberar(data);
        }

        var horaAtual = TimeOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        var abertoNoHorario = horarios.Any(horario =>
            horaAtual >= horario.HoraAbertura &&
            horaAtual <= horario.HoraFechamento);

        return abertoNoHorario
            ? Liberar(data)
            : Bloquear(
                data,
                configuracao.MensagemFechado ?? "Restaurante fechado no momento.");
    }

    private static ValidacaoDisponibilidadePedidoResposta AvaliarRestaurante(
        DateOnly data,
        DateOnly dataAtual,
        ConfiguracaoRestaurante? configuracao,
        IReadOnlyDictionary<DiaSemana, List<HorarioFuncionamento>> horariosPorDia)
    {
        if (ConverterDiaSemana(data) == DiaSemana.Domingo)
        {
            return Bloquear(
                data,
                "Hoje não temos atendimento. Consulte o cardápio dos outros dias.");
        }

        if (configuracao is null)
        {
            return Bloquear(data, "Não conseguimos carregar o status do restaurante.");
        }

        if (!configuracao.EstaAtivo ||
            !configuracao.AceitaPedidos ||
            configuracao.ModoFuncionamento == ModoFuncionamento.FechadoManualmente)
        {
            return Bloquear(
                data,
                configuracao.MensagemFechado ?? "Restaurante fechado no momento.");
        }

        if (configuracao.ModoFuncionamento == ModoFuncionamento.AbertoManualmente)
        {
            return Liberar(data);
        }

        if (!horariosPorDia.TryGetValue(ConverterDiaSemana(data), out var horarios) ||
            horarios.Count == 0)
        {
            return Bloquear(data, "Restaurante fechado nessa data.");
        }

        if (data != dataAtual)
        {
            return Liberar(data);
        }

        var horaAtual = TimeOnly.FromDateTime(DateTimeOffset.Now.DateTime);
        var abertoNoHorario = horarios.Any(horario =>
            horaAtual >= horario.HoraAbertura &&
            horaAtual <= horario.HoraFechamento);

        return abertoNoHorario
            ? Liberar(data)
            : Bloquear(
                data,
                configuracao.MensagemFechado ?? "Restaurante fechado no momento.");
    }

    private static void AtualizarRegistro(
        FechamentoExcepcional registro,
        bool permitirPedidos,
        string? motivo)
    {
        var motivoNormalizado = NormalizarTextoOpcional(motivo);
        registro.PermitirPedidos = permitirPedidos;
        registro.Motivo = motivoNormalizado;
        registro.MensagemCliente = motivoNormalizado;
        registro.DiaInteiro = true;
        registro.HoraInicio = null;
        registro.HoraFim = null;
        registro.EstaAtivo = true;
        registro.AtualizadoEm = DateTimeOffset.UtcNow;
    }

    private static DisponibilidadeDataResposta MapearResposta(
        DateOnly data,
        DateOnly dataAtual,
        FechamentoExcepcional? registro)
    {
        var avaliacao = AvaliarData(data, dataAtual, registro);
        var permitirPedidos = avaliacao.PermitirPedidos;

        return new DisponibilidadeDataResposta
        {
            Data = data,
            Status = permitirPedidos ? "Liberado" : "Bloqueado",
            Liberado = permitirPedidos,
            Bloqueado = !permitirPedidos,
            PermitirPedidos = permitirPedidos,
            Motivo = registro?.Motivo ?? avaliacao.MotivoBloqueio
        };
    }

    private static ValidacaoDisponibilidadePedidoResposta AvaliarData(
        DateOnly data,
        DateOnly dataAtual,
        FechamentoExcepcional? registro)
    {
        if (data < dataAtual)
        {
            return Bloquear(data, "Não é possível criar pedidos para dias anteriores.");
        }

        if (registro is { PermitirPedidos: false })
        {
            return Bloquear(
                data,
                registro.MensagemCliente ??
                    registro.Motivo ??
                    "Essa data está bloqueada para pedidos.");
        }

        if (data > dataAtual && registro is not { PermitirPedidos: true })
        {
            return Bloquear(data, "Essa data ainda não foi liberada para pedidos.");
        }

        return Liberar(data);
    }

    private static (DateOnly Inicio, DateOnly Fim) NormalizarPeriodo(
        DateOnly? dataInicial,
        DateOnly? dataFinal,
        DateOnly dataPadrao)
    {
        var inicio = dataInicial ?? dataPadrao;
        var fim = dataFinal ?? inicio.AddDays(QuantidadeDiasPadrao);

        if (inicio == default || fim == default)
        {
            throw new ArgumentException("As datas informadas são inválidas.");
        }

        if (fim < inicio)
        {
            throw new ArgumentException("A data final deve ser maior ou igual à data inicial.");
        }

        if (inicio.AddDays(QuantidadeMaximaDiasConsulta) < fim)
        {
            throw new ArgumentException(
                "O período de consulta não pode ultrapassar 366 dias.");
        }

        return (inicio, fim);
    }

    private static IEnumerable<DateOnly> CriarPeriodo(DateOnly dataInicial, DateOnly dataFinal)
    {
        for (var data = dataInicial; data <= dataFinal; data = data.AddDays(1))
        {
            yield return data;
        }
    }

    private static bool DeveAplicarExcecaoPedidosTeste(DateOnly data, DateOnly dataAtual)
    {
        return data == DataExcecaoPedidosTeste && dataAtual == DataExcecaoPedidosTeste;
    }

    private static ValidacaoDisponibilidadePedidoResposta Liberar(DateOnly data)
    {
        return new ValidacaoDisponibilidadePedidoResposta
        {
            Data = data,
            PermitirPedidos = true
        };
    }

    private static ValidacaoDisponibilidadePedidoResposta Bloquear(
        DateOnly data,
        string motivo)
    {
        return new ValidacaoDisponibilidadePedidoResposta
        {
            Data = data,
            PermitirPedidos = false,
            MotivoBloqueio = motivo
        };
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

    private static DiaSemana ConverterDiaSemana(DateOnly data)
    {
        return (DiaSemana)(int)data.ToDateTime(TimeOnly.MinValue).DayOfWeek;
    }
}
