using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class PedidosDisponibilidadeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PermitirPedidos",
                table: "fechamento_excepcional",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataPedido = table.Column<DateOnly>(type: "date", nullable: false),
                    NomeCliente = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TelefoneCliente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PrecisaTroco = table.Column<bool>(type: "boolean", nullable: false),
                    ValorTroco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TipoEntrega = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EnderecoEntrega = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Referencia = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fechamento_excepcional_DataFechamento_EstaAtivo",
                table: "fechamento_excepcional",
                columns: new[] { "DataFechamento", "EstaAtivo" });

            migrationBuilder.CreateIndex(
                name: "IX_pedido_DataPedido",
                table: "pedido",
                column: "DataPedido");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_FormaPagamento",
                table: "pedido",
                column: "FormaPagamento");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_TipoEntrega",
                table: "pedido",
                column: "TipoEntrega");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropIndex(
                name: "IX_fechamento_excepcional_DataFechamento_EstaAtivo",
                table: "fechamento_excepcional");

            migrationBuilder.DropColumn(
                name: "PermitirPedidos",
                table: "fechamento_excepcional");
        }
    }
}
