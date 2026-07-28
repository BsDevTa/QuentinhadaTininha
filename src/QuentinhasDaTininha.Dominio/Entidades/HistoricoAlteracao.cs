using QuentinhasDaTininha.Dominio.Enumeracoes;

namespace QuentinhasDaTininha.Dominio.Entidades;

public class HistoricoAlteracao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UsuarioAdministrativoId { get; set; }
    public string TipoEntidade { get; set; } = string.Empty;
    public Guid? EntidadeId { get; set; }
    public TipoAcaoHistorico Acao { get; set; } = TipoAcaoHistorico.Criacao;
    public string Descricao { get; set; } = string.Empty;
    public string? DadosAnteriores { get; set; }
    public string? DadosNovos { get; set; }
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
    public UsuarioAdministrativo? UsuarioAdministrativo { get; set; }
}
