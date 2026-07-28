using System.Net;
using System.Net.Http.Headers;
using QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Armazenamento.Interfaces;

namespace QuentinhasDaTininha.Infraestrutura.Armazenamento.Servicos;

public class ServicoSupabaseStorage : IServicoArmazenamentoImagem
{
    private const long TamanhoMaximoEmBytes = 5 * 1024 * 1024;
    private const string MensagemErroUpload =
        "Não foi possível enviar a imagem para o armazenamento.";
    private const string MensagemErroRemocao =
        "Não foi possível remover a imagem do armazenamento.";

    private static readonly IReadOnlyDictionary<string, string[]> ExtensoesPorTipoConteudo =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = new[] { ".jpg", ".jpeg" },
            ["image/png"] = new[] { ".png" },
            ["image/webp"] = new[] { ".webp" }
        };

    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly string _chaveServico;
    private readonly string _bucket;

    public ServicoSupabaseStorage(
        HttpClient httpClient,
        string url,
        string chaveServico,
        string bucket)
    {
        _httpClient = httpClient;
        _url = url.Trim().TrimEnd('/');
        _chaveServico = chaveServico;
        _bucket = bucket.Trim();
    }

    public async Task<ArquivoUploadResposta> EnviarAsync(
        ArquivoUploadRequisicao requisicao,
        string pasta,
        CancellationToken cancellationToken = default)
    {
        var imagem = await ValidarImagemAsync(requisicao, cancellationToken);
        var caminho = GerarCaminho(pasta, imagem.Extensao);
        var urlObjeto = MontarUrlObjeto(caminho);

        using var conteudo = new ByteArrayContent(imagem.Bytes);
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(imagem.TipoConteudo);

        using var requisicaoHttp = new HttpRequestMessage(HttpMethod.Post, urlObjeto)
        {
            Content = conteudo
        };

        AdicionarCabecalhosAutenticacao(requisicaoHttp);
        requisicaoHttp.Headers.TryAddWithoutValidation("x-upsert", "false");

        using var resposta = await EnviarHttpAsync(
            requisicaoHttp,
            MensagemErroUpload,
            cancellationToken);

        if (!resposta.IsSuccessStatusCode)
        {
            _ = await resposta.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(MensagemErroUpload);
        }

        return new ArquivoUploadResposta
        {
            Caminho = caminho,
            UrlPublica = $"{_url}/storage/v1/object/public/{Uri.EscapeDataString(_bucket)}/{caminho}"
        };
    }

    public async Task RemoverAsync(
        string caminho,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(caminho))
        {
            return;
        }

        using var requisicaoHttp = new HttpRequestMessage(
            HttpMethod.Delete,
            MontarUrlObjeto(caminho));

        AdicionarCabecalhosAutenticacao(requisicaoHttp);

        using var resposta = await EnviarHttpAsync(
            requisicaoHttp,
            MensagemErroRemocao,
            cancellationToken);

        if (resposta.IsSuccessStatusCode ||
            resposta.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        _ = await resposta.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(MensagemErroRemocao);
    }

    private static async Task<ImagemValidada> ValidarImagemAsync(
        ArquivoUploadRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao is null || requisicao.Conteudo is null)
        {
            throw new ArgumentException("A imagem é obrigatória.");
        }

        if (requisicao.Tamanho <= 0)
        {
            throw new ArgumentException("A imagem não pode estar vazia.");
        }

        if (requisicao.Tamanho > TamanhoMaximoEmBytes)
        {
            throw new ArgumentException("A imagem deve possuir no máximo 5 MB.");
        }

        var tipoConteudo = requisicao.TipoConteudo.Trim().ToLowerInvariant();
        var extensao = Path.GetExtension(requisicao.NomeArquivo).ToLowerInvariant();

        if (!ExtensoesPorTipoConteudo.TryGetValue(tipoConteudo, out var extensoesPermitidas) ||
            !extensoesPermitidas.Contains(extensao, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Formato de imagem não permitido.");
        }

        await using var memoria = new MemoryStream();
        await requisicao.Conteudo.CopyToAsync(memoria, cancellationToken);
        var bytes = memoria.ToArray();

        if (bytes.Length == 0)
        {
            throw new ArgumentException("A imagem não pode estar vazia.");
        }

        if (bytes.Length > TamanhoMaximoEmBytes)
        {
            throw new ArgumentException("A imagem deve possuir no máximo 5 MB.");
        }

        if (!AssinaturaCorresponde(tipoConteudo, bytes))
        {
            throw new ArgumentException(
                "O conteúdo do arquivo não corresponde a uma imagem válida.");
        }

        return new ImagemValidada(bytes, tipoConteudo, extensao);
    }

    private static bool AssinaturaCorresponde(
        string tipoConteudo,
        IReadOnlyList<byte> bytes)
    {
        return tipoConteudo switch
        {
            "image/jpeg" => bytes.Count >= 3 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8 &&
                bytes[2] == 0xFF,
            "image/png" => bytes.Count >= 8 &&
                bytes[0] == 0x89 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x4E &&
                bytes[3] == 0x47 &&
                bytes[4] == 0x0D &&
                bytes[5] == 0x0A &&
                bytes[6] == 0x1A &&
                bytes[7] == 0x0A,
            "image/webp" => bytes.Count >= 12 &&
                bytes[0] == 0x52 &&
                bytes[1] == 0x49 &&
                bytes[2] == 0x46 &&
                bytes[3] == 0x46 &&
                bytes[8] == 0x57 &&
                bytes[9] == 0x45 &&
                bytes[10] == 0x42 &&
                bytes[11] == 0x50,
            _ => false
        };
    }

    private static string GerarCaminho(string pasta, string extensao)
    {
        var pastaNormalizada = NormalizarPasta(pasta);
        var agora = DateTimeOffset.UtcNow;

        return string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{pastaNormalizada}/{agora:yyyy}/{agora:MM}/{Guid.NewGuid()}{extensao}");
    }

    private static string NormalizarPasta(string pasta)
    {
        if (string.IsNullOrWhiteSpace(pasta))
        {
            return "imagens";
        }

        var segmentos = pasta
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segmento => segmento.Trim())
            .ToList();

        if (segmentos.Count == 0 ||
            segmentos.Any(segmento =>
                segmento == "." ||
                segmento == ".." ||
                segmento.Any(caractere =>
                    !char.IsLetterOrDigit(caractere) &&
                    caractere != '-' &&
                    caractere != '_')))
        {
            throw new InvalidOperationException(MensagemErroUpload);
        }

        return string.Join("/", segmentos);
    }

    private string MontarUrlObjeto(string caminho)
    {
        var caminhoCodificado = string.Join(
            "/",
            caminho
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

        return $"{_url}/storage/v1/object/{Uri.EscapeDataString(_bucket)}/{caminhoCodificado}";
    }

    private void AdicionarCabecalhosAutenticacao(HttpRequestMessage requisicaoHttp)
    {
        requisicaoHttp.Headers.TryAddWithoutValidation("apikey", _chaveServico);
        requisicaoHttp.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _chaveServico);
    }

    private async Task<HttpResponseMessage> EnviarHttpAsync(
        HttpRequestMessage requisicaoHttp,
        string mensagemErro,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(requisicaoHttp, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException(mensagemErro);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(mensagemErro);
        }
    }

    private sealed record ImagemValidada(
        byte[] Bytes,
        string TipoConteudo,
        string Extensao);
}
