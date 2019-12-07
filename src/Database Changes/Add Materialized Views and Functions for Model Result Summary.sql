-- View: public.scenariomodelresult_farm_yearly

-- DROP MATERIALIZED VIEW public.scenariomodelresult_farm_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_farm_yearly
TABLESPACE pg_default
AS
 SELECT "Farm"."Id" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
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

-- DROP MATERIALIZED VIEW public.scenariomodelresult_lsd_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_lsd_yearly
TABLESPACE pg_default
AS
 SELECT "SubArea"."LegalSubDivisionId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
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

-- DROP MATERIALIZED VIEW public.scenariomodelresult_municipality_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_municipality_yearly
TABLESPACE pg_default
AS
 SELECT "Municipality"."Id" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
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
	
-- View: public.scenariomodelresult_parcel_yearly

-- DROP MATERIALIZED VIEW public.scenariomodelresult_parcel_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_parcel_yearly
TABLESPACE pg_default
AS
 SELECT "SubArea"."ParcelId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
            ELSE sum("ScenarioModelResult"."Value") / sum("SubArea"."Area")
        END AS resultvalue
   FROM "ScenarioModelResult"
     RIGHT JOIN "SubArea" ON "SubArea"."ModelComponentId" = "ScenarioModelResult"."ModelComponentId"
     JOIN "Scenario" ON "Scenario"."Id" = "ScenarioModelResult"."ScenarioId"
  GROUP BY "SubArea"."ParcelId", "ScenarioModelResult"."Year", "ScenarioModelResult"."ScenarioModelResultTypeId", "Scenario"."ScenarioTypeId"
WITH DATA;

ALTER TABLE public.scenariomodelresult_parcel_yearly
    OWNER TO postgres;
	
	
-- View: public.scenariomodelresult_subwatershed_yearly

-- DROP MATERIALIZED VIEW public.scenariomodelresult_subwatershed_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_subwatershed_yearly
TABLESPACE pg_default
AS
 SELECT "Subbasin"."SubWatershedId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
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

-- DROP MATERIALIZED VIEW public.scenariomodelresult_watershed_yearly;

CREATE MATERIALIZED VIEW public.scenariomodelresult_watershed_yearly
TABLESPACE pg_default
AS
 SELECT "SubWatershed"."WatershedId" AS locationid,
    "Scenario"."ScenarioTypeId" AS scenariotype,
    "ScenarioModelResult"."Year" AS resultyear,
    "ScenarioModelResult"."ScenarioModelResultTypeId" AS resulttype,
        CASE
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 2 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area")
            WHEN "ScenarioModelResult"."ScenarioModelResultTypeId" <= 6 THEN sum("ScenarioModelResult"."Value" * "SubArea"."Area") / sum("SubArea"."Area") / 10::numeric
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
	
-- View: public.farm_info

-- DROP MATERIALIZED VIEW public.farm_info;

CREATE MATERIALIZED VIEW public.farm_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "Farm"."Id" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
             JOIN "Farm" ON st_intersects("SubArea"."Geometry", "Farm"."Geometry")
          GROUP BY "Farm"."Id") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "Farm"."Id" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Farm" ON st_intersects("SubArea"."Geometry", "Farm"."Geometry")
                  GROUP BY "Farm"."Id", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "Farm"."Id" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Farm" ON st_intersects("SubArea"."Geometry", "Farm"."Geometry")
                  GROUP BY "Farm"."Id", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.farm_info
    OWNER TO postgres;	
	
-- View: public.lsd_info

-- DROP MATERIALIZED VIEW public.lsd_info;

CREATE MATERIALIZED VIEW public.lsd_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "SubArea"."LegalSubDivisionId" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
          GROUP BY "SubArea"."LegalSubDivisionId") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "SubArea"."LegalSubDivisionId" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                  GROUP BY "SubArea"."LegalSubDivisionId", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "SubArea"."LegalSubDivisionId" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                  GROUP BY "SubArea"."LegalSubDivisionId", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.lsd_info
    OWNER TO postgres;
	
-- View: public.municipality_info

-- DROP MATERIALIZED VIEW public.municipality_info;

CREATE MATERIALIZED VIEW public.municipality_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "Municipality"."Id" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
             JOIN "Municipality" ON st_intersects("SubArea"."Geometry", "Municipality"."Geometry")
          GROUP BY "Municipality"."Id") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "Municipality"."Id" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Municipality" ON st_intersects("SubArea"."Geometry", "Municipality"."Geometry")
                  GROUP BY "Municipality"."Id", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "Municipality"."Id" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Municipality" ON st_intersects("SubArea"."Geometry", "Municipality"."Geometry")
                  GROUP BY "Municipality"."Id", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.municipality_info
    OWNER TO postgres;
	
-- View: public.parcel_info

-- DROP MATERIALIZED VIEW public.parcel_info;

CREATE MATERIALIZED VIEW public.parcel_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "SubArea"."ParcelId" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
          GROUP BY "SubArea"."ParcelId") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "SubArea"."ParcelId" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                  GROUP BY "SubArea"."ParcelId", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "SubArea"."ParcelId" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                  GROUP BY "SubArea"."ParcelId", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.parcel_info
    OWNER TO postgres;
	
-- View: public.subwatershed_info

-- DROP MATERIALIZED VIEW public.subwatershed_info;

CREATE MATERIALIZED VIEW public.subwatershed_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "Subbasin"."SubWatershedId" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
             JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
          GROUP BY "Subbasin"."SubWatershedId") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "Subbasin"."SubWatershedId" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
                  GROUP BY "Subbasin"."SubWatershedId", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "Subbasin"."SubWatershedId" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
                  GROUP BY "Subbasin"."SubWatershedId", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.subwatershed_info
    OWNER TO postgres;
	
-- View: public.watershed_info

-- DROP MATERIALIZED VIEW public.watershed_info;

CREATE MATERIALIZED VIEW public.watershed_info
TABLESPACE pg_default
AS
 SELECT area_elev_slope.locationid,
    area_elev_slope.area,
    area_elev_slope.elevation,
    area_elev_slope.slope,
    lus.landuse,
    sts.soiltexture
   FROM ( SELECT "SubWatershed"."WatershedId" AS locationid,
            sum("SubArea"."Area") AS area,
            sum("SubArea"."Elevation" * "SubArea"."Area") / sum("SubArea"."Area") AS elevation,
            sum("SubArea"."Slope" * "SubArea"."Area") / sum("SubArea"."Area") AS slope
           FROM "SubArea"
             JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
             JOIN "SubWatershed" ON "SubWatershed"."Id" = "Subbasin"."SubWatershedId"
          GROUP BY "SubWatershed"."WatershedId") area_elev_slope
     JOIN ( SELECT DISTINCT ON (lu.locationid) lu.locationid,
            lu.landuse
           FROM ( SELECT "SubWatershed"."WatershedId" AS locationid,
                    "SubArea"."LandUse" AS landuse,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
                     JOIN "SubWatershed" ON "SubWatershed"."Id" = "Subbasin"."SubWatershedId"
                  GROUP BY "SubWatershed"."WatershedId", "SubArea"."LandUse") lu
          ORDER BY lu.locationid, lu.area DESC) lus ON area_elev_slope.locationid = lus.locationid
     JOIN ( SELECT DISTINCT ON (st.locationid) st.locationid,
            st.soiltexture
           FROM ( SELECT "SubWatershed"."WatershedId" AS locationid,
                    "SubArea"."SoilTexture" AS soiltexture,
                    sum("SubArea"."Area") AS area
                   FROM "SubArea"
                     JOIN "Subbasin" ON "Subbasin"."Id" = "SubArea"."SubbasinId"
                     JOIN "SubWatershed" ON "SubWatershed"."Id" = "Subbasin"."SubWatershedId"
                  GROUP BY "SubWatershed"."WatershedId", "SubArea"."SoilTexture") st
          ORDER BY st.locationid, st.area DESC) sts ON area_elev_slope.locationid = sts.locationid
  ORDER BY area_elev_slope.locationid
WITH DATA;

ALTER TABLE public.watershed_info
    OWNER TO postgres;
	
	
-- FUNCTION: public.agbmptool_getuserlocations(integer, integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getuserlocations(integer, integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getuserlocations(
	userid integer,
	municipalityid integer,
	watershedid integer,
	subwatershedid integer,
	summarizationtypeid integer)
    RETURNS TABLE(locationid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- user type
   usertype integer;
BEGIN
   if summarizationtypeid = 1 then 		-- lsd   
	   RETURN QUERY   
	   select distinct public."SubArea"."LegalSubDivisionId" locationid
	   from public."SubArea"
	   join public."Subbasin" on public."Subbasin"."Id" = public."SubArea"."SubbasinId"
	   where 
	   public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid));   
   elseif summarizationtypeid = 2 then	-- parcel
	   RETURN QUERY   
	   select distinct public."SubArea"."ParcelId" locationid
	   from public."SubArea"
	   join public."Subbasin" on public."Subbasin"."Id" = public."SubArea"."SubbasinId"
	   where 
	   public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid));    
   elseif summarizationtypeid = 5 then 	-- subwatershed
	   RETURN QUERY   
	   select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid);  
   elseif summarizationtypeid = 6 then -- watershed
 	   RETURN QUERY   
	   select distinct public."SubWatershed"."WatershedId" locationid
	   from public."SubWatershed"
	   where 
	   public."SubWatershed"."Id" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid));   
   elseif summarizationtypeid = 4 then -- municipality
	   RETURN QUERY   
	   select distinct public."Municipality"."Id" locationid
	   from public."Municipality" 
	   join public."SubWatershed" on ST_Intersects(public."Municipality"."Geometry", public."SubWatershed"."Geometry")
	   where 
	   public."SubWatershed"."Id" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid));      
   elseif summarizationtypeid = 3 then -- farm
	   RETURN QUERY   
	   select distinct public."Farm"."Id" locationid
	   from public."Farm" 
	   join public."SubWatershed" on ST_Intersects(public."Farm"."Geometry", public."SubWatershed"."Geometry")
	   where 
	   public."SubWatershed"."Id" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid));      
  
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getuserlocations(integer, integer, integer, integer, integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getusersummaryresult(integer, integer, integer, integer, integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersummaryresult(integer, integer, integer, integer, integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getusersummaryresult(
	userid integer,
	municipalityid integer,
	watershedid integer,
	subwatershedid integer,
	summarizationtypeid integer,
	startyear integer,
	endyear integer,
	scenariotypeid integer)
    RETURNS TABLE(locationid integer, resulttype integer, resultvalue numeric, stdvalue numeric) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
	if summarizationtypeid = 1 then -- lsd
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_lsd_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;	
	elseif summarizationtypeid = 2 then -- parcel
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_parcel_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;	
	elseif summarizationtypeid = 3 then -- farm
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_farm_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;
	elseif summarizationtypeid = 4 then -- municipality
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_municipality_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;
	elseif summarizationtypeid = 5 then -- subwatershed
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_subwatershed_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;
	elseif summarizationtypeid = 6 then -- watershed
		return query
			select yearlyresult.locationid locationid,yearlyresult.resulttype resulttype,
			round(avg(yearlyresult.resultvalue), 24) resultvalue, 
			round(stddev(yearlyresult.resultvalue), 24) stdvalue
			from scenariomodelresult_watershed_yearly yearlyresult
			where yearlyresult.scenariotype = scenariotypeid and
			(startYear <= 0 or (startYear > 0 and yearlyresult.resultyear >= startYear)) and 	-- start year
			(endYear <= 0 or (endYear > 0 and yearlyresult.resultyear <= endYear)) and  		-- end year
			yearlyresult.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid) subarealocation)
			group by yearlyresult.locationid,yearlyresult.resulttype;
	end if;
	

	
END; $BODY$;

ALTER FUNCTION public.agbmptool_getusersummaryresult(integer, integer, integer, integer, integer, integer, integer, integer)
    OWNER TO postgres;



-- FUNCTION: public.agbmptool_getuserlocationinfo(integer, integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getuserlocationinfo(integer, integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getuserlocationinfo(
	userid integer,
	municipalityid integer,
	watershedid integer,
	subwatershedid integer,
	summarizationtypeid integer)
    RETURNS TABLE(locationid integer, area numeric, elevation numeric, slope numeric, landuse text, soiltexture text) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
	if summarizationtypeid = 1 then -- lsd
		return query
			select * from lsd_info
			where lsd_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));
	elseif summarizationtypeid = 2 then -- parcel
		return query
			select * from parcel_info
			where parcel_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));
	elseif summarizationtypeid = 3 then -- farm
		return query
			select * from farm_info
			where farm_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));
	elseif summarizationtypeid = 4 then -- municipality
		return query
			select * from municipality_info
			where municipality_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));
	elseif summarizationtypeid = 5 then -- subwatershed
		return query
			select * from subwatershed_info
			where subwatershed_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));
	elseif summarizationtypeid = 6 then -- watershed
		return query
			select * from watershed_info
			where watershed_info.locationid in (select * from agbmptool_getuserlocations(userid, municipalityid, watershedid, subwatershedid, summarizationtypeid));

	end if;
	

	
END; $BODY$;

ALTER FUNCTION public.agbmptool_getuserlocationinfo(integer, integer, integer, integer, integer)
    OWNER TO postgres;
