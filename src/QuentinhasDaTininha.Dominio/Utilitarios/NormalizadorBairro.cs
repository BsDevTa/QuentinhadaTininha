using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace QuentinhasDaTininha.Dominio.Utilitarios;

public static class NormalizadorBairro
{
    public static string LimparNome(string bairro)
    {
        return Regex.Replace(bairro.Trim(), @"\s+", " ");
    }

    public static string NormalizarParaComparacao(string bairro)
    {
        var texto = LimparNome(bairro).Normalize(NormalizationForm.FormD);
        var normalizado = new StringBuilder(texto.Length);

        foreach (var caractere in texto)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);
            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                normalizado.Append(caractere);
            }
        }

        return normalizado
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }

    public static bool ContemApenasNumeros(string bairro)
    {
        var texto = LimparNome(bairro);
        var semEspacos = texto.Replace(" ", string.Empty);

        return semEspacos.Length > 0 && semEspacos.All(char.IsDigit);
    }
}
