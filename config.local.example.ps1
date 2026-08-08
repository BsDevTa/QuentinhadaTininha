# Copie este arquivo para config.local.ps1 e preencha com os valores reais da maquina.
# O arquivo config.local.ps1 esta no .gitignore e nao deve ser commitado.

$ConfiguracaoLocal = @{
    PostgresConnectionString = ''

    JwtChave = ''
    JwtEmissor = ''
    JwtAudiencia = ''
    JwtExpiracaoEmMinutos = '120'

    SupabaseUrl = ''
    SupabaseServiceKey = ''
    SupabaseBucket = ''

    # Opcional: use apenas quando quiser semear/criar o administrador inicial localmente.
    AdministradorInicialNome = ''
    AdministradorInicialEmail = ''
    AdministradorInicialSenha = ''
}
