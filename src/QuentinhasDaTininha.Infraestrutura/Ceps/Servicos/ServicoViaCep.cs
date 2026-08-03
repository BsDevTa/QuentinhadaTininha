using System.Net.Http.Json;
using System.Text.Json.Serialization;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;

namespace QuentinhasDaTininha.Infraestrutura.Ceps.Servicos;

public class ServicoViaCep : IServicoCep
{
    private readonly HttpClient _httpClient;

    public ServicoViaCep(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EnderecoCepResposta?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var cepNumerico = SomenteNumeros(cep);
        if (cepNumerico.Length != 8)
        {
            throw new ArgumentException("Informe um CEP com 8 números.");
        }

        try
        {
            var resposta = await _httpClient.GetAsync(
                $"{cepNumerico}/json/",
                cancellationToken);

            if (!resposta.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    "Não foi possível consultar o CEP agora. Tente novamente em alguns instantes.");
            }

            var endereco = await resposta.Content
                .ReadFromJsonAsync<ViaCepResposta>(cancellationToken);

            if (endereco is null || endereco.Erro)
            {
                return null;
            }

            return new EnderecoCepResposta
            {
                Cep = cepNumerico,
                Logradouro = NormalizarOpcional(endereco.Logradouro),
                Bairro = NormalizarObrigatorio(endereco.Bairro),
                Cidade = NormalizarObrigatorio(endereco.Localidade),
                Estado = NormalizarObrigatorio(endereco.Uf).ToUpperInvariant()
            };
        }
        catch (TaskCanceledException excecao) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                "Não foi possível consultar o CEP agora. Tente novamente em alguns instantes.",
                excecao);
        }
        catch (HttpRequestException excecao)
        {
            throw new InvalidOperationException(
                "Não foi possível consultar o CEP agora. Tente novamente em alguns instantes.",
                excecao);
        }
    }

    private static string SomenteNumeros(string valor)
    {
        return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private static string NormalizarObrigatorio(string? valor)
    {
        return valor?.Trim() ?? string.Empty;
    }

    private static string? NormalizarOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private class ViaCepResposta
    {
        [JsonPropertyName("cep")]
        public string? Cep { get; set; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; set; }

        [JsonPropertyName("uf")]
        public string? Uf { get; set; }

        [JsonPropertyName("erro")]
        public bool Erro { get; set; }
    }
}
