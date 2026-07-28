using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PratoAcompanhamentoConfiguracao : IEntityTypeConfiguration<PratoAcompanhamento>
{
    public void Configure(EntityTypeBuilder<PratoAcompanhamento> builder)
    {
        builder.ToTable("prato_acompanhamento");

        builder.HasKey(pratoAcompanhamento => new
        {
            pratoAcompanhamento.PratoId,
            pratoAcompanhamento.AcompanhamentoId
        });

        builder.Property(pratoAcompanhamento => pratoAcompanhamento.EstaIncluido)
            .IsRequired();

        builder.Property(pratoAcompanhamento => pratoAcompanhamento.EhObrigatorio)
            .IsRequired();

        builder.HasOne(pratoAcompanhamento => pratoAcompanhamento.Prato)
            .WithMany(prato => prato.PratoAcompanhamentos)
            .HasForeignKey(pratoAcompanhamento => pratoAcompanhamento.PratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pratoAcompanhamento => pratoAcompanhamento.Acompanhamento)
            .WithMany(acompanhamento => acompanhamento.PratoAcompanhamentos)
            .HasForeignKey(pratoAcompanhamento => pratoAcompanhamento.AcompanhamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
