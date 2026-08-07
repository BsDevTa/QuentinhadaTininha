using Microsoft.EntityFrameworkCore;
using QuentinhasDaTininha.Aplicacao.Ceps.DTOs;
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

        await GarantirAliasNormalizadoLivreAsync(
            dados.BairroNormalizado,
            idIgnorado: null,
            cancellationToken);

        frete.Aliases.Add(CriarAliasAutomatico(
            frete.Id,
            dados.BairroNormalizado,
            agora));

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
        var agora = DateTimeOffset.UtcNow;

        await SincronizarAliasAutomaticoAsync(
            frete.Id,
            dados.BairroNormalizado,
            agora,
            cancellationToken);

        frete.Bairro = dados.Bairro;
        frete.BairroNormalizado = dados.BairroNormalizado;
        frete.Valor = requisicao.Valor;
        frete.Ativo = requisicao.Ativo;
        frete.AtualizadoEm = agora;

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
        var frete = await _dbContext.FretesBairrosAliases
            .AsNoTracking()
            .Where(alias =>
                alias.AliasNormalizado == bairroNormalizado &&
                alias.Ativo &&
                alias.FreteBairro.Ativo)
            .Select(alias => new DadosFreteEncontrado(
                alias.FreteBairro.Bairro,
                alias.FreteBairro.Valor))
            .FirstOrDefaultAsync(
                cancellationToken);

        if (frete is not null)
        {
            return new ConsultaFreteBairroResposta
            {
                Atendido = true,
                Bairro = frete.Bairro,
                ValorFrete = frete.ValorFrete
            };
        }

        return new ConsultaFreteBairroResposta
        {
            Atendido = false,
            Bairro = bairroLimpo,
            ValorFrete = null,
            Mensagem = "Bairro não atendido."
        };
    }

    public async Task<ConsultaFreteCepResposta> ConsultarPorCepAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var cepNumerico = NormalizadorCep.SomenteNumeros(cep);
        if (cepNumerico.Length != 8)
        {
            throw new ArgumentException("Informe um CEP com 8 números.");
        }

        var fretePorCep = await BuscarFretePorCepAsync(cepNumerico, cancellationToken);
        var enderecoLocal = await BuscarEnderecoLocalAsync(cepNumerico, cancellationToken);

        if (enderecoLocal is not null)
        {
            if (!EhSalvadorBahia(enderecoLocal) && fretePorCep is null)
            {
                return MapearConsultaCepNaoAtendido(enderecoLocal);
            }

            return await ConsultarFreteComEnderecoAsync(
                enderecoLocal,
                fretePorCep,
                cancellationToken);
        }

        var enderecoViaCep = await _servicoCep.ConsultarAsync(
            cepNumerico,
            cancellationToken);
        if (enderecoViaCep is null)
        {
            throw new KeyNotFoundException("CEP não encontrado. Verifique os números informados.");
        }

        var endereco = MapearEnderecoViaCep(enderecoViaCep, cepNumerico);
        if (fretePorCep is not null)
        {
            return MapearConsultaCep(endereco, fretePorCep);
        }

        if (!EhSalvadorBahia(endereco))
        {
            return MapearConsultaCepNaoAtendido(endereco);
        }

        return await ConsultarFreteComEnderecoAsync(
            endereco,
            fretePorCep: null,
            cancellationToken);
    }

    private async Task<DadosFreteEncontrado?> BuscarFretePorCepAsync(
        string cep,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FretesCep
            .AsNoTracking()
            .Where(freteCep =>
                freteCep.Cep == cep &&
                freteCep.Ativo &&
                freteCep.FreteBairro.Ativo)
            .Select(freteCep => new DadosFreteEncontrado(
                freteCep.FreteBairro.Bairro,
                freteCep.FreteBairro.Valor))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<DadosEnderecoCep?> BuscarEnderecoLocalAsync(
        string cep,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CepsSalvador
            .AsNoTracking()
            .Where(cepSalvador =>
                cepSalvador.Cep == cep &&
                cepSalvador.Ativo)
            .Select(cepSalvador => new DadosEnderecoCep(
                cepSalvador.Cep,
                cepSalvador.Logradouro,
                cepSalvador.Bairro,
                cepSalvador.Cidade,
                cepSalvador.Uf,
                cepSalvador.BairroNormalizado))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<DadosFreteEncontrado?> BuscarFretePorBairroNormalizadoAsync(
        string bairroNormalizado,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FretesBairros
            .AsNoTracking()
            .Where(frete =>
                frete.BairroNormalizado == bairroNormalizado &&
                frete.Ativo)
            .Select(frete => new DadosFreteEncontrado(
                frete.Bairro,
                frete.Valor))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<DadosFreteEncontrado?> BuscarFretePorAliasAsync(
        string aliasNormalizado,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FretesBairrosAliases
            .AsNoTracking()
            .Where(alias =>
                alias.AliasNormalizado == aliasNormalizado &&
                alias.Ativo &&
                alias.FreteBairro.Ativo)
            .Select(alias => new DadosFreteEncontrado(
                alias.FreteBairro.Bairro,
                alias.FreteBairro.Valor))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ConsultaFreteCepResposta> ConsultarFreteComEnderecoAsync(
        DadosEnderecoCep endereco,
        DadosFreteEncontrado? fretePorCep,
        CancellationToken cancellationToken)
    {
        if (fretePorCep is not null)
        {
            return MapearConsultaCep(endereco, fretePorCep);
        }

        var fretePorBairro = string.IsNullOrWhiteSpace(endereco.BairroNormalizado)
            ? null
            : await BuscarFretePorBairroNormalizadoAsync(
                endereco.BairroNormalizado,
                cancellationToken);

        if (fretePorBairro is not null)
        {
            return MapearConsultaCep(endereco, fretePorBairro);
        }

        var fretePorAlias = string.IsNullOrWhiteSpace(endereco.BairroNormalizado)
            ? null
            : await BuscarFretePorAliasAsync(
                endereco.BairroNormalizado,
                cancellationToken);

        return fretePorAlias is null
            ? MapearConsultaCepNaoAtendido(endereco)
            : MapearConsultaCep(endereco, fretePorAlias);
    }

    private static DadosEnderecoCep MapearEnderecoViaCep(
        EnderecoCepResposta endereco,
        string cepNumerico)
    {
        var bairroNormalizado = string.IsNullOrWhiteSpace(endereco.Bairro)
            ? string.Empty
            : NormalizadorBairro.NormalizarParaComparacao(endereco.Bairro);

        return new DadosEnderecoCep(
            cepNumerico,
            endereco.Logradouro,
            endereco.Bairro,
            endereco.Cidade,
            endereco.Estado,
            bairroNormalizado);
    }

    private static bool EhSalvadorBahia(DadosEnderecoCep endereco)
    {
        var cidadeNormalizada = string.IsNullOrWhiteSpace(endereco.Cidade)
            ? string.Empty
            : NormalizadorBairro.NormalizarParaComparacao(endereco.Cidade);

        return cidadeNormalizada == "salvador" &&
            string.Equals(endereco.Estado, "BA", StringComparison.OrdinalIgnoreCase);
    }

    private static ConsultaFreteCepResposta MapearConsultaCep(
        DadosEnderecoCep endereco,
        DadosFreteEncontrado frete)
    {
        return new ConsultaFreteCepResposta
        {
            Cep = NormalizadorCep.Formatar(endereco.Cep),
            Logradouro = endereco.Logradouro,
            Bairro = endereco.Bairro,
            Cidade = endereco.Cidade,
            Estado = endereco.Estado,
            BairroFrete = frete.Bairro,
            Atendido = true,
            ValorFrete = frete.ValorFrete
        };
    }

    private static ConsultaFreteCepResposta MapearConsultaCepNaoAtendido(
        DadosEnderecoCep endereco)
    {
        return new ConsultaFreteCepResposta
        {
            Cep = NormalizadorCep.Formatar(endereco.Cep),
            Logradouro = endereco.Logradouro,
            Bairro = endereco.Bairro,
            Cidade = endereco.Cidade,
            Estado = endereco.Estado,
            BairroFrete = null,
            Atendido = false,
            ValorFrete = null,
            Mensagem = "No momento não realizamos entregas para esta localidade."
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

    private async Task SincronizarAliasAutomaticoAsync(
        Guid freteBairroId,
        string aliasNormalizado,
        DateTimeOffset agora,
        CancellationToken cancellationToken)
    {
        var aliasAutomatico = await _dbContext.FretesBairrosAliases
            .Where(alias =>
                alias.FreteBairroId == freteBairroId &&
                alias.GeradoAutomaticamente)
            .OrderBy(alias => alias.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

        await GarantirAliasNormalizadoLivreAsync(
            aliasNormalizado,
            aliasAutomatico?.Id,
            cancellationToken);

        if (aliasAutomatico is null)
        {
            await _dbContext.FretesBairrosAliases.AddAsync(
                CriarAliasAutomatico(freteBairroId, aliasNormalizado, agora),
                cancellationToken);

            return;
        }

        aliasAutomatico.AliasNormalizado = aliasNormalizado;
        aliasAutomatico.Ativo = true;
        aliasAutomatico.AtualizadoEm = agora;
    }

    private async Task GarantirAliasNormalizadoLivreAsync(
        string aliasNormalizado,
        Guid? idIgnorado,
        CancellationToken cancellationToken)
    {
        var aliasJaExiste = await _dbContext.FretesBairrosAliases
            .AsNoTracking()
            .AnyAsync(
                alias =>
                    alias.AliasNormalizado == aliasNormalizado &&
                    (!idIgnorado.HasValue || alias.Id != idIgnorado.Value),
                cancellationToken);

        if (aliasJaExiste)
        {
            throw new InvalidOperationException("Já existe alias cadastrado para esse bairro.");
        }
    }

    private static FreteBairroAlias CriarAliasAutomatico(
        Guid freteBairroId,
        string aliasNormalizado,
        DateTimeOffset agora)
    {
        return new FreteBairroAlias
        {
            FreteBairroId = freteBairroId,
            AliasNormalizado = aliasNormalizado,
            Ativo = true,
            GeradoAutomaticamente = true,
            CriadoEm = agora,
            AtualizadoEm = agora
        };
    }

    private static FreteBairroResposta MapearResposta(FreteBairro frete)
    {
        return new FreteBairroResposta
        {
            Id = frete.Id,
            Bairro = frete.Bairro,
            Valor = frete.Valor,
            Ativo = frete.Ativo,
            CriadoEm = frete.CriadoEm,
            AtualizadoEm = frete.AtualizadoEm
        };
    }

    private sealed record DadosFreteValidados(
        string Bairro,
        string BairroNormalizado);

    private sealed record DadosFreteEncontrado(
        string Bairro,
        decimal ValorFrete);

    private sealed record DadosEnderecoCep(
        string Cep,
        string? Logradouro,
        string Bairro,
        string Cidade,
        string Estado,
        string BairroNormalizado);
}
