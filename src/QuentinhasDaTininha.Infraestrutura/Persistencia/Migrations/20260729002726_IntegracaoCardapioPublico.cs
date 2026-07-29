using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class IntegracaoCardapioPublico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoAcompanhamentoId",
                table: "prato",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrupoExclusivo",
                table: "acompanhamento",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoSelecao",
                table: "acompanhamento",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "grupo_acompanhamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EstaAtivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo_acompanhamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "preco_prato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tamanho = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preco_prato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preco_prato_prato_PratoId",
                        column: x => x.PratoId,
                        principalTable: "prato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grupo_acompanhamento_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GrupoAcompanhamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcompanhamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "boolean", nullable: false),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo_acompanhamento_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grupo_acompanhamento_item_acompanhamento_AcompanhamentoId",
                        column: x => x.AcompanhamentoId,
                        principalTable: "acompanhamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grupo_acompanhamento_item_grupo_acompanhamento_GrupoAcompan~",
                        column: x => x.GrupoAcompanhamentoId,
                        principalTable: "grupo_acompanhamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prato_GrupoAcompanhamentoId",
                table: "prato",
                column: "GrupoAcompanhamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_grupo_acompanhamento_Codigo",
                table: "grupo_acompanhamento",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grupo_acompanhamento_item_AcompanhamentoId",
                table: "grupo_acompanhamento_item",
                column: "AcompanhamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_grupo_acompanhamento_item_GrupoAcompanhamentoId_Acompanhame~",
                table: "grupo_acompanhamento_item",
                columns: new[] { "GrupoAcompanhamentoId", "AcompanhamentoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preco_prato_PratoId_Tamanho_FormaPagamento",
                table: "preco_prato",
                columns: new[] { "PratoId", "Tamanho", "FormaPagamento" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_prato_grupo_acompanhamento_GrupoAcompanhamentoId",
                table: "prato",
                column: "GrupoAcompanhamentoId",
                principalTable: "grupo_acompanhamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prato_grupo_acompanhamento_GrupoAcompanhamentoId",
                table: "prato");

            migrationBuilder.DropTable(
                name: "grupo_acompanhamento_item");

            migrationBuilder.DropTable(
                name: "preco_prato");

            migrationBuilder.DropTable(
                name: "grupo_acompanhamento");

            migrationBuilder.DropIndex(
                name: "IX_prato_GrupoAcompanhamentoId",
                table: "prato");

            migrationBuilder.DropColumn(
                name: "GrupoAcompanhamentoId",
                table: "prato");

            migrationBuilder.DropColumn(
                name: "GrupoExclusivo",
                table: "acompanhamento");

            migrationBuilder.DropColumn(
                name: "TipoSelecao",
                table: "acompanhamento");
        }
    }
}
