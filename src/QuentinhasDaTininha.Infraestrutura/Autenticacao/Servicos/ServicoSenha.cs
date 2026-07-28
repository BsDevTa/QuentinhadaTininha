using BCrypt.Net;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;

namespace QuentinhasDaTininha.Infraestrutura.Autenticacao.Servicos;

public class ServicoSenha : IServicoSenha
{
    private const int WorkFactor = 12;

    public string GerarHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));
        }

        return BCrypt.Net.BCrypt.HashPassword(senha, WorkFactor);
    }

    public bool Verificar(string senha, string hash)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (SaltParseException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
