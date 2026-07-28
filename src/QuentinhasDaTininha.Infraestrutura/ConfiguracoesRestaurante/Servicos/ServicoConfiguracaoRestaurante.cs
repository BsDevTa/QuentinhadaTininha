using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.DTOs;
using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.ConfiguracoesRestaurante.Servicos;

public class ServicoConfiguracaoRestaurante : IServicoConfiguracaoRestaurante
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;

    public ServicoConfiguracaoRestaurante(QuentinhasDaTininhaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConfiguracaoRestauranteResposta?> ObterAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .AsNoTracking()
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        return configuracao is null ? null : MapearResposta(configuracao);
    }

    public async Task<ConfiguracaoRestauranteResposta> AtualizarAsync(
        ConfiguracaoRestauranteAtualizacaoRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var nome = NormalizarNome(requisicao.Nome);
        var configuracao = await _dbContext.ConfiguracoesRestaurante
            .OrderBy(configuracao => configuracao.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        var agora = DateTimeOffset.UtcNow;
        if (configuracao is null)
        {
            configuracao = new ConfiguracaoRestaurante
            {
                CriadoEm = agora
            };

            await _dbContext.ConfiguracoesRestaurante.AddAsync(configuracao, cancellationToken);
        }

        configuracao.Nome = nome;
        configuracao.Descricao = NormalizarTextoOpcional(requisicao.Descricao);
        configuracao.UrlLogotipo = NormalizarTextoOpcional(requisicao.UrlLogotipo);
        configuracao.UrlImagemCapa = NormalizarTextoOpcional(requisicao.UrlImagemCapa);
        configuracao.Telefone = NormalizarTextoOpcional(requisicao.Telefone);
        configuracao.Whatsapp = NormalizarTextoOpcional(requisicao.Whatsapp);
        configuracao.Endereco = NormalizarTextoOpcional(requisicao.Endereco);
        configuracao.Cidade = NormalizarTextoOpcional(requisicao.Cidade);
        configuracao.Estado = NormalizarTextoOpcional(requisicao.Estado);
        configuracao.Cep = NormalizarTextoOpcional(requisicao.Cep);
        configuracao.ModoFuncionamento = requisicao.ModoFuncionamento;
        configuracao.MensagemAberto = NormalizarTextoOpcional(requisicao.MensagemAberto);
        configuracao.MensagemFechado = NormalizarTextoOpcional(requisicao.MensagemFechado);
        configuracao.AceitaPedidos = requisicao.AceitaPedidos;
        configuracao.EstaAtivo = requisicao.Ativo;
        configuracao.AtualizadoEm = agora;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(configuracao);
    }

    private static string NormalizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.");
        }

        return nome.Trim();
    }

    private static string? NormalizarTextoOpcional(string? texto)
    {
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    private static ConfiguracaoRestauranteResposta MapearResposta(
        ConfiguracaoRestaurante configuracao)
    {
        return new ConfiguracaoRestauranteResposta
        {
            Id = configuracao.Id,
            Nome = configuracao.Nome,
            Descricao = configuracao.Descricao,
            UrlLogotipo = configuracao.UrlLogotipo,
            UrlImagemCapa = configuracao.UrlImagemCapa,
            Telefone = configuracao.Telefone,
            Whatsapp = configuracao.Whatsapp,
            Endereco = configuracao.Endereco,
            Cidade = configuracao.Cidade,
            Estado = configuracao.Estado,
            Cep = configuracao.Cep,
            ModoFuncionamento = configuracao.ModoFuncionamento,
            MensagemAberto = configuracao.MensagemAberto,
            MensagemFechado = configuracao.MensagemFechado,
            AceitaPedidos = configuracao.AceitaPedidos,
            Ativo = configuracao.EstaAtivo
        };
    }
}
