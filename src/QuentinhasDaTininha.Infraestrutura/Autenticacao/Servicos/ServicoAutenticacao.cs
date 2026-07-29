using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Autenticacao.DTOs;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.Autenticacao.Servicos;

public class ServicoAutenticacao : IServicoAutenticacao
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoSenha _servicoSenha;
    private readonly IServicoToken _servicoToken;
    private readonly int _expiracaoEmMinutos;

    public ServicoAutenticacao(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoSenha servicoSenha,
        IServicoToken servicoToken,
        int expiracaoEmMinutos)
    {
        _dbContext = dbContext;
        _servicoSenha = servicoSenha;
        _servicoToken = servicoToken;
        _expiracaoEmMinutos = expiracaoEmMinutos;
    }

    public async Task<LoginResposta?> AutenticarAsync(
        LoginRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        if (string.IsNullOrWhiteSpace(requisicao.Email) ||
            string.IsNullOrWhiteSpace(requisicao.Senha))
        {
            return null;
        }

        var emailNormalizado = requisicao.Email.Trim().ToLowerInvariant();

        var usuario = await _dbContext.UsuariosAdministrativos
            .FirstOrDefaultAsync(
                usuarioAdministrativo => usuarioAdministrativo.Email == emailNormalizado,
                cancellationToken);

        if (usuario is null || !usuario.EstaAtivo)
        {
            return null;
        }

        if (!_servicoSenha.Verificar(requisicao.Senha, usuario.SenhaHash))
        {
            return null;
        }

        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(_expiracaoEmMinutos);
        var token = _servicoToken.GerarToken(usuario, expiraEm);
        usuario.UltimoAcessoEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoEm = usuario.UltimoAcessoEm.Value;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResposta
        {
            Token = token,
            TipoToken = "Bearer",
            ExpiraEm = expiraEm,
            Usuario = new UsuarioAutenticadoDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Perfil = usuario.Perfil
            }
        };
    }
}
