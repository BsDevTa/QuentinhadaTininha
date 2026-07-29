using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PrecoPratoConfiguracao : IEntityTypeConfiguration<PrecoPrato>
{
    public void Configure(EntityTypeBuilder<PrecoPrato> builder)
    {
        builder.ToTable("preco_prato");

        builder.HasKey(precoPrato => precoPrato.Id);

        builder.Property(precoPrato => precoPrato.Tamanho)
            .HasConversion<string>()
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(precoPrato => precoPrato.FormaPagamento)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(precoPrato => precoPrato.Valor)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.HasIndex(precoPrato => new
        {
            precoPrato.PratoId,
            precoPrato.Tamanho,
            precoPrato.FormaPagamento
        }).IsUnique();

        builder.HasOne(precoPrato => precoPrato.Prato)
            .WithMany(prato => prato.Precos)
            .HasForeignKey(precoPrato => precoPrato.PratoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
