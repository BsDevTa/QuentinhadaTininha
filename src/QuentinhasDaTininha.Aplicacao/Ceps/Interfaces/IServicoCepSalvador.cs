using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;

public interface IServicoCepSalvador
{
    Task<CepSalvadorImportacaoResposta> ImportarAsync(
        IReadOnlyCollection<CepSalvadorImportacaoItem> itens,
        CancellationToken cancellationToken = default,
        int tamanhoLote = 1000);
}
