using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Autenticacao.Servicos;

public class ServicoToken : IServicoToken
{
    private readonly string _chave;
    private readonly string _emissor;
    private readonly string _audiencia;

    public ServicoToken(string chave, string emissor, string audiencia)
    {
        if (string.IsNullOrWhiteSpace(chave))
        {
            throw new ArgumentException("A chave JWT não pode ser vazia.", nameof(chave));
        }

        if (string.IsNullOrWhiteSpace(emissor))
        {
            throw new ArgumentException("O emissor JWT não pode ser vazio.", nameof(emissor));
        }

        if (string.IsNullOrWhiteSpace(audiencia))
        {
            throw new ArgumentException("A audiência JWT não pode ser vazia.", nameof(audiencia));
        }

        _chave = chave;
        _emissor = emissor;
        _audiencia = audiencia;
    }

    public string GerarToken(
        UsuarioAdministrativo usuario,
        DateTimeOffset expiraEm)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        if (expiraEm <= DateTimeOffset.UtcNow)
        {
            throw new ArgumentException("A data de expiração do token deve estar no futuro.", nameof(expiraEm));
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Nome),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("nome", usuario.Nome),
            new Claim("usuarioId", usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("role", usuario.Perfil.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_chave));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var descricaoToken = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiraEm.UtcDateTime,
            Issuer = _emissor,
            Audience = _audiencia,
            SigningCredentials = credenciais
        };

        var manipuladorToken = new JwtSecurityTokenHandler();
        var token = manipuladorToken.CreateToken(descricaoToken);

        return manipuladorToken.WriteToken(token);
    }
}
