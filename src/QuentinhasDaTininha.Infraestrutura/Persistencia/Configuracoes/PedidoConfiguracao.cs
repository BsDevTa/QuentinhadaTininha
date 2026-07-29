using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PedidoConfiguracao : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedido");

        builder.HasKey(pedido => pedido.Id);

        builder.Property(pedido => pedido.DataPedido)
            .IsRequired();

        builder.Property(pedido => pedido.NomeCliente)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(pedido => pedido.TelefoneCliente)
            .HasMaxLength(30);

        builder.Property(pedido => pedido.ValorTotal)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(pedido => pedido.FormaPagamento)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pedido => pedido.PrecisaTroco)
            .IsRequired();

        builder.Property(pedido => pedido.ValorTroco)
            .HasPrecision(10, 2);

        builder.Property(pedido => pedido.TipoEntrega)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pedido => pedido.EnderecoEntrega)
            .HasMaxLength(250);

        builder.Property(pedido => pedido.Bairro)
            .HasMaxLength(120);

        builder.Property(pedido => pedido.Referencia)
            .HasMaxLength(250);

        builder.Property(pedido => pedido.Observacao)
            .HasMaxLength(500);

        builder.Property(pedido => pedido.CriadoEm)
            .IsRequired();

        builder.Property(pedido => pedido.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(pedido => pedido.DataPedido);

        builder.HasIndex(pedido => pedido.FormaPagamento);

        builder.HasIndex(pedido => pedido.TipoEntrega);
    }
}
