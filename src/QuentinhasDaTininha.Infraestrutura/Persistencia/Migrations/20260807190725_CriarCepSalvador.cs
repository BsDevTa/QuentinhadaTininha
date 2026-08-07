using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarCepSalvador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cep_salvador",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Logradouro = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    BairroNormalizado = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cep_salvador", x => x.Id);
                    table.CheckConstraint("CK_cep_salvador_Cep_Tamanho", "char_length(\"Cep\") = 8");
                });

            migrationBuilder.CreateIndex(
                name: "IX_cep_salvador_Ativo",
                table: "cep_salvador",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_cep_salvador_BairroNormalizado",
                table: "cep_salvador",
                column: "BairroNormalizado");

            migrationBuilder.CreateIndex(
                name: "IX_cep_salvador_BairroNormalizado_Ativo",
                table: "cep_salvador",
                columns: new[] { "BairroNormalizado", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_cep_salvador_Cep",
                table: "cep_salvador",
                column: "Cep",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nao executar DROP TABLE automaticamente. A base de CEPs pode conter
            // dados importados; remocao exige procedimento manual deliberado.
        }
    }
}
