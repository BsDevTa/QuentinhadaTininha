using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class FretesBairrosPedidoItens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "pedido",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "pedido",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "pedido",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "pedido",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "pedido",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "pedido",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFrete",
                table: "pedido",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorSubtotal",
                table: "pedido",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "frete_bairro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BairroNormalizado = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frete_bairro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pedido_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomePrato = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Tamanho = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Acompanhamentos = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ValorUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Observacao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pedido_item_pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pedido_item_prato_PratoId",
                        column: x => x.PratoId,
                        principalTable: "prato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_frete_bairro_Ativo",
                table: "frete_bairro",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_frete_bairro_BairroNormalizado",
                table: "frete_bairro",
                column: "BairroNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedido_item_PedidoId",
                table: "pedido_item",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_item_PratoId",
                table: "pedido_item",
                column: "PratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "frete_bairro");

            migrationBuilder.DropTable(
                name: "pedido_item");

            migrationBuilder.DropColumn(
                name: "Cep",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "ValorFrete",
                table: "pedido");

            migrationBuilder.DropColumn(
                name: "ValorSubtotal",
                table: "pedido");
        }
    }
}
