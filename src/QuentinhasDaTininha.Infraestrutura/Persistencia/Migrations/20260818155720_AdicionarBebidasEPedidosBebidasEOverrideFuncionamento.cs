using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBebidasEPedidosBebidasEOverrideFuncionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataOverrideManual",
                table: "configuracao_restaurante",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bebida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    ImagemUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bebida", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pedido_bebida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BebidaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeBebida = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_bebida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pedido_bebida_bebida_BebidaId",
                        column: x => x.BebidaId,
                        principalTable: "bebida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedido_bebida_pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bebida_Nome",
                table: "bebida",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pedido_bebida_BebidaId",
                table: "pedido_bebida",
                column: "BebidaId");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_bebida_PedidoId",
                table: "pedido_bebida",
                column: "PedidoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pedido_bebida");

            migrationBuilder.DropTable(
                name: "bebida");

            migrationBuilder.DropColumn(
                name: "DataOverrideManual",
                table: "configuracao_restaurante");
        }
    }
}
