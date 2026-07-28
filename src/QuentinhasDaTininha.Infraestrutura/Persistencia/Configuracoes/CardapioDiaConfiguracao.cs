using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class CardapioDiaConfiguracao : IEntityTypeConfiguration<CardapioDia>
{
    public void Configure(EntityTypeBuilder<CardapioDia> builder)
    {
        builder.ToTable("cardapio_dia");

        builder.HasKey(cardapioDia => cardapioDia.Id);

        builder.Property(cardapioDia => cardapioDia.DiaSemana)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(cardapioDia => cardapioDia.EstaAtivo)
            .IsRequired();

        builder.Property(cardapioDia => cardapioDia.Observacao)
            .HasMaxLength(500);

        builder.Property(cardapioDia => cardapioDia.CriadoEm)
            .IsRequired();

        builder.Property(cardapioDia => cardapioDia.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(cardapioDia => cardapioDia.DiaSemana)
            .IsUnique();

        builder.HasMany(cardapioDia => cardapioDia.CardapiosDiaPratos)
            .WithOne(cardapioDiaPrato => cardapioDiaPrato.CardapioDia)
            .HasForeignKey(cardapioDiaPrato => cardapioDiaPrato.CardapioDiaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
