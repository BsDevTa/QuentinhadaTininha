namespace QuentinhasDaTininha.Aplicacao.Qz.Interfaces;

public interface IServicoQzSigning
{
    string ObterCertificado();
    string Assinar(string dados);
}
