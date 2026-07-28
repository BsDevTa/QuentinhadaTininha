using QuentinhasDaTininha.Aplicacao.Cardapios.DTOs;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Cardapios.Interfaces;

public interface IServicoCardapio
{
    Task<IReadOnlyList<CardapioDiaResposta>> ListarTodosAsync(
        CancellationToken cancellationToken = default);

    Task<CardapioDiaResposta?> ObterPorDiaAsync(
        DiaSemana diaSemana,
        CancellationToken cancellationToken = default);

    Task<CardapioDiaResposta> AtualizarAsync(
        DiaSemana diaSemana,
        CardapioDiaAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default);

    Task<bool> AlterarDisponibilidadePratoAsync(
        DiaSemana diaSemana,
        Guid pratoId,
        bool disponivel,
        CancellationToken cancellationToken = default);
}
