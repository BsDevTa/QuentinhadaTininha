using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Inicializacao;

public class InicializadorAdministrador
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoSenha _servicoSenha;

    public InicializadorAdministrador(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoSenha servicoSenha)
    {
        _dbContext = dbContext;
        _servicoSenha = servicoSenha;
    }

    public async Task InicializarAsync(
        string? nome,
        string? email,
        string? senha,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nome) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(senha))
        {
            return;
        }

        var emailNormalizado = email.Trim().ToLowerInvariant();

        var usuarioJaExiste = await _dbContext.UsuariosAdministrativos
            .AsNoTracking()
            .AnyAsync(
                usuarioAdministrativo => usuarioAdministrativo.Email == emailNormalizado,
                cancellationToken);

        if (usuarioJaExiste)
        {
            return;
        }

        var agora = DateTimeOffset.UtcNow;

        var usuario = new UsuarioAdministrativo
        {
            Nome = nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = _servicoSenha.GerarHash(senha),
            Perfil = PerfilUsuario.Administrador,
            EstaAtivo = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.UsuariosAdministrativos.AddAsync(usuario, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
