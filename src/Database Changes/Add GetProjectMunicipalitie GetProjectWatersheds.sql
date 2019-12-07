-- FUNCTION: public.agbmptool_getprojectmunicipalities(integer)

-- DROP FUNCTION public.agbmptool_getprojectmunicipalities(integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectmunicipalities(
	projectid integer)
    RETURNS TABLE("Id" integer, "Name" text) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   return query
   select Public."Municipality"."Id", Public."Municipality"."Name"   from Public."Municipality"
   join Public."Watershed" on ST_Intersects(Public."Watershed"."Geometry", Public."Municipality"."Geometry")
   where Public."Watershed"."Id" in (select * from agbmptool_getprojectwatersheds(projectid));
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectmunicipalities(integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getprojectwatersheds(integer, integer)

-- DROP FUNCTION public.agbmptool_getprojectwatersheds(integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectwatersheds(
	projectid integer,
	municipalityid integer)
    RETURNS TABLE("Id" integer, "Name" text) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   if municipalityid <= 0 then
   		return query
   		select Public."Watershed"."Id",Public."Watershed"."Name" from Public."Watershed"
		where Public."Watershed"."Id" in (select * from agbmptool_getprojectwatersheds(projectid));
   else
   		return query
   		select Public."Watershed"."Id",Public."Watershed"."Name" from Public."Watershed"
		join Public."Municipality" on ST_Intersects(Public."Watershed"."Geometry", Public."Municipality"."Geometry")
		where 
		Public."Municipality"."Id" = municipalityid and
		Public."Watershed"."Id" in (select * from agbmptool_getprojectwatersheds(projectid));   		
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectwatersheds(integer, integer)
    OWNER TO postgres;
