-- FUNCTION: public.agbmptool_getprojectdefaultbmplocations_aggregate(integer)

-- DROP FUNCTION public.agbmptool_getprojectdefaultbmplocations_aggregate(integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectdefaultbmplocations_aggregate(
	projectid integer)
    RETURNS TABLE(bmptypeid integer, bmptypename text, modelcomponenttypeid integer, modelcomponentid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- project spatial unit type id
   projectspatialunittypeid integer;
   
   -- number of subarea
   numsubarea integer;
BEGIN
   drop table if exists originaltable;
   drop table if exists finaltable;
   
   create temp table originaltable
   (
	   bmptypeid integer, 
	   bmptypename text, 
	   modelcomponenttypeid integer, 
	   modelcomponentid integer
   );
   
   create temp table finaltable
   (
	   bmptypeid integer, 
	   bmptypename text, 
	   modelcomponenttypeid integer, 
	   modelcomponentid integer
   );
   
   -- get all original model components
   insert into originaltable select * from agbmptool_getprojectdefaultbmplocations(projectid);
   
   -- get all the components that are not subarea
   insert into finaltable
   select * from originaltable where originaltable.modelcomponenttypeid > 1;
   
   -- aggregate subarea to parcel or lsd
   select count(originaltable.bmptypeid) into numsubarea from originaltable where originaltable.modelcomponenttypeid = 1;
   
   if numsubarea > 0 then
       select Public."Project"."ProjectSpatialUnitTypeId" into projectspatialunittypeid
	   from Public."Project"
	   where Public."Project"."Id" = projectId;
	   
	   if projectspatialunittypeid = 1 then	-- lsd
	   	    insert into finaltable
	   		select distinct originaltable.bmptypeid, originaltable.bmptypename, -1, Public."SubArea"."LegalSubDivisionId" modelcomponentid
			from originaltable
			join Public."SubArea" on Public."SubArea"."ModelComponentId" = originaltable.modelcomponentid;
	   else	-- parcel
	   	    insert into finaltable
	   		select distinct originaltable.bmptypeid, originaltable.bmptypename, -2, Public."SubArea"."ParcelId" modelcomponentid
			from originaltable
			join Public."SubArea" on Public."SubArea"."ModelComponentId" = originaltable.modelcomponentid;	   
	   end if;
   end if;
   
   return query
   select * from finaltable
   order by finaltable.bmptypeid,finaltable.modelcomponentid;
   
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectdefaultbmplocations_aggregate(integer)
    OWNER TO postgres;
