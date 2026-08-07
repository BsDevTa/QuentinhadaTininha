using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class FreteCepConfiguracao : IEntityTypeConfiguration<FreteCep>
{
    public void Configure(EntityTypeBuilder<FreteCep> builder)
    {
        builder.ToTable("frete_cep");

        builder.HasKey(freteCep => freteCep.Id);

        builder.Property(freteCep => freteCep.Id)
            .HasColumnName("Id");

        builder.Property(freteCep => freteCep.FreteBairroId)
            .HasColumnName("FreteBairroId")
            .IsRequired();

        builder.Property(freteCep => freteCep.Cep)
            .HasColumnName("Cep")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(freteCep => freteCep.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(freteCep => freteCep.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(freteCep => freteCep.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired();

        builder.HasIndex(freteCep => freteCep.Cep)
            .IsUnique();

        builder.HasIndex(freteCep => freteCep.Ativo);

        builder.HasOne(freteCep => freteCep.FreteBairro)
            .WithMany(freteBairro => freteBairro.Ceps)
            .HasForeignKey(freteCep => freteCep.FreteBairroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
