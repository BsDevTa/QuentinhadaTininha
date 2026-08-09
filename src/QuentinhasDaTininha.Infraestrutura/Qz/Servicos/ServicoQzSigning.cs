using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using QuentinhasDaTininha.Aplicacao.Qz.Interfaces;
using QuentinhasDaTininha.Infraestrutura.Qz.Configuracoes;

namespace QuentinhasDaTininha.Infraestrutura.Qz.Servicos;

public class ServicoQzSigning : IServicoQzSigning
{
    public const int TamanhoMaximoDadosAssinatura = 16 * 1024;

    private readonly QzSigningConfiguracao _configuracao;

    public ServicoQzSigning(IOptions<QzSigningConfiguracao> configuracao)
    {
        _configuracao = configuracao.Value;
    }

    public string ObterCertificado()
    {
        var certificado = NormalizarPem(_configuracao.Certificate);

        if (string.IsNullOrWhiteSpace(certificado))
        {
            throw new InvalidOperationException("Certificado publico QZ nao configurado.");
        }

        return certificado;
    }

    public string Assinar(string dados)
    {
        if (string.IsNullOrEmpty(dados))
        {
            throw new ArgumentException("Dados para assinatura QZ sao obrigatorios.");
        }

        if (dados.Length > TamanhoMaximoDadosAssinatura)
        {
            throw new ArgumentException("Dados para assinatura QZ excedem o tamanho permitido.");
        }

        var privateKey = NormalizarPem(_configuracao.PrivateKey);

        if (string.IsNullOrWhiteSpace(privateKey))
        {
            throw new InvalidOperationException("Private key QZ nao configurada.");
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKey.AsSpan());

            var bytes = Encoding.UTF8.GetBytes(dados);
            var assinatura = rsa.SignData(
                bytes,
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(assinatura);
        }
        catch (CryptographicException excecao)
        {
            throw new InvalidOperationException("Private key QZ invalida ou incompativel.", excecao);
        }
        catch (ArgumentException excecao)
        {
            throw new InvalidOperationException("Private key QZ invalida ou incompativel.", excecao);
        }
    }

    private static string NormalizarPem(string valor)
    {
        return valor
            .Replace("\\r\\n", "\n", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal)
            .Trim();
    }
}
