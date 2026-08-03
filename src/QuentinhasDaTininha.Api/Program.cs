using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuentinhasDaTininha.Aplicacao.Acompanhamentos.Interfaces;
using QuentinhasDaTininha.Aplicacao.Armazenamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Autenticacao.Interfaces;
using QuentinhasDaTininha.Aplicacao.Cardapios.Interfaces;
using QuentinhasDaTininha.Aplicacao.Categorias.Interfaces;
using QuentinhasDaTininha.Aplicacao.Ceps.Interfaces;
using QuentinhasDaTininha.Aplicacao.ConfiguracoesRestaurante.Interfaces;
using QuentinhasDaTininha.Aplicacao.FretesBairros.Interfaces;
using QuentinhasDaTininha.Aplicacao.Funcionamento.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pedidos.Interfaces;
using QuentinhasDaTininha.Aplicacao.Pratos.Interfaces;
using QuentinhasDaTininha.Aplicacao.Publico.Interfaces;
using QuentinhasDaTininha.Api.Configuracoes;
using QuentinhasDaTininha.Infraestrutura.Acompanhamentos.Servicos;
using QuentinhasDaTininha.Infraestrutura.Armazenamento.Servicos;
using QuentinhasDaTininha.Infraestrutura.Autenticacao.Servicos;
using QuentinhasDaTininha.Infraestrutura.Cardapios.Servicos;
using QuentinhasDaTininha.Infraestrutura.Categorias.Servicos;
using QuentinhasDaTininha.Infraestrutura.Ceps.Servicos;
using QuentinhasDaTininha.Infraestrutura.ConfiguracoesRestaurante.Servicos;
using QuentinhasDaTininha.Infraestrutura.FretesBairros.Servicos;
using QuentinhasDaTininha.Infraestrutura.Funcionamento.Servicos;
using QuentinhasDaTininha.Infraestrutura.Pedidos.Servicos;
using QuentinhasDaTininha.Infraestrutura.Persistencia;
using QuentinhasDaTininha.Infraestrutura.Persistencia.Inicializacao;
using QuentinhasDaTininha.Infraestrutura.Pratos.Servicos;
using QuentinhasDaTininha.Infraestrutura.Publico.Servicos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtConfiguracao>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SupabaseStorageConfiguracao>(
    builder.Configuration.GetSection("SupabaseStorage"));

var jwtConfiguracao = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtConfiguracao>() ?? new JwtConfiguracao();
var supabaseStorageConfiguracao = builder.Configuration
    .GetSection("SupabaseStorage")
    .Get<SupabaseStorageConfiguracao>() ?? new SupabaseStorageConfiguracao();

if (string.IsNullOrWhiteSpace(jwtConfiguracao.Chave))
{
    throw new InvalidOperationException("A chave JWT não foi configurada.");
}

if (string.IsNullOrWhiteSpace(jwtConfiguracao.Emissor))
{
    throw new InvalidOperationException("O emissor JWT não foi configurado.");
}

if (string.IsNullOrWhiteSpace(jwtConfiguracao.Audiencia))
{
    throw new InvalidOperationException("A audiência JWT não foi configurada.");
}

if (jwtConfiguracao.ExpiracaoEmMinutos <= 0)
{
    throw new InvalidOperationException("O tempo de expiração do JWT deve ser maior que zero.");
}

if (string.IsNullOrWhiteSpace(supabaseStorageConfiguracao.Url))
{
    throw new InvalidOperationException(
        "A URL do Supabase Storage não foi configurada.");
}

if (string.IsNullOrWhiteSpace(supabaseStorageConfiguracao.ChaveServico))
{
    throw new InvalidOperationException(
        "A chave do Supabase Storage não foi configurada.");
}

if (string.IsNullOrWhiteSpace(supabaseStorageConfiguracao.Bucket))
{
    throw new InvalidOperationException(
        "O bucket do Supabase Storage não foi configurado.");
}

var connectionString = builder.Configuration.GetConnectionString("ConexaoPostgreSql");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A connection string 'ConexaoPostgreSql' não foi configurada.");
}

builder.Services.AddDbContext<QuentinhasDaTininhaDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IServicoSenha, ServicoSenha>();
builder.Services.AddSingleton<IServicoToken>(_ =>
    new ServicoToken(
        jwtConfiguracao.Chave,
        jwtConfiguracao.Emissor,
        jwtConfiguracao.Audiencia));
builder.Services.AddScoped<IServicoAutenticacao>(serviceProvider =>
    new ServicoAutenticacao(
        serviceProvider.GetRequiredService<QuentinhasDaTininhaDbContext>(),
        serviceProvider.GetRequiredService<IServicoSenha>(),
        serviceProvider.GetRequiredService<IServicoToken>(),
        jwtConfiguracao.ExpiracaoEmMinutos));
builder.Services.AddScoped<IServicoCategoria, ServicoCategoria>();
builder.Services.AddScoped<IServicoAcompanhamento, ServicoAcompanhamento>();
builder.Services.AddScoped<IServicoPrato, ServicoPrato>();
builder.Services.AddScoped<IServicoImagemPrato, ServicoImagemPrato>();
builder.Services.AddScoped<IServicoCardapio, ServicoCardapio>();
builder.Services.AddScoped<IServicoHorarioFuncionamento, ServicoHorarioFuncionamento>();
builder.Services.AddScoped<IServicoFechamentoExcepcional, ServicoFechamentoExcepcional>();
builder.Services.AddScoped<IServicoDisponibilidadePedido, ServicoDisponibilidadePedido>();
builder.Services.AddScoped<IServicoConfiguracaoRestaurante, ServicoConfiguracaoRestaurante>();
builder.Services.AddScoped<IServicoFreteBairro, ServicoFreteBairro>();
builder.Services.AddScoped<IServicoPedido, ServicoPedido>();
builder.Services.AddScoped<IServicoCardapioPublico, ServicoCardapioPublico>();
builder.Services.AddScoped<IServicoDataLocal, ServicoDataLocal>();
builder.Services.AddScoped<IServicoCardapioDiaPublico, ServicoCardapioDiaPublico>();
builder.Services.AddScoped<InicializadorAdministrador>();
builder.Services.AddScoped<InicializadorCardapioPublico>();
builder.Services.AddHttpClient<IServicoCep, ServicoViaCep>(httpClient =>
{
    httpClient.BaseAddress = new Uri("https://viacep.com.br/ws/");
    httpClient.Timeout = TimeSpan.FromSeconds(6);
});
builder.Services.AddHttpClient("SupabaseStorage", httpClient =>
{
    httpClient.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<IServicoArmazenamentoImagem>(serviceProvider =>
{
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

    return new ServicoSupabaseStorage(
        httpClientFactory.CreateClient("SupabaseStorage"),
        supabaseStorageConfiguracao.Url,
        supabaseStorageConfiguracao.ChaveServico,
        supabaseStorageConfiguracao.Bucket);
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfiguracao.Chave)),
            ValidateIssuer = true,
            ValidIssuer = jwtConfiguracao.Emissor,
            ValidateAudience = true,
            ValidAudience = jwtConfiguracao.Audiencia,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "https://quentinhas-da-tininha.web.app",
                "https://quentinhas-da-tininha.firebaseapp.com",
                "https://quentinha-da-tininha.web.app",
                "https://quentinha-da-tininha.firebaseapp.com",
                "https://quentinhadatininha.web.app",
                "https://quentinhadatininha.firebaseapp.com",
                "http://localhost:4200",
                "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddHealthChecks();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<QuentinhasDaTininhaDbContext>();

    await dbContext.Database.MigrateAsync();

    var inicializadorAdministrador =
        scope.ServiceProvider.GetRequiredService<InicializadorAdministrador>();

    var administradorNome = builder.Configuration["ADMIN_NOME"] ??
        builder.Configuration["AdministradorInicial:Nome"];
    var administradorEmail = builder.Configuration["ADMIN_EMAIL"] ??
        builder.Configuration["AdministradorInicial:Email"];
    var administradorSenha = builder.Configuration["ADMIN_SENHA"] ??
        builder.Configuration["AdministradorInicial:Senha"];

    if (string.IsNullOrWhiteSpace(administradorNome) ||
        string.IsNullOrWhiteSpace(administradorEmail) ||
        string.IsNullOrWhiteSpace(administradorSenha))
    {
        app.Logger.LogWarning(
            "Administrador inicial nao configurado. Defina ADMIN_NOME, ADMIN_EMAIL e ADMIN_SENHA ou AdministradorInicial:* em User Secrets.");
    }

    await inicializadorAdministrador.InicializarAsync(
        administradorNome,
        administradorEmail,
        administradorSenha);

    var inicializadorCardapioPublico =
        scope.ServiceProvider.GetRequiredService<InicializadorCardapioPublico>();

    await inicializadorCardapioPublico.InicializarAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
