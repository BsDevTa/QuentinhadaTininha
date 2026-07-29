using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;

namespace QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

public class ServicoDataLocal : IServicoDataLocal
{
    private static readonly TimeZoneInfo TimeZone = ObterTimeZone();

    public DateOnly ObterDataAtual()
    {
        var agoraLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone);
        return DateOnly.FromDateTime(agoraLocal.DateTime);
    }

    private static TimeZoneInfo ObterTimeZone()
    {
        foreach (var id in new[] { "America/Bahia", "E. South America Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
