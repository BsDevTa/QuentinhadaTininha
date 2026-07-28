using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class HorarioFuncionamentoConfiguracao : IEntityTypeConfiguration<HorarioFuncionamento>
{
    public void Configure(EntityTypeBuilder<HorarioFuncionamento> builder)
    {
        builder.ToTable("horario_funcionamento");

        builder.HasKey(horarioFuncionamento => horarioFuncionamento.Id);

        builder.Property(horarioFuncionamento => horarioFuncionamento.DiaSemana)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(horarioFuncionamento => horarioFuncionamento.HoraAbertura)
            .IsRequired();

        builder.Property(horarioFuncionamento => horarioFuncionamento.HoraFechamento)
            .IsRequired();

        builder.Property(horarioFuncionamento => horarioFuncionamento.EstaAtivo)
            .IsRequired();

        builder.Property(horarioFuncionamento => horarioFuncionamento.CriadoEm)
            .IsRequired();

        builder.Property(horarioFuncionamento => horarioFuncionamento.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(horarioFuncionamento => new
        {
            horarioFuncionamento.DiaSemana,
            horarioFuncionamento.HoraAbertura,
            horarioFuncionamento.HoraFechamento
        }).IsUnique();
    }
}
