namespace QuentinhasDaTininha.Aplicacao.Armazenamento.DTOs;

public class ArquivoUploadRequisicao
{
    public Stream Conteudo { get; set; } = Stream.Null;
    public string NomeArquivo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public long Tamanho { get; set; }
}
