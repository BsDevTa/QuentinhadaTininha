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
    public async Task ValidarPedidoAsync_LiberaExcecaoTemporariaEmNoveDeAgosto()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 9);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.True(resposta.PermitirPedidos);
        Assert.Null(resposta.MotivoBloqueio);
    }

    [Fact]
    public async Task ValidarPedidoAsync_LiberaQualquerDataDuranteModoTesteDeNoveDeAgosto()
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
            new ServicoDataLocalFake(hoje));

        var resposta = await servico.ValidarPedidoAsync(hoje.AddDays(1));

        Assert.True(resposta.PermitirPedidos);
        Assert.Null(resposta.MotivoBloqueio);
    }

    [Fact]
    public async Task ListarPublicaAsync_LiberaPeriodoDuranteModoTesteDeNoveDeAgosto()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 9);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje));

        var resposta = await servico.ListarPublicaAsync(hoje, hoje.AddDays(2));

        Assert.All(resposta.Datas, data => Assert.True(data.PermitirPedidos));
        Assert.Empty(resposta.DatasBloqueadas);
    }

    [Fact]
    public async Task ValidarPedidoAsync_NaoMantemExcecaoTemporariaEmDezDeAgosto()
    {
        await using var dbContext = CriarDbContext();
        var hoje = new DateOnly(2026, 8, 10);
        await ConfigurarRestauranteFechadoManualmenteAsync(dbContext);
        await LiberarHorarioIntegralAsync(dbContext);
        await dbContext.SaveChangesAsync();
        var servico = new ServicoDisponibilidadePedido(
            dbContext,
            new ServicoDataLocalFake(hoje));

        var resposta = await servico.ValidarPedidoAsync(hoje);

        Assert.False(resposta.PermitirPedidos);
        Assert.Equal("Fechado.", resposta.MotivoBloqueio);
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
        QuentinhasDaTininhaDbContext dbContext)
    {
        await dbContext.ConfiguracoesRestaurante.AddAsync(new ConfiguracaoRestaurante
        {
            Nome = "Quentinhas da Tininha",
            EstaAtivo = true,
            AceitaPedidos = true,
            ModoFuncionamento = ModoFuncionamento.FechadoManualmente,
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

        public ServicoDataLocalFake(DateOnly dataAtual)
        {
            _dataAtual = dataAtual;
        }

        public DateOnly ObterDataAtual()
        {
            return _dataAtual;
        }
    }
}
