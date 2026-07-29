using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class AcompanhamentoConfiguracao : IEntityTypeConfiguration<Acompanhamento>
{
    public void Configure(EntityTypeBuilder<Acompanhamento> builder)
    {
        builder.ToTable("acompanhamento");

        builder.HasKey(acompanhamento => acompanhamento.Id);

        builder.Property(acompanhamento => acompanhamento.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(acompanhamento => acompanhamento.Descricao)
            .HasMaxLength(500);

        builder.Property(acompanhamento => acompanhamento.PrecoAdicional)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.EstaAtivo)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.EstaDisponivel)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.MotivoIndisponibilidade)
            .HasMaxLength(250);

        builder.Property(acompanhamento => acompanhamento.TipoSelecao)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.GrupoExclusivo)
            .HasMaxLength(80);

        builder.Property(acompanhamento => acompanhamento.OrdemExibicao)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.CriadoEm)
            .IsRequired();

        builder.Property(acompanhamento => acompanhamento.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(acompanhamento => acompanhamento.Nome)
            .IsUnique();

        builder.HasMany(acompanhamento => acompanhamento.PratoAcompanhamentos)
            .WithOne(pratoAcompanhamento => pratoAcompanhamento.Acompanhamento)
            .HasForeignKey(pratoAcompanhamento => pratoAcompanhamento.AcompanhamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(acompanhamento => acompanhamento.GruposAcompanhamentoItens)
            .WithOne(item => item.Acompanhamento)
            .HasForeignKey(item => item.AcompanhamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
