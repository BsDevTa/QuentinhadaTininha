using QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;

public interface IServicoImagemPrato
{
    Task<string?> AtualizarImagemAsync(
        Guid pratoId,
        ArquivoUploadRequisicao arquivo,
        CancellationToken cancellationToken = default);

    Task<bool> RemoverImagemAsync(
        Guid pratoId,
        CancellationToken cancellationToken = default);
}
