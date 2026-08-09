using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using QuentinhasDaTininha.Api.Controllers;
using QuentinhasDaTininha.Infraestrutura.Qz.Configuracoes;
using QuentinhasDaTininha.Infraestrutura.Qz.Servicos;

namespace QuentinhasDaTininha.Tests;

public class ServicoQzSigningTests
{
    [Fact]
    public void Assinar_GeraAssinaturaRsaSha512Valida()
    {
        using var rsa = RSA.Create(2048);
        var servico = CriarServico(rsa.ExportPkcs8PrivateKeyPem());
        var dados = "dados enviados exatamente pelo qz";

        var assinatura = Convert.FromBase64String(servico.Assinar(dados));

        Assert.True(rsa.VerifyData(
            Encoding.UTF8.GetBytes(dados),
            assinatura,
            HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void Assinar_AceitaPrivateKeyComQuebrasEscapadas()
    {
        using var rsa = RSA.Create(2048);
        var privateKeyEscapada = rsa.ExportPkcs8PrivateKeyPem().Replace("\n", "\\n");
        var servico = CriarServico(privateKeyEscapada);
        var dados = "dados qz";

        var assinatura = Convert.FromBase64String(servico.Assinar(dados));

        Assert.True(rsa.VerifyData(
            Encoding.UTF8.GetBytes(dados),
            assinatura,
            HashAlgorithmName.SHA512,
            RSASignaturePadding.Pkcs1));
    }

    [Fact]
    public void ObterCertificado_NormalizaQuebrasEscapadas()
    {
        using var rsa = RSA.Create(2048);
        var servico = new ServicoQzSigning(Options.Create(new QzSigningConfiguracao
        {
            Certificate = "-----BEGIN CERTIFICATE-----\\ncertificado-publico\\n-----END CERTIFICATE-----",
            PrivateKey = rsa.ExportPkcs8PrivateKeyPem()
        }));

        var certificado = servico.ObterCertificado();

        Assert.Contains("\ncertificado-publico\n", certificado);
        Assert.DoesNotContain("\\n", certificado);
    }

    [Fact]
    public void Assinar_RejeitaDadosVazios()
    {
        using var rsa = RSA.Create(2048);
        var servico = CriarServico(rsa.ExportPkcs8PrivateKeyPem());

        Assert.Throws<ArgumentException>(() => servico.Assinar(string.Empty));
    }

    [Fact]
    public void Assinar_GeraErroControladoQuandoPrivateKeyInvalida()
    {
        var servico = CriarServico("private key invalida");

        var excecao = Assert.Throws<InvalidOperationException>(() => servico.Assinar("dados"));

        Assert.Contains("Private key QZ invalida", excecao.Message);
    }

    [Fact]
    public void AdminQzController_ExigeAutenticacao()
    {
        var atributo = typeof(AdminQzController)
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .FirstOrDefault();

        Assert.NotNull(atributo);
    }

    private static ServicoQzSigning CriarServico(string privateKey)
    {
        return new ServicoQzSigning(Options.Create(new QzSigningConfiguracao
        {
            Certificate = "-----BEGIN CERTIFICATE-----\ncertificado-publico\n-----END CERTIFICATE-----",
            PrivateKey = privateKey
        }));
    }
}
