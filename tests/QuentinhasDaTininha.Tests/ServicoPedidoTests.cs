using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;
using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.Publico.DTOs;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Dominio.Utilitarios;
using QuentinhasDaTininha.Infraestrutura.FretesBairros.Servicos;
using QuentinhasDaTininha.Infraestrutura.Pedidos.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Tests;

public class ServicoPedidoTests
{
    [Fact]
    public async Task CriarAsync_QuandoClienteManipulaFrete_UsaValorCalculadoPeloBackend()
    {
        await using var dbContext = CriarDbContext();
        var prato = await CriarPratoAsync(dbContext, 20m);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var pedido = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorFrete = 1m,
            ValorTotal = 21m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Entrega,
            Cep = "40221-005",
            Numero = "129",
            Itens =
            [
                new PedidoItemCriacaoRequisicao
                {
                    PratoId = prato.Id,
                    Tamanho = TamanhoRefeicao.P
                }
            ]
        });

        Assert.Equal(20m, pedido.ValorSubtotal);
        Assert.Equal(10m, pedido.ValorFrete);
        Assert.Equal(30m, pedido.ValorTotal);
        Assert.Equal("40221005", pedido.Cep);
    }

    [Fact]
    public async Task CriarAsync_QuandoClienteManipulaFreteComCepSalvador_UsaValorCalculadoPeloBackend()
    {
        await using var dbContext = CriarDbContext();
        var prato = await CriarPratoAsync(dbContext, 20m);
        dbContext.Add(CriarFreteBairro("Imbuí", 12m));
        dbContext.Add(CriarCepSalvador("41720-000", "Imbuí"));
        await dbContext.SaveChangesAsync();
        var servicoFrete = new ServicoFreteBairro(
            dbContext,
            new ServicoCepFake(null));
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            servicoFrete);

        var pedido = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorFrete = 1m,
            ValorTotal = 21m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Entrega,
            Cep = "41720-000",
            Numero = "129",
            Itens =
            [
                new PedidoItemCriacaoRequisicao
                {
                    PratoId = prato.Id,
                    Tamanho = TamanhoRefeicao.P
                }
            ]
        });

        Assert.Equal(20m, pedido.ValorSubtotal);
        Assert.Equal(12m, pedido.ValorFrete);
        Assert.Equal(32m, pedido.ValorTotal);
        Assert.Equal("41720000", pedido.Cep);
        Assert.Equal("Imbuí", pedido.Bairro);
    }

    [Fact]
    public async Task CriarAsync_AdicionaBebidaAtivaAoPedidoEUsaPrecoDoServidor()
    {
        await using var dbContext = CriarDbContext();
        var bebida = await CriarBebidaAsync(dbContext, "Pepsi 1L", 9m, ativa: true);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var pedido = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorSubtotal = 0m,
            ValorTotal = 1m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Retirada,
            Bebidas =
            [
                new PedidoBebidaCriacaoRequisicao
                {
                    BebidaId = bebida.Id,
                    Quantidade = 1,
                    ValorUnitario = 1m
                }
            ]
        });

        Assert.Single(pedido.Bebidas);
        Assert.Equal("Pepsi 1L", pedido.Bebidas[0].NomeBebida);
        Assert.Equal(9m, pedido.Bebidas[0].ValorUnitario);
        Assert.Equal(9m, pedido.ValorSubtotal);
        Assert.Equal(9m, pedido.ValorTotal);
    }

    [Fact]
    public async Task CriarAsync_RejeitaBebidaInativa()
    {
        await using var dbContext = CriarDbContext();
        var bebida = await CriarBebidaAsync(dbContext, "Coca-Cola Lata", 6m, ativa: false);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var excecao = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
            {
                DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                NomeCliente = "Cliente Teste",
                ValorSubtotal = 0m,
                ValorTotal = 1m,
                FormaPagamento = FormaPagamento.Pix,
                TipoEntrega = TipoEntrega.Retirada,
                Bebidas =
                [
                    new PedidoBebidaCriacaoRequisicao
                    {
                        BebidaId = bebida.Id,
                        Quantidade = 1
                    }
                ]
            }));

        Assert.Contains("bebidas ativas", excecao.Message);
    }

    [Fact]
    public async Task CriarAsync_PermitePedidoSemBebida()
    {
        await using var dbContext = CriarDbContext();
        var prato = await CriarPratoAsync(dbContext, 20m);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var pedido = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorSubtotal = 20m,
            ValorTotal = 20m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Retirada,
            Itens =
            [
                new PedidoItemCriacaoRequisicao
                {
                    PratoId = prato.Id,
                    Tamanho = TamanhoRefeicao.P
                }
            ]
        });

        Assert.Empty(pedido.Bebidas);
        Assert.Equal(20m, pedido.ValorSubtotal);
        Assert.Equal(20m, pedido.ValorTotal);
    }

    [Fact]
    public async Task CriarAsync_PermiteMultiplasBebidas()
    {
        await using var dbContext = CriarDbContext();
        var bebida1 = await CriarBebidaAsync(dbContext, "Pepsi Lata", 6m, ativa: true);
        var bebida2 = await CriarBebidaAsync(dbContext, "Coca-Cola Zero Lata", 6m, ativa: true);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var pedido = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorSubtotal = 0m,
            ValorTotal = 1m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Retirada,
            Bebidas =
            [
                new PedidoBebidaCriacaoRequisicao
                {
                    BebidaId = bebida1.Id,
                    Quantidade = 1
                },
                new PedidoBebidaCriacaoRequisicao
                {
                    BebidaId = bebida2.Id,
                    Quantidade = 2
                }
            ]
        });

        Assert.Equal(2, pedido.Bebidas.Count);
        Assert.Equal(18m, pedido.ValorSubtotal);
        Assert.Equal(18m, pedido.ValorTotal);
    }

    [Fact]
    public async Task AtualizarPrecoDaBebidaNaoAlteraPedidoAntigo()
    {
        await using var dbContext = CriarDbContext();
        var bebida = await CriarBebidaAsync(dbContext, "Pepsi 1L", 9m, ativa: true);
        var servicoPedido = new ServicoPedido(
            dbContext,
            new ServicoDisponibilidadePedidoFake(),
            new ServicoFreteBairroFake(10m));

        var pedidoCriado = await servicoPedido.CriarAsync(new PedidoCriacaoRequisicao
        {
            DataPedido = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            NomeCliente = "Cliente Teste",
            ValorSubtotal = 0m,
            ValorTotal = 1m,
            FormaPagamento = FormaPagamento.Pix,
            TipoEntrega = TipoEntrega.Retirada,
            Bebidas =
            [
                new PedidoBebidaCriacaoRequisicao
                {
                    BebidaId = bebida.Id,
                    Quantidade = 1
                }
            ]
        });

        bebida.Preco = 12m;
        await dbContext.SaveChangesAsync();

        var pedidoRecarregado = await servicoPedido.ObterPorIdAsync(pedidoCriado.Id);

        Assert.NotNull(pedidoRecarregado);
        Assert.Equal(9m, pedidoRecarregado!.Bebidas[0].ValorUnitario);
        Assert.Equal(9m, pedidoRecarregado.ValorTotal);
    }

    private static QuentinhasDaTininhaDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<QuentinhasDaTininhaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuentinhasDaTininhaDbContext(options);
    }

    private static async Task<Prato> CriarPratoAsync(
        QuentinhasDaTininhaDbContext dbContext,
        decimal valor)
    {
        var categoria = new Categoria
        {
            Nome = "Quentinhas",
            EstaAtiva = true
        };
        var prato = new Prato
        {
            Categoria = categoria,
            CategoriaId = categoria.Id,
            Nome = "Bife ao molho",
            Preco = valor,
            EstaAtivo = true,
            EstaDisponivel = true,
            Precos =
            [
                new PrecoPrato
                {
                    Tamanho = TamanhoRefeicao.P,
                    FormaPagamento = TipoPrecoPagamento.DinheiroPix,
                    Valor = valor
                }
            ]
        };

        dbContext.Add(prato);
        await dbContext.SaveChangesAsync();
        return prato;
    }

    private static async Task<Bebida> CriarBebidaAsync(
        QuentinhasDaTininhaDbContext dbContext,
        string nome,
        decimal preco,
        bool ativa)
    {
        var bebida = new Bebida
        {
            Nome = nome,
            Preco = preco,
            Ativa = ativa
        };

        dbContext.Add(bebida);
        await dbContext.SaveChangesAsync();
        return bebida;
    }

    private static FreteBairro CriarFreteBairro(
        string bairro,
        decimal valor)
    {
        return new FreteBairro
        {
            Bairro = bairro,
            BairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro),
            Valor = valor,
            Ativo = true
        };
    }

    private static CepSalvador CriarCepSalvador(
        string cep,
        string bairro)
    {
        return new CepSalvador
        {
            Cep = NormalizadorCep.SomenteNumeros(cep),
            Logradouro = "Rua Teste",
            Bairro = bairro,
            BairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro),
            Cidade = "Salvador",
            Uf = "BA",
            Ativo = true
        };
    }

    private sealed class ServicoFreteBairroFake : IServicoFreteBairro
    {
        private readonly decimal _valorFrete;

        public ServicoFreteBairroFake(decimal valorFrete)
        {
            _valorFrete = valorFrete;
        }

        public Task<ConsultaFreteCepResposta> ConsultarPorCepAsync(
            string cep,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ConsultaFreteCepResposta
            {
                Cep = "40221-005",
                Logradouro = "Rua Apolinario de Santana",
                Bairro = "Engenho Velho da Federação",
                Cidade = "Salvador",
                Estado = "BA",
                BairroFrete = "Vila Vale",
                Atendido = true,
                ValorFrete = _valorFrete
            });
        }

        public Task<ConsultaFreteBairroResposta> ConsultarPorBairroAsync(
            string bairro,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<FreteBairroResposta>> ListarAsync(
            string? bairro,
            bool? ativo,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<FreteBairroResposta?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<FreteBairroResposta> CriarAsync(
            FreteBairroSalvarRequisicao requisicao,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<FreteBairroResposta?> AtualizarAsync(
            Guid id,
            FreteBairroSalvarRequisicao requisicao,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<FreteBairroResposta?> AlterarStatusAsync(
            Guid id,
            bool ativo,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ExcluirAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class ServicoCepFake : IServicoCep
    {
        private readonly EnderecoCepResposta? _endereco;

        public ServicoCepFake(EnderecoCepResposta? endereco)
        {
            _endereco = endereco;
        }

        public Task<EnderecoCepResposta?> ConsultarAsync(
            string cep,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_endereco);
        }
    }

    private sealed class ServicoDisponibilidadePedidoFake : IServicoDisponibilidadePedido
    {
        public Task<ValidacaoDisponibilidadePedidoResposta> ValidarPedidoAsync(
            DateOnly data,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ValidacaoDisponibilidadePedidoResposta
            {
                Data = data,
                PermitirPedidos = true
            });
        }

        public Task<IReadOnlyList<DisponibilidadeDataResposta>> ListarAsync(
            DateOnly? dataInicial,
            DateOnly? dataFinal,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<DisponibilidadeDataResposta> ObterPorDataAsync(
            DateOnly data,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<DisponibilidadeDataResposta> LiberarDataAsync(
            DateOnly data,
            string? motivo,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<DisponibilidadeDataResposta> BloquearDataAsync(
            DateOnly data,
            string? motivo,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<DisponibilidadeDataResposta> AlterarMotivoAsync(
            DateOnly data,
            string? motivo,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<DisponibilidadePublicaResposta> ListarPublicaAsync(
            DateOnly? dataInicial,
            DateOnly? dataFinal,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
