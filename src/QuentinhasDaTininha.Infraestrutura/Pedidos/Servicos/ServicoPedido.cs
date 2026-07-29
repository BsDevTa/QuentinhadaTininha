using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.Pedidos.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Pedidos.Servicos;

public class ServicoPedido : IServicoPedido
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoDisponibilidadePedido _servicoDisponibilidadePedido;

    public ServicoPedido(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoDisponibilidadePedido servicoDisponibilidadePedido)
    {
        _dbContext = dbContext;
        _servicoDisponibilidadePedido = servicoDisponibilidadePedido;
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

        var precisaTroco = false;
        decimal? valorTroco = null;
        if (requisicao.FormaPagamento == FormaPagamento.Dinheiro)
        {
            precisaTroco = requisicao.PrecisaTroco;
            valorTroco = ValidarTroco(requisicao);
        }

        var enderecoEntrega = NormalizarTextoOpcional(requisicao.EnderecoEntrega);
        var bairro = NormalizarTextoOpcional(requisicao.Bairro);
        var referencia = NormalizarTextoOpcional(requisicao.Referencia);

        if (requisicao.TipoEntrega == TipoEntrega.Entrega)
        {
            ValidarEntrega(enderecoEntrega, bairro, referencia);
        }
        else
        {
            enderecoEntrega = null;
            bairro = null;
            referencia = null;
        }

        var agora = DateTimeOffset.UtcNow;
        var pedido = new Pedido
        {
            DataPedido = requisicao.DataPedido,
            NomeCliente = requisicao.NomeCliente.Trim(),
            TelefoneCliente = NormalizarTextoOpcional(requisicao.TelefoneCliente),
            ValorTotal = requisicao.ValorTotal,
            FormaPagamento = requisicao.FormaPagamento,
            PrecisaTroco = precisaTroco,
            ValorTroco = valorTroco,
            TipoEntrega = requisicao.TipoEntrega,
            EnderecoEntrega = enderecoEntrega,
            Bairro = bairro,
            Referencia = referencia,
            Observacao = NormalizarTextoOpcional(requisicao.Observacao),
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.Pedidos.AddAsync(pedido, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(pedido);
    }

    public async Task<PedidoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.Id == id)
            .Select(pedido => MapearResposta(pedido))
            .FirstOrDefaultAsync(cancellationToken);
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

        if (requisicao.ValorTotal <= 0)
        {
            throw new ArgumentException("Valor do pedido deve ser maior que zero.");
        }

        if (!Enum.IsDefined(requisicao.FormaPagamento))
        {
            throw new ArgumentException("Forma de pagamento inválida.");
        }

        if (!Enum.IsDefined(requisicao.TipoEntrega))
        {
            throw new ArgumentException("Tipo de entrega inválido.");
        }
    }

    private static decimal? ValidarTroco(PedidoCriacaoRequisicao requisicao)
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

        if (requisicao.ValorTroco.Value <= requisicao.ValorTotal)
        {
            throw new ArgumentException(
                "O valor para troco deve ser maior que o valor do pedido.");
        }

        return requisicao.ValorTroco.Value;
    }

    private static void ValidarEntrega(
        string? enderecoEntrega,
        string? bairro,
        string? referencia)
    {
        if (string.IsNullOrWhiteSpace(enderecoEntrega))
        {
            throw new ArgumentException("Endereço de entrega é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(bairro))
        {
            throw new ArgumentException("Bairro é obrigatório para entrega.");
        }

        if (string.IsNullOrWhiteSpace(referencia))
        {
            throw new ArgumentException("Referência é obrigatória para entrega.");
        }
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static PedidoResposta MapearResposta(Pedido pedido)
    {
        return new PedidoResposta
        {
            Id = pedido.Id,
            DataPedido = pedido.DataPedido,
            NomeCliente = pedido.NomeCliente,
            TelefoneCliente = pedido.TelefoneCliente,
            ValorTotal = pedido.ValorTotal,
            FormaPagamento = pedido.FormaPagamento,
            PrecisaTroco = pedido.PrecisaTroco,
            ValorTroco = pedido.ValorTroco,
            TipoEntrega = pedido.TipoEntrega,
            EnderecoEntrega = pedido.EnderecoEntrega,
            Bairro = pedido.Bairro,
            Referencia = pedido.Referencia,
            Observacao = pedido.Observacao,
            CriadoEm = pedido.CriadoEm
        };
    }
}
