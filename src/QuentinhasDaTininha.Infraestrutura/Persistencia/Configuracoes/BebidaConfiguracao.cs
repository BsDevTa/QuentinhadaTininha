using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class BebidaConfiguracao : IEntityTypeConfiguration<Bebida>
{
    public void Configure(EntityTypeBuilder<Bebida> builder)
    {
        builder.ToTable("bebida");

        builder.HasKey(bebida => bebida.Id);

        builder.Property(bebida => bebida.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(bebida => bebida.Descricao)
            .HasMaxLength(250);

        builder.Property(bebida => bebida.Preco)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(bebida => bebida.ImagemUrl)
            .HasMaxLength(500);

        builder.Property(bebida => bebida.Ativa)
            .IsRequired();

        builder.Property(bebida => bebida.CriadoEm)
            .IsRequired();

        builder.Property(bebida => bebida.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(bebida => bebida.Nome)
            .IsUnique();
    }
}
