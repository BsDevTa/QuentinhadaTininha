using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class ConfiguracaoRestaurante
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? UrlLogotipo { get; set; }
    public string? UrlImagemCapa { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Instagram { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? HorarioFuncionamento { get; set; }
    public ModoFuncionamento ModoFuncionamento { get; set; } = ModoFuncionamento.Automatico;
    public DateOnly? DataOverrideManual { get; set; }
    public string? MensagemAberto { get; set; }
    public string? MensagemFechado { get; set; }
    public bool AceitaPedidos { get; set; }
    public bool EstaAtivo { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;
}
