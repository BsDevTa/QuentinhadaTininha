using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia;

public class QuentinhasDaTininhaDbContext : DbContext
{
    public QuentinhasDaTininhaDbContext(DbContextOptions<QuentinhasDaTininhaDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConfiguracaoRestaurante> ConfiguracoesRestaurante { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Prato> Pratos { get; set; } = null!;
    public DbSet<PrecoPrato> PrecosPratos { get; set; } = null!;
    public DbSet<GrupoAcompanhamento> GruposAcompanhamento { get; set; } = null!;
    public DbSet<GrupoAcompanhamentoItem> GruposAcompanhamentoItens { get; set; } = null!;
    public DbSet<Acompanhamento> Acompanhamentos { get; set; } = null!;
    public DbSet<Bebida> Bebidas { get; set; } = null!;
    public DbSet<PratoAcompanhamento> PratosAcompanhamentos { get; set; } = null!;
    public DbSet<CardapioDia> CardapiosDia { get; set; } = null!;
    public DbSet<CardapioDiaPrato> CardapiosDiaPratos { get; set; } = null!;
    public DbSet<HorarioFuncionamento> HorariosFuncionamento { get; set; } = null!;
    public DbSet<FechamentoExcepcional> FechamentosExcepcionais { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<PedidoItem> PedidosItens { get; set; } = null!;
    public DbSet<PedidoBebida> PedidosBebidas { get; set; } = null!;
    public DbSet<ImpressaoPedido> ImpressoesPedidos { get; set; } = null!;
    public DbSet<CepSalvador> CepsSalvador { get; set; } = null!;
    public DbSet<FreteBairro> FretesBairros { get; set; } = null!;
    public DbSet<FreteCep> FretesCep { get; set; } = null!;
    public DbSet<FreteBairroAlias> FretesBairrosAliases { get; set; } = null!;
    public DbSet<UsuarioAdministrativo> UsuariosAdministrativos { get; set; } = null!;
    public DbSet<HistoricoAlteracao> HistoricosAlteracoes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuentinhasDaTininhaDbContext).Assembly);
    }
}
