using System.Diagnostics;
using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Infraestrutura.Ceps.Servicos;
using QuentinhasDaTininha.Infraestrutura.FretesBairros.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

var raizRepositorio = EncontrarRaizRepositorio();
var argumentos = ArgumentosImportacao.Criar(args, raizRepositorio);

if (!File.Exists(argumentos.ArquivoCsv))
{
    Console.Error.WriteLine($"Arquivo nao encontrado: {argumentos.ArquivoCsv}");
    return 2;
}

var connectionString = ObterConnectionString(raizRepositorio);
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Connection string 'ConexaoPostgreSql' nao encontrada na configuracao da aplicacao.");
    return 2;
}

var parse = LerCsv(argumentos.ArquivoCsv);
if (parse.ErroCabecalho is not null)
{
    Console.Error.WriteLine(parse.ErroCabecalho);
    return 2;
}

var options = new DbContextOptionsBuilder<QuentinhasDaTininhaDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new QuentinhasDaTininhaDbContext(options);
var importador = new ServicoCepSalvador(dbContext);
var totalAntes = await ContarCepSalvadorAsync(dbContext);

var cronometro = Stopwatch.StartNew();
var resposta = await importador.ImportarAsync(
    parse.Itens,
    CancellationToken.None,
    argumentos.TamanhoLote);
cronometro.Stop();

var totalFinal = await ContarCepSalvadorAsync(dbContext);
var bairrosPorNome = await dbContext.CepsSalvador
    .AsNoTracking()
    .GroupBy(cep => new { cep.BairroNormalizado, cep.Bairro })
    .Select(grupo => new BairroContagem(
        grupo.Key.BairroNormalizado,
        grupo.Key.Bairro,
        grupo.Count()))
    .ToListAsync();
var bairros = bairrosPorNome
    .GroupBy(bairro => bairro.BairroNormalizado)
    .Select(grupo => new BairroContagem(
        grupo.Key,
        grupo
            .OrderByDescending(bairro => bairro.Total)
            .ThenBy(bairro => bairro.Bairro)
            .First()
            .Bairro,
        grupo.Sum(bairro => bairro.Total)))
    .OrderByDescending(bairro => bairro.Total)
    .ThenBy(bairro => bairro.BairroNormalizado)
    .ToList();
var consultas = await ConsultarAmostrasAsync(dbContext);

var erros = parse.Erros
    .Concat(resposta.Erros)
    .ToList();
var relatorio = new RelatorioImportacao(
    Arquivo: argumentos.ArquivoCsv,
    TamanhoLote: argumentos.TamanhoLote,
    TotalLido: parse.TotalLido,
    Validos: resposta.Validos,
    Inseridos: resposta.Inseridos,
    Atualizados: resposta.Atualizados,
    Ignorados: parse.Invalidos + resposta.Ignorados,
    Invalidos: parse.Invalidos + resposta.Invalidos,
    Duplicados: resposta.Duplicados,
    TotalAntes: totalAntes,
    TotalFinal: totalFinal,
    BairrosDistintos: bairros.Count,
    DuracaoSegundos: Math.Round(cronometro.Elapsed.TotalSeconds, 2),
    Erros: erros,
    Bairros: bairros,
    Consultas: consultas);

var caminhoRelatorio = Path.Combine(
    raizRepositorio,
    ".codex",
    "relatorio_importacao_ceps_salvador.json");
Directory.CreateDirectory(Path.GetDirectoryName(caminhoRelatorio)!);
await File.WriteAllTextAsync(
    caminhoRelatorio,
    JsonSerializer.Serialize(relatorio, new JsonSerializerOptions
    {
        WriteIndented = true
    }),
    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine($"Arquivo: {relatorio.Arquivo}");
Console.WriteLine($"Total lido: {relatorio.TotalLido}");
Console.WriteLine($"Validos: {relatorio.Validos}");
Console.WriteLine($"Inseridos: {relatorio.Inseridos}");
Console.WriteLine($"Atualizados: {relatorio.Atualizados}");
Console.WriteLine($"Ignorados: {relatorio.Ignorados}");
Console.WriteLine($"Invalidos: {relatorio.Invalidos}");
Console.WriteLine($"Duplicados: {relatorio.Duplicados}");
Console.WriteLine($"Total antes: {relatorio.TotalAntes}");
Console.WriteLine($"Total final cep_salvador: {relatorio.TotalFinal}");
Console.WriteLine($"Bairros distintos: {relatorio.BairrosDistintos}");
Console.WriteLine($"Duracao aproximada: {relatorio.DuracaoSegundos}s");
Console.WriteLine($"Erros: {relatorio.Erros.Count}");
Console.WriteLine($"Relatorio: {caminhoRelatorio}");
Console.WriteLine("Amostras de consulta:");
foreach (var consulta in consultas)
{
    Console.WriteLine(
        $"{consulta.Cep} | {consulta.Bairro} | atendido={consulta.Atendido} | bairroFrete={consulta.BairroFrete ?? "-"} | valor={consulta.ValorFrete?.ToString("0.00") ?? "-"}");
}

return 0;

static async Task<List<ConsultaCepResumo>> ConsultarAmostrasAsync(
    QuentinhasDaTininhaDbContext dbContext)
{
    var amostrasComFrete = await dbContext.CepsSalvador
        .AsNoTracking()
        .Join(
            dbContext.FretesBairros
                .AsNoTracking()
                .Where(frete => frete.Ativo),
            cep => cep.BairroNormalizado,
            frete => frete.BairroNormalizado,
            (cep, _) => cep.Cep)
        .OrderBy(cep => cep)
        .Take(5)
        .ToListAsync();
    var amostras = amostrasComFrete;

    if (amostras.Count < 5)
    {
        var cepsComplementares = await dbContext.CepsSalvador
            .AsNoTracking()
            .OrderBy(cep => cep.Cep)
            .Select(cep => cep.Cep)
            .Take(5)
            .ToListAsync();

        amostras = amostras
            .Concat(cepsComplementares)
            .Distinct()
            .Take(5)
            .ToList();
    }

    var servicoFrete = new ServicoFreteBairro(
        dbContext,
        new ServicoCepSemFallback());
    var consultas = new List<ConsultaCepResumo>();

    foreach (var cep in amostras)
    {
        var resposta = await servicoFrete.ConsultarPorCepAsync(cep);
        consultas.Add(new ConsultaCepResumo(
            resposta.Cep,
            resposta.Bairro,
            resposta.BairroFrete,
            resposta.Atendido,
            resposta.ValorFrete));
    }

    return consultas;
}

static async Task<int> ContarCepSalvadorAsync(
    QuentinhasDaTininhaDbContext dbContext)
{
    var conexao = dbContext.Database.GetDbConnection();
    var deveFechar = conexao.State != ConnectionState.Open;

    if (deveFechar)
    {
        await conexao.OpenAsync();
    }

    try
    {
        await using var comando = conexao.CreateCommand();
        comando.CommandText = "SELECT COUNT(*) FROM cep_salvador;";
        var resultado = await comando.ExecuteScalarAsync();

        return Convert.ToInt32(resultado);
    }
    finally
    {
        if (deveFechar)
        {
            await conexao.CloseAsync();
        }
    }
}

static ResultadoLeituraCsv LerCsv(string caminho)
{
    var itens = new List<CepSalvadorImportacaoItem>();
    var erros = new List<string>();
    var totalLido = 0;

    using var reader = new StreamReader(
        caminho,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
        detectEncodingFromByteOrderMarks: true);
    var cabecalho = reader.ReadLine();
    if (cabecalho is null)
    {
        return new ResultadoLeituraCsv(
            itens,
            totalLido,
            Invalidos: 0,
            erros,
            "Arquivo CSV vazio.");
    }

    var colunasCabecalho = ParseCsv(cabecalho);
    var cabecalhoEsperado = new[] { "Cep", "Logradouro", "Bairro", "Cidade", "Uf" };
    if (colunasCabecalho.Count != cabecalhoEsperado.Length ||
        colunasCabecalho
            .Select(coluna => coluna.Trim())
            .Where((coluna, indice) =>
                string.Equals(coluna, cabecalhoEsperado[indice], StringComparison.Ordinal))
            .Count() != cabecalhoEsperado.Length)
    {
        return new ResultadoLeituraCsv(
            itens,
            totalLido,
            Invalidos: 0,
            erros,
            "Cabecalho invalido. Use exatamente: Cep,Logradouro,Bairro,Cidade,Uf");
    }

    var linha = 1;
    while (!reader.EndOfStream)
    {
        linha++;
        var texto = reader.ReadLine();
        totalLido++;

        if (string.IsNullOrWhiteSpace(texto))
        {
            erros.Add($"Linha {linha}: linha vazia.");
            continue;
        }

        IReadOnlyList<string> colunas;
        try
        {
            colunas = ParseCsv(texto);
        }
        catch (FormatException excecao)
        {
            erros.Add($"Linha {linha}: {excecao.Message}");
            continue;
        }

        if (colunas.Count != 5)
        {
            erros.Add($"Linha {linha}: esperado 5 colunas, encontrado {colunas.Count}.");
            continue;
        }

        itens.Add(new CepSalvadorImportacaoItem
        {
            Cep = colunas[0],
            Logradouro = colunas[1],
            Bairro = colunas[2],
            Cidade = colunas[3],
            Uf = colunas[4],
            LinhaOrigem = linha
        });
    }

    return new ResultadoLeituraCsv(
        itens,
        totalLido,
        erros.Count,
        erros,
        ErroCabecalho: null);
}

static List<string> ParseCsv(string linha)
{
    var colunas = new List<string>();
    var coluna = new StringBuilder();
    var entreAspas = false;

    for (var indice = 0; indice < linha.Length; indice++)
    {
        var caractere = linha[indice];
        if (caractere == '"')
        {
            if (entreAspas &&
                indice + 1 < linha.Length &&
                linha[indice + 1] == '"')
            {
                coluna.Append('"');
                indice++;
                continue;
            }

            entreAspas = !entreAspas;
            continue;
        }

        if (caractere == ',' && !entreAspas)
        {
            colunas.Add(coluna.ToString());
            coluna.Clear();
            continue;
        }

        coluna.Append(caractere);
    }

    if (entreAspas)
    {
        throw new FormatException("campo com aspas sem fechamento.");
    }

    colunas.Add(coluna.ToString());
    return colunas;
}

static string? ObterConnectionString(string raizRepositorio)
{
    var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
        "Development";
    var apiDir = Path.Combine(
        raizRepositorio,
        "src",
        "QuentinhasDaTininha.Api");
    var arquivos = new[]
    {
        Path.Combine(apiDir, "appsettings.json"),
        Path.Combine(apiDir, $"appsettings.{ambiente}.json"),
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            "3e7382a7-c41b-4ccf-b785-4f735247b0c2",
            "secrets.json")
    };

    var connectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__ConexaoPostgreSql") ??
        Environment.GetEnvironmentVariable("ConnectionStrings:ConexaoPostgreSql");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return connectionString;
    }

    foreach (var arquivo in arquivos.Where(File.Exists))
    {
        var json = JsonDocument.Parse(File.ReadAllText(arquivo));
        connectionString =
            ObterValor(json.RootElement, "ConnectionStrings:ConexaoPostgreSql") ??
            ObterValor(json.RootElement, "ConnectionStrings", "ConexaoPostgreSql");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }
    }

    return null;
}

static string? ObterValor(JsonElement elemento, params string[] caminho)
{
    if (caminho.Length == 1 &&
        elemento.TryGetProperty(caminho[0], out var valorPlano) &&
        valorPlano.ValueKind == JsonValueKind.String)
    {
        return valorPlano.GetString();
    }

    var atual = elemento;
    foreach (var parte in caminho)
    {
        if (atual.ValueKind != JsonValueKind.Object ||
            !atual.TryGetProperty(parte, out atual))
        {
            return null;
        }
    }

    return atual.ValueKind == JsonValueKind.String
        ? atual.GetString()
        : null;
}

static string EncontrarRaizRepositorio()
{
    var diretorio = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (diretorio is not null)
    {
        if (File.Exists(Path.Combine(diretorio.FullName, "QuentinhasDaTininha.sln")))
        {
            return diretorio.FullName;
        }

        diretorio = diretorio.Parent;
    }

    throw new DirectoryNotFoundException("Nao foi possivel localizar a raiz do repositorio.");
}

sealed class ServicoCepSemFallback : IServicoCep
{
    public Task<EnderecoCepResposta?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "A validacao do runner deve usar apenas CEPs existentes em cep_salvador.");
    }
}

sealed record ArgumentosImportacao(
    string ArquivoCsv,
    int TamanhoLote)
{
    public static ArgumentosImportacao Criar(
        string[] args,
        string raizRepositorio)
    {
        var arquivoCsv = Path.Combine(
            raizRepositorio,
            "dados",
            "ceps_salvador_importacao.csv");
        var tamanhoLote = 1000;

        for (var indice = 0; indice < args.Length; indice++)
        {
            switch (args[indice])
            {
                case "--arquivo" when indice + 1 < args.Length:
                    arquivoCsv = args[++indice];
                    break;
                case "--lote" when indice + 1 < args.Length &&
                    int.TryParse(args[++indice], out var lote):
                    tamanhoLote = lote;
                    break;
            }
        }

        if (!Path.IsPathRooted(arquivoCsv))
        {
            arquivoCsv = Path.GetFullPath(Path.Combine(raizRepositorio, arquivoCsv));
        }

        return new ArgumentosImportacao(arquivoCsv, tamanhoLote);
    }
}

sealed record ResultadoLeituraCsv(
    IReadOnlyList<CepSalvadorImportacaoItem> Itens,
    int TotalLido,
    int Invalidos,
    List<string> Erros,
    string? ErroCabecalho);

sealed record RelatorioImportacao(
    string Arquivo,
    int TamanhoLote,
    int TotalLido,
    int Validos,
    int Inseridos,
    int Atualizados,
    int Ignorados,
    int Invalidos,
    int Duplicados,
    int TotalAntes,
    int TotalFinal,
    int BairrosDistintos,
    double DuracaoSegundos,
    IReadOnlyList<string> Erros,
    IReadOnlyList<BairroContagem> Bairros,
    IReadOnlyList<ConsultaCepResumo> Consultas);

sealed record BairroContagem(
    string BairroNormalizado,
    string Bairro,
    int Total);

sealed record ConsultaCepResumo(
    string Cep,
    string Bairro,
    string? BairroFrete,
    bool Atendido,
    decimal? ValorFrete);
