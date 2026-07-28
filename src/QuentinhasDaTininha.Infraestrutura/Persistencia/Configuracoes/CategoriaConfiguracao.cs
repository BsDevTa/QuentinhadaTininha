using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class CategoriaConfiguracao : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categoria");

        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(categoria => categoria.Descricao)
            .HasMaxLength(500);

        builder.Property(categoria => categoria.OrdemExibicao)
            .IsRequired();

        builder.Property(categoria => categoria.EstaAtiva)
            .IsRequired();

        builder.Property(categoria => categoria.CriadoEm)
            .IsRequired();

        builder.Property(categoria => categoria.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(categoria => categoria.Nome)
            .IsUnique();

        builder.HasMany(categoria => categoria.Pratos)
            .WithOne(prato => prato.Categoria)
            .HasForeignKey(prato => prato.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
