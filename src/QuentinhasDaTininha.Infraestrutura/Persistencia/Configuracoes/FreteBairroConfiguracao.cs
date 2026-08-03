using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class FreteBairroConfiguracao : IEntityTypeConfiguration<FreteBairro>
{
    public void Configure(EntityTypeBuilder<FreteBairro> builder)
    {
        builder.ToTable("frete_bairro");

        builder.HasKey(frete => frete.Id);

        builder.Property(frete => frete.Bairro)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(frete => frete.BairroNormalizado)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(frete => frete.Valor)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(frete => frete.Ativo)
            .IsRequired();

        builder.Property(frete => frete.CriadoEm)
            .IsRequired();

        builder.Property(frete => frete.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(frete => frete.BairroNormalizado)
            .IsUnique();

        builder.HasIndex(frete => frete.Ativo);
    }
}
