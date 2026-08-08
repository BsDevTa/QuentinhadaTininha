param(
    [string]$ApiProject = (Join-Path (Split-Path $PSScriptRoot -Parent) 'src/QuentinhasDaTininha.Api/QuentinhasDaTininha.Api.csproj')
)

$ErrorActionPreference = 'Stop'

function Converter-UserSecretsParaHashtable {
    $json = & dotnet user-secrets list --json --project $ApiProject 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($json)) {
        return @{}
    }

    $jsonLimpo = ($json | Where-Object {
        $_ -notmatch '^\s*//BEGIN\s*$' -and $_ -notmatch '^\s*//END\s*$'
    }) -join [Environment]::NewLine

    $objeto = $jsonLimpo | ConvertFrom-Json
    $hash = @{}
    foreach ($propriedade in $objeto.PSObject.Properties) {
        $hash[$propriedade.Name] = [string]$propriedade.Value
    }

    return $hash
}

function Adicionar-JsonConfig {
    param(
        [hashtable]$Destino,
        [string]$Caminho
    )

    if (-not (Test-Path -LiteralPath $Caminho)) {
        return
    }

    $json = Get-Content -LiteralPath $Caminho -Raw | ConvertFrom-Json

    function Adicionar-Propriedades {
        param(
            [object]$Objeto,
            [string]$Prefixo
        )

        foreach ($propriedade in $Objeto.PSObject.Properties) {
            $chave = if ($Prefixo) { "$Prefixo`:$($propriedade.Name)" } else { $propriedade.Name }

            if ($null -ne $propriedade.Value -and $propriedade.Value -is [System.Management.Automation.PSCustomObject]) {
                Adicionar-Propriedades $propriedade.Value $chave
            } else {
                $Destino[$chave] = [string]$propriedade.Value
            }
        }
    }

    Adicionar-Propriedades $json ''
}

function Obter-CampoConnectionString {
    param(
        [string]$ConnectionString,
        [string[]]$Nomes
    )

    foreach ($parte in $ConnectionString -split ';') {
        $indice = $parte.IndexOf('=')
        if ($indice -lt 1) {
            continue
        }

        $nome = $parte.Substring(0, $indice).Trim()
        $valor = $parte.Substring($indice + 1).Trim()
        if ($Nomes -contains $nome) {
            return $valor
        }
    }

    return $null
}

function Mascarar {
    param([string]$Valor)

    if ([string]::IsNullOrWhiteSpace($Valor)) {
        return 'nao informado'
    }

    if ($Valor.Length -le 6) {
        return '***'
    }

    return "$($Valor.Substring(0, 3))***$($Valor.Substring($Valor.Length - 3))"
}

if (-not (Test-Path -LiteralPath $ApiProject)) {
    throw "Projeto API nao encontrado em $ApiProject."
}

$apiDiretorio = Split-Path $ApiProject -Parent
$configuracao = @{}
Adicionar-JsonConfig $configuracao (Join-Path $apiDiretorio 'appsettings.json')
Adicionar-JsonConfig $configuracao (Join-Path $apiDiretorio 'appsettings.Development.json')

$secrets = Converter-UserSecretsParaHashtable
foreach ($chave in $secrets.Keys) {
    $configuracao[$chave] = $secrets[$chave]
}

$connectionString = $configuracao['ConnectionStrings:ConexaoPostgreSql']

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host 'PostgreSQL: nao configurado'
} else {
    Write-Host 'PostgreSQL: configurado'
    Write-Host "Host: $(Obter-CampoConnectionString $connectionString @('Host', 'Server'))"
    Write-Host "Database: $(Obter-CampoConnectionString $connectionString @('Database', 'Initial Catalog'))"
    Write-Host "Username: $(Mascarar (Obter-CampoConnectionString $connectionString @('Username', 'User Id', 'UserID', 'User')))"
}

$jwtConfigurado =
    -not [string]::IsNullOrWhiteSpace($configuracao['Jwt:Chave']) -and
    -not [string]::IsNullOrWhiteSpace($configuracao['Jwt:Emissor']) -and
    -not [string]::IsNullOrWhiteSpace($configuracao['Jwt:Audiencia']) -and
    -not [string]::IsNullOrWhiteSpace($configuracao['Jwt:ExpiracaoEmMinutos'])

$storageConfigurado =
    -not [string]::IsNullOrWhiteSpace($configuracao['SupabaseStorage:Url']) -and
    -not [string]::IsNullOrWhiteSpace($configuracao['SupabaseStorage:ChaveServico']) -and
    -not [string]::IsNullOrWhiteSpace($configuracao['SupabaseStorage:Bucket'])

$adminConfigurado =
    (
        -not [string]::IsNullOrWhiteSpace($configuracao['AdministradorInicial:Nome']) -and
        -not [string]::IsNullOrWhiteSpace($configuracao['AdministradorInicial:Email']) -and
        -not [string]::IsNullOrWhiteSpace($configuracao['AdministradorInicial:Senha'])
    ) -or
    (
        -not [string]::IsNullOrWhiteSpace($secrets['ADMIN_NOME']) -and
        -not [string]::IsNullOrWhiteSpace($secrets['ADMIN_EMAIL']) -and
        -not [string]::IsNullOrWhiteSpace($secrets['ADMIN_SENHA'])
    )

Write-Host "JWT: $(if ($jwtConfigurado) { 'configurado' } else { 'nao configurado' })"
Write-Host "Supabase Storage: $(if ($storageConfigurado) { 'configurado' } else { 'nao configurado' })"
Write-Host "AdministradorInicial: $(if ($adminConfigurado) { 'configurado' } else { 'nao configurado' })"
