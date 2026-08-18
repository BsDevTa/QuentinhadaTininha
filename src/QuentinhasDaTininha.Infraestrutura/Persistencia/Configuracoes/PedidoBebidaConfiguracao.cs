using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PedidoBebidaConfiguracao : IEntityTypeConfiguration<PedidoBebida>
{
    public void Configure(EntityTypeBuilder<PedidoBebida> builder)
    {
        builder.ToTable("pedido_bebida");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.NomeBebida)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(item => item.Quantidade)
            .IsRequired();

        builder.Property(item => item.ValorUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(item => item.CriadoEm)
            .IsRequired();

        builder.HasIndex(item => item.PedidoId);
        builder.HasIndex(item => item.BebidaId);

        builder.HasOne(item => item.Pedido)
            .WithMany(pedido => pedido.Bebidas)
            .HasForeignKey(item => item.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Bebida)
            .WithMany()
            .HasForeignKey(item => item.BebidaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
