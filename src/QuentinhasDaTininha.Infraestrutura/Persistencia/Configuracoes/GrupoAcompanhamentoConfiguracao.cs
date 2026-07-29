using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class GrupoAcompanhamentoConfiguracao : IEntityTypeConfiguration<GrupoAcompanhamento>
{
    public void Configure(EntityTypeBuilder<GrupoAcompanhamento> builder)
    {
        builder.ToTable("grupo_acompanhamento");

        builder.HasKey(grupoAcompanhamento => grupoAcompanhamento.Id);

        builder.Property(grupoAcompanhamento => grupoAcompanhamento.Nome)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(grupoAcompanhamento => grupoAcompanhamento.Codigo)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(grupoAcompanhamento => grupoAcompanhamento.EstaAtivo)
            .IsRequired();

        builder.HasIndex(grupoAcompanhamento => grupoAcompanhamento.Codigo)
            .IsUnique();
    }
}
