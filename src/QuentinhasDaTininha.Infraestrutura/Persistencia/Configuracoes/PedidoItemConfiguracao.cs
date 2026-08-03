using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PedidoItemConfiguracao : IEntityTypeConfiguration<PedidoItem>
{
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
        builder.ToTable("pedido_item");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.NomePrato)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(item => item.Tamanho)
            .HasConversion<string>()
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(item => item.Acompanhamentos)
            .HasMaxLength(1000);

        builder.Property(item => item.ValorUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(item => item.Observacao)
            .HasMaxLength(250);

        builder.Property(item => item.CriadoEm)
            .IsRequired();

        builder.HasIndex(item => item.PedidoId);

        builder.HasIndex(item => item.PratoId);

        builder.HasOne(item => item.Pedido)
            .WithMany(pedido => pedido.Itens)
            .HasForeignKey(item => item.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Prato)
            .WithMany()
            .HasForeignKey(item => item.PratoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
