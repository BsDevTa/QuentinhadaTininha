namespace QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;

public interface IServicoSenha
{
    string GerarHash(string senha);

    bool Verificar(string senha, string hash);
}
