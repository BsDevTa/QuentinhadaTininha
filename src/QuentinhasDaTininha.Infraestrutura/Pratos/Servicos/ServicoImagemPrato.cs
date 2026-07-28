using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Armazenamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Pratos.Servicos;

public class ServicoImagemPrato : IServicoImagemPrato
{
    private const string MarcadorUrlPublica = "/storage/v1/object/public/";

    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoArmazenamentoImagem _servicoArmazenamentoImagem;

    public ServicoImagemPrato(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoArmazenamentoImagem servicoArmazenamentoImagem)
    {
        _dbContext = dbContext;
        _servicoArmazenamentoImagem = servicoArmazenamentoImagem;
    }

    public async Task<string?> AtualizarImagemAsync(
        Guid pratoId,
        ArquivoUploadRequisicao arquivo,
        CancellationToken cancellationToken = default)
    {
        var prato = await _dbContext.Pratos
            .FirstOrDefaultAsync(prato => prato.Id == pratoId, cancellationToken);

        if (prato is null)
        {
            return null;
        }

        var urlAntiga = prato.UrlImagem;
        var imagemNova = await _servicoArmazenamentoImagem.EnviarAsync(
            arquivo,
            "pratos",
            cancellationToken);

        try
        {
            prato.UrlImagem = imagemNova.UrlPublica;
            prato.AtualizadoEm = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await TentarRemoverAsync(imagemNova.Caminho, cancellationToken);
            throw;
        }

        var caminhoImagemAntiga = ExtrairCaminho(urlAntiga);
        if (!string.IsNullOrWhiteSpace(caminhoImagemAntiga))
        {
            await TentarRemoverAsync(caminhoImagemAntiga, cancellationToken);
        }

        return imagemNova.UrlPublica;
    }

    public async Task<bool> RemoverImagemAsync(
        Guid pratoId,
        CancellationToken cancellationToken = default)
    {
        var prato = await _dbContext.Pratos
            .FirstOrDefaultAsync(prato => prato.Id == pratoId, cancellationToken);

        if (prato is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(prato.UrlImagem))
        {
            return true;
        }

        var caminhoImagem = ExtrairCaminho(prato.UrlImagem);

        prato.UrlImagem = null;
        prato.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(caminhoImagem))
        {
            await _servicoArmazenamentoImagem.RemoverAsync(caminhoImagem, cancellationToken);
        }

        return true;
    }

    private async Task TentarRemoverAsync(
        string caminho,
        CancellationToken cancellationToken)
    {
        try
        {
            await _servicoArmazenamentoImagem.RemoverAsync(caminho, cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static string? ExtrairCaminho(string? urlImagem)
    {
        if (string.IsNullOrWhiteSpace(urlImagem) ||
            !Uri.TryCreate(urlImagem, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var caminhoUrl = uri.AbsolutePath;
        var indiceMarcador = caminhoUrl.IndexOf(
            MarcadorUrlPublica,
            StringComparison.OrdinalIgnoreCase);

        if (indiceMarcador < 0)
        {
            return null;
        }

        var restante = caminhoUrl[(indiceMarcador + MarcadorUrlPublica.Length)..];
        var indiceSeparadorBucket = restante.IndexOf('/');

        if (indiceSeparadorBucket < 0 ||
            indiceSeparadorBucket == restante.Length - 1)
        {
            return null;
        }

        var caminhoObjeto = restante[(indiceSeparadorBucket + 1)..];
        var caminhoDecodificado = Uri.UnescapeDataString(caminhoObjeto);

        return CaminhoSeguro(caminhoDecodificado) ? caminhoDecodificado : null;
    }

    private static bool CaminhoSeguro(string caminho)
    {
        var segmentos = caminho.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segmentos.Length > 0 &&
            segmentos.All(segmento => segmento is not "." and not "..");
    }
}
