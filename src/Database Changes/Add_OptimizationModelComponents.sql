-- SEQUENCE: public."OptimizationModelComponents_Id_seq"

-- DROP SEQUENCE public."OptimizationModelComponents_Id_seq";

CREATE SEQUENCE public."OptimizationModelComponents_Id_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 2147483647
    CACHE 1;

ALTER SEQUENCE public."OptimizationModelComponents_Id_seq"
    OWNER TO postgres;

-- Table: public."OptimizationModelComponents"

-- DROP TABLE public."OptimizationModelComponents";

CREATE TABLE public."OptimizationModelComponents"
(
    "Id" integer NOT NULL DEFAULT nextval('"OptimizationModelComponents_Id_seq"'::regclass),
    "OptimizationId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "IsSelected" boolean NOT NULL,
    CONSTRAINT "PK_OptimizationModelComponents" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptimizationModelComponents_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId")
        REFERENCES public."BMPType" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "FK_OptimizationModelComponents_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId")
        REFERENCES public."ModelComponent" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "FK_OptimizationModelComponents_Optimization_OptimizationId" FOREIGN KEY ("OptimizationId")
        REFERENCES public."Optimization" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."OptimizationModelComponents"
    OWNER to postgres;

-- Index: IX_OptimizationModelComponents_BMPTypeId

-- DROP INDEX public."IX_OptimizationModelComponents_BMPTypeId";

CREATE INDEX "IX_OptimizationModelComponents_BMPTypeId"
    ON public."OptimizationModelComponents" USING btree
    ("BMPTypeId")
    TABLESPACE pg_default;

-- Index: IX_OptimizationModelComponents_ModelComponentId

-- DROP INDEX public."IX_OptimizationModelComponents_ModelComponentId";

CREATE INDEX "IX_OptimizationModelComponents_ModelComponentId"
    ON public."OptimizationModelComponents" USING btree
    ("ModelComponentId")
    TABLESPACE pg_default;

-- Index: IX_OptimizationModelComponents_OptimizationId

-- DROP INDEX public."IX_OptimizationModelComponents_OptimizationId";

CREATE INDEX "IX_OptimizationModelComponents_OptimizationId"
    ON public."OptimizationModelComponents" USING btree
    ("OptimizationId")
    TABLESPACE pg_default;