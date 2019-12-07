-- FUNCTION: public.agbmptool_getprojectwatersheds(integer)

-- DROP FUNCTION public.agbmptool_getprojectwatersheds(integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectwatersheds(
	projectid integer)
    RETURNS TABLE(watershedid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- number of project watersheds
   numprojectwatersheds integer;
   -- user id, only used when use project municipalities table
   userid integer;
BEGIN
   -- check project watersheds table first
   -- if it doesn't has any records, we will check the project municipalities table   
   select count(Public."ProjectWatersheds"."Id") into numprojectwatersheds from Public."ProjectWatersheds" 
   where Public."ProjectWatersheds"."ProjectId" = projectid;

   -- if number of project watershed is larger than 0, we will read it and return
   if numprojectwatersheds > 0 then			
       return query
	   select distinct Public."ProjectWatersheds"."WatershedId" watershedid from Public."ProjectWatersheds" 
	   where Public."ProjectWatersheds"."ProjectId" = projectid;
   -- we will try to read project municipalities right now.
   else
   	   -- get user id first from project
   	   select Public."Project"."UserId" into userid from Public."Project"  	
   	   where Public."Project"."Id" = projectid;
   
	   -- we will also user user watersheds relationship to limit the watersheds
       return query
	   select distinct public."Watershed"."Id" watershedid from Public."ProjectMunicipalities"
	   join public."Municipality" on Public."ProjectMunicipalities"."MunicipalityId" = public."Municipality"."Id"
	   join public."Watershed" on ST_Intersects(public."Watershed"."Geometry", Public."Municipality"."Geometry")
	   join public."SubWatershed" on public."SubWatershed"."WatershedId" = public."Watershed"."Id"
	   where Public."ProjectMunicipalities"."ProjectId" = projectid 
	   and public."SubWatershed"."Id" in (select * from agbmptool_getusersubwatersheds(userid));    
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectwatersheds(integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getprojectdefaultbmplocations(integer)

-- DROP FUNCTION public.agbmptool_getprojectdefaultbmplocations(integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectdefaultbmplocations(
	projectid integer)
    RETURNS TABLE(bmptypeid integer, bmptypename text, modelcomponenttypeid integer, modelcomponentid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
declare
   -- number of project watersheds
   numprojectwatersheds integer;
   -- user id, only used when use project municipalities table
   userid integer;
BEGIN
   return query
   select distinct Public."BMPType"."Id" bmptypeid, Public."BMPType"."Name" bmptypename, Public."ModelComponent"."ModelComponentTypeId" modelcomponenttypeid, 
   Public."ModelComponent"."Id" modelcomponentid
   from Public."Scenario"
   join Public."Project" on Public."Project"."ScenarioTypeId" = Public."Scenario"."ScenarioTypeId"
   join Public."UnitScenario" on Public."UnitScenario"."ScenarioId" = Public."Scenario"."Id"
   join Public."BMPCombinationType" on Public."UnitScenario"."BMPCombinationId" = Public."BMPCombinationType"."Id"
   join Public."BMPCombinationBMPTypes" on Public."BMPCombinationBMPTypes"."BMPCombinationTypeId" = Public."BMPCombinationType"."Id"
   join Public."BMPType" on Public."BMPType"."Id" = Public."BMPCombinationBMPTypes"."BMPTypeId"
   join Public."ModelComponent" on Public."ModelComponent"."Id" = Public."UnitScenario"."ModelComponentId"
   where Public."Project"."Id" = projectId and Public."Scenario"."WatershedId" in (select * from agbmptool_getprojectwatersheds(projectid))
   order by bmptypeid;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectdefaultbmplocations(integer)
    OWNER TO postgres;


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
			join Public."SubArea" on Public."SubArea"."Id" = originaltable.modelcomponentid;
	   else	-- parcel
	   	    insert into finaltable
	   		select distinct originaltable.bmptypeid, originaltable.bmptypename, -2, Public."SubArea"."ParcelId" modelcomponentid
			from originaltable
			join Public."SubArea" on Public."SubArea"."Id" = originaltable.modelcomponentid;	   
	   end if;
   end if;
   
   return query
   select * from finaltable
   order by finaltable.bmptypeid,finaltable.modelcomponentid;
   
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectdefaultbmplocations_aggregate(integer)
    OWNER TO postgres;


-- PROCEDURE: public.agbmptool_setprojectdefaultbmplocations(integer)

-- DROP PROCEDURE public.agbmptool_setprojectdefaultbmplocations(integer);

CREATE OR REPLACE PROCEDURE public.agbmptool_setprojectdefaultbmplocations(
	projectid integer)
LANGUAGE 'plpgsql'

AS $BODY$
declare
   numsolution integer;
   numopt integer;
   
   solutionid integer;
   numsolutionparcels integer;
   numsolutionlsds integer;
   numsolutioncomponents integer;

   optimizationId integer;
   numoptparcels integer;
   numoptlsds integer;
   numoptcomponents integer;   
BEGIN
   -- check if the project already has default bmp locations
   select Count(Public."Solution"."Id") into numsolution from Public."Solution" where Public."Solution"."ProjectId" = projectid;
   
   if numsolution = 0 then
     insert into Public."Solution" ("ProjectId", "FromOptimization") values (projectid, false);
   end if;
   select Public."Solution"."Id" into solutionid from Public."Solution" where Public."Solution"."ProjectId" = projectid;
   
   select count(Public."SolutionParcels"."Id") into numsolutionparcels 
   from Public."SolutionParcels" 
   where Public."SolutionParcels"."SolutionId" = solutionid;
   
   select count(Public."SolutionLegalSubDivisions"."Id") into numsolutionlsds 
   from Public."SolutionLegalSubDivisions" 
   where Public."SolutionLegalSubDivisions"."SolutionId" = solutionid;
   
   select count(Public."SolutionModelComponents"."Id") into numsolutioncomponents 
   from Public."SolutionModelComponents" 
   where Public."SolutionModelComponents"."SolutionId" = solutionid;
   
   drop table if exists defaultlocations;
       create temp table defaultlocations
	  (
		   bmptypeid integer, 
		   bmptypename text, 
		   modelcomponenttypeid integer, 
		   modelcomponentid integer
	  );
   insert into defaultlocations select * from agbmptool_getprojectdefaultbmplocations_aggregate(projectid);
   
   -- only insert when there is no records in these tables
   if numsolutionparcels + numsolutionlsds + numsolutioncomponents = 0 then
      insert into Public."SolutionLegalSubDivisions" ("SolutionId","BMPTypeId","LegalSubDivisionId", "IsSelected")
	  select solutionid, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , false 
	  from defaultlocations where defaultlocations.modelcomponenttypeid = -1;
   
   	  insert into Public."SolutionParcels" ("SolutionId","BMPTypeId","ParcelId", "IsSelected")
	  select solutionid, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , false 
	  from defaultlocations where defaultlocations.modelcomponenttypeid = -2;
	  
	  insert into Public."SolutionModelComponents" ("SolutionId","BMPTypeId","ModelComponentId", "IsSelected")
	  select solutionid, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , false 
	  from defaultlocations where defaultlocations.modelcomponenttypeid > 0;
   
   end if;
   
   -- optimization
   
   -- check if the project already has default bmp locations
   select count(Public."Optimization"."Id") into numopt from Public."Optimization" where Public."Optimization"."ProjectId" = projectid;

   if numopt = 0 then
     insert into Public."Optimization" ("ProjectId", "OptimizationTypeId") values (projectid, 1);
   end if;
   select Public."Optimization"."Id" into optimizationId from Public."Optimization" where Public."Optimization"."ProjectId" = projectid;
   
   select count(Public."OptimizationParcels"."Id") into numoptparcels 
   from Public."OptimizationParcels" 
   where Public."OptimizationParcels"."OptimizationId" = optimizationId;
   
   select count(Public."OptimizationLegalSubDivisions"."Id") into numoptlsds 
   from Public."OptimizationLegalSubDivisions" 
   where Public."OptimizationLegalSubDivisions"."OptimizationId" = optimizationId;
   
   select count(Public."OptimizationModelComponents"."Id") into numoptcomponents 
   from Public."OptimizationModelComponents" 
   where Public."OptimizationModelComponents"."OptimizationId" = optimizationId;
   
   -- only insert when there is no records in these tables
   if numoptparcels + numoptlsds + numoptcomponents = 0 then  
      insert into Public."OptimizationLegalSubDivisions" ("OptimizationId","BMPTypeId","LegalSubDivisionId", "IsSelected")
	  select optimizationId, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , true 
	  from defaultlocations where defaultlocations.modelcomponenttypeid = -1;
   
   	  insert into Public."OptimizationParcels" ("OptimizationId","BMPTypeId","ParcelId", "IsSelected")
	  select optimizationId, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , true 
	  from defaultlocations where defaultlocations.modelcomponenttypeid = -2;
	  
	  insert into Public."OptimizationModelComponents" ("OptimizationId","BMPTypeId","ModelComponentId", "IsSelected")
	  select optimizationId, defaultlocations.bmptypeid, defaultlocations.modelcomponentid , true 
	  from defaultlocations where defaultlocations.modelcomponenttypeid > 0;
   
   end if;
  
   drop table if exists defaultlocations;
  
END; $BODY$;
