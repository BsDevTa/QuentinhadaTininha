using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuentinhasDaTininha.Dominio.Entidades;

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Configuracoes;

public class ConfiguracaoRestauranteConfiguracao : IEntityTypeConfiguration<ConfiguracaoRestaurante>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoRestaurante> builder)
    {
        builder.ToTable("configuracao_restaurante");

        builder.HasKey(configuracaoRestaurante => configuracaoRestaurante.Id);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Nome)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Descricao)
            .HasMaxLength(500);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.UrlLogotipo)
            .HasMaxLength(500);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.UrlImagemCapa)
            .HasMaxLength(500);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Telefone)
            .HasMaxLength(20);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Whatsapp)
            .HasMaxLength(20);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Endereco)
            .HasMaxLength(250);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Cidade)
            .HasMaxLength(100);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Estado)
            .HasMaxLength(2);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.Cep)
            .HasMaxLength(10);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.ModoFuncionamento)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.MensagemAberto)
            .HasMaxLength(250);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.MensagemFechado)
            .HasMaxLength(250);

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.AceitaPedidos)
            .IsRequired();

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.EstaAtivo)
            .IsRequired();

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.CriadoEm)
            .IsRequired();

        builder.Property(configuracaoRestaurante => configuracaoRestaurante.AtualizadoEm)
            .IsRequired();
    }
}
