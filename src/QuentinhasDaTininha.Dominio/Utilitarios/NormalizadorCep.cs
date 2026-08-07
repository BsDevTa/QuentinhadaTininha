namespace QuentinhasDaTininha.Dominio.Utilitarios;

public static class NormalizadorCep
{
    public static string SomenteNumeros(string? valor)
    {
        return new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    public static string Formatar(string? valor)
    {
        var numeros = SomenteNumeros(valor);
        return numeros.Length == 8
            ? $"{numeros[..5]}-{numeros[5..]}"
            : numeros;
    }
}
