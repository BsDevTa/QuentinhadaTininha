using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class GrupoAcompanhamentoItemConfiguracao : IEntityTypeConfiguration<GrupoAcompanhamentoItem>
{
    public void Configure(EntityTypeBuilder<GrupoAcompanhamentoItem> builder)
    {
        builder.ToTable("grupo_acompanhamento_item");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Obrigatorio)
            .IsRequired();

        builder.Property(item => item.OrdemExibicao)
            .IsRequired();

        builder.HasIndex(item => new
        {
            item.GrupoAcompanhamentoId,
            item.AcompanhamentoId
        }).IsUnique();

        builder.HasOne(item => item.GrupoAcompanhamento)
            .WithMany(grupo => grupo.Itens)
            .HasForeignKey(item => item.GrupoAcompanhamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Acompanhamento)
            .WithMany(acompanhamento => acompanhamento.GruposAcompanhamentoItens)
            .HasForeignKey(item => item.AcompanhamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
