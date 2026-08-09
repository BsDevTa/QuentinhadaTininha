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
        if (string.IsNullOrWhiteSpace(_configuracao.Certificate))
        {
            throw new InvalidOperationException("Certificado publico QZ nao configurado.");
        }

        return _configuracao.Certificate;
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

        if (string.IsNullOrWhiteSpace(_configuracao.PrivateKey))
        {
            throw new InvalidOperationException("Private key QZ nao configurada.");
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(_configuracao.PrivateKey.AsSpan());

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
}
