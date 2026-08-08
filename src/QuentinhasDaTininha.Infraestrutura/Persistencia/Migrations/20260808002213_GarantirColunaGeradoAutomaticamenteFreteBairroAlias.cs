using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuentinhasDaTininha.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class GarantirColunaGeradoAutomaticamenteFreteBairroAlias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE frete_bairro_alias
                ADD COLUMN IF NOT EXISTS "GeradoAutomaticamente" boolean;

                UPDATE frete_bairro_alias
                SET "GeradoAutomaticamente" = TRUE
                WHERE "GeradoAutomaticamente" IS NULL
                  AND "AliasNormalizado" IN (
                      SELECT frete."BairroNormalizado"
                      FROM frete_bairro frete
                      WHERE frete."Id" = frete_bairro_alias."FreteBairroId"
                  );

                UPDATE frete_bairro_alias
                SET "GeradoAutomaticamente" = FALSE
                WHERE "GeradoAutomaticamente" IS NULL;

                ALTER TABLE frete_bairro_alias
                ALTER COLUMN "GeradoAutomaticamente" SET DEFAULT FALSE;

                ALTER TABLE frete_bairro_alias
                ALTER COLUMN "GeradoAutomaticamente" SET NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE frete_bairro_alias
                DROP COLUMN IF EXISTS "GeradoAutomaticamente";
                """);
        }
    }
}
