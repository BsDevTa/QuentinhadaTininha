namespace QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

public interface IControleCacheCardapioPublico
{
    long Versao { get; }

    void Invalidar();
}
