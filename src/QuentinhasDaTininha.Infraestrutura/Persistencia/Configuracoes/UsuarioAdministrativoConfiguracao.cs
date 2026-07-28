using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class UsuarioAdministrativoConfiguracao : IEntityTypeConfiguration<UsuarioAdministrativo>
{
    public void Configure(EntityTypeBuilder<UsuarioAdministrativo> builder)
    {
        builder.ToTable("usuario_administrativo");

        builder.HasKey(usuarioAdministrativo => usuarioAdministrativo.Id);

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.Email)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.SenhaHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.Perfil)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.EstaAtivo)
            .IsRequired();

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.CriadoEm)
            .IsRequired();

        builder.Property(usuarioAdministrativo => usuarioAdministrativo.AtualizadoEm)
            .IsRequired();

        builder.HasIndex(usuarioAdministrativo => usuarioAdministrativo.Email)
            .IsUnique();

        builder.HasMany(usuarioAdministrativo => usuarioAdministrativo.HistoricosAlteracao)
            .WithOne(historicoAlteracao => historicoAlteracao.UsuarioAdministrativo)
            .HasForeignKey(historicoAlteracao => historicoAlteracao.UsuarioAdministrativoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
