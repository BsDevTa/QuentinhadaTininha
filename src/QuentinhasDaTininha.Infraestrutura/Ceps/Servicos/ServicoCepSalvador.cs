using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Utilitarios;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Ceps.Servicos;

public class ServicoCepSalvador : IServicoCepSalvador
{
    private const int TamanhoLotePadrao = 1000;

    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoCepSalvador(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CepSalvadorImportacaoResposta> ImportarAsync(
        IReadOnlyCollection<CepSalvadorImportacaoItem> itens,
        CancellationToken cancellationToken = default,
        int tamanhoLote = TamanhoLotePadrao)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var resposta = new CepSalvadorImportacaoResposta
        {
            TotalRecebidos = itens.Count
        };
        var registros = new Dictionary<string, DadosCepSalvadorImportacao>();
        var linha = 0;

        foreach (var item in itens)
        {
            linha++;
            var linhaRelatorio = item.LinhaOrigem ?? linha;
            var registro = NormalizarItem(item, linhaRelatorio, resposta);
            if (registro is null)
            {
                continue;
            }

            resposta.Validos++;

            if (registros.ContainsKey(registro.Cep))
            {
                resposta.Duplicados++;
                resposta.Ignorados++;
            }

            registros[registro.Cep] = registro;
        }

        if (registros.Count == 0)
        {
            return resposta;
        }

        IDbContextTransaction? transacao = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transacao = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            }

            var tamanhoLoteEfetivo = tamanhoLote <= 0
                ? TamanhoLotePadrao
                : tamanhoLote;
            var agora = DateTimeOffset.UtcNow;

            foreach (var lote in registros.Values.Chunk(tamanhoLoteEfetivo))
            {
                var ceps = lote
                    .Select(registro => registro.Cep)
                    .ToList();
                var existentes = await _dbContext.CepsSalvador
                    .Where(cep => ceps.Contains(cep.Cep))
                    .ToDictionaryAsync(cep => cep.Cep, cancellationToken);

                foreach (var registro in lote)
                {
                    if (existentes.TryGetValue(registro.Cep, out var cepExistente))
                    {
                        cepExistente.Logradouro = registro.Logradouro;
                        cepExistente.Bairro = registro.Bairro;
                        cepExistente.BairroNormalizado = registro.BairroNormalizado;
                        cepExistente.Cidade = registro.Cidade;
                        cepExistente.Uf = registro.Uf;
                        cepExistente.Ativo = true;
                        cepExistente.AtualizadoEm = agora;
                        resposta.Atualizados++;
                        continue;
                    }

                    await _dbContext.CepsSalvador.AddAsync(new CepSalvador
                    {
                        Cep = registro.Cep,
                        Logradouro = registro.Logradouro,
                        Bairro = registro.Bairro,
                        BairroNormalizado = registro.BairroNormalizado,
                        Cidade = registro.Cidade,
                        Uf = registro.Uf,
                        Ativo = true,
                        CriadoEm = agora,
                        AtualizadoEm = agora
                    }, cancellationToken);
                    resposta.Inseridos++;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
            }

            if (transacao is not null)
            {
                await transacao.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (transacao is not null)
            {
                await transacao.DisposeAsync();
            }
        }

        return resposta;
    }

    private static DadosCepSalvadorImportacao? NormalizarItem(
        CepSalvadorImportacaoItem item,
        int linha,
        CepSalvadorImportacaoResposta resposta)
    {
        var cep = NormalizadorCep.SomenteNumeros(item.Cep);
        if (cep.Length != 8)
        {
            AdicionarErro(resposta, linha, "CEP deve conter exatamente 8 numeros.");
            return null;
        }

        var bairro = NormalizarTextoObrigatorio(item.Bairro);
        if (bairro is null)
        {
            AdicionarErro(resposta, linha, "Bairro e obrigatorio.");
            return null;
        }

        if (bairro.Length > 120)
        {
            AdicionarErro(resposta, linha, "Bairro deve ter no maximo 120 caracteres.");
            return null;
        }

        var cidade = NormalizarTextoObrigatorio(item.Cidade);
        var uf = NormalizarUf(item.Uf);
        if (cidade is null || uf is null || !EhSalvadorBahia(cidade, uf))
        {
            AdicionarErro(resposta, linha, "Somente CEPs de Salvador/BA sao aceitos.");
            return null;
        }

        var logradouro = NormalizarTextoOpcional(item.Logradouro);
        if (logradouro.Length > 180)
        {
            AdicionarErro(resposta, linha, "Logradouro deve ter no maximo 180 caracteres.");
            return null;
        }

        return new DadosCepSalvadorImportacao(
            cep,
            logradouro,
            bairro,
            NormalizadorBairro.NormalizarParaComparacao(bairro),
            cidade,
            uf);
    }

    private static string? NormalizarTextoObrigatorio(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? null
            : NormalizadorBairro.LimparNome(texto);
    }

    private static string NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto)
            ? string.Empty
            : NormalizadorBairro.LimparNome(texto);
    }

    private static string? NormalizarUf(string? uf)
    {
        return string.IsNullOrWhiteSpace(uf)
            ? null
            : uf.Trim().ToUpperInvariant();
    }

    private static bool EhSalvadorBahia(string cidade, string uf)
    {
        return NormalizadorBairro.NormalizarParaComparacao(cidade) == "salvador" &&
            uf == "BA";
    }

    private static void AdicionarErro(
        CepSalvadorImportacaoResposta resposta,
        int linha,
        string erro)
    {
        resposta.Invalidos++;
        resposta.Ignorados++;
        resposta.Erros.Add($"Linha {linha}: {erro}");
    }

    private sealed record DadosCepSalvadorImportacao(
        string Cep,
        string Logradouro,
        string Bairro,
        string BairroNormalizado,
        string Cidade,
        string Uf);
}
