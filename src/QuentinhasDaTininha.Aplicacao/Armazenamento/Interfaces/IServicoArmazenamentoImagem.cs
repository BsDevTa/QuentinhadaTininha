using QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Armazenamento.Interfaces;

public interface IServicoArmazenamentoImagem
{
    Task<ArquivoUploadResposta> EnviarAsync(
        ArquivoUploadRequisicao requisicao,
        string pasta,
        CancellationToken cancellationToken = default);

    Task RemoverAsync(
        string caminho,
        CancellationToken cancellationToken = default);
}
