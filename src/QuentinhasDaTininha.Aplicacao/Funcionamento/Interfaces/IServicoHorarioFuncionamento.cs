using QuentinhasDaTininha.Aplicacao.Funcionamento.DTOs;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;

public interface IServicoHorarioFuncionamento
{
    Task<IReadOnlyList<HorarioFuncionamentoResposta>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HorarioFuncionamentoResposta>> SubstituirDiaAsync(
        DiaSemana diaSemana,
        IReadOnlyCollection<HorarioFuncionamentoRequisicao> horarios,
        CancellationToken cancellationToken = default);

    Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
