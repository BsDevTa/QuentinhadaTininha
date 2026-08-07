using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Utilitarios;
using QuentinhasDaTininha.Infraestrutura.FretesBairros.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Tests;

public class ServicoFreteBairroTests
{
    [Fact]
    public async Task CriarAsync_CriaAliasAutomaticoComNomeNormalizado()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(dbContext);

        var resposta = await servico.CriarAsync(new FreteBairroSalvarRequisicao
        {
            Bairro = "Imbuí",
            Valor = 12m,
            Ativo = true
        });

        var alias = await dbContext.FretesBairrosAliases.SingleAsync();
        Assert.Equal(resposta.Id, alias.FreteBairroId);
        Assert.Equal("imbui", alias.AliasNormalizado);
        Assert.True(alias.Ativo);
        Assert.True(alias.GeradoAutomaticamente);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoViaCepRetornaBairroCriadoNoAdmin_UsaAliasAutomatico()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "41720000",
                Bairro = "Imbuí",
                Cidade = "Salvador",
                Estado = "BA"
            });
        await servico.CriarAsync(new FreteBairroSalvarRequisicao
        {
            Bairro = "Imbuí",
            Valor = 12m,
            Ativo = true
        });

        var resposta = await servico.ConsultarPorCepAsync("41720000");

        Assert.True(resposta.Atendido);
        Assert.Equal("Imbuí", resposta.Bairro);
        Assert.Equal("Imbuí", resposta.BairroFrete);
        Assert.Equal(12m, resposta.ValorFrete);
    }

    [Fact]
    public async Task CriarAsync_QuandoBairroTemAcento_NormalizaAliasAutomatico()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(dbContext);

        await servico.CriarAsync(new FreteBairroSalvarRequisicao
        {
            Bairro = "São Marcos",
            Valor = 10m,
            Ativo = true
        });

        var alias = await dbContext.FretesBairrosAliases.SingleAsync();
        Assert.Equal("sao marcos", alias.AliasNormalizado);
        Assert.True(alias.GeradoAutomaticamente);
    }

    [Fact]
    public async Task CriarAsync_QuandoAliasJaExiste_NaoDuplicaNemCriaBairroSemAlias()
    {
        await using var dbContext = CriarDbContext();
        var freteExistente = CriarFreteBairro("Outro bairro", 8m);
        dbContext.Add(freteExistente);
        dbContext.Add(CriarAlias(freteExistente, "Imbuí"));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            servico.CriarAsync(new FreteBairroSalvarRequisicao
            {
                Bairro = "Imbuí",
                Valor = 12m,
                Ativo = true
            }));

        Assert.Equal(
            1,
            await dbContext.FretesBairrosAliases.CountAsync(
                alias => alias.AliasNormalizado == "imbui"));
        Assert.False(await dbContext.FretesBairros.AnyAsync(
            frete => frete.BairroNormalizado == "imbui"));
    }

    [Fact]
    public async Task AtualizarAsync_QuandoNomeMuda_AtualizaAliasAutomatico()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(dbContext);
        var frete = await servico.CriarAsync(new FreteBairroSalvarRequisicao
        {
            Bairro = "Imbuí",
            Valor = 12m,
            Ativo = true
        });

        await servico.AtualizarAsync(
            frete.Id,
            new FreteBairroSalvarRequisicao
            {
                Bairro = "Imbuí Novo",
                Valor = 13m,
                Ativo = true
            });

        var alias = await dbContext.FretesBairrosAliases.SingleAsync(
            alias => alias.GeradoAutomaticamente);
        Assert.Equal("imbui novo", alias.AliasNormalizado);
        Assert.True(alias.Ativo);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoExisteAliasTecnico_MantemAliasTecnicoSemAlterar()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(dbContext);
        var freteResposta = await servico.CriarAsync(new FreteBairroSalvarRequisicao
        {
            Bairro = "Imbuí",
            Valor = 12m,
            Ativo = true
        });
        var frete = await dbContext.FretesBairros.SingleAsync(
            frete => frete.Id == freteResposta.Id);
        dbContext.Add(CriarAlias(frete, "Imbuí antigo"));
        await dbContext.SaveChangesAsync();

        await servico.AtualizarAsync(
            freteResposta.Id,
            new FreteBairroSalvarRequisicao
            {
                Bairro = "Imbuí Novo",
                Valor = 13m,
                Ativo = true
            });

        var aliasAutomatico = await dbContext.FretesBairrosAliases.SingleAsync(
            alias => alias.GeradoAutomaticamente);
        var aliasTecnico = await dbContext.FretesBairrosAliases.SingleAsync(
            alias => !alias.GeradoAutomaticamente);
        Assert.Equal("imbui novo", aliasAutomatico.AliasNormalizado);
        Assert.Equal("imbui antigo", aliasTecnico.AliasNormalizado);
        Assert.True(aliasTecnico.Ativo);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepExisteEmFreteCepECepSalvador_FreteCepTemPrioridade()
    {
        await using var dbContext = CriarDbContext();
        var freteCep = CriarFreteBairro("Vila Vale", 3m);
        var freteBairro = CriarFreteBairro("Engenho Velho da Federação", 12m);
        dbContext.AddRange(freteCep, freteBairro);
        dbContext.Add(CriarFreteCep(freteCep, "40221005"));
        dbContext.Add(CriarCepSalvador("40221005", "Engenho Velho da Federação"));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(dbContext);

        var resposta = await servico.ConsultarPorCepAsync("40221005");

        Assert.True(resposta.Atendido);
        Assert.Equal("Engenho Velho da Federação", resposta.Bairro);
        Assert.Equal("Vila Vale", resposta.BairroFrete);
        Assert.Equal(3m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepSalvadorTemBairroCadastrado_UsaFreteDoBairro()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Imbuí", 12m);
        dbContext.Add(frete);
        dbContext.Add(CriarCepSalvador("41720000", "Imbuí"));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(dbContext);

        var resposta = await servico.ConsultarPorCepAsync("41720000");

        Assert.True(resposta.Atendido);
        Assert.Equal("41720-000", resposta.Cep);
        Assert.Equal("Imbuí", resposta.Bairro);
        Assert.Equal("Imbuí", resposta.BairroFrete);
        Assert.Equal(12m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoAdministradorAlteraPrecoComCepSalvador_RetornaValorAtualizado()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Pituba", 10m);
        dbContext.Add(frete);
        dbContext.Add(CriarCepSalvador("41810000", "Pituba"));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(dbContext);

        var primeiraConsulta = await servico.ConsultarPorCepAsync("41810000");
        frete.Valor = 12m;
        await dbContext.SaveChangesAsync();
        var segundaConsulta = await servico.ConsultarPorCepAsync("41810000");

        Assert.Equal(10m, primeiraConsulta.ValorFrete);
        Assert.Equal(12m, segundaConsulta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepSalvadorNaoTemBairroCadastrado_RetornaNaoAtendido()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Add(CriarCepSalvador("41720000", "Imbuí"));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(dbContext);

        var resposta = await servico.ConsultarPorCepAsync("41720000");

        Assert.False(resposta.Atendido);
        Assert.Equal("Imbuí", resposta.Bairro);
        Assert.Null(resposta.BairroFrete);
        Assert.Null(resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepNaoExisteLocalmenteEViaCepEncontraSalvador_UsaFreteDoBairro()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Add(CriarFreteBairro("Imbuí", 12m));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "41720010",
                Logradouro = "Rua das Araras",
                Bairro = "Imbuí",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("41720010");

        Assert.True(resposta.Atendido);
        Assert.Equal("Rua das Araras", resposta.Logradouro);
        Assert.Equal("Imbuí", resposta.BairroFrete);
        Assert.Equal(12m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoViaCepRetornaOutraCidade_RetornaNaoAtendido()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Add(CriarFreteBairro("Centro", 12m));
        await dbContext.SaveChangesAsync();
        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "42700000",
                Bairro = "Centro",
                Cidade = "Lauro de Freitas",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("42700000");

        Assert.False(resposta.Atendido);
        Assert.Equal("Centro", resposta.Bairro);
        Assert.Null(resposta.BairroFrete);
        Assert.Null(resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepExiste_UsaFreteDoCepEspecifico()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Calabar", 3m);
        dbContext.Add(frete);
        dbContext.Add(CriarFreteCep(frete, "40226460"));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40226460",
                Bairro = "Alto das Pombas",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40226460");

        Assert.True(resposta.Atendido);
        Assert.Equal("Alto das Pombas", resposta.Bairro);
        Assert.Equal("Calabar", resposta.BairroFrete);
        Assert.Equal(3m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoNaoHaCepMasAliasExiste_UsaFreteDoAlias()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Graca", 7m);
        dbContext.Add(frete);
        dbContext.Add(CriarAlias(frete, "Graça"));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40150000",
                Bairro = "Graça",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40150000");

        Assert.True(resposta.Atendido);
        Assert.Equal("Graça", resposta.Bairro);
        Assert.Equal("Graca", resposta.BairroFrete);
        Assert.Equal(7m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepEAliasExistem_CepTemPrioridade()
    {
        await using var dbContext = CriarDbContext();
        var freteCep = CriarFreteBairro("Calabar", 3m);
        var freteAlias = CriarFreteBairro("Xisto", 5m);
        dbContext.AddRange(freteCep, freteAlias);
        dbContext.Add(CriarFreteCep(freteCep, "40226460"));
        dbContext.Add(CriarAlias(freteAlias, "Engenho Velho da Federação"));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40226460",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40226460");

        Assert.True(resposta.Atendido);
        Assert.Equal("Calabar", resposta.BairroFrete);
        Assert.Equal(3m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoNaoHaCepNemAlias_RetornaNaoAtendido()
    {
        await using var dbContext = CriarDbContext();
        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40000000",
                Bairro = "Federacao",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40000000");

        Assert.False(resposta.Atendido);
        Assert.Null(resposta.BairroFrete);
        Assert.Null(resposta.ValorFrete);
        Assert.Equal("No momento não realizamos entregas para esta localidade.", resposta.Mensagem);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoFreteCepEstaInativo_NaoUtiliza()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Vila Vale", 3m);
        dbContext.Add(frete);
        dbContext.Add(CriarFreteCep(frete, "40221005", ativo: false));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40221005",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40221005");

        Assert.False(resposta.Atendido);
        Assert.Null(resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoFreteBairroRelacionadoEstaInativo_NaoConsideraAtendido()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Muriçoca", 6m, ativo: false);
        dbContext.Add(frete);
        dbContext.Add(CriarFreteCep(frete, "40221005"));
        dbContext.Add(CriarAlias(frete, "Engenho Velho da Federação"));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40221005",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40221005");

        Assert.False(resposta.Atendido);
        Assert.Null(resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoAliasEstaInativo_NaoUtiliza()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Xisto", 5m);
        dbContext.Add(frete);
        dbContext.Add(CriarAlias(frete, "Engenho Velho da Federação", ativo: false));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40221005",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var resposta = await servico.ConsultarPorCepAsync("40221005");

        Assert.False(resposta.Atendido);
        Assert.Null(resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoCepTemMascara_ConsultaBancoComApenasNumeros()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Vila Vale", 3m);
        dbContext.Add(frete);
        dbContext.Add(CriarFreteCep(frete, "40221005"));
        await dbContext.SaveChangesAsync();
        var servicoCep = new ServicoCepFake(new EnderecoCepResposta
        {
            Cep = "40221005",
            Bairro = "Engenho Velho da Federação",
            Cidade = "Salvador",
            Estado = "BA"
        });
        var servico = new ServicoFreteBairro(dbContext, servicoCep);

        var resposta = await servico.ConsultarPorCepAsync("40221-005");

        Assert.True(resposta.Atendido);
        Assert.Equal("40221005", servicoCep.UltimoCepConsultado);
        Assert.Equal(3m, resposta.ValorFrete);
    }

    [Fact]
    public async Task ConsultarPorCepAsync_QuandoAdministradorAlteraPreco_RetornaValorAtualizado()
    {
        await using var dbContext = CriarDbContext();
        var frete = CriarFreteBairro("Xisto", 5m);
        dbContext.Add(frete);
        dbContext.Add(CriarFreteCep(frete, "40221005"));
        await dbContext.SaveChangesAsync();

        var servico = CriarServico(
            dbContext,
            new EnderecoCepResposta
            {
                Cep = "40221005",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA"
            });

        var primeiraConsulta = await servico.ConsultarPorCepAsync("40221005");
        frete.Valor = 6m;
        await dbContext.SaveChangesAsync();
        var segundaConsulta = await servico.ConsultarPorCepAsync("40221005");

        Assert.Equal(5m, primeiraConsulta.ValorFrete);
        Assert.Equal(6m, segundaConsulta.ValorFrete);
    }

    private static QuentinhasDaTininhaDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<QuentinhasDaTininhaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuentinhasDaTininhaDbContext(options);
    }

    private static ServicoFreteBairro CriarServico(
        QuentinhasDaTininhaDbContext dbContext,
        EnderecoCepResposta? endereco = null)
    {
        return new ServicoFreteBairro(dbContext, new ServicoCepFake(endereco));
    }

    private static FreteBairro CriarFreteBairro(
        string bairro,
        decimal valor,
        bool ativo = true)
    {
        return new FreteBairro
        {
            Bairro = bairro,
            BairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro),
            Valor = valor,
            Ativo = ativo
        };
    }

    private static FreteCep CriarFreteCep(
        FreteBairro freteBairro,
        string cep,
        bool ativo = true)
    {
        return new FreteCep
        {
            FreteBairro = freteBairro,
            FreteBairroId = freteBairro.Id,
            Cep = NormalizadorCep.SomenteNumeros(cep),
            Ativo = ativo
        };
    }

    private static CepSalvador CriarCepSalvador(
        string cep,
        string bairro,
        string logradouro = "Rua Teste",
        string cidade = "Salvador",
        string uf = "BA",
        bool ativo = true)
    {
        return new CepSalvador
        {
            Cep = NormalizadorCep.SomenteNumeros(cep),
            Logradouro = logradouro,
            Bairro = bairro,
            BairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro),
            Cidade = cidade,
            Uf = uf,
            Ativo = ativo
        };
    }

    private static FreteBairroAlias CriarAlias(
        FreteBairro freteBairro,
        string alias,
        bool ativo = true,
        bool geradoAutomaticamente = false)
    {
        return new FreteBairroAlias
        {
            FreteBairro = freteBairro,
            FreteBairroId = freteBairro.Id,
            AliasNormalizado = NormalizadorBairro.NormalizarParaComparacao(alias),
            Ativo = ativo,
            GeradoAutomaticamente = geradoAutomaticamente
        };
    }

    private sealed class ServicoCepFake : IServicoCep
    {
        private readonly EnderecoCepResposta? _endereco;

        public ServicoCepFake(EnderecoCepResposta? endereco)
        {
            _endereco = endereco;
        }

        public string? UltimoCepConsultado { get; private set; }

        public Task<EnderecoCepResposta?> ConsultarAsync(
            string cep,
            CancellationToken cancellationToken = default)
        {
            UltimoCepConsultado = cep;
            return Task.FromResult(_endereco);
        }
    }
}
