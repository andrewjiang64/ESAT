-- View: public.scenariomodelresult_parcel_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_parcel_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_parcel_yearly
TABLESPACE pg_default
AS
 SELECT "SubArea"."ParcelId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "SubArea"."ParcelId", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_parcel_yearly
    OWNER TO postgres;
	
-- View: public.scenariomodelresult_farm_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_farm_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_farm_yearly
TABLESPACE pg_default
AS
 SELECT "Farm"."Id" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Farm" ON st_intersects("Farm"."Geometry", "SubArea"."Geometry")
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "Farm"."Id", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_farm_yearly
    OWNER TO postgres;
	
-- View: public.scenariomodelresult_lsd_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_lsd_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_lsd_yearly
TABLESPACE pg_default
AS
 SELECT "SubArea"."LegalSubDivisionId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "SubArea"."LegalSubDivisionId", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_lsd_yearly
    OWNER TO postgres;
	
	
-- View: public.scenariomodelresult_municipality_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_municipality_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_municipality_yearly
TABLESPACE pg_default
AS
 SELECT "Municipality"."Id" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Municipality" ON st_intersects("Municipality"."Geometry", "SubArea"."Geometry")
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "Municipality"."Id", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_municipality_yearly
    OWNER TO postgres;
	
	
-- View: public.scenariomodelresult_subwatershed_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_subwatershed_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_subwatershed_yearly
TABLESPACE pg_default
AS
 SELECT "Subbasin"."SubWatershedId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "Subbasin"."SubWatershedId", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_subwatershed_yearly
    OWNER TO postgres;

-- View: public.scenariomodelresult_watershed_yearly

DROP MATERIALIZED VIEW public.scenariomodelresult_watershed_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_watershed_yearly
TABLESPACE pg_default
AS
 SELECT "SubWatershed"."WatershedId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 or "ScenarioModelResult"."ScenarioModelResultTypeId" >= 15 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
     JOIN "SubWatershed" ON "SubWatershed"."Id" = "Subbasin"."SubWatershedId"
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "SubWatershed"."WatershedId", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_watershed_yearly
    OWNER TO postgres;	