using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class FreteBairroAliasConfiguracao : IEntityTypeConfiguration<FreteBairroAlias>
{
    public void Configure(EntityTypeBuilder<FreteBairroAlias> builder)
    {
        builder.ToTable("frete_bairro_alias");

        builder.HasKey(alias => alias.Id);

        builder.Property(alias => alias.Id)
            .HasColumnName("Id");

        builder.Property(alias => alias.FreteBairroId)
            .HasColumnName("FreteBairroId")
            .IsRequired();

        builder.Property(alias => alias.AliasNormalizado)
            .HasColumnName("AliasNormalizado")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(alias => alias.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(alias => alias.GeradoAutomaticamente)
            .HasColumnName("GeradoAutomaticamente")
            .IsRequired();

        builder.Property(alias => alias.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(alias => alias.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired();

        builder.HasIndex(alias => alias.AliasNormalizado)
            .IsUnique();

        builder.HasIndex(alias => alias.Ativo);

        builder.HasOne(alias => alias.FreteBairro)
            .WithMany(freteBairro => freteBairro.Aliases)
            .HasForeignKey(alias => alias.FreteBairroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
