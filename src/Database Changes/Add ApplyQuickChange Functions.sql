-- FUNCTION: public.agbmptool_getusersubwatersheds(integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersubwatersheds(integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectsubwatersheds(
	projectid integer,
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
   -- get subwatershed based on the filter
   if filtersubwatershedid > 0 then
   		return query
		select Public."SubWatershed"."Id" from Public."SubWatershed" 
		where Public."SubWatershed"."Id" = filtersubwatershedid and 
		Public."SubWatershed"."WatershedId" in (select * from agbmptool_getprojectwatersheds(projectid));
   elsif filterwatershedid > 0 then
   		return query
		select Public."SubWatershed"."Id" from Public."SubWatershed" 
		where Public."SubWatershed"."WatershedId" = filterwatershedid and 
		Public."SubWatershed"."WatershedId" in (select * from agbmptool_getprojectwatersheds(projectid));
   elsif filtermunicipalityid > 0 then
   		return query
		select Public."SubWatershed"."Id" from Public."SubWatershed" 
		join Public."Municipality" on ST_Intersects(Public."Municipality"."Geometry", public."SubWatershed"."Geometry")
		where Public."Municipality"."Id" = filtermunicipalityid and
		Public."SubWatershed"."WatershedId" in (select * from agbmptool_getprojectwatersheds(projectid));
   else
   		return query
		select Public."SubWatershed"."Id" from Public."SubWatershed" 
		where 
		Public."SubWatershed"."WatershedId" in (select * from agbmptool_getprojectwatersheds(projectid));
   end if;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectsubwatersheds(integer, integer, integer, integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getusersubwatersheds(integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersubwatersheds(integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_getprojectmodelcomponents(
	projectid integer,
	modelcomponenttypeid integer,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE(modelcomponentid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
BEGIN
   case modelcomponenttypeid 
   		when 3 then 
			return query
			select Public."IsolatedWetland"."ModelComponentId" from Public."IsolatedWetland"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."IsolatedWetland"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 4 then 
			return query
			select Public."RiparianWetland"."ModelComponentId" from Public."RiparianWetland"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."RiparianWetland"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));		
   		when 5 then 
			return query
			select Public."Lake"."ModelComponentId" from Public."Lake"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."Lake"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 6 then 
			return query
			select Public."VegetativeFilterStrip"."ModelComponentId" from Public."VegetativeFilterStrip"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."VegetativeFilterStrip"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));	
   		when 7 then 
			return query
			select Public."RiparianBuffer"."ModelComponentId" from Public."RiparianBuffer"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."RiparianBuffer"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 8 then 
			return query
			select Public."GrassedWaterway"."ModelComponentId" from Public."GrassedWaterway"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."GrassedWaterway"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 9 then 
			return query
			select Public."FlowDiversion"."ModelComponentId" from Public."FlowDiversion"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."FlowDiversion"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 10 then 
			return query
			select Public."Reservoir"."ModelComponentId" from Public."Reservoir"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."Reservoir"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
		when 11 then 
			return query
			select Public."SmallDam"."ModelComponentId" from Public."SmallDam"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."SmallDam"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 12 then 
			return query
			select Public."Wascob"."ModelComponentId" from Public."Wascob"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."Wascob"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 13 then 
			return query
			select Public."Dugout"."ModelComponentId" from Public."Dugout"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."Dugout"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 14 then 
			return query
			select Public."CatchBasin"."ModelComponentId" from Public."CatchBasin"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."CatchBasin"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 15 then 
			return query
			select Public."Feedlot"."ModelComponentId" from Public."Feedlot"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."Feedlot"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 16 then 
			return query
			select Public."ManureStorage"."ModelComponentId" from Public."ManureStorage"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."ManureStorage"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 17 then 
			return query
			select Public."RockChute"."ModelComponentId" from Public."RockChute"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."RockChute"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 18 then 
			return query
			select Public."PointSource"."ModelComponentId" from Public."PointSource"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."PointSource"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
   		when 19 then 
			return query
			select Public."ClosedDrain"."ModelComponentId" from Public."ClosedDrain"
			join Public."SubWatershed" on ST_Intersects(Public."SubWatershed"."Geometry", Public."ClosedDrain"."Geometry")
			where Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
    end case;
END; $BODY$;

ALTER FUNCTION public.agbmptool_getprojectmodelcomponents(integer, integer, integer, integer, integer)
    OWNER TO postgres;

-- FUNCTION: public.agbmptool_getusersubwatersheds(integer, integer, integer, integer)

-- DROP FUNCTION public.agbmptool_getusersubwatersheds(integer, integer, integer, integer);

CREATE OR REPLACE FUNCTION public.agbmptool_applyquickchanges(
	projectid integer,
	bmptypeid integer,
	isoptimization boolean,
	isselected boolean,
	filtermunicipalityid integer,
	filterwatershedid integer,
	filtersubwatershedid integer)
    RETURNS TABLE(locationid integer) 
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE 
    ROWS 1000
AS $BODY$
DECLARE
	modelcomponenttypeid integer;
	projectspatialunittypeid integer;
BEGIN
   -- get modelcomponenttypeid for bmp type id
   select Public."BMPType"."ModelComponentTypeId" into modelcomponenttypeid
   from Public."BMPType" where Public."BMPType"."Id" = bmptypeid;
   
   -- create temp table for 
   drop table if exists temp_locations;   
   create temp table temp_locations (tableid integer, locationid integer);
   
   if modelcomponenttypeid = 1 then --subarea     
		   -- get project spatial unit type id
		   select Public."Project"."ProjectSpatialUnitTypeId" into projectspatialunittypeid
		   from Public."Project" where Public."Project"."Id" = projectid;
		   
		   if projectspatialunittypeid = 1 then -- lsd
		      if isoptimization then -- optimization
			  		-- get table id and location id
			  		insert into temp_locations
					select Public."OptimizationLegalSubDivisions"."Id" tableid, Public."OptimizationLegalSubDivisions"."LegalSubDivisionId" locationid  
					from Public."OptimizationLegalSubDivisions"
					join Public."Optimization" on Public."Optimization"."Id" = Public."OptimizationLegalSubDivisions"."OptimizationId"
					join Public."LegalSubDivision" on Public."LegalSubDivision"."Id" = Public."OptimizationLegalSubDivisions"."LegalSubDivisionId"
					join Public."SubWatershed" on ST_Intersects(Public."LegalSubDivision"."Geometry", Public."SubWatershed"."Geometry")
					where 
					Public."OptimizationLegalSubDivisions"."BMPTypeId" = bmptypeid and 
					Public."Optimization"."ProjectId" = projectId and
					Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
					
					-- update status
					update Public."OptimizationLegalSubDivisions" set "IsSelected" = isSelected
					where "Id" in (select tableid from temp_locations);
			  else -- solution
			  		-- get table id and location id
			  		insert into temp_locations
					select Public."SolutionLegalSubDivisions"."Id" tableid, Public."SolutionLegalSubDivisions"."LegalSubDivisionId" locationid  
					from Public."SolutionLegalSubDivisions"
					join Public."Solution" on Public."Solution"."Id" = Public."SolutionLegalSubDivisions"."SolutionId"
					join Public."LegalSubDivision" on Public."LegalSubDivision"."Id" = Public."SolutionLegalSubDivisions"."LegalSubDivisionId"
					join Public."SubWatershed" on ST_Intersects(Public."LegalSubDivision"."Geometry", Public."SubWatershed"."Geometry")
					where 
					Public."SolutionLegalSubDivisions"."BMPTypeId" = bmptypeid and 
					Public."Solution"."ProjectId" = projectId and
					Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
					
					-- update status
					update Public."SolutionLegalSubDivisions" set "IsSelected" = isSelected
					where "Id" in (select tableid from temp_locations);			  
			  end if;
		   else -- parcel
		      if isoptimization then -- optimization
			  		-- get table id and location id
			  		insert into temp_locations
					select Public."OptimizationParcels"."Id" tableid, Public."OptimizationParcels"."ParcelId" locationid  
					from Public."OptimizationParcels"
					join Public."Optimization" on Public."Optimization"."Id" = Public."OptimizationParcels"."OptimizationId"
					join Public."Parcel" on Public."Parcel"."Id" = Public."OptimizationParcels"."ParcelId"
					join Public."SubWatershed" on ST_Intersects(Public."Parcel"."Geometry", Public."SubWatershed"."Geometry")
					where 
					Public."OptimizationParcels"."BMPTypeId" = bmptypeid and 
					Public."Optimization"."ProjectId" = projectId and
					Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
					
					-- update status
					update Public."OptimizationParcels" set "IsSelected" = isSelected
					where "Id" in (select tableid from temp_locations);
			  else -- solution
			  		-- get table id and location id
			  		insert into temp_locations
					select Public."SolutionParcels"."Id" tableid, Public."SolutionParcels"."ParcelId" locationid  
					from Public."SolutionParcels"
					join Public."Solution" on Public."Solution"."Id" = Public."SolutionParcels"."SolutionId"
					join Public."Parcel" on Public."Parcel"."Id" = Public."SolutionParcels"."ParcelId"
					join Public."SubWatershed" on ST_Intersects(Public."Parcel"."Geometry", Public."SubWatershed"."Geometry")
					where 
					Public."SolutionParcels"."BMPTypeId" = bmptypeid and 
					Public."Solution"."ProjectId" = projectId and
					Public."SubWatershed"."Id" in (select * from agbmptool_getprojectsubwatersheds(projectid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
					
					-- update status
					update Public."SolutionParcels" set "IsSelected" = isSelected
					where "Id" in (select tableid from temp_locations);			  
			  end if;		   
		   end if;
   else	-- structure bmps
   	   if isoptimization then -- optimization
			insert into temp_locations
			select Public."OptimizationModelComponents"."Id" tableid, Public."OptimizationModelComponents"."ModelComponentId" locationid  
			from Public."OptimizationModelComponents"
			join Public."Optimization" on Public."Optimization"."Id" = Public."OptimizationModelComponents"."OptimizationId"
			where 
			Public."OptimizationModelComponents"."BMPTypeId" = bmptypeid and 
			Public."Optimization"."ProjectId" = projectId and
			Public."OptimizationModelComponents"."ModelComponentId" in (select * from agbmptool_getprojectmodelcomponents(projectid,modelcomponenttypeid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));
	   
	   		-- update status
			update Public."OptimizationModelComponents" set "IsSelected" = isSelected
			where "Id" in (select tableid from temp_locations);	
	   else -- solution
			insert into temp_locations
			select Public."SultionModelComponents"."Id" tableid, Public."SultionModelComponents"."ModelComponentId" locationid  
			from Public."SultionModelComponents"
			join Public."Solution" on Public."Solution"."Id" = Public."SultionModelComponents"."SolutionId"
			where 
			Public."SultionModelComponents"."BMPTypeId" = bmptypeid and 
			Public."Solution"."ProjectId" = projectId and
			Public."SultionModelComponents"."ModelComponentId" in (select * from agbmptool_getprojectmodelcomponents(projectid,modelcomponenttypeid,filtermunicipalityid, filterwatershedid,filtersubwatershedid));

	   		-- update status
			update Public."SultionModelComponents" set "IsSelected" = isSelected
			where "Id" in (select tableid from temp_locations);	
	   end if;
   end if;
   
   -- return location ids
   return query
   select distinct temp_locations.locationid from temp_locations order by temp_locations.locationid;
END; $BODY$;

ALTER FUNCTION public.agbmptool_applyquickchanges(integer, integer, boolean, boolean, integer, integer, integer)
    OWNER TO postgres;
