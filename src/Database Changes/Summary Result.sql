-- FUNCTION: public.agbmptool_getusersubwatersheds(integer)

-- DROP FUNCTION public.agbmptool_getusersubwatersheds(integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getusersubwatersheds(
	userid integer)
    RETURNS TABLE(subwatershedid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- user type
   usertype integer;
BEGIN
   -- get user type
   select Public."User"."UserTypeId" into usertype from Public."User" where Public."User"."Id" = userid;

   -- get all subwatersheds for the user id base one user type
   if usertype = 2 then			-- watershed manager
       return query
	   select Public."SubWatershed"."Id" SubWatershedId from Public."SubWatershed"
	   join public."Watershed" on public."Watershed"."Id" = Public."SubWatershed"."WatershedId"
	   join public."UserWatersheds" on public."UserWatersheds"."WatershedId" = public."Watershed"."Id"
	   join public."User" on public."User"."Id" = public."UserWatersheds"."UserId"
	   where public."User"."Id" = userid;  
   elsif usertype = 3 then		-- municipality manager
       return query
	   select Public."SubWatershed"."Id" SubWatershedId from Public."SubWatershed"
	   join public."Watershed" on public."Watershed"."Id" = Public."SubWatershed"."WatershedId"
	   join public."Municipality" on ST_Intersects(public."Municipality"."Geometry", public."Watershed"."Geometry")
	   join public."UserMunicipalities" on public."UserMunicipalities"."MunicipalityId" = Public."Municipality"."Id"
	   join public."User" on public."User"."Id" = public."UserMunicipalities"."UserId"
	   where public."User"."Id" = userid;     
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getusersubwatersheds(integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getusersubwatersheds(integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersubwatersheds(integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getusersubwatersheds(
	userid integer,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE(subwatershedid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- user type
   usertype integer;
BEGIN
   -- get user type
   if filtersubwatershedid > 0 then
   		return query
		select * from agbmptool_getusersubwatersheds(userid) userSubwatersheds where userSubwatersheds."subwatershedid" = filtersubwatershedid;
   elsif filterwatershedid > 0 then
   		return query
		select userSubwatersheds."subwatershedid" from agbmptool_getusersubwatersheds(userid) userSubwatersheds 
		join Public."SubWatershed" on Public."SubWatershed"."Id" = userSubwatersheds."subwatershedid"
		where Public."SubWatershed"."WatershedId" = filterwatershedid;
   elsif filtermunicipalityid > 0 then
   		return query
		select userSubwatersheds."subwatershedid" from agbmptool_getusersubwatersheds(userid) userSubwatersheds 
		join public."SubWatershed" on public."SubWatershed"."Id" = userSubwatersheds."subwatershedid"
		join Public."Municipality" on ST_Intersects(Public."Municipality"."Geometry", public."SubWatershed"."Geometry")
		where Public."Municipality"."Id" = filtermunicipalityid;
   else
   		return query
		select * from agbmptool_getusersubwatersheds(userid);   	
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getusersubwatersheds(integer, integer, integer, integer)
    OWNER TO postgres;
	
-- FUNCTION: public.agbmptool_getusersubareas(integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersubareas(integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getusersubareas(
	userid integer,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE(subwatershedid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   -- use subwatershed id for filter
   if filtersubwatershedid > 0 then
   		return query
		select Public."SubArea"."Id" subareaid 
		from Public."SubArea"
		join Public."Subbasin" on Public."Subbasin"."Id" = Public."SubArea"."SubbasinId"
		where 
		Public."Subbasin"."SubWatershedId" = filtersubwatershedid and 
		Public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid));		
   elsif filterwatershedid > 0 then
   		return query
		select Public."SubArea"."Id" subareaid 
		from Public."SubArea"
		join Public."Subbasin" on Public."Subbasin"."Id" = Public."SubArea"."SubbasinId"
		join Public."SubWatershed" on Public."SubWatershed"."Id" = Public."Subbasin"."SubWatershedId"
		where 
		Public."SubWatershed"."WatershedId" = filterwatershedid and 
		Public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid));
   elsif filtermunicipalityid > 0 then
   		return query
		select Public."SubArea"."Id" subareaid 
		from Public."SubArea"
		join Public."Subbasin" on Public."Subbasin"."Id" = Public."SubArea"."SubbasinId"
		join Public."Municipality" on ST_Intersects(Public."Municipality"."Geometry", public."SubArea"."Geometry")
		where 
		Public."Municipality"."Id" = filtermunicipalityid and 
		Public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid));		
   else
   		return query
		select Public."SubArea"."Id" subareaid 
		from Public."SubArea"
		join Public."Subbasin" on Public."Subbasin"."Id" = Public."SubArea"."SubbasinId"
		where 
		Public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid));   	
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getusersubareas(integer, integer, integer, integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getsubarearesult(integer, integer, integer, integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getsubarearesult(integer, integer, integer, integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getsubarearesult(
	userid integer,
	scenariotypeid integer,
	municipalityid integer,
	watershedid integer,
	subwatershedid integer,
	startyear integer,
	endyear integer)
    RETURNS TABLE(subareaid integer, modelresulttypeid integer, resultyear integer, resultvalue numeric) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- user type
   usertype integer;
BEGIN
   RETURN QUERY    -- get filter geometry based on filter
   select public."SubArea"."Id" SubAreaId, public."ScenarioModelResultType"."Id" ModelResultTypeId,
   public."ScenarioModelResult"."Year" ResultYear, public."ScenarioModelResult"."Value" ResultValue
   from public."ScenarioModelResult"
   join public."Scenario" on public."ScenarioModelResult"."ScenarioId" = public."Scenario"."Id"
   join public."ModelComponent" on public."ScenarioModelResult"."ModelComponentId" = public."ModelComponent"."Id"
   join public."SubArea" on public."SubArea"."Id" = public."ModelComponent"."Id"
   join public."ScenarioModelResultType" on public."ScenarioModelResultType"."Id" = public."ScenarioModelResult"."ScenarioModelResultTypeId"
   join public."Subbasin" on public."Subbasin"."Id" = public."SubArea"."SubbasinId"
   where 
   public."Scenario"."ScenarioTypeId" = scenarioTypeId and 	-- scenario type id = 1 limit to selected baseline scenario type
   public."ModelComponent"."ModelComponentTypeId" = 1 and	-- model component type id = 1 limit to just subareas
   (startYear <= 0 or (startYear > 0 and public."ScenarioModelResult"."Year" >= startYear)) and 	-- start year
   (endYear <= 0 or (endYear > 0 and public."ScenarioModelResult"."Year" <= endYear)) and 		-- end year
   public."Subbasin"."SubWatershedId" in (select * from agbmptool_getusersubwatersheds(userid,municipalityid,watershedid,subwatershedid))
   order by SubAreaId,ModelResultTypeId,public."ScenarioModelResult"."Year";
END; $BODY$;

ALTER FUNCTION public.agbmptool_getsubarearesult(integer, integer, integer, integer, integer, integer, integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getuserstartendyear(integer, integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getuserstartendyear(integer, integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getuserstartendyear(
	userid integer,
	basescenariotypeid integer,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE(startyear integer, endyear integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   RETURN QUERY   
   select min(public."ScenarioModelResult"."Year") startyear, max(public."ScenarioModelResult"."Year") endyear
   from public."ScenarioModelResult"
   join public."Scenario" on public."ScenarioModelResult"."ScenarioId" = public."Scenario"."Id"
   join public."Watershed" on public."Watershed"."Id" = public."Scenario"."WatershedId"
   join public."SubWatershed" on public."SubWatershed"."WatershedId" = public."Watershed"."Id"
   where 
   public."Scenario"."ScenarioTypeId" = basescenariotypeid and 
   public."SubWatershed"."Id" in (select * from agbmptool_getusersubwatersheds(userid,filtermunicipalityid,filterwatershedid,filtersubwatershedid));
 END; $BODY$;

ALTER FUNCTION public.agbmptool_getuserstartendyear(integer, integer, integer, integer, integer)
    OWNER TO postgres;

