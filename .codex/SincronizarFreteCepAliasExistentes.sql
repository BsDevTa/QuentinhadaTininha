START TRANSACTION;

CREATE TABLE IF NOT EXISTS frete_bairro_alias (
    "Id" uuid NOT NULL,
    "FreteBairroId" uuid NOT NULL,
    "AliasNormalizado" character varying(120) NOT NULL,
    "Ativo" boolean NOT NULL,
    "GeradoAutomaticamente" boolean NOT NULL DEFAULT FALSE,
    "CriadoEm" timestamp with time zone NOT NULL,
    "AtualizadoEm" timestamp with time zone NOT NULL
);

ALTER TABLE frete_bairro_alias
ADD COLUMN IF NOT EXISTS "GeradoAutomaticamente" boolean;

UPDATE frete_bairro_alias
SET "GeradoAutomaticamente" = FALSE
WHERE "GeradoAutomaticamente" IS NULL;

ALTER TABLE frete_bairro_alias
ALTER COLUMN "GeradoAutomaticamente" SET DEFAULT FALSE;

ALTER TABLE frete_bairro_alias
ALTER COLUMN "GeradoAutomaticamente" SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'frete_bairro_alias'::regclass
          AND contype = 'p'
    ) THEN
        ALTER TABLE frete_bairro_alias
        ADD CONSTRAINT "PK_frete_bairro_alias"
        PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint c
        JOIN pg_attribute a
          ON a.attrelid = c.conrelid
         AND a.attnum = ANY(c.conkey)
        WHERE c.conrelid = 'frete_bairro_alias'::regclass
          AND c.confrelid = 'frete_bairro'::regclass
          AND c.contype = 'f'
          AND a.attname = 'FreteBairroId'
    ) THEN
        ALTER TABLE frete_bairro_alias
        ADD CONSTRAINT "FK_frete_bairro_alias_frete_bairro_FreteBairroId"
        FOREIGN KEY ("FreteBairroId")
        REFERENCES frete_bairro ("Id")
        ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_frete_bairro_alias_AliasNormalizado"
ON frete_bairro_alias ("AliasNormalizado");

CREATE INDEX IF NOT EXISTS "IX_frete_bairro_alias_Ativo"
ON frete_bairro_alias ("Ativo");

CREATE INDEX IF NOT EXISTS "IX_frete_bairro_alias_FreteBairroId"
ON frete_bairro_alias ("FreteBairroId");

INSERT INTO frete_bairro_alias (
    "Id",
    "FreteBairroId",
    "AliasNormalizado",
    "Ativo",
    "GeradoAutomaticamente",
    "CriadoEm",
    "AtualizadoEm"
)
SELECT
    md5('frete_bairro_alias:' || frete."Id"::text)::uuid,
    frete."Id",
    frete."BairroNormalizado",
    TRUE,
    TRUE,
    NOW(),
    NOW()
FROM frete_bairro frete
WHERE frete."BairroNormalizado" IS NOT NULL
  AND frete."BairroNormalizado" <> ''
  AND NOT EXISTS (
      SELECT 1
      FROM frete_bairro_alias alias
      WHERE alias."AliasNormalizado" = frete."BairroNormalizado"
  );

CREATE TABLE IF NOT EXISTS frete_cep (
    "Id" uuid NOT NULL,
    "FreteBairroId" uuid NOT NULL,
    "Cep" character varying(8) NOT NULL,
    "Ativo" boolean NOT NULL,
    "CriadoEm" timestamp with time zone NOT NULL,
    "AtualizadoEm" timestamp with time zone NOT NULL
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'frete_cep'::regclass
          AND contype = 'p'
    ) THEN
        ALTER TABLE frete_cep
        ADD CONSTRAINT "PK_frete_cep"
        PRIMARY KEY ("Id");
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint c
        JOIN pg_attribute a
          ON a.attrelid = c.conrelid
         AND a.attnum = ANY(c.conkey)
        WHERE c.conrelid = 'frete_cep'::regclass
          AND c.confrelid = 'frete_bairro'::regclass
          AND c.contype = 'f'
          AND a.attname = 'FreteBairroId'
    ) THEN
        ALTER TABLE frete_cep
        ADD CONSTRAINT "FK_frete_cep_frete_bairro_FreteBairroId"
        FOREIGN KEY ("FreteBairroId")
        REFERENCES frete_bairro ("Id")
        ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS "IX_frete_cep_Cep"
ON frete_cep ("Cep");

CREATE INDEX IF NOT EXISTS "IX_frete_cep_Ativo"
ON frete_cep ("Ativo");

CREATE INDEX IF NOT EXISTS "IX_frete_cep_FreteBairroId"
ON frete_cep ("FreteBairroId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807170300_SincronizarFreteCepAliasExistentes', '8.0.29');

COMMIT;

