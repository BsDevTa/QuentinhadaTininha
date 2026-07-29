using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class PratoConfiguracao : IEntityTypeConfiguration<Prato>
{
    public void Configure(EntityTypeBuilder<Prato> builder)
    {
        builder.ToTable("prato");

        builder.HasKey(prato => prato.Id);

        builder.Property(prato => prato.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(prato => prato.Descricao)
            .HasMaxLength(500);

        builder.Property(prato => prato.Preco)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(prato => prato.UrlImagem)
            .HasMaxLength(500);

        builder.Property(prato => prato.EstaAtivo)
            .IsRequired();

        builder.Property(prato => prato.EstaDisponivel)
            .IsRequired();

        builder.Property(prato => prato.MotivoIndisponibilidade)
            .HasMaxLength(250);

        builder.Property(prato => prato.EhDestaque)
            .IsRequired();

        builder.Property(prato => prato.OrdemExibicao)
            .IsRequired();

        builder.Property(prato => prato.CriadoEm)
            .IsRequired();

        builder.Property(prato => prato.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(prato => prato.CategoriaId);

        builder.HasIndex(prato => prato.GrupoAcompanhamentoId);

        builder.HasIndex(prato => prato.Nome);

        builder.HasIndex(prato => prato.EstaAtivo);

        builder.HasIndex(prato => prato.EstaDisponivel);

        builder.HasIndex(prato => prato.OrdemExibicao);

        builder.HasOne(prato => prato.Categoria)
            .WithMany(categoria => categoria.Pratos)
            .HasForeignKey(prato => prato.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(prato => prato.GrupoAcompanhamento)
            .WithMany(grupo => grupo.Pratos)
            .HasForeignKey(prato => prato.GrupoAcompanhamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(prato => prato.Precos)
            .WithOne(preco => preco.Prato)
            .HasForeignKey(preco => preco.PratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(prato => prato.PratoAcompanhamentos)
            .WithOne(pratoAcompanhamento => pratoAcompanhamento.Prato)
            .HasForeignKey(pratoAcompanhamento => pratoAcompanhamento.PratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(prato => prato.CardapiosDiaPratos)
            .WithOne(cardapioDiaPrato => cardapioDiaPrato.Prato)
            .HasForeignKey(cardapioDiaPrato => cardapioDiaPrato.PratoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
