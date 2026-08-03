using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Aplicacao.FretesBairros.DTOs;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;
using QuentinhasDaTininha.Dominio.Entidades;
using QuentinhasDaTininha.Dominio.Utilitarios;
using QuentinhasDaTininha.Infraestrutura.Persistencia;

namespace QuentinhasDaTininha.Infraestrutura.FretesBairros.Servicos;

public class ServicoFreteBairro : IServicoFreteBairro
{
    private readonly QuentinhasDaTininhaDbContext _dbContext;
    private readonly IServicoCep _servicoCep;

    public ServicoFreteBairro(
        QuentinhasDaTininhaDbContext dbContext,
        IServicoCep servicoCep)
    {
        _dbContext = dbContext;
        _servicoCep = servicoCep;
    }

    public async Task<IReadOnlyList<FreteBairroResposta>> ListarAsync(
        string? bairro,
        bool? ativo,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FretesBairros
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(bairro))
        {
            var bairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro);
            query = query.Where(frete =>
                frete.BairroNormalizado.Contains(bairroNormalizado));
        }

        if (ativo.HasValue)
        {
            query = query.Where(frete => frete.Ativo == ativo.Value);
        }

        return await query
            .OrderBy(frete => frete.Bairro)
            .Select(frete => MapearResposta(frete))
            .ToListAsync(cancellationToken);
    }

    public async Task<FreteBairroResposta?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FretesBairros
            .AsNoTracking()
            .Where(frete => frete.Id == id)
            .Select(frete => MapearResposta(frete))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FreteBairroResposta> CriarAsync(
        FreteBairroSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var dados = await ValidarDadosAsync(requisicao, null, cancellationToken);
        var agora = DateTimeOffset.UtcNow;
        var frete = new FreteBairro
        {
            Bairro = dados.Bairro,
            BairroNormalizado = dados.BairroNormalizado,
            Valor = requisicao.Valor,
            Ativo = requisicao.Ativo,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        await _dbContext.FretesBairros.AddAsync(frete, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(frete);
    }

    public async Task<FreteBairroResposta?> AtualizarAsync(
        Guid id,
        FreteBairroSalvarRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requisicao);

        var frete = await _dbContext.FretesBairros
            .FirstOrDefaultAsync(frete => frete.Id == id, cancellationToken);

        if (frete is null)
        {
            return null;
        }

        var dados = await ValidarDadosAsync(requisicao, id, cancellationToken);
        frete.Bairro = dados.Bairro;
        frete.BairroNormalizado = dados.BairroNormalizado;
        frete.Valor = requisicao.Valor;
        frete.Ativo = requisicao.Ativo;
        frete.AtualizadoEm = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(frete);
    }

    public async Task<FreteBairroResposta?> AlterarStatusAsync(
        Guid id,
        bool ativo,
        CancellationToken cancellationToken = default)
    {
        var frete = await _dbContext.FretesBairros
            .FirstOrDefaultAsync(frete => frete.Id == id, cancellationToken);

        if (frete is null)
        {
            return null;
        }

        frete.Ativo = ativo;
        frete.AtualizadoEm = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapearResposta(frete);
    }

    public async Task<bool> ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var frete = await _dbContext.FretesBairros
            .FirstOrDefaultAsync(frete => frete.Id == id, cancellationToken);

        if (frete is null)
        {
            return false;
        }

        _dbContext.FretesBairros.Remove(frete);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<ConsultaFreteBairroResposta> ConsultarPorBairroAsync(
        string bairro,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bairro))
        {
            throw new ArgumentException("Bairro é obrigatório.");
        }

        var bairroLimpo = NormalizadorBairro.LimparNome(bairro);
        var bairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairroLimpo);
        var frete = await _dbContext.FretesBairros
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.BairroNormalizado == bairroNormalizado,
                cancellationToken);

        if (frete?.Ativo == true)
        {
            return new ConsultaFreteBairroResposta
            {
                Atendido = true,
                Bairro = frete.Bairro,
                ValorFrete = frete.Valor
            };
        }

        return new ConsultaFreteBairroResposta
        {
            Atendido = false,
            Bairro = frete?.Bairro ?? bairroLimpo,
            ValorFrete = null,
            Mensagem = "Bairro não atendido."
        };
    }

    public async Task<ConsultaFreteCepResposta> ConsultarPorCepAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var endereco = await _servicoCep.ConsultarAsync(cep, cancellationToken);
        if (endereco is null)
        {
            throw new KeyNotFoundException("CEP não encontrado. Verifique os números informados.");
        }

        var consultaFrete = string.IsNullOrWhiteSpace(endereco.Bairro)
            ? new ConsultaFreteBairroResposta
            {
                Atendido = false,
                Bairro = endereco.Bairro,
                ValorFrete = null,
                Mensagem = "Bairro não atendido."
            }
            : await ConsultarPorBairroAsync(endereco.Bairro, cancellationToken);

        return new ConsultaFreteCepResposta
        {
            Cep = endereco.Cep,
            Logradouro = endereco.Logradouro,
            Bairro = endereco.Bairro,
            Cidade = endereco.Cidade,
            Estado = endereco.Estado,
            Atendido = consultaFrete.Atendido,
            ValorFrete = consultaFrete.ValorFrete,
            Mensagem = consultaFrete.Atendido
                ? null
                : $"No momento, ainda não realizamos entregas para o bairro {endereco.Bairro}. Você pode selecionar a opção de retirada no local."
        };
    }

    private async Task<DadosFreteValidados> ValidarDadosAsync(
        FreteBairroSalvarRequisicao requisicao,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Bairro))
        {
            throw new ArgumentException("Bairro é obrigatório.");
        }

        var bairro = NormalizadorBairro.LimparNome(requisicao.Bairro);
        if (bairro.Length is < 2 or > 120)
        {
            throw new ArgumentException("Bairro deve ter entre 2 e 120 caracteres.");
        }

        if (NormalizadorBairro.ContemApenasNumeros(bairro))
        {
            throw new ArgumentException("Bairro não pode conter somente números.");
        }

        if (requisicao.Valor < 0)
        {
            throw new ArgumentException("Valor do frete não pode ser negativo.");
        }

        var bairroNormalizado = NormalizadorBairro.NormalizarParaComparacao(bairro);
        var duplicado = await _dbContext.FretesBairros
            .AsNoTracking()
            .AnyAsync(
                frete =>
                    frete.BairroNormalizado == bairroNormalizado &&
                    (!idIgnorado.HasValue || frete.Id != idIgnorado.Value),
                cancellationToken);

        if (duplicado)
        {
            throw new InvalidOperationException("Já existe frete cadastrado para esse bairro.");
        }

        return new DadosFreteValidados(bairro, bairroNormalizado);
    }

    private static FreteBairroResposta MapearResposta(FreteBairro frete)
    {
        return new FreteBairroResposta
        {
            Id = frete.Id,
            Bairro = frete.Bairro,
            BairroNormalizado = frete.BairroNormalizado,
            Valor = frete.Valor,
            Ativo = frete.Ativo,
            CriadoEm = frete.CriadoEm,
            AtualizadoEm = frete.AtualizadoEm
        };
    }

    private sealed record DadosFreteValidados(
        string Bairro,
        string BairroNormalizado);
}
