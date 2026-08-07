using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class CepSalvadorConfiguracao : IEntityTypeConfiguration<CepSalvador>
{
    public void Configure(EntityTypeBuilder<CepSalvador> builder)
    {
        builder.ToTable("cep_salvador", tabela =>
            tabela.HasCheckConstraint(
                "CK_cep_salvador_Cep_Tamanho",
                "char_length(\"Cep\") = 8"));

        builder.HasKey(cep => cep.Id);

        builder.Property(cep => cep.Id)
            .HasColumnName("Id");

        builder.Property(cep => cep.Cep)
            .HasColumnName("Cep")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(cep => cep.Logradouro)
            .HasColumnName("Logradouro")
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(cep => cep.Bairro)
            .HasColumnName("Bairro")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(cep => cep.BairroNormalizado)
            .HasColumnName("BairroNormalizado")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(cep => cep.Cidade)
            .HasColumnName("Cidade")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cep => cep.Uf)
            .HasColumnName("Uf")
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(cep => cep.Ativo)
            .HasColumnName("Ativo")
            .IsRequired();

        builder.Property(cep => cep.CriadoEm)
            .HasColumnName("CriadoEm")
            .IsRequired();

        builder.Property(cep => cep.AtualizadoEm)
            .HasColumnName("AtualizadoEm")
            .IsRequired();

        builder.HasIndex(cep => cep.Cep)
            .IsUnique();

        builder.HasIndex(cep => cep.BairroNormalizado);

        builder.HasIndex(cep => cep.Ativo);

        builder.HasIndex(cep => new { cep.BairroNormalizado, cep.Ativo });
    }
}
