using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;

public interface IServicoToken
{
    string GerarToken(
        UsuarioAdministrativo usuario,
        DateTimeOffset expiraEm);
}
