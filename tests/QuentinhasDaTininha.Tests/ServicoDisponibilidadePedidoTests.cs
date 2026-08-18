using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Funcionamento.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Tests;

public class ServicoDisponibilidadePedidoTests
{
    [Fact]
    public async Task ListarPublicaAsync_AvaliaPeriodoSemDependerDeConsultaPorDia()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 3);
        await ConfigurarRestauranteAbertoAsync(dbContext);
        await LiberarHorarioIntegralAsync(dbContext);
        dbContext.FechamentosExcepcionais.Add(new FechamentoExcepcional
        {
            DataFechamento = hoje.AddDays(2),
            PermitirPedidos = true,
            EstaAtivo = true,
            Motivo = "Liberado",
            MensagemCliente = "Liberado"
        });
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje));

        var resposta = await servico.ListarPublicaAsync(hoje, hoje.AddDays(6));

        Assert.Equal(7, resposta.Datas.Count);
        Assert.Contains(
            resposta.Datas,
            data => data.Data == hoje.AddDays(2) && data.PermitirPedidos);
        Assert.Contains(
            resposta.Datas,
            data => data.Data.DayOfWeek == DayOfWeek.Sunday && !data.PermitirPedidos);
    }

    [Fact]
    public async Task ValidarPedidoAsync_BloqueiaNoveDeAgostoQuandoRestauranteEstaFechado()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 9);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(12, 0)));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.False(resposta.PermitirPedidos);
        Assert.Contains("Hoje não temos atendimento", resposta.MotivoBloqueio);
    }

    [Fact]
    public async Task ValidarPedidoAsync_RespeitaDataBloqueadaEmNoveDeAgosto()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 9);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        dbContext.FechamentosExcepcionais.Add(new FechamentoExcepcional
        {
            DataFechamento = hoje.AddDays(1),
            PermitirPedidos = false,
            EstaAtivo = true,
            Motivo = "Bloqueado",
            MensagemCliente = "Bloqueado"
        });
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(12, 0)));

        var resposta = await servico.ValidarPedidoAsync(hoje.AddDays(1));

        Assert.False(resposta.PermitirPedidos);
        Assert.Equal("Bloqueado", resposta.MotivoBloqueio);
    }

    [Fact]
    public async Task ListarPublicaAsync_RespeitaFechamentoDuranteNoveDeAgosto()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 9);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(12, 0)));

        var resposta = await servico.ListarPublicaAsync(hoje, hoje.AddDays(2));

        Assert.All(resposta.Datas, data => Assert.False(data.PermitirPedidos));
        Assert.Equal(3, resposta.DatasBloqueadas.Count);
    }

    [Fact]
    public async Task ValidarPedidoAsync_NaoMantemOverrideDoDiaAnterior()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 11);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext, hoje.AddDays(-1));
        await LiberarHorarioIntegralAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(12, 0)));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.True(resposta.PermitirPedidos);
    }

    [Fact]
    public async Task ValidarPedidoAsync_RespeitaOverrideManualNoMesmoDia()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 10);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext, hoje);
        await LiberarHorarioIntegralAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(12, 0)));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.False(resposta.PermitirPedidos);
        Assert.Equal("Fechado.", resposta.MotivoBloqueio);
    }

    [Theory]
    [InlineData(5, 59, false)]
    [InlineData(6, 0, true)]
    [InlineData(14, 59, true)]
    [InlineData(15, 0, false)]
    public async Task ValidarPedidoAsync_RespeitaHorarioAutomaticoDoDiaAtual(
        int hora,
        int minuto,
        bool esperadoAberto)
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 11);
        await ConfigurarRestauranteAbertoAsync(dbContext);
        await LiberarHorarioIntegralAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje, new TimeOnly(hora, minuto)));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.Equal(esperadoAberto, resposta.PermitirPedidos);
    }

    private static QuentinhasDaTininhaDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<QuentinhasDaTininhaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuentinhasDaTininhaDbContext(options);
    }

    private static async Task ConfigurarRestauranteAbertoAsync(
        QuentinhasDaTininhaDbContext dbContext)
    {
        await dbContext.ConfiguracoesRestaurante.AddAsync(new ConfiguracaoRestaurante
        {
            Nome = "Quentinhas da Tininha",
            EstaAtivo = true,
            AceitaPedidos = true,
            ModoFuncionamento = ModoFuncionamento.Automatico,
            MensagemAberto = "Estamos atendendo.",
            MensagemFechado = "Fechado."
        });
    }

    private static async Task ConfigurarRestauranteFechadoManualmenteAsync(
        QuentinhasDaTininhaDbContext dbContext,
        DateOnly? dataOverride = null)
    {
        await dbContext.ConfiguracoesRestaurante.AddAsync(new ConfiguracaoRestaurante
        {
            Nome = "Quentinhas da Tininha",
            EstaAtivo = true,
            AceitaPedidos = true,
            ModoFuncionamento = ModoFuncionamento.FechadoManualmente,
            DataOverrideManual = dataOverride,
            MensagemAberto = "Estamos atendendo.",
            MensagemFechado = "Fechado."
        });
    }

    private static async Task LiberarHorarioIntegralAsync(
        QuentinhasDaTininhaDbContext dbContext)
    {
        foreach (var dia in Enum.GetValues<DiaSemana>().Where(dia => dia != DiaSemana.Domingo))
        {
            await dbContext.HorariosFuncionamento.AddAsync(new HorarioFuncionamento
            {
                DiaSemana = dia,
                EstaAtivo = true,
                HoraAbertura = TimeOnly.MinValue,
                HoraFechamento = new TimeOnly(23, 59)
            });
        }
    }

    private sealed class ServicoDataLocalFake : IServicoDataLocal
    {
        private readonly DateOnly _dataAtual;
        private readonly TimeOnly _horaAtual;

        public ServicoDataLocalFake(DateOnly dataAtual, TimeOnly? horaAtual = null)
        {
            _dataAtual = dataAtual;
            _horaAtual = horaAtual ?? new TimeOnly(12, 0);
        }

        public DateTimeOffset ObterAgora()
        {
            return _dataAtual.ToDateTime(_horaAtual, DateTimeKind.Unspecified);
        }

        public DateOnly ObterDataAtual()
        {
            return _dataAtual;
        }
    }
}
