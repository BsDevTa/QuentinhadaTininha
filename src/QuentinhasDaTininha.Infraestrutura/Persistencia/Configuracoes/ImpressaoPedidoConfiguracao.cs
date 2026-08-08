using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class ImpressaoPedidoConfiguracao : IEntityTypeConfiguration<ImpressaoPedido>
{
    public void Configure(EntityTypeBuilder<ImpressaoPedido> builder)
    {
        builder.ToTable("impressao_pedido");

        builder.HasKey(impressao => impressao.Id);

        builder.Property(impressao => impressao.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(impressao => impressao.Tentativas)
            .IsRequired();

        builder.Property(impressao => impressao.Reimpressao)
            .IsRequired();

        builder.Property(impressao => impressao.CriadoEm)
            .IsRequired();

        builder.Property(impressao => impressao.AtualizadoEm)
            .IsRequired();

        builder.Property(impressao => impressao.UltimoErro)
            .HasMaxLength(500);

        builder.HasIndex(impressao => impressao.Status);

        builder.HasIndex(impressao => impressao.AtualizadoEm);

        builder.HasIndex(impressao => impressao.PedidoId)
            .IsUnique()
            .HasFilter("\"Reimpressao\" = false");

        builder.HasOne(impressao => impressao.Pedido)
            .WithMany()
            .HasForeignKey(impressao => impressao.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
