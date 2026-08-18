namespace QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

public interface IServicoDataLocal
{
    DateTimeOffset ObterAgora();
    DateOnly ObterDataAtual();
}
