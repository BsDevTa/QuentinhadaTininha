using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class CardapioDiaPratoConfiguracao : IEntityTypeConfiguration<CardapioDiaPrato>
{
    public void Configure(EntityTypeBuilder<CardapioDiaPrato> builder)
    {
        builder.ToTable("cardapio_dia_prato");

        builder.HasKey(cardapioDiaPrato => cardapioDiaPrato.Id);

        builder.Property(cardapioDiaPrato => cardapioDiaPrato.OrdemExibicao)
            .IsRequired();

        builder.Property(cardapioDiaPrato => cardapioDiaPrato.EstaDisponivel)
            .IsRequired();

        builder.HasIndex(cardapioDiaPrato => new
        {
            cardapioDiaPrato.CardapioDiaId,
            cardapioDiaPrato.PratoId
        }).IsUnique();

        builder.HasOne(cardapioDiaPrato => cardapioDiaPrato.CardapioDia)
            .WithMany(cardapioDia => cardapioDia.CardapiosDiaPratos)
            .HasForeignKey(cardapioDiaPrato => cardapioDiaPrato.CardapioDiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cardapioDiaPrato => cardapioDiaPrato.Prato)
            .WithMany(prato => prato.CardapiosDiaPratos)
            .HasForeignKey(cardapioDiaPrato => cardapioDiaPrato.PratoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
