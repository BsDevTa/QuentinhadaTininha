using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class FechamentoExcepcionalConfiguracao : IEntityTypeConfiguration<FechamentoExcepcional>
{
    public void Configure(EntityTypeBuilder<FechamentoExcepcional> builder)
    {
        builder.ToTable("fechamento_excepcional");

        builder.HasKey(fechamentoExcepcional => fechamentoExcepcional.Id);

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.DataFechamento)
            .IsRequired();

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.Motivo)
            .HasMaxLength(250);

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.MensagemCliente)
            .HasMaxLength(250);

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.DiaInteiro)
            .IsRequired();

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.EstaAtivo)
            .IsRequired();

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.CriadoEm)
            .IsRequired();

        builder.Property(fechamentoExcepcional => fechamentoExcepcional.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(fechamentoExcepcional => fechamentoExcepcional.DataFechamento);
    }
}
