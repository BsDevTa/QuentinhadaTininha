using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Cache;

public class ControleCacheCardapioPublico : IControleCacheCardapioPublico
{
    private long _versao;

    public long Versao => Interlocked.Read(ref _versao);

    public void Invalidar()
    {
        Interlocked.Increment(ref _versao);
    }
}
