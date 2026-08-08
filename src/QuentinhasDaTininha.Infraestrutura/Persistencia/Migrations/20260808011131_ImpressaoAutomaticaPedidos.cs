using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class ImpressaoAutomaticaPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "impressao_pedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PedidoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    Reimpressao = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ImpressoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UltimoErro = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_impressao_pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_impressao_pedido_pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_impressao_pedido_AtualizadoEm",
                table: "impressao_pedido",
                column: "AtualizadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_impressao_pedido_PedidoId",
                table: "impressao_pedido",
                column: "PedidoId",
                unique: true,
                filter: "\"Reimpressao\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_impressao_pedido_Status",
                table: "impressao_pedido",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "impressao_pedido");
        }
    }
}
