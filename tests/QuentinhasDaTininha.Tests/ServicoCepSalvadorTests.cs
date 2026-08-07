using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Infraestrutura.Ceps.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Tests;

public class ServicoCepSalvadorTests
{
    [Fact]
    public async Task ImportarAsync_QuandoCepDuplicadoNoLote_NaoDuplica()
    {
        await using var dbContext = CriarDbContext();
        var servico = new ServicoCepSalvador(dbContext);

        var resposta = await servico.ImportarAsync(
        [
            new CepSalvadorImportacaoItem
            {
                Cep = "41720-000",
                Logradouro = "Rua Antiga",
                Bairro = "Imbuí",
                Cidade = "Salvador",
                Uf = "BA"
            },
            new CepSalvadorImportacaoItem
            {
                Cep = "41720000",
                Logradouro = "Rua Nova",
                Bairro = "Imbuí",
                Cidade = "Salvador",
                Uf = "BA"
            }
        ]);

        var cep = await dbContext.CepsSalvador.SingleAsync();
        Assert.Equal(1, resposta.Inseridos);
        Assert.Equal(1, resposta.Ignorados);
        Assert.Equal("41720000", cep.Cep);
        Assert.Equal("Rua Nova", cep.Logradouro);
    }

    [Fact]
    public async Task ImportarAsync_QuandoCepJaExiste_AtualizaSemDuplicar()
    {
        await using var dbContext = CriarDbContext();
        var servico = new ServicoCepSalvador(dbContext);
        await servico.ImportarAsync(
        [
            new CepSalvadorImportacaoItem
            {
                Cep = "41810-000",
                Logradouro = "Rua Antiga",
                Bairro = "Pituba",
                Cidade = "Salvador",
                Uf = "BA"
            }
        ]);

        var resposta = await servico.ImportarAsync(
        [
            new CepSalvadorImportacaoItem
            {
                Cep = "41810000",
                Logradouro = "Rua Atualizada",
                Bairro = "Pituba",
                Cidade = "Salvador",
                Uf = "ba"
            }
        ]);

        var cep = await dbContext.CepsSalvador.SingleAsync();
        Assert.Equal(0, resposta.Inseridos);
        Assert.Equal(1, resposta.Atualizados);
        Assert.Equal("Rua Atualizada", cep.Logradouro);
        Assert.Equal("BA", cep.Uf);
    }

    [Fact]
    public async Task ImportarAsync_QuandoBairroTemAcento_NormalizaBairro()
    {
        await using var dbContext = CriarDbContext();
        var servico = new ServicoCepSalvador(dbContext);

        await servico.ImportarAsync(
        [
            new CepSalvadorImportacaoItem
            {
                Cep = "41250-000",
                Logradouro = "Rua São Marcos",
                Bairro = "São Marcos",
                Cidade = "Salvador",
                Uf = "BA"
            }
        ]);

        var cep = await dbContext.CepsSalvador.SingleAsync();
        Assert.Equal("sao marcos", cep.BairroNormalizado);
        Assert.Equal("São Marcos", cep.Bairro);
    }

    [Fact]
    public async Task ImportarAsync_QuandoCidadeNaoEhSalvador_IgnoraRegistro()
    {
        await using var dbContext = CriarDbContext();
        var servico = new ServicoCepSalvador(dbContext);

        var resposta = await servico.ImportarAsync(
        [
            new CepSalvadorImportacaoItem
            {
                Cep = "42700-000",
                Logradouro = "Rua Centro",
                Bairro = "Centro",
                Cidade = "Lauro de Freitas",
                Uf = "BA"
            }
        ]);

        Assert.Equal(0, await dbContext.CepsSalvador.CountAsync());
        Assert.Equal(1, resposta.Ignorados);
        Assert.Contains("Salvador/BA", resposta.Erros.Single());
    }

    private static QuentinhasDaTininhaDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<QuentinhasDaTininhaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuentinhasDaTininhaDbContext(options);
    }
}
