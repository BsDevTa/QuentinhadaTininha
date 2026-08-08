param(
    [string]$ConfigPath = (Join-Path (Split-Path $PSScriptRoot -Parent) 'config.local.ps1'),
    [string]$ApiProject = (Join-Path (Split-Path $PSScriptRoot -Parent) 'src/QuentinhasDaTininha.Api/QuentinhasDaTininha.Api.csproj')
)

$ErrorActionPreference = 'Stop'

function Obter-ValorObrigatorio {
    param(
        [hashtable]$Config,
        [string]$Nome
    )

    $valor = $Config[$Nome]
    if ($null -eq $valor -or [string]::IsNullOrWhiteSpace([string]$valor)) {
        throw "Preencha '$Nome' em $ConfigPath."
    }

    return [string]$valor
}

function Obter-ValorOpcional {
    param(
        [hashtable]$Config,
        [string]$Nome
    )

    $valor = $Config[$Nome]
    if ($null -eq $valor -or [string]::IsNullOrWhiteSpace([string]$valor)) {
        return $null
    }

    return [string]$valor
}

function Salvar-UserSecret {
    param(
        [string]$Chave,
        [string]$Valor
    )

    & dotnet user-secrets set $Chave $Valor --project $ApiProject | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao configurar '$Chave'."
    }

    Write-Host "Configurado: $Chave"
}

if (-not (Test-Path -LiteralPath $ApiProject)) {
    throw "Projeto API nao encontrado em $ApiProject."
}

if (-not (Test-Path -LiteralPath $ConfigPath)) {
    $exemplo = Join-Path (Split-Path $PSScriptRoot -Parent) 'config.local.example.ps1'
    throw "Arquivo local nao encontrado. Copie '$exemplo' para '$ConfigPath' e preencha os valores reais."
}

. $ConfigPath

if ($null -eq $ConfiguracaoLocal -or $ConfiguracaoLocal -isnot [hashtable]) {
    throw "O arquivo $ConfigPath deve definir um hashtable chamado `$ConfiguracaoLocal."
}

Write-Host "Configurando User Secrets locais do projeto API..."

Salvar-UserSecret 'ConnectionStrings:ConexaoPostgreSql' (Obter-ValorObrigatorio $ConfiguracaoLocal 'PostgresConnectionString')
Salvar-UserSecret 'Jwt:Chave' (Obter-ValorObrigatorio $ConfiguracaoLocal 'JwtChave')
Salvar-UserSecret 'Jwt:Emissor' (Obter-ValorObrigatorio $ConfiguracaoLocal 'JwtEmissor')
Salvar-UserSecret 'Jwt:Audiencia' (Obter-ValorObrigatorio $ConfiguracaoLocal 'JwtAudiencia')
Salvar-UserSecret 'Jwt:ExpiracaoEmMinutos' (Obter-ValorObrigatorio $ConfiguracaoLocal 'JwtExpiracaoEmMinutos')
Salvar-UserSecret 'SupabaseStorage:Url' (Obter-ValorObrigatorio $ConfiguracaoLocal 'SupabaseUrl')
Salvar-UserSecret 'SupabaseStorage:ChaveServico' (Obter-ValorObrigatorio $ConfiguracaoLocal 'SupabaseServiceKey')
Salvar-UserSecret 'SupabaseStorage:Bucket' (Obter-ValorObrigatorio $ConfiguracaoLocal 'SupabaseBucket')

$adminNome = Obter-ValorOpcional $ConfiguracaoLocal 'AdministradorInicialNome'
$adminEmail = Obter-ValorOpcional $ConfiguracaoLocal 'AdministradorInicialEmail'
$adminSenha = Obter-ValorOpcional $ConfiguracaoLocal 'AdministradorInicialSenha'

if ($adminNome -or $adminEmail -or $adminSenha) {
    if (-not ($adminNome -and $adminEmail -and $adminSenha)) {
        throw "Preencha AdministradorInicialNome, AdministradorInicialEmail e AdministradorInicialSenha juntos, ou deixe os tres vazios."
    }

    Salvar-UserSecret 'AdministradorInicial:Nome' $adminNome
    Salvar-UserSecret 'AdministradorInicial:Email' $adminEmail
    Salvar-UserSecret 'AdministradorInicial:Senha' $adminSenha
}

Write-Host "Ambiente local configurado. Nenhum valor secreto foi exibido."
