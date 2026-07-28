using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.DTOs;

public class ConfiguracaoRestauranteResposta
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlLogotipo { get; set; }
    public string? UrlImagemCapa { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public ModoFuncionamento ModoFuncionamento { get; set; }
    public string? MensagemAberto { get; set; }
    public string? MensagemFechado { get; set; }
    public bool AceitaPedidos { get; set; }
    public bool Ativo { get; set; }
}
