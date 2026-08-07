using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.Pedidos.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Dominio.Utilitarios;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Pedidos.Servicos;

public class ServicoPedido : IServicoPedido
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;
    private readonly IServicoFreteBairro _servicoFreteBairro;

    public ServicoPedido(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoDisponibilidadePedido servicoDisponibilidadePedido,
        IServicoFreteBairro servicoFreteBairro)
    {
        _dbContext = dbContext;
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
        _servicoFreteBairro = servicoFreteBairro;
    }

    public async Task<PedidoResposta> CriarAsync(
        PedidoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        ValidarDadosBasicos(requisicao);

        var disponibilidade = await _servicoDisponibilidadePedido.ValidarPedidoAsync(
            requisicao.DataPedido,
            cancellationToken);

        if (!disponibilidade.PermitirPedidos)
        {
            throw new InvalidOperationException(
                disponibilidade.MotivoBloqueio ??
                "Não é possível criar pedido para essa data.");
        }

        var itens = await MontarItensPedidoAsync(
            requisicao.Itens ?? new List<PedidoItemCriacaoRequisicao>(),
            requisicao.FormaPagamento,
            cancellationToken);
        var dadosEntrega = await PrepararEntregaAsync(requisicao, cancellationToken);
        var subtotal = CalcularSubtotal(requisicao, itens, dadosEntrega?.ValorFrete);
        var valorTotal = subtotal + (dadosEntrega?.ValorFrete ?? 0);

        if (itens.Count > 0)
        {
            ValidarSubtotalInformado(requisicao, subtotal);
        }

        var precisaTroco = false;
        decimal? valorTroco = null;
        if (requisicao.FormaPagamento == FormaPagamento.Dinheiro)
        {
            precisaTroco = requisicao.PrecisaTroco;
            valorTroco = ValidarTroco(requisicao, valorTotal);
        }

        var agora = DateTimeOffset.UtcNow;
        var pedido = new Pedido
        {
            DataPedido = requisicao.DataPedido,
            NomeCliente = requisicao.NomeCliente.Trim(),
            TelefoneCliente = NormalizarTextoOpcional(requisicao.TelefoneCliente),
            ValorSubtotal = subtotal,
            ValorFrete = dadosEntrega?.ValorFrete,
            ValorTotal = valorTotal,
            FormaPagamento = requisicao.FormaPagamento,
            PrecisaTroco = precisaTroco,
            ValorTroco = valorTroco,
            TipoEntrega = requisicao.TipoEntrega,
            Cep = dadosEntrega?.Cep,
            Logradouro = dadosEntrega?.Logradouro,
            Numero = dadosEntrega?.Numero,
            Complemento = dadosEntrega?.Complemento,
            EnderecoEntrega = dadosEntrega?.EnderecoEntrega,
            Bairro = dadosEntrega?.Bairro,
            Cidade = dadosEntrega?.Cidade,
            Estado = dadosEntrega?.Estado,
            Referencia = dadosEntrega?.Referencia,
            Observacao = NormalizarTextoOpcional(requisicao.Observacao),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        foreach (var item in itens)
        {
            pedido.Itens.Add(item);
        }

        await _dbContext.Pedidos.AddAsync(pedido, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(pedido);
    }

    public async Task<PedidoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pedido = await _dbContext.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .Where(pedido => pedido.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return pedido is null ? null : MapearResposta(pedido);
    }

    private static void ValidarDadosBasicos(PedidoCriacaoRequisicao requisicao)
    {
        if (requisicao.DataPedido == default)
        {
            throw new ArgumentException("Data do pedido é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(requisicao.NomeCliente))
        {
            throw new ArgumentException("Nome do cliente é obrigatório.");
        }

        if (requisicao.NomeCliente.Trim().Length > 120)
        {
            throw new ArgumentException("Nome do cliente deve ter no máximo 120 caracteres.");
        }

        if (!Enum.IsDefined(requisicao.FormaPagamento))
        {
            throw new ArgumentException("Forma de pagamento inválida.");
        }

        if (!Enum.IsDefined(requisicao.TipoEntrega))
        {
            throw new ArgumentException("Tipo de entrega inválido.");
        }

        if (!string.IsNullOrWhiteSpace(requisicao.Observacao) &&
            requisicao.Observacao.Trim().Length > 500)
        {
            throw new ArgumentException("Observação do pedido deve ter no máximo 500 caracteres.");
        }

        if ((requisicao.Itens is null || requisicao.Itens.Count == 0) &&
            requisicao.ValorTotal <= 0)
        {
            throw new ArgumentException("Valor do pedido deve ser maior que zero.");
        }
    }

    private static decimal? ValidarTroco(
        PedidoCriacaoRequisicao requisicao,
        decimal valorTotal)
    {
        if (!requisicao.PrecisaTroco)
        {
            return null;
        }

        if (!requisicao.ValorTroco.HasValue)
        {
            throw new ArgumentException(
                "Informe o valor para troco quando o pagamento for em dinheiro.");
        }

        if (requisicao.ValorTroco.Value <= valorTotal)
        {
            throw new ArgumentException(
                "O valor para troco deve ser maior que o valor do pedido.");
        }

        return requisicao.ValorTroco.Value;
    }

    private async Task<List<PedidoItem>> MontarItensPedidoAsync(
        IReadOnlyCollection<PedidoItemCriacaoRequisicao> itens,
        FormaPagamento formaPagamento,
        CancellationToken cancellationToken)
    {
        var itensPedido = new List<PedidoItem>();
        var tipoPreco = formaPagamento == FormaPagamento.Cartao
            ? TipoPrecoPagamento.Cartao
            : TipoPrecoPagamento.DinheiroPix;

        foreach (var item in itens)
        {
            ValidarItem(item);

            var prato = await _dbContext.Pratos
                .AsNoTracking()
                .Include(prato => prato.Precos)
                .FirstOrDefaultAsync(
                    prato =>
                        prato.Id == item.PratoId &&
                        prato.EstaAtivo &&
                        prato.EstaDisponivel,
                    cancellationToken);

            if (prato is null)
            {
                throw new ArgumentException("Informe apenas pratos ativos e disponíveis.");
            }

            var valor = prato.Precos
                .Where(preco =>
                    preco.Tamanho == item.Tamanho &&
                    preco.FormaPagamento == tipoPreco)
                .Select(preco => preco.Valor)
                .FirstOrDefault();

            if (valor <= 0)
            {
                throw new InvalidOperationException(
                    "Não foi possível calcular o valor de um item do pedido.");
            }

            var acompanhamentos = await ObterTextoAcompanhamentosAsync(
                item.AcompanhamentoIds ?? new List<Guid>(),
                cancellationToken);

            itensPedido.Add(new PedidoItem
            {
                PratoId = prato.Id,
                NomePrato = prato.Nome,
                Tamanho = item.Tamanho,
                Acompanhamentos = acompanhamentos,
                ValorUnitario = valor,
                Observacao = NormalizarObservacaoItem(item.Observacao)
            });
        }

        return itensPedido;
    }

    private static void ValidarItem(PedidoItemCriacaoRequisicao item)
    {
        if (item.PratoId == Guid.Empty)
        {
            throw new ArgumentException("Prato do item é obrigatório.");
        }

        if (!Enum.IsDefined(item.Tamanho))
        {
            throw new ArgumentException("Tamanho do item inválido.");
        }

        if ((item.AcompanhamentoIds ?? new List<Guid>()).Distinct().Count() !=
            (item.AcompanhamentoIds ?? new List<Guid>()).Count)
        {
            throw new ArgumentException("Não repita acompanhamentos no mesmo item.");
        }

        if (!string.IsNullOrWhiteSpace(item.Observacao) &&
            item.Observacao.Trim().Length > 250)
        {
            throw new ArgumentException("Observação do item deve ter no máximo 250 caracteres.");
        }
    }

    private async Task<string?> ObterTextoAcompanhamentosAsync(
        IReadOnlyCollection<Guid> acompanhamentoIds,
        CancellationToken cancellationToken)
    {
        var ids = acompanhamentoIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return null;
        }

        if (ids.Count != acompanhamentoIds.Count)
        {
            throw new ArgumentException("Acompanhamento inválido no item do pedido.");
        }

        var acompanhamentos = await _dbContext.Acompanhamentos
            .AsNoTracking()
            .Where(acompanhamento =>
                ids.Contains(acompanhamento.Id) &&
                acompanhamento.EstaAtivo &&
                acompanhamento.EstaDisponivel)
            .Select(acompanhamento => new
            {
                acompanhamento.Id,
                acompanhamento.Nome
            })
            .ToListAsync(cancellationToken);

        if (acompanhamentos.Count != ids.Count)
        {
            throw new ArgumentException("Informe apenas acompanhamentos ativos e disponíveis.");
        }

        var nomesPorId = acompanhamentos.ToDictionary(
            acompanhamento => acompanhamento.Id,
            acompanhamento => acompanhamento.Nome);

        return string.Join(", ", ids.Select(id => nomesPorId[id]));
    }

    private async Task<DadosEntrega?> PrepararEntregaAsync(
        PedidoCriacaoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        if (requisicao.TipoEntrega != TipoEntrega.Entrega)
        {
            return null;
        }

        var cep = NormalizadorCep.SomenteNumeros(requisicao.Cep);
        if (cep.Length != 8)
        {
            throw new ArgumentException("Informe um CEP com 8 números.");
        }

        var consultaFrete = await _servicoFreteBairro.ConsultarPorCepAsync(
            cep,
            cancellationToken);

        if (!consultaFrete.Atendido || !consultaFrete.ValorFrete.HasValue)
        {
            throw new InvalidOperationException(
                consultaFrete.Mensagem ??
                $"No momento, ainda não realizamos entregas para o bairro {consultaFrete.Bairro}. Você pode selecionar a opção de retirada no local.");
        }

        var numero = NormalizarTextoOpcional(requisicao.Numero);
        if (string.IsNullOrWhiteSpace(numero))
        {
            throw new ArgumentException("Número é obrigatório para entrega.");
        }

        var logradouro =
            NormalizarTextoOpcional(consultaFrete.Logradouro) ??
            NormalizarTextoOpcional(requisicao.Logradouro) ??
            NormalizarTextoOpcional(requisicao.EnderecoEntrega);

        if (string.IsNullOrWhiteSpace(logradouro))
        {
            throw new ArgumentException("Logradouro é obrigatório para entrega.");
        }

        var complemento = NormalizarTextoOpcional(requisicao.Complemento);
        var enderecoEntrega = MontarEnderecoEntrega(logradouro, numero, complemento);

        return new DadosEntrega(
            cep,
            logradouro,
            numero,
            complemento,
            enderecoEntrega,
            consultaFrete.Bairro,
            consultaFrete.Cidade,
            consultaFrete.Estado,
            NormalizarTextoOpcional(requisicao.Referencia),
            consultaFrete.ValorFrete.Value);
    }

    private static decimal CalcularSubtotal(
        PedidoCriacaoRequisicao requisicao,
        IReadOnlyCollection<PedidoItem> itens,
        decimal? valorFrete)
    {
        if (itens.Count > 0)
        {
            return itens.Sum(item => item.ValorUnitario);
        }

        if (requisicao.ValorSubtotal > 0)
        {
            return requisicao.ValorSubtotal;
        }

        var subtotal = requisicao.ValorTotal - (valorFrete ?? 0);
        if (subtotal <= 0)
        {
            throw new ArgumentException("Subtotal do pedido deve ser maior que zero.");
        }

        return subtotal;
    }

    private static void ValidarSubtotalInformado(
        PedidoCriacaoRequisicao requisicao,
        decimal subtotalCalculado)
    {
        if (requisicao.ValorSubtotal > 0 &&
            requisicao.ValorSubtotal != subtotalCalculado)
        {
            throw new ArgumentException("Subtotal do pedido divergente.");
        }
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static string? NormalizarObservacaoItem(string? texto)
    {
        var observacao = NormalizarTextoOpcional(texto);
        if (observacao?.Length > 250)
        {
            throw new ArgumentException("Observação do item deve ter no máximo 250 caracteres.");
        }

        return observacao;
    }

    private static string MontarEnderecoEntrega(
        string logradouro,
        string numero,
        string? complemento)
    {
        return string.IsNullOrWhiteSpace(complemento)
            ? $"{logradouro}, {numero}"
            : $"{logradouro}, {numero} - {complemento}";
    }

    private static PedidoResposta MapearResposta(Pedido pedido)
    {
        return new PedidoResposta
        {
            Id = pedido.Id,
            DataPedido = pedido.DataPedido,
            NomeCliente = pedido.NomeCliente,
            TelefoneCliente = pedido.TelefoneCliente,
            ValorSubtotal = pedido.ValorSubtotal,
            ValorFrete = pedido.ValorFrete,
            ValorTotal = pedido.ValorTotal,
            FormaPagamento = pedido.FormaPagamento,
            PrecisaTroco = pedido.PrecisaTroco,
            ValorTroco = pedido.ValorTroco,
            TipoEntrega = pedido.TipoEntrega,
            Cep = pedido.Cep,
            Logradouro = pedido.Logradouro,
            Numero = pedido.Numero,
            Complemento = pedido.Complemento,
            EnderecoEntrega = pedido.EnderecoEntrega,
            Bairro = pedido.Bairro,
            Cidade = pedido.Cidade,
            Estado = pedido.Estado,
            Referencia = pedido.Referencia,
            Observacao = pedido.Observacao,
            Itens = pedido.Itens
                .OrderBy(item => item.CriadoEm)
                .Select(item => new PedidoItemResposta
                {
                    Id = item.Id,
                    PratoId = item.PratoId,
                    NomePrato = item.NomePrato,
                    Tamanho = item.Tamanho,
                    Acompanhamentos = item.Acompanhamentos,
                    ValorUnitario = item.ValorUnitario,
                    Observacao = item.Observacao
                })
                .ToList(),
            CriadoEm = pedido.CriadoEm
        };
    }

    private sealed record DadosEntrega(
        string Cep,
        string Logradouro,
        string Numero,
        string? Complemento,
        string EnderecoEntrega,
        string Bairro,
        string Cidade,
        string Estado,
        string? Referencia,
        decimal ValorFrete);
}
