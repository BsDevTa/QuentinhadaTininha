namespace QuentinhasDaTininha.Aplicacao.Ceps.DTOs;

public class CepSalvadorImportacaoResposta
{
    public int TotalRecebidos { get; set; }
    public int Validos { get; set; }
    public int Inseridos { get; set; }
    public int Atualizados { get; set; }
    public int Ignorados { get; set; }
    public int Invalidos { get; set; }
    public int Duplicados { get; set; }
    public List<string> Erros { get; set; } = new();
}
