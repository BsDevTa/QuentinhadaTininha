using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Inicializacao;

public class InicializadorCardapioPublico
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public InicializadorCardapioPublico(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var categoria = await ObterOuCriarCategoriaAsync(agora, cancellationToken);
        await ObterOuAtualizarRestauranteAsync(agora, cancellationToken);

        var acompanhamentos = await ObterOuCriarAcompanhamentosAsync(agora, cancellationToken);
        var grupos = await ObterOuCriarGruposAsync(cancellationToken);
        await ConfigurarItensGruposAsync(grupos, acompanhamentos, cancellationToken);
        var pratos = await ObterOuCriarPratosAsync(categoria.Id, grupos, agora, cancellationToken);
        await ConfigurarCardapiosAsync(pratos, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Categoria> ObterOuCriarCategoriaAsync(
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        var categoria = await _dbContext.Categorias
            .FirstOrDefaultAsync(categoria => categoria.Nome == "Quentinhas", cancellationToken);

        if (categoria is not null)
        {
            categoria.EstaAtiva = true;
            return categoria;
        }

        categoria = new Categoria
        {
            Nome = "Quentinhas",
            Descricao = "Pratos principais do cardápio público",
            EstaAtiva = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Categorias.AddAsync(categoria, cancellationToken);
        return categoria;
    }

    private async Task ObterOuAtualizarRestauranteAsync(
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuracao is null)
        {
            configuracao = new ConfiguracaoRestaurante
            {
                CriadoEm = agora
            };
            await _dbContext.ConfiguracoesRestaurante.AddAsync(configuracao, cancellationToken);
        }

        configuracao.Nome = "Quentinhas da Tininha";
        configuracao.Descricao = "Comida 100% caseira com sabor de família.";
        configuracao.Telefone = "5571982189319";
        configuracao.Whatsapp = "5571982189319";
        configuracao.Instagram = "@quentinhasdatininha";
        configuracao.Endereco = "Rua Apolinario de Santana, 129 - Engenho Velho da Federacao";
        configuracao.Cidade = "Salvador";
        configuracao.Estado = "BA";
        configuracao.HorarioFuncionamento ??= "Segunda a sabado, das 10h as 14h";
        configuracao.UrlLogotipo = "/assets/logo-tininha.svg";
        configuracao.MensagemAberto = "Estamos atendendo hoje.";
        configuracao.MensagemFechado = "Hoje não temos atendimento. Consulte o cardápio dos outros dias.";
        configuracao.ModoFuncionamento = ModoFuncionamento.Automatico;
        configuracao.AceitaPedidos = true;
        configuracao.EstaAtivo = true;
        configuracao.AtualizadoEm = agora;
    }

    private async Task<Dictionary<string, Acompanhamento>> ObterOuCriarAcompanhamentosAsync(
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        var dados = new[]
        {
            ("feijao-caldo", "Feijão de caldo", TipoSelecaoAcompanhamento.Exclusiva, "TIPO_FEIJAO", 1),
            ("feijao-tropeiro", "Feijão tropeiro", TipoSelecaoAcompanhamento.Exclusiva, "TIPO_FEIJAO", 2),
            ("arroz", "Arroz", TipoSelecaoAcompanhamento.Multipla, null, 3),
            ("macarrao", "Macarrão", TipoSelecaoAcompanhamento.Multipla, null, 4),
            ("salada", "Salada", TipoSelecaoAcompanhamento.Multipla, null, 5),
            ("feijao-fradinho", "Feijão fradinho", TipoSelecaoAcompanhamento.Multipla, null, 6),
            ("caruru", "Caruru", TipoSelecaoAcompanhamento.Multipla, null, 7),
            ("vatapa", "Vatapá", TipoSelecaoAcompanhamento.Multipla, null, 8),
            ("farofa", "Farofa", TipoSelecaoAcompanhamento.Multipla, null, 9),
            ("pirao", "Pirão", TipoSelecaoAcompanhamento.Multipla, null, 10),
            ("salada-vinagrete", "Salada vinagrete", TipoSelecaoAcompanhamento.Multipla, null, 11)
        };

        var existentes = await _dbContext.Acompanhamentos
            .ToDictionaryAsync(a => a.Nome, cancellationToken);
        var resultado = new Dictionary<string, Acompanhamento>();

        foreach (var (codigo, nome, tipoSelecao, grupoExclusivo, ordem) in dados)
        {
            if (!existentes.TryGetValue(nome, out var acompanhamento))
            {
                acompanhamento = new Acompanhamento
                {
                    Nome = nome,
                    CriadoEm = agora
                };
                await _dbContext.Acompanhamentos.AddAsync(acompanhamento, cancellationToken);
            }

            acompanhamento.PrecoAdicional = 0;
            acompanhamento.EstaAtivo = true;
            acompanhamento.EstaDisponivel = true;
            acompanhamento.TipoSelecao = tipoSelecao;
            acompanhamento.GrupoExclusivo = grupoExclusivo;
            acompanhamento.OrdemExibicao = ordem;
            acompanhamento.AtualizadoEm = agora;
            resultado[codigo] = acompanhamento;
        }

        return resultado;
    }

    private async Task<Dictionary<string, GrupoAcompanhamento>> ObterOuCriarGruposAsync(
        CancellationToken cancellationToken)
    {
        var dados = new[]
        {
            ("PADRAO", "Acompanhamentos padrão"),
            ("COMIDA_BAIANA", "Comida baiana"),
            ("COZIDO", "Cozido"),
            ("SARAPATEL_XINXIM", "Sarapatel e xinxim"),
            ("ARRUMADINHO", "Arrumadinho")
        };

        var existentes = await _dbContext.GruposAcompanhamento
            .ToDictionaryAsync(grupo => grupo.Codigo, cancellationToken);
        var resultado = new Dictionary<string, GrupoAcompanhamento>();

        foreach (var (codigo, nome) in dados)
        {
            if (!existentes.TryGetValue(codigo, out var grupo))
            {
                grupo = new GrupoAcompanhamento { Codigo = codigo };
                await _dbContext.GruposAcompanhamento.AddAsync(grupo, cancellationToken);
            }

            grupo.Nome = nome;
            grupo.EstaAtivo = true;
            resultado[codigo] = grupo;
        }

        return resultado;
    }

    private async Task ConfigurarItensGruposAsync(
        IReadOnlyDictionary<string, GrupoAcompanhamento> grupos,
        IReadOnlyDictionary<string, Acompanhamento> acompanhamentos,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);

        var configuracoes = new Dictionary<string, string[]>
        {
            ["PADRAO"] = new[] { "feijao-caldo", "feijao-tropeiro", "arroz", "macarrao", "salada" },
            ["COMIDA_BAIANA"] = new[] { "arroz", "feijao-fradinho", "caruru", "vatapa", "farofa" },
            ["COZIDO"] = new[] { "arroz", "pirao" },
            ["SARAPATEL_XINXIM"] = new[] { "arroz", "feijao-caldo", "feijao-tropeiro" },
            ["ARRUMADINHO"] = new[] { "arroz", "farofa", "feijao-fradinho", "salada-vinagrete" }
        };

        var existentes = await _dbContext.GruposAcompanhamentoItens
            .ToListAsync(cancellationToken);

        foreach (var (codigoGrupo, codigosAcompanhamentos) in configuracoes)
        {
            var grupo = grupos[codigoGrupo];
            for (var indice = 0; indice < codigosAcompanhamentos.Length; indice++)
            {
                var acompanhamento = acompanhamentos[codigosAcompanhamentos[indice]];
                var item = existentes.FirstOrDefault(item =>
                    item.GrupoAcompanhamentoId == grupo.Id &&
                    item.AcompanhamentoId == acompanhamento.Id);

                if (item is null)
                {
                    item = new GrupoAcompanhamentoItem
                    {
                        GrupoAcompanhamentoId = grupo.Id,
                        AcompanhamentoId = acompanhamento.Id
                    };
                    await _dbContext.GruposAcompanhamentoItens.AddAsync(item, cancellationToken);
                }

                item.Obrigatorio = false;
                item.OrdemExibicao = indice + 1;
            }
        }
    }

    private async Task<Dictionary<string, Prato>> ObterOuCriarPratosAsync(
        Guid categoriaId,
        IReadOnlyDictionary<string, GrupoAcompanhamento> grupos,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        var existentes = await _dbContext.Pratos
            .Include(prato => prato.Precos)
            .ToDictionaryAsync(prato => prato.Nome, cancellationToken);
        var resultado = new Dictionary<string, Prato>();

        foreach (var item in ObterDadosPratos())
        {
            if (!existentes.TryGetValue(item.Nome, out var prato))
            {
                prato = new Prato
                {
                    Nome = item.Nome,
                    CriadoEm = agora
                };
                await _dbContext.Pratos.AddAsync(prato, cancellationToken);
            }

            prato.CategoriaId = categoriaId;
            prato.GrupoAcompanhamentoId = grupos[item.Grupo].Id;
            prato.Descricao = item.Descricao;
            prato.Preco = item.PequenaDinheiroPix;
            prato.UrlImagem = ObterUrlImagemPrato(item.Nome);
            prato.EstaAtivo = true;
            prato.EstaDisponivel = item.EstaDisponivel;
            prato.EhDestaque = false;
            prato.AtualizadoEm = agora;

            ConfigurarPreco(prato, TamanhoRefeicao.P, TipoPrecoPagamento.DinheiroPix, item.PequenaDinheiroPix);
            ConfigurarPreco(prato, TamanhoRefeicao.P, TipoPrecoPagamento.Cartao, item.PequenaCartao);
            ConfigurarPreco(prato, TamanhoRefeicao.G, TipoPrecoPagamento.DinheiroPix, item.GrandeDinheiroPix);
            ConfigurarPreco(prato, TamanhoRefeicao.G, TipoPrecoPagamento.Cartao, item.GrandeCartao);

            resultado[item.Nome] = prato;
        }

        return resultado;
    }

    private static void ConfigurarPreco(
        Prato prato,
        TamanhoRefeicao tamanho,
        TipoPrecoPagamento formaPagamento,
        decimal valor)
    {
        var preco = prato.Precos.FirstOrDefault(preco =>
            preco.Tamanho == tamanho &&
            preco.FormaPagamento == formaPagamento);

        if (preco is null)
        {
            preco = new PrecoPrato
            {
                Tamanho = tamanho,
                FormaPagamento = formaPagamento
            };
            prato.Precos.Add(preco);
        }

        preco.Valor = valor;
    }

    private static string? ObterUrlImagemPrato(string nome)
    {
        return nome switch
        {
            "Bife ao molho" => "/assets/pratos/bife-ao-molho.png",
            "Bisteca" => "/assets/pratos/bisteca-real.png",
            "Frango à milanesa" => "/assets/pratos/frango-a-milanesa.png",
            "Estrogonofe de frango" => "/assets/pratos/estrogonofe-de-frango.png",
            "Quiabada" => "/assets/pratos/quiabada.png",
            "Frango grelhado" => "/assets/pratos/frango-grelhado.png",
            _ => null
        };
    }

    private async Task ConfigurarCardapiosAsync(
        IReadOnlyDictionary<string, Prato> pratos,
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dadosPorDia = ObterCardapioPorDia();
        var diasCardapioPublico = dadosPorDia.Keys.ToArray();
        var cardapioIds = await _dbContext.CardapiosDia
            .Where(cardapio => diasCardapioPublico.Contains(cardapio.DiaSemana))
            .Select(cardapio => cardapio.Id)
            .ToListAsync(cancellationToken);

        await _dbContext.CardapiosDiaPratos
            .Where(cardapioPrato => cardapioIds.Contains(cardapioPrato.CardapioDiaId))
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.ChangeTracker.Clear();

        var cardapios = await _dbContext.CardapiosDia
            .Where(cardapio => diasCardapioPublico.Contains(cardapio.DiaSemana))
            .ToListAsync(cancellationToken);

        foreach (var (dia, nomesPratos) in dadosPorDia)
        {
            var cardapio = cardapios.FirstOrDefault(cardapio => cardapio.DiaSemana == dia);
            if (cardapio is null)
            {
                cardapio = new CardapioDia
                {
                    DiaSemana = dia
                };
                await _dbContext.CardapiosDia.AddAsync(cardapio, cancellationToken);
                cardapios.Add(cardapio);
            }

            cardapio.EstaAtivo = true;

            for (var indice = 0; indice < nomesPratos.Length; indice++)
            {
                var prato = pratos[nomesPratos[indice]];
                await _dbContext.CardapiosDiaPratos.AddAsync(new CardapioDiaPrato
                {
                    CardapioDiaId = cardapio.Id,
                    PratoId = prato.Id,
                    OrdemExibicao = indice + 1,
                    EstaDisponivel = prato.EstaDisponivel
                }, cancellationToken);
            }
        }
    }

    private static IEnumerable<(string Nome, string Descricao, string Grupo,
        decimal PequenaDinheiroPix, decimal PequenaCartao, decimal GrandeDinheiroPix,
        decimal GrandeCartao, bool EstaDisponivel)> ObterDadosPratos()
    {
        (decimal, decimal, decimal, decimal) padrao = (17, 18, 21, 22);
        (decimal, decimal, decimal, decimal) intermediario = (19, 20, 23, 24);

        yield return Prato("Omelete de frango", "Omelete recheado com frango desfiado e tempero caseiro.", "PADRAO", padrao);
        yield return Prato("Frango grelhado", "Filé de frango grelhado, leve e bem temperado.", "PADRAO", padrao);
        yield return Prato("Bisteca", "Bisteca suína grelhada, suculenta e bem temperada.", "PADRAO", padrao);
        yield return Prato("Frango à milanesa", "Filé de frango empanado, crocante por fora e macio por dentro.", "PADRAO", padrao);
        yield return Prato("Ensopado de boi", "Carne bovina cozida lentamente com molho caseiro encorpado.", "PADRAO", padrao);
        yield return Prato("Ensopado de frango", "Frango cozido em molho caseiro com tempero marcante.", "PADRAO", padrao);
        yield return Prato("Fígado ao molho com purê de batata", "Fígado macio ao molho, servido com purê cremoso.", "PADRAO", padrao);
        yield return Prato("Quiabada", "Quiabada tradicional com tempero baiano e sabor marcante.", "PADRAO", padrao);
        yield return Prato("Isca de fígado acebolado", "Iscas de fígado aceboladas e bem temperadas.", "PADRAO", padrao);
        yield return Prato("Arrumadinho misto", "Arrumadinho caprichado com mistura saborosa e tempero caseiro.", "ARRUMADINHO", padrao);
        yield return Prato("Coxinha da asa + Toscana", "Coxinha da asa assada com linguiça toscana saborosa.", "PADRAO", intermediario);
        yield return Prato("Carne de panela", "Carne cozida lentamente, macia e cheia de sabor.", "PADRAO", intermediario);
        yield return Prato("Estrogonofe de frango", "Frango cremoso com molho suave e gostinho caseiro.", "PADRAO", intermediario);
        yield return Prato("Bife ao molho", "Bife macio com molho caseiro bem temperado.", "PADRAO", intermediario);
        yield return Prato("Bife acebolado", "Bife grelhado com cebola dourada e tempero caseiro.", "PADRAO", intermediario);
        yield return Prato("Isca de carne ao molho", "Iscas de carne macias em molho caseiro.", "PADRAO", intermediario);
        yield return Prato("Xinxim de bofe", "Xinxim de bofe tradicional, temperado e marcante.", "SARAPATEL_XINXIM", intermediario);
        yield return Prato("Sarapatel", "Sarapatel tradicional com tempero forte e caseiro.", "SARAPATEL_XINXIM", intermediario, false);
        yield return Prato("Frango à parmegiana", "Filé de frango empanado com molho e queijo derretido.", "PADRAO", (20, 21, 24, 25));
        yield return Prato("Peixe frito", "Peixe sequinho, crocante e temperado no ponto.", "PADRAO", (25, 26, 45, 46));
        yield return Prato("Cozido", "Cozido completo, farto e preparado com caldo encorpado.", "COZIDO", (20, 21, 36, 37));
        yield return Prato("Comida baiana com xinxim de frango", "Xinxim de frango cremoso com sabor baiano.", "COMIDA_BAIANA", (20, 21, 37, 38));
        yield return Prato("Comida baiana com moqueca de peixe", "Moqueca de peixe com molho aromático e tempero baiano.", "COMIDA_BAIANA", (25, 26, 45, 46));
        yield return Prato("Comida baiana com peixe frito", "Prato baiano com peixe frito e temperos tradicionais.", "COMIDA_BAIANA", (25, 26, 45, 46));
    }

    private static (string Nome, string Descricao, string Grupo,
        decimal PequenaDinheiroPix, decimal PequenaCartao, decimal GrandeDinheiroPix,
        decimal GrandeCartao, bool EstaDisponivel) Prato(
            string nome,
            string descricao,
            string grupo,
            (decimal PequenaDinheiroPix, decimal PequenaCartao, decimal GrandeDinheiroPix,
                decimal GrandeCartao) precos,
            bool estaDisponivel = true)
    {
        return (nome, descricao, grupo, precos.PequenaDinheiroPix, precos.PequenaCartao,
            precos.GrandeDinheiroPix, precos.GrandeCartao, estaDisponivel);
    }

    private static Dictionary<DiaSemana, string[]> ObterCardapioPorDia()
    {
        return new Dictionary<DiaSemana, string[]>
        {
            [DiaSemana.SegundaFeira] = new[] { "Omelete de frango", "Bisteca", "Frango à milanesa", "Ensopado de boi", "Frango grelhado", "Coxinha da asa + Toscana" },
            [DiaSemana.TercaFeira] = new[] { "Ensopado de frango", "Peixe frito", "Bisteca", "Frango à milanesa", "Fígado ao molho com purê de batata", "Frango à parmegiana", "Carne de panela" },
            [DiaSemana.QuartaFeira] = new[] { "Bife ao molho", "Bisteca", "Frango à milanesa", "Estrogonofe de frango", "Quiabada", "Frango grelhado" },
            [DiaSemana.QuintaFeira] = new[] { "Cozido", "Bife acebolado", "Bisteca", "Frango à milanesa", "Isca de carne ao molho", "Frango à parmegiana" },
            [DiaSemana.SextaFeira] = new[] { "Comida baiana com peixe frito", "Comida baiana com xinxim de frango", "Bisteca", "Frango à milanesa", "Comida baiana com moqueca de peixe", "Isca de fígado acebolado", "Frango grelhado" },
            [DiaSemana.Sabado] = new[] { "Arrumadinho misto", "Bisteca", "Frango à milanesa", "Xinxim de bofe", "Sarapatel", "Frango grelhado" }
        };
    }
}
