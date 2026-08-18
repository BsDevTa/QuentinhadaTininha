using QuentinhasDaTininha.Aplicacao.Bebidas.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Bebidas.Interfaces;

public interface IServicoBebida
{
    Task<IReadOnlyList<BebidaResposta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<BebidaResposta?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BebidaResposta> CriarAsync(BebidaSalvarRequisicao requisicao, CancellationToken cancellationToken = default);
    Task<BebidaResposta?> AtualizarAsync(Guid id, BebidaSalvarRequisicao requisicao, CancellationToken cancellationToken = default);
    Task<bool> ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
