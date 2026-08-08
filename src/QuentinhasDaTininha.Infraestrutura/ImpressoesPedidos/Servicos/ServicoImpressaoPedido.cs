using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.DTOs;
using QuentinhasDaTininha.Aplicacao.ImpressoesPedidos.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pedidos.DTOs;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.ImpressoesPedidos.Servicos;

public class ServicoImpressaoPedido : IServicoImpressaoPedido
{
    private static readonly TimeSpan LeaseProcessamento = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IntervaloRetentativaErro = TimeSpan.FromSeconds(15);
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoImpressaoPedido(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ImpressaoPedidoResposta>> ListarPendentesAsync(
        int limite = 10,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var limiteLease = agora.Subtract(LeaseProcessamento);
        var limiteErro = agora.Subtract(IntervaloRetentativaErro);
        var quantidade = Math.Clamp(limite, 1, 25);

        var impressoes = await _dbContext.ImpressoesPedidos
            .AsNoTracking()
            .Include(impressao => impressao.Pedido)
                .ThenInclude(pedido => pedido.Itens)
            .Where(impressao =>
                impressao.Status == StatusImpressaoPedido.Pendente ||
                (impressao.Status == StatusImpressaoPedido.Erro &&
                    impressao.AtualizadoEm <= limiteErro) ||
                (impressao.Status == StatusImpressaoPedido.Processando &&
                    impressao.AtualizadoEm <= limiteLease))
            .OrderBy(impressao => impressao.Pedido.CriadoEm)
            .ThenBy(impressao => impressao.CriadoEm)
            .Take(quantidade)
            .ToListAsync(cancellationToken);

        return impressoes.Select(MapearResposta).ToList();
    }

    public async Task<ImpressaoPedidoResposta?> IniciarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var limiteLease = agora.Subtract(LeaseProcessamento);

        var atualizados = await _dbContext.ImpressoesPedidos
            .Where(impressao =>
                impressao.Id == id &&
                (
                    impressao.Status == StatusImpressaoPedido.Pendente ||
                    impressao.Status == StatusImpressaoPedido.Erro ||
                    (
                        impressao.Status == StatusImpressaoPedido.Processando &&
                        impressao.AtualizadoEm <= limiteLease
                    )
                ))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(impressao => impressao.Status, StatusImpressaoPedido.Processando)
                .SetProperty(impressao => impressao.Tentativas, impressao => impressao.Tentativas + 1)
                .SetProperty(impressao => impressao.AtualizadoEm, agora)
                .SetProperty(impressao => impressao.UltimoErro, (string?)null),
                cancellationToken);

        if (atualizados == 0)
        {
            return null;
        }

        return await ObterPorIdAsync(id, cancellationToken);
    }

    public async Task<bool> ConcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;

        var atualizados = await _dbContext.ImpressoesPedidos
            .Where(impressao =>
                impressao.Id == id &&
                impressao.Status == StatusImpressaoPedido.Processando)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(impressao => impressao.Status, StatusImpressaoPedido.Impresso)
                .SetProperty(impressao => impressao.AtualizadoEm, agora)
                .SetProperty(impressao => impressao.ImpressoEm, agora)
                .SetProperty(impressao => impressao.UltimoErro, (string?)null),
                cancellationToken);

        return atualizados > 0;
    }

    public async Task<bool> RegistrarErroAsync(
        Guid id,
        string? erro,
        CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var mensagem = ResumirErro(erro);

        var atualizados = await _dbContext.ImpressoesPedidos
            .Where(impressao =>
                impressao.Id == id &&
                impressao.Status == StatusImpressaoPedido.Processando)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(impressao => impressao.Status, StatusImpressaoPedido.Erro)
                .SetProperty(impressao => impressao.AtualizadoEm, agora)
                .SetProperty(impressao => impressao.UltimoErro, mensagem),
                cancellationToken);

        return atualizados > 0;
    }

    public async Task<ImpressaoPedidoResposta?> CriarReimpressaoAsync(
        Guid pedidoId,
        CancellationToken cancellationToken = default)
    {
        var pedidoExiste = await _dbContext.Pedidos
            .AsNoTracking()
            .AnyAsync(pedido => pedido.Id == pedidoId, cancellationToken);

        if (!pedidoExiste)
        {
            return null;
        }

        var agora = DateTimeOffset.UtcNow;
        var impressao = new ImpressaoPedido
        {
            PedidoId = pedidoId,
            Status = StatusImpressaoPedido.Pendente,
            Reimpressao = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.ImpressoesPedidos.AddAsync(impressao, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await ObterPorIdAsync(impressao.Id, cancellationToken);
    }

    private async Task<ImpressaoPedidoResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var impressao = await _dbContext.ImpressoesPedidos
            .AsNoTracking()
            .Include(item => item.Pedido)
                .ThenInclude(pedido => pedido.Itens)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return impressao is null ? null : MapearResposta(impressao);
    }

    private static string ResumirErro(string? erro)
    {
        var mensagem = string.IsNullOrWhiteSpace(erro)
            ? "Falha ao enviar impressao."
            : erro.Trim();

        return mensagem.Length <= 500 ? mensagem : mensagem[..500];
    }

    private static ImpressaoPedidoResposta MapearResposta(ImpressaoPedido impressao)
    {
        return new ImpressaoPedidoResposta
        {
            Id = impressao.Id,
            PedidoId = impressao.PedidoId,
            Status = impressao.Status,
            Tentativas = impressao.Tentativas,
            Reimpressao = impressao.Reimpressao,
            CriadoEm = impressao.CriadoEm,
            AtualizadoEm = impressao.AtualizadoEm,
            ImpressoEm = impressao.ImpressoEm,
            UltimoErro = impressao.UltimoErro,
            Pedido = MapearPedido(impressao.Pedido)
        };
    }

    private static PedidoResposta MapearPedido(Pedido pedido)
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
}
