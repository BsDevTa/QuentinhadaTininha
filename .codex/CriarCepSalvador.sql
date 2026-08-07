START TRANSACTION;

CREATE TABLE cep_salvador (
    "Id" uuid NOT NULL,
    "Cep" character varying(8) NOT NULL,
    "Logradouro" character varying(180) NOT NULL,
    "Bairro" character varying(120) NOT NULL,
    "BairroNormalizado" character varying(120) NOT NULL,
    "Cidade" character varying(100) NOT NULL,
    "Uf" character varying(2) NOT NULL,
    "Ativo" boolean NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "AtualizadoEm" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_cep_salvador" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_cep_salvador_Cep_Tamanho" CHECK (char_length("Cep") = 8)
);

CREATE INDEX "IX_cep_salvador_Ativo" ON cep_salvador ("Ativo");

CREATE INDEX "IX_cep_salvador_BairroNormalizado" ON cep_salvador ("BairroNormalizado");

CREATE INDEX "IX_cep_salvador_BairroNormalizado_Ativo" ON cep_salvador ("BairroNormalizado", "Ativo");

CREATE UNIQUE INDEX "IX_cep_salvador_Cep" ON cep_salvador ("Cep");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807190725_CriarCepSalvador', '8.0.29');

COMMIT;

