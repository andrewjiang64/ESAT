-- View: public.subwatershed_startendyear

-- DROP MATERIALIZED VIEW public.subwatershed_startendyear;

CREATE MATERIALIZED VIEW public.subwatershed_startendyear
TABLESPACE pg_default
AS
 SELECT "Subbasin"."SubWatershedId",
    "Scenario"."ScenarioTypeId",
    min("ScenarioModelResult"."Year") AS "StartYear",
    max("ScenarioModelResult"."Year") AS "EndYear"
   FROM "ScenarioModelResult"
     JOIN "Scenario" ON "ScenarioModelResult"."ScenarioId" = "Scenario"."Id"
     JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
  GROUP BY "Subbasin"."SubWatershedId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.subwatershed_startendyear
    OWNER TO postgres;
	
-- FUNCTION: public.agbmptool_getuserstartendyear(integer, integer, boolean, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getuserstartendyear(integer, integer, boolean, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getuserstartendyear(
	userid integer,
	basescenariotypeid integer,
	fornewproject boolean,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE("StartYear" integer, "EndYear" integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   RETURN QUERY   
   select 
   case fornewproject 
   		when 'true' then max(swyr."StartYear")
		else min(swyr."StartYear")
   end,
   case fornewproject 
   		when 'true' then min(swyr."EndYear")
		else max(swyr."EndYear")
   end
   from subwatershed_startendyear swyr
   where 
   swyr."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid,filtermunicipalityid,filterwatershedid,filtersubwatershedid));
 END; $BODY$;

ALTER FUNCTION public.agbmptool_getuserstartendyear(integer, integer, boolean, integer, integer, integer)
    OWNER TO postgres;
