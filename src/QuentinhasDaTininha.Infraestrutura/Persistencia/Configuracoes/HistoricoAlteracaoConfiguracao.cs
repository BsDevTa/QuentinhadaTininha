using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class HistoricoAlteracaoConfiguracao : IEntityTypeConfiguration<HistoricoAlteracao>
{
    public void Configure(EntityTypeBuilder<HistoricoAlteracao> builder)
    {
        builder.ToTable("historico_alteracao");

        builder.HasKey(historicoAlteracao => historicoAlteracao.Id);

        builder.Property(historicoAlteracao => historicoAlteracao.TipoEntidade)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(historicoAlteracao => historicoAlteracao.Acao)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(historicoAlteracao => historicoAlteracao.Descricao)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(historicoAlteracao => historicoAlteracao.DadosAnteriores)
            .HasColumnType("text");

        builder.Property(historicoAlteracao => historicoAlteracao.DadosNovos)
            .HasColumnType("text");

        builder.Property(historicoAlteracao => historicoAlteracao.CriadoEm)
            .IsRequired();

        builder.HasIndex(historicoAlteracao => historicoAlteracao.UsuarioAdministrativoId);

        builder.HasIndex(historicoAlteracao => new
        {
            historicoAlteracao.TipoEntidade,
            historicoAlteracao.EntidadeId
        });

        builder.HasOne(historicoAlteracao => historicoAlteracao.UsuarioAdministrativo)
            .WithMany(usuarioAdministrativo => usuarioAdministrativo.HistoricosAlteracao)
            .HasForeignKey(historicoAlteracao => historicoAlteracao.UsuarioAdministrativoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
