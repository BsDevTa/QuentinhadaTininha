using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acompanhamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrecoAdicional = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    EstaDisponivel = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoIndisponibilidade = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acompanhamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cardapio_dia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cardapio_dia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "configuracao_restaurante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UrlLogotipo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UrlImagemCapa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Whatsapp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Cep = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ModoFuncionamento = table.Column<int>(type: "integer", nullable: false),
                    MensagemAberto = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MensagemFechado = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AceitaPedidos = table.Column<bool>(type: "boolean", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracao_restaurante", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fechamento_excepcional",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataFechamento = table.Column<DateOnly>(type: "date", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MensagemCliente = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    DiaInteiro = table.Column<bool>(type: "boolean", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    HoraFim = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechamento_excepcional", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "horario_funcionamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraAbertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFechamento = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horario_funcionamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuario_administrativo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    UltimoAcessoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_administrativo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    UrlImagem = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false),
                    EstaDisponivel = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoIndisponibilidade = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EhDestaque = table.Column<bool>(type: "boolean", nullable: false),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prato_categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "historico_alteracao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAdministrativoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoEntidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntidadeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Acao = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DadosAnteriores = table.Column<string>(type: "text", nullable: true),
                    DadosNovos = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_alteracao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_historico_alteracao_usuario_administrativo_UsuarioAdministr~",
                        column: x => x.UsuarioAdministrativoId,
                        principalTable: "usuario_administrativo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "cardapio_dia_prato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardapioDiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false),
                    EstaDisponivel = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cardapio_dia_prato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cardapio_dia_prato_cardapio_dia_CardapioDiaId",
                        column: x => x.CardapioDiaId,
                        principalTable: "cardapio_dia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cardapio_dia_prato_prato_PratoId",
                        column: x => x.PratoId,
                        principalTable: "prato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prato_acompanhamento",
                columns: table => new
                {
                    PratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcompanhamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstaIncluido = table.Column<bool>(type: "boolean", nullable: false),
                    EhObrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    QuantidadeMaxima = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prato_acompanhamento", x => new { x.PratoId, x.AcompanhamentoId });
                    table.ForeignKey(
                        name: "FK_prato_acompanhamento_acompanhamento_AcompanhamentoId",
                        column: x => x.AcompanhamentoId,
                        principalTable: "acompanhamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prato_acompanhamento_prato_PratoId",
                        column: x => x.PratoId,
                        principalTable: "prato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_acompanhamento_Nome",
                table: "acompanhamento",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_dia_DiaSemana",
                table: "cardapio_dia",
                column: "DiaSemana",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_dia_prato_CardapioDiaId_PratoId",
                table: "cardapio_dia_prato",
                columns: new[] { "CardapioDiaId", "PratoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cardapio_dia_prato_PratoId",
                table: "cardapio_dia_prato",
                column: "PratoId");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_Nome",
                table: "categoria",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_excepcional_DataFechamento",
                table: "fechamento_excepcional",
                column: "DataFechamento");

            migrationBuilder.CreateIndex(
                name: "IX_historico_alteracao_TipoEntidade_EntidadeId",
                table: "historico_alteracao",
                columns: new[] { "TipoEntidade", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_historico_alteracao_UsuarioAdministrativoId",
                table: "historico_alteracao",
                column: "UsuarioAdministrativoId");

            migrationBuilder.CreateIndex(
                name: "IX_horario_funcionamento_DiaSemana_HoraAbertura_HoraFechamento",
                table: "horario_funcionamento",
                columns: new[] { "DiaSemana", "HoraAbertura", "HoraFechamento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prato_CategoriaId",
                table: "prato",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_prato_EstaAtivo",
                table: "prato",
                column: "EstaAtivo");

            migrationBuilder.CreateIndex(
                name: "IX_prato_EstaDisponivel",
                table: "prato",
                column: "EstaDisponivel");

            migrationBuilder.CreateIndex(
                name: "IX_prato_Nome",
                table: "prato",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_prato_OrdemExibicao",
                table: "prato",
                column: "OrdemExibicao");

            migrationBuilder.CreateIndex(
                name: "IX_prato_acompanhamento_AcompanhamentoId",
                table: "prato_acompanhamento",
                column: "AcompanhamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_administrativo_Email",
                table: "usuario_administrativo",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cardapio_dia_prato");

            migrationBuilder.DropTable(
                name: "configuracao_restaurante");

            migrationBuilder.DropTable(
                name: "fechamento_excepcional");

            migrationBuilder.DropTable(
                name: "historico_alteracao");

            migrationBuilder.DropTable(
                name: "horario_funcionamento");

            migrationBuilder.DropTable(
                name: "prato_acompanhamento");

            migrationBuilder.DropTable(
                name: "cardapio_dia");

            migrationBuilder.DropTable(
                name: "usuario_administrativo");

            migrationBuilder.DropTable(
                name: "acompanhamento");

            migrationBuilder.DropTable(
                name: "prato");

            migrationBuilder.DropTable(
                name: "categoria");
        }
    }
}
