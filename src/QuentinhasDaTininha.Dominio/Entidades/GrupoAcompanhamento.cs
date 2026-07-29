namespace QuentinhasDaTininha.Dominio.Entidades;

public class GrupoAcompanhamento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool EstaAtivo { get; set; }
    public ICollection<Prato> Pratos { get; set; } = new List<Prato>();
    public ICollection<GrupoAcompanhamentoItem> Itens { get; set; } = new List<GrupoAcompanhamentoItem>();
}
