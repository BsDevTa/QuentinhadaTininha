using QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;

namespace QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;

public interface IServicoAutenticacao
{
    Task<LoginResposta?> AutenticarAsync(
        LoginRequisicao requisicao,
        CancellationToken cancellationToken = default);
}
