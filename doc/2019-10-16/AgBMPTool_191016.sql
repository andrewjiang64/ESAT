PGDMP                     	    w        	   AgBMPTool    11.4    11.5 �   �           0    0    ENCODING    ENCODING        SET client_encoding = 'UTF8';
                       false            �           0    0 
   STDSTRINGS 
   STDSTRINGS     (   SET standard_conforming_strings = 'on';
                       false            �           0    0 
   SEARCHPATH 
   SEARCHPATH     8   SELECT pg_catalog.set_config('search_path', '', false);
                       false            �           1262    272977 	   AgBMPTool    DATABASE     �   CREATE DATABASE "AgBMPTool" WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_Canada.936' LC_CTYPE = 'English_Canada.936';
    DROP DATABASE "AgBMPTool";
             postgres    false                        3079    272983    postgis 	   EXTENSION     ;   CREATE EXTENSION IF NOT EXISTS postgis WITH SCHEMA public;
    DROP EXTENSION postgis;
                  false            �           0    0    EXTENSION postgis    COMMENT     g   COMMENT ON EXTENSION postgis IS 'PostGIS geometry, geography, and raster spatial types and functions';
                       false    2            /           1255    276055 Y   agbmptool_getsubarearesult(integer, integer, integer, integer, integer, integer, integer)    FUNCTION     S  CREATE FUNCTION public.agbmptool_getsubarearesult(userid integer, scenariotypeid integer, municipalityid integer, watershedid integer, subwatershedid integer, startyear integer, endyear integer) RETURNS TABLE(subareaid integer, modelresulttypeid integer, resultyear integer, resultvalue numeric)
    LANGUAGE plpgsql
    AS $$
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
END; $$;
 �   DROP FUNCTION public.agbmptool_getsubarearesult(userid integer, scenariotypeid integer, municipalityid integer, watershedid integer, subwatershedid integer, startyear integer, endyear integer);
       public       postgres    false            0           1255    276056 J   agbmptool_getuserstartendyear(integer, integer, integer, integer, integer)    FUNCTION     �  CREATE FUNCTION public.agbmptool_getuserstartendyear(userid integer, basescenariotypeid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer) RETURNS TABLE(startyear integer, endyear integer)
    LANGUAGE plpgsql
    AS $$
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
 END; $$;
 �   DROP FUNCTION public.agbmptool_getuserstartendyear(userid integer, basescenariotypeid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer);
       public       postgres    false            .           1255    276054 =   agbmptool_getusersubareas(integer, integer, integer, integer)    FUNCTION     a  CREATE FUNCTION public.agbmptool_getusersubareas(userid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer) RETURNS TABLE(subwatershedid integer)
    LANGUAGE plpgsql
    AS $$
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
END; $$;
 �   DROP FUNCTION public.agbmptool_getusersubareas(userid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer);
       public       postgres    false            ,           1255    276052 '   agbmptool_getusersubwatersheds(integer)    FUNCTION     �  CREATE FUNCTION public.agbmptool_getusersubwatersheds(userid integer) RETURNS TABLE(subwatershedid integer)
    LANGUAGE plpgsql
    AS $$
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
END; $$;
 E   DROP FUNCTION public.agbmptool_getusersubwatersheds(userid integer);
       public       postgres    false            -           1255    276053 B   agbmptool_getusersubwatersheds(integer, integer, integer, integer)    FUNCTION     F  CREATE FUNCTION public.agbmptool_getusersubwatersheds(userid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer) RETURNS TABLE(subwatershedid integer)
    LANGUAGE plpgsql
    AS $$
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
END; $$;
 �   DROP FUNCTION public.agbmptool_getusersubwatersheds(userid integer, filtermunicipalityid integer, filterwatershedid integer, filtersubwatershedid integer);
       public       postgres    false            �            1259    274563 
   AnimalType    TABLE     �   CREATE TABLE public."AnimalType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL
);
     DROP TABLE public."AnimalType";
       public         postgres    false            �            1259    274561    AnimalType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."AnimalType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."AnimalType_Id_seq";
       public       postgres    false    214            �           0    0    AnimalType_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE public."AnimalType_Id_seq" OWNED BY public."AnimalType"."Id";
            public       postgres    false    213                       1259    274915    BMPCombinationBMPTypes    TABLE     �   CREATE TABLE public."BMPCombinationBMPTypes" (
    "Id" integer NOT NULL,
    "BMPCombinationTypeId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL
);
 ,   DROP TABLE public."BMPCombinationBMPTypes";
       public         postgres    false                       1259    274913    BMPCombinationBMPTypes_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."BMPCombinationBMPTypes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 6   DROP SEQUENCE public."BMPCombinationBMPTypes_Id_seq";
       public       postgres    false    268            �           0    0    BMPCombinationBMPTypes_Id_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE public."BMPCombinationBMPTypes_Id_seq" OWNED BY public."BMPCombinationBMPTypes"."Id";
            public       postgres    false    267            �            1259    274574    BMPCombinationType    TABLE     �   CREATE TABLE public."BMPCombinationType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL
);
 (   DROP TABLE public."BMPCombinationType";
       public         postgres    false            �            1259    274572    BMPCombinationType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."BMPCombinationType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."BMPCombinationType_Id_seq";
       public       postgres    false    216            �           0    0    BMPCombinationType_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public."BMPCombinationType_Id_seq" OWNED BY public."BMPCombinationType"."Id";
            public       postgres    false    215            �            1259    274585    BMPEffectivenessLocationType    TABLE     �   CREATE TABLE public."BMPEffectivenessLocationType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 2   DROP TABLE public."BMPEffectivenessLocationType";
       public         postgres    false            �            1259    274583 #   BMPEffectivenessLocationType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."BMPEffectivenessLocationType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 <   DROP SEQUENCE public."BMPEffectivenessLocationType_Id_seq";
       public       postgres    false    218            �           0    0 #   BMPEffectivenessLocationType_Id_seq    SEQUENCE OWNED BY     q   ALTER SEQUENCE public."BMPEffectivenessLocationType_Id_seq" OWNED BY public."BMPEffectivenessLocationType"."Id";
            public       postgres    false    217                       1259    274933    BMPEffectivenessType    TABLE     �  CREATE TABLE public."BMPEffectivenessType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "ScenarioModelResultTypeId" integer,
    "UnitTypeId" integer NOT NULL,
    "ScenarioModelResultVariableTypeId" integer,
    "DefaultWeight" integer NOT NULL,
    "DefaultConstraintTypeId" integer,
    "DefaultConstraint" numeric,
    "BMPEffectivenessLocationTypeId" integer NOT NULL
);
 *   DROP TABLE public."BMPEffectivenessType";
       public         postgres    false                       1259    274931    BMPEffectivenessType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."BMPEffectivenessType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 4   DROP SEQUENCE public."BMPEffectivenessType_Id_seq";
       public       postgres    false    270            �           0    0    BMPEffectivenessType_Id_seq    SEQUENCE OWNED BY     a   ALTER SEQUENCE public."BMPEffectivenessType_Id_seq" OWNED BY public."BMPEffectivenessType"."Id";
            public       postgres    false    269                        1259    274799    BMPType    TABLE     �   CREATE TABLE public."BMPType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "ModelComponentTypeId" integer NOT NULL
);
    DROP TABLE public."BMPType";
       public         postgres    false            �            1259    274797    BMPType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."BMPType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public."BMPType_Id_seq";
       public       postgres    false    256            �           0    0    BMPType_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public."BMPType_Id_seq" OWNED BY public."BMPType"."Id";
            public       postgres    false    255            0           1259    275303 
   CatchBasin    TABLE     A  CREATE TABLE public."CatchBasin" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,0) NOT NULL
);
     DROP TABLE public."CatchBasin";
       public         postgres    false    2    2    2    2    2    2    2    2            /           1259    275301    CatchBasin_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."CatchBasin_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 *   DROP SEQUENCE public."CatchBasin_Id_seq";
       public       postgres    false    304            �           0    0    CatchBasin_Id_seq    SEQUENCE OWNED BY     M   ALTER SEQUENCE public."CatchBasin_Id_seq" OWNED BY public."CatchBasin"."Id";
            public       postgres    false    303            2           1259    275329    ClosedDrain    TABLE     �   CREATE TABLE public."ClosedDrain" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPoint)
);
 !   DROP TABLE public."ClosedDrain";
       public         postgres    false    2    2    2    2    2    2    2    2            1           1259    275327    ClosedDrain_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ClosedDrain_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 +   DROP SEQUENCE public."ClosedDrain_Id_seq";
       public       postgres    false    306            �           0    0    ClosedDrain_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public."ClosedDrain_Id_seq" OWNED BY public."ClosedDrain"."Id";
            public       postgres    false    305            �            1259    274596    Country    TABLE     �   CREATE TABLE public."Country" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL
);
    DROP TABLE public."Country";
       public         postgres    false            �            1259    274594    Country_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Country_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public."Country_Id_seq";
       public       postgres    false    220            �           0    0    Country_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public."Country_Id_seq" OWNED BY public."Country"."Id";
            public       postgres    false    219            4           1259    275355    Dugout    TABLE     b  CREATE TABLE public."Dugout" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,0) NOT NULL,
    "AnimalTypeId" integer NOT NULL
);
    DROP TABLE public."Dugout";
       public         postgres    false    2    2    2    2    2    2    2    2            3           1259    275353    Dugout_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Dugout_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public."Dugout_Id_seq";
       public       postgres    false    308            �           0    0    Dugout_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public."Dugout_Id_seq" OWNED BY public."Dugout"."Id";
            public       postgres    false    307            �            1259    274607    Farm    TABLE     y   CREATE TABLE public."Farm" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "Name" text
);
    DROP TABLE public."Farm";
       public         postgres    false    2    2    2    2    2    2    2    2            �            1259    274605    Farm_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Farm_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 $   DROP SEQUENCE public."Farm_Id_seq";
       public       postgres    false    222            �           0    0    Farm_Id_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE public."Farm_Id_seq" OWNED BY public."Farm"."Id";
            public       postgres    false    221            6           1259    275386    Feedlot    TABLE     �  CREATE TABLE public."Feedlot" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "AnimalTypeId" integer NOT NULL,
    "AnimalNumber" integer NOT NULL,
    "AnimalAdultRatio" numeric(3,3) NOT NULL,
    "Area" numeric(5,4) NOT NULL
);
    DROP TABLE public."Feedlot";
       public         postgres    false    2    2    2    2    2    2    2    2            5           1259    275384    Feedlot_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Feedlot_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public."Feedlot_Id_seq";
       public       postgres    false    310            �           0    0    Feedlot_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public."Feedlot_Id_seq" OWNED BY public."Feedlot"."Id";
            public       postgres    false    309            8           1259    275417    FlowDiversion    TABLE        CREATE TABLE public."FlowDiversion" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPoint),
    "Length" numeric(6,0) NOT NULL
);
 #   DROP TABLE public."FlowDiversion";
       public         postgres    false    2    2    2    2    2    2    2    2            7           1259    275415    FlowDiversion_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."FlowDiversion_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."FlowDiversion_Id_seq";
       public       postgres    false    312            �           0    0    FlowDiversion_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE public."FlowDiversion_Id_seq" OWNED BY public."FlowDiversion"."Id";
            public       postgres    false    311            �            1259    274618    GeometryLayerStyle    TABLE     �   CREATE TABLE public."GeometryLayerStyle" (
    "Id" integer NOT NULL,
    layername text,
    type text,
    style text,
    color text,
    simplelinewidth text,
    outlinecolor text,
    outlinewidth text
);
 (   DROP TABLE public."GeometryLayerStyle";
       public         postgres    false            �            1259    274616    GeometryLayerStyle_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."GeometryLayerStyle_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."GeometryLayerStyle_Id_seq";
       public       postgres    false    224            �           0    0    GeometryLayerStyle_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public."GeometryLayerStyle_Id_seq" OWNED BY public."GeometryLayerStyle"."Id";
            public       postgres    false    223            :           1259    275443    GrassedWaterway    TABLE     G  CREATE TABLE public."GrassedWaterway" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Width" numeric(5,0) NOT NULL,
    "Length" numeric(5,0) NOT NULL
);
 %   DROP TABLE public."GrassedWaterway";
       public         postgres    false    2    2    2    2    2    2    2    2            9           1259    275441    GrassedWaterway_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."GrassedWaterway_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."GrassedWaterway_Id_seq";
       public       postgres    false    314            �           0    0    GrassedWaterway_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE public."GrassedWaterway_Id_seq" OWNED BY public."GrassedWaterway"."Id";
            public       postgres    false    313            �            1259    274629    Investor    TABLE     �   CREATE TABLE public."Investor" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL
);
    DROP TABLE public."Investor";
       public         postgres    false            �            1259    274627    Investor_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Investor_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."Investor_Id_seq";
       public       postgres    false    226            �           0    0    Investor_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."Investor_Id_seq" OWNED BY public."Investor"."Id";
            public       postgres    false    225            <           1259    275469    IsolatedWetland    TABLE     F  CREATE TABLE public."IsolatedWetland" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,0) NOT NULL
);
 %   DROP TABLE public."IsolatedWetland";
       public         postgres    false    2    2    2    2    2    2    2    2            ;           1259    275467    IsolatedWetland_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."IsolatedWetland_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."IsolatedWetland_Id_seq";
       public       postgres    false    316            �           0    0    IsolatedWetland_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE public."IsolatedWetland_Id_seq" OWNED BY public."IsolatedWetland"."Id";
            public       postgres    false    315            >           1259    275495    Lake    TABLE     ;  CREATE TABLE public."Lake" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,4) NOT NULL
);
    DROP TABLE public."Lake";
       public         postgres    false    2    2    2    2    2    2    2    2            =           1259    275493    Lake_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Lake_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 $   DROP SEQUENCE public."Lake_Id_seq";
       public       postgres    false    318            �           0    0    Lake_Id_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE public."Lake_Id_seq" OWNED BY public."Lake"."Id";
            public       postgres    false    317            �            1259    274640    LegalSubDivision    TABLE     E  CREATE TABLE public."LegalSubDivision" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "Meridian" smallint NOT NULL,
    "Range" smallint NOT NULL,
    "Township" smallint NOT NULL,
    "Section" smallint NOT NULL,
    "Quarter" text,
    "LSD" smallint NOT NULL,
    "FullDescription" text
);
 &   DROP TABLE public."LegalSubDivision";
       public         postgres    false    2    2    2    2    2    2    2    2            �            1259    274638    LegalSubDivision_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."LegalSubDivision_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."LegalSubDivision_Id_seq";
       public       postgres    false    228            �           0    0    LegalSubDivision_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE public."LegalSubDivision_Id_seq" OWNED BY public."LegalSubDivision"."Id";
            public       postgres    false    227            @           1259    275521    ManureStorage    TABLE     B  CREATE TABLE public."ManureStorage" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPoint),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,0) NOT NULL
);
 #   DROP TABLE public."ManureStorage";
       public         postgres    false    2    2    2    2    2    2    2    2            ?           1259    275519    ManureStorage_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ManureStorage_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 -   DROP SEQUENCE public."ManureStorage_Id_seq";
       public       postgres    false    320            �           0    0    ManureStorage_Id_seq    SEQUENCE OWNED BY     S   ALTER SEQUENCE public."ManureStorage_Id_seq" OWNED BY public."ManureStorage"."Id";
            public       postgres    false    319                       1259    274836    ModelComponent    TABLE     �   CREATE TABLE public."ModelComponent" (
    "Id" integer NOT NULL,
    "ModelId" integer NOT NULL,
    "Name" text,
    "Description" text,
    "WatershedId" integer NOT NULL,
    "ModelComponentTypeId" integer NOT NULL
);
 $   DROP TABLE public."ModelComponent";
       public         postgres    false                       1259    274969    ModelComponentBMPTypes    TABLE     �   CREATE TABLE public."ModelComponentBMPTypes" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL
);
 ,   DROP TABLE public."ModelComponentBMPTypes";
       public         postgres    false                       1259    274967    ModelComponentBMPTypes_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ModelComponentBMPTypes_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 6   DROP SEQUENCE public."ModelComponentBMPTypes_Id_seq";
       public       postgres    false    272            �           0    0    ModelComponentBMPTypes_Id_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE public."ModelComponentBMPTypes_Id_seq" OWNED BY public."ModelComponentBMPTypes"."Id";
            public       postgres    false    271            �            1259    274651    ModelComponentType    TABLE     �   CREATE TABLE public."ModelComponentType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsStructure" boolean NOT NULL
);
 (   DROP TABLE public."ModelComponentType";
       public         postgres    false            �            1259    274649    ModelComponentType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ModelComponentType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."ModelComponentType_Id_seq";
       public       postgres    false    230            �           0    0    ModelComponentType_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public."ModelComponentType_Id_seq" OWNED BY public."ModelComponentType"."Id";
            public       postgres    false    229                       1259    274834    ModelComponent_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ModelComponent_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."ModelComponent_Id_seq";
       public       postgres    false    260            �           0    0    ModelComponent_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE public."ModelComponent_Id_seq" OWNED BY public."ModelComponent"."Id";
            public       postgres    false    259            �            1259    274662    Municipality    TABLE     �   CREATE TABLE public."Municipality" (
    "Id" integer NOT NULL,
    "Name" text,
    "Region" text,
    "Geometry" public.geometry(MultiPolygon)
);
 "   DROP TABLE public."Municipality";
       public         postgres    false    2    2    2    2    2    2    2    2            �            1259    274660    Municipality_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Municipality_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."Municipality_Id_seq";
       public       postgres    false    232            �           0    0    Municipality_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public."Municipality_Id_seq" OWNED BY public."Municipality"."Id";
            public       postgres    false    231            (           1259    275233    Optimization    TABLE     �   CREATE TABLE public."Optimization" (
    "Id" integer NOT NULL,
    "ProjectId" integer NOT NULL,
    "OptimizationTypeId" integer NOT NULL,
    "BudgetTarget" numeric
);
 "   DROP TABLE public."Optimization";
       public         postgres    false            �            1259    274673    OptimizationConstraintValueType    TABLE     �   CREATE TABLE public."OptimizationConstraintValueType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 5   DROP TABLE public."OptimizationConstraintValueType";
       public         postgres    false            �            1259    274671 &   OptimizationConstraintValueType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationConstraintValueType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ?   DROP SEQUENCE public."OptimizationConstraintValueType_Id_seq";
       public       postgres    false    234            �           0    0 &   OptimizationConstraintValueType_Id_seq    SEQUENCE OWNED BY     w   ALTER SEQUENCE public."OptimizationConstraintValueType_Id_seq" OWNED BY public."OptimizationConstraintValueType"."Id";
            public       postgres    false    233            R           1259    275755    OptimizationConstraints    TABLE        CREATE TABLE public."OptimizationConstraints" (
    "Id" integer NOT NULL,
    "OptimizationId" integer NOT NULL,
    "BMPEffectivenessTypeId" integer NOT NULL,
    "OptimizationConstraintValueTypeId" integer NOT NULL,
    "Constraint" numeric NOT NULL
);
 -   DROP TABLE public."OptimizationConstraints";
       public         postgres    false            Q           1259    275753    OptimizationConstraints_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationConstraints_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."OptimizationConstraints_Id_seq";
       public       postgres    false    338            �           0    0    OptimizationConstraints_Id_seq    SEQUENCE OWNED BY     g   ALTER SEQUENCE public."OptimizationConstraints_Id_seq" OWNED BY public."OptimizationConstraints"."Id";
            public       postgres    false    337            T           1259    275781    OptimizationLegalSubDivisions    TABLE     �   CREATE TABLE public."OptimizationLegalSubDivisions" (
    "Id" integer NOT NULL,
    "OptimizationId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "LegalSubDivisionId" integer NOT NULL,
    "IsSelected" boolean NOT NULL
);
 3   DROP TABLE public."OptimizationLegalSubDivisions";
       public         postgres    false            S           1259    275779 $   OptimizationLegalSubDivisions_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationLegalSubDivisions_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 =   DROP SEQUENCE public."OptimizationLegalSubDivisions_Id_seq";
       public       postgres    false    340                        0    0 $   OptimizationLegalSubDivisions_Id_seq    SEQUENCE OWNED BY     s   ALTER SEQUENCE public."OptimizationLegalSubDivisions_Id_seq" OWNED BY public."OptimizationLegalSubDivisions"."Id";
            public       postgres    false    339            V           1259    275804    OptimizationParcels    TABLE     �   CREATE TABLE public."OptimizationParcels" (
    "Id" integer NOT NULL,
    "OptimizationId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "ParcelId" integer NOT NULL,
    "IsSelected" boolean NOT NULL
);
 )   DROP TABLE public."OptimizationParcels";
       public         postgres    false            U           1259    275802    OptimizationParcels_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationParcels_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."OptimizationParcels_Id_seq";
       public       postgres    false    342                       0    0    OptimizationParcels_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE public."OptimizationParcels_Id_seq" OWNED BY public."OptimizationParcels"."Id";
            public       postgres    false    341            �            1259    274684    OptimizationType    TABLE     �   CREATE TABLE public."OptimizationType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 &   DROP TABLE public."OptimizationType";
       public         postgres    false            �            1259    274682    OptimizationType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 0   DROP SEQUENCE public."OptimizationType_Id_seq";
       public       postgres    false    236                       0    0    OptimizationType_Id_seq    SEQUENCE OWNED BY     Y   ALTER SEQUENCE public."OptimizationType_Id_seq" OWNED BY public."OptimizationType"."Id";
            public       postgres    false    235            X           1259    275827    OptimizationWeights    TABLE     �   CREATE TABLE public."OptimizationWeights" (
    "Id" integer NOT NULL,
    "OptimizationId" integer NOT NULL,
    "BMPEffectivenessTypeId" integer NOT NULL,
    "Weight" integer NOT NULL
);
 )   DROP TABLE public."OptimizationWeights";
       public         postgres    false            W           1259    275825    OptimizationWeights_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."OptimizationWeights_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."OptimizationWeights_Id_seq";
       public       postgres    false    344                       0    0    OptimizationWeights_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE public."OptimizationWeights_Id_seq" OWNED BY public."OptimizationWeights"."Id";
            public       postgres    false    343            '           1259    275231    Optimization_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Optimization_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."Optimization_Id_seq";
       public       postgres    false    296                       0    0    Optimization_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public."Optimization_Id_seq" OWNED BY public."Optimization"."Id";
            public       postgres    false    295            �            1259    274695    Parcel    TABLE       CREATE TABLE public."Parcel" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "Meridian" smallint NOT NULL,
    "Range" smallint NOT NULL,
    "Township" smallint NOT NULL,
    "Section" smallint NOT NULL,
    "Quarter" text,
    "FullDescription" text
);
    DROP TABLE public."Parcel";
       public         postgres    false    2    2    2    2    2    2    2    2            �            1259    274693    Parcel_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Parcel_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public."Parcel_Id_seq";
       public       postgres    false    238                       0    0    Parcel_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public."Parcel_Id_seq" OWNED BY public."Parcel"."Id";
            public       postgres    false    237            B           1259    275547    PointSource    TABLE     �   CREATE TABLE public."PointSource" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPoint)
);
 !   DROP TABLE public."PointSource";
       public         postgres    false    2    2    2    2    2    2    2    2            A           1259    275545    PointSource_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."PointSource_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 +   DROP SEQUENCE public."PointSource_Id_seq";
       public       postgres    false    322                       0    0    PointSource_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public."PointSource_Id_seq" OWNED BY public."PointSource"."Id";
            public       postgres    false    321                       1259    275080    Project    TABLE     �  CREATE TABLE public."Project" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "Created" timestamp without time zone NOT NULL,
    "Modified" timestamp without time zone NOT NULL,
    "Active" boolean NOT NULL,
    "StartYear" integer NOT NULL,
    "EndYear" integer NOT NULL,
    "UserId" integer NOT NULL,
    "ScenarioTypeId" integer NOT NULL,
    "ProjectSpatialUnitTypeId" integer NOT NULL
);
    DROP TABLE public."Project";
       public         postgres    false            *           1259    275254    ProjectMunicipalities    TABLE     �   CREATE TABLE public."ProjectMunicipalities" (
    "Id" integer NOT NULL,
    "ProjectId" integer NOT NULL,
    "MunicipalityId" integer NOT NULL
);
 +   DROP TABLE public."ProjectMunicipalities";
       public         postgres    false            )           1259    275252    ProjectMunicipalities_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ProjectMunicipalities_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."ProjectMunicipalities_Id_seq";
       public       postgres    false    298                       0    0    ProjectMunicipalities_Id_seq    SEQUENCE OWNED BY     c   ALTER SEQUENCE public."ProjectMunicipalities_Id_seq" OWNED BY public."ProjectMunicipalities"."Id";
            public       postgres    false    297            �            1259    274706    ProjectSpatialUnitType    TABLE     �   CREATE TABLE public."ProjectSpatialUnitType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 ,   DROP TABLE public."ProjectSpatialUnitType";
       public         postgres    false            �            1259    274704    ProjectSpatialUnitType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ProjectSpatialUnitType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 6   DROP SEQUENCE public."ProjectSpatialUnitType_Id_seq";
       public       postgres    false    240                       0    0    ProjectSpatialUnitType_Id_seq    SEQUENCE OWNED BY     e   ALTER SEQUENCE public."ProjectSpatialUnitType_Id_seq" OWNED BY public."ProjectSpatialUnitType"."Id";
            public       postgres    false    239            ,           1259    275272    ProjectWatersheds    TABLE     �   CREATE TABLE public."ProjectWatersheds" (
    "Id" integer NOT NULL,
    "ProjectId" integer NOT NULL,
    "WatershedId" integer NOT NULL
);
 '   DROP TABLE public."ProjectWatersheds";
       public         postgres    false            +           1259    275270    ProjectWatersheds_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ProjectWatersheds_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 1   DROP SEQUENCE public."ProjectWatersheds_Id_seq";
       public       postgres    false    300            	           0    0    ProjectWatersheds_Id_seq    SEQUENCE OWNED BY     [   ALTER SEQUENCE public."ProjectWatersheds_Id_seq" OWNED BY public."ProjectWatersheds"."Id";
            public       postgres    false    299                       1259    275078    Project_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Project_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public."Project_Id_seq";
       public       postgres    false    282            
           0    0    Project_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public."Project_Id_seq" OWNED BY public."Project"."Id";
            public       postgres    false    281            �            1259    274783    Province    TABLE     �   CREATE TABLE public."Province" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "Code" character varying(2),
    "CountryId" integer NOT NULL
);
    DROP TABLE public."Province";
       public         postgres    false            �            1259    274781    Province_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Province_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."Province_Id_seq";
       public       postgres    false    254                       0    0    Province_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."Province_Id_seq" OWNED BY public."Province"."Id";
            public       postgres    false    253            $           1259    275181    Reach    TABLE     �   CREATE TABLE public."Reach" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubbasinId" integer NOT NULL,
    "Geometry" public.geometry(MultiLineString)
);
    DROP TABLE public."Reach";
       public         postgres    false    2    2    2    2    2    2    2    2            #           1259    275179    Reach_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Reach_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 %   DROP SEQUENCE public."Reach_Id_seq";
       public       postgres    false    292                       0    0    Reach_Id_seq    SEQUENCE OWNED BY     C   ALTER SEQUENCE public."Reach_Id_seq" OWNED BY public."Reach"."Id";
            public       postgres    false    291            D           1259    275573 	   Reservoir    TABLE     B  CREATE TABLE public."Reservoir" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(12,4) NOT NULL,
    "Volume" numeric(12,0) NOT NULL
);
    DROP TABLE public."Reservoir";
       public         postgres    false    2    2    2    2    2    2    2    2            C           1259    275571    Reservoir_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Reservoir_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."Reservoir_Id_seq";
       public       postgres    false    324                       0    0    Reservoir_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public."Reservoir_Id_seq" OWNED BY public."Reservoir"."Id";
            public       postgres    false    323            F           1259    275599    RiparianBuffer    TABLE     �  CREATE TABLE public."RiparianBuffer" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Width" numeric(5,0) NOT NULL,
    "Length" numeric(5,0) NOT NULL,
    "Area" numeric(12,4) NOT NULL,
    "AreaRatio" numeric(12,0) NOT NULL,
    "DrainageArea" public.geometry(Polygon)
);
 $   DROP TABLE public."RiparianBuffer";
       public         postgres    false    2    2    2    2    2    2    2    2    2    2    2    2    2    2    2    2            E           1259    275597    RiparianBuffer_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."RiparianBuffer_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."RiparianBuffer_Id_seq";
       public       postgres    false    326                       0    0    RiparianBuffer_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE public."RiparianBuffer_Id_seq" OWNED BY public."RiparianBuffer"."Id";
            public       postgres    false    325            H           1259    275625    RiparianWetland    TABLE     F  CREATE TABLE public."RiparianWetland" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(5,4) NOT NULL,
    "Volume" numeric(6,4) NOT NULL
);
 %   DROP TABLE public."RiparianWetland";
       public         postgres    false    2    2    2    2    2    2    2    2            G           1259    275623    RiparianWetland_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."RiparianWetland_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."RiparianWetland_Id_seq";
       public       postgres    false    328                       0    0    RiparianWetland_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE public."RiparianWetland_Id_seq" OWNED BY public."RiparianWetland"."Id";
            public       postgres    false    327            J           1259    275651 	   RockChute    TABLE     �   CREATE TABLE public."RockChute" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPoint)
);
    DROP TABLE public."RockChute";
       public         postgres    false    2    2    2    2    2    2    2    2            I           1259    275649    RockChute_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."RockChute_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."RockChute_Id_seq";
       public       postgres    false    330                       0    0    RockChute_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public."RockChute_Id_seq" OWNED BY public."RockChute"."Id";
            public       postgres    false    329                       1259    274857    Scenario    TABLE     �   CREATE TABLE public."Scenario" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "WatershedId" integer NOT NULL,
    "ScenarioTypeId" integer NOT NULL
);
    DROP TABLE public."Scenario";
       public         postgres    false                       1259    275015    ScenarioModelResult    TABLE       CREATE TABLE public."ScenarioModelResult" (
    "Id" integer NOT NULL,
    "ScenarioId" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "ScenarioModelResultTypeId" integer NOT NULL,
    "Year" integer NOT NULL,
    "Value" numeric NOT NULL
);
 )   DROP TABLE public."ScenarioModelResult";
       public         postgres    false                       1259    274815    ScenarioModelResultType    TABLE     "  CREATE TABLE public."ScenarioModelResultType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "UnitTypeId" integer NOT NULL,
    "ModelComponentTypeId" integer NOT NULL,
    "ScenarioModelResultVariableTypeId" integer NOT NULL
);
 -   DROP TABLE public."ScenarioModelResultType";
       public         postgres    false                       1259    274813    ScenarioModelResultType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ScenarioModelResultType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."ScenarioModelResultType_Id_seq";
       public       postgres    false    258                       0    0    ScenarioModelResultType_Id_seq    SEQUENCE OWNED BY     g   ALTER SEQUENCE public."ScenarioModelResultType_Id_seq" OWNED BY public."ScenarioModelResultType"."Id";
            public       postgres    false    257            �            1259    274717    ScenarioModelResultVariableType    TABLE     �   CREATE TABLE public."ScenarioModelResultVariableType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 5   DROP TABLE public."ScenarioModelResultVariableType";
       public         postgres    false            �            1259    274715 &   ScenarioModelResultVariableType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ScenarioModelResultVariableType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ?   DROP SEQUENCE public."ScenarioModelResultVariableType_Id_seq";
       public       postgres    false    242                       0    0 &   ScenarioModelResultVariableType_Id_seq    SEQUENCE OWNED BY     w   ALTER SEQUENCE public."ScenarioModelResultVariableType_Id_seq" OWNED BY public."ScenarioModelResultVariableType"."Id";
            public       postgres    false    241                       1259    275013    ScenarioModelResult_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ScenarioModelResult_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 3   DROP SEQUENCE public."ScenarioModelResult_Id_seq";
       public       postgres    false    276                       0    0    ScenarioModelResult_Id_seq    SEQUENCE OWNED BY     _   ALTER SEQUENCE public."ScenarioModelResult_Id_seq" OWNED BY public."ScenarioModelResult"."Id";
            public       postgres    false    275            �            1259    274728    ScenarioResultSummarizationType    TABLE     �   CREATE TABLE public."ScenarioResultSummarizationType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsDefault" boolean NOT NULL
);
 5   DROP TABLE public."ScenarioResultSummarizationType";
       public         postgres    false            �            1259    274726 &   ScenarioResultSummarizationType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ScenarioResultSummarizationType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ?   DROP SEQUENCE public."ScenarioResultSummarizationType_Id_seq";
       public       postgres    false    244                       0    0 &   ScenarioResultSummarizationType_Id_seq    SEQUENCE OWNED BY     w   ALTER SEQUENCE public."ScenarioResultSummarizationType_Id_seq" OWNED BY public."ScenarioResultSummarizationType"."Id";
            public       postgres    false    243            �            1259    274739    ScenarioType    TABLE     �   CREATE TABLE public."ScenarioType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "IsBaseLine" boolean NOT NULL,
    "IsDefault" boolean NOT NULL
);
 "   DROP TABLE public."ScenarioType";
       public         postgres    false            �            1259    274737    ScenarioType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."ScenarioType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."ScenarioType_Id_seq";
       public       postgres    false    246                       0    0    ScenarioType_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public."ScenarioType_Id_seq" OWNED BY public."ScenarioType"."Id";
            public       postgres    false    245                       1259    274855    Scenario_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Scenario_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."Scenario_Id_seq";
       public       postgres    false    262                       0    0    Scenario_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."Scenario_Id_seq" OWNED BY public."Scenario"."Id";
            public       postgres    false    261            L           1259    275677    SmallDam    TABLE     A  CREATE TABLE public."SmallDam" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(12,4) NOT NULL,
    "Volume" numeric(12,0) NOT NULL
);
    DROP TABLE public."SmallDam";
       public         postgres    false    2    2    2    2    2    2    2    2            K           1259    275675    SmallDam_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SmallDam_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."SmallDam_Id_seq";
       public       postgres    false    332                       0    0    SmallDam_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."SmallDam_Id_seq" OWNED BY public."SmallDam"."Id";
            public       postgres    false    331            .           1259    275290    Solution    TABLE     �   CREATE TABLE public."Solution" (
    "Id" integer NOT NULL,
    "ProjectId" integer NOT NULL,
    "FromOptimization" boolean NOT NULL
);
    DROP TABLE public."Solution";
       public         postgres    false            Z           1259    275845    SolutionLegalSubDivisions    TABLE     �   CREATE TABLE public."SolutionLegalSubDivisions" (
    "Id" integer NOT NULL,
    "SolutionId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "LegalSubDivisionId" integer NOT NULL,
    "IsSelected" boolean NOT NULL
);
 /   DROP TABLE public."SolutionLegalSubDivisions";
       public         postgres    false            Y           1259    275843     SolutionLegalSubDivisions_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SolutionLegalSubDivisions_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."SolutionLegalSubDivisions_Id_seq";
       public       postgres    false    346                       0    0     SolutionLegalSubDivisions_Id_seq    SEQUENCE OWNED BY     k   ALTER SEQUENCE public."SolutionLegalSubDivisions_Id_seq" OWNED BY public."SolutionLegalSubDivisions"."Id";
            public       postgres    false    345            \           1259    275868    SolutionModelComponents    TABLE     �   CREATE TABLE public."SolutionModelComponents" (
    "Id" integer NOT NULL,
    "SolutionId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "IsSelected" boolean NOT NULL
);
 -   DROP TABLE public."SolutionModelComponents";
       public         postgres    false            [           1259    275866    SolutionModelComponents_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SolutionModelComponents_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 7   DROP SEQUENCE public."SolutionModelComponents_Id_seq";
       public       postgres    false    348                       0    0    SolutionModelComponents_Id_seq    SEQUENCE OWNED BY     g   ALTER SEQUENCE public."SolutionModelComponents_Id_seq" OWNED BY public."SolutionModelComponents"."Id";
            public       postgres    false    347            ^           1259    275891    SolutionParcels    TABLE     �   CREATE TABLE public."SolutionParcels" (
    "Id" integer NOT NULL,
    "SolutionId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "ParcelId" integer NOT NULL,
    "IsSelected" boolean NOT NULL
);
 %   DROP TABLE public."SolutionParcels";
       public         postgres    false            ]           1259    275889    SolutionParcels_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SolutionParcels_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 /   DROP SEQUENCE public."SolutionParcels_Id_seq";
       public       postgres    false    350                       0    0    SolutionParcels_Id_seq    SEQUENCE OWNED BY     W   ALTER SEQUENCE public."SolutionParcels_Id_seq" OWNED BY public."SolutionParcels"."Id";
            public       postgres    false    349            -           1259    275288    Solution_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Solution_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."Solution_Id_seq";
       public       postgres    false    302                       0    0    Solution_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."Solution_Id_seq" OWNED BY public."Solution"."Id";
            public       postgres    false    301            &           1259    275202    SubArea    TABLE     �  CREATE TABLE public."SubArea" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "ModelComponentId" integer NOT NULL,
    "SubbasinId" integer NOT NULL,
    "LegalSubDivisionId" integer NOT NULL,
    "ParcelId" integer NOT NULL,
    "Area" numeric NOT NULL,
    "Elevation" numeric NOT NULL,
    "Slope" numeric NOT NULL,
    "LandUse" text,
    "SoilTexture" text
);
    DROP TABLE public."SubArea";
       public         postgres    false    2    2    2    2    2    2    2    2            %           1259    275200    SubArea_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SubArea_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 '   DROP SEQUENCE public."SubArea_Id_seq";
       public       postgres    false    294                       0    0    SubArea_Id_seq    SEQUENCE OWNED BY     G   ALTER SEQUENCE public."SubArea_Id_seq" OWNED BY public."SubArea"."Id";
            public       postgres    false    293                       1259    274878    SubWatershed    TABLE       CREATE TABLE public."SubWatershed" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "Name" text,
    "Alias" text,
    "Description" text,
    "Area" numeric NOT NULL,
    "Modified" timestamp with time zone NOT NULL,
    "WatershedId" integer NOT NULL
);
 "   DROP TABLE public."SubWatershed";
       public         postgres    false    2    2    2    2    2    2    2    2                       1259    274876    SubWatershed_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."SubWatershed_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."SubWatershed_Id_seq";
       public       postgres    false    264                       0    0    SubWatershed_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public."SubWatershed_Id_seq" OWNED BY public."SubWatershed"."Id";
            public       postgres    false    263                       1259    275064    Subbasin    TABLE     �   CREATE TABLE public."Subbasin" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "SubWatershedId" integer NOT NULL
);
    DROP TABLE public."Subbasin";
       public         postgres    false    2    2    2    2    2    2    2    2                       1259    275062    Subbasin_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Subbasin_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."Subbasin_Id_seq";
       public       postgres    false    280                       0    0    Subbasin_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."Subbasin_Id_seq" OWNED BY public."Subbasin"."Id";
            public       postgres    false    279                       1259    275041    UnitScenario    TABLE     �   CREATE TABLE public."UnitScenario" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "ScenarioId" integer NOT NULL,
    "BMPCombinationId" integer NOT NULL
);
 "   DROP TABLE public."UnitScenario";
       public         postgres    false            "           1259    275160    UnitScenarioEffectiveness    TABLE     �   CREATE TABLE public."UnitScenarioEffectiveness" (
    "Id" integer NOT NULL,
    "UnitScenarioId" integer NOT NULL,
    "BMPEffectivenessTypeId" integer NOT NULL,
    "Year" integer NOT NULL,
    "Value" numeric NOT NULL
);
 /   DROP TABLE public."UnitScenarioEffectiveness";
       public         postgres    false            !           1259    275158     UnitScenarioEffectiveness_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UnitScenarioEffectiveness_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 9   DROP SEQUENCE public."UnitScenarioEffectiveness_Id_seq";
       public       postgres    false    290                       0    0     UnitScenarioEffectiveness_Id_seq    SEQUENCE OWNED BY     k   ALTER SEQUENCE public."UnitScenarioEffectiveness_Id_seq" OWNED BY public."UnitScenarioEffectiveness"."Id";
            public       postgres    false    289                       1259    275039    UnitScenario_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UnitScenario_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 ,   DROP SEQUENCE public."UnitScenario_Id_seq";
       public       postgres    false    278                        0    0    UnitScenario_Id_seq    SEQUENCE OWNED BY     Q   ALTER SEQUENCE public."UnitScenario_Id_seq" OWNED BY public."UnitScenario"."Id";
            public       postgres    false    277            �            1259    274750    UnitType    TABLE     �   CREATE TABLE public."UnitType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL,
    "UnitSymbol" text
);
    DROP TABLE public."UnitType";
       public         postgres    false            �            1259    274748    UnitType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UnitType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."UnitType_Id_seq";
       public       postgres    false    248            !           0    0    UnitType_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."UnitType_Id_seq" OWNED BY public."UnitType"."Id";
            public       postgres    false    247            
           1259    274894    User    TABLE     �  CREATE TABLE public."User" (
    "Id" integer NOT NULL,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "FirstName" text,
    "LastName" text,
    "Active" boolean NOT NULL,
    "Address1" text,
    "Address2" text,
    "PostalCode" text,
    "Municipality" text,
    "City" text,
    "ProvinceId" integer NOT NULL,
    "DateOfBirth" timestamp without time zone,
    "TaxRollNumber" text,
    "DriverLicense" text,
    "LastFourDigitOfSIN" character varying(4),
    "Organization" text,
    "LastModified" timestamp without time zone NOT NULL,
    "UserTypeId" integer NOT NULL
);
    DROP TABLE public."User";
       public         postgres    false                       1259    275106    UserMunicipalities    TABLE     �   CREATE TABLE public."UserMunicipalities" (
    "Id" integer NOT NULL,
    "UserId" integer NOT NULL,
    "MunicipalityId" integer NOT NULL
);
 (   DROP TABLE public."UserMunicipalities";
       public         postgres    false                       1259    275104    UserMunicipalities_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UserMunicipalities_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 2   DROP SEQUENCE public."UserMunicipalities_Id_seq";
       public       postgres    false    284            "           0    0    UserMunicipalities_Id_seq    SEQUENCE OWNED BY     ]   ALTER SEQUENCE public."UserMunicipalities_Id_seq" OWNED BY public."UserMunicipalities"."Id";
            public       postgres    false    283                       1259    275124    UserParcels    TABLE     �   CREATE TABLE public."UserParcels" (
    "Id" integer NOT NULL,
    "UserId" integer NOT NULL,
    "ParcelId" integer NOT NULL
);
 !   DROP TABLE public."UserParcels";
       public         postgres    false                       1259    275122    UserParcels_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UserParcels_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 +   DROP SEQUENCE public."UserParcels_Id_seq";
       public       postgres    false    286            #           0    0    UserParcels_Id_seq    SEQUENCE OWNED BY     O   ALTER SEQUENCE public."UserParcels_Id_seq" OWNED BY public."UserParcels"."Id";
            public       postgres    false    285            �            1259    274761    UserType    TABLE     �   CREATE TABLE public."UserType" (
    "Id" integer NOT NULL,
    "Name" text,
    "Description" text,
    "SortOrder" integer NOT NULL
);
    DROP TABLE public."UserType";
       public         postgres    false            �            1259    274759    UserType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UserType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 (   DROP SEQUENCE public."UserType_Id_seq";
       public       postgres    false    250            $           0    0    UserType_Id_seq    SEQUENCE OWNED BY     I   ALTER SEQUENCE public."UserType_Id_seq" OWNED BY public."UserType"."Id";
            public       postgres    false    249                        1259    275142    UserWatersheds    TABLE     �   CREATE TABLE public."UserWatersheds" (
    "Id" integer NOT NULL,
    "UserId" integer NOT NULL,
    "WatershedId" integer NOT NULL
);
 $   DROP TABLE public."UserWatersheds";
       public         postgres    false                       1259    275140    UserWatersheds_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."UserWatersheds_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 .   DROP SEQUENCE public."UserWatersheds_Id_seq";
       public       postgres    false    288            %           0    0    UserWatersheds_Id_seq    SEQUENCE OWNED BY     U   ALTER SEQUENCE public."UserWatersheds_Id_seq" OWNED BY public."UserWatersheds"."Id";
            public       postgres    false    287            	           1259    274892    User_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."User_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 $   DROP SEQUENCE public."User_Id_seq";
       public       postgres    false    266            &           0    0    User_Id_seq    SEQUENCE OWNED BY     A   ALTER SEQUENCE public."User_Id_seq" OWNED BY public."User"."Id";
            public       postgres    false    265            N           1259    275703    VegetativeFilterStrip    TABLE     �  CREATE TABLE public."VegetativeFilterStrip" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Width" numeric(5,0) NOT NULL,
    "Length" numeric(5,0) NOT NULL,
    "Area" numeric(12,4) NOT NULL,
    "AreaRatio" numeric(12,0) NOT NULL,
    "DrainageArea" public.geometry(Polygon)
);
 +   DROP TABLE public."VegetativeFilterStrip";
       public         postgres    false    2    2    2    2    2    2    2    2    2    2    2    2    2    2    2    2            M           1259    275701    VegetativeFilterStrip_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."VegetativeFilterStrip_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 5   DROP SEQUENCE public."VegetativeFilterStrip_Id_seq";
       public       postgres    false    334            '           0    0    VegetativeFilterStrip_Id_seq    SEQUENCE OWNED BY     c   ALTER SEQUENCE public."VegetativeFilterStrip_Id_seq" OWNED BY public."VegetativeFilterStrip"."Id";
            public       postgres    false    333            P           1259    275729    Wascob    TABLE     ?  CREATE TABLE public."Wascob" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "SubAreaId" integer NOT NULL,
    "ReachId" integer NOT NULL,
    "Name" character varying(50),
    "Geometry" public.geometry(MultiPolygon),
    "Area" numeric(12,4) NOT NULL,
    "Volume" numeric(12,0) NOT NULL
);
    DROP TABLE public."Wascob";
       public         postgres    false    2    2    2    2    2    2    2    2            O           1259    275727    Wascob_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Wascob_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 &   DROP SEQUENCE public."Wascob_Id_seq";
       public       postgres    false    336            (           0    0    Wascob_Id_seq    SEQUENCE OWNED BY     E   ALTER SEQUENCE public."Wascob_Id_seq" OWNED BY public."Wascob"."Id";
            public       postgres    false    335            �            1259    274772 	   Watershed    TABLE       CREATE TABLE public."Watershed" (
    "Id" integer NOT NULL,
    "Geometry" public.geometry(MultiPolygon),
    "Name" text,
    "Alias" text,
    "Description" text,
    "Area" numeric NOT NULL,
    "OutletReachId" integer NOT NULL,
    "Modified" timestamp with time zone NOT NULL
);
    DROP TABLE public."Watershed";
       public         postgres    false    2    2    2    2    2    2    2    2                       1259    274987    WatershedExistingBMPType    TABLE     �   CREATE TABLE public."WatershedExistingBMPType" (
    "Id" integer NOT NULL,
    "ModelComponentId" integer NOT NULL,
    "ScenarioTypeId" integer NOT NULL,
    "BMPTypeId" integer NOT NULL,
    "InvestorId" integer NOT NULL
);
 .   DROP TABLE public."WatershedExistingBMPType";
       public         postgres    false                       1259    274985    WatershedExistingBMPType_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."WatershedExistingBMPType_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 8   DROP SEQUENCE public."WatershedExistingBMPType_Id_seq";
       public       postgres    false    274            )           0    0    WatershedExistingBMPType_Id_seq    SEQUENCE OWNED BY     i   ALTER SEQUENCE public."WatershedExistingBMPType_Id_seq" OWNED BY public."WatershedExistingBMPType"."Id";
            public       postgres    false    273            �            1259    274770    Watershed_Id_seq    SEQUENCE     �   CREATE SEQUENCE public."Watershed_Id_seq"
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
 )   DROP SEQUENCE public."Watershed_Id_seq";
       public       postgres    false    252            *           0    0    Watershed_Id_seq    SEQUENCE OWNED BY     K   ALTER SEQUENCE public."Watershed_Id_seq" OWNED BY public."Watershed"."Id";
            public       postgres    false    251            �            1259    272978    __EFMigrationsHistory    TABLE     �   CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);
 +   DROP TABLE public."__EFMigrationsHistory";
       public         postgres    false                       2604    274566    AnimalType Id    DEFAULT     t   ALTER TABLE ONLY public."AnimalType" ALTER COLUMN "Id" SET DEFAULT nextval('public."AnimalType_Id_seq"'::regclass);
 @   ALTER TABLE public."AnimalType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    214    213    214                       2604    274918    BMPCombinationBMPTypes Id    DEFAULT     �   ALTER TABLE ONLY public."BMPCombinationBMPTypes" ALTER COLUMN "Id" SET DEFAULT nextval('public."BMPCombinationBMPTypes_Id_seq"'::regclass);
 L   ALTER TABLE public."BMPCombinationBMPTypes" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    268    267    268                       2604    274577    BMPCombinationType Id    DEFAULT     �   ALTER TABLE ONLY public."BMPCombinationType" ALTER COLUMN "Id" SET DEFAULT nextval('public."BMPCombinationType_Id_seq"'::regclass);
 H   ALTER TABLE public."BMPCombinationType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    216    215    216                       2604    274588    BMPEffectivenessLocationType Id    DEFAULT     �   ALTER TABLE ONLY public."BMPEffectivenessLocationType" ALTER COLUMN "Id" SET DEFAULT nextval('public."BMPEffectivenessLocationType_Id_seq"'::regclass);
 R   ALTER TABLE public."BMPEffectivenessLocationType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    218    217    218                       2604    274936    BMPEffectivenessType Id    DEFAULT     �   ALTER TABLE ONLY public."BMPEffectivenessType" ALTER COLUMN "Id" SET DEFAULT nextval('public."BMPEffectivenessType_Id_seq"'::regclass);
 J   ALTER TABLE public."BMPEffectivenessType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    269    270    270                       2604    274802 
   BMPType Id    DEFAULT     n   ALTER TABLE ONLY public."BMPType" ALTER COLUMN "Id" SET DEFAULT nextval('public."BMPType_Id_seq"'::regclass);
 =   ALTER TABLE public."BMPType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    255    256    256            .           2604    275306    CatchBasin Id    DEFAULT     t   ALTER TABLE ONLY public."CatchBasin" ALTER COLUMN "Id" SET DEFAULT nextval('public."CatchBasin_Id_seq"'::regclass);
 @   ALTER TABLE public."CatchBasin" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    303    304    304            /           2604    275332    ClosedDrain Id    DEFAULT     v   ALTER TABLE ONLY public."ClosedDrain" ALTER COLUMN "Id" SET DEFAULT nextval('public."ClosedDrain_Id_seq"'::regclass);
 A   ALTER TABLE public."ClosedDrain" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    305    306    306                       2604    274599 
   Country Id    DEFAULT     n   ALTER TABLE ONLY public."Country" ALTER COLUMN "Id" SET DEFAULT nextval('public."Country_Id_seq"'::regclass);
 =   ALTER TABLE public."Country" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    219    220    220            0           2604    275358 	   Dugout Id    DEFAULT     l   ALTER TABLE ONLY public."Dugout" ALTER COLUMN "Id" SET DEFAULT nextval('public."Dugout_Id_seq"'::regclass);
 <   ALTER TABLE public."Dugout" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    308    307    308                       2604    274610    Farm Id    DEFAULT     h   ALTER TABLE ONLY public."Farm" ALTER COLUMN "Id" SET DEFAULT nextval('public."Farm_Id_seq"'::regclass);
 :   ALTER TABLE public."Farm" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    222    221    222            1           2604    275389 
   Feedlot Id    DEFAULT     n   ALTER TABLE ONLY public."Feedlot" ALTER COLUMN "Id" SET DEFAULT nextval('public."Feedlot_Id_seq"'::regclass);
 =   ALTER TABLE public."Feedlot" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    310    309    310            2           2604    275420    FlowDiversion Id    DEFAULT     z   ALTER TABLE ONLY public."FlowDiversion" ALTER COLUMN "Id" SET DEFAULT nextval('public."FlowDiversion_Id_seq"'::regclass);
 C   ALTER TABLE public."FlowDiversion" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    311    312    312                       2604    274621    GeometryLayerStyle Id    DEFAULT     �   ALTER TABLE ONLY public."GeometryLayerStyle" ALTER COLUMN "Id" SET DEFAULT nextval('public."GeometryLayerStyle_Id_seq"'::regclass);
 H   ALTER TABLE public."GeometryLayerStyle" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    224    223    224            3           2604    275446    GrassedWaterway Id    DEFAULT     ~   ALTER TABLE ONLY public."GrassedWaterway" ALTER COLUMN "Id" SET DEFAULT nextval('public."GrassedWaterway_Id_seq"'::regclass);
 E   ALTER TABLE public."GrassedWaterway" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    313    314    314                       2604    274632    Investor Id    DEFAULT     p   ALTER TABLE ONLY public."Investor" ALTER COLUMN "Id" SET DEFAULT nextval('public."Investor_Id_seq"'::regclass);
 >   ALTER TABLE public."Investor" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    225    226    226            4           2604    275472    IsolatedWetland Id    DEFAULT     ~   ALTER TABLE ONLY public."IsolatedWetland" ALTER COLUMN "Id" SET DEFAULT nextval('public."IsolatedWetland_Id_seq"'::regclass);
 E   ALTER TABLE public."IsolatedWetland" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    315    316    316            5           2604    275498    Lake Id    DEFAULT     h   ALTER TABLE ONLY public."Lake" ALTER COLUMN "Id" SET DEFAULT nextval('public."Lake_Id_seq"'::regclass);
 :   ALTER TABLE public."Lake" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    318    317    318                       2604    274643    LegalSubDivision Id    DEFAULT     �   ALTER TABLE ONLY public."LegalSubDivision" ALTER COLUMN "Id" SET DEFAULT nextval('public."LegalSubDivision_Id_seq"'::regclass);
 F   ALTER TABLE public."LegalSubDivision" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    228    227    228            6           2604    275524    ManureStorage Id    DEFAULT     z   ALTER TABLE ONLY public."ManureStorage" ALTER COLUMN "Id" SET DEFAULT nextval('public."ManureStorage_Id_seq"'::regclass);
 C   ALTER TABLE public."ManureStorage" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    319    320    320                       2604    274839    ModelComponent Id    DEFAULT     |   ALTER TABLE ONLY public."ModelComponent" ALTER COLUMN "Id" SET DEFAULT nextval('public."ModelComponent_Id_seq"'::regclass);
 D   ALTER TABLE public."ModelComponent" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    259    260    260                       2604    274972    ModelComponentBMPTypes Id    DEFAULT     �   ALTER TABLE ONLY public."ModelComponentBMPTypes" ALTER COLUMN "Id" SET DEFAULT nextval('public."ModelComponentBMPTypes_Id_seq"'::regclass);
 L   ALTER TABLE public."ModelComponentBMPTypes" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    272    271    272            	           2604    274654    ModelComponentType Id    DEFAULT     �   ALTER TABLE ONLY public."ModelComponentType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ModelComponentType_Id_seq"'::regclass);
 H   ALTER TABLE public."ModelComponentType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    229    230    230            
           2604    274665    Municipality Id    DEFAULT     x   ALTER TABLE ONLY public."Municipality" ALTER COLUMN "Id" SET DEFAULT nextval('public."Municipality_Id_seq"'::regclass);
 B   ALTER TABLE public."Municipality" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    231    232    232            *           2604    275236    Optimization Id    DEFAULT     x   ALTER TABLE ONLY public."Optimization" ALTER COLUMN "Id" SET DEFAULT nextval('public."Optimization_Id_seq"'::regclass);
 B   ALTER TABLE public."Optimization" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    296    295    296                       2604    274676 "   OptimizationConstraintValueType Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationConstraintValueType" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationConstraintValueType_Id_seq"'::regclass);
 U   ALTER TABLE public."OptimizationConstraintValueType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    233    234    234            ?           2604    275758    OptimizationConstraints Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationConstraints" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationConstraints_Id_seq"'::regclass);
 M   ALTER TABLE public."OptimizationConstraints" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    338    337    338            @           2604    275784     OptimizationLegalSubDivisions Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationLegalSubDivisions_Id_seq"'::regclass);
 S   ALTER TABLE public."OptimizationLegalSubDivisions" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    339    340    340            A           2604    275807    OptimizationParcels Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationParcels" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationParcels_Id_seq"'::regclass);
 I   ALTER TABLE public."OptimizationParcels" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    341    342    342                       2604    274687    OptimizationType Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationType" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationType_Id_seq"'::regclass);
 F   ALTER TABLE public."OptimizationType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    235    236    236            B           2604    275830    OptimizationWeights Id    DEFAULT     �   ALTER TABLE ONLY public."OptimizationWeights" ALTER COLUMN "Id" SET DEFAULT nextval('public."OptimizationWeights_Id_seq"'::regclass);
 I   ALTER TABLE public."OptimizationWeights" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    343    344    344                       2604    274698 	   Parcel Id    DEFAULT     l   ALTER TABLE ONLY public."Parcel" ALTER COLUMN "Id" SET DEFAULT nextval('public."Parcel_Id_seq"'::regclass);
 <   ALTER TABLE public."Parcel" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    238    237    238            7           2604    275550    PointSource Id    DEFAULT     v   ALTER TABLE ONLY public."PointSource" ALTER COLUMN "Id" SET DEFAULT nextval('public."PointSource_Id_seq"'::regclass);
 A   ALTER TABLE public."PointSource" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    322    321    322            #           2604    275083 
   Project Id    DEFAULT     n   ALTER TABLE ONLY public."Project" ALTER COLUMN "Id" SET DEFAULT nextval('public."Project_Id_seq"'::regclass);
 =   ALTER TABLE public."Project" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    281    282    282            +           2604    275257    ProjectMunicipalities Id    DEFAULT     �   ALTER TABLE ONLY public."ProjectMunicipalities" ALTER COLUMN "Id" SET DEFAULT nextval('public."ProjectMunicipalities_Id_seq"'::regclass);
 K   ALTER TABLE public."ProjectMunicipalities" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    298    297    298                       2604    274709    ProjectSpatialUnitType Id    DEFAULT     �   ALTER TABLE ONLY public."ProjectSpatialUnitType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ProjectSpatialUnitType_Id_seq"'::regclass);
 L   ALTER TABLE public."ProjectSpatialUnitType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    240    239    240            ,           2604    275275    ProjectWatersheds Id    DEFAULT     �   ALTER TABLE ONLY public."ProjectWatersheds" ALTER COLUMN "Id" SET DEFAULT nextval('public."ProjectWatersheds_Id_seq"'::regclass);
 G   ALTER TABLE public."ProjectWatersheds" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    300    299    300                       2604    274786    Province Id    DEFAULT     p   ALTER TABLE ONLY public."Province" ALTER COLUMN "Id" SET DEFAULT nextval('public."Province_Id_seq"'::regclass);
 >   ALTER TABLE public."Province" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    254    253    254            (           2604    275184    Reach Id    DEFAULT     j   ALTER TABLE ONLY public."Reach" ALTER COLUMN "Id" SET DEFAULT nextval('public."Reach_Id_seq"'::regclass);
 ;   ALTER TABLE public."Reach" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    291    292    292            8           2604    275576    Reservoir Id    DEFAULT     r   ALTER TABLE ONLY public."Reservoir" ALTER COLUMN "Id" SET DEFAULT nextval('public."Reservoir_Id_seq"'::regclass);
 ?   ALTER TABLE public."Reservoir" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    323    324    324            9           2604    275602    RiparianBuffer Id    DEFAULT     |   ALTER TABLE ONLY public."RiparianBuffer" ALTER COLUMN "Id" SET DEFAULT nextval('public."RiparianBuffer_Id_seq"'::regclass);
 D   ALTER TABLE public."RiparianBuffer" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    326    325    326            :           2604    275628    RiparianWetland Id    DEFAULT     ~   ALTER TABLE ONLY public."RiparianWetland" ALTER COLUMN "Id" SET DEFAULT nextval('public."RiparianWetland_Id_seq"'::regclass);
 E   ALTER TABLE public."RiparianWetland" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    327    328    328            ;           2604    275654    RockChute Id    DEFAULT     r   ALTER TABLE ONLY public."RockChute" ALTER COLUMN "Id" SET DEFAULT nextval('public."RockChute_Id_seq"'::regclass);
 ?   ALTER TABLE public."RockChute" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    329    330    330                       2604    274860    Scenario Id    DEFAULT     p   ALTER TABLE ONLY public."Scenario" ALTER COLUMN "Id" SET DEFAULT nextval('public."Scenario_Id_seq"'::regclass);
 >   ALTER TABLE public."Scenario" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    261    262    262                        2604    275018    ScenarioModelResult Id    DEFAULT     �   ALTER TABLE ONLY public."ScenarioModelResult" ALTER COLUMN "Id" SET DEFAULT nextval('public."ScenarioModelResult_Id_seq"'::regclass);
 I   ALTER TABLE public."ScenarioModelResult" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    276    275    276                       2604    274818    ScenarioModelResultType Id    DEFAULT     �   ALTER TABLE ONLY public."ScenarioModelResultType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ScenarioModelResultType_Id_seq"'::regclass);
 M   ALTER TABLE public."ScenarioModelResultType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    257    258    258                       2604    274720 "   ScenarioModelResultVariableType Id    DEFAULT     �   ALTER TABLE ONLY public."ScenarioModelResultVariableType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ScenarioModelResultVariableType_Id_seq"'::regclass);
 U   ALTER TABLE public."ScenarioModelResultVariableType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    242    241    242                       2604    274731 "   ScenarioResultSummarizationType Id    DEFAULT     �   ALTER TABLE ONLY public."ScenarioResultSummarizationType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ScenarioResultSummarizationType_Id_seq"'::regclass);
 U   ALTER TABLE public."ScenarioResultSummarizationType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    244    243    244                       2604    274742    ScenarioType Id    DEFAULT     x   ALTER TABLE ONLY public."ScenarioType" ALTER COLUMN "Id" SET DEFAULT nextval('public."ScenarioType_Id_seq"'::regclass);
 B   ALTER TABLE public."ScenarioType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    245    246    246            <           2604    275680    SmallDam Id    DEFAULT     p   ALTER TABLE ONLY public."SmallDam" ALTER COLUMN "Id" SET DEFAULT nextval('public."SmallDam_Id_seq"'::regclass);
 >   ALTER TABLE public."SmallDam" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    332    331    332            -           2604    275293    Solution Id    DEFAULT     p   ALTER TABLE ONLY public."Solution" ALTER COLUMN "Id" SET DEFAULT nextval('public."Solution_Id_seq"'::regclass);
 >   ALTER TABLE public."Solution" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    302    301    302            C           2604    275848    SolutionLegalSubDivisions Id    DEFAULT     �   ALTER TABLE ONLY public."SolutionLegalSubDivisions" ALTER COLUMN "Id" SET DEFAULT nextval('public."SolutionLegalSubDivisions_Id_seq"'::regclass);
 O   ALTER TABLE public."SolutionLegalSubDivisions" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    346    345    346            D           2604    275871    SolutionModelComponents Id    DEFAULT     �   ALTER TABLE ONLY public."SolutionModelComponents" ALTER COLUMN "Id" SET DEFAULT nextval('public."SolutionModelComponents_Id_seq"'::regclass);
 M   ALTER TABLE public."SolutionModelComponents" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    348    347    348            E           2604    275894    SolutionParcels Id    DEFAULT     ~   ALTER TABLE ONLY public."SolutionParcels" ALTER COLUMN "Id" SET DEFAULT nextval('public."SolutionParcels_Id_seq"'::regclass);
 E   ALTER TABLE public."SolutionParcels" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    349    350    350            )           2604    275205 
   SubArea Id    DEFAULT     n   ALTER TABLE ONLY public."SubArea" ALTER COLUMN "Id" SET DEFAULT nextval('public."SubArea_Id_seq"'::regclass);
 =   ALTER TABLE public."SubArea" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    294    293    294                       2604    274881    SubWatershed Id    DEFAULT     x   ALTER TABLE ONLY public."SubWatershed" ALTER COLUMN "Id" SET DEFAULT nextval('public."SubWatershed_Id_seq"'::regclass);
 B   ALTER TABLE public."SubWatershed" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    263    264    264            "           2604    275067    Subbasin Id    DEFAULT     p   ALTER TABLE ONLY public."Subbasin" ALTER COLUMN "Id" SET DEFAULT nextval('public."Subbasin_Id_seq"'::regclass);
 >   ALTER TABLE public."Subbasin" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    279    280    280            !           2604    275044    UnitScenario Id    DEFAULT     x   ALTER TABLE ONLY public."UnitScenario" ALTER COLUMN "Id" SET DEFAULT nextval('public."UnitScenario_Id_seq"'::regclass);
 B   ALTER TABLE public."UnitScenario" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    277    278    278            '           2604    275163    UnitScenarioEffectiveness Id    DEFAULT     �   ALTER TABLE ONLY public."UnitScenarioEffectiveness" ALTER COLUMN "Id" SET DEFAULT nextval('public."UnitScenarioEffectiveness_Id_seq"'::regclass);
 O   ALTER TABLE public."UnitScenarioEffectiveness" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    289    290    290                       2604    274753    UnitType Id    DEFAULT     p   ALTER TABLE ONLY public."UnitType" ALTER COLUMN "Id" SET DEFAULT nextval('public."UnitType_Id_seq"'::regclass);
 >   ALTER TABLE public."UnitType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    248    247    248                       2604    274897    User Id    DEFAULT     h   ALTER TABLE ONLY public."User" ALTER COLUMN "Id" SET DEFAULT nextval('public."User_Id_seq"'::regclass);
 :   ALTER TABLE public."User" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    266    265    266            $           2604    275109    UserMunicipalities Id    DEFAULT     �   ALTER TABLE ONLY public."UserMunicipalities" ALTER COLUMN "Id" SET DEFAULT nextval('public."UserMunicipalities_Id_seq"'::regclass);
 H   ALTER TABLE public."UserMunicipalities" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    283    284    284            %           2604    275127    UserParcels Id    DEFAULT     v   ALTER TABLE ONLY public."UserParcels" ALTER COLUMN "Id" SET DEFAULT nextval('public."UserParcels_Id_seq"'::regclass);
 A   ALTER TABLE public."UserParcels" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    286    285    286                       2604    274764    UserType Id    DEFAULT     p   ALTER TABLE ONLY public."UserType" ALTER COLUMN "Id" SET DEFAULT nextval('public."UserType_Id_seq"'::regclass);
 >   ALTER TABLE public."UserType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    249    250    250            &           2604    275145    UserWatersheds Id    DEFAULT     |   ALTER TABLE ONLY public."UserWatersheds" ALTER COLUMN "Id" SET DEFAULT nextval('public."UserWatersheds_Id_seq"'::regclass);
 D   ALTER TABLE public."UserWatersheds" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    287    288    288            =           2604    275706    VegetativeFilterStrip Id    DEFAULT     �   ALTER TABLE ONLY public."VegetativeFilterStrip" ALTER COLUMN "Id" SET DEFAULT nextval('public."VegetativeFilterStrip_Id_seq"'::regclass);
 K   ALTER TABLE public."VegetativeFilterStrip" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    333    334    334            >           2604    275732 	   Wascob Id    DEFAULT     l   ALTER TABLE ONLY public."Wascob" ALTER COLUMN "Id" SET DEFAULT nextval('public."Wascob_Id_seq"'::regclass);
 <   ALTER TABLE public."Wascob" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    336    335    336                       2604    274775    Watershed Id    DEFAULT     r   ALTER TABLE ONLY public."Watershed" ALTER COLUMN "Id" SET DEFAULT nextval('public."Watershed_Id_seq"'::regclass);
 ?   ALTER TABLE public."Watershed" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    251    252    252                       2604    274990    WatershedExistingBMPType Id    DEFAULT     �   ALTER TABLE ONLY public."WatershedExistingBMPType" ALTER COLUMN "Id" SET DEFAULT nextval('public."WatershedExistingBMPType_Id_seq"'::regclass);
 N   ALTER TABLE public."WatershedExistingBMPType" ALTER COLUMN "Id" DROP DEFAULT;
       public       postgres    false    274    273    274            V          0    274563 
   AnimalType 
   TABLE DATA               P   COPY public."AnimalType" ("Id", "Name", "Description", "SortOrder") FROM stdin;
    public       postgres    false    214   0      �          0    274915    BMPCombinationBMPTypes 
   TABLE DATA               ]   COPY public."BMPCombinationBMPTypes" ("Id", "BMPCombinationTypeId", "BMPTypeId") FROM stdin;
    public       postgres    false    268   �      X          0    274574    BMPCombinationType 
   TABLE DATA               X   COPY public."BMPCombinationType" ("Id", "Name", "Description", "SortOrder") FROM stdin;
    public       postgres    false    216   �      Z          0    274585    BMPEffectivenessLocationType 
   TABLE DATA               o   COPY public."BMPEffectivenessLocationType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    218   <      �          0    274933    BMPEffectivenessType 
   TABLE DATA                 COPY public."BMPEffectivenessType" ("Id", "Name", "Description", "SortOrder", "ScenarioModelResultTypeId", "UnitTypeId", "ScenarioModelResultVariableTypeId", "DefaultWeight", "DefaultConstraintTypeId", "DefaultConstraint", "BMPEffectivenessLocationTypeId") FROM stdin;
    public       postgres    false    270   s      �          0    274799    BMPType 
   TABLE DATA               e   COPY public."BMPType" ("Id", "Name", "Description", "SortOrder", "ModelComponentTypeId") FROM stdin;
    public       postgres    false    256   H      �          0    275303 
   CatchBasin 
   TABLE DATA               ~   COPY public."CatchBasin" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    304   �      �          0    275329    ClosedDrain 
   TABLE DATA               m   COPY public."ClosedDrain" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry") FROM stdin;
    public       postgres    false    306   '       \          0    274596    Country 
   TABLE DATA               M   COPY public."Country" ("Id", "Name", "Description", "SortOrder") FROM stdin;
    public       postgres    false    220   D       �          0    275355    Dugout 
   TABLE DATA               �   COPY public."Dugout" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume", "AnimalTypeId") FROM stdin;
    public       postgres    false    308   u       ^          0    274607    Farm 
   TABLE DATA               :   COPY public."Farm" ("Id", "Geometry", "Name") FROM stdin;
    public       postgres    false    222   �       �          0    275386    Feedlot 
   TABLE DATA               �   COPY public."Feedlot" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "AnimalTypeId", "AnimalNumber", "AnimalAdultRatio", "Area") FROM stdin;
    public       postgres    false    310   �       �          0    275417    FlowDiversion 
   TABLE DATA               y   COPY public."FlowDiversion" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Length") FROM stdin;
    public       postgres    false    312   ]!      `          0    274618    GeometryLayerStyle 
   TABLE DATA               �   COPY public."GeometryLayerStyle" ("Id", layername, type, style, color, simplelinewidth, outlinecolor, outlinewidth) FROM stdin;
    public       postgres    false    224   z!      �          0    275443    GrassedWaterway 
   TABLE DATA               �   COPY public."GrassedWaterway" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Width", "Length") FROM stdin;
    public       postgres    false    314   H"      b          0    274629    Investor 
   TABLE DATA               N   COPY public."Investor" ("Id", "Name", "Description", "SortOrder") FROM stdin;
    public       postgres    false    226   �.      �          0    275469    IsolatedWetland 
   TABLE DATA               �   COPY public."IsolatedWetland" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    316   /      �          0    275495    Lake 
   TABLE DATA               x   COPY public."Lake" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    318   �3      d          0    274640    LegalSubDivision 
   TABLE DATA               �   COPY public."LegalSubDivision" ("Id", "Geometry", "Meridian", "Range", "Township", "Section", "Quarter", "LSD", "FullDescription") FROM stdin;
    public       postgres    false    228   �3      �          0    275521    ManureStorage 
   TABLE DATA               �   COPY public."ManureStorage" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    320   �A      �          0    274836    ModelComponent 
   TABLE DATA               y   COPY public."ModelComponent" ("Id", "ModelId", "Name", "Description", "WatershedId", "ModelComponentTypeId") FROM stdin;
    public       postgres    false    260   �A      �          0    274969    ModelComponentBMPTypes 
   TABLE DATA               Y   COPY public."ModelComponentBMPTypes" ("Id", "ModelComponentId", "BMPTypeId") FROM stdin;
    public       postgres    false    272   �D      f          0    274651    ModelComponentType 
   TABLE DATA               g   COPY public."ModelComponentType" ("Id", "Name", "Description", "SortOrder", "IsStructure") FROM stdin;
    public       postgres    false    230   �D      h          0    274662    Municipality 
   TABLE DATA               L   COPY public."Municipality" ("Id", "Name", "Region", "Geometry") FROM stdin;
    public       postgres    false    232   �F      �          0    275233    Optimization 
   TABLE DATA               a   COPY public."Optimization" ("Id", "ProjectId", "OptimizationTypeId", "BudgetTarget") FROM stdin;
    public       postgres    false    296   �G      j          0    274673    OptimizationConstraintValueType 
   TABLE DATA               r   COPY public."OptimizationConstraintValueType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    234   �G      �          0    275755    OptimizationConstraints 
   TABLE DATA               �   COPY public."OptimizationConstraints" ("Id", "OptimizationId", "BMPEffectivenessTypeId", "OptimizationConstraintValueTypeId", "Constraint") FROM stdin;
    public       postgres    false    338   �G      �          0    275781    OptimizationLegalSubDivisions 
   TABLE DATA               �   COPY public."OptimizationLegalSubDivisions" ("Id", "OptimizationId", "BMPTypeId", "LegalSubDivisionId", "IsSelected") FROM stdin;
    public       postgres    false    340   �G      �          0    275804    OptimizationParcels 
   TABLE DATA               n   COPY public."OptimizationParcels" ("Id", "OptimizationId", "BMPTypeId", "ParcelId", "IsSelected") FROM stdin;
    public       postgres    false    342   H      l          0    274684    OptimizationType 
   TABLE DATA               c   COPY public."OptimizationType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    236   8H      �          0    275827    OptimizationWeights 
   TABLE DATA               k   COPY public."OptimizationWeights" ("Id", "OptimizationId", "BMPEffectivenessTypeId", "Weight") FROM stdin;
    public       postgres    false    344   uH      n          0    274695    Parcel 
   TABLE DATA               ~   COPY public."Parcel" ("Id", "Geometry", "Meridian", "Range", "Township", "Section", "Quarter", "FullDescription") FROM stdin;
    public       postgres    false    238   �H      �          0    275547    PointSource 
   TABLE DATA               m   COPY public."PointSource" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry") FROM stdin;
    public       postgres    false    322   �T      �          0    275080    Project 
   TABLE DATA               �   COPY public."Project" ("Id", "Name", "Description", "Created", "Modified", "Active", "StartYear", "EndYear", "UserId", "ScenarioTypeId", "ProjectSpatialUnitTypeId") FROM stdin;
    public       postgres    false    282   �T      �          0    275254    ProjectMunicipalities 
   TABLE DATA               V   COPY public."ProjectMunicipalities" ("Id", "ProjectId", "MunicipalityId") FROM stdin;
    public       postgres    false    298   �W      p          0    274706    ProjectSpatialUnitType 
   TABLE DATA               i   COPY public."ProjectSpatialUnitType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    240   �X      �          0    275272    ProjectWatersheds 
   TABLE DATA               O   COPY public."ProjectWatersheds" ("Id", "ProjectId", "WatershedId") FROM stdin;
    public       postgres    false    300   �X      ~          0    274783    Province 
   TABLE DATA               c   COPY public."Province" ("Id", "Name", "Description", "SortOrder", "Code", "CountryId") FROM stdin;
    public       postgres    false    254   �Y      �          0    275181    Reach 
   TABLE DATA               U   COPY public."Reach" ("Id", "ModelComponentId", "SubbasinId", "Geometry") FROM stdin;
    public       postgres    false    292   R]      �          0    275573 	   Reservoir 
   TABLE DATA               }   COPY public."Reservoir" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    324   7`      �          0    275599    RiparianBuffer 
   TABLE DATA               �   COPY public."RiparianBuffer" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Width", "Length", "Area", "AreaRatio", "DrainageArea") FROM stdin;
    public       postgres    false    326   T`      �          0    275625    RiparianWetland 
   TABLE DATA               �   COPY public."RiparianWetland" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    328   q`      �          0    275651 	   RockChute 
   TABLE DATA               k   COPY public."RockChute" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry") FROM stdin;
    public       postgres    false    330   �`      �          0    274857    Scenario 
   TABLE DATA               b   COPY public."Scenario" ("Id", "Name", "Description", "WatershedId", "ScenarioTypeId") FROM stdin;
    public       postgres    false    262   �`      �          0    275015    ScenarioModelResult 
   TABLE DATA               �   COPY public."ScenarioModelResult" ("Id", "ScenarioId", "ModelComponentId", "ScenarioModelResultTypeId", "Year", "Value") FROM stdin;
    public       postgres    false    276   a      �          0    274815    ScenarioModelResultType 
   TABLE DATA               �   COPY public."ScenarioModelResultType" ("Id", "Name", "Description", "SortOrder", "UnitTypeId", "ModelComponentTypeId", "ScenarioModelResultVariableTypeId") FROM stdin;
    public       postgres    false    258   �h      r          0    274717    ScenarioModelResultVariableType 
   TABLE DATA               r   COPY public."ScenarioModelResultVariableType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    242   �j      t          0    274728    ScenarioResultSummarizationType 
   TABLE DATA               r   COPY public."ScenarioResultSummarizationType" ("Id", "Name", "Description", "SortOrder", "IsDefault") FROM stdin;
    public       postgres    false    244   �k      v          0    274739    ScenarioType 
   TABLE DATA               m   COPY public."ScenarioType" ("Id", "Name", "Description", "SortOrder", "IsBaseLine", "IsDefault") FROM stdin;
    public       postgres    false    246   �k      �          0    275677    SmallDam 
   TABLE DATA               |   COPY public."SmallDam" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    332   <l      �          0    275290    Solution 
   TABLE DATA               K   COPY public."Solution" ("Id", "ProjectId", "FromOptimization") FROM stdin;
    public       postgres    false    302   Yl      �          0    275845    SolutionLegalSubDivisions 
   TABLE DATA               z   COPY public."SolutionLegalSubDivisions" ("Id", "SolutionId", "BMPTypeId", "LegalSubDivisionId", "IsSelected") FROM stdin;
    public       postgres    false    346   Km      �          0    275868    SolutionModelComponents 
   TABLE DATA               v   COPY public."SolutionModelComponents" ("Id", "SolutionId", "BMPTypeId", "ModelComponentId", "IsSelected") FROM stdin;
    public       postgres    false    348   �p      �          0    275891    SolutionParcels 
   TABLE DATA               f   COPY public."SolutionParcels" ("Id", "SolutionId", "BMPTypeId", "ParcelId", "IsSelected") FROM stdin;
    public       postgres    false    350   kq      �          0    275202    SubArea 
   TABLE DATA               �   COPY public."SubArea" ("Id", "Geometry", "ModelComponentId", "SubbasinId", "LegalSubDivisionId", "ParcelId", "Area", "Elevation", "Slope", "LandUse", "SoilTexture") FROM stdin;
    public       postgres    false    294   �r      �          0    274878    SubWatershed 
   TABLE DATA               }   COPY public."SubWatershed" ("Id", "Geometry", "Name", "Alias", "Description", "Area", "Modified", "WatershedId") FROM stdin;
    public       postgres    false    264   4�      �          0    275064    Subbasin 
   TABLE DATA               H   COPY public."Subbasin" ("Id", "Geometry", "SubWatershedId") FROM stdin;
    public       postgres    false    280   ��      �          0    275041    UnitScenario 
   TABLE DATA               d   COPY public."UnitScenario" ("Id", "ModelComponentId", "ScenarioId", "BMPCombinationId") FROM stdin;
    public       postgres    false    278   	�      �          0    275160    UnitScenarioEffectiveness 
   TABLE DATA               x   COPY public."UnitScenarioEffectiveness" ("Id", "UnitScenarioId", "BMPEffectivenessTypeId", "Year", "Value") FROM stdin;
    public       postgres    false    290   ��      x          0    274750    UnitType 
   TABLE DATA               \   COPY public."UnitType" ("Id", "Name", "Description", "SortOrder", "UnitSymbol") FROM stdin;
    public       postgres    false    248   ��	      �          0    274894    User 
   TABLE DATA               �  COPY public."User" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "FirstName", "LastName", "Active", "Address1", "Address2", "PostalCode", "Municipality", "City", "ProvinceId", "DateOfBirth", "TaxRollNumber", "DriverLicense", "LastFourDigitOfSIN", "Organization", "LastModified", "UserTypeId") FROM stdin;
    public       postgres    false    266   ��	      �          0    275106    UserMunicipalities 
   TABLE DATA               P   COPY public."UserMunicipalities" ("Id", "UserId", "MunicipalityId") FROM stdin;
    public       postgres    false    284   T�	      �          0    275124    UserParcels 
   TABLE DATA               C   COPY public."UserParcels" ("Id", "UserId", "ParcelId") FROM stdin;
    public       postgres    false    286   {�	      z          0    274761    UserType 
   TABLE DATA               N   COPY public."UserType" ("Id", "Name", "Description", "SortOrder") FROM stdin;
    public       postgres    false    250   Ɋ	      �          0    275142    UserWatersheds 
   TABLE DATA               I   COPY public."UserWatersheds" ("Id", "UserId", "WatershedId") FROM stdin;
    public       postgres    false    288   '�	      �          0    275703    VegetativeFilterStrip 
   TABLE DATA               �   COPY public."VegetativeFilterStrip" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Width", "Length", "Area", "AreaRatio", "DrainageArea") FROM stdin;
    public       postgres    false    334   N�	      �          0    275729    Wascob 
   TABLE DATA               z   COPY public."Wascob" ("Id", "ModelComponentId", "SubAreaId", "ReachId", "Name", "Geometry", "Area", "Volume") FROM stdin;
    public       postgres    false    336   �	      |          0    274772 	   Watershed 
   TABLE DATA               |   COPY public."Watershed" ("Id", "Geometry", "Name", "Alias", "Description", "Area", "OutletReachId", "Modified") FROM stdin;
    public       postgres    false    252   2�	      �          0    274987    WatershedExistingBMPType 
   TABLE DATA               {   COPY public."WatershedExistingBMPType" ("Id", "ModelComponentId", "ScenarioTypeId", "BMPTypeId", "InvestorId") FROM stdin;
    public       postgres    false    274   1�	      T          0    272978    __EFMigrationsHistory 
   TABLE DATA               R   COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
    public       postgres    false    197   x�	      �          0    273292    spatial_ref_sys 
   TABLE DATA               X   COPY public.spatial_ref_sys (srid, auth_name, auth_srid, srtext, proj4text) FROM stdin;
    public       postgres    false    199   ��	      +           0    0    AnimalType_Id_seq    SEQUENCE SET     B   SELECT pg_catalog.setval('public."AnimalType_Id_seq"', 1, false);
            public       postgres    false    213            ,           0    0    BMPCombinationBMPTypes_Id_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('public."BMPCombinationBMPTypes_Id_seq"', 1, false);
            public       postgres    false    267            -           0    0    BMPCombinationType_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public."BMPCombinationType_Id_seq"', 1, false);
            public       postgres    false    215            .           0    0 #   BMPEffectivenessLocationType_Id_seq    SEQUENCE SET     T   SELECT pg_catalog.setval('public."BMPEffectivenessLocationType_Id_seq"', 1, false);
            public       postgres    false    217            /           0    0    BMPEffectivenessType_Id_seq    SEQUENCE SET     L   SELECT pg_catalog.setval('public."BMPEffectivenessType_Id_seq"', 1, false);
            public       postgres    false    269            0           0    0    BMPType_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."BMPType_Id_seq"', 1, false);
            public       postgres    false    255            1           0    0    CatchBasin_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public."CatchBasin_Id_seq"', 1, true);
            public       postgres    false    303            2           0    0    ClosedDrain_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public."ClosedDrain_Id_seq"', 1, false);
            public       postgres    false    305            3           0    0    Country_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."Country_Id_seq"', 1, false);
            public       postgres    false    219            4           0    0    Dugout_Id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public."Dugout_Id_seq"', 1, false);
            public       postgres    false    307            5           0    0    Farm_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('public."Farm_Id_seq"', 1, false);
            public       postgres    false    221            6           0    0    Feedlot_Id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public."Feedlot_Id_seq"', 1, true);
            public       postgres    false    309            7           0    0    FlowDiversion_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('public."FlowDiversion_Id_seq"', 1, false);
            public       postgres    false    311            8           0    0    GeometryLayerStyle_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public."GeometryLayerStyle_Id_seq"', 1, false);
            public       postgres    false    223            9           0    0    GrassedWaterway_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public."GrassedWaterway_Id_seq"', 8, true);
            public       postgres    false    313            :           0    0    Investor_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."Investor_Id_seq"', 4, true);
            public       postgres    false    225            ;           0    0    IsolatedWetland_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public."IsolatedWetland_Id_seq"', 3, true);
            public       postgres    false    315            <           0    0    Lake_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('public."Lake_Id_seq"', 1, false);
            public       postgres    false    317            =           0    0    LegalSubDivision_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('public."LegalSubDivision_Id_seq"', 17, true);
            public       postgres    false    227            >           0    0    ManureStorage_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('public."ManureStorage_Id_seq"', 1, false);
            public       postgres    false    319            ?           0    0    ModelComponentBMPTypes_Id_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('public."ModelComponentBMPTypes_Id_seq"', 1, false);
            public       postgres    false    271            @           0    0    ModelComponentType_Id_seq    SEQUENCE SET     J   SELECT pg_catalog.setval('public."ModelComponentType_Id_seq"', 1, false);
            public       postgres    false    229            A           0    0    ModelComponent_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public."ModelComponent_Id_seq"', 59, true);
            public       postgres    false    259            B           0    0    Municipality_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public."Municipality_Id_seq"', 1, true);
            public       postgres    false    231            C           0    0 &   OptimizationConstraintValueType_Id_seq    SEQUENCE SET     W   SELECT pg_catalog.setval('public."OptimizationConstraintValueType_Id_seq"', 1, false);
            public       postgres    false    233            D           0    0    OptimizationConstraints_Id_seq    SEQUENCE SET     O   SELECT pg_catalog.setval('public."OptimizationConstraints_Id_seq"', 1, false);
            public       postgres    false    337            E           0    0 $   OptimizationLegalSubDivisions_Id_seq    SEQUENCE SET     U   SELECT pg_catalog.setval('public."OptimizationLegalSubDivisions_Id_seq"', 1, false);
            public       postgres    false    339            F           0    0    OptimizationParcels_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('public."OptimizationParcels_Id_seq"', 1, false);
            public       postgres    false    341            G           0    0    OptimizationType_Id_seq    SEQUENCE SET     H   SELECT pg_catalog.setval('public."OptimizationType_Id_seq"', 1, false);
            public       postgres    false    235            H           0    0    OptimizationWeights_Id_seq    SEQUENCE SET     K   SELECT pg_catalog.setval('public."OptimizationWeights_Id_seq"', 1, false);
            public       postgres    false    343            I           0    0    Optimization_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('public."Optimization_Id_seq"', 1, false);
            public       postgres    false    295            J           0    0    Parcel_Id_seq    SEQUENCE SET     =   SELECT pg_catalog.setval('public."Parcel_Id_seq"', 6, true);
            public       postgres    false    237            K           0    0    PointSource_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public."PointSource_Id_seq"', 1, false);
            public       postgres    false    321            L           0    0    ProjectMunicipalities_Id_seq    SEQUENCE SET     M   SELECT pg_catalog.setval('public."ProjectMunicipalities_Id_seq"', 65, true);
            public       postgres    false    297            M           0    0    ProjectSpatialUnitType_Id_seq    SEQUENCE SET     N   SELECT pg_catalog.setval('public."ProjectSpatialUnitType_Id_seq"', 1, false);
            public       postgres    false    239            N           0    0    ProjectWatersheds_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('public."ProjectWatersheds_Id_seq"', 65, true);
            public       postgres    false    299            O           0    0    Project_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."Project_Id_seq"', 65, true);
            public       postgres    false    281            P           0    0    Province_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."Province_Id_seq"', 1, false);
            public       postgres    false    253            Q           0    0    Reach_Id_seq    SEQUENCE SET     <   SELECT pg_catalog.setval('public."Reach_Id_seq"', 6, true);
            public       postgres    false    291            R           0    0    Reservoir_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public."Reservoir_Id_seq"', 1, false);
            public       postgres    false    323            S           0    0    RiparianBuffer_Id_seq    SEQUENCE SET     F   SELECT pg_catalog.setval('public."RiparianBuffer_Id_seq"', 1, false);
            public       postgres    false    325            T           0    0    RiparianWetland_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('public."RiparianWetland_Id_seq"', 1, false);
            public       postgres    false    327            U           0    0    RockChute_Id_seq    SEQUENCE SET     A   SELECT pg_catalog.setval('public."RockChute_Id_seq"', 1, false);
            public       postgres    false    329            V           0    0    ScenarioModelResultType_Id_seq    SEQUENCE SET     O   SELECT pg_catalog.setval('public."ScenarioModelResultType_Id_seq"', 1, false);
            public       postgres    false    257            W           0    0 &   ScenarioModelResultVariableType_Id_seq    SEQUENCE SET     W   SELECT pg_catalog.setval('public."ScenarioModelResultVariableType_Id_seq"', 1, false);
            public       postgres    false    241            X           0    0    ScenarioModelResult_Id_seq    SEQUENCE SET     M   SELECT pg_catalog.setval('public."ScenarioModelResult_Id_seq"', 9660, true);
            public       postgres    false    275            Y           0    0 &   ScenarioResultSummarizationType_Id_seq    SEQUENCE SET     W   SELECT pg_catalog.setval('public."ScenarioResultSummarizationType_Id_seq"', 1, false);
            public       postgres    false    243            Z           0    0    ScenarioType_Id_seq    SEQUENCE SET     D   SELECT pg_catalog.setval('public."ScenarioType_Id_seq"', 1, false);
            public       postgres    false    245            [           0    0    Scenario_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."Scenario_Id_seq"', 2, true);
            public       postgres    false    261            \           0    0    SmallDam_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."SmallDam_Id_seq"', 1, false);
            public       postgres    false    331            ]           0    0     SolutionLegalSubDivisions_Id_seq    SEQUENCE SET     R   SELECT pg_catalog.setval('public."SolutionLegalSubDivisions_Id_seq"', 193, true);
            public       postgres    false    345            ^           0    0    SolutionModelComponents_Id_seq    SEQUENCE SET     O   SELECT pg_catalog.setval('public."SolutionModelComponents_Id_seq"', 49, true);
            public       postgres    false    347            _           0    0    SolutionParcels_Id_seq    SEQUENCE SET     G   SELECT pg_catalog.setval('public."SolutionParcels_Id_seq"', 89, true);
            public       postgres    false    349            `           0    0    Solution_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."Solution_Id_seq"', 65, true);
            public       postgres    false    301            a           0    0    SubArea_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."SubArea_Id_seq"', 29, true);
            public       postgres    false    293            b           0    0    SubWatershed_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public."SubWatershed_Id_seq"', 2, true);
            public       postgres    false    263            c           0    0    Subbasin_Id_seq    SEQUENCE SET     ?   SELECT pg_catalog.setval('public."Subbasin_Id_seq"', 6, true);
            public       postgres    false    279            d           0    0     UnitScenarioEffectiveness_Id_seq    SEQUENCE SET     T   SELECT pg_catalog.setval('public."UnitScenarioEffectiveness_Id_seq"', 39140, true);
            public       postgres    false    289            e           0    0    UnitScenario_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('public."UnitScenario_Id_seq"', 188, true);
            public       postgres    false    277            f           0    0    UnitType_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."UnitType_Id_seq"', 1, false);
            public       postgres    false    247            g           0    0    UserMunicipalities_Id_seq    SEQUENCE SET     I   SELECT pg_catalog.setval('public."UserMunicipalities_Id_seq"', 3, true);
            public       postgres    false    283            h           0    0    UserParcels_Id_seq    SEQUENCE SET     C   SELECT pg_catalog.setval('public."UserParcels_Id_seq"', 13, true);
            public       postgres    false    285            i           0    0    UserType_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."UserType_Id_seq"', 1, false);
            public       postgres    false    249            j           0    0    UserWatersheds_Id_seq    SEQUENCE SET     E   SELECT pg_catalog.setval('public."UserWatersheds_Id_seq"', 3, true);
            public       postgres    false    287            k           0    0    User_Id_seq    SEQUENCE SET     ;   SELECT pg_catalog.setval('public."User_Id_seq"', 3, true);
            public       postgres    false    265            l           0    0    VegetativeFilterStrip_Id_seq    SEQUENCE SET     M   SELECT pg_catalog.setval('public."VegetativeFilterStrip_Id_seq"', 11, true);
            public       postgres    false    333            m           0    0    Wascob_Id_seq    SEQUENCE SET     >   SELECT pg_catalog.setval('public."Wascob_Id_seq"', 1, false);
            public       postgres    false    335            n           0    0    WatershedExistingBMPType_Id_seq    SEQUENCE SET     O   SELECT pg_catalog.setval('public."WatershedExistingBMPType_Id_seq"', 6, true);
            public       postgres    false    273            o           0    0    Watershed_Id_seq    SEQUENCE SET     @   SELECT pg_catalog.setval('public."Watershed_Id_seq"', 1, true);
            public       postgres    false    251            K           2606    274571    AnimalType PK_AnimalType 
   CONSTRAINT     \   ALTER TABLE ONLY public."AnimalType"
    ADD CONSTRAINT "PK_AnimalType" PRIMARY KEY ("Id");
 F   ALTER TABLE ONLY public."AnimalType" DROP CONSTRAINT "PK_AnimalType";
       public         postgres    false    214            �           2606    274920 0   BMPCombinationBMPTypes PK_BMPCombinationBMPTypes 
   CONSTRAINT     t   ALTER TABLE ONLY public."BMPCombinationBMPTypes"
    ADD CONSTRAINT "PK_BMPCombinationBMPTypes" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."BMPCombinationBMPTypes" DROP CONSTRAINT "PK_BMPCombinationBMPTypes";
       public         postgres    false    268            M           2606    274582 (   BMPCombinationType PK_BMPCombinationType 
   CONSTRAINT     l   ALTER TABLE ONLY public."BMPCombinationType"
    ADD CONSTRAINT "PK_BMPCombinationType" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."BMPCombinationType" DROP CONSTRAINT "PK_BMPCombinationType";
       public         postgres    false    216            O           2606    274593 <   BMPEffectivenessLocationType PK_BMPEffectivenessLocationType 
   CONSTRAINT     �   ALTER TABLE ONLY public."BMPEffectivenessLocationType"
    ADD CONSTRAINT "PK_BMPEffectivenessLocationType" PRIMARY KEY ("Id");
 j   ALTER TABLE ONLY public."BMPEffectivenessLocationType" DROP CONSTRAINT "PK_BMPEffectivenessLocationType";
       public         postgres    false    218            �           2606    274941 ,   BMPEffectivenessType PK_BMPEffectivenessType 
   CONSTRAINT     p   ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "PK_BMPEffectivenessType" PRIMARY KEY ("Id");
 Z   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "PK_BMPEffectivenessType";
       public         postgres    false    270            w           2606    274807    BMPType PK_BMPType 
   CONSTRAINT     V   ALTER TABLE ONLY public."BMPType"
    ADD CONSTRAINT "PK_BMPType" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."BMPType" DROP CONSTRAINT "PK_BMPType";
       public         postgres    false    256            �           2606    275311    CatchBasin PK_CatchBasin 
   CONSTRAINT     \   ALTER TABLE ONLY public."CatchBasin"
    ADD CONSTRAINT "PK_CatchBasin" PRIMARY KEY ("Id");
 F   ALTER TABLE ONLY public."CatchBasin" DROP CONSTRAINT "PK_CatchBasin";
       public         postgres    false    304            �           2606    275337    ClosedDrain PK_ClosedDrain 
   CONSTRAINT     ^   ALTER TABLE ONLY public."ClosedDrain"
    ADD CONSTRAINT "PK_ClosedDrain" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."ClosedDrain" DROP CONSTRAINT "PK_ClosedDrain";
       public         postgres    false    306            Q           2606    274604    Country PK_Country 
   CONSTRAINT     V   ALTER TABLE ONLY public."Country"
    ADD CONSTRAINT "PK_Country" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."Country" DROP CONSTRAINT "PK_Country";
       public         postgres    false    220            �           2606    275363    Dugout PK_Dugout 
   CONSTRAINT     T   ALTER TABLE ONLY public."Dugout"
    ADD CONSTRAINT "PK_Dugout" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Dugout" DROP CONSTRAINT "PK_Dugout";
       public         postgres    false    308            S           2606    274615    Farm PK_Farm 
   CONSTRAINT     P   ALTER TABLE ONLY public."Farm"
    ADD CONSTRAINT "PK_Farm" PRIMARY KEY ("Id");
 :   ALTER TABLE ONLY public."Farm" DROP CONSTRAINT "PK_Farm";
       public         postgres    false    222            �           2606    275394    Feedlot PK_Feedlot 
   CONSTRAINT     V   ALTER TABLE ONLY public."Feedlot"
    ADD CONSTRAINT "PK_Feedlot" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."Feedlot" DROP CONSTRAINT "PK_Feedlot";
       public         postgres    false    310            �           2606    275425    FlowDiversion PK_FlowDiversion 
   CONSTRAINT     b   ALTER TABLE ONLY public."FlowDiversion"
    ADD CONSTRAINT "PK_FlowDiversion" PRIMARY KEY ("Id");
 L   ALTER TABLE ONLY public."FlowDiversion" DROP CONSTRAINT "PK_FlowDiversion";
       public         postgres    false    312            U           2606    274626 (   GeometryLayerStyle PK_GeometryLayerStyle 
   CONSTRAINT     l   ALTER TABLE ONLY public."GeometryLayerStyle"
    ADD CONSTRAINT "PK_GeometryLayerStyle" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."GeometryLayerStyle" DROP CONSTRAINT "PK_GeometryLayerStyle";
       public         postgres    false    224            �           2606    275451 "   GrassedWaterway PK_GrassedWaterway 
   CONSTRAINT     f   ALTER TABLE ONLY public."GrassedWaterway"
    ADD CONSTRAINT "PK_GrassedWaterway" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."GrassedWaterway" DROP CONSTRAINT "PK_GrassedWaterway";
       public         postgres    false    314            W           2606    274637    Investor PK_Investor 
   CONSTRAINT     X   ALTER TABLE ONLY public."Investor"
    ADD CONSTRAINT "PK_Investor" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."Investor" DROP CONSTRAINT "PK_Investor";
       public         postgres    false    226            �           2606    275477 "   IsolatedWetland PK_IsolatedWetland 
   CONSTRAINT     f   ALTER TABLE ONLY public."IsolatedWetland"
    ADD CONSTRAINT "PK_IsolatedWetland" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."IsolatedWetland" DROP CONSTRAINT "PK_IsolatedWetland";
       public         postgres    false    316                       2606    275503    Lake PK_Lake 
   CONSTRAINT     P   ALTER TABLE ONLY public."Lake"
    ADD CONSTRAINT "PK_Lake" PRIMARY KEY ("Id");
 :   ALTER TABLE ONLY public."Lake" DROP CONSTRAINT "PK_Lake";
       public         postgres    false    318            Y           2606    274648 $   LegalSubDivision PK_LegalSubDivision 
   CONSTRAINT     h   ALTER TABLE ONLY public."LegalSubDivision"
    ADD CONSTRAINT "PK_LegalSubDivision" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."LegalSubDivision" DROP CONSTRAINT "PK_LegalSubDivision";
       public         postgres    false    228            	           2606    275529    ManureStorage PK_ManureStorage 
   CONSTRAINT     b   ALTER TABLE ONLY public."ManureStorage"
    ADD CONSTRAINT "PK_ManureStorage" PRIMARY KEY ("Id");
 L   ALTER TABLE ONLY public."ManureStorage" DROP CONSTRAINT "PK_ManureStorage";
       public         postgres    false    320                       2606    274844     ModelComponent PK_ModelComponent 
   CONSTRAINT     d   ALTER TABLE ONLY public."ModelComponent"
    ADD CONSTRAINT "PK_ModelComponent" PRIMARY KEY ("Id");
 N   ALTER TABLE ONLY public."ModelComponent" DROP CONSTRAINT "PK_ModelComponent";
       public         postgres    false    260            �           2606    274974 0   ModelComponentBMPTypes PK_ModelComponentBMPTypes 
   CONSTRAINT     t   ALTER TABLE ONLY public."ModelComponentBMPTypes"
    ADD CONSTRAINT "PK_ModelComponentBMPTypes" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."ModelComponentBMPTypes" DROP CONSTRAINT "PK_ModelComponentBMPTypes";
       public         postgres    false    272            [           2606    274659 (   ModelComponentType PK_ModelComponentType 
   CONSTRAINT     l   ALTER TABLE ONLY public."ModelComponentType"
    ADD CONSTRAINT "PK_ModelComponentType" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."ModelComponentType" DROP CONSTRAINT "PK_ModelComponentType";
       public         postgres    false    230            ]           2606    274670    Municipality PK_Municipality 
   CONSTRAINT     `   ALTER TABLE ONLY public."Municipality"
    ADD CONSTRAINT "PK_Municipality" PRIMARY KEY ("Id");
 J   ALTER TABLE ONLY public."Municipality" DROP CONSTRAINT "PK_Municipality";
       public         postgres    false    232            �           2606    275241    Optimization PK_Optimization 
   CONSTRAINT     `   ALTER TABLE ONLY public."Optimization"
    ADD CONSTRAINT "PK_Optimization" PRIMARY KEY ("Id");
 J   ALTER TABLE ONLY public."Optimization" DROP CONSTRAINT "PK_Optimization";
       public         postgres    false    296            _           2606    274681 B   OptimizationConstraintValueType PK_OptimizationConstraintValueType 
   CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationConstraintValueType"
    ADD CONSTRAINT "PK_OptimizationConstraintValueType" PRIMARY KEY ("Id");
 p   ALTER TABLE ONLY public."OptimizationConstraintValueType" DROP CONSTRAINT "PK_OptimizationConstraintValueType";
       public         postgres    false    234            6           2606    275763 2   OptimizationConstraints PK_OptimizationConstraints 
   CONSTRAINT     v   ALTER TABLE ONLY public."OptimizationConstraints"
    ADD CONSTRAINT "PK_OptimizationConstraints" PRIMARY KEY ("Id");
 `   ALTER TABLE ONLY public."OptimizationConstraints" DROP CONSTRAINT "PK_OptimizationConstraints";
       public         postgres    false    338            ;           2606    275786 >   OptimizationLegalSubDivisions PK_OptimizationLegalSubDivisions 
   CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions"
    ADD CONSTRAINT "PK_OptimizationLegalSubDivisions" PRIMARY KEY ("Id");
 l   ALTER TABLE ONLY public."OptimizationLegalSubDivisions" DROP CONSTRAINT "PK_OptimizationLegalSubDivisions";
       public         postgres    false    340            @           2606    275809 *   OptimizationParcels PK_OptimizationParcels 
   CONSTRAINT     n   ALTER TABLE ONLY public."OptimizationParcels"
    ADD CONSTRAINT "PK_OptimizationParcels" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."OptimizationParcels" DROP CONSTRAINT "PK_OptimizationParcels";
       public         postgres    false    342            a           2606    274692 $   OptimizationType PK_OptimizationType 
   CONSTRAINT     h   ALTER TABLE ONLY public."OptimizationType"
    ADD CONSTRAINT "PK_OptimizationType" PRIMARY KEY ("Id");
 R   ALTER TABLE ONLY public."OptimizationType" DROP CONSTRAINT "PK_OptimizationType";
       public         postgres    false    236            D           2606    275832 *   OptimizationWeights PK_OptimizationWeights 
   CONSTRAINT     n   ALTER TABLE ONLY public."OptimizationWeights"
    ADD CONSTRAINT "PK_OptimizationWeights" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."OptimizationWeights" DROP CONSTRAINT "PK_OptimizationWeights";
       public         postgres    false    344            c           2606    274703    Parcel PK_Parcel 
   CONSTRAINT     T   ALTER TABLE ONLY public."Parcel"
    ADD CONSTRAINT "PK_Parcel" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Parcel" DROP CONSTRAINT "PK_Parcel";
       public         postgres    false    238                       2606    275555    PointSource PK_PointSource 
   CONSTRAINT     ^   ALTER TABLE ONLY public."PointSource"
    ADD CONSTRAINT "PK_PointSource" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."PointSource" DROP CONSTRAINT "PK_PointSource";
       public         postgres    false    322            �           2606    275088    Project PK_Project 
   CONSTRAINT     V   ALTER TABLE ONLY public."Project"
    ADD CONSTRAINT "PK_Project" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."Project" DROP CONSTRAINT "PK_Project";
       public         postgres    false    282            �           2606    275259 .   ProjectMunicipalities PK_ProjectMunicipalities 
   CONSTRAINT     r   ALTER TABLE ONLY public."ProjectMunicipalities"
    ADD CONSTRAINT "PK_ProjectMunicipalities" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."ProjectMunicipalities" DROP CONSTRAINT "PK_ProjectMunicipalities";
       public         postgres    false    298            e           2606    274714 0   ProjectSpatialUnitType PK_ProjectSpatialUnitType 
   CONSTRAINT     t   ALTER TABLE ONLY public."ProjectSpatialUnitType"
    ADD CONSTRAINT "PK_ProjectSpatialUnitType" PRIMARY KEY ("Id");
 ^   ALTER TABLE ONLY public."ProjectSpatialUnitType" DROP CONSTRAINT "PK_ProjectSpatialUnitType";
       public         postgres    false    240            �           2606    275277 &   ProjectWatersheds PK_ProjectWatersheds 
   CONSTRAINT     j   ALTER TABLE ONLY public."ProjectWatersheds"
    ADD CONSTRAINT "PK_ProjectWatersheds" PRIMARY KEY ("Id");
 T   ALTER TABLE ONLY public."ProjectWatersheds" DROP CONSTRAINT "PK_ProjectWatersheds";
       public         postgres    false    300            t           2606    274791    Province PK_Province 
   CONSTRAINT     X   ALTER TABLE ONLY public."Province"
    ADD CONSTRAINT "PK_Province" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."Province" DROP CONSTRAINT "PK_Province";
       public         postgres    false    254            �           2606    275189    Reach PK_Reach 
   CONSTRAINT     R   ALTER TABLE ONLY public."Reach"
    ADD CONSTRAINT "PK_Reach" PRIMARY KEY ("Id");
 <   ALTER TABLE ONLY public."Reach" DROP CONSTRAINT "PK_Reach";
       public         postgres    false    292                       2606    275581    Reservoir PK_Reservoir 
   CONSTRAINT     Z   ALTER TABLE ONLY public."Reservoir"
    ADD CONSTRAINT "PK_Reservoir" PRIMARY KEY ("Id");
 D   ALTER TABLE ONLY public."Reservoir" DROP CONSTRAINT "PK_Reservoir";
       public         postgres    false    324                       2606    275607     RiparianBuffer PK_RiparianBuffer 
   CONSTRAINT     d   ALTER TABLE ONLY public."RiparianBuffer"
    ADD CONSTRAINT "PK_RiparianBuffer" PRIMARY KEY ("Id");
 N   ALTER TABLE ONLY public."RiparianBuffer" DROP CONSTRAINT "PK_RiparianBuffer";
       public         postgres    false    326                       2606    275633 "   RiparianWetland PK_RiparianWetland 
   CONSTRAINT     f   ALTER TABLE ONLY public."RiparianWetland"
    ADD CONSTRAINT "PK_RiparianWetland" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."RiparianWetland" DROP CONSTRAINT "PK_RiparianWetland";
       public         postgres    false    328            "           2606    275659    RockChute PK_RockChute 
   CONSTRAINT     Z   ALTER TABLE ONLY public."RockChute"
    ADD CONSTRAINT "PK_RockChute" PRIMARY KEY ("Id");
 D   ALTER TABLE ONLY public."RockChute" DROP CONSTRAINT "PK_RockChute";
       public         postgres    false    330            �           2606    274865    Scenario PK_Scenario 
   CONSTRAINT     X   ALTER TABLE ONLY public."Scenario"
    ADD CONSTRAINT "PK_Scenario" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."Scenario" DROP CONSTRAINT "PK_Scenario";
       public         postgres    false    262            �           2606    275023 *   ScenarioModelResult PK_ScenarioModelResult 
   CONSTRAINT     n   ALTER TABLE ONLY public."ScenarioModelResult"
    ADD CONSTRAINT "PK_ScenarioModelResult" PRIMARY KEY ("Id");
 X   ALTER TABLE ONLY public."ScenarioModelResult" DROP CONSTRAINT "PK_ScenarioModelResult";
       public         postgres    false    276            {           2606    274823 2   ScenarioModelResultType PK_ScenarioModelResultType 
   CONSTRAINT     v   ALTER TABLE ONLY public."ScenarioModelResultType"
    ADD CONSTRAINT "PK_ScenarioModelResultType" PRIMARY KEY ("Id");
 `   ALTER TABLE ONLY public."ScenarioModelResultType" DROP CONSTRAINT "PK_ScenarioModelResultType";
       public         postgres    false    258            g           2606    274725 B   ScenarioModelResultVariableType PK_ScenarioModelResultVariableType 
   CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioModelResultVariableType"
    ADD CONSTRAINT "PK_ScenarioModelResultVariableType" PRIMARY KEY ("Id");
 p   ALTER TABLE ONLY public."ScenarioModelResultVariableType" DROP CONSTRAINT "PK_ScenarioModelResultVariableType";
       public         postgres    false    242            i           2606    274736 B   ScenarioResultSummarizationType PK_ScenarioResultSummarizationType 
   CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioResultSummarizationType"
    ADD CONSTRAINT "PK_ScenarioResultSummarizationType" PRIMARY KEY ("Id");
 p   ALTER TABLE ONLY public."ScenarioResultSummarizationType" DROP CONSTRAINT "PK_ScenarioResultSummarizationType";
       public         postgres    false    244            k           2606    274747    ScenarioType PK_ScenarioType 
   CONSTRAINT     `   ALTER TABLE ONLY public."ScenarioType"
    ADD CONSTRAINT "PK_ScenarioType" PRIMARY KEY ("Id");
 J   ALTER TABLE ONLY public."ScenarioType" DROP CONSTRAINT "PK_ScenarioType";
       public         postgres    false    246            '           2606    275685    SmallDam PK_SmallDam 
   CONSTRAINT     X   ALTER TABLE ONLY public."SmallDam"
    ADD CONSTRAINT "PK_SmallDam" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."SmallDam" DROP CONSTRAINT "PK_SmallDam";
       public         postgres    false    332            �           2606    275295    Solution PK_Solution 
   CONSTRAINT     X   ALTER TABLE ONLY public."Solution"
    ADD CONSTRAINT "PK_Solution" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."Solution" DROP CONSTRAINT "PK_Solution";
       public         postgres    false    302            I           2606    275850 6   SolutionLegalSubDivisions PK_SolutionLegalSubDivisions 
   CONSTRAINT     z   ALTER TABLE ONLY public."SolutionLegalSubDivisions"
    ADD CONSTRAINT "PK_SolutionLegalSubDivisions" PRIMARY KEY ("Id");
 d   ALTER TABLE ONLY public."SolutionLegalSubDivisions" DROP CONSTRAINT "PK_SolutionLegalSubDivisions";
       public         postgres    false    346            N           2606    275873 2   SolutionModelComponents PK_SolutionModelComponents 
   CONSTRAINT     v   ALTER TABLE ONLY public."SolutionModelComponents"
    ADD CONSTRAINT "PK_SolutionModelComponents" PRIMARY KEY ("Id");
 `   ALTER TABLE ONLY public."SolutionModelComponents" DROP CONSTRAINT "PK_SolutionModelComponents";
       public         postgres    false    348            S           2606    275896 "   SolutionParcels PK_SolutionParcels 
   CONSTRAINT     f   ALTER TABLE ONLY public."SolutionParcels"
    ADD CONSTRAINT "PK_SolutionParcels" PRIMARY KEY ("Id");
 P   ALTER TABLE ONLY public."SolutionParcels" DROP CONSTRAINT "PK_SolutionParcels";
       public         postgres    false    350            �           2606    275210    SubArea PK_SubArea 
   CONSTRAINT     V   ALTER TABLE ONLY public."SubArea"
    ADD CONSTRAINT "PK_SubArea" PRIMARY KEY ("Id");
 @   ALTER TABLE ONLY public."SubArea" DROP CONSTRAINT "PK_SubArea";
       public         postgres    false    294            �           2606    274886    SubWatershed PK_SubWatershed 
   CONSTRAINT     `   ALTER TABLE ONLY public."SubWatershed"
    ADD CONSTRAINT "PK_SubWatershed" PRIMARY KEY ("Id");
 J   ALTER TABLE ONLY public."SubWatershed" DROP CONSTRAINT "PK_SubWatershed";
       public         postgres    false    264            �           2606    275072    Subbasin PK_Subbasin 
   CONSTRAINT     X   ALTER TABLE ONLY public."Subbasin"
    ADD CONSTRAINT "PK_Subbasin" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."Subbasin" DROP CONSTRAINT "PK_Subbasin";
       public         postgres    false    280            �           2606    275046    UnitScenario PK_UnitScenario 
   CONSTRAINT     `   ALTER TABLE ONLY public."UnitScenario"
    ADD CONSTRAINT "PK_UnitScenario" PRIMARY KEY ("Id");
 J   ALTER TABLE ONLY public."UnitScenario" DROP CONSTRAINT "PK_UnitScenario";
       public         postgres    false    278            �           2606    275168 6   UnitScenarioEffectiveness PK_UnitScenarioEffectiveness 
   CONSTRAINT     z   ALTER TABLE ONLY public."UnitScenarioEffectiveness"
    ADD CONSTRAINT "PK_UnitScenarioEffectiveness" PRIMARY KEY ("Id");
 d   ALTER TABLE ONLY public."UnitScenarioEffectiveness" DROP CONSTRAINT "PK_UnitScenarioEffectiveness";
       public         postgres    false    290            m           2606    274758    UnitType PK_UnitType 
   CONSTRAINT     X   ALTER TABLE ONLY public."UnitType"
    ADD CONSTRAINT "PK_UnitType" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."UnitType" DROP CONSTRAINT "PK_UnitType";
       public         postgres    false    248            �           2606    274902    User PK_User 
   CONSTRAINT     P   ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "PK_User" PRIMARY KEY ("Id");
 :   ALTER TABLE ONLY public."User" DROP CONSTRAINT "PK_User";
       public         postgres    false    266            �           2606    275111 (   UserMunicipalities PK_UserMunicipalities 
   CONSTRAINT     l   ALTER TABLE ONLY public."UserMunicipalities"
    ADD CONSTRAINT "PK_UserMunicipalities" PRIMARY KEY ("Id");
 V   ALTER TABLE ONLY public."UserMunicipalities" DROP CONSTRAINT "PK_UserMunicipalities";
       public         postgres    false    284            �           2606    275129    UserParcels PK_UserParcels 
   CONSTRAINT     ^   ALTER TABLE ONLY public."UserParcels"
    ADD CONSTRAINT "PK_UserParcels" PRIMARY KEY ("Id");
 H   ALTER TABLE ONLY public."UserParcels" DROP CONSTRAINT "PK_UserParcels";
       public         postgres    false    286            o           2606    274769    UserType PK_UserType 
   CONSTRAINT     X   ALTER TABLE ONLY public."UserType"
    ADD CONSTRAINT "PK_UserType" PRIMARY KEY ("Id");
 B   ALTER TABLE ONLY public."UserType" DROP CONSTRAINT "PK_UserType";
       public         postgres    false    250            �           2606    275147     UserWatersheds PK_UserWatersheds 
   CONSTRAINT     d   ALTER TABLE ONLY public."UserWatersheds"
    ADD CONSTRAINT "PK_UserWatersheds" PRIMARY KEY ("Id");
 N   ALTER TABLE ONLY public."UserWatersheds" DROP CONSTRAINT "PK_UserWatersheds";
       public         postgres    false    288            ,           2606    275711 .   VegetativeFilterStrip PK_VegetativeFilterStrip 
   CONSTRAINT     r   ALTER TABLE ONLY public."VegetativeFilterStrip"
    ADD CONSTRAINT "PK_VegetativeFilterStrip" PRIMARY KEY ("Id");
 \   ALTER TABLE ONLY public."VegetativeFilterStrip" DROP CONSTRAINT "PK_VegetativeFilterStrip";
       public         postgres    false    334            1           2606    275737    Wascob PK_Wascob 
   CONSTRAINT     T   ALTER TABLE ONLY public."Wascob"
    ADD CONSTRAINT "PK_Wascob" PRIMARY KEY ("Id");
 >   ALTER TABLE ONLY public."Wascob" DROP CONSTRAINT "PK_Wascob";
       public         postgres    false    336            q           2606    274780    Watershed PK_Watershed 
   CONSTRAINT     Z   ALTER TABLE ONLY public."Watershed"
    ADD CONSTRAINT "PK_Watershed" PRIMARY KEY ("Id");
 D   ALTER TABLE ONLY public."Watershed" DROP CONSTRAINT "PK_Watershed";
       public         postgres    false    252            �           2606    274992 4   WatershedExistingBMPType PK_WatershedExistingBMPType 
   CONSTRAINT     x   ALTER TABLE ONLY public."WatershedExistingBMPType"
    ADD CONSTRAINT "PK_WatershedExistingBMPType" PRIMARY KEY ("Id");
 b   ALTER TABLE ONLY public."WatershedExistingBMPType" DROP CONSTRAINT "PK_WatershedExistingBMPType";
       public         postgres    false    274            G           2606    272982 .   __EFMigrationsHistory PK___EFMigrationsHistory 
   CONSTRAINT     {   ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");
 \   ALTER TABLE ONLY public."__EFMigrationsHistory" DROP CONSTRAINT "PK___EFMigrationsHistory";
       public         postgres    false    197            �           1259    275912 .   IX_BMPCombinationBMPTypes_BMPCombinationTypeId    INDEX     �   CREATE INDEX "IX_BMPCombinationBMPTypes_BMPCombinationTypeId" ON public."BMPCombinationBMPTypes" USING btree ("BMPCombinationTypeId");
 D   DROP INDEX public."IX_BMPCombinationBMPTypes_BMPCombinationTypeId";
       public         postgres    false    268            �           1259    275913 #   IX_BMPCombinationBMPTypes_BMPTypeId    INDEX     q   CREATE INDEX "IX_BMPCombinationBMPTypes_BMPTypeId" ON public."BMPCombinationBMPTypes" USING btree ("BMPTypeId");
 9   DROP INDEX public."IX_BMPCombinationBMPTypes_BMPTypeId";
       public         postgres    false    268            �           1259    275914 6   IX_BMPEffectivenessType_BMPEffectivenessLocationTypeId    INDEX     �   CREATE INDEX "IX_BMPEffectivenessType_BMPEffectivenessLocationTypeId" ON public."BMPEffectivenessType" USING btree ("BMPEffectivenessLocationTypeId");
 L   DROP INDEX public."IX_BMPEffectivenessType_BMPEffectivenessLocationTypeId";
       public         postgres    false    270            �           1259    275915 /   IX_BMPEffectivenessType_DefaultConstraintTypeId    INDEX     �   CREATE INDEX "IX_BMPEffectivenessType_DefaultConstraintTypeId" ON public."BMPEffectivenessType" USING btree ("DefaultConstraintTypeId");
 E   DROP INDEX public."IX_BMPEffectivenessType_DefaultConstraintTypeId";
       public         postgres    false    270            �           1259    275916 1   IX_BMPEffectivenessType_ScenarioModelResultTypeId    INDEX     �   CREATE INDEX "IX_BMPEffectivenessType_ScenarioModelResultTypeId" ON public."BMPEffectivenessType" USING btree ("ScenarioModelResultTypeId");
 G   DROP INDEX public."IX_BMPEffectivenessType_ScenarioModelResultTypeId";
       public         postgres    false    270            �           1259    275917 9   IX_BMPEffectivenessType_ScenarioModelResultVariableTypeId    INDEX     �   CREATE INDEX "IX_BMPEffectivenessType_ScenarioModelResultVariableTypeId" ON public."BMPEffectivenessType" USING btree ("ScenarioModelResultVariableTypeId");
 O   DROP INDEX public."IX_BMPEffectivenessType_ScenarioModelResultVariableTypeId";
       public         postgres    false    270            �           1259    275918 "   IX_BMPEffectivenessType_UnitTypeId    INDEX     o   CREATE INDEX "IX_BMPEffectivenessType_UnitTypeId" ON public."BMPEffectivenessType" USING btree ("UnitTypeId");
 8   DROP INDEX public."IX_BMPEffectivenessType_UnitTypeId";
       public         postgres    false    270            u           1259    275919    IX_BMPType_ModelComponentTypeId    INDEX     i   CREATE INDEX "IX_BMPType_ModelComponentTypeId" ON public."BMPType" USING btree ("ModelComponentTypeId");
 5   DROP INDEX public."IX_BMPType_ModelComponentTypeId";
       public         postgres    false    256            �           1259    275920    IX_CatchBasin_ModelComponentId    INDEX     n   CREATE UNIQUE INDEX "IX_CatchBasin_ModelComponentId" ON public."CatchBasin" USING btree ("ModelComponentId");
 4   DROP INDEX public."IX_CatchBasin_ModelComponentId";
       public         postgres    false    304            �           1259    275921    IX_CatchBasin_ReachId    INDEX     U   CREATE INDEX "IX_CatchBasin_ReachId" ON public."CatchBasin" USING btree ("ReachId");
 +   DROP INDEX public."IX_CatchBasin_ReachId";
       public         postgres    false    304            �           1259    275922    IX_CatchBasin_SubAreaId    INDEX     Y   CREATE INDEX "IX_CatchBasin_SubAreaId" ON public."CatchBasin" USING btree ("SubAreaId");
 -   DROP INDEX public."IX_CatchBasin_SubAreaId";
       public         postgres    false    304            �           1259    275923    IX_ClosedDrain_ModelComponentId    INDEX     i   CREATE INDEX "IX_ClosedDrain_ModelComponentId" ON public."ClosedDrain" USING btree ("ModelComponentId");
 5   DROP INDEX public."IX_ClosedDrain_ModelComponentId";
       public         postgres    false    306            �           1259    275924    IX_ClosedDrain_ReachId    INDEX     W   CREATE INDEX "IX_ClosedDrain_ReachId" ON public."ClosedDrain" USING btree ("ReachId");
 ,   DROP INDEX public."IX_ClosedDrain_ReachId";
       public         postgres    false    306            �           1259    275925    IX_ClosedDrain_SubAreaId    INDEX     [   CREATE INDEX "IX_ClosedDrain_SubAreaId" ON public."ClosedDrain" USING btree ("SubAreaId");
 .   DROP INDEX public."IX_ClosedDrain_SubAreaId";
       public         postgres    false    306            �           1259    275926    IX_Dugout_AnimalTypeId    INDEX     W   CREATE INDEX "IX_Dugout_AnimalTypeId" ON public."Dugout" USING btree ("AnimalTypeId");
 ,   DROP INDEX public."IX_Dugout_AnimalTypeId";
       public         postgres    false    308            �           1259    275927    IX_Dugout_ModelComponentId    INDEX     f   CREATE UNIQUE INDEX "IX_Dugout_ModelComponentId" ON public."Dugout" USING btree ("ModelComponentId");
 0   DROP INDEX public."IX_Dugout_ModelComponentId";
       public         postgres    false    308            �           1259    275928    IX_Dugout_ReachId    INDEX     M   CREATE INDEX "IX_Dugout_ReachId" ON public."Dugout" USING btree ("ReachId");
 '   DROP INDEX public."IX_Dugout_ReachId";
       public         postgres    false    308            �           1259    275929    IX_Dugout_SubAreaId    INDEX     Q   CREATE INDEX "IX_Dugout_SubAreaId" ON public."Dugout" USING btree ("SubAreaId");
 )   DROP INDEX public."IX_Dugout_SubAreaId";
       public         postgres    false    308            �           1259    275930    IX_Feedlot_AnimalTypeId    INDEX     Y   CREATE INDEX "IX_Feedlot_AnimalTypeId" ON public."Feedlot" USING btree ("AnimalTypeId");
 -   DROP INDEX public."IX_Feedlot_AnimalTypeId";
       public         postgres    false    310            �           1259    275931    IX_Feedlot_ModelComponentId    INDEX     h   CREATE UNIQUE INDEX "IX_Feedlot_ModelComponentId" ON public."Feedlot" USING btree ("ModelComponentId");
 1   DROP INDEX public."IX_Feedlot_ModelComponentId";
       public         postgres    false    310            �           1259    275932    IX_Feedlot_ReachId    INDEX     O   CREATE INDEX "IX_Feedlot_ReachId" ON public."Feedlot" USING btree ("ReachId");
 (   DROP INDEX public."IX_Feedlot_ReachId";
       public         postgres    false    310            �           1259    275933    IX_Feedlot_SubAreaId    INDEX     S   CREATE INDEX "IX_Feedlot_SubAreaId" ON public."Feedlot" USING btree ("SubAreaId");
 *   DROP INDEX public."IX_Feedlot_SubAreaId";
       public         postgres    false    310            �           1259    275934 !   IX_FlowDiversion_ModelComponentId    INDEX     t   CREATE UNIQUE INDEX "IX_FlowDiversion_ModelComponentId" ON public."FlowDiversion" USING btree ("ModelComponentId");
 7   DROP INDEX public."IX_FlowDiversion_ModelComponentId";
       public         postgres    false    312            �           1259    275935    IX_FlowDiversion_ReachId    INDEX     [   CREATE INDEX "IX_FlowDiversion_ReachId" ON public."FlowDiversion" USING btree ("ReachId");
 .   DROP INDEX public."IX_FlowDiversion_ReachId";
       public         postgres    false    312            �           1259    275936    IX_FlowDiversion_SubAreaId    INDEX     _   CREATE INDEX "IX_FlowDiversion_SubAreaId" ON public."FlowDiversion" USING btree ("SubAreaId");
 0   DROP INDEX public."IX_FlowDiversion_SubAreaId";
       public         postgres    false    312            �           1259    275937 #   IX_GrassedWaterway_ModelComponentId    INDEX     x   CREATE UNIQUE INDEX "IX_GrassedWaterway_ModelComponentId" ON public."GrassedWaterway" USING btree ("ModelComponentId");
 9   DROP INDEX public."IX_GrassedWaterway_ModelComponentId";
       public         postgres    false    314            �           1259    275938    IX_GrassedWaterway_ReachId    INDEX     _   CREATE INDEX "IX_GrassedWaterway_ReachId" ON public."GrassedWaterway" USING btree ("ReachId");
 0   DROP INDEX public."IX_GrassedWaterway_ReachId";
       public         postgres    false    314            �           1259    275939    IX_GrassedWaterway_SubAreaId    INDEX     c   CREATE INDEX "IX_GrassedWaterway_SubAreaId" ON public."GrassedWaterway" USING btree ("SubAreaId");
 2   DROP INDEX public."IX_GrassedWaterway_SubAreaId";
       public         postgres    false    314            �           1259    275940 #   IX_IsolatedWetland_ModelComponentId    INDEX     x   CREATE UNIQUE INDEX "IX_IsolatedWetland_ModelComponentId" ON public."IsolatedWetland" USING btree ("ModelComponentId");
 9   DROP INDEX public."IX_IsolatedWetland_ModelComponentId";
       public         postgres    false    316            �           1259    275941    IX_IsolatedWetland_ReachId    INDEX     _   CREATE INDEX "IX_IsolatedWetland_ReachId" ON public."IsolatedWetland" USING btree ("ReachId");
 0   DROP INDEX public."IX_IsolatedWetland_ReachId";
       public         postgres    false    316            �           1259    275942    IX_IsolatedWetland_SubAreaId    INDEX     c   CREATE INDEX "IX_IsolatedWetland_SubAreaId" ON public."IsolatedWetland" USING btree ("SubAreaId");
 2   DROP INDEX public."IX_IsolatedWetland_SubAreaId";
       public         postgres    false    316                        1259    275943    IX_Lake_ModelComponentId    INDEX     b   CREATE UNIQUE INDEX "IX_Lake_ModelComponentId" ON public."Lake" USING btree ("ModelComponentId");
 .   DROP INDEX public."IX_Lake_ModelComponentId";
       public         postgres    false    318                       1259    275944    IX_Lake_ReachId    INDEX     I   CREATE INDEX "IX_Lake_ReachId" ON public."Lake" USING btree ("ReachId");
 %   DROP INDEX public."IX_Lake_ReachId";
       public         postgres    false    318                       1259    275945    IX_Lake_SubAreaId    INDEX     M   CREATE INDEX "IX_Lake_SubAreaId" ON public."Lake" USING btree ("SubAreaId");
 '   DROP INDEX public."IX_Lake_SubAreaId";
       public         postgres    false    318                       1259    275946 !   IX_ManureStorage_ModelComponentId    INDEX     t   CREATE UNIQUE INDEX "IX_ManureStorage_ModelComponentId" ON public."ManureStorage" USING btree ("ModelComponentId");
 7   DROP INDEX public."IX_ManureStorage_ModelComponentId";
       public         postgres    false    320                       1259    275947    IX_ManureStorage_ReachId    INDEX     [   CREATE INDEX "IX_ManureStorage_ReachId" ON public."ManureStorage" USING btree ("ReachId");
 .   DROP INDEX public."IX_ManureStorage_ReachId";
       public         postgres    false    320                       1259    275948    IX_ManureStorage_SubAreaId    INDEX     _   CREATE INDEX "IX_ManureStorage_SubAreaId" ON public."ManureStorage" USING btree ("SubAreaId");
 0   DROP INDEX public."IX_ManureStorage_SubAreaId";
       public         postgres    false    320            �           1259    275951 #   IX_ModelComponentBMPTypes_BMPTypeId    INDEX     q   CREATE INDEX "IX_ModelComponentBMPTypes_BMPTypeId" ON public."ModelComponentBMPTypes" USING btree ("BMPTypeId");
 9   DROP INDEX public."IX_ModelComponentBMPTypes_BMPTypeId";
       public         postgres    false    272            �           1259    275952 *   IX_ModelComponentBMPTypes_ModelComponentId    INDEX        CREATE INDEX "IX_ModelComponentBMPTypes_ModelComponentId" ON public."ModelComponentBMPTypes" USING btree ("ModelComponentId");
 @   DROP INDEX public."IX_ModelComponentBMPTypes_ModelComponentId";
       public         postgres    false    272            |           1259    275949 &   IX_ModelComponent_ModelComponentTypeId    INDEX     w   CREATE INDEX "IX_ModelComponent_ModelComponentTypeId" ON public."ModelComponent" USING btree ("ModelComponentTypeId");
 <   DROP INDEX public."IX_ModelComponent_ModelComponentTypeId";
       public         postgres    false    260            }           1259    275950    IX_ModelComponent_WatershedId    INDEX     e   CREATE INDEX "IX_ModelComponent_WatershedId" ON public."ModelComponent" USING btree ("WatershedId");
 3   DROP INDEX public."IX_ModelComponent_WatershedId";
       public         postgres    false    260            2           1259    275955 1   IX_OptimizationConstraints_BMPEffectivenessTypeId    INDEX     �   CREATE INDEX "IX_OptimizationConstraints_BMPEffectivenessTypeId" ON public."OptimizationConstraints" USING btree ("BMPEffectivenessTypeId");
 G   DROP INDEX public."IX_OptimizationConstraints_BMPEffectivenessTypeId";
       public         postgres    false    338            3           1259    275956 <   IX_OptimizationConstraints_OptimizationConstraintValueTypeId    INDEX     �   CREATE INDEX "IX_OptimizationConstraints_OptimizationConstraintValueTypeId" ON public."OptimizationConstraints" USING btree ("OptimizationConstraintValueTypeId");
 R   DROP INDEX public."IX_OptimizationConstraints_OptimizationConstraintValueTypeId";
       public         postgres    false    338            4           1259    275957 )   IX_OptimizationConstraints_OptimizationId    INDEX     }   CREATE INDEX "IX_OptimizationConstraints_OptimizationId" ON public."OptimizationConstraints" USING btree ("OptimizationId");
 ?   DROP INDEX public."IX_OptimizationConstraints_OptimizationId";
       public         postgres    false    338            7           1259    275958 *   IX_OptimizationLegalSubDivisions_BMPTypeId    INDEX        CREATE INDEX "IX_OptimizationLegalSubDivisions_BMPTypeId" ON public."OptimizationLegalSubDivisions" USING btree ("BMPTypeId");
 @   DROP INDEX public."IX_OptimizationLegalSubDivisions_BMPTypeId";
       public         postgres    false    340            8           1259    275959 3   IX_OptimizationLegalSubDivisions_LegalSubDivisionId    INDEX     �   CREATE INDEX "IX_OptimizationLegalSubDivisions_LegalSubDivisionId" ON public."OptimizationLegalSubDivisions" USING btree ("LegalSubDivisionId");
 I   DROP INDEX public."IX_OptimizationLegalSubDivisions_LegalSubDivisionId";
       public         postgres    false    340            9           1259    275960 /   IX_OptimizationLegalSubDivisions_OptimizationId    INDEX     �   CREATE INDEX "IX_OptimizationLegalSubDivisions_OptimizationId" ON public."OptimizationLegalSubDivisions" USING btree ("OptimizationId");
 E   DROP INDEX public."IX_OptimizationLegalSubDivisions_OptimizationId";
       public         postgres    false    340            <           1259    275961     IX_OptimizationParcels_BMPTypeId    INDEX     k   CREATE INDEX "IX_OptimizationParcels_BMPTypeId" ON public."OptimizationParcels" USING btree ("BMPTypeId");
 6   DROP INDEX public."IX_OptimizationParcels_BMPTypeId";
       public         postgres    false    342            =           1259    275962 %   IX_OptimizationParcels_OptimizationId    INDEX     u   CREATE INDEX "IX_OptimizationParcels_OptimizationId" ON public."OptimizationParcels" USING btree ("OptimizationId");
 ;   DROP INDEX public."IX_OptimizationParcels_OptimizationId";
       public         postgres    false    342            >           1259    275963    IX_OptimizationParcels_ParcelId    INDEX     i   CREATE INDEX "IX_OptimizationParcels_ParcelId" ON public."OptimizationParcels" USING btree ("ParcelId");
 5   DROP INDEX public."IX_OptimizationParcels_ParcelId";
       public         postgres    false    342            A           1259    275964 -   IX_OptimizationWeights_BMPEffectivenessTypeId    INDEX     �   CREATE INDEX "IX_OptimizationWeights_BMPEffectivenessTypeId" ON public."OptimizationWeights" USING btree ("BMPEffectivenessTypeId");
 C   DROP INDEX public."IX_OptimizationWeights_BMPEffectivenessTypeId";
       public         postgres    false    344            B           1259    275965 %   IX_OptimizationWeights_OptimizationId    INDEX     u   CREATE INDEX "IX_OptimizationWeights_OptimizationId" ON public."OptimizationWeights" USING btree ("OptimizationId");
 ;   DROP INDEX public."IX_OptimizationWeights_OptimizationId";
       public         postgres    false    344            �           1259    275953 "   IX_Optimization_OptimizationTypeId    INDEX     o   CREATE INDEX "IX_Optimization_OptimizationTypeId" ON public."Optimization" USING btree ("OptimizationTypeId");
 8   DROP INDEX public."IX_Optimization_OptimizationTypeId";
       public         postgres    false    296            �           1259    275954    IX_Optimization_ProjectId    INDEX     d   CREATE UNIQUE INDEX "IX_Optimization_ProjectId" ON public."Optimization" USING btree ("ProjectId");
 /   DROP INDEX public."IX_Optimization_ProjectId";
       public         postgres    false    296            
           1259    275966    IX_PointSource_ModelComponentId    INDEX     p   CREATE UNIQUE INDEX "IX_PointSource_ModelComponentId" ON public."PointSource" USING btree ("ModelComponentId");
 5   DROP INDEX public."IX_PointSource_ModelComponentId";
       public         postgres    false    322                       1259    275967    IX_PointSource_ReachId    INDEX     W   CREATE INDEX "IX_PointSource_ReachId" ON public."PointSource" USING btree ("ReachId");
 ,   DROP INDEX public."IX_PointSource_ReachId";
       public         postgres    false    322                       1259    275968    IX_PointSource_SubAreaId    INDEX     [   CREATE INDEX "IX_PointSource_SubAreaId" ON public."PointSource" USING btree ("SubAreaId");
 .   DROP INDEX public."IX_PointSource_SubAreaId";
       public         postgres    false    322            �           1259    275972 '   IX_ProjectMunicipalities_MunicipalityId    INDEX     y   CREATE INDEX "IX_ProjectMunicipalities_MunicipalityId" ON public."ProjectMunicipalities" USING btree ("MunicipalityId");
 =   DROP INDEX public."IX_ProjectMunicipalities_MunicipalityId";
       public         postgres    false    298            �           1259    275973 "   IX_ProjectMunicipalities_ProjectId    INDEX     o   CREATE INDEX "IX_ProjectMunicipalities_ProjectId" ON public."ProjectMunicipalities" USING btree ("ProjectId");
 8   DROP INDEX public."IX_ProjectMunicipalities_ProjectId";
       public         postgres    false    298            �           1259    275974    IX_ProjectWatersheds_ProjectId    INDEX     g   CREATE INDEX "IX_ProjectWatersheds_ProjectId" ON public."ProjectWatersheds" USING btree ("ProjectId");
 4   DROP INDEX public."IX_ProjectWatersheds_ProjectId";
       public         postgres    false    300            �           1259    275975     IX_ProjectWatersheds_WatershedId    INDEX     k   CREATE INDEX "IX_ProjectWatersheds_WatershedId" ON public."ProjectWatersheds" USING btree ("WatershedId");
 6   DROP INDEX public."IX_ProjectWatersheds_WatershedId";
       public         postgres    false    300            �           1259    275969 #   IX_Project_ProjectSpatialUnitTypeId    INDEX     q   CREATE INDEX "IX_Project_ProjectSpatialUnitTypeId" ON public."Project" USING btree ("ProjectSpatialUnitTypeId");
 9   DROP INDEX public."IX_Project_ProjectSpatialUnitTypeId";
       public         postgres    false    282            �           1259    275970    IX_Project_ScenarioTypeId    INDEX     ]   CREATE INDEX "IX_Project_ScenarioTypeId" ON public."Project" USING btree ("ScenarioTypeId");
 /   DROP INDEX public."IX_Project_ScenarioTypeId";
       public         postgres    false    282            �           1259    275971    IX_Project_UserId    INDEX     M   CREATE INDEX "IX_Project_UserId" ON public."Project" USING btree ("UserId");
 '   DROP INDEX public."IX_Project_UserId";
       public         postgres    false    282            r           1259    275976    IX_Province_CountryId    INDEX     U   CREATE INDEX "IX_Province_CountryId" ON public."Province" USING btree ("CountryId");
 +   DROP INDEX public."IX_Province_CountryId";
       public         postgres    false    254            �           1259    275977    IX_Reach_ModelComponentId    INDEX     d   CREATE UNIQUE INDEX "IX_Reach_ModelComponentId" ON public."Reach" USING btree ("ModelComponentId");
 /   DROP INDEX public."IX_Reach_ModelComponentId";
       public         postgres    false    292            �           1259    275978    IX_Reach_SubbasinId    INDEX     X   CREATE UNIQUE INDEX "IX_Reach_SubbasinId" ON public."Reach" USING btree ("SubbasinId");
 )   DROP INDEX public."IX_Reach_SubbasinId";
       public         postgres    false    292                       1259    275979    IX_Reservoir_ModelComponentId    INDEX     l   CREATE UNIQUE INDEX "IX_Reservoir_ModelComponentId" ON public."Reservoir" USING btree ("ModelComponentId");
 3   DROP INDEX public."IX_Reservoir_ModelComponentId";
       public         postgres    false    324                       1259    275980    IX_Reservoir_ReachId    INDEX     S   CREATE INDEX "IX_Reservoir_ReachId" ON public."Reservoir" USING btree ("ReachId");
 *   DROP INDEX public."IX_Reservoir_ReachId";
       public         postgres    false    324                       1259    275981    IX_Reservoir_SubAreaId    INDEX     W   CREATE INDEX "IX_Reservoir_SubAreaId" ON public."Reservoir" USING btree ("SubAreaId");
 ,   DROP INDEX public."IX_Reservoir_SubAreaId";
       public         postgres    false    324                       1259    275982 "   IX_RiparianBuffer_ModelComponentId    INDEX     v   CREATE UNIQUE INDEX "IX_RiparianBuffer_ModelComponentId" ON public."RiparianBuffer" USING btree ("ModelComponentId");
 8   DROP INDEX public."IX_RiparianBuffer_ModelComponentId";
       public         postgres    false    326                       1259    275983    IX_RiparianBuffer_ReachId    INDEX     ]   CREATE INDEX "IX_RiparianBuffer_ReachId" ON public."RiparianBuffer" USING btree ("ReachId");
 /   DROP INDEX public."IX_RiparianBuffer_ReachId";
       public         postgres    false    326                       1259    275984    IX_RiparianBuffer_SubAreaId    INDEX     a   CREATE INDEX "IX_RiparianBuffer_SubAreaId" ON public."RiparianBuffer" USING btree ("SubAreaId");
 1   DROP INDEX public."IX_RiparianBuffer_SubAreaId";
       public         postgres    false    326                       1259    275985 #   IX_RiparianWetland_ModelComponentId    INDEX     x   CREATE UNIQUE INDEX "IX_RiparianWetland_ModelComponentId" ON public."RiparianWetland" USING btree ("ModelComponentId");
 9   DROP INDEX public."IX_RiparianWetland_ModelComponentId";
       public         postgres    false    328                       1259    275986    IX_RiparianWetland_ReachId    INDEX     _   CREATE INDEX "IX_RiparianWetland_ReachId" ON public."RiparianWetland" USING btree ("ReachId");
 0   DROP INDEX public."IX_RiparianWetland_ReachId";
       public         postgres    false    328                       1259    275987    IX_RiparianWetland_SubAreaId    INDEX     c   CREATE INDEX "IX_RiparianWetland_SubAreaId" ON public."RiparianWetland" USING btree ("SubAreaId");
 2   DROP INDEX public."IX_RiparianWetland_SubAreaId";
       public         postgres    false    328                       1259    275988    IX_RockChute_ModelComponentId    INDEX     l   CREATE UNIQUE INDEX "IX_RockChute_ModelComponentId" ON public."RockChute" USING btree ("ModelComponentId");
 3   DROP INDEX public."IX_RockChute_ModelComponentId";
       public         postgres    false    330                       1259    275989    IX_RockChute_ReachId    INDEX     S   CREATE INDEX "IX_RockChute_ReachId" ON public."RockChute" USING btree ("ReachId");
 *   DROP INDEX public."IX_RockChute_ReachId";
       public         postgres    false    330                        1259    275990    IX_RockChute_SubAreaId    INDEX     W   CREATE INDEX "IX_RockChute_SubAreaId" ON public."RockChute" USING btree ("SubAreaId");
 ,   DROP INDEX public."IX_RockChute_SubAreaId";
       public         postgres    false    330            x           1259    275996 <   IX_ScenarioModelResultType_ScenarioModelResultVariableTypeId    INDEX     �   CREATE INDEX "IX_ScenarioModelResultType_ScenarioModelResultVariableTypeId" ON public."ScenarioModelResultType" USING btree ("ScenarioModelResultVariableTypeId");
 R   DROP INDEX public."IX_ScenarioModelResultType_ScenarioModelResultVariableTypeId";
       public         postgres    false    258            y           1259    275997 %   IX_ScenarioModelResultType_UnitTypeId    INDEX     u   CREATE INDEX "IX_ScenarioModelResultType_UnitTypeId" ON public."ScenarioModelResultType" USING btree ("UnitTypeId");
 ;   DROP INDEX public."IX_ScenarioModelResultType_UnitTypeId";
       public         postgres    false    258            �           1259    275993 '   IX_ScenarioModelResult_ModelComponentId    INDEX     y   CREATE INDEX "IX_ScenarioModelResult_ModelComponentId" ON public."ScenarioModelResult" USING btree ("ModelComponentId");
 =   DROP INDEX public."IX_ScenarioModelResult_ModelComponentId";
       public         postgres    false    276            �           1259    275994 !   IX_ScenarioModelResult_ScenarioId    INDEX     m   CREATE INDEX "IX_ScenarioModelResult_ScenarioId" ON public."ScenarioModelResult" USING btree ("ScenarioId");
 7   DROP INDEX public."IX_ScenarioModelResult_ScenarioId";
       public         postgres    false    276            �           1259    275995 0   IX_ScenarioModelResult_ScenarioModelResultTypeId    INDEX     �   CREATE INDEX "IX_ScenarioModelResult_ScenarioModelResultTypeId" ON public."ScenarioModelResult" USING btree ("ScenarioModelResultTypeId");
 F   DROP INDEX public."IX_ScenarioModelResult_ScenarioModelResultTypeId";
       public         postgres    false    276            �           1259    275991    IX_Scenario_ScenarioTypeId    INDEX     _   CREATE INDEX "IX_Scenario_ScenarioTypeId" ON public."Scenario" USING btree ("ScenarioTypeId");
 0   DROP INDEX public."IX_Scenario_ScenarioTypeId";
       public         postgres    false    262            �           1259    275992    IX_Scenario_WatershedId    INDEX     Y   CREATE INDEX "IX_Scenario_WatershedId" ON public."Scenario" USING btree ("WatershedId");
 -   DROP INDEX public."IX_Scenario_WatershedId";
       public         postgres    false    262            #           1259    275998    IX_SmallDam_ModelComponentId    INDEX     j   CREATE UNIQUE INDEX "IX_SmallDam_ModelComponentId" ON public."SmallDam" USING btree ("ModelComponentId");
 2   DROP INDEX public."IX_SmallDam_ModelComponentId";
       public         postgres    false    332            $           1259    275999    IX_SmallDam_ReachId    INDEX     Q   CREATE INDEX "IX_SmallDam_ReachId" ON public."SmallDam" USING btree ("ReachId");
 )   DROP INDEX public."IX_SmallDam_ReachId";
       public         postgres    false    332            %           1259    276000    IX_SmallDam_SubAreaId    INDEX     U   CREATE INDEX "IX_SmallDam_SubAreaId" ON public."SmallDam" USING btree ("SubAreaId");
 +   DROP INDEX public."IX_SmallDam_SubAreaId";
       public         postgres    false    332            E           1259    276002 &   IX_SolutionLegalSubDivisions_BMPTypeId    INDEX     w   CREATE INDEX "IX_SolutionLegalSubDivisions_BMPTypeId" ON public."SolutionLegalSubDivisions" USING btree ("BMPTypeId");
 <   DROP INDEX public."IX_SolutionLegalSubDivisions_BMPTypeId";
       public         postgres    false    346            F           1259    276003 /   IX_SolutionLegalSubDivisions_LegalSubDivisionId    INDEX     �   CREATE INDEX "IX_SolutionLegalSubDivisions_LegalSubDivisionId" ON public."SolutionLegalSubDivisions" USING btree ("LegalSubDivisionId");
 E   DROP INDEX public."IX_SolutionLegalSubDivisions_LegalSubDivisionId";
       public         postgres    false    346            G           1259    276004 '   IX_SolutionLegalSubDivisions_SolutionId    INDEX     y   CREATE INDEX "IX_SolutionLegalSubDivisions_SolutionId" ON public."SolutionLegalSubDivisions" USING btree ("SolutionId");
 =   DROP INDEX public."IX_SolutionLegalSubDivisions_SolutionId";
       public         postgres    false    346            J           1259    276005 $   IX_SolutionModelComponents_BMPTypeId    INDEX     s   CREATE INDEX "IX_SolutionModelComponents_BMPTypeId" ON public."SolutionModelComponents" USING btree ("BMPTypeId");
 :   DROP INDEX public."IX_SolutionModelComponents_BMPTypeId";
       public         postgres    false    348            K           1259    276006 +   IX_SolutionModelComponents_ModelComponentId    INDEX     �   CREATE INDEX "IX_SolutionModelComponents_ModelComponentId" ON public."SolutionModelComponents" USING btree ("ModelComponentId");
 A   DROP INDEX public."IX_SolutionModelComponents_ModelComponentId";
       public         postgres    false    348            L           1259    276007 %   IX_SolutionModelComponents_SolutionId    INDEX     u   CREATE INDEX "IX_SolutionModelComponents_SolutionId" ON public."SolutionModelComponents" USING btree ("SolutionId");
 ;   DROP INDEX public."IX_SolutionModelComponents_SolutionId";
       public         postgres    false    348            O           1259    276008    IX_SolutionParcels_BMPTypeId    INDEX     c   CREATE INDEX "IX_SolutionParcels_BMPTypeId" ON public."SolutionParcels" USING btree ("BMPTypeId");
 2   DROP INDEX public."IX_SolutionParcels_BMPTypeId";
       public         postgres    false    350            P           1259    276009    IX_SolutionParcels_ParcelId    INDEX     a   CREATE INDEX "IX_SolutionParcels_ParcelId" ON public."SolutionParcels" USING btree ("ParcelId");
 1   DROP INDEX public."IX_SolutionParcels_ParcelId";
       public         postgres    false    350            Q           1259    276010    IX_SolutionParcels_SolutionId    INDEX     e   CREATE INDEX "IX_SolutionParcels_SolutionId" ON public."SolutionParcels" USING btree ("SolutionId");
 3   DROP INDEX public."IX_SolutionParcels_SolutionId";
       public         postgres    false    350            �           1259    276001    IX_Solution_ProjectId    INDEX     \   CREATE UNIQUE INDEX "IX_Solution_ProjectId" ON public."Solution" USING btree ("ProjectId");
 +   DROP INDEX public."IX_Solution_ProjectId";
       public         postgres    false    302            �           1259    276011    IX_SubArea_LegalSubDivisionId    INDEX     e   CREATE INDEX "IX_SubArea_LegalSubDivisionId" ON public."SubArea" USING btree ("LegalSubDivisionId");
 3   DROP INDEX public."IX_SubArea_LegalSubDivisionId";
       public         postgres    false    294            �           1259    276012    IX_SubArea_ModelComponentId    INDEX     h   CREATE UNIQUE INDEX "IX_SubArea_ModelComponentId" ON public."SubArea" USING btree ("ModelComponentId");
 1   DROP INDEX public."IX_SubArea_ModelComponentId";
       public         postgres    false    294            �           1259    276013    IX_SubArea_ParcelId    INDEX     Q   CREATE INDEX "IX_SubArea_ParcelId" ON public."SubArea" USING btree ("ParcelId");
 )   DROP INDEX public."IX_SubArea_ParcelId";
       public         postgres    false    294            �           1259    276014    IX_SubArea_SubbasinId    INDEX     U   CREATE INDEX "IX_SubArea_SubbasinId" ON public."SubArea" USING btree ("SubbasinId");
 +   DROP INDEX public."IX_SubArea_SubbasinId";
       public         postgres    false    294            �           1259    276016    IX_SubWatershed_WatershedId    INDEX     a   CREATE INDEX "IX_SubWatershed_WatershedId" ON public."SubWatershed" USING btree ("WatershedId");
 1   DROP INDEX public."IX_SubWatershed_WatershedId";
       public         postgres    false    264            �           1259    276015    IX_Subbasin_SubWatershedId    INDEX     _   CREATE INDEX "IX_Subbasin_SubWatershedId" ON public."Subbasin" USING btree ("SubWatershedId");
 0   DROP INDEX public."IX_Subbasin_SubWatershedId";
       public         postgres    false    280            �           1259    276020 3   IX_UnitScenarioEffectiveness_BMPEffectivenessTypeId    INDEX     �   CREATE INDEX "IX_UnitScenarioEffectiveness_BMPEffectivenessTypeId" ON public."UnitScenarioEffectiveness" USING btree ("BMPEffectivenessTypeId");
 I   DROP INDEX public."IX_UnitScenarioEffectiveness_BMPEffectivenessTypeId";
       public         postgres    false    290            �           1259    276021 +   IX_UnitScenarioEffectiveness_UnitScenarioId    INDEX     �   CREATE INDEX "IX_UnitScenarioEffectiveness_UnitScenarioId" ON public."UnitScenarioEffectiveness" USING btree ("UnitScenarioId");
 A   DROP INDEX public."IX_UnitScenarioEffectiveness_UnitScenarioId";
       public         postgres    false    290            �           1259    276017     IX_UnitScenario_BMPCombinationId    INDEX     k   CREATE INDEX "IX_UnitScenario_BMPCombinationId" ON public."UnitScenario" USING btree ("BMPCombinationId");
 6   DROP INDEX public."IX_UnitScenario_BMPCombinationId";
       public         postgres    false    278            �           1259    276018     IX_UnitScenario_ModelComponentId    INDEX     k   CREATE INDEX "IX_UnitScenario_ModelComponentId" ON public."UnitScenario" USING btree ("ModelComponentId");
 6   DROP INDEX public."IX_UnitScenario_ModelComponentId";
       public         postgres    false    278            �           1259    276019    IX_UnitScenario_ScenarioId    INDEX     _   CREATE INDEX "IX_UnitScenario_ScenarioId" ON public."UnitScenario" USING btree ("ScenarioId");
 0   DROP INDEX public."IX_UnitScenario_ScenarioId";
       public         postgres    false    278            �           1259    276024 $   IX_UserMunicipalities_MunicipalityId    INDEX     s   CREATE INDEX "IX_UserMunicipalities_MunicipalityId" ON public."UserMunicipalities" USING btree ("MunicipalityId");
 :   DROP INDEX public."IX_UserMunicipalities_MunicipalityId";
       public         postgres    false    284            �           1259    276025    IX_UserMunicipalities_UserId    INDEX     c   CREATE INDEX "IX_UserMunicipalities_UserId" ON public."UserMunicipalities" USING btree ("UserId");
 2   DROP INDEX public."IX_UserMunicipalities_UserId";
       public         postgres    false    284            �           1259    276026    IX_UserParcels_ParcelId    INDEX     Y   CREATE INDEX "IX_UserParcels_ParcelId" ON public."UserParcels" USING btree ("ParcelId");
 -   DROP INDEX public."IX_UserParcels_ParcelId";
       public         postgres    false    286            �           1259    276027    IX_UserParcels_UserId    INDEX     U   CREATE INDEX "IX_UserParcels_UserId" ON public."UserParcels" USING btree ("UserId");
 +   DROP INDEX public."IX_UserParcels_UserId";
       public         postgres    false    286            �           1259    276028    IX_UserWatersheds_UserId    INDEX     [   CREATE INDEX "IX_UserWatersheds_UserId" ON public."UserWatersheds" USING btree ("UserId");
 .   DROP INDEX public."IX_UserWatersheds_UserId";
       public         postgres    false    288            �           1259    276029    IX_UserWatersheds_WatershedId    INDEX     e   CREATE INDEX "IX_UserWatersheds_WatershedId" ON public."UserWatersheds" USING btree ("WatershedId");
 3   DROP INDEX public."IX_UserWatersheds_WatershedId";
       public         postgres    false    288            �           1259    276022    IX_User_ProvinceId    INDEX     O   CREATE INDEX "IX_User_ProvinceId" ON public."User" USING btree ("ProvinceId");
 (   DROP INDEX public."IX_User_ProvinceId";
       public         postgres    false    266            �           1259    276023    IX_User_UserTypeId    INDEX     O   CREATE INDEX "IX_User_UserTypeId" ON public."User" USING btree ("UserTypeId");
 (   DROP INDEX public."IX_User_UserTypeId";
       public         postgres    false    266            (           1259    276030 )   IX_VegetativeFilterStrip_ModelComponentId    INDEX     �   CREATE UNIQUE INDEX "IX_VegetativeFilterStrip_ModelComponentId" ON public."VegetativeFilterStrip" USING btree ("ModelComponentId");
 ?   DROP INDEX public."IX_VegetativeFilterStrip_ModelComponentId";
       public         postgres    false    334            )           1259    276031     IX_VegetativeFilterStrip_ReachId    INDEX     k   CREATE INDEX "IX_VegetativeFilterStrip_ReachId" ON public."VegetativeFilterStrip" USING btree ("ReachId");
 6   DROP INDEX public."IX_VegetativeFilterStrip_ReachId";
       public         postgres    false    334            *           1259    276032 "   IX_VegetativeFilterStrip_SubAreaId    INDEX     o   CREATE INDEX "IX_VegetativeFilterStrip_SubAreaId" ON public."VegetativeFilterStrip" USING btree ("SubAreaId");
 8   DROP INDEX public."IX_VegetativeFilterStrip_SubAreaId";
       public         postgres    false    334            -           1259    276033    IX_Wascob_ModelComponentId    INDEX     f   CREATE UNIQUE INDEX "IX_Wascob_ModelComponentId" ON public."Wascob" USING btree ("ModelComponentId");
 0   DROP INDEX public."IX_Wascob_ModelComponentId";
       public         postgres    false    336            .           1259    276034    IX_Wascob_ReachId    INDEX     M   CREATE INDEX "IX_Wascob_ReachId" ON public."Wascob" USING btree ("ReachId");
 '   DROP INDEX public."IX_Wascob_ReachId";
       public         postgres    false    336            /           1259    276035    IX_Wascob_SubAreaId    INDEX     Q   CREATE INDEX "IX_Wascob_SubAreaId" ON public."Wascob" USING btree ("SubAreaId");
 )   DROP INDEX public."IX_Wascob_SubAreaId";
       public         postgres    false    336            �           1259    276036 %   IX_WatershedExistingBMPType_BMPTypeId    INDEX     u   CREATE INDEX "IX_WatershedExistingBMPType_BMPTypeId" ON public."WatershedExistingBMPType" USING btree ("BMPTypeId");
 ;   DROP INDEX public."IX_WatershedExistingBMPType_BMPTypeId";
       public         postgres    false    274            �           1259    276037 &   IX_WatershedExistingBMPType_InvestorId    INDEX     w   CREATE INDEX "IX_WatershedExistingBMPType_InvestorId" ON public."WatershedExistingBMPType" USING btree ("InvestorId");
 <   DROP INDEX public."IX_WatershedExistingBMPType_InvestorId";
       public         postgres    false    274            �           1259    276038 ,   IX_WatershedExistingBMPType_ModelComponentId    INDEX     �   CREATE INDEX "IX_WatershedExistingBMPType_ModelComponentId" ON public."WatershedExistingBMPType" USING btree ("ModelComponentId");
 B   DROP INDEX public."IX_WatershedExistingBMPType_ModelComponentId";
       public         postgres    false    274            �           1259    276039 *   IX_WatershedExistingBMPType_ScenarioTypeId    INDEX        CREATE INDEX "IX_WatershedExistingBMPType_ScenarioTypeId" ON public."WatershedExistingBMPType" USING btree ("ScenarioTypeId");
 @   DROP INDEX public."IX_WatershedExistingBMPType_ScenarioTypeId";
       public         postgres    false    274            _           2606    274921 V   BMPCombinationBMPTypes FK_BMPCombinationBMPTypes_BMPCombinationType_BMPCombinationTyp~    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPCombinationBMPTypes"
    ADD CONSTRAINT "FK_BMPCombinationBMPTypes_BMPCombinationType_BMPCombinationTyp~" FOREIGN KEY ("BMPCombinationTypeId") REFERENCES public."BMPCombinationType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."BMPCombinationBMPTypes" DROP CONSTRAINT "FK_BMPCombinationBMPTypes_BMPCombinationType_BMPCombinationTyp~";
       public       postgres    false    216    268    4685            `           2606    274926 B   BMPCombinationBMPTypes FK_BMPCombinationBMPTypes_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPCombinationBMPTypes"
    ADD CONSTRAINT "FK_BMPCombinationBMPTypes_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."BMPCombinationBMPTypes" DROP CONSTRAINT "FK_BMPCombinationBMPTypes_BMPType_BMPTypeId";
       public       postgres    false    268    4727    256            a           2606    274942 T   BMPEffectivenessType FK_BMPEffectivenessType_BMPEffectivenessLocationType_BMPEffect~    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "FK_BMPEffectivenessType_BMPEffectivenessLocationType_BMPEffect~" FOREIGN KEY ("BMPEffectivenessLocationTypeId") REFERENCES public."BMPEffectivenessLocationType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "FK_BMPEffectivenessType_BMPEffectivenessLocationType_BMPEffect~";
       public       postgres    false    4687    218    270            b           2606    274947 T   BMPEffectivenessType FK_BMPEffectivenessType_OptimizationConstraintValueType_Defaul~    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "FK_BMPEffectivenessType_OptimizationConstraintValueType_Defaul~" FOREIGN KEY ("DefaultConstraintTypeId") REFERENCES public."OptimizationConstraintValueType"("Id") ON DELETE RESTRICT;
 �   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "FK_BMPEffectivenessType_OptimizationConstraintValueType_Defaul~";
       public       postgres    false    270    234    4703            c           2606    274952 T   BMPEffectivenessType FK_BMPEffectivenessType_ScenarioModelResultType_ScenarioModelR~    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "FK_BMPEffectivenessType_ScenarioModelResultType_ScenarioModelR~" FOREIGN KEY ("ScenarioModelResultTypeId") REFERENCES public."ScenarioModelResultType"("Id") ON DELETE RESTRICT;
 �   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "FK_BMPEffectivenessType_ScenarioModelResultType_ScenarioModelR~";
       public       postgres    false    4731    258    270            d           2606    274957 T   BMPEffectivenessType FK_BMPEffectivenessType_ScenarioModelResultVariableType_Scenar~    FK CONSTRAINT       ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "FK_BMPEffectivenessType_ScenarioModelResultVariableType_Scenar~" FOREIGN KEY ("ScenarioModelResultVariableTypeId") REFERENCES public."ScenarioModelResultVariableType"("Id") ON DELETE RESTRICT;
 �   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "FK_BMPEffectivenessType_ScenarioModelResultVariableType_Scenar~";
       public       postgres    false    270    4711    242            e           2606    274962 @   BMPEffectivenessType FK_BMPEffectivenessType_UnitType_UnitTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPEffectivenessType"
    ADD CONSTRAINT "FK_BMPEffectivenessType_UnitType_UnitTypeId" FOREIGN KEY ("UnitTypeId") REFERENCES public."UnitType"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."BMPEffectivenessType" DROP CONSTRAINT "FK_BMPEffectivenessType_UnitType_UnitTypeId";
       public       postgres    false    248    4717    270            U           2606    274808 :   BMPType FK_BMPType_ModelComponentType_ModelComponentTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."BMPType"
    ADD CONSTRAINT "FK_BMPType_ModelComponentType_ModelComponentTypeId" FOREIGN KEY ("ModelComponentTypeId") REFERENCES public."ModelComponentType"("Id") ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."BMPType" DROP CONSTRAINT "FK_BMPType_ModelComponentType_ModelComponentTypeId";
       public       postgres    false    230    256    4699            �           2606    275312 8   CatchBasin FK_CatchBasin_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."CatchBasin"
    ADD CONSTRAINT "FK_CatchBasin_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 f   ALTER TABLE ONLY public."CatchBasin" DROP CONSTRAINT "FK_CatchBasin_ModelComponent_ModelComponentId";
       public       postgres    false    304    260    4735            �           2606    275317 &   CatchBasin FK_CatchBasin_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."CatchBasin"
    ADD CONSTRAINT "FK_CatchBasin_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."CatchBasin" DROP CONSTRAINT "FK_CatchBasin_Reach_ReachId";
       public       postgres    false    304    292    4805            �           2606    275322 *   CatchBasin FK_CatchBasin_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."CatchBasin"
    ADD CONSTRAINT "FK_CatchBasin_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 X   ALTER TABLE ONLY public."CatchBasin" DROP CONSTRAINT "FK_CatchBasin_SubArea_SubAreaId";
       public       postgres    false    294    4811    304            �           2606    275338 :   ClosedDrain FK_ClosedDrain_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ClosedDrain"
    ADD CONSTRAINT "FK_ClosedDrain_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."ClosedDrain" DROP CONSTRAINT "FK_ClosedDrain_ModelComponent_ModelComponentId";
       public       postgres    false    260    306    4735            �           2606    275343 (   ClosedDrain FK_ClosedDrain_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ClosedDrain"
    ADD CONSTRAINT "FK_ClosedDrain_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 V   ALTER TABLE ONLY public."ClosedDrain" DROP CONSTRAINT "FK_ClosedDrain_Reach_ReachId";
       public       postgres    false    306    4805    292            �           2606    275348 ,   ClosedDrain FK_ClosedDrain_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ClosedDrain"
    ADD CONSTRAINT "FK_ClosedDrain_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."ClosedDrain" DROP CONSTRAINT "FK_ClosedDrain_SubArea_SubAreaId";
       public       postgres    false    294    306    4811            �           2606    275364 (   Dugout FK_Dugout_AnimalType_AnimalTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Dugout"
    ADD CONSTRAINT "FK_Dugout_AnimalType_AnimalTypeId" FOREIGN KEY ("AnimalTypeId") REFERENCES public."AnimalType"("Id") ON DELETE CASCADE;
 V   ALTER TABLE ONLY public."Dugout" DROP CONSTRAINT "FK_Dugout_AnimalType_AnimalTypeId";
       public       postgres    false    308    4683    214            �           2606    275369 0   Dugout FK_Dugout_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Dugout"
    ADD CONSTRAINT "FK_Dugout_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."Dugout" DROP CONSTRAINT "FK_Dugout_ModelComponent_ModelComponentId";
       public       postgres    false    260    4735    308            �           2606    275374    Dugout FK_Dugout_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Dugout"
    ADD CONSTRAINT "FK_Dugout_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 L   ALTER TABLE ONLY public."Dugout" DROP CONSTRAINT "FK_Dugout_Reach_ReachId";
       public       postgres    false    4805    308    292            �           2606    275379 "   Dugout FK_Dugout_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Dugout"
    ADD CONSTRAINT "FK_Dugout_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 P   ALTER TABLE ONLY public."Dugout" DROP CONSTRAINT "FK_Dugout_SubArea_SubAreaId";
       public       postgres    false    294    4811    308            �           2606    275395 *   Feedlot FK_Feedlot_AnimalType_AnimalTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Feedlot"
    ADD CONSTRAINT "FK_Feedlot_AnimalType_AnimalTypeId" FOREIGN KEY ("AnimalTypeId") REFERENCES public."AnimalType"("Id") ON DELETE CASCADE;
 X   ALTER TABLE ONLY public."Feedlot" DROP CONSTRAINT "FK_Feedlot_AnimalType_AnimalTypeId";
       public       postgres    false    214    4683    310            �           2606    275400 2   Feedlot FK_Feedlot_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Feedlot"
    ADD CONSTRAINT "FK_Feedlot_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 `   ALTER TABLE ONLY public."Feedlot" DROP CONSTRAINT "FK_Feedlot_ModelComponent_ModelComponentId";
       public       postgres    false    260    310    4735            �           2606    275405     Feedlot FK_Feedlot_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Feedlot"
    ADD CONSTRAINT "FK_Feedlot_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 N   ALTER TABLE ONLY public."Feedlot" DROP CONSTRAINT "FK_Feedlot_Reach_ReachId";
       public       postgres    false    4805    310    292            �           2606    275410 $   Feedlot FK_Feedlot_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Feedlot"
    ADD CONSTRAINT "FK_Feedlot_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 R   ALTER TABLE ONLY public."Feedlot" DROP CONSTRAINT "FK_Feedlot_SubArea_SubAreaId";
       public       postgres    false    294    310    4811            �           2606    275426 >   FlowDiversion FK_FlowDiversion_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."FlowDiversion"
    ADD CONSTRAINT "FK_FlowDiversion_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."FlowDiversion" DROP CONSTRAINT "FK_FlowDiversion_ModelComponent_ModelComponentId";
       public       postgres    false    312    260    4735            �           2606    275431 ,   FlowDiversion FK_FlowDiversion_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."FlowDiversion"
    ADD CONSTRAINT "FK_FlowDiversion_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."FlowDiversion" DROP CONSTRAINT "FK_FlowDiversion_Reach_ReachId";
       public       postgres    false    312    4805    292            �           2606    275436 0   FlowDiversion FK_FlowDiversion_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."FlowDiversion"
    ADD CONSTRAINT "FK_FlowDiversion_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."FlowDiversion" DROP CONSTRAINT "FK_FlowDiversion_SubArea_SubAreaId";
       public       postgres    false    312    4811    294            �           2606    275452 B   GrassedWaterway FK_GrassedWaterway_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."GrassedWaterway"
    ADD CONSTRAINT "FK_GrassedWaterway_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."GrassedWaterway" DROP CONSTRAINT "FK_GrassedWaterway_ModelComponent_ModelComponentId";
       public       postgres    false    260    314    4735            �           2606    275457 0   GrassedWaterway FK_GrassedWaterway_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."GrassedWaterway"
    ADD CONSTRAINT "FK_GrassedWaterway_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."GrassedWaterway" DROP CONSTRAINT "FK_GrassedWaterway_Reach_ReachId";
       public       postgres    false    292    314    4805            �           2606    275462 4   GrassedWaterway FK_GrassedWaterway_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."GrassedWaterway"
    ADD CONSTRAINT "FK_GrassedWaterway_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."GrassedWaterway" DROP CONSTRAINT "FK_GrassedWaterway_SubArea_SubAreaId";
       public       postgres    false    294    314    4811            �           2606    275478 B   IsolatedWetland FK_IsolatedWetland_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."IsolatedWetland"
    ADD CONSTRAINT "FK_IsolatedWetland_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."IsolatedWetland" DROP CONSTRAINT "FK_IsolatedWetland_ModelComponent_ModelComponentId";
       public       postgres    false    4735    260    316            �           2606    275483 0   IsolatedWetland FK_IsolatedWetland_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."IsolatedWetland"
    ADD CONSTRAINT "FK_IsolatedWetland_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."IsolatedWetland" DROP CONSTRAINT "FK_IsolatedWetland_Reach_ReachId";
       public       postgres    false    292    4805    316            �           2606    275488 4   IsolatedWetland FK_IsolatedWetland_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."IsolatedWetland"
    ADD CONSTRAINT "FK_IsolatedWetland_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."IsolatedWetland" DROP CONSTRAINT "FK_IsolatedWetland_SubArea_SubAreaId";
       public       postgres    false    294    4811    316            �           2606    275504 ,   Lake FK_Lake_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Lake"
    ADD CONSTRAINT "FK_Lake_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."Lake" DROP CONSTRAINT "FK_Lake_ModelComponent_ModelComponentId";
       public       postgres    false    318    260    4735            �           2606    275509    Lake FK_Lake_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Lake"
    ADD CONSTRAINT "FK_Lake_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 H   ALTER TABLE ONLY public."Lake" DROP CONSTRAINT "FK_Lake_Reach_ReachId";
       public       postgres    false    4805    318    292            �           2606    275514    Lake FK_Lake_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Lake"
    ADD CONSTRAINT "FK_Lake_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 L   ALTER TABLE ONLY public."Lake" DROP CONSTRAINT "FK_Lake_SubArea_SubAreaId";
       public       postgres    false    294    318    4811            �           2606    275530 >   ManureStorage FK_ManureStorage_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ManureStorage"
    ADD CONSTRAINT "FK_ManureStorage_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."ManureStorage" DROP CONSTRAINT "FK_ManureStorage_ModelComponent_ModelComponentId";
       public       postgres    false    320    260    4735            �           2606    275535 ,   ManureStorage FK_ManureStorage_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ManureStorage"
    ADD CONSTRAINT "FK_ManureStorage_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."ManureStorage" DROP CONSTRAINT "FK_ManureStorage_Reach_ReachId";
       public       postgres    false    292    320    4805            �           2606    275540 0   ManureStorage FK_ManureStorage_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ManureStorage"
    ADD CONSTRAINT "FK_ManureStorage_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."ManureStorage" DROP CONSTRAINT "FK_ManureStorage_SubArea_SubAreaId";
       public       postgres    false    4811    294    320            f           2606    274975 B   ModelComponentBMPTypes FK_ModelComponentBMPTypes_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ModelComponentBMPTypes"
    ADD CONSTRAINT "FK_ModelComponentBMPTypes_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."ModelComponentBMPTypes" DROP CONSTRAINT "FK_ModelComponentBMPTypes_BMPType_BMPTypeId";
       public       postgres    false    4727    272    256            g           2606    274980 P   ModelComponentBMPTypes FK_ModelComponentBMPTypes_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ModelComponentBMPTypes"
    ADD CONSTRAINT "FK_ModelComponentBMPTypes_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."ModelComponentBMPTypes" DROP CONSTRAINT "FK_ModelComponentBMPTypes_ModelComponent_ModelComponentId";
       public       postgres    false    4735    272    260            X           2606    274845 H   ModelComponent FK_ModelComponent_ModelComponentType_ModelComponentTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ModelComponent"
    ADD CONSTRAINT "FK_ModelComponent_ModelComponentType_ModelComponentTypeId" FOREIGN KEY ("ModelComponentTypeId") REFERENCES public."ModelComponentType"("Id") ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."ModelComponent" DROP CONSTRAINT "FK_ModelComponent_ModelComponentType_ModelComponentTypeId";
       public       postgres    false    260    4699    230            Y           2606    274850 6   ModelComponent FK_ModelComponent_Watershed_WatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ModelComponent"
    ADD CONSTRAINT "FK_ModelComponent_Watershed_WatershedId" FOREIGN KEY ("WatershedId") REFERENCES public."Watershed"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."ModelComponent" DROP CONSTRAINT "FK_ModelComponent_Watershed_WatershedId";
       public       postgres    false    4721    260    252            �           2606    275764 W   OptimizationConstraints FK_OptimizationConstraints_BMPEffectivenessType_BMPEffectivene~    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationConstraints"
    ADD CONSTRAINT "FK_OptimizationConstraints_BMPEffectivenessType_BMPEffectivene~" FOREIGN KEY ("BMPEffectivenessTypeId") REFERENCES public."BMPEffectivenessType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."OptimizationConstraints" DROP CONSTRAINT "FK_OptimizationConstraints_BMPEffectivenessType_BMPEffectivene~";
       public       postgres    false    338    4757    270            �           2606    275769 W   OptimizationConstraints FK_OptimizationConstraints_OptimizationConstraintValueType_Opt~    FK CONSTRAINT       ALTER TABLE ONLY public."OptimizationConstraints"
    ADD CONSTRAINT "FK_OptimizationConstraints_OptimizationConstraintValueType_Opt~" FOREIGN KEY ("OptimizationConstraintValueTypeId") REFERENCES public."OptimizationConstraintValueType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."OptimizationConstraints" DROP CONSTRAINT "FK_OptimizationConstraints_OptimizationConstraintValueType_Opt~";
       public       postgres    false    4703    234    338            �           2606    275774 N   OptimizationConstraints FK_OptimizationConstraints_Optimization_OptimizationId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationConstraints"
    ADD CONSTRAINT "FK_OptimizationConstraints_Optimization_OptimizationId" FOREIGN KEY ("OptimizationId") REFERENCES public."Optimization"("Id") ON DELETE CASCADE;
 |   ALTER TABLE ONLY public."OptimizationConstraints" DROP CONSTRAINT "FK_OptimizationConstraints_Optimization_OptimizationId";
       public       postgres    false    296    338    4815            �           2606    275787 P   OptimizationLegalSubDivisions FK_OptimizationLegalSubDivisions_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions"
    ADD CONSTRAINT "FK_OptimizationLegalSubDivisions_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."OptimizationLegalSubDivisions" DROP CONSTRAINT "FK_OptimizationLegalSubDivisions_BMPType_BMPTypeId";
       public       postgres    false    256    340    4727            �           2606    275792 ]   OptimizationLegalSubDivisions FK_OptimizationLegalSubDivisions_LegalSubDivision_LegalSubDivi~    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions"
    ADD CONSTRAINT "FK_OptimizationLegalSubDivisions_LegalSubDivision_LegalSubDivi~" FOREIGN KEY ("LegalSubDivisionId") REFERENCES public."LegalSubDivision"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions" DROP CONSTRAINT "FK_OptimizationLegalSubDivisions_LegalSubDivision_LegalSubDivi~";
       public       postgres    false    4697    228    340            �           2606    275797 Z   OptimizationLegalSubDivisions FK_OptimizationLegalSubDivisions_Optimization_OptimizationId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions"
    ADD CONSTRAINT "FK_OptimizationLegalSubDivisions_Optimization_OptimizationId" FOREIGN KEY ("OptimizationId") REFERENCES public."Optimization"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."OptimizationLegalSubDivisions" DROP CONSTRAINT "FK_OptimizationLegalSubDivisions_Optimization_OptimizationId";
       public       postgres    false    340    4815    296            �           2606    275810 <   OptimizationParcels FK_OptimizationParcels_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationParcels"
    ADD CONSTRAINT "FK_OptimizationParcels_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."OptimizationParcels" DROP CONSTRAINT "FK_OptimizationParcels_BMPType_BMPTypeId";
       public       postgres    false    256    342    4727            �           2606    275815 F   OptimizationParcels FK_OptimizationParcels_Optimization_OptimizationId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationParcels"
    ADD CONSTRAINT "FK_OptimizationParcels_Optimization_OptimizationId" FOREIGN KEY ("OptimizationId") REFERENCES public."Optimization"("Id") ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."OptimizationParcels" DROP CONSTRAINT "FK_OptimizationParcels_Optimization_OptimizationId";
       public       postgres    false    4815    296    342            �           2606    275820 :   OptimizationParcels FK_OptimizationParcels_Parcel_ParcelId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationParcels"
    ADD CONSTRAINT "FK_OptimizationParcels_Parcel_ParcelId" FOREIGN KEY ("ParcelId") REFERENCES public."Parcel"("Id") ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."OptimizationParcels" DROP CONSTRAINT "FK_OptimizationParcels_Parcel_ParcelId";
       public       postgres    false    4707    238    342            �           2606    275833 S   OptimizationWeights FK_OptimizationWeights_BMPEffectivenessType_BMPEffectivenessTy~    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationWeights"
    ADD CONSTRAINT "FK_OptimizationWeights_BMPEffectivenessType_BMPEffectivenessTy~" FOREIGN KEY ("BMPEffectivenessTypeId") REFERENCES public."BMPEffectivenessType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."OptimizationWeights" DROP CONSTRAINT "FK_OptimizationWeights_BMPEffectivenessType_BMPEffectivenessTy~";
       public       postgres    false    344    270    4757            �           2606    275838 F   OptimizationWeights FK_OptimizationWeights_Optimization_OptimizationId    FK CONSTRAINT     �   ALTER TABLE ONLY public."OptimizationWeights"
    ADD CONSTRAINT "FK_OptimizationWeights_Optimization_OptimizationId" FOREIGN KEY ("OptimizationId") REFERENCES public."Optimization"("Id") ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."OptimizationWeights" DROP CONSTRAINT "FK_OptimizationWeights_Optimization_OptimizationId";
       public       postgres    false    4815    344    296            �           2606    275242 @   Optimization FK_Optimization_OptimizationType_OptimizationTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Optimization"
    ADD CONSTRAINT "FK_Optimization_OptimizationType_OptimizationTypeId" FOREIGN KEY ("OptimizationTypeId") REFERENCES public."OptimizationType"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."Optimization" DROP CONSTRAINT "FK_Optimization_OptimizationType_OptimizationTypeId";
       public       postgres    false    296    236    4705            �           2606    275247 .   Optimization FK_Optimization_Project_ProjectId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Optimization"
    ADD CONSTRAINT "FK_Optimization_Project_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES public."Project"("Id") ON DELETE CASCADE;
 \   ALTER TABLE ONLY public."Optimization" DROP CONSTRAINT "FK_Optimization_Project_ProjectId";
       public       postgres    false    296    4785    282            �           2606    275556 :   PointSource FK_PointSource_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."PointSource"
    ADD CONSTRAINT "FK_PointSource_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 h   ALTER TABLE ONLY public."PointSource" DROP CONSTRAINT "FK_PointSource_ModelComponent_ModelComponentId";
       public       postgres    false    4735    322    260            �           2606    275561 (   PointSource FK_PointSource_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."PointSource"
    ADD CONSTRAINT "FK_PointSource_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 V   ALTER TABLE ONLY public."PointSource" DROP CONSTRAINT "FK_PointSource_Reach_ReachId";
       public       postgres    false    292    322    4805            �           2606    275566 ,   PointSource FK_PointSource_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."PointSource"
    ADD CONSTRAINT "FK_PointSource_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."PointSource" DROP CONSTRAINT "FK_PointSource_SubArea_SubAreaId";
       public       postgres    false    322    294    4811            �           2606    275260 J   ProjectMunicipalities FK_ProjectMunicipalities_Municipality_MunicipalityId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ProjectMunicipalities"
    ADD CONSTRAINT "FK_ProjectMunicipalities_Municipality_MunicipalityId" FOREIGN KEY ("MunicipalityId") REFERENCES public."Municipality"("Id") ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."ProjectMunicipalities" DROP CONSTRAINT "FK_ProjectMunicipalities_Municipality_MunicipalityId";
       public       postgres    false    232    298    4701            �           2606    275265 @   ProjectMunicipalities FK_ProjectMunicipalities_Project_ProjectId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ProjectMunicipalities"
    ADD CONSTRAINT "FK_ProjectMunicipalities_Project_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES public."Project"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."ProjectMunicipalities" DROP CONSTRAINT "FK_ProjectMunicipalities_Project_ProjectId";
       public       postgres    false    282    4785    298            �           2606    275278 8   ProjectWatersheds FK_ProjectWatersheds_Project_ProjectId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ProjectWatersheds"
    ADD CONSTRAINT "FK_ProjectWatersheds_Project_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES public."Project"("Id") ON DELETE CASCADE;
 f   ALTER TABLE ONLY public."ProjectWatersheds" DROP CONSTRAINT "FK_ProjectWatersheds_Project_ProjectId";
       public       postgres    false    4785    300    282            �           2606    275283 <   ProjectWatersheds FK_ProjectWatersheds_Watershed_WatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ProjectWatersheds"
    ADD CONSTRAINT "FK_ProjectWatersheds_Watershed_WatershedId" FOREIGN KEY ("WatershedId") REFERENCES public."Watershed"("Id") ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."ProjectWatersheds" DROP CONSTRAINT "FK_ProjectWatersheds_Watershed_WatershedId";
       public       postgres    false    252    4721    300            s           2606    275089 B   Project FK_Project_ProjectSpatialUnitType_ProjectSpatialUnitTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Project"
    ADD CONSTRAINT "FK_Project_ProjectSpatialUnitType_ProjectSpatialUnitTypeId" FOREIGN KEY ("ProjectSpatialUnitTypeId") REFERENCES public."ProjectSpatialUnitType"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."Project" DROP CONSTRAINT "FK_Project_ProjectSpatialUnitType_ProjectSpatialUnitTypeId";
       public       postgres    false    240    4709    282            t           2606    275094 .   Project FK_Project_ScenarioType_ScenarioTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Project"
    ADD CONSTRAINT "FK_Project_ScenarioType_ScenarioTypeId" FOREIGN KEY ("ScenarioTypeId") REFERENCES public."ScenarioType"("Id") ON DELETE CASCADE;
 \   ALTER TABLE ONLY public."Project" DROP CONSTRAINT "FK_Project_ScenarioType_ScenarioTypeId";
       public       postgres    false    246    4715    282            u           2606    275099    Project FK_Project_User_UserId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Project"
    ADD CONSTRAINT "FK_Project_User_UserId" FOREIGN KEY ("UserId") REFERENCES public."User"("Id") ON DELETE CASCADE;
 L   ALTER TABLE ONLY public."Project" DROP CONSTRAINT "FK_Project_User_UserId";
       public       postgres    false    266    4746    282            T           2606    274792 &   Province FK_Province_Country_CountryId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Province"
    ADD CONSTRAINT "FK_Province_Country_CountryId" FOREIGN KEY ("CountryId") REFERENCES public."Country"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."Province" DROP CONSTRAINT "FK_Province_Country_CountryId";
       public       postgres    false    220    254    4689            ~           2606    275190 .   Reach FK_Reach_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Reach"
    ADD CONSTRAINT "FK_Reach_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 \   ALTER TABLE ONLY public."Reach" DROP CONSTRAINT "FK_Reach_ModelComponent_ModelComponentId";
       public       postgres    false    4735    292    260                       2606    275195 "   Reach FK_Reach_Subbasin_SubbasinId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Reach"
    ADD CONSTRAINT "FK_Reach_Subbasin_SubbasinId" FOREIGN KEY ("SubbasinId") REFERENCES public."Subbasin"("Id") ON DELETE CASCADE;
 P   ALTER TABLE ONLY public."Reach" DROP CONSTRAINT "FK_Reach_Subbasin_SubbasinId";
       public       postgres    false    280    292    4780            �           2606    275582 6   Reservoir FK_Reservoir_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Reservoir"
    ADD CONSTRAINT "FK_Reservoir_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."Reservoir" DROP CONSTRAINT "FK_Reservoir_ModelComponent_ModelComponentId";
       public       postgres    false    324    260    4735            �           2606    275587 $   Reservoir FK_Reservoir_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Reservoir"
    ADD CONSTRAINT "FK_Reservoir_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 R   ALTER TABLE ONLY public."Reservoir" DROP CONSTRAINT "FK_Reservoir_Reach_ReachId";
       public       postgres    false    324    292    4805            �           2606    275592 (   Reservoir FK_Reservoir_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Reservoir"
    ADD CONSTRAINT "FK_Reservoir_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 V   ALTER TABLE ONLY public."Reservoir" DROP CONSTRAINT "FK_Reservoir_SubArea_SubAreaId";
       public       postgres    false    294    324    4811            �           2606    275608 @   RiparianBuffer FK_RiparianBuffer_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianBuffer"
    ADD CONSTRAINT "FK_RiparianBuffer_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."RiparianBuffer" DROP CONSTRAINT "FK_RiparianBuffer_ModelComponent_ModelComponentId";
       public       postgres    false    326    260    4735            �           2606    275613 .   RiparianBuffer FK_RiparianBuffer_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianBuffer"
    ADD CONSTRAINT "FK_RiparianBuffer_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 \   ALTER TABLE ONLY public."RiparianBuffer" DROP CONSTRAINT "FK_RiparianBuffer_Reach_ReachId";
       public       postgres    false    292    326    4805            �           2606    275618 2   RiparianBuffer FK_RiparianBuffer_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianBuffer"
    ADD CONSTRAINT "FK_RiparianBuffer_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 `   ALTER TABLE ONLY public."RiparianBuffer" DROP CONSTRAINT "FK_RiparianBuffer_SubArea_SubAreaId";
       public       postgres    false    4811    326    294            �           2606    275634 B   RiparianWetland FK_RiparianWetland_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianWetland"
    ADD CONSTRAINT "FK_RiparianWetland_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 p   ALTER TABLE ONLY public."RiparianWetland" DROP CONSTRAINT "FK_RiparianWetland_ModelComponent_ModelComponentId";
       public       postgres    false    260    328    4735            �           2606    275639 0   RiparianWetland FK_RiparianWetland_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianWetland"
    ADD CONSTRAINT "FK_RiparianWetland_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."RiparianWetland" DROP CONSTRAINT "FK_RiparianWetland_Reach_ReachId";
       public       postgres    false    292    328    4805            �           2606    275644 4   RiparianWetland FK_RiparianWetland_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RiparianWetland"
    ADD CONSTRAINT "FK_RiparianWetland_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."RiparianWetland" DROP CONSTRAINT "FK_RiparianWetland_SubArea_SubAreaId";
       public       postgres    false    4811    328    294            �           2606    275660 6   RockChute FK_RockChute_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RockChute"
    ADD CONSTRAINT "FK_RockChute_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."RockChute" DROP CONSTRAINT "FK_RockChute_ModelComponent_ModelComponentId";
       public       postgres    false    260    330    4735            �           2606    275665 $   RockChute FK_RockChute_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RockChute"
    ADD CONSTRAINT "FK_RockChute_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 R   ALTER TABLE ONLY public."RockChute" DROP CONSTRAINT "FK_RockChute_Reach_ReachId";
       public       postgres    false    292    330    4805            �           2606    275670 (   RockChute FK_RockChute_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."RockChute"
    ADD CONSTRAINT "FK_RockChute_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 V   ALTER TABLE ONLY public."RockChute" DROP CONSTRAINT "FK_RockChute_SubArea_SubAreaId";
       public       postgres    false    294    330    4811            V           2606    274824 W   ScenarioModelResultType FK_ScenarioModelResultType_ScenarioModelResultVariableType_Sce~    FK CONSTRAINT       ALTER TABLE ONLY public."ScenarioModelResultType"
    ADD CONSTRAINT "FK_ScenarioModelResultType_ScenarioModelResultVariableType_Sce~" FOREIGN KEY ("ScenarioModelResultVariableTypeId") REFERENCES public."ScenarioModelResultVariableType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."ScenarioModelResultType" DROP CONSTRAINT "FK_ScenarioModelResultType_ScenarioModelResultVariableType_Sce~";
       public       postgres    false    4711    242    258            W           2606    274829 F   ScenarioModelResultType FK_ScenarioModelResultType_UnitType_UnitTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioModelResultType"
    ADD CONSTRAINT "FK_ScenarioModelResultType_UnitType_UnitTypeId" FOREIGN KEY ("UnitTypeId") REFERENCES public."UnitType"("Id") ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."ScenarioModelResultType" DROP CONSTRAINT "FK_ScenarioModelResultType_UnitType_UnitTypeId";
       public       postgres    false    4717    258    248            l           2606    275024 J   ScenarioModelResult FK_ScenarioModelResult_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioModelResult"
    ADD CONSTRAINT "FK_ScenarioModelResult_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."ScenarioModelResult" DROP CONSTRAINT "FK_ScenarioModelResult_ModelComponent_ModelComponentId";
       public       postgres    false    4735    260    276            n           2606    275034 S   ScenarioModelResult FK_ScenarioModelResult_ScenarioModelResultType_ScenarioModelRe~    FK CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioModelResult"
    ADD CONSTRAINT "FK_ScenarioModelResult_ScenarioModelResultType_ScenarioModelRe~" FOREIGN KEY ("ScenarioModelResultTypeId") REFERENCES public."ScenarioModelResultType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."ScenarioModelResult" DROP CONSTRAINT "FK_ScenarioModelResult_ScenarioModelResultType_ScenarioModelRe~";
       public       postgres    false    276    258    4731            m           2606    275029 >   ScenarioModelResult FK_ScenarioModelResult_Scenario_ScenarioId    FK CONSTRAINT     �   ALTER TABLE ONLY public."ScenarioModelResult"
    ADD CONSTRAINT "FK_ScenarioModelResult_Scenario_ScenarioId" FOREIGN KEY ("ScenarioId") REFERENCES public."Scenario"("Id") ON DELETE CASCADE;
 l   ALTER TABLE ONLY public."ScenarioModelResult" DROP CONSTRAINT "FK_ScenarioModelResult_Scenario_ScenarioId";
       public       postgres    false    4739    262    276            Z           2606    274866 0   Scenario FK_Scenario_ScenarioType_ScenarioTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Scenario"
    ADD CONSTRAINT "FK_Scenario_ScenarioType_ScenarioTypeId" FOREIGN KEY ("ScenarioTypeId") REFERENCES public."ScenarioType"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."Scenario" DROP CONSTRAINT "FK_Scenario_ScenarioType_ScenarioTypeId";
       public       postgres    false    246    262    4715            [           2606    274871 *   Scenario FK_Scenario_Watershed_WatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Scenario"
    ADD CONSTRAINT "FK_Scenario_Watershed_WatershedId" FOREIGN KEY ("WatershedId") REFERENCES public."Watershed"("Id") ON DELETE CASCADE;
 X   ALTER TABLE ONLY public."Scenario" DROP CONSTRAINT "FK_Scenario_Watershed_WatershedId";
       public       postgres    false    4721    252    262            �           2606    275686 4   SmallDam FK_SmallDam_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SmallDam"
    ADD CONSTRAINT "FK_SmallDam_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."SmallDam" DROP CONSTRAINT "FK_SmallDam_ModelComponent_ModelComponentId";
       public       postgres    false    332    260    4735            �           2606    275691 "   SmallDam FK_SmallDam_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SmallDam"
    ADD CONSTRAINT "FK_SmallDam_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 P   ALTER TABLE ONLY public."SmallDam" DROP CONSTRAINT "FK_SmallDam_Reach_ReachId";
       public       postgres    false    4805    332    292            �           2606    275696 &   SmallDam FK_SmallDam_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SmallDam"
    ADD CONSTRAINT "FK_SmallDam_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."SmallDam" DROP CONSTRAINT "FK_SmallDam_SubArea_SubAreaId";
       public       postgres    false    4811    332    294            �           2606    275851 H   SolutionLegalSubDivisions FK_SolutionLegalSubDivisions_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionLegalSubDivisions"
    ADD CONSTRAINT "FK_SolutionLegalSubDivisions_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."SolutionLegalSubDivisions" DROP CONSTRAINT "FK_SolutionLegalSubDivisions_BMPType_BMPTypeId";
       public       postgres    false    4727    256    346            �           2606    275856 Y   SolutionLegalSubDivisions FK_SolutionLegalSubDivisions_LegalSubDivision_LegalSubDivision~    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionLegalSubDivisions"
    ADD CONSTRAINT "FK_SolutionLegalSubDivisions_LegalSubDivision_LegalSubDivision~" FOREIGN KEY ("LegalSubDivisionId") REFERENCES public."LegalSubDivision"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."SolutionLegalSubDivisions" DROP CONSTRAINT "FK_SolutionLegalSubDivisions_LegalSubDivision_LegalSubDivision~";
       public       postgres    false    228    346    4697            �           2606    275861 J   SolutionLegalSubDivisions FK_SolutionLegalSubDivisions_Solution_SolutionId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionLegalSubDivisions"
    ADD CONSTRAINT "FK_SolutionLegalSubDivisions_Solution_SolutionId" FOREIGN KEY ("SolutionId") REFERENCES public."Solution"("Id") ON DELETE CASCADE;
 x   ALTER TABLE ONLY public."SolutionLegalSubDivisions" DROP CONSTRAINT "FK_SolutionLegalSubDivisions_Solution_SolutionId";
       public       postgres    false    302    346    4826            �           2606    275874 D   SolutionModelComponents FK_SolutionModelComponents_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionModelComponents"
    ADD CONSTRAINT "FK_SolutionModelComponents_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."SolutionModelComponents" DROP CONSTRAINT "FK_SolutionModelComponents_BMPType_BMPTypeId";
       public       postgres    false    256    4727    348            �           2606    275879 R   SolutionModelComponents FK_SolutionModelComponents_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionModelComponents"
    ADD CONSTRAINT "FK_SolutionModelComponents_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."SolutionModelComponents" DROP CONSTRAINT "FK_SolutionModelComponents_ModelComponent_ModelComponentId";
       public       postgres    false    4735    348    260            �           2606    275884 F   SolutionModelComponents FK_SolutionModelComponents_Solution_SolutionId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionModelComponents"
    ADD CONSTRAINT "FK_SolutionModelComponents_Solution_SolutionId" FOREIGN KEY ("SolutionId") REFERENCES public."Solution"("Id") ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."SolutionModelComponents" DROP CONSTRAINT "FK_SolutionModelComponents_Solution_SolutionId";
       public       postgres    false    4826    348    302            �           2606    275897 4   SolutionParcels FK_SolutionParcels_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionParcels"
    ADD CONSTRAINT "FK_SolutionParcels_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."SolutionParcels" DROP CONSTRAINT "FK_SolutionParcels_BMPType_BMPTypeId";
       public       postgres    false    350    4727    256            �           2606    275902 2   SolutionParcels FK_SolutionParcels_Parcel_ParcelId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionParcels"
    ADD CONSTRAINT "FK_SolutionParcels_Parcel_ParcelId" FOREIGN KEY ("ParcelId") REFERENCES public."Parcel"("Id") ON DELETE CASCADE;
 `   ALTER TABLE ONLY public."SolutionParcels" DROP CONSTRAINT "FK_SolutionParcels_Parcel_ParcelId";
       public       postgres    false    238    4707    350            �           2606    275907 6   SolutionParcels FK_SolutionParcels_Solution_SolutionId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SolutionParcels"
    ADD CONSTRAINT "FK_SolutionParcels_Solution_SolutionId" FOREIGN KEY ("SolutionId") REFERENCES public."Solution"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."SolutionParcels" DROP CONSTRAINT "FK_SolutionParcels_Solution_SolutionId";
       public       postgres    false    4826    350    302            �           2606    275296 &   Solution FK_Solution_Project_ProjectId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Solution"
    ADD CONSTRAINT "FK_Solution_Project_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES public."Project"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."Solution" DROP CONSTRAINT "FK_Solution_Project_ProjectId";
       public       postgres    false    4785    302    282            �           2606    275211 6   SubArea FK_SubArea_LegalSubDivision_LegalSubDivisionId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SubArea"
    ADD CONSTRAINT "FK_SubArea_LegalSubDivision_LegalSubDivisionId" FOREIGN KEY ("LegalSubDivisionId") REFERENCES public."LegalSubDivision"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."SubArea" DROP CONSTRAINT "FK_SubArea_LegalSubDivision_LegalSubDivisionId";
       public       postgres    false    4697    294    228            �           2606    275216 2   SubArea FK_SubArea_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SubArea"
    ADD CONSTRAINT "FK_SubArea_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 `   ALTER TABLE ONLY public."SubArea" DROP CONSTRAINT "FK_SubArea_ModelComponent_ModelComponentId";
       public       postgres    false    4735    294    260            �           2606    275221 "   SubArea FK_SubArea_Parcel_ParcelId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SubArea"
    ADD CONSTRAINT "FK_SubArea_Parcel_ParcelId" FOREIGN KEY ("ParcelId") REFERENCES public."Parcel"("Id") ON DELETE CASCADE;
 P   ALTER TABLE ONLY public."SubArea" DROP CONSTRAINT "FK_SubArea_Parcel_ParcelId";
       public       postgres    false    238    294    4707            �           2606    275226 &   SubArea FK_SubArea_Subbasin_SubbasinId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SubArea"
    ADD CONSTRAINT "FK_SubArea_Subbasin_SubbasinId" FOREIGN KEY ("SubbasinId") REFERENCES public."Subbasin"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."SubArea" DROP CONSTRAINT "FK_SubArea_Subbasin_SubbasinId";
       public       postgres    false    294    4780    280            \           2606    274887 2   SubWatershed FK_SubWatershed_Watershed_WatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."SubWatershed"
    ADD CONSTRAINT "FK_SubWatershed_Watershed_WatershedId" FOREIGN KEY ("WatershedId") REFERENCES public."Watershed"("Id") ON DELETE CASCADE;
 `   ALTER TABLE ONLY public."SubWatershed" DROP CONSTRAINT "FK_SubWatershed_Watershed_WatershedId";
       public       postgres    false    264    4721    252            r           2606    275073 0   Subbasin FK_Subbasin_SubWatershed_SubWatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Subbasin"
    ADD CONSTRAINT "FK_Subbasin_SubWatershed_SubWatershedId" FOREIGN KEY ("SubWatershedId") REFERENCES public."SubWatershed"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."Subbasin" DROP CONSTRAINT "FK_Subbasin_SubWatershed_SubWatershedId";
       public       postgres    false    264    4742    280            |           2606    275169 Y   UnitScenarioEffectiveness FK_UnitScenarioEffectiveness_BMPEffectivenessType_BMPEffective~    FK CONSTRAINT     �   ALTER TABLE ONLY public."UnitScenarioEffectiveness"
    ADD CONSTRAINT "FK_UnitScenarioEffectiveness_BMPEffectivenessType_BMPEffective~" FOREIGN KEY ("BMPEffectivenessTypeId") REFERENCES public."BMPEffectivenessType"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."UnitScenarioEffectiveness" DROP CONSTRAINT "FK_UnitScenarioEffectiveness_BMPEffectivenessType_BMPEffective~";
       public       postgres    false    4757    270    290            }           2606    275174 R   UnitScenarioEffectiveness FK_UnitScenarioEffectiveness_UnitScenario_UnitScenarioId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UnitScenarioEffectiveness"
    ADD CONSTRAINT "FK_UnitScenarioEffectiveness_UnitScenario_UnitScenarioId" FOREIGN KEY ("UnitScenarioId") REFERENCES public."UnitScenario"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."UnitScenarioEffectiveness" DROP CONSTRAINT "FK_UnitScenarioEffectiveness_UnitScenario_UnitScenarioId";
       public       postgres    false    4777    290    278            o           2606    275047 @   UnitScenario FK_UnitScenario_BMPCombinationType_BMPCombinationId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UnitScenario"
    ADD CONSTRAINT "FK_UnitScenario_BMPCombinationType_BMPCombinationId" FOREIGN KEY ("BMPCombinationId") REFERENCES public."BMPCombinationType"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."UnitScenario" DROP CONSTRAINT "FK_UnitScenario_BMPCombinationType_BMPCombinationId";
       public       postgres    false    216    4685    278            p           2606    275052 <   UnitScenario FK_UnitScenario_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UnitScenario"
    ADD CONSTRAINT "FK_UnitScenario_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."UnitScenario" DROP CONSTRAINT "FK_UnitScenario_ModelComponent_ModelComponentId";
       public       postgres    false    260    4735    278            q           2606    275057 0   UnitScenario FK_UnitScenario_Scenario_ScenarioId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UnitScenario"
    ADD CONSTRAINT "FK_UnitScenario_Scenario_ScenarioId" FOREIGN KEY ("ScenarioId") REFERENCES public."Scenario"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."UnitScenario" DROP CONSTRAINT "FK_UnitScenario_Scenario_ScenarioId";
       public       postgres    false    4739    278    262            v           2606    275112 D   UserMunicipalities FK_UserMunicipalities_Municipality_MunicipalityId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserMunicipalities"
    ADD CONSTRAINT "FK_UserMunicipalities_Municipality_MunicipalityId" FOREIGN KEY ("MunicipalityId") REFERENCES public."Municipality"("Id") ON DELETE CASCADE;
 r   ALTER TABLE ONLY public."UserMunicipalities" DROP CONSTRAINT "FK_UserMunicipalities_Municipality_MunicipalityId";
       public       postgres    false    4701    232    284            w           2606    275117 4   UserMunicipalities FK_UserMunicipalities_User_UserId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserMunicipalities"
    ADD CONSTRAINT "FK_UserMunicipalities_User_UserId" FOREIGN KEY ("UserId") REFERENCES public."User"("Id") ON DELETE CASCADE;
 b   ALTER TABLE ONLY public."UserMunicipalities" DROP CONSTRAINT "FK_UserMunicipalities_User_UserId";
       public       postgres    false    266    4746    284            x           2606    275130 *   UserParcels FK_UserParcels_Parcel_ParcelId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserParcels"
    ADD CONSTRAINT "FK_UserParcels_Parcel_ParcelId" FOREIGN KEY ("ParcelId") REFERENCES public."Parcel"("Id") ON DELETE CASCADE;
 X   ALTER TABLE ONLY public."UserParcels" DROP CONSTRAINT "FK_UserParcels_Parcel_ParcelId";
       public       postgres    false    286    238    4707            y           2606    275135 &   UserParcels FK_UserParcels_User_UserId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserParcels"
    ADD CONSTRAINT "FK_UserParcels_User_UserId" FOREIGN KEY ("UserId") REFERENCES public."User"("Id") ON DELETE CASCADE;
 T   ALTER TABLE ONLY public."UserParcels" DROP CONSTRAINT "FK_UserParcels_User_UserId";
       public       postgres    false    266    4746    286            z           2606    275148 ,   UserWatersheds FK_UserWatersheds_User_UserId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserWatersheds"
    ADD CONSTRAINT "FK_UserWatersheds_User_UserId" FOREIGN KEY ("UserId") REFERENCES public."User"("Id") ON DELETE CASCADE;
 Z   ALTER TABLE ONLY public."UserWatersheds" DROP CONSTRAINT "FK_UserWatersheds_User_UserId";
       public       postgres    false    266    288    4746            {           2606    275153 6   UserWatersheds FK_UserWatersheds_Watershed_WatershedId    FK CONSTRAINT     �   ALTER TABLE ONLY public."UserWatersheds"
    ADD CONSTRAINT "FK_UserWatersheds_Watershed_WatershedId" FOREIGN KEY ("WatershedId") REFERENCES public."Watershed"("Id") ON DELETE CASCADE;
 d   ALTER TABLE ONLY public."UserWatersheds" DROP CONSTRAINT "FK_UserWatersheds_Watershed_WatershedId";
       public       postgres    false    252    288    4721            ]           2606    274903     User FK_User_Province_ProvinceId    FK CONSTRAINT     �   ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "FK_User_Province_ProvinceId" FOREIGN KEY ("ProvinceId") REFERENCES public."Province"("Id") ON DELETE CASCADE;
 N   ALTER TABLE ONLY public."User" DROP CONSTRAINT "FK_User_Province_ProvinceId";
       public       postgres    false    254    4724    266            ^           2606    274908     User FK_User_UserType_UserTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."User"
    ADD CONSTRAINT "FK_User_UserType_UserTypeId" FOREIGN KEY ("UserTypeId") REFERENCES public."UserType"("Id") ON DELETE CASCADE;
 N   ALTER TABLE ONLY public."User" DROP CONSTRAINT "FK_User_UserType_UserTypeId";
       public       postgres    false    250    4719    266            �           2606    275712 N   VegetativeFilterStrip FK_VegetativeFilterStrip_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."VegetativeFilterStrip"
    ADD CONSTRAINT "FK_VegetativeFilterStrip_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 |   ALTER TABLE ONLY public."VegetativeFilterStrip" DROP CONSTRAINT "FK_VegetativeFilterStrip_ModelComponent_ModelComponentId";
       public       postgres    false    260    334    4735            �           2606    275717 <   VegetativeFilterStrip FK_VegetativeFilterStrip_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."VegetativeFilterStrip"
    ADD CONSTRAINT "FK_VegetativeFilterStrip_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 j   ALTER TABLE ONLY public."VegetativeFilterStrip" DROP CONSTRAINT "FK_VegetativeFilterStrip_Reach_ReachId";
       public       postgres    false    292    334    4805            �           2606    275722 @   VegetativeFilterStrip FK_VegetativeFilterStrip_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."VegetativeFilterStrip"
    ADD CONSTRAINT "FK_VegetativeFilterStrip_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 n   ALTER TABLE ONLY public."VegetativeFilterStrip" DROP CONSTRAINT "FK_VegetativeFilterStrip_SubArea_SubAreaId";
       public       postgres    false    4811    334    294            �           2606    275738 0   Wascob FK_Wascob_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Wascob"
    ADD CONSTRAINT "FK_Wascob_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 ^   ALTER TABLE ONLY public."Wascob" DROP CONSTRAINT "FK_Wascob_ModelComponent_ModelComponentId";
       public       postgres    false    336    4735    260            �           2606    275743    Wascob FK_Wascob_Reach_ReachId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Wascob"
    ADD CONSTRAINT "FK_Wascob_Reach_ReachId" FOREIGN KEY ("ReachId") REFERENCES public."Reach"("Id") ON DELETE CASCADE;
 L   ALTER TABLE ONLY public."Wascob" DROP CONSTRAINT "FK_Wascob_Reach_ReachId";
       public       postgres    false    4805    292    336            �           2606    275748 "   Wascob FK_Wascob_SubArea_SubAreaId    FK CONSTRAINT     �   ALTER TABLE ONLY public."Wascob"
    ADD CONSTRAINT "FK_Wascob_SubArea_SubAreaId" FOREIGN KEY ("SubAreaId") REFERENCES public."SubArea"("Id") ON DELETE CASCADE;
 P   ALTER TABLE ONLY public."Wascob" DROP CONSTRAINT "FK_Wascob_SubArea_SubAreaId";
       public       postgres    false    4811    294    336            h           2606    274993 F   WatershedExistingBMPType FK_WatershedExistingBMPType_BMPType_BMPTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."WatershedExistingBMPType"
    ADD CONSTRAINT "FK_WatershedExistingBMPType_BMPType_BMPTypeId" FOREIGN KEY ("BMPTypeId") REFERENCES public."BMPType"("Id") ON DELETE CASCADE;
 t   ALTER TABLE ONLY public."WatershedExistingBMPType" DROP CONSTRAINT "FK_WatershedExistingBMPType_BMPType_BMPTypeId";
       public       postgres    false    4727    274    256            i           2606    274998 H   WatershedExistingBMPType FK_WatershedExistingBMPType_Investor_InvestorId    FK CONSTRAINT     �   ALTER TABLE ONLY public."WatershedExistingBMPType"
    ADD CONSTRAINT "FK_WatershedExistingBMPType_Investor_InvestorId" FOREIGN KEY ("InvestorId") REFERENCES public."Investor"("Id") ON DELETE CASCADE;
 v   ALTER TABLE ONLY public."WatershedExistingBMPType" DROP CONSTRAINT "FK_WatershedExistingBMPType_Investor_InvestorId";
       public       postgres    false    274    4695    226            j           2606    275003 T   WatershedExistingBMPType FK_WatershedExistingBMPType_ModelComponent_ModelComponentId    FK CONSTRAINT     �   ALTER TABLE ONLY public."WatershedExistingBMPType"
    ADD CONSTRAINT "FK_WatershedExistingBMPType_ModelComponent_ModelComponentId" FOREIGN KEY ("ModelComponentId") REFERENCES public."ModelComponent"("Id") ON DELETE CASCADE;
 �   ALTER TABLE ONLY public."WatershedExistingBMPType" DROP CONSTRAINT "FK_WatershedExistingBMPType_ModelComponent_ModelComponentId";
       public       postgres    false    274    260    4735            k           2606    275008 P   WatershedExistingBMPType FK_WatershedExistingBMPType_ScenarioType_ScenarioTypeId    FK CONSTRAINT     �   ALTER TABLE ONLY public."WatershedExistingBMPType"
    ADD CONSTRAINT "FK_WatershedExistingBMPType_ScenarioType_ScenarioTypeId" FOREIGN KEY ("ScenarioTypeId") REFERENCES public."ScenarioType"("Id") ON DELETE CASCADE;
 ~   ALTER TABLE ONLY public."WatershedExistingBMPType" DROP CONSTRAINT "FK_WatershedExistingBMPType_ScenarioType_ScenarioTypeId";
       public       postgres    false    274    246    4715            V   ~   x�=��
�@���9O����lo�6C\I,'&�탙n�߽�O�ޚ5px��;A�
�>�\���yG5�1l�"Q�93�v;��9�3�8���)tfr{Ts�W�f�G��:��N$���A*      �   �   x��ۍ�@�o��Ux̫��k,����hց�.��`�ƀ�	
(��`|0F�c4��`�cC�k�X�KVø����qa�c~0��w0�l3`�c�����J���Ƽ0v@6;'�u��Z���3���{r�4���7Y'����`�d}mNֆ�c{�p��h||d��DK�7�u�Λ�����d�j$t����#���L0      X   �  x��T�n�6}��
�@[�"[~t�8k��6�F�m"�(�R���w$%��݇>���ggΐ+X-79l�	�+^����r-��G�͒%lu�y�����V�|
�������Ե����,�'g�����ϐǰ]�������k���Ŷ'���9�d�L!�\0%+��5p�|�Ev�A�����x<��E����%l���� ŉ��`�?�quy6u\"�|��쎮�?�l�Z�*����������=�tU� e�/�dw{�Ҧ�K�T�>+\�zW��Op�v���L�Y��@�a���sXWC�.��z�( ]�{�m����sO�����=l�+Y���U�ó9��t�v�U/(��6�0"��o�+�Kw8N�a���C_�K����(&��e�����u�k_��	
��q���Չ��:[��R=��f�=�~���O�R;�;z���b�B�>�>�8�B�2C7�%�	�j���@+�[[�ti�K^ڭ&(�N{��{{u� �eٷ�d]h�r��2����u�yR��� c���,���:A6oLaM�]��a��7 g(��Ϳd���=wgF�~�(��O,T�!�%�fI�Rl�Q�[q
I�}��mN.��]>
,QJ�7={n�y����	� �OSȍ�� E
%�a�� �a��5 9�	$Y�������Q���@��<'4���P�qs1�w)�m]�P_A�8��f���]XI����A=��(��s���g��p+^�G#ح�������<)B)`�&�m|�h0?J�1���s_~����A���S�}|��+�Q�!\��P��a?�&S}�7t7�"Gן���HE���8�^Wj�j6�����T3T�͏�����u���0��1��9��U4��~G� �n�      Z   '   x�3���+�,I�Q��i\F��iiA(m�Y����� S)      �   �  x���Qk�0��_E~��M[�ގ�]Md
�ٍk�&8Q����$龦�R�J,��7�#%�?b���-��6��?KI�9������L��*��vTN�B5�ڏ���0��?TU$� ioNxI�S��F�:����4=��E��oaj���̲�`F�g���a���a9M	O�]j�)����5SZ/#�����ѭ��: 7T�v�<6nHl9���, \�!i�2����r����6!�G]E$	_%�*R��N|�v'�b�u�Ou'�/e�w%��F
c�6������$_����"~�9��2 N���؞ѥ�0kEUYΪYY�h��l�O�x��Y�%���q2N@K�H��h��\�¨�Gmޣzq"?��S�
+�W^�Cu졂~� �K���^��g��
؂9$�c)�$*�W��[�}�x�@~������~�YlM���׻$I~c�      �   ?  x��TM��8=��
����	�G�FC�V�i���:�#;i��뷜4�js��{Uv�{�p��g�͇����E� �I�+���!�iӀ�9�,; �fM'��!��*6Nُa��*�y�qZ�2�tS�����g��o�~���Z�]7WD�b�n���u�xg���ɢPαZ6tD��D%��D����r���N���
O<q���O�n>	��AbEX��=R��V1ٶ�.>�;K�JF?�芵7�賦w�ҵ��D>?�����otg�U5�d?�S���Wɚ���Vv7��Ɇ]dU!�z�#�.��O�������za��R�~u�S�Y���|�t7��<��)�m�)����o�^�)���V:��0���^�	���k�L��j����\���d�c"r�Uy7|(v��z�u/H4F��2��ZY��� ]���r��+����"�{��5���4�g��*��M�oo�l:�*�r~ҝ�k��mDޕ�^�~M�2��� (��2���f��bz[(��y��Ⱦ�/dv�[q��eⳀO��x�-�qx��8�|�d�ϟ�zQ��LGj�i��=�I�������tݚ��$<�z{ȱ� -
�)��E Ս����r�Qn�zc�2-���[@���&��R� o�@F8-E|X���f�)�=���=B�>o��0�!ٯi�T��J�X33,�����a���&�阻�* BP?�	����'���MF�'�RS�;��I�� ���sv��망��1l�9���%D�:�Z��F��]X#���$�%D��sdq�n�Uh�e��(��� �6�      �   �   x���1!k������}�y���A	���H��*����"0
fwa|�0���؂t7T�|���,���-W�5���,-���v.��Y=���>f=|�����{?W\2Է���p ���H)��12b      �      x������ � �      \   !   x�3�tN�KLI�Q�\F����`l����� �4�      �      x������ � �      ^      x������ � �      �   �   x���1!�Z��(���k���ϑ�3�*�^� (	�3�PpM�Q�|�ʌߋ��)>[e��ӡU�G�+��Pe��=�S�=�꒶[nN4l�r�C�`K�E��1|�Uc�Q�z-�L��ru7���MM�����z3�Q���9����KBT      �      x������ � �      `   �   x����j�P���>�K���� �iݸ�܌fp���}�
�Jh¬��}g&Ŏ̳��s��D�(�v���b�:��3Y� ])���>�|y'U*rms�ͱ%;�e0�U����n�k���/5���*���"M����~�%�6��Ȗ�\�>~e��=��O��OY��������l��?��s���p�      �   *  x��YKrd�\��UK�K���?�5��+Ɗ���ǃ�de���X?���??!��%a�~��^5��_���.Kc��^����X�{l�%O�4���h��Rd�!�{�
1��⺂���תj�K
_�WO�U�^?UC#/��w�-�f:q_k�4�^��F|�^zߝ��*qb���W�gĮ�xƛK���잖�Z�Z��jH>b�r�k$X||�wJ���/�[|?1|����ŏ�y��G��e�����������>}Υ�g�����yϑ��8N�~�X�O-��M�ؗ��R��G���Չ4��목�j���z^=�6��Z�q�K|��k�H���u�I<큺�r�u����;�y/�xҼ�ƞ��S���R��y^s�V��[G��J�g���O�)��s�;�E��ؾS.�Ư禳�R��
�F�N|�5m}xTtM.o���R�u�{yn�U�9k">�G&�����������"����#5ֽ��y����$n�b-k�q�(������W��� �uN�mV͡d�V'Rs��C�YN�т�X&�h���xGi*�xC6��$�{�N�6C���y�RP�e�u���*�-�9&x�RAk�	�K����Y4�^��ƈ������z:q�g�Z��	��5_8P���U�h��8,����Z���3�$���بJ��8x̺�N|�Û)�ˣ���	�=_��&��gGȞ� �>2J;�����(�踉���7/����#�/D�)�����Z��=�<�����(%ἷ>Z��(�[ߩ-������A���CP`�����/�zx\c�����O�k"��NM����Q���N"m;!�&
�<��7�US� �[�4��X<U!
�#�x��Sk�xIh�'��/�C.ѶY:��^"�@�6��(����
��h��nk]��"f��7��"���kP��r׷�������%�p��'�����=�Z����Y�&��e	��-�#�^"7+�Y$�lؠ�($;ebްP@"^V�{�bf��)<*mF���ƐJ��"D
�4QQ
�8��6�X:{S�U(�1��Wpx.���Zܯ�$*�
�7�\�s]U'J�6J���nG'G77�����<<e�ϻ�u�`�w[˚��F=v$#�#Б��^J����!��MTG�.�0��:�¶�7�!s�!ҋ(F����i5�gHt�V��#��� G��MK�0
Rk=��E������Hj��>���W�P�_���OiT*�4{�bOٶ������ͪԺ�
��J��S�U���O�'j׈~��� ���j�<h�[(m�uḸ�t�~��@�!Z|ׂ {^o�S��8��6-�8�=i�H�%*d�0��VA)�+���:-�n�|��H{;?��J�O1�>5�*�Dhg��ĉ &ӽ��ME��&�>��&����X��
qE��ڒ'��Yd�������8<�`�E
�G�(:����j�����и%-�v*�8m�#�/����@�_|ǹ�J�8X�t@��F���+��(��*,��>��(#�>��x]�h�8؃�5�]����JG�K �^���Y�a��j��4����������nBn
��A jB#w�Rˈ1T���>�^����ව�qs�q�y.�1Y��59&��SC������A�.�}%�*m>����%V���g�[���խ��fC6��8�:}́�'���\3'�?��#�&X u���q�M<�	q�3(�-t5q\��݌�>r��֏qeL`�4�����{���'�>��-�����B��ׯp�?�E�ГG��
<�y�C�	��P} җ�< �N\� ~���cp�BK��ᮠ��_��$��Ȩ�w=���3�4#n�uW��ďG✸�ܰ\Ot8�`�"��|<
J��o� HM��y��;����(�����.đ�R�z������Z�Ca�����/����..��O��7Q�Z�|¸�	�I�����&��j/~9k۱�:ܘ=Ѕo}4-L��������k���L&��~��$�,�Ǆ_�:~�˗AM�Or!�7�:����o���ُ���� `���Ӆz�o���o<[!�ڛ�0z��(͒P/l�*�Op�]���{�A���#�I"�
?n���b��p��5�Z\�c����?�$�zd�w0�/^�`��}B��Re�A<Et��������?�Z��,đ���Ӊ��5C��h@)�>�%�i��Oh��P��2��"XiIy���'�����A����x���48��Fip��ì� � ��E��P�u8�C���b��3�l� ?��y��:O�{'�R��Gd�Ո�	m�.!^+���x>���8��V��8�!��%��5�>2�V¡��Ù�I���dC��,�r5���K;Ώ���w���T�l���o�ABg*��k\Mњw��G��v��T�ͱ��o���>]<!����k<f:�O� K8�H���}Зk��h01
����/`��y4����y�W��j�X�8�=�ll�<h񘉷\:B���|G�kVA�;�*�j!�[����c�S�H�V���U�'4<�xF�f�<�f�-���g������.�v惪�Q���yp�u·gv	���l��7pT$aH;���;���O�|d���*�p��|���C{>��h,�.��0��z�a�4�-�N!���W#����7Ԙ���[��ǘ�D>1�iꉚ�[.���hme����u���k�������Y��۱>�.�Ѓ�
����E�p�����Gz1a��)p���g�sR���j����J�N'���¿�¬"E�p��K���?��L�"%�14����Z�@܇ĝFb|�L��ga_�����&n0KAw���T��������V�������Y��a�t��^��U�_=�(qj�O���������w�T�	�P�'��C�?Z��?�yx-	�����1tq��l濕3R�$�����;.���_?B�>|4��IH=�M���~v��#~>`;r�8iFXl��YU� ��o�����o����ϟ?�~}�)      b   r   x�3�t	u�t)M�.V�����,IMQpN�KLI�4�2�ttt�t�IJ-*ITpL/�L.�))-JUH�KQp�/J-.)��4�2�����IM΀3��8��L8}B���l�=... �&�      �   p  x��VAr$)<�3���(@���wl�%zcO�p�::��T����<��y
�V��%���|�Y�����#���=��Zū/*˧�I!����/Ék�����,�<m��/%�"#���c�_�����G���b鋗����2_��2��{լ���������{�E/�U��^|m�de.���\�k�Ol�U���Y������뭯v����^�&���Af��=?x��>������*�������O�[������ͩ��zT7�Z��ZcK��5��yW��CeI��l}t���;D�w�pڻb���N:�N;��ޗW����n�ƔZ�Kb��<��/�O���Z'�>K�s�>CE�����h:���B��O�7α��\؛^����yt�;G����+vAؠԲ�<� w�kTQ���kl����Qⰱ��z'Հ]��a�Z���|�:/�I]����V���X��dW��y%~|�쟸v�cq�S��6���գ���:�������>��x3�:������O���&r��>�2 ԟ���v�omTܕ����zVd}��$v��ἅV8��?�J����>�͋��?Ћ�Cb-_���OO��n��>����vGr�E��9�i�N�<�n�~�8��s!�<݄>�_�m0�0��iM{P�?����r݊=#f�đ놠�ք�P�]���B��Tc����h�<;02��B�u���saT���
u�2��t��dM����-��e�<��cT@�e%_}ܴ�Q�|�!�$L#����>�؎@8�&��;_�1��8Y�ˈ���9�	���Ơ�kt\��G���g�8��2S��Q�
��Q�Đs�eu�#22q�]W���k\,�-�l}�i�䷝@��ַ��HE{��1[��A6L��R�~r�j���a��d7V���Ǯf�@ϋ#=��H�?㴂��40�yM�~X����٭��H ���k�P�4��(�����mn9���_��}�Q�����(0�n�H|O��?�\e���`�Z��Y�"�6X��KxO@�a���!����*�Yv����OD�=�-Y��>��-f,n~q��!���;��'_ШߑnW�Ѝ��Lۆ��R����OHC���0Z�������!#UO      �      x������ � �      d   +  x��ZI�$�<W��?�2�$@���E��fd6��rf ��e*��l��j<�8�Io���ĠB���� _�~�z��8G�P2ω#%]Jq�\s�ch)�Y.<�K�p\u��su|p֪ܩ^�*����#�4���W�4s�[�v�q�T��,Sy-ǅ9��;�<V�)����[����Ǖ����G�=�Ӷ��2�Vv����q��<p晚͜9���s�Sn\�)쐚���ƕ�����7Q��榜�q�-��n����9���V���g�!�zὌמTm���qXs_x����Dj�kKaa�o�ԹN�kM��\x��S������a����8-�C�+�������l��G�ZT��ڥt���j�m��ᣇH{&_�������JEX��G+u����H�R��_ۥ�P�q��:;��Ro�l�SR��-��
�6���q<�r��煟���(��/ּv�W�1�ټ}���"�ʏ�K�+i���`;%TC��)���r㺊Pְ�J������*e;�T?o�-�7y���/}ko��}}	���:�P���?��'������>�S��?ş�g|"�'�x���f����]y��$҅�)�}�H,�����?Yy�^�J��"�4�M׍��I@\��sO#��8�Q����(��c�$� �.���Qk�V⨉�n��d��ʍW/q_x�	8�)�t鈫�O��6�H^9D�n�����������5Ft��"���`�v�\{���ކ�Q��q��1BP�k�]��~�!tkE��m�ψ0���K��̎�F�7m�b���h�x�	�7<l�:��	S�֣kW���U.<wV}����7�S��i'��W`g^�OHƖl~�6v����G���jgʲH��_��ܳ:>���%v�@�]t;���ܤ�+\89���LiQ[>��5ovh%�d|����ǟ���D����牘?��FfO����ǰ�`���Ӫa��ϧ+���=Tcئ��_vc��sNu}�u��h3��Qz�𰮅!�ĝ˅ǆ,��:�&#Ru|V̰'1����Q(Ӆ�DZ! /2߉!q[��u}~�B:R�6���p�ac��s�b�c��С�Jl�ACF�V�#z��s;��HL䔕��]��:?Tn�9^v� :������=��ڡ�ߣ��8�M������V��ZBv>hC#�8n�#�'�Ew�C�}[ �%�w+��R[��S$����n|�YR���I[)�䍍]	�D����\y�^�r�)�z�Ш�S���3���6��1;�/m�j�5��x%h� 5z3�E�[�N�(Nǟ��]��n�����[=�]�:)��a�d��ɜ��ōJS�Q�rT�ד�e6�[�8��i�,�M0�7NĄ�v�aTwۭ��HV�/�8�&��}���/$~��6A�~@J��^� ]H�@�]j5�%Q��S���};^"����x��	XL(�ةXu��Ҡ?��V�Gu|C��=�v��S��b�C���i8G5���!��&I�����?�\j�gMSUʝ�Q��A��s�f�Cj����H8r�A��p���@��Z+F�*	t�a��:�I4@���A����Q8��y��(���q|��aM�o�%<���)����8��*�ޭ��J<���<UR�cߡ�,���ǵ����M_R������tA��|Yv������"�i����m���Q��I��[��9&��2�N:��.f�`��3z�W\GUA0����J%2�aG\�q0��!�8�m,����!�ߝt�N:�{��/)����̾��p����q'�@G����F���&����bEf�[����kd]��
����T����]��~��יZϝ||N^�2�?��WǛF4[0���a��X ��S���jtu�zk)!5/ 4�y�~R�����U=�CНs�ki��������k�3�\&�p� �I���z��ʦ
�n�{�IEѣ6��[M���nZ>����k�u�0��*������?�Z��ጦ�9�����UB�*!�[	�c�g���?U�c%1�w�e
��a��U��#��T��6��gR��*����8�8�s/'�9�!+1�U�h�cM=��o�a�ߝD�N"�{p:
��O����s�r�4-=fK������y�����������'�{Z���[������h>��q��z���j����##(9^��"ǹ�	: oO-�i�?P�G�}����?�yô�'��]��@����H���.jK�6)Rϣ�D)���!���f� ��� 0��o��+/��G�D;��B��6�A����q����h�hT4@h�h��r��.�xL������q��,wquJ�d"�4��t��/i%�`���wTr�VVZRjn2�F�`�<�2u��K ]�}�
8)yC�7v|K��_��hN;��O�[\�4Ԝ�D�y�G���[��-!��%��B�ͯ��0���#����ي�T�������]�/<���n��=�0Q5��b��>:��-w����)��X�S'����r�/T.z����^���=�ҺWv�Mw��B�T��I�b�L�^	i� ����J|�Nx�|}�߷'Y��,������N�i'O���(!'W��g���ٹ��?&�˞���2�O _8��e�L=�s�Y4��&9e8i_�\�N�e�#^-ӎ�#����������I*���8r1Ŏi��:<��u^�G�g�$�ç�	3�����tC���ܑ۽��+�JkyU�b�	�8#*e��z���T?l֚Z!��	�R���1`�l��������~&����9e�bwT�)�&2f��=,���{¡�x��2:X�3rך
�Q�[�ǂy��\�I����=��B�:�I:C:�N���	��ǳ�}��r�t:���a����F.Ce[�����������{Gֲ�ה�!��bS)nhF*9�F���qT0dT�ʉ�N�]n,<�^�땣�?a�S���GY�S�@�Yw�;�"3GA�%;���U��[�aC+�	)d��y����b_؀Z]�jv\N��x&�s�Gb�V�++�ac�&���D8�FF��DǴ���ΌvE���=;��&���nk���@Xh͵;.y*�6;^�D{S([�l��k.������+A�#�n���U�Y�Gc֊:Z�� k�%.����������?����H���U��C�y`#�O��?�^A>]�=�׷�~=��?���]��c���|�i4:�S�a�NV����7�8G��`N+�mx��m��pC�nɺ�UD�8'�5�غ+���"9���ESb�ꕰ���Zm}P�H3&�k�"Ǒthh#�5�	�k5G�A���%�_�E���C�}|(�(�����g$�r�{��������]]Ђ�X��������]j���$>>�|���ܱ�l||������!��`{��y�<�a���km;^K+9>�Qo{�1X��M�v�5������v��/�e��R<�������y����O�>�r��      �      x������ � �      �   �  x���O��@���'a.(����H���{ABa7b+U�� �=IdG�K&\�����x�����ݷ�}��<~��I*!Yg�A4i<�u�;�&M���Ѥ���� �4��:�D�����}w黥�U��U��,/nO�����w�x�qmM�Ap�x�uj�m�Y(֮$��g�����q�xʷ�m�Y��=h�&�����o��Z�#d���t8F@���%F��X)IM�9����H1�be%B�i��O����@���B�H���r�D���r���	r�&�k�����RnҒ 7An��|M���r�� n��D��q�7An�ܼ'n��D��@���r��	r�擭�O��s7�O�p�^��5��g�olߡ@
FQc��_0z5���_��������<����r��<p�)U���ޗ�a�"l-���a�E����,	Z���ߗ��"Q���e�H�"��}YsX��"��}Y>,��Hcw;�=,�j���C�y��ǭ�:�rG��[��X���t���r�o��O�w��<S�"���@
FQ�ؐ���^���`j6�(�cTc�FA*��-
���Qcc���\0f5f�����w��˂�65��P�e������uXīx�_Q}}[U�?���      �      x������ � �      f     x�eS�n�0<��b�-���$�я�(��E�6�^V�R",�I����[�e]J��Y�����
�>�x&�R0��Dt?g�,)R���s�'f����+���WH����0���@��p�T���\:�\C��G��LL5�΍���p�إ�`K���/�8R4O|0��ˢ7ݛ.ꡍa�����D���f��9�[����D�H�'z��x�4V[áq�����8;TXN�Z ꓜ>�rƿy��|��#���0��Ҵl#�F���
~��r}�����`�dL�4n���7�hl*9����X�ہ�g:Bty车����i�ڻ	5�[��؋�΢��d{�b��T��T���gP)uj\�ͨ�����h�t�lHٜk�@__@h�����q|�����,T�SI��8��>2lR�}��XS�,�\׀:#�`jT�Y���=������B���v����ľ��Fɾ�N~�3�?{j�r2u�g������p+��^c�e����5�ё�G�����l���8a      h   �   x���;�0D��� �Ͽ�^;���"BH����ۓ�PP1���v�;���m����o;��4v��`	Au�lX��{��j)XЋU�0��1g	\#�D����ާXkMi�^г��Y�+$�5��ԣ�%���0�R����io�y�!8�      �      x������ � �      j   4   x�3�H-JN�+ILOEfr�pq:&�甖�*�%攦�s�8Ӹb���� BY      �      x������ � �      �      x������ � �      �      x������ � �      l   -   x�3�tM��-N-*�LNEar�qq:������(#��=... ���      �      x������ � �      n   �  x���I�%ˍE�?#X��!�-@@�P��F��@���
H��W� ��֑����!��������������/'ɻ�z��Pvȩ��*��J�:R�/�W��h��ݣm���C��/�g��o���K5��̘�����XJKa�nm�o_m���<���H'���ĔC��{	;�؜�Y��=���S��]_ҸBn��b<xu~Z޷�������������������W�SG����Og���3Ծ��ʠ��}k�sW���p��k�|5�=����k�a�s���b�t�|N&��<.�`�S��'Ī�Z/�!��^��yצ;�Q��+����!�=��,c�Y����,g���:bPMĀ�6os��������ei�'�?է��*�v��F��ݩ�������V�˹�*wr@�Bud6�m��S򬒦��sZ2��q�\eEͧ1ƹv~��\[Χ������L�����hZ�{���ֈ��̙1�w��x�y��"�9���ZDry�&	C��t]k����o��^w>w'u�F7��|�\3�5��O�T�k_��̶��A4Փ�����o�a��<�����y�]�F��-1^I0ƨ��Ty�ҹ��?��l<���|�6�����ojibj��g����d�K�9��ovv�Ƹ�r�t}9?�sKr~K�XWt�f&T��o������3�|ӜWe?Ɏ��y�B�֣�J{iX|��i;g3X������A^�W�����e�u�S�{��<>-W��⋲���/'�c��Y�hu�!�O��g��Xu����E-�_�)�4����/�Y�u��'~J�΋�x����m��e�/G�V�e6�O
cr���ggA���B���6N��S���Ԡ1E�q��^&�z��6��?���骖޼����K�Jv���VO���>������J!������P|("�j���'@���d��S
�Xg�s��Ry��J�T{���Q�y����V��ލ���S^Js�Dz:iM&��Ƨ֭�����j�ǹH��$}sL��~�b��ʦ���^~�!�U�GYԜ�(}�G"_>����xy��U���+F�8���4廈�[7�|篍�f�D�>-����m�OYK�Bl^�F�3p�S�p@�Fz���|�J~>,�g7+1�vu�� ��Χ-�:�B)C��8�fV�r»���2*ؐc{n�"&��Ո��2.:Zp.Բ�lj��O���L��e�:)����4��SS����6>�3��Kۛ6�e|��sHp>)~�_%��=�(a�T�$D�Н�<��Xvl3m��?�;N+5��<e�ֈ/������燧"�݊������K5ل�8�iF�q9�f
�n��� �O�nEB����\�@�p�����m�#}�)�K�#����7R�|&R���������{ؚZKi�go������9ӏ>}Я_He�*N*���A�>I�'����*5
)6�s��c���X�8;Q���B�W��(��)k8�<6*�p~ȋ{�'wߞ��b��,��я���rN��f��+ǮF�T�e<⮩UԔ5nO��/V�]1�-O�R��V���S���9o,mt;��I�~�c����� �㕗=�'9m�d��yҫ��|�1	�?Xj�3���L��i��q��ɝA���Վox����/�yx���ʮ�u�l2��9T~�+��/����=����;윙�QcvW���㴭�t}w�1�yq��߃�~J7]L��r*�}��
_Ż:��0<�d�#�.� ��Ox�l|�{��8?u'���cR�y�sʵ�?%�h�g	u Sf�-��R���.7����ӥz�!80�����Xߜ�]+������O�ݝҗ_HY������奈3{��r�(ͨր�B$ȕh�D�$��Smv�����C�8��rXRB��*V��/�3�s�������T�F�E2�\�[r8Gq(��%DqD�}E���F6���SZy�>���Q>���y�4-�׬���J��|P�&�8���$"2�F.����������4��Ǽ]��7�����ʨs��Ҳ�b��D�gM���I�Q#7L��j��Hb�B���;?�J���qW���O����7?ݏ�i��8}}��|x�-|}�z�]�	�����^'����(9��e�՝��E_�|�\N����T������O�1�����:�����.&?�Fu��;���2��`��E�T��}b�����f��Mu��;����A�z�>ڊ��5�S*�8�,�W��^$5��c~�"W,���aP�����֦9O�ˈ�ty~��/�Pm~�r@k����s|�s�̨zy��?WdR,�y��L�Zy����S��r�l$���}�w������]�ׇ���|ׯ����o6�G"�9)�X'��y��+˹
ދ���5�8͚���`.��̳��?r΄��}�kI�Y�՗�K��|Ŷ*������Tz��	P��]|��-,h��K�m/ɚ�v�x��=��<����,��4=�,�������	�T�x��4���>���M!"��Q��S��
9s�3I��Ӹ����JZ�V���s}���	����.:��k����2i��b��U�F����ێ�ס
�Ƒ��L�n|X'P8(�8�Sb��:������� ��6�������8�+�u�u�ɭAa(ޔc2��2����R-��yn��Q����^�ߏ�Kz�\�8�>7�V
vB쨔�s�y�&�d�c�ײ�w���ѯ��7;�+��QL���m�@��h���x��|Vb}~hb�}�N�z'h��|4.�ر���_�Œ�|�S�Lu;?*�̜l��:y���񩫳���74Q����� �2��{�l����W�(����QaƟ�x��)U�Q��%eoVqN�4�?��R7�wj�����}U�8u������9���p~����ﺟϷڴ��4��d}�N��06>em3w{��ֶ��F;�������?O��Ԋ͏���w��z�@s~ba�U~��Ï�?~���p��\��������럹y�      �      x������ � �      �   �  x����j�@F�����hgl��m��@
�)��T���qK��y?��Z��11Zp���4�S���צx��~5ݾ�v�f������j���2|1�zU��˷��fYw�s�+*�ߛ�禪�>���E�+E�+���J�L2�wn�I�j#վ��׻�f��Y*]ĺ$Vo�T������e�vϊO������yZ;쉳H��'�>M�:��� ��O*�8[��_¥L�/���(�*S�\��թ8�J+�W�j�5U��ľr��k�Z5�����UʷV2�|�j`�&^Gru��k������H���u4\��ДYLHXQ6�[Q5)f��p������D�.��+���Ȏ��(��1�En4z����#�{9�Ep��,���h6!�E�|��0�O
bi:,�k&��}QY��lKc�-kFY�K���,���4�x�Ò�{X�~�:{�^6}�C�/{r�b^,���.=,oֲ���W��ba,�6���]�ř����;��U矮BO�`+��U��U­���\�l�J�CZ��+X��s��հ�R%�,��N�0����\.L*e���A`n��f�>��_n8�R%�/��mi΂=槝EǲИ7��z��g.�<��Ot[�3Ds����B����_o���,����|��R5yˏu��CZ~q��K��°�R��(+$:/�AD��´��p,ث���2�.�<���,˿��:      �   �   x��A��0�u�0#I�����#Y���6AW�ۢ�Bۦ��lo��ó}y�q�b�=;n!�KX�E��2�b![���X�^�^��g/d/���"�e��rH{y���!��KګC٫��UR����J��j�^e�n�^=��z){:Ȟ�S"{*���~�=5��A�t#{z�=��^�^m����E�k��v�zh{}������Kۛ�؛`�M2��{#���ﺮoPD!      p   %   x�3�H,JŃQF�%\��>�.`lș����� ��	      �   �   x��A��0�u�0#I�����#Y���6AW�ۢ�Bۦ��lo��ó}y�q�b�=;n!�KX�E��2�b![���X�^�^��g/d/���"�e��rH{y���!��KګC٫��UR����J��j�^e�n�^=��z){:Ȟ�S"{*���~�=5��A�t#{z�=��^�^m����E�k��v�zh{}������Kۛ�؛`�M2��{#���ﺮoPD!      ~   �  x�}V�r�8}��B_�1o���&����e붙��وc�̐R\��2A�r۝I�� ��&j_L�{��V���=�I����ʻh���)��ņg� S��t��{n�;��H<��}���t�&����X�[�R�wk��S��
[�⣭�c*-[����W,�N�M�m���9�m�>e�h�>4�	����j���?&J�H��s��4��V�gl�7�Sm	��iT��3�ޛ`+튕�{}閴����mS�z둺��h�`�ѵ��y\�;}��f��|o�G�to9]�����ѽ�#����p�`����n�s�jl�6#�IIl�a\So� q��p�w����� �T
)�Ec4�)�VV�u��y�{ˈk�Px:nu6,�S�r����8 ��d7}������c�[�_�۲e<I��$�C�$�L�AM�C�Q'�)���5Umڍ6M��ok֦�f��sx�U��k�7�!��>8O%3xp���vF��|�Ū�R��4A��9|���t�����n2�OF�b�M)ބ�Ń���L��i���� DJqx���X(�v��c��� �k��V�l�Y^uK�������a���v;���|�S��υ�.��������r��a�:<��#�;w�����Q�mj
�<�3MFQ��<lw��\{ �`ruw�� i?9K��o�ȑ����x�����'�_}�@R��$�;�{�Ϡ�ƒ��Ǆh��Pc������[|~ƞ��8|���dُE���Q�݋��y�>6�z���ļ�(K����l��A�ZH�Ũ����i�'�녂;[��{���>��Ɠ>$�fXc���bGX���)��` Hb
�k�j{� 2�����n�lIJ��V�wj� 9|��L&?��      �   �  x���K��+F��`j	*J��(���q�ѿ�'&�K�����A��>؂��|�Q'�/�����t�>͚����*c�F�կÇ-�m�F�ީ��`X;0˕􇷾���L����	��jB�f��O����ާ.�>{���0P��p18;6^���p�q�i�=#Z�@���?*t�0�Y�0�ë\�r�r+c|�#��J���[-� _bS������~�O�n�6���gE�&�[��(}��.������g��&����b�ë��YM���;�|���=��,Pͨ����Ǿ��s�4H��Ӳ@���WR�")m��P�������WO�9~si�^*�yU�$Ϊ��i3֦��`	�b]^)��g��p3Z���O��դ;EG���m����#Xh9�*G�Q��>��z=OyK�e�a��ք6���J$P5xJn�j$��άV
^����ջʭ�Ϙ�?�����ؿ��B���:��\v]����|}�'��\ˤ���Bc��*���KF�誨�KL���?M�|������jZm���v��ǻ����6�&Ǿ����Vy��&�쪠t����p�R^^3�^X��Y�T���*��&vU<����O��!��?�b>�IF��:��`i��p�-m�(�p�\�J�c���������Z:[kV�3�U�Mn7$2�Rmxx�i\�;�� w1ؘ�����rW����~�6���      �      x������ � �      �      x������ � �      �      x������ � �      �      x������ � �      �   d   x�3�tJ,N���K�w��+K�+���K̉N�-�IU(O,I-*�HM�+S@V�P����X�����_���G�ӐӐ�a�kEfqIf^:�aJ�l����� ֺDa      �      x�d]]�.�
|���t�����MAf��y�s#8�~��)b�~��x���'��6��o��?i�T�՟��_�-��rZk�V3?���A� j%����M�R���k�wP�_�ߝ}���,K��oz(L>�0Q�@���+�.}��o*�M}���T!�+�75�:�K鯾-R���_+���P��d�&���r���7Q��hY
]VˮQ�͖3���T�_o��P��O�͕B�V6�s�0I��s��Q_mx��[f�<!����+[��8�oy ���W��[�C��f�BC���=n���(�?�߁��Zֿ�~K�,Ƴ�< ���pB���y� �o���@S����[��p��=����p��Z)�p��W�om�ǚyN��o�W8(��|�夌�Y�/��s�������Δ[�p���e�d���N�mTi�;�U
�e��F�d��&�Q�+�H��xm2�l㽝Z���_����8�7�o��m��Kz�6�;Ν���c�v���
�o�R��a�K^��vN%�_�_����Կ�R9:��#�CaD��~G�����~G�p��wDF�l����!�/�Y���|qtH5���|@(%_{��=!�����*t*9��5��J����*ӽq��N�AN����o)�f�0ɝ��e�P���5n5��o�]�j��Œ� ��mr=��z���E�d��W�%����Hc��ע�
��[���r���vkP�	�.��O�c�h�wQ%�]�V_@(wJ�����ƻ>�W�t,=Z�>bW���6�U���Ń�@�f?��|I'��*�hQ�ć�:Q76x�+L�����1W^I�$f���au�S�쒨Ӂ\L�b�Կ��?qf]�~J��t�(Q>�L�0�h��D	f�8Yc��[	h���p�D�+�� Pٱ��P�q��Wc���q� E٦|g�}Kݮ��
uS�$K���q'�v٬p�>�d�B�T�B"�m뙰�v��:���܏��:��0|g�5� ��#�|C�� #�ar��/p���c�|��7���1f�v����܎�7�9����z��J5���Or��"vwG90�r=i�$�!�����q��o�.��߮5*�|�q����f�r�5t�zG��#��h�tQ��:�#_���9G��;��g;��Li���Gf ���G���X�m9���j{o��a۠�֩S�X�Z�S�f߿t�u����Ac�u29���'�3��G>�d3a����G�G�|�\r���\���H��9�$ߐ29�_L���񉟎B�F��7ÊɁHްrL�C�ƕ��݁H�x�hɑH���b���O���uka@,�1�'#yl�a,��r�~���@���4���}�Тr�1�t����܅ГÒ��)d�2X��K��1�L��1�ޭ�C��S(��؇��1�M�\�l���8״Dvu��d���W:�����t�wk�$�3H�*d�$9A�ZXF�|�OvL����m�/��D��k�r�>[���廿�o�u�NR���<�9��¤T5�`����^o��.~,�����4Q���?�3�4�t؞3����ŀE�E
`�dɌ�2f�����1&}���d��Ә6����=����Y��mL@�uO�l��1��e`���K�P;����N�2��o�%=�%~�f6���.,-p��܈#�l�Y��I3�1�M��rL����	6���ڠ��vH5�}&�քk������/ȴF��5�lT�I�w�͍z)=���4�%�60���@�. (ɟ�*��wb�W[��|X�گi��@ə x7j�dg�--�$o#Z� &�;+��a$9�پ����r�f�����%��tj��8w�v��s�>�ɐEFNd���=L.����,l�x��.6�B'�jH>�$�;�^���1R�c��mxcy�A�k�%�u"�����&����*��þ-w	A��o߁�&�!��l͂&#�{Ȅ�xT|��~`�b¦� {��n�^�H����,trS��6��Ng���Ic&�l��0�#�h�Ô� r ����LIӫ��ɊOM8��_��-��"T"3�dQ�<T���`�<T�0�eȟr@1sJ�I���}�'d~��@wy��|g�Wh��g�8ٯM�Ǿ�)N�s���)�Nʳ��E��4A���i`�8>ᚨ�/O�s|:Y�����g�C�x���7��c�ߘT�N�1���X�j���]��K#�ݗ̝�g@�V̬����מB�w7S�=@~�A�d�l�R1�qV�V�L�.;f)v�1k����J1s���ܗB�d�����$�0d��S
#��,2��@)C�@h��'e�jkQ)��.�"�R�]8e�O�1߼�p�*�
�ƁIb��h_�Ib��5P�`#in#D�;e��`C����4�����B���x�$2�B�ց��0�=�-��A��=@��Mi�D�M�'z�ś�֞(��`IH���>�_\^J4���,%aG�R�$�Tu�(Q�����#F���h�4?�,B&�%�KnKvmr΅��'�Z�b��d�E�������n�;��G�.�ji��6.�\���J�);���:l!��Js����lC9�����SxG�����S�-s�Zsr�q��m�PI����a(i}�,���g��L��҆cR�c�I����RX-�I��џR�2�������R
�]K)��6�	q�t#����
�xe1����x-����a�dq�����39����9Sx�p��w�p�������E	��fW8�5;�X)v��>���`��b`K���k������%a;(9�8^���Ӏ�3|��I�Ƣ:&�����7X�|R���q�\�!WG$;X�8�:���l��:9ϕ�	Q��D�����ri�X�x\��X��V���>�.���ג�E��ҕ-�
]g�5C��v��ܶsR���u G!�xcZ���CʎI~8[����o�BJ;v?ɾ-�
o,[��Ð1ɖ�W7G!%�R.0���A���;);(�f�_-TOK����;�V+���uM�D���T�õ�#_aލ�Ha�J��Bz:�C"��o�������p�΋O�����B��`զ}�=�E>	��H��_�D���;C�3�񎺣�����C�Y�ѩ����p��K�����@�2 ��]����v賏̕;�	�%�,�}�x���]��T�6�c��	9šC�%�:�g�!I��:��+���Z{{��v����D��E���˿O��9u�:8�����F9ѣ�KGe#�aH�:�(�)��/�c����֫:�(��&��N��6J�L`����^�Sl�SN;ү6ʍ)�j�5ʅ)��E)o.7�����V�/j�a�E%��|��FI{�^�j�$;�?k�AF3N�㌒66{7��qFI{[����2uQ��f��`{�ս�m�4�
��#�N#�G�sMi��QF�,bs�Q��Y��#��6L����8��w��j���-�/�Ӑ����!��!��X���X��Q)�^t��Fi,��Z�jk�8�(&S�Ů9�(�&p�tAK��x�G��w�p�z{ٷ�.&��ɖ�\L�b[����e���&�f �eiw������>��G9f�}�(��Rߟ�8���3@��&H^&ـ=ʞC�*� =�1�� x�=���aXrX�m �䴏�Z�ҷ�P����jK*w��J��fU�]Ө*����J���e�J���Fp�vh?�hn���!�u�r��!,���͞�͖i�w-�ԧ���}KaӃI�%C�����o跭bk�K7u�̠��T�DM9��om5�wR>5nL�$�w��8TCLo�Y��Zg~�B�#�μո��ʎ��,�h~ө��8���N�dǅ��e�.�n;�`��k��C�7��m��)i�+R ېo�<�!_=�RW��3_K�w���M}Y߉9��
�7��'��>U�Ot�y�0Tt��V����tIul횐���M��C��<�F�M&���&�,8�    6���'����<���n��M�Y�BE�gU����=�i�c�MʴN�����}4�ȴ�Ď|��Sm)m߾�,�4�^x��:�d��A[L�ВݠZ[LY�%W��V�d�퇪ɔ/�|���d11SVҠ���P�Ӓ��b�%�� y����!eI�X�1{kA鸬o�ZM���o�'δFޞ%N4i�e,u����x�*8`���yX*��_��jHo���k���,��Y鉗-�٫Af�1���+:
X�V����+�w��2��ʺ�j��67W�Z��,��3��Wu"�����#v���9mW�/]Ȑ�"YG�KfKuT�̰d����J��W�lE�"S��;UE�3ba���o^��=��sf+腷I�1�yu�1�]��^a�l��W*vY�J��d�%Ɯ^ϒ��Xe��.�{�_����␚�˚:\��^��]7�jw��������#Hf�=*Ev84)����mwKTx���;pMG���yrQ�E���"��w��;J���C͢nX�w�.�p���m�K3����0D�j�)�4�M�9t�)����ړ|�vU���+��,Y��0՝��'v#G�Y
G���x��t �'"Rv֝Y�BҠ[�Y�%Y��r�,�3$�8��N�D���^��F��m�����վ�?{�P��'�6�Ä��H	�F�{������[�P�O�����3�&�6�GLm�n%ֵ�n(��$����:�$'�k[�9�c}z�g1����E��ӊ��bHR���)��`m�`1����W�lµB���]+�x�ٽB����qu<T���x��e��0	tX�>fH��ÂɈ\ID�ׯǣ�VX�E�Qz+,YߟK�nmK�WJ�bӳL�vʊ�:}�h��A�S�z�HLJ��ԛUN2V��
��ߋ��oK:��bje�#�C�ul52��{5}�}db�j��C���Shd�a쁯�,�0�)dW��tk���Y�j׸g�v�{f
�A�Βh(s@����+W�+L�_�#?��\Q�K��k"�0	���F�Q��1���Q���l�2�u�P�ۘ�	�ۘ��}&Q�r�1�t��Y�'ZU^���GU��K�:��a��q�U�r�)p�B4����5�\��T�}֟}�)�c�hW�!�8��@�}k,�aN�)����?�����P�xOc��:�L�ʝ��}�M�N�b��F;��Zs3 ��P~���3����	?GW��~�\u�Em����U�ú���<Î�Ubh��W8��2OM� ���
�d2�t1�,���%ߞj�Yީ��0�����-��i���2�u�f;��*p"��t��n��ogHw�^�4&�8�t�jLr�w�\�>s���;��}����k�'k!c~{�_���h#�^��<�F��<gj����IG�L�œ�H�
�x��#[c�9v��u��z�At����3&֢f��}�|�ڝ:�u;-9�vYr>�q����<�:n�ī}�:�0e��U�y&^�r�H���J*w�u���2�H�o&�d��/��%��T�8��]s�J� x"_��FZ~5a"e2΍~}�gB���b��E�X�'QS�O���f
�d�q_��>��:Ź�eO���n|�9���
u��߫~�>�/��a>�)uC�ׁP�� �
'�ůYN��8�i>@DE��8�E0f4a����KS�$�c<5�N.[
�|l�c�����)�li���F���>q���K>7*�q������q?�UD�di4��5+�:q?�ڏq#��g۟��+�{�J�B�Wg�c��e��5��h�q�vV(h88���m�h�9~k��D	O�ÒO�j6�[\q��dg��`6�[D�e6\\q����{%Z�J�(�<���³"�_�;�<�z;չ�\(v�gv��E\��(�ZZ��Qt&�yE��a
�e�(9�2@s@�+g<�E�Z�����߬��\�s�MW�(!9�b�Ц&��;�vlq69�*<��i�ٔ�E�y�&GJY��7�{����5� �lM��*I@%ΜWEBE	��D�1>�h��ց
�:�S$n��  O�~ܗ��:�2/yz��S��XY���������bw4\Z��!^��H�Ι�<'R��k�h=������'&���9�a	��� Ⱦ@h]�&`���S�e�%�q�r	�)�]XB aJg�X� �)�0�mʕ�U;镴U�l����t�+���j���Y６k%޹f�Ľ�ݹ�A�|�#�g��v�F���]���[[Y9�]1��-+��?M�J�]4�
���9��Vf�.*m�2SvQ�VL	��b��۫]�x^v�
5���yǢP���W+�����:��ְB�U���c%��)�q+��h���(i��|�(i�¨���۠����t� ����ڙ��-���.@�9V�nm�
�s��b9ΪTm�#�Aّ�eqb@�G�h L��]N��q$<^T�?��"��90k<gG~�]�x�>'�<g��r�o;��������g��@vv'��ξ	��������N�=v�@wv-qT�TvxE���E��ح>�;�]��N)q�sL����Q8�_.i��e�kP���a������eu��kP�˄Cj�&�Y�a��ّ�[VG�@|v栭�I�O] >;���57���C��.P��5�m �g�q�E�I�FX����~���oM����gǙ_SgP��ٸ��|6�#f����)��R�>;P�}�!�C��N��n%��.�YM�]u@M����|I�{���?sh�xm9����"�4ђ���Ap���CF|�E|���+���
qL��7Hc�to�C��=.�n����P�m�7��b�&-���P���+O��F}����+g���g��qro�|�����-Pw��4vA�wq�I4��aB1�)���yD�1$[5�+w=w�/�r�����p��Bqdp|0P`�ɸ���D_���W��]��G^��;6#z2�l�=��"olH�	��8�T��Z�lU���r*y~����/R2�Uxy�h�t��!��������ֿ���6=w8����=�U�u��
�O�n�sZ���⫣ӑ�:}ƥ�'.����B�Ñ�Y�B�X����T�v[�R��!3�b�J�Zy�y�\ �8���|�ۯ�B#=�x�x��r.�w��V�o�����/����&�G��|����-�i�ql= .�q}�_r�'����+�QQ�]� ��2���8�GR�ʟ����+^��m%1���#�H[?_K��M�F�<.|�ȈW��-�?4�C:��R�H�lP�ʿ��jѯn�A���B@�8�L�*x�	�8�2�G<��Nj���^#&��|'G1\8ۙ�+gYT�KvcO�����.j�L6��)Jd(|ֵ-J(�z�׾(9h���΂�ɬ^�3J��s�5J@����%���ϽQ�}f6��ss��_��׹9z���9�6���^5�S]ۡ�`�OI'>�����8��fo飽m����̈́�ď{6�^#��~�ɪT���-]ׄW��{�3F�������Ë��jyɶ�uN�%aT��'���U�P�*VMR�j��U�96L��ٙzS

�0_�_T�,�U�'Eb��޿,ge��o������q����v�̣�p����L0���?]�9VLɸʳ瘋uR�����<{��׺�Co�2���s̄L�v����.��<������p�0%��ï�d�)r1��tt{I�m������M?
FF��Fn�,��xX#?�e�°��T�#[�2�`� Kv�R&�h��Q
|�J�%�m�`�#K����*�۝�J�a�m'ha���gL�F-o6�{���x���X�а�K9'��j�n���(P�K Ի��F�Jb����~�g�ޡ|�},=�k�a���5,�u��;�Z��j����f���������H���{��q5,�X���UŢA�b=�!ז_��b렎�U�T���'�U���n�L��#�eEl)�I�)�"?�v��џD��[�)� ��mβ�ĀB�.�PF`'�n��tz0�Ā�-;���*�9l�<����nË8v/�"g�ro�    ��9v��^<���~/��|[����!��w�-NJ�ͻ$�����v�8]�b�2i/��N��\N��.�Av�C  �B6j�̂FQ��&�nB����}�$;d+�BOY�(
=	l���̍�jN���~�Kv~��l��4��'*�K�)e�m��M�eI);nCFٱ;�� ���"DV���N�OZ����3��N���y�ˮ�ߞ��bo��,.�<Н�����-��U�˽v�]6�Z��r�g�k�������v�m_��l׹�w��p�P���l���l��+�r�����*�rC���p�m?�J��U� ;-��g��9~ӥR�f;.3y����g�h6������ �W��7��"��H�����l$�2��8Udݼ|��Ū"�fL9��0�[W�*C#gKJ�JX�K�S�
5�Nb���U¼�J��/�%��*��m��6�Vº�CQ��'��@@��D�J�m��H?ĸ)���sni�O�]�<�8Z�W#2��t_Vٔ@D�.6��D�ɇ�!�x]:I�Oo\':t�T�FgBk�"�r�hu������ԙX�A�ȓ��lgX�� m9��;2�����r���gݶ�R�%έ>�3��q��#�6콲��C��P��l��hƻ=aY�s*mG��QY-�q4�)��)so�I�Cf�I���Vѐ@L�T��8�i�t{;vRGY��M��*�.�Xf���)�Ӯ��%�P}x4](�R���F-T�y�1�iG��]���Q�����>ýTu#��b(-����8jˆ��WpԖv C} �-�8��	r=�Z�]^!�B�T	,�%0��Ǹ�E_�q&��vĐg�*Q�v�YkC5|�'Sm�H�8Q�ސ��}�M	��k'Q��Ef}A�=;Q���?�=!S��%~�?�ja���Zª�1E�-�j�[8�l�U�gM�8[ªӆ]�-����H��$p֮c��VHˠ{�ⴒ&#�d�Sx'�d*4�{�Ĭ7��ڵ��&֮��N^�@X�����v��z	���X �]�����Y�@X;72Y��e� ���*Gw����*Ox¼��i�8���B�8E'�M`�������͸7�@Z;�\�		Hkg�����sG��GQl�l��6��ذ��nK6�������-��Y�y���q ?�[c1i�>W��6V�o8�+��]���u)�ŕܵ��G"}�?�"�̑�4q>������r@Ͼyg�g~̅�e�t��I"EP�8(��< �N��~.�g>:�{�{���	,^u�[	4�J#zxp�ܥǅ��J��{��YZ��U-A���ͻ�`�43d>f��f�A�9v����L8IG����*0�1k<Ex�4z�0V��OV��	��ԓ�{�MMѫ��Rcqe]�-�L� =Mx�@n�%�	1�YK�[�
Ȅ��6��w����RmG	�a}_�S�h�ep{���T����+XnS`��O �Mzd;}�m:�m�;�ܦ��4��m�m:�ݟ��mڑ�˩���M�x��T��c����E%��M| /��T���
�4�L�Ҹ���T=]�s��g�\�s�޷�8Zep�A����6�O��mڰ������~Ҁ���v3̙��m�x��LV��6m0�ӑ�!�FV
gc��bN��깉�k���|��f~�*p��s�'P��c��7Q��6a;`�g���B5�
����ci�S9�tn�as�G��f�{���h>�>b��87��q�c@������&p���U�!k?`FBQ|�NH1I�~�f80��[�0�=0��n�V��!r�Жf#�Q����5���=0��l���PD����<p��>�g���^�Ϥb�[u�
$�vD7�G��>�%n�5�k%n��,�W�Fkz@�΅𳚂W���m-��t&�>^���`��kA�	�i�tx����tb��wd�"N~�|�Yn�����A���h�� [��hl���b l��S� �����ϝwRɯY'���:��m��-�'7N�Ɏ��3��5�1��"H�Q'������ڸ8���38s�6�Q�i�N�K���9Uۇ'���(�n;,�~�X�@�K+A�4�:Zwn��./���8��ײgP���ff#@�	̹�<c���s�q�^��*N��"w���dp��*l����r5=l�[�"�A7ȯ<�c���n;�E	p�)V"	�q���O�V*�K�� nEl[~<����>l��Y,�X��/Xts:���/�R͘n�+`���[*���b����(�����kz�P�m�MOƔ+x�V�ʈ�b�Q7��� �u��>�]2�uc�y���!���~{������0'p�}Q�qW7�����6�y��k���f��	i`��-/Hw]�}�ׅ�ڶ��{�Ա)�L�REi��Dz���uy�#����_wo���y������
�p�����e���Qf��[CRlzqCE�^@�Pф^���*ˮlj	4�wȷM��'k,(Ӯl:=[@��O:#�v?������;�MH�ޜ�9#��+,�s��ey_P��m{@��.�|��8�!)H��q��s�qƑ(�u�8���0��ݩ�w�>a7:��ݴa�D4��h+� �n����|z�z�Ew��M�FG� �M�u��^] 
�N�y�(���AP�]|������O�OB��ӛ��|�i_�|��(��%q�>N);to:���K�a���|����E�Y��[U�k��������7I.�Ǹ{��	�}'/�Q�4�y��>�ܼ�U�io�?��1ц�n��~�-�y�����n�ϛx�9��t�[3
(z�	p�� �P�3�&M�*�x�,�逸��;��/xzg(i�M�g�S�v��P��!�8��="�zoÊ��cX�n��W$��f�I��y�0KD������cw�1c���N���P�{��������w�y��y>�{�>xF�-�{�>�^�V��Vy�Eڟz9wY|b"uOUH�>z9wY|�"�r�
CG/�.�^�]�]��@���*?8N���e�S,����S,�3���+ޒ�\E���[�\%�@i�:oWQ�<ǟ�Kn�:�o��U[ȃ�mH�N��s�j�x	=�	]CZyO'�6�mL;�hdH'cJ�l4K�c�M�l4S�u�<�3Ⓦ���F�|׿��B5j&%�iW��c�ɐiP�Ώ�����%;U�H��P�=cw&74�^H�u�j��nݧR�3�ր�W�*�x���Uw��P�� �M�z@ܧ��8�r�=nT���q�*~txWtd�b����m!uݪ�'tݪ�/ ��u���P�81�Nv]�:��U�u��Ū�[�a"�}�d����m�əxX���*�u�Ǘ����y�쾑f�W��7��rI��:p�~����EY��~S�!�F��[���χE����oɯ���_'������K���<�@����^�O��z�rz$�oݥ�ť,F�2R H�[�R���f-�V����$���n{�����(Y?jk:�~��{$�=+��D���G�^r�D�߫r�G��U����s\�+�֟����5cC57�J�j&��mv=�C�գ]��R��	�&_s��mv��%��m�F��$ .�X6S�۲YZޖ����r�o%Y�F3Hn�p�}E,7��dy+��s�x#�}D�Z"�Y�M�q]��<0�~�>�������f[��x�G�#�q\� L��&�yΣ�+8�PH~�`�F�V�
x��?���Xn�GcP��Yb;��~�|�	T��܂
xܡu�ꊬdn�$��O�2c�Yvԕ[��p���<�	|�Q�g_�pT��#�����X�|���@N`U�y��u���/&�!���,��b�='�Cc�_T!-hԩ�=����'Xgp�L�._����!�rV�*�������\&T�>�����Yr	۴�#8}�,8����#8}1.H��r_��#Ƶ�*����4���*vw<�i��b��	<��|��g�8�U�    ����N��݃(8m��q�\�i~gߤ��m���<{߀��ӆ�x�2�18}�-(���l�F��e���L9l�{�h�)gcG��q�l��&[�a]ˋ1�|�+��{�!Μm�5��	������{��?IE�9\N=Ì�8��ۂE8���y��ms8����"��7�Ż��T�'2��9�{���fx�����&��}Թ����@���!��s:/I�7�T�09SY��LJ1'�����̇����m��H~Q����ĵ����0ɳG�?�y�ex
x2 ��<�L�/{J{�z_���c�8ֺ-���n�<�0�,kY~o[��|��g��v�����Ŗ��P��V��S$?<�������a8�[�&��F�dX��"ܦ��c��̲P2ΠNǓ��"�RS���*�g�\���O��f8�c�n�7����'	4�4��S�D}�VMV�zU�խe�u�=����J�	��ᨈ+D: ��D>���8�><�u��v�NL��HlGw�J�|�I��y8��~�)��"??E����;�Xt��a�HP������"H�U���g��6m��\��4:���u3htb>Y����ٖ�Ev�Gg[��~SL:a�=9�V��@J�/���h���pg�ܑ�B�����!;d�'��R��˾���XG2�96��sr�1��{b�%o��E@��u$�p�׾�ӞX��W��X�&�+���X'��
q�u��d��'�N��|�5c?�0��U�k���ӼJt+AW%��i��*(�U�[~�<��X%��8� �������X5�;U�b��4���T%���O"�4C�y���mT"߶]�����8�Jk�@^��/Y&����d�,�ů��1K�;�]��E+ε���"��]Q��r��!��G������E�Ꞹ��~XЖ��2��OK�G7��O�%�C����J犙�ۄ[�=;3]�������L�FD�?�q	��ĕ.���Vn�R��E����KR�e�Hy�_̰�|�~�O���K��C2�	�`O8�[O��kU��@��.�ê�-���'��~n�)y[U�R�s��e�oE���u8	�ˏ։֣��^�8Α�k8y˱JD��U� w�(��g��z����{X�/=S�o�5�I��K�� d�Q3hb{=U���y�ʸ��\��,�3X��Rex�aM����G4�a2�n#���N������k�w�TU&�6��b�K�V1aQ���`�L�}��qԵ�=/�X�c�%-�Y���K���M1��fe��]��}e�H�@��^v@ dmO�i�����'�`BΑn�ϊ�Z�����A���tYE�':�K�K�gw���K�':�/��%)8�.a�{���>�?����"�͂0y�w"�zM	���1�>�g9�H�f`ҔjX/�d��n�OH�߰���e"�%��b��:��/��i�����x�v�k	 ݆ �v���a�p^���]�����k��p��ÌHe2a�'>����Y!-&�k���B�آZ��b�fo�JS�=�ab�j��w�`J��ni�L���.��V�ɕ<�,�;��\���r�x"�*y�_ɤ��1ż�&�*y�->g26�X7p�I�<?kY&S�}�6�(�s�6�(��n�_+Ky!�HyJ���+2����ޕ�MI�0=�z΋�>>�Y�0�ɘ��v��sL�l��.�ʰ��g�$��� �$y�,�'ΤI�,ًN3y��ǮE��ۮE{��h&�&��._����R�����n$`R���Wo�y��e~�w�(�#����[�;��"�;�e�Y�`K����h�D�>4@;'��6�@;�VK�f&�aM�^OH�;Z^�n]���v���Ql�q�iM7�.�БF�"���L���"8�z�."��ؕ<m�Q��zÆR�`~����ܓ�>�;��&��[��l�X�DX��\\!�量�aێ��3���	��V�P����y�fqr�F�!-o�yn"�71�e/�7�<ߦ+������G8V�<ѻw?�ɢ�ܭ���q?�L߹��1��!���x3~/wo��o��Gd�����G�D��2�0��DdN�`��z!3��yBՌ�Ɠ�N��~�N�S�g�����r!u�mVQ'̪w�>��w�¬X��NՎ`�{2(��K�^��V�OU���+��X�K���`N� 2����=��,��3H���{�����,v7(w�NE�����y����:�s;6:�I@���w����۝�� P��죡(�u���d�'��f�'����O���'��t��<����_��ρt���?�ț��
�`wX���=�n1�3u��G�E\�90��JGeW]v�9�;9�c1�ߣ�;�������t��3U��Z��mԢH��AnWuUx&N��a�n�C	Y9��򌳨+{#���m3ͣ
��"h�^�	D����ږ� �N���99�n�c+=uwnY�yƱ���`����4���87æ�<���;�99�]'��cNƻR<�99�Osr�Hw>P�g#�>=��99�.����99�ϢJ��~�0�9@ωtV�үg�'���,�c�y|�9��K�1��$�N����iWg��A�v5|�� qr�#�9�6���?F팫g9�:y'>�!R�W7
����5P����\�j4��752�����o�g'����>9�Ɩ���Ï4sҍ�y�	�vp��!��J�}�a��M�n�#g�9�p+�G��MNq�U�yr��Y9��ø��$��0���}cN��,}��[S��M�oD��]�ꐎ�Q\�Z��M��V�ճ}9��V�� ���e��~�a�`��f5�����YNq��_͈�A��e)�hY;ۮ�9��d�ꍃ6Y	6���]��,��#�L��ڗ�dp&k�>�de�A��F�����nࡾ�u]k�˥<�Q7c��5�K�	���Y'tW�to�� 1��G�e�s̳�x>c��8`�T[r�,'U1yf
KC��9�e�,�丬�����Z��5�l��}.g��dki�G6)3X12�2Q'���ƜuS�M�J�a���fr%ϰlYI�<o�)��la��01sX۲޲�y�]M��r�mWoZ�mW�_Q�n�����+&��Q>�1��1|͋=3i�[��;�}&Or�MR��Sݧt��?L\����cYѤ,~]Ҽx,~�2��Y孟�b�G!?r�����e��x�;Tc���n~
����EһI��+4]�ٮ�b���+t���A�,oH�؍�~$F��"������<i{C�F��I2�r����#wh�eʢ�[��Xxd��x8����.%�ô>b�	�Vx��(�L;��ǴA������d�A�A�+���7�b���4Σt�>��+�6D翩�o��S~U�,�EI�\��ƚ�>�	���f2V��43��x��s^��Q��z1�M����D<ܑp���������U����)�1Тz�{�ཌ-�ɘ�o'dp$GA�³�,�Q���c�|0���R���j�$�*[[(���HV9�M]��HV9�;o3)K���dUz�2� YՀf���ɪ\��-�G%O��!Yu��IA�E&�o��}1�:nx���a?����{YKr>�,҇�I��6�#����o(����Kr����=�-�BV:�?h�a�4�����u�+��.�KP����Β->�)9���>Lɹm�0�	��{�w(����=�����w��09�5���}gr��p0&��V	6�k�P�����L�`��a�zl<�(������zyW�����O��M3���S��x���:-2��.�ۆ�(�s�"6�����W鯻io�޺�����P@��7á3ȓ�p�7�|h̝#�=9��$k<�W�1��l��+��x�s�B���SW7��r����6?�(���a��&����sѓXָ���������]��ǌ3H��ީٳ,$�ylTcG9 Q��;�:��������d����^�J^��R�cX=�u�| Q���c�5��1��ڑ�ɗc    �M�c��X	��c.��+��%d�w���_sB��J4�l�t�W���u^eX��V:C��y�a���2�2,�Ao�,����-�eX�c�5���Ͻc�s��c�5Yk�ɿ�B9�X��,��|C�b����́�vQͽhn�r� �">Y�'�O��ܷ��~�X�*�Dȣ�����*F���A�Lk����*΍�U�����FX���rT�-���Q�|[��k��Ȳ٣I�3�|��u���e�3�s��⬜�	a`� �����ƣ�Ik�W�3��-���7�u�*�\����<������l��	t�y#[�������^Ӗ�!U���	2�| [[����پ�է7ؔs�+�+����IEyؔs �R���@o;e�x9��@�>��]^|ic	Г�Z��Q��Z�X��۪&U)o$�-uR圷Q�7�J�X�KYȻ�j
V�^�D�*���f��4��YU�rL3�K�U�ʗ���>��W�����N�4��BU��r/��Xr�yϊ�i�B��a{��?Weax�\A'Y��'
gvf�GQ��@&�ǩ�s�ßEaY�����Όc����x�d���we��~�N��^�T��*�Ӣi��w���c�TថO>�i{����@�i����&��d�5γ�>���S��+0H���|L&��ħf%a�RSD�g��4'@�?F�#�.�ɁbY�U�\��څ�1�cYh��П��`Y�ֲ��XϲP����t�,��6�dY�H$˪����g���2kZ��/o�E-�q��dU��
S5-W�,����bU�\!O��^���]�3��,�X�h8�o����I��̪w@���qY{�i��xL�+��yX�
�>FN����cX�.�s�Y�H���aq�K��u����k���^ዴ�s� ����,돠�w@w�z�UF�z�GU9H˾k��$R/O��{����3l�x��������gc^����2I�lս���Çݤg�����'�����X ��	�p�-o��	�4���G��8�n�:�>�)��Υ���wȳ��l��i��{PL�<O@��?_' 	-���5�Z���<m;aUkYP'�py� $��v �W���PA��l�91���5!M��ڄu.�
�|mZ���65��mӔ��Lʨ�s%d[NaT���n9��IW�4��P������} ],`�>6�td[n��I�-���տ�G��H��Ξ{#LH�����R`��	����Y?sS�N8��o����j�Y;����鉬#�Z�	��w��R��.�>�{��t%#�hە����&BW2rc�Z��{.�O�V0�����tdL1[�@����d�ؖ���̉�lY���*�@���I]ٲ23�wRn�A���:^VUv�k�!W�泟�*܀МLP'��7<M`',��L`簪��Α��>;��sh��;�	��� Z�$x�w:�vOp�����Xb��8�Zӿ�?���7Ff~>����Ϩ|q�O���|�3R�>���v���_�j{�h{�N��O���&���I`�Z�\�����AE6�2��u=��7��,a=p+�v���Z���G�r�6�z��K��䤧�ʥm$n���e�0�*��V.�X�L����� j�#۫�&�{j�29��Mx��9| -,����:�|�·�I���l;�!��sf���y`�9' ��}#��?�7�*_�7�������o�{#�gS/�"�ob�D�hu=��O!�?���>��խ�Q�H��A�Ч��0'�����m��`&ơ��=۶��_�(��ģ�)$��eH�(��7��3ͮg>#Kf��s�;�W��I�#h��I����o�;�>ȇ>e2�@�(z���}�ɓ�.n���F��ྲྀ�H��5���G�_W��hA��}�@�(��r9��O���9��NѝA�\�����o�L���xZ��6DU��l���
B�>�*�ˆ�+���<��7B7T��+P*�t�͍:/�Ȧ�S� Tֽ��������?�.�u|ʺ��cn-8~�͟W�U5� ȔuCe:��-�`S����y�ꐆ�%2.�2��3��O^�� �v7`S��?�G�=nY��z\"��M��n��h��w�R���L��W�����?�Z:��Eq,�ތ,ߋ��kV�Z���u\�
��O�d�(���	{8�(���R��eUw0��R���{T����E�slQ��� Q.{]�q�4�/!���{M�dR.�$��5����pq����(�<n��E���;>!�,3т(��-�A�,���O�.���M�Ur���A��i�vۉ?�vV(�y��'�Z������#(�v�.C���Ƹ�;t�jH�����i�#��������U�4T��y�,�a�)�����iϸ~�d폼�>8:�JU��ב�)��01�Q)���dq�*I��&���\MK*�,}���+J�U�<t�*����o�U��wC׫n��J9��9���Хr�W���C$:�I����4Ht�IM�c7��y�N���)5�2Gp�l��+�CG&�>�šu�(֔E����c�'+A�MX�}��pks��]��f�]}~��<�*�����Zw�bv٪6t�j\��~`�!���Z�����.W�����C׫8,Ɖ�qV׫n_���/t�>ӫ� P�`�S�+Q��(q�J�A3q�*��#�w�h�T[u�j��ե�GѴ�R�ۆ��c�`�M�u:�e�˼��!
��.s�N����������M;��Ǵ����fS+�S��ԙ�)f}l15��r�����	9����I��ξ��r#,�؊��+~�9�2���1�|�F��:F�#*�W���P�F�ݦ{��s�]���^#e�҈���L^��{%c��C�<�3yŐ?Μ��5��"�ϐ�3�ׄ.��9î��43X�u!�۷9�d]�U����9�+ !H�u�>~LȻ9Z��He2���R�1m�Dgh��!�:�n���٦�es�B'L�J�)
�0���)�۴IJ~Lxݴ
�%er�}D]Vp&ߍ�2�j�\�M#gr>�n�(�&gu=-g�ʠM�9�ѺV�O�3k�v�-�2�)PE�oG�8;�z'Y$���M'� 1�����Q��ȓY��]�HH��c�E	P'��1֥<�����z,�U)?jw9��W��m�*-?���ö�[UT#�"%�\ŀ�$��U�f����OVџG��b��)��� eV��D��m3��Y�g�d�&�iݎ�7:nXw8v2����YP2+*�s+��ƽ�� @��3�"�c���Ȋ8l�4�=��ۊ���q��,��-����\ʞ���v{r)��J��v���G�~&��H��G�\g2k�y>�ɬc�3)�Yǒg�A5c��CY8��[����ŕ���e�[`�kX�|�D|:�am�7ŧ��^0Ũs[vHˏe�Q�pA��_���3#��u_	F��o��N�ְ��N���p�Q.y
���َ�-�e���&�>{tϔ_L�����R�5�j+{�ri�Zmv]:��Z�0>�G;Z�qʥ˵Ky��uw)�u�����΍ ��\�[{�P.���C�|�/(��?ؗ����J�$���W�����ҳY۸��J��g���ɲ�h_�r�]�=`e=IL�m�&r��iA�\�c�@��ôv"��RO���ʥ~���'!���OBd�ݬ-=�%��+�JP�׺����Xۙ33B�e}��\b�_��!��?`�y~�>��*00�F�F9o�o�d�(�[8 �r>A��hQ)��Ӌ�Q�#g�qp��4����ɥ�'���`Q�w���r��vݲ��P���:I�?R(��
z�{Ԫ�u��
�7I�GXPb�N��0Îf�),1���g���
Z�[�ءesu��D�3b0����XV;۲��ʒ�zhR]rl�Q�T�|�u�m��;�xcغgI����"���b�� S.��;Wλ�]���]e'����el��l�:�I�y�NY�2    �Ht���N���b�;O�"5��c� (Rcݥ<ک1�2@{{��R�K��r�Ʀ�c�u�r
"��|8 R.�]!h��?�y)��� R�;Ib���|�pw瞭���ݰ��jR�;�弎Գ�>@��בd�����݉R�f�cw�����b���6y��p����e��Z��Mɼ�	�3_�����%��f|���?�V,a����Պ%�������/�x�7���������[����N1M�#/�f6�\�'p��u��R	@1�GO����iy��T�R�B�<ρ���e±Q�5l���
����F"��1��OS���<B?�I�#�s��<:𺮯�G�]��l�'��J�L'^Y�,���=8\,�C��Ey�}�c���b��v��ݜ.=���5��B%�
��K�=���PeX��^�s�F,�Rmv
h��d���:L* P>rs�RP䲷�z����1. P��_@�|c����Y=z"���P�z�TPy�T�"d�-Gy�T���U��'��gI�^�����T�g������Pǌ��;����)�Öӊ-��wB�-ӭB��IO[r
���'EQ��<>�l,}��2�x�����
R/���F�>�����;H����� �8~����hy�,��:�����A�-�$ODh��)`M�
^�,�M�
�����59�Uy�<n�=\
�Ľ�b#&p3��!l�)lm4��ĥ<�XQ�f6��8ZѲ�MR?�-���H��ҡ5��q�g!'[��׃�]rc�~���WT������f����k���)6��Қ�9J�#XsU�f���VuO��ZU�沪��תS��{y��>�|�Z*�:�|Ǻ�����:��%(��.��$�g	���t�n���F�#��S:r|�	�2u����<�䳙�LGb�4)���3����]��L�t�Y�Yȗ�?�*�K���%�|��濨Һ�R�KV���K� LVQ��}R:�g L.�������(
dapar���r{crԯ�Ķ�19�mWo��G�B�;HEi�f����r�.t�6s�x�m�%�t�ֻ���L;�I���i�!I��N�qVi������/r�K�&��Ծ��@$��SarjGAD�~�F��\� ���}�{����\0&�o沀29]������@Ǥ��}o��wA�:/����2=���Jҫ����L-Aa��Aכ����z�Ae�l[���$��+`LN;g���ɩ~�V�ɩ~lZ��Ǧ�$蹑.`LN;e9+��7A#���B�N���M�#�oC�GA#E����� �^O�F���*��Ӯ�3�;Id��tT�911:�t�U[�A��N;�k�	�Ƃ-9��d����q�0z���	Lw^P�p���Ƒ���Ȃ���������~O��29}@.��?(7ŋ�gz��x4N��L�<��/Y�<J��BOby,oeR	+7���i�.�WAc>�7�59���l�&M+��y��"M>L9���X��~�&ߖi�ײ�$�,)^����7��f̾֋X{3f�� N>����#�e���"�Jl�b�f��"֊���=�q0�h���G��{<{^շ����|��wFaϳ7�ݿΐjZU�ıl�i�
�Ql�L� ��V2)�<V-�dNީJ{����Z^CO��o���;�J[#y�� O>k�lqy򙥴����J.o���!��[�u�8y��1��m�u`M>��Q���	J�u`L^��
��G���chY�����c�L�>��Ls��k%�
�~��v�t��.+,�K�����w�?���K.�C�B����w���i('{�������82&�����)7p��s����dL�e��j!cr���dL���g- ��>݀s��/$K�w�l!]r��Il�Ǒ��-������cԪ��&P�8� �*�Lޅ�a)�wa��r&�#`-o�P�t��XR&GC*�|&ir�&�q&��[P&ߘ���穕L5H8�ޝ���YO����@y�^>*Y��=S0ng����KZ7�rV����_;+[�}�����Ƹ�8�V�Φϭ����Ϭ�Y�����Y�≸��Y�"��uA�@nl���E%l��Y�qة��u*פ���8�⏮?�j�����t��YEmx��zV����׳dV�9��_nW3����Aq�h�����j�M@���:ZĤf-n�WIŬV��m�%o��ζ��u$�ߴ��fנ�	v3I<��î�Q�l�Ǯ%wՐe~��Χ�:���U�{��	��8sy�+>�&ǙK���׳b��+q;�e%
v�,��a����e��(�����P��,�21�,gH�
g)��%gh>2�KV�]K�]�
W��R"��mdM����%G�m�RA�b��N�/9�mw��#�f��Z�����F@�ٶn���&�i��������2�-�JV�-z�����u�R���X5Sɰ��~(`KV^�0l������.OY����$kX�\�p>����~iD����R%�o`%W����Q�zt��n�XI��cܝu��*� ���={�]�*�~FVk����xa?���#�;Jt����o���p��^���s�ze0��Ig��w�*ʪ	�%��U�T��<:�l
Y����`҉���R!ʧ��t�]`��D�2�E������潞�e����VO� ��Q�Ъ˯�"&����KZ�\a#U�����\���������a�<��س�J^����;�6�<��x����~{-X!W��1`z.*Ϲ�!�no*�����@-��)�
ْgxC��>ؒ㖫��:�I�$ވ�w�Xĭ�Ɋ��6���(�8�I![��ǙR�~ ْgX�;�$]�<-�]N-��kR�۴q�u�v�n�F0�Ka��[����#L;,�O����W
	�GX6�U��i�Z�f/:���奈<pn�3���ʝ�ea^]Wһ_x5��@g�"
��)�al���F�W�ހ29^xLM�29^xv/ÖKP&녗a�Y��c�e��}��d=�2x�C�d�!�/��;~���.dL�&b�&��NT�Z�(ܧ(��ċ�i ��z��Iv���d���Z�d�b���=��d������V��,]-�2Y�7S�O��vd^{-���`ɺ��Y��ۡu�\{�����Rf�º�~^��v��Ŷ�]�$�w¼�,�8���m�,5?���m]1��7�+_���O6_��(w��B�n�Ay��s��=�β>].<9�6�/Lg��˞�-�M��^�/�M��^��>٪�z�ڶU���ڋE"?�q��{1�Xm��{��m��&빗��B�䲝a��i�K8��F�d8ւ������TS���h�ƙ��{����=�%ێ�������[���}�m\+���BV�'OS�N�E=�������R�<��1w�v#��B��0.\�S�m[���Ja�}�Sɰa��z{mg%d�K<�3	������T�n��E(��B!���nv|��b�g�`���9����Pօz��\6�u�6�^�X�s<�Vzn���:T�8�CA�\6�5.[o��z��e�9���^����6��1�PSC�.t���xbw	�E��v�����}�(������:dNn�S��g��F8�xdێ�y>�j��A�s�V|<���p���+:�mX�^|<2,���x²�)��	����W��A�A�kHLvK�5�Ɗh9�FrA-��+^���$->����4��6Xe�ړߎ�A�����]�:�%������o�T�-y}ؒ�=�@����\���.�?��m��|&=$��i�Pк#G�oy��57U����H(P�T�3��o��<�)�@�<Ô�ߙ/�I�G��f3T�P�D.x�g���<
<��cJc}L��j]��{qT��N+Y��<~���$y� ��(,ih�*(�G�-���U}B�Y��m(F��D��j[r$�E`�\�c�&`����dX�X��Q����!Tsg�P�N^[�4��r��҄j�Mu��*���k>�k�W@lq���ɓ�-PM$,}d���ΰX֩��    uǵ�4!�@�i�b���ˁ.|o# <V��$8#�"i�g�U�7��#!� <>�Ԡ�	��v���5�����>c�����<�jO;��p�x����l�?��4=����38�ӕ�w���k����$9Y7��f������*%��?Ph��t��h�zNiTB�)9��P�(����V�H�����;7������x��F�� JN�fQr��%���Ti�>Q3%����W���%���ßG��ŪԤ2���"El����?w��4ѻ�;El�@�N kU/~��a�$���]��Ǚ���r�%k��\R�g<�yeA��6��
K���/3wes�P�ѕ�و�?�g�w����蒿�J�%�J���-yCm;p%�c����8��.M��f3�8�I�4a�0��I�҉i��o'���FY$�7�I���6ĶOX;a��F�b� ���2�g�t,�;;i<���K�8��ri=p����x8�g4��	�(��V��К�4v�����)PM�"�K��&̹��D5ۜn�������=L�����L1��j�$S���[q��f��ș�(9�Ϣ
��t�/��d�i�/Ԋf*g�����v���۝����U쌋�
z�tU�|��롫�{��	���s��Ǉ�99ΖA����!������9����A�|�XN����A�?��w���s�/[�@����Iȓ�u�6)y[�qk��o;�O����gv��i���L�0����YCX�MއX��F�y�7_�-�l����j�����V���o;��~�0X��I2�59�f�a+X��u��-X�s�a��6��r/�RH�?�ƾ�"�䜏���8�:N�\�T�s�d���R񶈞���9���`Q������8�9�������Iq�zV�����F�z�)�7�W=#���*}K����aXCJ]Oz�a����59�hfŷ�8��F�%-o��M�{�����,�9�C�z�����л�2�2�<ζ���O�"Z��n�a1����:����� M�u�����D�ɕYo�����@�^���|Y��N���.-���*��ԡ��AJw���9�,��69�l����M,�p����(K��|l�<��ɷ�=r��d荫0��RF5���Յ���ܐ�{���"�M�5޻iX�F\��2jk��&���ְ8�è���d5;�!i�'����6�㩅�>mh:��P�w��QS�0Fsr�_ ���Ab�NN_\�����NN��R�@:�6�p�8 ��yM�N��;9mt[����������Q@��6�.r�xϑ���:D��k�F*�r��-� �4θ�]lT�6n���q�����m���qpe]�%
,ʺ ����L�T��8�(+!B �se�R	�te-�vHcNy������ɩ���
�'��~���8o\��ʁe��o�'��ٙ�@9�,�#@O`Y���@��Vf�A���!ɠ�A�Ƞ�|�	�?��_����-��r:`n�M��9q�-�PN'Н�u�8���P�p6��I�����3R8�{",�Q�i9��P��kX��|W��r8��A����("�۲"R�Xɜ����圕Y�]��;��Ekr)_�Yo�R>+��X�L�G�#9H�r�9���J���h@&�c/a�4�߀B�Ϥ �ȣ\�	�zr��U�2�s��׸�L�nk�n�"�ԕ����%��\eI�7q��xa��s�@�OF���ݖ����c�5�T��tl��e�bB$u:�xD�L^��^	gU�W���=pa�v���J�xP��šf��E�Ǎ�-�!�mET��r��7u�<���1F�������̑q�6I����ː��2lv!�paDCO�t92�?cV&39T��'�rd¼����in�oR�u��]}=�P���2�c�0�*s{�-$N]��b����^�<�nd���c�e����8��vQ�{��]�����k�+�*ʅfܰ�W[>gܰJ�(�+yn�M]��LO8N]��'��i��P�'��8�������/��:�a�ODݱ������f�m��c��)��mWݟlk����]i�"#�L�J�eij	2�#G��o��#G�m���TVg����9��Ib;�#�g[�r��FV^h[pp,?N;���)��?��#�Co��:Ư� ������)q��x3�B�a���d=J���L�y�w�g�=�O�����#5�U�gʣ�f��M�~,�F�
�l�'���|ފ.vD}�e7;�jͬ�*�ѡ~���<'��L��,�^��f���M���	ôS�9a؇�S�0�t�Nv�O����)�A�sJ��)����G�+�svg6q�9���n�A��qq�9�yѣI�Ͼd^h#I^䨏���Y�a�]Ol����mdi�y�Kv5�8a@0#�#
�.��ձ8���� ��@�<�y�N�q� ��s���c/��	�9W�S��<pV��<���tVuUR!���kY�~Q=|�6�%��Z�	H��l]�~��RA�U}o���Dp!��o(�
.dU�Y_\��Z�\�����v	��U{恫z#��_Ûa�_T!��J^�L�*�ۓL�Qӧ�c!GM�M]��r����p�Dx�[�s1�ۦK	y�y��x����AN7zr���9]���E��RӋ?-� �4\�_���u�,خ��M[T���6�}c���Ν�=A�>�΍���-K���y�A��6���,�i~�^���Naz���ޣ���,�i���^� �P-H��w_��#�Zٜ):۴f�l�z]"�9l�b��d��	�%
����K:۶����m[z%sd[$&��s¶���s¶�E�9�'`BVSN;��-d�#Z�r�{Dpޒy�?�����N�ڎvH��an��q�p�����F�+�>�0����F,$A�(���?0c%w�.e�J���IN�ac�lG{qTF�9��\��rڏs,��	[-o�g;C�jYT��)u�!+(��������|��W_}����C}� V����������-۲U~g��1}k�;v>�O��.A�1++��ѩ_<��S��J�E���>���HrH�a#ˁ�so�it�u�����I�p�ҿ]����G%	r䛱��
�W՝<��sɋ�fHц�V�w&�ª�QI���9����9��������^X{��0��^��"�C��E�גUI�u��n�ȽjU�8�Y�q1�Ӭ�U�^񛪍��N�Z7�>��m���xm����m��Ȯ��+�����0��(���p�,genl�+��U�+���2��kp��v���/�_����	�yζk��Ş��ZL!��Ȁ��sݪ�|U��9X<W?F�&S�ct�^��c���X��c�9�k�D�1�U:��z��<'����l���lz4?�	W� u���aZ7��DL�f��9G��n�b��Õ��:��+đ��
͋U���5#�t�Ʈ�AU�!G=�$�n%r��"��� ٫�Y�k����ς\�v�T�g=����Ȫ�݁����\�̪��#p�|*:�v[x맪�ˬQs��u�Sor�zŜ��t:>�qqc���s)�x�wGٴ�+m�:2��I��O+L1��쫳	7��3#ﻳ�]������Z`Gـi�B�ԙꍥ?�'��*Ɲy�������N�4늠Nl}U։D��*���=|/X?��{/X�u������s�-6=��γP<N�1�z��,iz�э��~�N�;�g������U+	넫H�ĩ���$�#�xJ�	�"#\��Nl����I��+���-�vMz��J`UR#��~[�Jj�خ0d3@r������i�Jv��^i���w��=�ƽ��JkV�#G%,Y �j�}���d�Ⱦk<{1��
[^ ��|(�и�d#��?uG��f5��������B�g��9�c�%4⻭�P���a��*���4|̪�?﹖��g��ma��)�b?]����]AdUI��7��(k�㟑.�s	    5��ϥ؄��۟�l��㟑1����ys,���du=�M�䜮	g.�w�Õ�U�ge���ꥦ��DF��ye|"�����@oTڝ�6�A���f����nz=�	$�9��T��X����w�҃B'2�7\�.�`kY
,�K��s�&sE����\��wbr���Fx�y���^�X���j��_�2��+�&��۴BC�i���I���;��+!����>�������L&(�ŧ6�B+�f�ữRv�q��	��8�:U�D[����9���J�Y����d����9=C)O����SZ;�������v��get{2:W�|PMR�N̳F��g��X}��(ogXZ6gXJQ�ΰ�����bI����)�ɥe�*���;���U<8�:Y�8lkC�<A�Mk�?O��im�q��M��p���ֹ�7�B�)Tu��M�P;���%�۴N���-T�K�2t��~,b <Η܏�f��K��D��m760�FSn۱�!cr�������� ��b�H�׃�Ț�7N�xu��89�C��ϗ�C3Љ�}Lt��o���;9��tl֓:9 �2��ϭ�ك ���X`I��O��]ǈǓ:f�Q~�w�*ٓ��I�\�l,�8�ȡ�t���z�sS����9g�۟{���B���*�ì�?e܂����pV^����U͐5��p�<�dP��2 *��o�.Q]ɰm� �r~�)���F�<����35������(=:�h~���4r(��n8H�,R�h����9�ƺ 0�����+Y�$�r� 8s0��y:��ld�x�K�K��σ����?�6�A�(��}�Q�~�ͦ#�:(�pr[�^�R�I�P�G��{��;�ģ|V<ʯa�@��z+�]N��X;�e���#��%���6,��x+0��Ͳ����UDʁ�� p�J���Yq�_�Fy���c����2&������>�>G��RP�YI���Zߐ�!�rn{��@0���y��0�<Ё|��]I���;���C�D���l��٭;��F�x�:��|��L9_��F�_�z��ҍ� �=��g��k)���9���mG&�\�a#�r�ǒ>M�*;Ď�]G�9�Wr)�~��J&���vUr)�� �)�]�ՀOI���	L��}��8�������P9�ج$�r.00a�0p:w]�Ҟ�d�y�[0
Tr)�3 ��R���ۂxz�n��\�.�ҭ�K9oԻ8��>�N�c�I9��9ٔ�x'ٔ�8\��dS����W�)��n/ɦ,����,=o �����|��ž�	�O�kigT�;����'6 ����eɬE�sl?l��1�����a�rWj�)��+蔏};�R|ʱp�����mX*���`o���GJ�w�I�ۙ3SUym�{ZTR�#|�/,N�����ٵ�U�Y�<�q*�(8��� ��ƪ�b?��㯇� �r��T����8�S�~��E�r����[U#UN{?U-Inz��⢯3��.�'�)m�8��'�K?J���X9Ů־�6<=S:�-<�JG�k��Ji�oS�6\E����ƪ�"���_b���vm�kݕ��ڥ�c�-�iW��H.�_�k��Y9�T�5o�F��v����>����q��nڜ�F�|i0�ǠT�Ņ�j��)�j�oU�UN���wb���l����Dos-�J���}v�2�e��/�"�Ow�!�'���k�>�gvJc�;����';GyN����jת1+�8~1r�o��)�_b�W���=�;���X�ol��U����J�F��vέ���Q+��j�!B��p��~�YkR�b���h��"a��)�n1�F��"�V�۰�`��)�k0�傍[9E~��˃ �VN�_��ИL�^�5j��m]#WN;�Vq�r��5�|��+��^3��{�������Ț�w�ڏ&w�újrߕyyJP����1s����mB��2J��A���U����*pè���8t�
�ʇ�kř�J_�m1�F��2C�"8�S�lu�]�Yu��"ʼk�A���a���"��]����
���~O�o峔|�G(��K`�׋*��,*T��zQ�NmԊk*���h���E�:��M%�6���j��_��%|8ˠX.Q�CO&�Cz&��4����j��)n�l�V�3�;h��j�q�e5~�׋¸Uj>ƭ��;�k=���1�*b�Q,�}��zщ1,�}��~�B��o3�]mp�L����m�4s/���>=$��_WW��n.p��'���x�X��d�e_|��[��9���$F��A5暉2E�����v{1E���1����� �Z��w�B�i)��r���qP����n��0*j�k%*�D���JT�f���3-@Td�Jv ���j0�@e��p>�w�W2L;qnR�+�N��Zަ�R�1ᔖ����m��A2	�P�6 �)@P�X��,7R���ƺ�T�)
��[����[��r{�/@P>��ǭo�����櫋�'��������@yC���Z�r@ݎj�
��ԯ���<���肉�6 �|�"��ϕܞ�����:��u��>�x� �����3~M��tqį�.���3k�=�#~!������u�8B��F'>BZ�Z�$P#`� ���6�-��M3؆ ���>��
��D��Y���Hvg���~��L'�5Oh��9`���4�&�wn���w�9��\��J�8��B�1��`���?���(k<+\���Ƽ�\õ1G�1�@���Ldcކ�c��q��6#��n�[`R㥮c��q ��	�M�y����|�'�7��:ʮ\BZuî�ѯ���O�j�u�[��cX��|%@𹫸瘩ȓ���-ڭzx=����zt=�~��uO1Kڵ��Q�٪�������;6)���p�tk>#_�x�d�z���D�B]5� �䀵���@������c�E�̓7���C�]h@=y�چ]d����K����E6 ���wi�h߹�����G[�.�zm�ݫ4����k#�X�1$��-M,�}D	��W�c)�̮���͢��>��hl���m����q��q��|�;���K賶q�th�>���;���
�m����F�L_K���n�� v���V#�)O��lt[9S|�����m ܚ2D=������,ylD=m3c1O���1O`�dx\�b,��<�k�����
Ϯ��
^���ʍ�������t�� o�=�0���t|�դӥaՄ����
EA'��۬�b�����fJ�U��yv�ë;QO>࡭=���$c3�b��{ro�&��0I��[;S>;��fg�'�L»{�g_Kft�z"�����0tA�Di7�?��?E蹟���F	<���q���?cw��<���E�C+�>��<��}>��B���,��}�.�3]�8@B>#�D��@(���v;J��?6����vb��e���?�)�V�\J��w�ǸUۮm\k�����Љ~z�����0-!J��Bl���؊	��ϊ�!u�3�[�"�vO�^x�jra�0��7W1�����\�0��*^{+��m��#=�:���<zs�������B
���L�[x[AW��|� �8��4ؾ R�Tt�G�m�!��6�?+"N�ux�%U:��:<��C`B��=�8�?K�	}V�4����m'�i➵�a��{��[�O6�e��'��N[W�鐂�M��]�M�z��oMC�"�aT0r�>]M��}��RVe�O���*iCk�ee��6�ggذ�7�eG�G�������O蟶$�Q��B"�$>�.��݆M�'�)س�M���i1�=M��e[�G���f(!��(�s���ϭ���s��5�t���w]���	N|ڌ���DZ����88_;1_�u��b$���O���8xR���_���p���B>G��(8�}4�
oU��˰C��1l��kbm/é��h�B���FRϲ������)�f�aʆ��X�s:�_!    �PY)ʻ��B���F�����Ie��~Q�����=�����'���~�0{�һMa�i)��L�9ءVm/7��Q�щ��'��v�N�;td�Gq�UkW�v�"ow(JOn[�75+c�U�ɍn��U��cl�TOO0�x>d��&�z>$`�u�z�2PLŧ���0�����U�*��Q]�^аz�����Ut�:/�h��m�&o�6y�m�&O�Hb1���ք!̳��q��/ ᨌt <ǹ����w�� ����@;������P��(���߬�F�4�e��@:�>�
���m掅} ��$&
)pN$��R�}
�sT�X�y ����/���u*w�I ��JbQ�A�B�:���9\�ǘ�9����S��nU���?�q�1����a|��Ҏ->�.ղ6).+�6N-��Q,�b� �)�
50Nف��昮�^�дk�J�E�19���.WR��n&YW��Λ����v�3X��m_(��R��%���w�{,?P�s⪱�<�9���o�`aϟ���y3r0�J��e�	�s��,13�r�t���&I+~�Q�6�& N�er� 8�ܚ@8G@C�aс����m���D�"}5�C�ޞ��]���5{,	`3_K��ڮ����LTn粬�n��Ò�Nb=��s�1t'�
nf:g$�ЇLߌ�����L��a�N,%�&��0�O�k�@2�]�'�L�pup4�dJ ���{Ȕ ����)�5��2e�؟�-FM����R�y�Xk��b�	4S��
&�Ly@��)/�� 3%=��	0S6�]LN���1lf�f�ư����zn�bag̔t�� 3�0��	+�nVTO@�26���}��٘�5�;�]ð���oV�0� A�l��mUV/�תM��+�5�f�Pa��l��d�`sq@AD���-$�8��Q��ھm6��O��[w�Va9����W�<���l�j/�ر�dg����	Lī�嚴�4Պ����zI��6��J-���֔����{ kJ��j�tQ��4 �@�9K�p=�'x�2��)����m�:��y�Đ��͇OK�*ae��7�E���������h��D7�>Nt���h�VoʞjK�7���Ó��ɆQ �){���aݔ���:�]��q�ӕ��\lN��딒�]�������:�\[˥T���̥\U�h,CK�r��4`�����|7�)-��-�O5ʁ��:�=~��9���T�FH(�>�Me���p]�����4���	Vj���i;U�J����7�8+�8Yb[o��ɪ�$���>�UѰ�%:詪�Zbg�T�3�aS1��Է��W�\���+\m%Wq�ZN�����K]�U�U�J��m�$����j_�G;2$+��¬6�WVBn�դ��m��TٸO6���ʞ�Z�v�c�S��Ћ�*��f|빸q�avG�v�vVf#��A�J�n��\t��E#�YG4m3f?�G���*~�3����a��i�X����W1������s�k?�)�uE��y�����P�8ގ`��*�#|xb���<�~G|���4Oʬz
64�\�Ş�r�r���ܮ�ժ������Spn�>ط�*ʬ<�_�Ut�Z,�\C)�}�5���vm��k$p�]��	��٩�N0�ׇF�1�>�����v*��]��lݧ�[e�dnu���V��Z]��VW�Gtq�GU�?j+��Wuk���W|4���_�Y
���/��������1���*ͨE�^����0
�u�3m���:�?YÕ��~H�g臔�����=�G�Ry�]�ʚ~�Gv��	�ufؕ3g����Y�b�)��j�b�Ό��[�k�^�ӊ��7{�N������x��']�0lb^˵����cإ�=ۂ����e���@''�*Jz����$o���k��U�k�b�t^K��z�>w�^Ѿ����	Ϟ���:�R���T���k/����������Ύ���';j'���:��-�	�PgG�eR?ӑ[4s��%Wr[�X~S�T��Y3h��m�ڗ\��ki��5i4O$Ӿ��A�Ä��%Uj8?{��}IU�Y�� o�|~�a��h_V5��g;�i�w0"�[��!B�4��>f�m�6�f�r��E&l�q(}M?��0{e�ۛۗ��w�?,�/@�^�̳m�Z��7%3ɺ^|����+�o��дL��Ｖ����o�B�b�A��o�bb{Ծ�[��]�������(ܾz�X��}��	�X#��[{�c#��:=���-�
.H����K��д'��@&��]��9�'�e�}��{f`s���Q��~��׾��FmR�1j���c���-uv��k����B��OX۔��V���w�7��'�ª����T,�tO߅�{ݽu7'��xM�IؽHwS���^��]�<Z�+#L�Ė��>���U9�dl�~��x��3�����0�����q&���T|<axN�Y���	��Z��+�����M�j�C�7i�T�.]�3���e���MOjm�V��Y-Y�vi���,��vz��	�����Ų8]�0n�;�훞���O���ywI�Ǹˣ�a\s�%��_�#��{�Yח����]�0Q�/��y��	��SXq�
�Nt�ҧ�+=�Z�T����Y��Պ�5�^9-�%�c�4�����W|s6�����x�&���|�I��l��Ѱ+�u�I��+�
.[J��Go)��׸��j�{�S:KyO9��Ky��$mKIռE�g|�Z�KN5�jyöbո�m���Z�^ͻ��7R�b^�o-e/�Ye����j���疲�n�ڄM�Y�T�%-�N=�+T�|[7J�+��� S�D���[b�g=m/�<��ŽT�$��ݘ�_/���2-_0�Ƶ\>/^�q#�TT��p �-^溁���T�������	1k��0(�<�t���ȇ3T���ȇU|�GA;ff�~$[��ꕑ1{P��R=+y��TW1��8��u��ڤ�c��C�X�in*�U���Tʻ���L��i[�RS%o�F���5��饼;�_�J�ȍ6k�{)�m���ʰ�󬻎ۀp��ފ���t[�Y��%&~��F$&~�`b�/	�\�.Z��s��Y&��x^�%����%�D�����î�1�%�u���c��]����j\&
�Ќ���g����NePԒ Ц�@#��"�F6�(RY��@��<���Z�9�k��$ �m�q!�ۺB@�uu�uW��/Y�s����+nwa.�mzS5μ�G#��6� �� ��QO���ڻ|�U��˻��u�ꏾ��\�W�~�6~��Ͻv�O������u!����������g>������˵����X��1+^��V�K^�x�?�x��eN���!iN,�,iwȳɽp0b�er���@|����e��O���f���^Z�4����X8(c~�{b�`�=��g�Cq�3����R��'��\X�a-͙���̮�cMU��+g��TZ�@��r֞2"[W�DD,�U9������peKR.�ѝ�c+���X� ��E�8AoY��+���N��p-���:2ȁq���/�7����8A$`{�,��)�L��QEв�U��r=WF҉�J9Q-�ܪ�<�4�\��-W���:U81��x;����@���Zn�D�%9-ʉR5D*��}��['��
�h���ʖ�pn���q6 ��9�A��wV0�6�`�kخ��I��r�f2�S�J]��u��y�d���GV�:���U�NB+Jc�}<kA���A�)�������JI�:P�ա��Y��PJ2��2�9
{��y�9I�s��iɨ�D��ܔ&��d,�v;���*�E��U��y��l�S�h���x��TN��X{,�Q{r��JIhA؝� -��4���" -��H&����Y��u�$m��}��D��[����[^���
ғ���S��є�:[��4�m�������d��"nӾ8ƹ�;�Aαg��x�θ��t��'��G�ŐN�N�>��߅L�O�|'C�^���<`w.�$'ۧ���8�x���I�����d�e�U�Ů�<����`��X�s��[��    ���3���\�mM۵��;?�q%�f��R��v[3K�ǚ�|J�Z!����\��%;ؑ����� v@�`�����E?������r�Y��Jq�+�Y��x�*Jq��sP+Ž�[�kܭ�р�`�j�����mZ|�Gv��DӋ�4�]����[
YǳY(��7���g!��8��T��[��no��<�@���l
9��3�%I�cIS���g���HR0��a+�G
����,H�����R�N�|�qu�ȗw�
;1�cVq/o�v��f�D��s��˷Yž���܃�d݃�ފ�~{s�v������<6,�)"�]����ʄr_E������R���"w����+R,:�Xg�9��w�M���z��DKN����(:�Vόq�I��S��:�F�U�ל|�{��t��c�B<��d(�0
ӕ�G~J�g��}�ת�*E-'/��I7��ٗτ%�g�f�q����j�>�^���͞]�e� ʉde��,���%\6&��_�.*wg*I�����tXT�I��n�Iٓl��Yɖ�C��ǤZ>ΰ�Pu%k���q��rZδ�=�D�O��y��+��x��y�ߪ�Y�U��X�Y��
k��i�ϲ+"r�峬�P��,ﲆN���DU'Y�x&��c��l���I��U�W�>ut�声�a#��鏥D�����ﳷs;�N��3��|4��u���x�B���;lA:\*��G'��<����>Ϟf-�ɛm�g�6�H����$���UE,X�_#��i����O��JqB�6�k.��Z-��*��\뵵JXjMK�w�R�*n�{��[�~�ӵ�{�ĺ�V빷�5T12������b��[��*��[���*����WϭU�n�꾖���&Z��Uj� <ί�G�95�k}7n�d�L��qa+��h[9:�@��������u�#��^�0U���U�;fvَ��q_ŵv�8�b�	�V Ԫ�ۮ]J�v��ζ�|�����$X��r�t;a���(G�j��*-��39�#͐,1K��HTİ{�ذ"����%Y��������W��;�k�KǸ��gYA+���Y�(��Z{�Y���g����Z��+��j�<�鮖��t:��j�|��hU��nw����2Y�P���v����~�Y���²�!�r$�LQD̥�\X\J�}2�gy����˳sn�,Eze%ܒ֧��96}���9�b?_��	�r�&ŲRn����fr��,��
�a92��KH��s��kM�Oj�w��%�q��9����	hN�<�������tU�j�9�r��ǅ���������>{�)��>GH&n.�"�V��m
N��w��P����h؋x�lM{P,�u�&vS�N���_̮cȹ�q����5K�۬A�sE�d;��Z8zh������Ŷ��C��
L�v�ND� �Ĺ�4��~�����ĸ3b^�G�p�Y��w�e+Į�r��\w����<�K�K�k�׽*�aw�,M[��cym�M�3,��m�՜cyMg4�'=��S�2��Q��,��q�8Ϻ�^�m�Uu�Zp���5�T9Xřiy�5�#
�N�YO
G1�6���G�d�3-ϰ8$'Z1ԕ�r��lfi^9x�D�Y�Ht����DgZ�|����m�H�m���#��/��N�:����
�G~T��)�&Ī��bi�U�����
�ܩ5������ٯ��aU�~k�-Ƕ+[j�\ۮl�	���v#1�}W�G��F�e���U��RO�e����_���������<w^�K��<w^�F&O�g�w��[6 Ѳ���l�<��gPg�cOh%�Q��ɳ	�_��Cɴ[�L�F��H�e{	ˤ�ђi��R��M[x���#�f�����
�jMl<CÆ���G�5�43��x�i\5q�ȴ�c��Lk�������iɷ�d\����',k!��������A��$�&�rd8�;~��vY-�ay��lDI�
{d~���\�'��^A��8��v��!������?%z�؈�~�������66�-�|�\ʢ��u>�-[k�'?����o=��0�i��,lRW1|�g�N�\�4cy���=�:�]D�k�7=�%��^�ÀX#�/=�y������j9�r�6NdZVR� -�$ZVRάm��]/k5�<�vm �])�mW��:�]���U|�i��7
�����Z�626{��&��:�a�ȱ��wA�%���5<��Ȱ<�s���Y	�g<}���ѵ��M)2,�v ���I��Ƽ���E�X�}��E�؊H��1oɈs�X.�y�N��i~�GPfa;K����*l!��e�^��G����޹�{ˎ���Jބ���Yɫu�ɕۃnz��\�9�욙�����J�z��MH�2^�^��y¤��.j�������Z�&���',�y$b�cadW��ޘ��E�nk����62,���p�;���ha��;����LE���HL����iO�|V��Wy��ʂ#�*�=�y�IZ���Eg�\﹥�{nM�>�w�Tn,=��<�ܠ>>�%���Qy���	�熂\E�n���1g�a�^GŮ�Y��Q��A����Q����m%�R��(��� /���Gy>��Y�eH�(�s�����!˂%I�<tK�(��d|{V�f���H�<��w71�RfK�sTO�ED$PV���s�'�8}r}�9CXf�*Mi!�HU�x�����
�h��c�!��Ѱ�p��mc=����/�a�G�s�h8��[�8���}mHa�Н���3�T�NX-4^O\�P;��
�M,,������Z�,�!�����p̓�2w^e��F�G�CP&��B`�6���c�x%�JT=�O6cCτ���*���	��7H��M����	���ц��l���=��b�'}r�����Ɂ��~U�GQ�г��#�k�D�ڲ;І*�W�pV�蘶K!}rڐ����D�]n^Ƣi�j�'�7�F����&l�\�p��>woohR���x"�JW�>9�9�suCo��IxH>�H�63��/��z.r�z$t焭�z"t��	/G���;l�m�;EE��t�QN_I��꟰5��ÞC�����Aw�2�\T�1,] �r�����#)�x�2���)����Ni����  ͑��-j2(ϣ{։Y��1a=�Eb����C�T��Ld�+���$&�65�\���H&B�}�8�>H_r���h�P&vV�ü���`��[h�C��q�Y犵Ɓ-��Jɕ<�(`�ց-��J����s�����qa�NG2;Y2c:�� ��)�L�8z:�ٶ4�7����>P��� �)�9���ժ�F%+�hv��&��l�[\������[�@�d%5-��-Y�ųq_ �^'�~O��F<g��#�����bw=�ED����䷁�X�촓�ɑ]��-R'�(Xpj%r'��X0�s�=��}-��י��������z�������X j#�rz*H����6m�4�OH�����Mr'��Y�	���j��N>�0��W���􄔡3���59g+\�2�\����1ׁ	���w���܁���V�^�{�:uW8��:sW�;s�߲�)ԁZ�<�:`�X"]r�o��)�}�y���e�09?��|��E�S�}�ɑ��>c�}�3N�}nPhGyS~n\�x#�����L���Ŗx�z�3�Ym��?�-7J#�Z�׊=!�J���g%�]ɣ ��m���u����?�L}ދ��C���2V�M=�3�аz�S�]�z�S��IC��mM�0z�3vZ����	mH�t�9K.r��K�'J���K�'ز�#	��^��|�� ��#ar>�+X�����W~-���t�4��E��DM�y�.9�- 
dK΁a���,�Q{5��u���:W�a�+9v2�6���3���������aUL�x��ĉK�}�0�ĉ�]/0M�)� � -�n�i�TF�xM1��/�Q��Z�m�(y��6���|�ԙ��l�S%�ٌ�J�i�*Y��F|Rj��:s�J��#��)9=ЕL��Ea$    JN�3�IN�����[9;%�����8d�M���� Urڰ��v���>WϘ�$KN���ْ��p�C�r�Mɒ�x��#�.@"�%����ɗ��[-F�d�ڦ-�{�_ˉ!o�����D�_�9*X�������a�ua%��MR&��~/'b�-q$LNG���ٚ��:�I������.9�sݲ�K����X�a.[}	�8�"cr�g�6a�f���M��ȉ|��+}��y��p����=:o�q�7����MN�	��MN���0�79�(=�.�0�J��t�\+�"mr�8wЊ��@׶dMNҵ��s&��Rv����cWq��9���#<z�19<�UPA�|[U�ɏU����U���G��79m�<��5>\�Ĭ�I���+��9g�Qx����0�Ƃ}�L>�lc���نk��J�x+�^�YzS��|gM�����3�M��w��<7YŦ��&�x���&��Tc�s��d�����8�Xx]�'[���0�ֱ��W�Z������Y��@Z�b�W32h��_�r��d�z���N�\��q���E[��C����mn�O��4�m�:7�i��_�t{<,��I=�������9I�~����ɷx��7-��9��S�s����(�s��mʊP5��/;9����|����~���y�6�$D��K�ωgKѬ�6�v=�Ì�3�ӥ����ӭ*u�Ou�!�8%Z����D7�J��Ur��矍v���"Wۿ�Q�?j�ש�Ɛ��f?��'���[����Tr�B��}�x���ǺN�*sԕ�yMu]���빮T�̲����U�a�@�6l��<�64e���vr!G��^���rr9⌍OUF.i��]Oٶ�U�Uɶ���r:�􅊗N.��goÝ�~�9}w��$DV���K��ս�I[B;)�G��x�����>��a�M�՛�zu���32��	��2h�v����׼%V"�zw'D����!?�z����b�jZ6�����o�@t��sT�Y+��9�����r�r=��͉�r�b?*R��CWTh�S|�=�?�xXz�;-�.�+m�]ͨ1��f�D���pw^�]�W���zަR�6m��E&���(ư�e��w�P�iR����L�YVt9G�����rdY���O�9K��L*(�1��U�<i���#�<^��7#2J��)"B�]�M?͉@����Z+��
~��;�hMw(��?�1�؏-N��9��y�͎ �E���x�����A;���[uTu��%����;rH��ݩ�[x����+¢ڙ�?�p��[�Ȅ|��nR!��/c�hR/	��Jވ��E�J����"p,	�BV�^.R!���:��g'���28����kv�YU|1�H�U|���N.��3�"��I�U|f܁��ۈ�Į������
�լM;���ʏ{�Z''Z>7U�*�AN�`�AN�&�ó��kSUx_��
9m��N*�4���X��U{�k��s[U�.j��m��B+��ڞ艧u�K&���e�+xw�k���� ��9ݛ�N*�4_��;��HQʄm'V��9a[���s�mz�$ݶ�K�9a��_y��*��/�a��z��iE�s?�ؓ;w|K��Q��2"�#�%���z�F�>���6���=ߡ��2d'rZ��[���~g|�'=�h��5?��;������x�h==�����Wr�6	ڜy�^��N��q/i=��"o���NV企�Z���3Xڥ���w�>�{���bp�)���N^�| bPjv�"罏��a�ɋ��&�T��������۾z��V$=���aY���(�N~T4�T��~�ke�b׎1J�E������LO�Et�4�oc����1�2���D������i֞�-_X"G&��z�Ք�����a��9��\70$CN�kNTqy+�;����Ul$Q��I�����%��{ȗ�:��ɐ�(����rw�!G=!���ի1�줴�9��z�t�h?�ڧ�]�u�|��|�5��,�-�{�Ɉ��k�,.�m�nҳF��Ȇ~�5��C')�JteW�"�D7욃I��L:��7�z*��J�\%��W�{��1�и�ɮ���Yt:���u���݇�� $X��,�@��+=�6p�uso�#Ǚ2t&�qOt���խ#�B���)���~�.�}>�C���D(��F։LD r#�D�V���?�y�w�GV�I5N�no(^�z/\�G�ZF��8��H�����&S�cl�iY��\a�^Ź�������B�ɪ�-^,�ɑ���z�N�d�$߳iS�
�H��!Y��e�uQe<2�X4���o�F�m�z��a���J훼�\ު:dy+.�t2$������Xώ�$�%�[���U��$SGN���y&\��N��/�6Z ��20S�Y�GW����]b�)���!���,�:-;9�Y�g���Y�'�,�@�=��YXq��ϵE$I� Hl�N��L���ٸu���?$�z��-k}�s�ǥI�
��e*rY�'�b��ĝI�@O�2��y¬(�Y��Nme���8.�Y��Ime!�۲B>�e����z46��h��j^�.�]����ޓ�6�<�͕�$O������ҥJ���,�9Sr�=���ߕaey�ݾ�eã�@�;��r�p��KuvJǘ�	㖗a��@u�� G/��3�.����(��dH����XJ���'\H���Q��,�*$�˫ @�IV����no(z TV]xӤ�*��.�=*�2GA��4�@�\� ��N�d�Z���kWS���Tɪ�~�G�伹2
c:���I�<3XE�dmR=�e&O��kC=~~�+)�s$����I�ϏHN�/F�Gf�^~vm�����B*��D �ا����u��>\!�	���/����;ŉ*����{�w9ir;�=�K���)�i��z�&cnc���K����8�|a���j/�@��^�d܉�ez
��+����ļ��}Ո?h���8qr=���D��Pa	 �ŧfV�R�9�R��ۮ�[�����r]�ҕ����ȼb`.�dW���jt�z�RnP8��Ӕw������tVq����2�K�G|5��t��co�Jy��`۝.y>nЕ��� ��ݠ+K�n�H�I��K[U�����L��w;�mmߑ�Z�X<�RBsw���,�>��d���)�̝�')ô���Η<��bW�@�0�p-��5\�۴�LSn�NW�	_S;�*�!��2f�ar2&�K4���l< ��=Z���$ar��f���3!�����3G��K���X*7��<i��:��4!�9�.L�����n\N�o���˙���9	��s��L?&�9Ю��99<����NΛ(���ȝ��9v�=9o�k$�����%$O�/ڭA"x�Ѫ@o�z �d2�U�<<&].��N�*�4�����lA���y6�I���}�
!urށ-�E�N��<_�&��ܷ�qW2��a�A��6�'�7�U���0���:*#�W��gxr�?��˻�#{r��t2�I{@�n�d���⡌�g�?�cf�?�_.�QCmC���p����(��}!������q= zN@� �p �����д�o�#s-����u2(����d�f��$��c3M����R�6�^�zf�Ɗj*�w�v�;���`P�t:�aS�C��x\�W2��%��=9@��N�����D��%Ν�=�'��c�M��T�zg��{�w(Z,�he�)s�ۓ>9����tȟ���Z�'�v����=�ua�gC]��t�'��>9�P��/��_��P��}�=�	_������t��s��o��#�.$��%y�u�]�t�>9�=�����>��uc�؂+��ܨ�9�Pk��N����鐂�M�r W����q,��:�8l
�W9��o����m��+c�tg8 7�~�'�@�ԯ����׭����?������ v_|eU/y���8�rK'qr޸�␆��yc��:B2'�v�_I���r�I��<����S���<�)�&�u�y��uɛ    �_�K��ܟ]y�s?f��(Λ|B��8��1��v��KB����~qQ�����h�c�٧"MW�L���eA�|V���L�u	��=H��:��ɷQŚ��p�M8o�NL�a���/���n��\5�!@��;h�a/���:c��YO�'@���N�dw��y��Z9�Z?,=X���Z`ɨ���j-������i�pz���4�q�'��[r���r��Z��m,Z��Z��k��<�r��_&�MI���+~я�M�Iވ�ͬ���)�ź�8��[؅�c&��mA\�(����Ե�ˬU>f��mV�Sqɬ�a1l�����&�ٕu&M\;ۮ�Yhq�%�6���v�z��D�v��E�3�0��vܰ�3�j���X*��i,9�o?�Ak�d82^%�$MV�a6�&���A>ԛ�lMY��+[��}�0���MW����{���K��~�#�(�Ou=�C����]uZ�<�K[��91O��ږ�j�܎K[Y�ۏƥ��Q��ƥ-���io�zb�'9���;���vH�[�?��"dw��y���t%�& ��%�=�x.�t��6l�m=����:k]�*.�a3ZV�.�j��MY�mU���]�5C���̊�x�J�~�$�*y�ƺ���􃈗˧dDt��ӝ)���}�$�@��"l3�7�6~�>�a�,w�I��t�I�$9G�q��$�c�'�^�Q��R�Iֵ�� �J%�ڑ���$�Ws%ǵ��u�U�O����T�)7�z�7�b�2��b���`��s%�=��ù����<w���h�/��dR�$ӂ�w������'"Uv�����9u��u�6K�Ǵ��sE��<������s
�-϶�f�K]6�*:sV�D�a�r��Y�:Yr=��HQAH=a��%��|��H8��tޓ�{����5���Q?�	c��y�k�1�I���5��R9�U�B/�Ś$�*�	�\�]6�*?���[�Q�Ӭ�;��	�{�D2��	c� U)aL/K��*9j���H�5��U�/��U�2a�pj:#���0=��n�Ol^H����_��$IV���D$IVŠ�ڤ�c����V�.�-���td�uA�mT��%Or�o���>�Q]��>����gI�~n�}��ϊ)ׯ}��7�;ْs;��`$��r��^�iRO��*Ixr%綽�2R�J΁q=�ٝ����p%��m���:��.9P��t�|ɹ��{�U��:��c֠� MJ�nb�&(yv$�X9�p��8y�-��]�3�e�w"��~���'♥T�8y���:-F7�U0�#wrnoL'OӢ�,w��4�pW]��H�0,R�@�s;O��|�[[IH�����h�#{r�w� wr�g���9OxBE�������O��-���x�����	����ГX򅆙�,���i�O|k0g(���+k:)%�31mH���.�òdR�c�K��I9�g�#�r�3P�j\C]ˀ9x{��J9߉��Y�i�kyÜ�W��$K&U~g�1S'�ʧc�G9_i����9�+��g�T��h�
H��c��^4D"�|/��Q�ʭE�N��BX5]�jξ�HE�^����ڶ�,�9��@z4�-�Co�*��lk��=�Є����պQ���ل�AYՇҼJ�g̫n��ө����y2��Q�H�Շ(y$��f��y/� L�$Q��^k�j��G�1.YC�<ڐq�2D�34��xv��l%2�;�5��s�7:�gQ�m��F*^�L�ˏi���Z�F��ȴ�o#�xb>Y
bܗ��'��5D�ζ�C�a��΋l��Ϧq�i�+�s����Bsv?	�}��^��C��h�*��Z����yC]���T9���!�?�Q��~�s͖f2*���y4�Q9�cYEî`D��jM�)���v�S%�^�γ.7!�ɯI�Uѻ�sd^���9�Jz�$�WI��*zi�dV�y�k�<�z�A;�����`���w�I}�h�N-�,c�`�h9f�ʖ/�-�=��T�"��u!�y��Bt��{��	��7�M���)ԓ(l�@�`�8�C9��8Ϫ�\1�$[2����n90ȴ�-~��dz-��������;W�	�'��L��E�� !�o�O���N�$���i+-��=���h���wQ�v�S�Gr��įJ�Ǽ:�:̋ϵ媲ˇ��������LGS/�ʸ�|!������8�̐���|�ݰ�mYc��>۲�d$鲶�fىu���ښN��������5��ԏ!���޽�d\VIȴC}�\VEz�)^��/��.���z�Z�o�{�������`mT���3�Io�s��
>W	ax��ESρno0d1�hL5�$S���7�0��& �30��T�w=K�/Q���^oh��1h���y���9��+yٵI�Ǯz�aM�.�����Tz
T�}r�z
T�V�z
t��CO�ʮv��'�K�n���w@g�T����'`^���'����T���$ݓ `R���Bb��zb��Y������[0S��ɨ3�@O����`O����n
�\�>���Pr��>�	C&_��N����Ď��L?����;�Z#�v����'Ć⦀ODW�-�=۬��`���D���q�m��=�Ny�hc
�h:e<�ܧ��eV!�׬K;�mVk{i�.5���~��	��1�,[���0,[�u�O�B���>6s�+3��J�!���&`�q��Xy��Ecg`nG�?|����6�Ve]t��S07u� +��M�����'�a��31�e+�c�L�����'&�e�\�)�۳ؒ�Y�Jw'`n�;8s}V-R0ǖhz��̑��%%&7���JL�Y��Tm]@[�H��|%���Ȟ��If�$`V�Rp����W��A'o!����T�"���148�4!�v��h��,��J�ȪY>V���+v�9v��zv�/Gxz���#��@�K�'Rф�yæ�U"��qH��;:ɗ�4&���d1ɼ<�S2/�f��-"��NafLsr/�#�E�tƱ4$������b��?�
�������B���?#+���O�����"�*`�y�X�h9�Qa�$[�Ӗ$Z���\z��p
�����@fR=��)�(H-=��7c	͌���¬�W?c�Z@
K�~�l��z�SH��}��π2���,�4r-���4���Zkf[�>upd[.;��>���l��2 �yl�����	O��?��O��i�{N��g�{O��i�{���j�N�9����ד�Y��g$���T_���N��E�L�>N4[�ßG��t�?5�*#����6���d��2�i�ǋ|�]ˏ9�`y[q��c.�K՞�Yq,_�t��ג�Z�����(z�t-��H���o3+�����;���ϝ �����zF���?�âf�@���ʺ��u|"<����M�7y�<����*-\�ab��P��?���uf����/�ĪZЮ_��4�y�@[q�Φ�($��i2�5Y�d�d��[A��u�f��;��,�]� �L��D�?�6H�\�V�2��$X��M�I
q���߾4���,�z�e�>�F׃�4��o�Ȫ����˺��� ŲnM<G��ϫ,��O���Y�_Hz͚��8�X�,�qC��t,��P��7�|G5H�7�&z�wߺ�Z��V§�Žu�]ͧ���:X�� �r� �.�����AW	�n#��W�XO�}�W��z�͈�{��k�dY.u������@�	���$˥^�� �r��z:ȱ\굞R,�����˥��i9�K�=�I����g��nm��U��
�9��1ϲ���������U�dwqxİ�$�){�Q:)�<3Ѣ(&Y�ŭ,���o�r�4A���$KDq+G��Fp+_&�R�1i��޶��|uz�һY��4�ue���L��������Ϫ�Ǳs����9��3f'C;�Y�Ǳo���
e�&CR���7��v��ӭ�Diϸ�4H����gݛ�	�`�D�������v�Csk:�]%�a�v�I�dTe��ǘ��+��?�]�
�b�֥+7iY���^J떹k����\9׭b�1H��)�m�%�s    ��6'�!��pp�b�0�M-ei�=��Y�:�=�2nnO��$v���_��g��������ʞX��B�ʪOM��/�s>V�t���u`}��MVe��*$U�C��+�T�CJn�t�j\�ǝ�7U�%]�ja~�H����*�U�{4�PI׭.WHq&u���U�آ�J��~����lSY��V�b�W��kk#n\�]#')�=�����EFeO�ٹ'B8	�=�gH�=O��a��dWR�E�`�N�h0,�		�=��.�$T�ڶl���e���ò�_eY@�)+�C_3�k)�a� ������;���R�~:��C�s)�Rq;d8��_Ľ��\��Ob����R���a�AN��4�x����F���y������(/��/$�.k8��
�+����R�˶�]D��b��H�u伣ޤ�+㌢�a�(�J��'�WGf-��I�:2k����ī�uR.f�J�v�#�XGa�[�n$�<M�:a�j��D��Mk�$^�ô͸�a�IG��m�.=^�b�r��8NN�ݸH����h1Ȣ�+�pk1�_+����^�˧u-�O��y�k��s���ˉp�Y8�bn��
�JY�+�����ٍ�l���\�I~�-A:H����&��Om"��_��MX�s�`q�*��%��O�Ϲ�aV��Ȥ�۪q8�Xu�XV5�-�M��o��"KU��U��H��"5�1�KE�IA��T/�v��N��*��Z��Ƌ�@�0��E�C�`�n��g��!ٔ��o��,R��e��*��.9�ϵ��(̹�KB��e��|�%�Y��ȧ\���ӱς���y�O��#z@�t촠N$�r)�W&�$T.�%�r�zs:�Z=r$x4�5J���+^�J���+�U2*���!�rٻ��M&W*+�B��$U.�p��� �r9�u��u�ȜE��m� v5��lc�]�'0�V�<э���Ot���s�[p�����+�����ŭ���m(��u�ia�뼦-�h�	���u�v�=�65����?��z�u/ֆ�H�\ڱXۺA^�Ҏ��b5y��ƾ%aSA^���nF�GT?��b=�sɫ\�^�;��C�p	n�ȫ\���Ou��ak���Y�<�7����zK��	4���\ �lV���n��i�um�"�r���[;�6s�vOgɴ >�T.�0-��=c��d�K}�rw%�e+Ğ��[:I�\�6lf���m�.��%�g��!-YЈ�M���dx-8t)�iŧ�� )�K��sї�ד�='][Ҟ���y^c\�EU[)�a�y��F��1���(��^n����=�W9o�qbM���(�����}����2���c�AR�<�m��/����mW3b��~�y����*�WzV��El������-���|;E�)/�Y��F�NL*���vb^-��Pj�ogj���rgh[���pG�mH�(o�c�**P>2!�����mG��]�ˆ弙u�-ɕK:0~spt��/�|�{%�=��V.���[�l��&�+��{���Av����']Oϓ�<�T��uTc��*�+�_8&���j(N&�1�N��+:��=ٕ˷sc��@������;rc��앐���Qީr+����T4~��°x,j�]�<��g]�a�Y�Q���ygI~��NBH��דy&�r^O���y=�gr+�ud�M�r��rٴ~��v�Y�ew����A��cp;�-�U��X;�6�E��C8^YE�j�e�"d��ZF��5F:n3Yr��E��`
:jFJ��b�Qt�u]%κ� aQ.�\7Q���,v�\���A�`H%x�g�'�_�HP�ŷ��������Y���sJ��by�"�`�-tV�5�λ�2�Q���t2YW��辀W̰q�����^=x��@�W��"7�c��hX��)�M,`���ŠA~�R?6��׃�a�ks� �/��y��4KAΣ#2+ol�)���3�'K\�|N�d��F������,k� ��J*-?Y�r���%.O��7Do�W���6t)�U�Ev0@V����-��"1S�����̾���[r��Q�Q��pQ�GD�e���g�D.���ȥ���W�u{�N�R~p����]H����O�Q�z���Z�ɵ��c�iR	n�#|,׌8�!��tF�� ��<�'����cH�\�ST:H��!�#7H��!Nq0H��!� 0j ��gm~^p�����ц*�F�ل6B7�����X��-�U�渽n��
��� �
�q��P�q��qQ��W��Ogq�+H�ׇpu��q@�R(�=!�X5�M�����7Q��A�6ր�:-hs�X�*d�	6�V!�Ǫ�6��p���MpT�ZU�沪��kբM��	0j����J����sgQ>8d>:�M��������+�-�r���*-yG����:R���p��-�ה�w��A�B�p�|N����V��^�
wX�zV2�>�{��w(�9OIދ*)��I2f6�0,�К�^;u��&�PV����r��]�vV�@��%]9ɍVLϮ��fA2�R���c�}��H�HI��]Ϡ�A��}��A	�Kz��pOR�Q��9bG���5hT=�~y�^�ؙ[�����ߘ;9���BA"���8��o�'� c�.dPN�X|���PN�Ͱ�C9=�KR(�?�Kr(�u,6}U�JjW=z����У���z(4�T�%�E�	�wBw��uBt��3��z&t����S��i+�'�zd.��?9]IK��ONuWC�>{�C9=YK2(�?YK2(��I��v�r���#?M\]�����b0���^
��<M/��-��4�&�p-G	��y�e&wUo��ʢuE��ܟ��D�v�K�t ��4����.�d�/��$PN��u�O��4�	(��,�?9��� $9��xϋH���K���6=��8����EG��kq��������hz'�HcA�\�*��M��,��M0(�t�(�@9��Bs�'�aqgf�]!��G�A�ܕ|�8�A�6��_�ꑬ;t5������oz$k��l��╬،Y"8h��T ��;;酏}N�5nM��G
��N��s��+4�{�c����~�gwѺ�Ҟ�W��;��NVfV�:��w�@��xf��K<�h�����c�F�;b,v%�-��(o)��(?��܂��wiy֘�F��`V|[�Ž���m�(�yJ�S�D�LS��ה��&N�U�]ʅ�"Oj/(�/�l?̉����&�I�|��3R@�M^o>����I��_�5y>f<�_Y�gM.G��ƍ�O��Y��{����O����w���Ʒu8�Y���L��:�N�|8`\	s�H�K�i��]�`?G����p�S&GaC���uúE���!�K�O��钟s�`K>�)!���c���si�X������.�s&��uB�]�q۰9er:"��*gLN;IX�'M��>�8�3Y�<�5�3y���I�,�$�&�ǲA��gL��Y[�]�^c�Jd�������Xr&�}�	Ϯ�������&=k=Lgޮ!i��P�o9u�͚�Ķ��YC�麚�)��o�3]w�����֥�vV���֘3]׳
�L���t{s���Y�!��]V������:�J��1�ǁU�צc<��.���nV�Ƕ����%�Z�꺚����~]/7iG'=I��.:ٔW���td��1��l:T��LZ;�z+lZ��m�H�l���L�3O�I=�t¨������ŹǛX�Qf.ĖW"or$�?���&G:�{����q��=KA��8S�9C�c>�D���Ŭ�֕��=Dx��Y�$�瞶�I���b�Y%G׽�=��?�q��*W�qf�;
��ΒG�1���Y��e�%tA�v�;��m!�Mq���U�.�W�z����Mh]�����s�r��윛�<�;Y���p���J�u�2'+�~2<5�ZvxrN�%�����\�%`���ܗ�ky[vH�ۂCJ�l���zjj~RӰ��'/)�w���'���7!�̣�c�o��l�/^�l�    +[��sΞ܏X��0���w�Y���X`��Z��� /N��w����_��`���ϸ>Y�:v����>U�����J�?�Y������s(�Ǯ#(u.���ԑ]3�Y#Hu"Z��N������XTW_x���q��e-��~��M�W�S(7��g'Pn۲x;u��܈`�T� l�;[�H�9�����)�CyBK�������Q�����7��'�cѵ�3(��m.�Π������8��zB�:�Cd���/�P�{���N�<�!�,?�Ey�C�eV�`�	;�:�Q<a�i�g�?'��Q&0���s٥7'R�����ۺ�隆u���p.��bgS��u���[���FN�<º�U,�Φ<ºFY7�Ny�q�f�p6��偠�)�0n�l��V�8�u.�1���!F�������1�q]L��Z',K�R)��.�+Hɥ/�/'�r<���:q�5�|-��d�LY���^�J.e��2��ɔ��� �qF#��j�H�_�ޫR��ڳ�~$
����l�z�e���)�ٗ_�ș3�{+��2�I.e���V)�LY;Xk������^bK@.e��LUEW�΍[p�n�vG�EU�펛�ޟ1�'��F��[O�ɲ�� �tG��a��N���"C�;2���p%o���XV�;۰f���'Q�;Ev*�*[��?�N�#)�$SV�1���<�~��G�#��st�ODԾ�F��[�{f�dާD/=�.?�G੢AV�xf n}_��Zr+���-w	��~�~�p!9��� 9a�ܵ|\b���� 9�ry]�J �V�VH���a�)S/f{�K��k�ԋY�),�5�`�6lB+J᥻'��ɰ��P�3p�-��S��m!�tʞ�㈙2�sx1՘\����v%���۬�-<�=S���tʞ��L:û=�dʪQ$j��C6�u�Ăɔ�T/�)���{КT�#�g"�QOײ��==˪Ⱥpμ_e6���3^-Z��Y%Ys>���s0^Yխ�9�"$�r	ܛxxB.��7�V9ɔK�+��QR)��|����),�k�;Z>KxY�|qc�P7�L��^͝G�=g!϶+�*��][��A��?��3(y°_��7 �m��(y°�PLq�ȰWĦHy�aٸ;kXv�c���q�(���&�"��Z�� y� ���>�N43/���\�ZW;���pd� �D�kG��f�6�,$��:|�}�T��Z�hN����0W�{�uW�b�;���孱�>9�g�T��ɠ<�������ͤVmB��a!NI�<6���]���%X�����e��d�1��gXrr�j���%5�%�O�A�nKW���`q��5y�%S_8n m�8Z��� ,V�D��q�@��LaJ+O�ߛtK���������KZn����@cݑX�f���т�u ��G�������N`��iÅS����M��[�u.�;*��W�e�LKQq��p�8Պ�%��^�Y�s�Нei_ȭ;c�X�j\NZ�%P3����%L�
K��>$X�4��z	����_��ػ�%XsYv	�ܖ%_r�<��/90����39�IH2�u�e�[B��T�z��=덝s�39EN����W@�*ds�������29P��8r&��?
��H��6 �@�dMN' �_�^I�n���~�k���	P�S2ʢ�*wb�Y^����ȼ����ɩ=�F$MNG���]\��%��y�D���K�[�W@!or�{:��Ko��Q>JU.�3���5d&S�`��-u���N�z
t���gzt�VY�S���3��=������h qr�o����IА;Y��m�x�tK���dONѹ���y�����y�1�(�s��Y_m�}]lG�{�o�5,t3�y@�����������~R�l�����&�Ø-�X#|8��r��QM����rXs#uQ&�p��P����aG���b��<�t �����0�C����;7�� ���l�I��q�#4��a{v�q���qT�hT��Xk��	�rT��a�>V�C���HJbq�T.L��ݤv���ڶO�3�C��h;vXdm]�pΣ��!&�t���o˗�@*x_i�3Yy����Bx�z)�x!�&Gڑ�b��u�;��V��7ۍ.=�� �d�P��mJN�'�����
9I�������˺�lYR]��5���?�˛�8^Z�A�U�8�I0�!D ���Up�t�:h1m
(��nJ(����N��6e�˛ϰ����cZvSF�޾�A�9�㯊_>����m��Je��6؀����<�ֆ=ϙ��^�w��/9Q�5��4M�K��M.�;���Q|��$2��mj'��1e]�33�mS:��<E�蕓">�,���m��)9x_�A�M�U�mE%�⳧o�N���4v��S���Nկ9�3�os�+�'�2&�'����F�3�Or�QM��N��JR���b,>�惞/}������;����M�}~-ۣ�7�b���Xv����6ܔM��E�ey6U��=�|j$�O>�Y$&�|��8�{�'_�L����~�ϴa�G��s&(�<�.��ٔN�U�������g����W7]�aS<�j��b���z�|�J�X�<߼�L�9_�<�,F�o^I&�:/���Ƥ�cp�����\#6��������'���C?�������
ϼf���|��3S��+Iݨ��ՋQ�����u5�����M����7[D�f�MQ��T��}��Ӫ���#�|��i���i��O
(���o����۪�Ϸ�RB9�*Dz65�����p��:Og?�Q>�:�n�(��i������;�Z^�9�a�-��\�c�DgIJﰸ�sS��cqc�;d�6���Mn��QI����0ƔR.����q]�YW��R.�U�%�O��fSM�|7�r�e}c/��ˇ���������SP9o=���ĕ��z-Dq���s���yg�	Zզ�r^b�⸴s������s��'K�O|�u�e�M�)�\��ߔS.����Cћz�e~����^�6�O:;�V���r�{��,�TT.��6F�uw=�@ݡ���y�;T�ez����o��=�Mo�d��Q�swSS�H���rye���G�Mw����ﺹ��܄�Oi6E��&�h�f��x�5���Q�3S�)���8�(�WO����q���*�͚�����of���R�*�'�-�̳�VVj�g�!���,�OBXY����E+_��уF��O 1iO C���3���7��3��#�^�՛U�CW�˱vA���9���m�v�Zx��⁒]��6�v����#1�Vv[���2e��wI�l����V>'�K`9ѭ�pF�ݫ�?&����WѭxIf��}�7.vI"N�+޸�%�^�S�G��C7�%p�cH��a�Ļ����cɂ��{�@͐ϹO�z�!�s^@�L�} u}�*a�%�sF�,'-Y��5�sҒ|�sS7y�-��_���؝�ޱ�yOS��U��qyra�;WR�J����������]yP}Zm��{�#���|�W6·�I�M�jdSϽ������s������UN��w�kW�=Oak޺�y�� w�KW�=V�\Nܦ�5��c+'횗���U�bt�cٚ��Բ�P�-K���儠\��~}�<�z�hó9�&��z�饉��2|��\ֹ���@�,�`QM\羁���%����/֘�iX�JNlu��5F��مU�j��<@V�VӦZ�:&��|��t}�%A�8�N�A���/��w;�����4X��)W5�(/�д@c��A�%��wh%�|;Ē?D�bɗȷ��%���-��\�������G����%_"�"����-�7�6뛃�J:�����th��٣��'�I%�Ó�t��9���%��6m�^~L{�tަ=Z:���&��U��Kw�1er�b:�[���N�v;3q��ӹ|6/���z�O���
T�:�K<C�2����{�#�,ti/-��eGM��u� �:��FG����2�6��^�9�1# 8ջ��Ry]    �H����>�rm�#�wX͓�OT]��{19w(#�3�.��/�E�Mq�C(�|�%/�XX�<�pI����
��*�+Hb�\�`j#'�o��Ħ4r2�$p���۹vt��-�)���>��vM�Mq������͏s������;�ų��ȇ��7��P����^{����4�;��;�^4�u<s���GAź���b�U�߶)�|6TO9�@q䳡��1=���ڼ���|6T�Jl���gC�}�f�<r99.�8�#���lꩍ\�u��*�I�L�S�F.��n/�N,��TF.����4ryo(�\��0P�wNKQ�Ī���������α�[/�tĨӋ�K��Z�������M��;6���RK�cӄr�M}��RH�WpvK)�4*pܔE.��FQ�1�l�(r�'�Q��woBY�b�����=Z�v3l�$�Mt���M�w���I֧}-O>^[��D����-D��*���oX�|��a��Vv(yrCn���U>=�`�@e�z2݊��7u���t;�>��c� �����Δ�G�XJ]��|;�"��ޯ����m�,��7<�:��L�n�J� ���H�ݠ��u���p_;�d���8��A;�����c�Ә���>���i?��n�"��NXo�2੡{�`5�֘�"�^;�@I�r�q}�g��f/%����r鲵�f�T�/�8p�Vf�!'+�{{�;޼\վ��T�͍Ti��b3����K�ؘ�rb�U}�rU;�d�zȥ_H���AϽO(�.��� P�4Y±���k�RLG2F���yo�)����4�Vn��h鼧��҉ߜܣ���s�:y�t�yq*�\�"⤦Nup�KU�c?L�׭�x5ő�&�Dy�{��xwR8����㶹�qy�sڌ���L�T����%���G�c��)���//�����{�����;~N4��F�A���y���t!e�<rKb*P9!�T�<�!��f���S/���G>�P����m��ő%�U�8���<��:rrr}~|��\$�(UJ.r"��Rr��2r1k<��:r2r�������@�=y�/���Ǫ�E�M���ȇz�x��̉z��CK?���a%[���x���&��������u�2���l�������_���in����c������Nb���\'v�0v�\'�<�-�{�:g'�iަ+d^G�=s����t&;run�Lu^�@
$g�q���<�W{vﯔ�םƔ��{�}ᆥ狠��P��IP�1饙�m��\�,hyf:�¤�=����Vd�󾆵{&:�f�s캱���u�ECF�3�y�6���i�{�o��s�g���O_1(�|R�E����AЋk4T��w�7��3�?M�A���&����T��B��Cd�;>Rݽ���ρ^���2�wΌ�s���%����k��&a����p ��%�3%����wJ���E���+:)S�O��y	�c���AGW7��TLN��~��7���-�c[��|4-Y���=д,�Iz�z�����zɇ�v�U{>��\��=�4-x��{������~#�E'?�f��o��������{�Z��t��L���|�����y����&���I��^\$#����$��I�g�C5��h./��8��c��M��f�}-�>�~���C����̃r�h3#��,����~&�G>�\��fM��*62z�Z�#���=2:| ��#���u3�Z���W��h25	�.A�S��O��X�49\���
=���ő
�)u�d&�O�Y��גZ#	z���}������@��\
'�PX�q���4X�.ig��>-`��[T����,y� q�M�۵��<q�8�I�9��ƐMޟy@���g�'��^���e�BdaS6�����:�3t���YO��v�59+��_��������s-�����ףf�&��lD?��B�&/�`hˈn�//�N^�E(Ÿ#��o�܄e�1'O�2_�B9y��UǬ���K��DyF7��ߛq|�^xF�?�:[@G&@a�_�@�B��z��J���Iw����դ;��+Ľ�v��(�����M�j7��^�(ד�Vƴ�e����M>zB�����$��<�R�7��[�n�}<j!=j�N����>~�ɑ|�9#���|��s�O�#�����V#_�.����g����U<�?�+��Y���jy���1<L����i�MbA<��g��x�LQ��p��������A�~ޣ;��G>����7*+����Vnϗ�Ge�V����퓧QY���SZ�H�����G't�Mt�S(�|[C;lSZ�H��_�tG�q���-�/aMD��%L����ʭ\f�;�D
�$��BQ�V�>GY�V��Ȫ-��T�*W�H�������Ǯ�%Зo�|+�HJ�P�R֍d�7�|(�F2_��y*��A�=�Odv4}�KY'�(�,=jX,�y��0E��F���kT��GVY��)����O�>���@1Ҭ�TV>�|n'�(>������ib�����uH<pG��r��|(�m�+כ�B�jS[�~�^j+���W��!�|�^HX��Vּ��١���{QI�����v���sS_?|��r�Է�ݛ�g��������/���M��)�\�0tpD3�=�4e���u9�=�÷QGv�cԑ��+�RW�\��\���+[M�/��>�x��>/͛MQ�s�h�ej*�v�̧W}NV��UTT�M�o�����Cڙ�H��QIL�Ld��QO��Md�-[
*AʟU		QQ���򥛒���x�zSS���z?���u}]l�C޴���禽�h����?�����)Y,Ӂ��7���r��ylh*k��Xhh*k֋(���l�ཐT�\�AՃ�ww�vL�ç���]�f�*�FA�W�x��%�:2s�?;�����j**�ʴg|��U<�2EU�!� U�){���r���9�'v9N�����J<��)�L��/�?^�D'�:���H�r%e�n�V<�]I��b'�5/�������T҅�Q\���]G�5�E2�:�(m,�8{�{�^�����6H��ŗ#N{���\8|m���(�������wT3��"F�!v�P�v����z�8jia�����Ƣh��z�p��O����~����N��~�6�|���~�L�ca��ŃVº�O��0�ǫ�.�ׁ��f��
�,/���C��5r�r�`���r��f�k�*�h~6��ؚX��(CGqk4��``�k�?<�]4�Z����y�5Hmn����h&�h0�e�߿�r.���h�X�R�������8޶�X-�t�4,�[~)�C]-�	��}�h���>ۥ�QM���hrc�q�d>���WJFq4�yۅ�QZ�#U4���+�*\&����97�_�B�\�G����H4�0da���h��O���*ɰQ�����C]��'㭞�֯v�Kt��;*1��Jx�?l�l�3	�w	;l|=$la>攨1`�����WW����'�'��8��o�]���p�9.�|�d�-oê�'#��8������Ϳ�D�_�(�.�|���XS�d��Qk���8���ɀl"�2��79��������P������	����wn��.n�!�}�7�_������O�d����^�%�����I�9�=Iu9仮(Y��VdedO��)z^�t-�k����l]LA�!��ա�律���b�-1������;�
��LLዔ��~��dΔq�1:u{q���o�.���Y�i��)�~��߄{����� �f��#�{?KB����z�Emx�/��$� �|_�y{��j��:���e�*	���I�5pE�]�����y�x��<�Lɸ�O�wY�q�?9L�˖�:��y����P�׾=ޙ��ug�"1�a��m�U�B�M<�@Z�G	�KF��kjH�(�Y��G�BC�W�˗D�B�ν�qXI��������n�x�܊xH���ު�H�����&.�y;���%~Ͱ�m�����k�)v/�x�q��˶�GekM�    ��>F�?���t���w/2����W���KYvo� �z�i�3u�,��Sw�!�p�)w��!lx_�!�oF[bHq����M�>ŋ�#�?�cY��i��Q����L/nb���#u�Zv�]��K��3�ܑ�dS��2�#y9C�)L��kp�k��C����kΞE�E�C�=�ċ_���=�ċ��y��]r�;�=�D��L@�0eb{���l��[V��f�vZ��{=�j���!V�T.������V��$�*໹9V���V�!�V�K�{Mɵ
v�>�kI���R�ז��Ί!]&m@��5�G�B&�k�EF���8J��_�2.:������ّ���M�wd.��x������ ��Dߦ�ʰ�x�oW3v���M~[��;P�q��g���"^�|��*^��9/�nk�'>ψ�L_�|�#��C܇��)��\ceK��3��Gi���v�ɰ����{q��G��=EfA�;9(��(h�=M�y���l��D����v{�8���7#��)�A^��M�Ql�cK,�=Z��&���x��ȲV��ޓRd]D��H^r��F�E�snyQ+������ܰ���/�Z���p�Q�!V�X�۰��Y11�/���Z}$Vx��!-�h�/�-��J�`�q�Sg�j�ܠ�q�e��"��:e!�~�[x���"��-kY���VM���)��Z�ݬ���Y QPk��E��V�W�")����֘�e���Z�(�6�S[�����q>���215��%��� K�W�Rxl�+�����]���ܤ�ZW��`��x��AF}[W���ޔ���x��Z>~N�N;V�wZ�KXW��B�Ϻ��G�Ȇ�l�P��|	Bˆ����P����T���IK��'�
x�َj�4ϫ<U����9!q�1�xF�n1���͘�xT|P����Gz�gSO�-9�GSO���M�YK�qEĘ�x�m�T��͝
x(��)<�l�ꩀ�o�
��R���$�x)��+.�bgK���a�s<q�f)���t�R��oy�/��~mc�
<�F#|d)މ�#��>ǯt+܉�1�r�V�s{n�y�m�;7��0A�❸��J�4O���n�;q+��m+��&o�;�oN���p�@>~M����[���Ym�l�p���L�΍�뷙��;7�f�vr�����)��3�& i�h�{��*���>T���t��ϞG�Ns,�W6d�gD�sP(P�'~uu�	}?6����?��g{�:]3�y0hoe���*/��%`��:]g�Rk�!�c�����A\�:�`O����x{� �."�^A:�O��Ӟ�Z,���v��:�E�N׬�Ğ"X���`OlO��:�s�)�u�u��
�	�ǂ쩂u��_[��
����u�{?'�'3�}i�1���^/�˰�
��m�v�=U�N��3K{���^^��'��N~L�&h'&�`�M���KS��yz_����r�F���4V�h���ަ`��s[6;�7ES���T�N�@�=���\��+��lKi�3��댩��4]�N�ޛ�u��x̞.X�����>�]�N���@kO��?��ٞ.h'fb���3�=��TF �v2\�iډ3n�:��v�q�EC�N���c{�����C��A;�/����/��=L��D?2s9K�=S��4s{�B���
uN߭bZO�:�CC��T�s�䍝
u:���s*�9�ٴg*�9�x��)�9��`,:��@��mP�`)�9�E�Y
tB��͌��J��"��Cd���T��g)�9��/E;�����28'���~76�يv��D�يv:;�#ϱg+��|�03��S�̞�x���=m+�ɦo�;���aيwN2���
x:��3<���m0<������t�qs��S������:g�x:���҃=�����"��"�ѽ��1�<}rm�*S̓�������]�<�l�b�<}!t�G!�雇_fl�Q�/B��*�	�j�*�9q��ʣ�'Z[E<�qŗ�xz�U��(�9q�W�iE!�_��V�"���޿��'q�R<��x+��F<'��{��>�֋����ĝ+��>���$�9�;}�
w�?n3�T�;�MּT���'n��h'���V�N���W�;��~P��*ܹ�b�O��EtX�*ܹy@�H�K�V�KqV�����A;���mW@$���z���2��ŝ�j
w�?M�N,J�NS�;��f(ܹ�,�ɦp�&G�JW���<���p�r���p'[����zr]2��9 ;jP�s�2���]�N����Y�
xB������a6&�lL:�P��|���'"v
xƵf+CO���*������\
{����]
{�^>W���x���P�sa���SaO̡�b�=�i��Tܓ���{R� (��[���T�s���-?�\��Y��|�J��3�����'~�7�e)��/�>}
-�>�4}�le)������uY
}.@��9E>�NV�"�����dY�|:�1c)� �@-�>z��	7d����Sv��A��K�腆#YW(F��tg`����K��M,Ň3��<`�%W��?+��g锘6���8��s.�����xC!�W{���bz@�U�'�GZh_"N���×j�5ny(��z�A�m���(�����P����O�����OOa):,���Qt�E�lŋ�_y�?ŝ��d�5�/�s�O�9��OV���F�	��z�/Rh�zy�R9-Bp�����?o\�b���uk�,�]��g[��Ն_�O��Rn�C�4�ȟF��7�s{��9́KH_�Qi�K�4*�s 0PiXȟ�;�P>��urXpW�B�PiX���C���n�fѥ���Ҵ2�2U�/��9;�u��rٟ�����<M�4� �c卅�
��Z�r&�&�k����\��hT��qŠ��	k��/S�G�\��D����i���5�`�-������"S
mae~��!!c ���)c�<�K��x��[\�y�0Z7q�>}ҏG\�O߻PQ���t���/ާs4q����KTuN#;��:Ў)n2�j�QRXȟY�7�h��A`��HQ�����,2�>|g'�_�FU������$�O�)*,�ϊ�Õ#�~�6�QVx�����-a$~̤���[��F���ˊ�c�6v��b��6E�_�OG ))|y���QRx�\oM1C���QL���4j	y�F-�C�>�f�r֧��Ѵ�=�ʮ+��ļ���˔�KbEC����۞�
���i7��p9���zg����ջ���e�I��yOy���>^.:�%ٚ$Z�XǬK��'�ɻE���8y�H\��Ö�Y��tcPO�`Ǣ��d����CJ�`�-M$~1K<��[.���q?@*K�`�}n���'�`1��h���$P8���+�u�x����ݢl��>����6�*�󥉊�/ڧ���Y.��*�Ƞ��S1x�7��S�C��y���·�Z��|8=ʻV�L���c������#ʛxI�[�k�����-�|�e~NN��\އCU�e��D÷��`'j0��m�+O��E�C�Ud"�
V:	��F��K�z�F�`e��H]�s<���_�s�������J�X�'$��Y���oN�v/g<Y(+�sy�z��A���&q��hu|�%C(~M�3�
N(ܻOY��Y���n����N��$�(����#��d�u>�Bp;ms��s(�N�
!`J�ce���^���O��h��/�Q�K�4���S�>�������n>2L/�׳ȀTO~��Y%DT������� f-��K�DC"y�d1O�Z$/�`�XHk���w�0v���K~�R9��#^�[#J�5
����SA6����O�u�B���ႫK|����>��S�uL��kJF��7���)^Gw��L�#x����K4�"mT���`��i�h����3�I�'S���/�|��4\�B��K<���U�Р���A�.W
z�푵DB�9+�6ָo-Ҙ�P���i̥���Es�/��H���0@����].	�oh�h�fb R  o"��gS�عgm�����4�����_�Ք��!��{l��)��NI(,P\�4J�8���$��ܷS��5��W�J������P��p@�Q
X9�<���r@=Y��j=`���1�$l���zԓ\O�u&1�,�2�J�4� h�F	`�yn_�� ,,Oȧ�3����%>`|�( �>`��b=����#>��">`�t�쯲<��Q���<�w�Iˡ�t�/dT��g��(�+$O<`���'h�F���n�Ʉw�'��?2��q��U�}�<v�䯐<��P�W9��@�����T�G8��sW���Ws�7�8�N�G�rH��f0
�~I�B��FrV(��nl�{q[l��~p�p9���fdx���[�s/�#k�O8d-yg��C�����`�K���S�2�Q��P��x���jI��{��m6		8u�B�4�}�ћC�FJ�×x�49��K����ʾ/����(���x:�@a_�x��*�L��1ԫJT��;��D%r<���KX�|[
J��=PÔl�7�+��FY_�x���޲���P�޷#'���?$Oj�^��B6LE_ayb�=�ay���xA֖���HYɳ3�y��`��U��ć�&��Ǝ���`y���\'q�g�Zf�:	h���R�Wi�߶�$�A"bX�Aŕ��q�':b�׻���5=Hw�P�WY��:e|��魠����3�*���l8"����I_ay�x$��Q*HC(�+,��?T;����A+6��*�����!y�y<
�^�'�{��9�(��"y��[ɓagD�rH��km��}�<G�.��ih����[lؒ�����Q�0��u[�|�r���t���"!��\	�H�q9����*��<���4�	�	�$�s0���K�t��p�s��2!�mT�}�<�ګ<O�a��^�gÓ�F�^�y���o�季�Q���k?�xބ<O_���[���Iu��^����1^�EXd��+�I?�]�r�����E(��⮰�d�
{N����+�9�b�(����>����S@�!P���@y�8��J���\J��X�T�}�@�J��@!jchr���0%z_�Њ_S��ɍ+��|2L桸�ĕ��� |�6異���7�\��3���P��z����6^9��Q���H��O>]�����}�\�H��u��!bX��b<�ĩ�����.f,>}|76!c)���bOL��<�@h�H��@���X�|N����K��ɇp�}�������}Ϳ�ا�(?y^��z^�/��n+�9���J4�3�ͽ�thE�b�>+J���*�	([����6�>'h]�rS��rc6L�O��'�����u�k�?��dn�Q�?�lS�s&���/*(���F7H�T�V�S���'�x�~D����'tgmF>s�a�]B�v�K��
�>}	�o�8O�r�?���𖳝�m:�PP����D+d��d�$�s��A9�ρ\�J��#:F)�r3��'�lfJ�1�c�(�L�b��8��_/1?H�l1��Lp��&��ug���>=�Q���>9C��ղ��%�;+�\���J�+�P����M`x��{9�xn�f@1��	˿�/��̮�\��n��j�һ�U�B�>��y��D&�!�m6�>�R�>Ѫ�V)��Y�J�]a�V^g��RA7����L�z�&�lf���_+����ˮ�'�@�PA=������M���s���ͮ���>�������+�|KXt(J.��o(��@�6*Pt��|a(�Fh�������9]qwe�@a|������I�ϸ��"�29g��Wh�������0�h�      �   �  x�}��n�0��ɧ����]��л�Xv��J*��2Y^��m)M�� ��H�I(
�bt]���+���h����e��J������]c�G��ë��?���7M�j��j�NZ�1��B����@\�L�,b���2�	^Bi��� ���(��'��� �� �� �2�r;���< ��i����k l�֐����u𯑆L���ǟ"6)�?��+(w�:'�~�X�p�q��@����6��u��C�C���!��In�z�ǥ\�.�R��<;�5�;_{Ǩër'=6=����w�f��8�1� �v���͚�w�FT�o��E���6^7R.A�J�vfY�O��'����H���f��x�m�=��\��:R�=(����6HD��t��.r���ix^���:gJ�u�Ǵ�'c+�%Z�/c��SN������������L`�C      r   �   x�m�M
�0�דS�b�n�����h�M���흩(V��{3	�Oam]mo�_mxLO�0�@�l��?�n�8�yR������vC0��*��ϡ$ts�%	�����B^BQ����ku��5^��od�:g̻Ц��]}WA����E���ߤ�	l$��-@��^��y�_��,~��Q�dU�'Gw�1ƞ1�h�      t   ^   x�3�O,I-*�HMAb�q�q�r�&�ÅP8�@yN�Ҽ��̂Ĝ̒JT�	Pޘ�-�(B���>�.`l�q$%���(#��=... M�+9      v   3   x�3�t��+K�+���K�A�r�p�qq�Vd�d�#F@��=... �D      �      x������ � �      �   �   x���m !�sm0����\|v�!�^	'�`��	�6�ۢn���v�n7��pn�r�݅|^�,(���Fv4Ȓ����� {��B�w�{!{���(�^4a/���������/i/E�� ��'��"�e��rH{�H{�I{yH{�R�J��
�^%����^5e���W��W��W���/m�E���u������7���������������1�&{���)�����<���Qa      �   6  x�M�Qv�0D����I i/]F�:� �'��<�וO>����G%��r��Y.�����cY��+KC���kO���������	ދ�%x��O�
Wj�Z�p�V��=3�������=E��=E�YG�>��'!��;�l,�H�W֞������O���S~��	�3�#�I��Ck��V#VQ�؞%s��u��ߣ�e"ì@s�I�@S	h!�U#vQ�8�;i�:�<�vB����t�G	~5B�,�G�M�zj�G�V�#L�|������uD�I𨦄h�ٿ'�!>�d���V#D�%E>b�"������YG����-�`���&D�!�r6��l!x��	u���Q�,�Z3��c��#����!�оq�!>�t��I�@G�b�Ō��
tv��B��5R������N
ݴA
��I�>%���k�f���.�9�0a������zϠ��C(����+Rn6���l�I��ܔ��I��$Ɯ�
cN��Z�T�[��+�7��Ep6��AFʾE��mR�R��7������=za�<Fa�I��@`R}��T�K&���IS79���H�6M�l�)5� Tb̙�0!N�Q U�zHU�R�+H����դ��W���X���%)rC�I�[�fFn��
s��v	*��%0����a���o��X};���!XV�A��k� Z��C�s�ww���C����l��d��A�$e���~����q���ko�֩��{���0�߂8w�`^{����/���~���vC����}���k�v�_;tz�������?pb��      �   �   x�=���@�&�-<W.��_+��^�M�ʨ��:�-勹�ofD�u�vN�v���un�v�vr���2m��V=|5F;���|�K0�-�;x|�]��JAkkvk%�ю@Z;�]�������^wAk��~-�h%�;p�N�N�
8�;�.�Ǆ[�a��e�nn���-p��
n��-p�����l��pa6      �   ^  x�=��q1C��b2+Jԧ����'@����dz��h+^��f� ����f�.��%�a���CB��0�o;��]�ϭ�T�1�}���[�W
�w
ڢԫ�Qe���2K�I~��O1�.�g5�ez��E줠�6�U�W�ev1<Ú}#'�Ymr���%�'���9N
\d����6j���Q5MLO���r�L��7+��Lo1=\�	�o#ޛ[V�Q�CL��#�)8��g����#��3��|D{
�V��j1�����Z)8��O����+���gz��ȶ�ŃP��S���bz��v
N�������9��c)p�3R���r��Y���lpU����<��.�x      �      x��\Y����.�� H���z���9ƙAD��HU?�W��2��p��(h��R��'� ����џ�iT�y����-q�1ro��@5������S�����<x�=�܃ʃ��8m��\hO-Mh��*a�m��%-�ax)G�sꃇ��b�훻/������D���qIm�۾YÈ|ۿt�I9����2<�"�N]���=�>_�(3����(kZ��{�e��}�(�r���\$�ex���p���Ǝw��P	4c���a��i����{�Ag�!�{<��J��t��%�]���t:?��iQ
�������o���"����)몙󋯙WG��_���$��>�`���.y����3���"~pN�b,�~~桥$�K��R��;�?R}pLЈ�&_�}��k%
;��G5�$����<Ǧ�p/����Xk#.cI�|����O��<�2҈vHwd7~Q^�#cX�4\S��Ӷ�ռg$Q{��X�e���?�ܛ�ё�9m{����n��慖��Ȟߘ�V�{���_������2�(_�����X��~�����ė�>?�x��ij�R�|q�KQN������'�tcf�esϋ��}���"��&�J�˧ ��"��#jН��K�i����ۇ�yƷ�iS��w�����ы�PG�0��m�=�T��q'��3%��cý��#4·j*��6�+¦J��	�]�������}��b�D����Թ��^.�qt�?4��,�����r7���[�^m�\7?xF��exb���`x�̽p^�b�w�'��|�� U݆�����g��z{S��.��o�c^$/�5+*爆s��6m�2372���/��1dx�(��n��.�����Tw\i�ǆ��V�ef�P#^^)R0LV���8CJ6?(�S#�t����WE��B�OF��I`�o�1�W�Y��י8�y���f���w�0tѰ��Kd�����z�ᅖ�^h{C�#E��杺zK-.�G[���'(��k��jIe������F���.���oE��j
�q������ʆ{��E�K?���	,D��F�|K%�Op��秗��䣐h��U���h�ǲ��6���mhX֕7#B��u���K�x�R#�u2�!�@�˞��6�F��#�y����Q�5�BT�=!٠�!*o���1{5<�����PI}�����O��$p5�����\Ӆ0 $ĕ�Kc��W>x��b8�L��dx,�>=鎏&hkB��s�	�4<�O�@\|�Z�Irߟ������<j4��p@�WÑ6�LF��/�����ت$ܓR�����B�O���t�²=c�*�����}��у������ON�.���+�Āz�V����o�W��CE��_�J	ӹ'�ׁN�7>�mDKT{~�����]���p�h�!������ᣠpL��^Qً#{pS}$�#z� �@gGo�b6� ��Gɴ�ȍ����ί\�)��dx*h��?nݫ��d�W7���H�=�6<�	�%d�H`�)`���G�n�����`�G,�t��p��
���z���4<���z�U9e���O܅��&N����Y���?�A�_��~CB2>^!�JIc�����+��	n/�r����y�s��2��P(s1eޜ�%�`~sŔ�|��p4�	'Ό����)/��%O0�$F�&��Qǿ�6#fx�8;K����]�>�1�d�/�����:�uEK./������#�|�����F#�ᱴ���N�(�u��je+v1ܛ9؎ӫn�G�<kbl���/z���D)U~4K���`�G��Y��y/�a�j�W����9��jL1���y7`�(���Yk�X(��M!5Cx�b��V�lZ�K)�JݖV���ᐠ�u۴y�ꅅV�x���73�Z�Ԏ������')�s��J���R~Ң���ݴ�r>�%!ē������{l�M���/: ���u1�K�x�D���G�����z��%��|^�z��Iy�
xV³"���k�+�Y9�
zVҳ������_���?鏀G?�&���Q�������QQ��Oh{B���l�B&5��,���,�^<���ÈH(�R�]-l|4i���۾U �$���?�'�=��QJ�7��q#K�]4%|6�����)$����"�,��RKy��@њ���n(�y�.�y"ڣ�*a1�^|�JȈ�lT����`����n+�K���~�K�R2�^gO��8��Y�Â#�|��p��lxqWE|^<2���YF�mp��i>V�`�fx�`)��޲X�§q���u��n8H81H���H�m^M��tz�A�@�O�,<����c���|���������b��	�|����Iti�B k�j8��O��l�Ш����f�c���	,����A��G��E�V6P�
�~�?1��p�e���v�X�g���oc;&۞��ڭE�gFe��XI?���L�Hs{���

�HV�A��<���
'M�%���
2F���b8��*����!b���Zق'ܘ#5�젂�i�$s7<����p�P�����L�O���_+h���3+A����j�y���_�I�
�]4�b8�<�ʶPj��/�"�'���/�/>"a^��G�\b����`�j�/��5��_���WVW��G,wH�`8�]V��n�uP�Z��u�a��T 襱��-������-M�ύ����Ȋ�$��E��A/�:��EV�S"��$���$G��f@�����X�߽�����/������c�]v*$
�GT����-W���Q^|���f�����`ϯgũ�i�YڤT%�$x��ع��RE���;�o&j͋'��p���h�#�����~�?cp�I����A�7��Z�lKx:Y�|�� .����GD홆���G���P?����1z��ڋC&!_¨ֿ>���W /:F,YuD�B���(mAi����cB`y�z���F����$T~�S�̴Ԩ�c�V ��U�q�D
���*�����f/�7�=����z������c³����5|V�_S��Β��w�����a8����exݠ:�d{���nꧏe��Ebܔ�8��3l���'����w����5��?�rOWx���
O�8����I���r�{��ȼ���r���ޛ=-��7�7�Z��.�3�(��G!���SL����!�6gぉ�BV,�c��Z�\P�삉0����z]J�u��x$*CEi3�C�èb/πW�ίK�+�2�(��=�K�.�`�9*m�.%�>��aL0�tl탬 k��p ��oyލ��x.�sQ��o�@�Ez.�u�N�~�9SxU^�?k �Qy�\�����?����.�T���<.������z�ɭ"{��s�=.����f�������x��{�[����J����Z���孕y[(��sԢ�6=��]O-{j�S���܂�6���[E�*�UqOx*�S!���T����*V�ҧ�ѷ��j����o���S�~xP�f�煲��9a��S�Ƈ���7&A�@7?��i/
�2�*���>�=��0��+��i7As�U$���^f7�
B36��6�i��w%�S��\�ሼ�n����૗�eܕ$�XP(�򽖀p�9�Z�P���H܄6������bL��g�	"%�xgW�Lw ��g'Mm{v��ܢ�=LL`,]�_���=����0��t�
�<{�Onr�Ĕd+Q�����i�
r��tE>g�����By� og,Wa�ː�b#{~��xǘ�rF[c1�.����^/�%\�����c�Ӹۛ��;�(���[�"�eU��/�=�C5�X��0�q-��Ę�`�ʻ�suk�Œ��7!�/�w��6\�I��{6�BD��a�dM�_��W�P���6�Qjx��b{��e>���j��}>��o�F����ޑ�^�}��S�1�o��>�Og{6i��D�o������o�Ƌ~7{�A�[�B%B%φ� F	  ��gBm���a�Z�1�:m��L��;��O�ʻf�7m(�{�� �6럞N���f����1��K�;f	�19Gc*��)-��~{d����<jk��\9yn-�u��Nm�-�n8��i�m������n=y�����\Y;��F�=]���5�y;э���٢������s+L4J��a��yD�������ƁD'#w����q<�G��#�g��3~?>��óI��J�;�q�h���l�aͫJ}���s�WH�� 2ߦa䗡�AO��A��ϓ��vK��Az�1�wg������`803K�+m�:'����9���p�\���]d�����>i�#w�mf�X"b[ 8ul��/�Bzx�ɶav>;~"�8*fN(�[A�p��3Գm��s�y��>F��UP5F�O�-��ypC*gܞ�G�<n�w0n͆�#�l�����ԑLz�C&��W�C����-�7->�����Nϡ[�.�2��:Y�"X�/���"�da<�����dJ|�k��ޱRD&��ʆcr�<�����E�����*7}:~�l�� �v���{�	�A�c5;�5"��Q�ޡ�|���h�ؑw��a0(G
���_��L�tWn���0���^J��_�S���dxF�5�-�R��1�����#��^����Ϸ �� ^����k{���~v���h���"f�$�3��½��<F4m�����K?	���7�/}C����Q������\4|�G��%����f4�����9ͽ�׹)�2�~o�AV�4�Կ@/������T� hluz�6���]��A5c[�*U>���Ć�X-��~2�٬�`q���2��7h}Ã$�;5�!�S8�:�u)I�s��_�u�Ά�Tc��*晭����������ȓ���*��_7�ESMOv��ſ�7/:���Ӌn/;�����n�<v���c����U����E�n�D�	�PL�$��%�'O|y��^���W9��Dw��J�Z�%2����~s@����妗�7x��q��m7z������~���Y��x�	�����pQ�^�N�-�|���c�K.#���}�<��kE�.��A��PAZ���6a�Z�{g��Y�-�:����.6��Ś)�^6d4K5�"�܅�΄�oL!R��[���vý���pJG�Lcv)weR]�����nz��{����~/��/Ȩ:���٦ť��l�QP���s��uԁ���k�<�����o;#������nF^l��1`�2�V�rn$_�he�����Y-u�R�/W����8;��G�Q=�|������:����9˰�����?1���I�u/�	?���e���/��T�R�Ҋ�Ìt�3f9���eg�t>��\J����x�u
�����Z��4+����5���EmC��X�`�	�_*����sŎ¨*i���A�81���jF��fg�s7���z9�����Q�d�{le��{2U+�i�\���l�(��[ �T��H�.�o�������\"���|�=���|V�v�/�ٲ�)��ʪ�V��M:'���%wT贫L��Gj��-�ښ�*;n���r��=*��*Þc��fΓf���D����6��|��q۷�=��v�n��\�������N�ω�q��Η�I8o��N�A_Dá<Z�/������KMa�f�Ʊ�\5���j5e>*����w���U�9M9����~���(�gڕ�C�Z�p�u�+�;��sq#��Z�6y��pN�޵��Kj���<[����z��%�ei=1�MJ�n?��(�%���٪4]mښi�R�1	M�FOv8��e����:Pw�C�����Z�TG�v���5�i�,<�V|h�2�Xmˋ��'&/>�E>g,}������`��Dmf,
5��7}�.�����������ܡ[����|Q݆gj��>�)��m�&4e��e���9 ���]�n����|Z?K�5�^�x�e��>�xN��sӰ���y�68f�T:g��a�s:�w�~��K�3�ą������pP);�.����0ͳ.k�Ƃ�B��Oq�f�'��ܖ�TB��ZZţؗ��,�`~�9���0\��1��}��n�oI�yA*�qw��K>�9������y~�Z*��Oz�p��6ݜ�����2��8�o�Q��j��۹"����+�9�s�;N�3:O�?r�u�5�6M6�^&��	�)1a\
�Mߙ��QlzP��BQ�lY|����d8R=��5~oz��u��//<�����K//=������o]ǋ�/�l�<�C�[!��-��y�_����ׯ�H�p      �   �  x���ˮ%��Eǥ��H`�M�����6�h��c��2���x3���NkR���d���;x�|s��?�v���������'9=zY2�h>���t'4	�O�ǈ5�Z��a�;J�;o���֢��������Ϫ�7���YB?�?�-6����#� ��>�w�i<��_G]�_y7��)�_>}�l`�%�>g4�JZ:r$<|�sr�u�Ó�.,'��i�J��C�R[����s>q��{_�5;nIg}���r.��j�˥�E�?�ʑ-����qB��g~�����|�G^��>���S����{6�qY���ω�k�$u/�pr���x&9���'���f}x�y�����iE"2����;ĥ�9������.��k>���x߻��=qzx�s��N]��!�Y��������e����Ev�u��WwR�m�������')���OV�sE���S�?Q�Wj)D��$!v�4�⹖�\��C�\e��3�!$���_�>�z������Et�2o;�Y���Vn�>B��=�3H��$��/�nF�:�/ް�x�z~=��V���z�mOF*�-Q��Ly���Mbϟ5'�(.�\�q����2n�٥��S������x֊y�}��P+�PQ��珒�u����{B����*Ҹp���-���w3����|J}�l���g�T9��E��6�k)���>��#�|:�|i��F�p���s�<�)�s��9��4��[�b�&�����_��u$�?)c�Y�y���(z�vH�t�l���-�k���*��~�Hh�q�9��q�e4U�8���K��M�:���]��/��He-�?�%G_5K���2�s���k�����:υ�7���5mJji~�޽���qZ�~���o�Nn(���ء�p��$�"m|�\dƵ��HM�U�����܍�AƔ%��͜�lG����P��ߨ.�K9����7��y1�?��,��X9�";G�����]��|��MOa=��p��`���{4�
����� d\}8r.2���y�?P��k%��n�
���8˹������x@AXK5��iYj��u��O���ςP��K�[�j��	�\n���QO�����1^|��(�N�/������������jN�?��W8��G�4�O��۴�n��  �����&�>���:T߆�bh�����ke��b���FL�e�'�$�h�8�:���8�"�gk~����,�;�ŸP_�7��Stm}>	K�NO��2eδ��C�Ή~[T�Je5��K�W(����.�x���x��;��o���5�a��8�9�j|Vް��_�.N��wB?q��%�� 9��g���X����h��ߓ�7�����������৑�K���<֏�/��>����z�L��4�NpN�i�ʑ[�T�qڋO���o�Ƿ���o���?������}/����������?��O��O��o��(��o�E�A��������G^â~p�|����~mbr:1�P���"Q�K�"�^[tb��P����?XD�8~W�q��&�?ڍY�b��vh{�����͍�9���K\�q��f��x�6�+��'�P�}>㍭�{;�����qA=29��g��\�	�S����,���q�0C8_,gf�+�k�Wh�8�ΐ5��C|8��7R�x8~0���\5^��]�q+zFA���-�B6�Ǡ����	Z�z�����U��f?��wP�`�\!��c�(E�����_��L|���*X�`qTƷO؂0��P_:���`
j�m^yw���)�3����a#�\�%H�@�T�b�ި�a�v��|fNw��.���� �q魅��B�������Xc0����7s��'�buƩю�YaZ���-�W��̴�[9z�L=�x�E�Q;�J4�T�T�ōS�q�:���'XVo�g:^X�>��\mH�~��ߑ�'G�[yG��a��x>�2�Dȋqܐ|���3��CW���`�z�\m�	�x]�/7���b'KS὚+����K�oc�药� �It"�k�MI��4?Q1��U��:������؊��7���2�\��}s�o.�ͥ����)�m�x�Rަ��)�m�z��ަ��)�}����Է)�mJ~��ߦ��)�������������������������������W"��o�Zz��햔��݌oT�W��$@�pV���UR���K�t(�>����-*�������FS�����>���~�5���r��K(�}` I� �!̩^���FE@S����qq|^�'�@��A�S���)������Y�S�D��"x;I7�J�7g���_��

`�P���j��ﵾ��c���"F�e�KH���C�|~���{�,�zeK��kr4	�l��9[���l%���O�n����v�4�6�Q����±?s�{�zx�x,@Φ��+�x-�F�g�A:���wB^��W�[ny�TBZX��b�H��oA
R��i�-2A�3���"���(��1���Xh:���m����
���̳�7��1�b�ߟ��n�cNƽ[��,�E�]�i;�F���ژ=3�Bc�素��
�s*v>l��D���9K�0�i�I6z���9��ˬ���g0&}�#ĞYA�����&����r�U�;(��6>:��O�$�����N>#�uY������'�?���c�ve��?�9��k!���X�:|,�ai��B=��0�[��ҽ'S��Jp���p��%�T��ґ~����v���m�X<�YY�+��HZ��0�l�?8��m�������- �<
i����M=*�}�'�f���D�����D���Q<�̚t�s�Y��E=��b�W�g�3�-�s㶔�U��xt�E�t �AU���j�����<�N��u8��3m1-0���i�ı��-r�v�9��Г��1+I���%~~�e��O��}�����F^՝I}?��3K�a�� ��Ԟ��I����m��>����"���;^�#��-y6>3���vKڽp���g�g��u���n��ƙ�ɏ#5���CF1^�\y�e��hn7�l�_�sXNU�U0���|Ϗ���W{,k��_��uڷ�(#������{��:�t�{G<��{�F˙V��0�S�ݿį�h�BSMx��G����O�ݢ�X{�m7[��+�8�z�%+�t7�a-�Ob��:���������{c%�8����������-T�w�_J1������8��|��N���������6>�FJ�q� -u��-�88��j4�p]��ċi!�8�'�Դ����dE��k�������� �d�{��e��p�2��t}�R�+�w�Mw���%�-	�����7�<�����4��
�o|�BE������_C��3���|~�������n��?���ԩ�(�W.���<�����w�}���u      �      x���[��D���8����*��u�����G���c����� �$Z~1��ǚ�|���? �����1�<�K�Ʌ�M^��ӻ�|���\�Im���,��d7s�Н�x����?�i��kK��t���/K���(�ȼ#8��6�J��|ɩf~��A��v�oŏe%�q>m�Kj=��͇h۲g|6ő���t���{���m͵��_�o�/	U�b=�r�/A|/����"+�xX�gS��|Z9����w�1-��`����]Ƙ�O��4,��f��3=;���Ϻ�r�^q߻H��]���p3�t���gM���\3+?�Z<���xqRV�ƃ�<G1�֮�u�n�z�l񉟒�Ɍ������j{��GW̢��e���Hݹe�߃�Jh���%��~��\���x�q�q��+2�bt~qxB:g�Ga�wy-kl���!�����--��]�5M+"���/�,���~���*g��`��R��r��2�x�������n�4�r51��'ӨK��gk��3��}�1�7�l��%���W�ײ�������^Ca4����S|Ȏ����Z*��x޿)�C>㇋l!�o�Xd�_|�vc�y���W�3�<`�.��YW��)n��GW��W���o�<I���ղ;�(.+-�l)r�o���]O���0����I�j�2/��я|�s/f���|[C�.6�S�ӗ:/����ć��	�����ee[�2W�����N4��[��(.����L��*�*	H�(�)��ٟ1��^b>��Ve@y(*�D������N�N~�X�=)Z���|W��J	kC��di�>?����_����p�O����S�'9��m�a��g���e)��lq�)�=�OƓ5������%��J�:��gq;#����C(�?��J�9�7BB���,Í�\+:������_3��K�fpߒ�Iә�MCF���o51�t�a�e����'�{��#�bԸ��Y�2���ު����ө/f��S�Y��O�z旲��`,O����!�x�}"���LЕr򃹋�����jZ,N닉�-T�Wy�w�߃aA��q�ǩ��Nq������2�A��~~�)�A��W�����W��܌�w��V:�M��\��Qh(S��g�
�E��*�৆�����C��j�1'>(�*N��+�x��0����^�Ҋ�_�>���gc�2+���0pf>�Re��Wˈ��۔vJ���Y�AP=2����g|dS���s�?�� ���N��\���dc]�C.������>���?�vxƿ�dD�'oE��0�q����B�uo���II�Yc����'�l�y��ď;Ϗ��t�����8D �R���Ლr���'�Q�D��#��⬞S})t����=�JD�:㳻�O�[�Y�
8��T�`��ͤ|���`ؓ��(��:Uf�[�����Q��	�I��s$2Cp���u���ϱ�h��xs	�Բ�{7�S��[=E٭�yf����V|�)��Gm����G~v�����=��U�pnMy���[_)Q�!e����iڥxC�x��=�\����D��gŻ� �N�J5���B�`kt�Hif�xb	��x(����E�?��2��<���J������'�0��㏈V�,�y�v#�m@^Qx}2����*�����}h}'�C�Yq� ����6��G?���9U����Nť���9U�����[��c��c����_	UkP��τ{��u����K��������~������9(�XBV�1�1n�;ʦ�9IxU�(�Z-rɾ��Q�Fq�V�[���'���WYc�{Yǩ\)'������S�~���3&�Ԥʠ��0+���,.�w��Y2��|p=�Y̧-�@q��gYEq�Q���~�����-eT�z�Q~>�dҊ�,Tq��:'�p�)�����F�Gu�3͑�>�Xɥ����%w����VOiP\�y��dl��t|�-٦:+��c]�:7@\�A��[�rև�Z�x��b��K"Y��H�$�wmb���:��K�gY��cgt}�pddV�";L�����������E3H�a��rj
�.v6�S�P��ª8~��Y�J��Jxr�"@d}>��Y���=�ۛ	��P�Z���;��U;Hen��G�ٱŤ����f�s����=G�'Т�=��ik��8�	O�|���yn����oo�͵����k�PD>)�fV<S�0��OO�n��͚�ތآ�f�h89����m�������th-�t�61w&F%T)BVPL��0�BI�	�մno��Â0L�#���`rDa�l�����|��#!_q$UQ<-<��˞4������C���R�d���+���@Cӈ��Fױ�Oi��@M�c����g$%,Y��y,�^Ʋ�	E0Q�,LJTqW+"�Z��1"R�+�����I�^���K*���&�)��v�����om9t��)I���PS�q\Q(���Q��W�)��<f�R��d�YtE��>P<�3�����E�X��IiG%���sh4����8ƅr*�L��h�����c}n5��/��`K_<�+o��L}>�Sl���I�c:��6��U��8�����O�Z��]�P�Eq�k���E-&AVO��Xؽ�/_ΡKpEQ��IDb���D"+(��*D_�J�f��8�@*�Lq�x�|zJ�1����C2�D�↿��1:�,%#>jW���y����q&�4��(�IH'hj��,m�R��[jI�C'W2�(����>h~�ƁZ����vOT�x��b��A�������>s�V�G}���
CqxV:N��+���1�Й�����|j�~߽�2�P\"��N|Ċ�*>�����Q�YɊwD#b'y�ǲ%���5��M�K#�;��8����6j�pC2�z���Eqƛ٤�x��E��K��O�6��?X�����i!�{T<B-�Ͳ��,�@5XZ_��+����������_���n�}��`.m>�?ܥx�y
a�x\����̳�K�O<ā!�R��ɲ��g�-f4�{n!i�z~��,�8��[$bhYqx���y�I̒���gc�a��!�mV|Z/��~�x(�8&��	��wEq?�@�I�1"es0����V|��틥J������e�*#oݚ'�0��z�ѩ"
f	"[��ĢZ��������Nvoՠ*�/d���߅%L�8B|��E/$�~���O��A���♅�V*��e0'u֏%k�N�Xf��g�#��܈�F���#��O�����TB4e��I��R�v�����Mq�,Z��;��1�!,ޣ���"�3~㋣:�m"FHiQ&s֬g�3�ۙ����v�q;3�����l~�s;3��9��������ݼ���^]�[��mq�ָu{ܺEn�&���k�z�<(t�J�Y�����*1�}�GsT�@�{����?��2)�(`�o理����C�?7ʺQ�M�޴�M��;�6�)L��R�N�v[�.�'�-�%z�����6��4"T�H3�\Uʫ�}ip�y�ʅ��o3N�*����Qtc}G>xk�r;��8��13Ga�� �]�[Uɀ����U|e��U��N�=%oo �fw��Sr��E��¹X$��e24;�)-=0�n���Ga;	���G대���'̂���5�>�~��3⺘Pw�$���Tj�jI/h�z�c��!������?���w��P�QJY�aD���'��6H�؍�����}�V�?�%-�4>c�p�f���@�S/�W/����O�=�J�͠��PLM�Y��z�{�� ��g�֟��h6�S��?�J޴;�6�=�o��}��|���K�K'����6�U��9��;����:FO��	�5�K|,F�+g������g�����GF|7��8o��&�n����ϋ���𛋿1������O%%�������w��������4ސCd���Ja7\�7+Y�p�{3C�bv�{3��ߛ�s}�7;�7��fh�qr�����-5�{�4X�JD*�u���*э��֛)	d��    ������U������Xh	���X�Q�!�B[�-u�Y����`��Qw�	y�Ȅ�^���˗W��(������C(�. �m�ߔh�˲�7%Z�h�o�.B�O�,z��|����d~)�'���/e�+���V.�}ۧ�Om�W)��f��(e�IgH�lٷ�[�jJ���&N)�<NT)3:dq�R��-;�x�ްg/e��6�l[e7����ݐÿ�?�
������4ߒ���uu~�w�9(et<+!�8��X4)���?�iW�3~�g�)nv|HM���{50��B�iW/:�O?5@q���:���DV{�,
g�d��[N���ǰM>#:8�3��}������N����,Z=�vW�8��Y���F6ﴧp��ӳrJ~�>P�/4\�� ��_��	�>?��Vj���'�!(�GQ|*`��z�Y?E�9�K�ћ_�,%)N @�3�I�]�2i�񟨸�猿�O��}�2/�'쳯=><���z�hr5�g���TklٽVp$5)^p�ʏ�q�s����Ҷ�G��%��3�V�奅P������ɼ��|^����Z����-�����#Bׯ~j2��O~R�R~i��`���Ş���@v��J�p b�x�B�舤ǻf70եx�B�ݑ�x��#ܝ����g��]-��G־��Έ��YgqK���ݔ����)�V��[����qd�J<�_[Ydh~{�q�qyQ��E���=_8�p�=vi���L�ţb��}����=�v7���T,��oO6�M���'[������k����J�roOx�T+�F���k����ܔ�;2[{�Ǿ�\�I6��ޞt*����i��W+���'�������О�B� �ߞz3�g����;\ȷ'?�o܄�'����c��=�^������!����f�����H��;A���;	�f�������ܾ�ip�R�k�v�d,��;���w�~'��0��N�ŝQq��TH�1�P�����[�~'�`s֬����b��w:�:����}��K�rӏ�����@��6T���8~w6����"��{�E\�<�]Iv��
z~���Y�o�����6�����������dϔ���^�#�ۥ1�n�x{%l���#B���=���s2vys��P�g����-�������1��}��5����Q{}�k��L���z�7�H��i�>%�}����8%	�����)|���_����a����Ď���O�A����9ΪH=f�[��8άֈ��ֽ�#oi�A���x*`(�nH�?���m���5�����S�;)�(�vb���q�n�-@�¹m�B|���~u�'ߧ(��}b'h�ʇ�(LMO�j�J߄5�Y��7�ٵ��O`��\U���|�#��� y{��|Fl+�<���������vfw;���og��3�ۙ����vf{;��Μo����[ɭ?���r돹��\�s.�=���[ѭ?���t돺�W���n�]7��!?����?���X      �   �  x�=��q1��U1��K��# ��f-���n_k_�V�D��z&A0�_�,������L�ו�J1�y[Y�2�珲�2���g��V������6z�U��M�]5�CU���ɩ�ɥ�ɭ�ɣ�ɫ��Q� ������(�s#=9ҳ#������%���� v�N�D8�k�%�k �k �k _o����z_o���h�e��'��g�;���{�J���U�L�����ף�S'.��	��	��	��	�:�l����U74\-��w��.��E��Hx*��i.�Rt֝�7���T��?�R��x��[�L����A��]nd��T��R���l��ق׳�x���K7O��/=|ʁ��������������о+q�<|YҾ-i_���K:���Ϥ�~���*�[+��x|G+<�>7����c�/{�o{گ{��{C�����2��ar����v>��ҡ=���_�s/��7Mګ�~�&���ۆ�Oƺy3ᾩ/��}���o>\9�Ϭ��|�W�ur>������+�+=�Wz���V_飾�׋0Ջl^�`�b�j!C��o�o�o�o�o�O�k��Z �HwH�A���n�tw��#��4�'�o4���s���F�`�3�]1~����'���d��SJ� �*&      �      x�d][�%�	��ދ;J������΄����#!)��ڟ��G����O�+��?C%���^���M4!���&Z���;M�!����Lt�����n���{L�]�������=h5�����7�ǿ�ʚ���2�@�<���|
m��m�h�#�i�]���DZ�ɍ���Tک���'#9����l�B������cBR��+�d���"&(=�����@��wÅZx�%�e����)����+��|4%ǅ��O᫔�S�ο���?�>y>�S�V���c*�u�X�o�>��>}I~�<4�~iH>����F��v5�ݿCJ��Wy�Z��xRЁ�Ƭ�~|�*�:��c�u5���ʼLbܲ�c����O�	�ldOGw�k;������i�C+sЎ��I�B�I����a��^�N_!�/�d�R������Wb�Ԗo�U��w�,RU�(cs=��5��\���Zdo���&�3ݍ��Cw����dqG���~��l�����B��~�������(��o~۱qL,�f�X���1����%�����֚-���E[�D������Gj)�]�e[��Ϩ�-L����R��gժ���[�lתٟ���Z���}n�jaw߇���7�4����+.B����O��e��g{ɾC���&�������Sf?u�:�K1*FҞ����X��Z��x�+�|ky��;�7�J������;)�ND�gQ��b�]�h"ڷ�-e��ﳩ����l++�� �~5��i�U�D����A�B}��W�h'����kӨ��۩�o�o��.{���n��42��a�E
�)H7��DC�ee�2�AN*�~�.�
[�W���-D������_F��*��GU��?�E��Q`��S�}Q\�������Rmu�d�Cs���d/|�L�e��VF�pb��
{�A"�l��4�!�E ≛P�7z�A;�]U�H��	�B٧�#��:�L�������-�]{�y�8���(3�N�5�l����t�6��fx`�Vv3H0]���*��ӆE3\0}�ڟ���L��-4m�`�
ۈ�!��6�C@in��=��m9���7�u^��^�o�U[�eڮ�\�۵��g����m�B�mSH�q���4d�w}�b���o�*u���年�i��}�hz��Bsb�����������K)�>�/PÞq7~�B�#ԟ]��]�~�o6g�a�[d1̰`G-��PÂckyz�.]%}�d�aѶ�QrXpwֶ�aO\z?�vXpl���%�ھs�>/����7��VY�`G�]F'n��:�嫂�ӊ1٩[�U��kt���6�T��G��`�>C�A��{`]�BL�3Vy|R_����
�C|�'�]D�o�J�j����k%���i�N1��>K�/�ǑI���R�G�+~�Õ�8/���F����"t�N�/��Q\���J������D֠��0�D�`��}Q��u7���(ޗG�'G�Uzܜåx�= G��i!����"����'��=(�f�	��h��A�:7���E $bB�_����{V��g�8VK$>���f+d���ԃO+f��wK|�2�~=�Ƞ�8�?�$~Xʤ��E�Ig�p"���B����Zq�׵�(�uq&�9o]@+ARa}i�7߽��B�M�H�"B6d"D��*l��ב_�(�
���GقE=�O�1T�|�9������p�5$R�x/�d���<���&�T�ْ��D�kh٠e3s{��)�{1�MIދ�l:`��z ���;���E����57��8�{Us�S����z��N�������"�Lwq6Yf�p�2��KF�.,�.��Q��.W���{>4��{��p���R�:��P�$����N.9�# ���	.y��n(BxNMSí|FD���{��J�5�O�C	;�K���S�+�|]���p����~��2\�HD���h���n�A��޹~��%m�MT�A����j���'�ە�|y>p�|��������,�k�k��`o����g���C�a�R�P8,5�.>�pX�#���ҿ�)��ҭ >���h@���*<�Hf�O�E��?��'�zo����ɼꠓ?()n���� ��N�����wv��!wǊ�@Uo:� ���+���%Z��������A��;n!�F�x��,j�U���_�<���&_��&�e�����+J��b��7T:���� g��.�נ�y
�x��꼅J�ﬧ I3]�bc�+��
��R���EQ�틒E��?�����~i�}��+�&��h��0D���W�!��w�!����� D��n� D��(��1�2d��0�8d�cV�]��j��t��P�NM9}g�O:]�i�7�n�Γ^�U�[�z�|$}�C]σ�0�|F7��1�Y��!���
$.U5�]��Wx*�S�B8����
�tM��b�
[�N�X���+�i��t�_�T��W�kD�?�%"$�-}�%$|%���T�B�t�jz�	�J����v���J6`�W�s4Hh�W8r4Վ���yXܯl�:⬠ڥ�L����Fp��
BT{�Ԟ2���Z�`�G������2BH�Q�QF{o�t����o�.`�h���Ϊ�\��<�vssz��ʕ�eL�3��[Я�>��w�@���!~�4�^��B��B�W���h�\�Ͱ��	���mf]�8/��蝌��8Gd��^Z�1�L3���諼 V��!�|q�#�`�*8h�k�f��ߐ�J��:tp�����:��#j�W:)�D�1 ��/�x�����������R�a+��I�ɻ�Q��Bߖq���W�+���(h��97�R� &�0���$bF�_������t�K����u�C' >���5�cG,$UZI�1 D��5�y�������*���DD'�!Q;EI��?�ˁ���6�:��ƦKN�̦
��0� ���ap�a��٫F3�Xf�M\��My�����n��;T;�9��i�&��@���k��h��hy��"]�x��-�@�÷�`���[0�Z�va���[�1.����j�Q�0"�25�H<��Э[&_�����-S-��c��_�X7�:�K�Lᯢ�|�D�ׇM�@|}،"���v2�pA3� >�jF����U6��ل����5)1N"���V�I��z��Q1��;ߨ��m{]W#c\��F�$0��Ȟ$����e7��0�'�q>O<�7Q��G��D	Dk���S8�'�%�pNOb��9=	E	��J�9=�e��I,,
!��6�A���
��=#�"-��Np�QQ�z��M�N�E$�̕�"j~�Տ<D�"�\:�SC��驡 _m>�W|L�������w� g@���6��M��e��~�84CP�|{�sri��X��sRqgǲ�%FȆ��#��;�"?!|N���@sR�g��gN*�숗sR�gG��\T��ÞA<���E�\]���EN">.1>�d.{�Ѹ�����(����E���ݢLr�x�=S�i�=S�r�M��(37%�;��ܔH�H$�M����f���u7����W��Kں��fTL�p��(W��� �u�q�������Tw�M{��u���^[�ՌZ�+Ҧ����d{'�tŬ#@5���j�?������F1��v�b)�=P�T'�)G����:���>ӠĦܾ:C�{?J�*o��Hl��8�����vZ���|����s�+>m�K�Q�B�WA%YQtt5փ5�Gn��
�]�z|y�����R�*�y��������w�����TK4^��>|�'�u���0�ʜ��g5�ɧ�i5_�*�z��M�u6�C��mu:�����L�'��
����57�p�d6��,���2�p��y%�$T��^��f3�X8�WIs�)��!R�*�	D:\%>�H���G.i9��_��GN��W�cOy%>��Y��u���5����Bqa��0�fM7t�p;'�7s��p'�7p���MD��>�n&�&?]�\
��+�ڹ�?�g����'�\�Y$�Dq���.�?~=�B�c�    ��NP�����v��~�]\`gI�W��u�Z5[�{��|V��}e(٢��w���ۻi֩����Z�TU�<fv�[yL4}oKyLu�G3lk�� u;ҏF�ym�U��Am�՝����Z�TȒܵoE�,�Y��KR.<��H��,�Y���T������ng�_H��d�͵w*�Z��rP^ǚ:�T|���S���t�"�������:�3�K�u~�.��EqѺUL�҅�����M��=������T%"����'�BV���v"ݽQ�HAP��,w�p�!���l;�	$�(~���+�e�	16J� j?���c#��ql��؍�YҎ�G�����JF���  U$*\	pI��
Y�� ȉm��[h����S +n
�4�Mx�-�ҋm�d���2ΒN2��ܲ�8Ñm�d���)O�o˥)�����Kމv��t{�B���T�� ��}V�w��L��e٢p����7E��׺�O�v�K~�1v:X%i�]"�]�)��dP<wɤx�Eh�%�"���a�HFo��e��	X�� �1� ��,��.�A��T2r��r����I��	Ly���Hb��qY$0^��K��uW�V��s�{I����~/W~��0�[�0'�`�U8)�rN�v�2�]�uk$�,���HRF�0Qܹ������޽�y�܃c;ⵙ=k/G��U�4Ð�&C����!K�����%M���%M�Lڧ��LU�!��{�W�i�mw�A�%���%�$tq�:��k͢�>�#;��אּr���U~g�3�n���ePr��8(���A�6,q��$S�����l�w���䐵�䒵��<�K��h�<R�����c�G��(�t<q<�	��y�LP�y2�W���Np�t:�E6紧�.2�5r~�>�4�,�i�֮�4I����I:���h��zݴz���vh�D��n�M�h��CKj�p��4�J���Kz��82���,>� ���G�jQv��S>r��r�ȥe�A$'e����gc���c�K>���.ჱK�`�M>HH�����8L2�8\����I�~�%�~�%���%���E+�M�wɡ��xv�-;��A��Ա��'u|�+u��a�����,��V�d�HQ�8�'8�q�'~�<�"�!�A ���6�j��]"��c�A�����uQ��Z�=�	,J�v��E����A�ȱ�E���@���%B��t�N>U��N.Y��Q�����ٽ���</�^)�;N��l����N���X�F�s�g�J��{I�Y\�A�x�Hw��O#mO��Dڞ/��)?��k����m���N,�𻸁߹�.��hT��1��R'`^��id������YLП����8+��9(�y'��W�T]A5�s�ѐ���+ń.�pq����{b�H��]�u�g�pD��Hn��KO7������|�:7�5�s����V�ʍ�;�R��'�������|��q��mԀ �o�5����m��9�]�9aF���e���vk+X���<�����
=֞��+BF8������f�� #�~�2����
�*���&�+);��+������9����g��9��v{]�$\���X�����c�!��H�;l�w���X��v�f�Ӷ��D���D�4w<d����hd��o�2ϒv2�pw2��cwL2��a�%V�����J��
ZSα����l�Cٓ�	o)�$kD�;ɚ4ؼJ�IXȤTX���I�0A	���(��<�ʁ\��D�cʿwqP�.΃ym�]��ڐ�8�wq�����ǂW�h���Zw�5er.��\��R�r,�&&��q7���R����gn$��z��� Аe"G�w��+P�ݛ<F��� _j ��	�@%4o����*u�:��\�ÖTݲ%U�lI�-];U�x���((�����	/�~f�E��K�?	E�����s�=D�Y�8�><}W�a}��l1�g.��ٕ?�)�S���1�t�=�_8Ҟ���-�N3��a�IkVn�7�����WK{���#��o�����J[��@�T*�wD�W��T{RK�KE|�h^�"k�:�n2W�*=d�=��$*�ڞ��:#�j䨌��	m���}ŝvuD�WL{0�\���� Ľf��ko� �y���������C�� ��?��V[v���+������(!E� PQ��:�
��T!��$�|:��|:E��[���m<�ȡY�Cw�h�;cІ��WJҹ@UHr����Ҏ���#w��<��dPi�b`;�u\ڪ�4�|��Ku��{(mLv&$f�Xژ�6r�>\k��p<���T녞UJ�^�1{Q�Qp�~ōFw��(9��N�b��A�����s���݉E�ݧ�39��uʀA��
o��D{�S�������H[ʀO����V88^)���ȷN�/�D������Wx(�犢 �w�>��ݿɱ�	�9B���mI��hҞCy� �|���� [�Jw�s��S�lm�7��9�k.����_.��$�n��d�ͫY�Hg�:h��\�KG卣�+����s�+dk��ad��t�����Cŕ�D�|��ga��$1���k[ŝ�1u=j݃�!��|��!�A�Jy`R<����������@����+�� M$~_���lڌ
��05-(TJ�ٯM�-��F��:��Sk�Df��Uť'/TV%����)�SN�$�U
�fQ�"�ҚPV��4��(-0S5Q�P4�6�L�M�&����l@'�S͉�֖ٚ�<����5�y[[N��|�Y[pR]��x]�h��f��T[\���%�҉��6R��2rDU�3����i�������U�Z �l���2z�U6��v$����pu�� P��A���('��]���ץ�ɇ��n����6�ȏ&''���bW��.M.Ɣ�%�Ջ��.�-*B�϶�
-]ޢ:��!�.Ͻ���|�,���ݤx�L�]7�£�qIg�PD��ʂBӸ U])T���d�d�\a�aҪ�5�8�Ve��UaH;U�/`�U���4c�|�T�=Tz}
Ϸ��̪�ZO�Fi��N||rb�f��*%fU���Q��������Ew0���k�����Ż�ZGѝ�Z��]G!ak�R�$�d)�k�:AE%�d�`F�k��d�?\j��Nʣ��~rЦ:�%�T��k:��S�����:�x"��B��>E*,�Fb�S�j�-�b�S��6F�W4+g��S���WM:��ԕY��1�Hm�R1֑xc��E�Hu���Nu"�	v��v�|���3���
۷�&�Ѩ�ef�+5�0e$q�D2L9e�L9s8F"��9#�S�g\�h$�d�)T3��/�n�"��%aXF#�V�ls3�4X�!؈$S���2I����H����Ig���Lb3.�2N�I2�����ZoF#z��~3�У�{/=Z���d(Y�fD��p�śI�3�\����o32����ft��d)s�EO�\�ےft��D)t�o��d;Q�{S��6��*]��I�]u�Ϳ���m.t5�(Fi�+]}��Y?���j3Rɶ�ظ٬���U�ևB���������3�i�%�mBM�$��0��%�m�6�wC|��J�m⫮�&�O�5U�<�Э��m:1��<V�j,�-�@���<4���o-����A�^a�j��>�jF8�6�O�|So�3��_�͌c��E�rg[��^L�~�'�Zӿ�>�P�<�����δl�1N��%���F:�v�������s�Al����ee,Ś�O�
�޷
O�Lk����(1>�t��<5���FA�Cص.��J[QY/W,6��+�ź)�L�Y�ӝ�Ye7���z�#��S�Xl#�e�.֣��Q��0:�0�ܪ�)�Tu:w��\r14�;r� ���̽��Ge+�d���[��. KmZKX���~��)x���B>Q���|b�O<��|�u2
4�XUj�i�7Tj<�i�7TjD�i������ͨ*a���m67M;��qU´]��Eh-m�UT(�Q=<�Z�Y���ʧx�u@.��o>,�@;�\�\����L2w NI    k߁X�`���v����:�N�Zߣ���= K���Wa���O\�d���
�Am\���}ީu���iF�߁X"7�ɒ����^�����u�`l��:�1��13�î��Nh,�:b$���H�v���V�m�Rd���2T�W6�d*˛ ����W��20Ҏ3�QY�޹�C��L2.�X̼�3.�����%�������ͤ�qYFu=͸,��`�k�e�]�]32�\�<��,S��b0F��(x*Ռ�2�o���Z���4������>�����'K�lo�����A{�:����+�ɮ�qsg,2��&�n-j�Tl}s&�ć���L�	}n6<di��L�I�!��w;:\�Z���ċ��7A��T�q�'urI��	�-|��� ْ8R�M�Бnq5�(�mɷ�����/D[?|B����E���#;|B/os��\�!�s~)�^2��x� ���e.��O7f.��)�80s��NƃƃGu�x0s���C��y\y#g}9��T3N�\���X1s����̵�N:i�:�4�iG��P:pD�<���6"�R��*<�:��*��O*=4��cC��mL,��,�Ȱ|*]� n�*'H��G}p�������n��6�F�Ї�pm�"P��FdY�wvB�7�2��bB�W�Y)����l�_G|E���Bo���S[d�U���R���>�4�W�J�)�Q��1�=�25v{˯sG����U�nC��ɩ��g�]f�,��FGb�Ŝ���2��j��A����Ln�2�Y>o� f�{@]�A��(�ncp�=���猄���7Cs��^ܧ��ao�>'�����Gb{���a/�q8 \���:����l�Y8�U��]5>�H��f&��~�&�p�-�|�o��'o�ɖ';��@�%}rsB¥*Ǐ�ִ��E!δp�v4�͖m�Pl����i3��V�?��2��V/@�3��Vo@����6[=mÇE9
��Q64����\"=`d\"־�x�ǧ��3`F���de'�J�F$]r:Ad]�+�L��1y ��H�T�3BԹt~H���T�#���c �rٖD.�ȶ$tQ&��vh��S�ٖ����%z���D����~�.ַ��{h��:�4�i#�+S#���﫬�z�Ͳʄ=#ic���cf$�����Km=��t����=/g��36b�4co��=D�%���k�ȷ���C��3�z~�\h��a��c�L/3#Kg���^~#�g��^�{��(:[�po�/���Gg��r�Kg���{Ogz�4Mc��4�Hm���3�93r�:A����c�y�ԨL2�w�c�[6�U�=a����=��6�Ζ�+������5��l�"�,��v�{�팷3<�A;�43,g���#�{g��x��;� �l�~����������9�)i@����ɉ;���jآqkTw�fA����48��U�IOI�Nz��H�qy�r+���o�QIO�v.72�T�DRvƵQ�5}̺l��î�&n�b�f@���9����&{I���ۧ��M��47���Vܕ�z����Yg��e�ߖ5��n�T��?��BhֳF�j9?�G��g+
���ahI�1�_�\d�d`n2��������a���[���3O���+�����y��Q��:ޥ��~�1
K�O����v/��>�_�p� :�_�x�0J�B�����H?�|{��3�NI�ޥû��%-���SA�0X:_��tʇ.�j�.]���*�ҵ_�x�wb�ҵ_O�9����8w���u��7�'���g�҈tj�����OSJ5��>��<��� ��z�X�c�gepE�f}r����x��;�%�F�t��F�TF�tgT3~��;�i�"4��6U�L�ĥƆ���~P�r�vھv�bJj��k�^����]�(� 2��w����mе�<}v@� ��A^̰�;�aK�0��fIkK8� �F Q�[�a��vx�3klm�w����w��_�;u#�.ፚ���t�>��OK{���m���{��AN!oD�h4}�6�� eur8���C�
P��U���K�Un��;@���F�?aC*��J_"M��o��g=`��u��/���@�d}'ZFE�=���������O�� �-F/ǝ���%o�^��C�K^c�7�u��%�1^^���EF�A��U�YNT��U��$'s2�.Ծ�k�kxn��r�g��*~1�GB���k�|����NJ��UР��ZH�а�kD��S�X�Ca����X�)V�e�)�ō��DZ(�a���D!娬.�ʘ��F�uo��ʛ|�0�a�K�����}m>��8��C���'�u� �:d�+��%z�����_��wW-9�2�S6ۈ"��1�������c�V{�X�3�f��֞#)���")�sб.�&V������p@ypGV+�L�:m\>,�Mq��B�Y�B!L&�D�S�����`�~��x���j�q@V��(�y��y��$��{G%O�'�'mb/��É�
�'.2��N\���q�bF4܍3�e7�\���T��S�)�8��ƶ��g<�i�+`�TC�l�BfeGv��ж�i��B��&��m�HU2�����5kN���Qà7�Z�&�תn�i��D�~ŷz��!���sw�5~а}��:��jGz&lw�5��W�tcH���/��$5c�>����kG�&<ѰƷ�����_@{PY����//�e�W�� <�"A�e�ןx|�L�Z ��6�C��K��"S�'�L����L�+m�IB�@p㎉)������e��5�j��ٻ��YM�?�S��iVg5m��ã�iMml��k�gG�]�+�q�ՖI��o/5}��P�yW��"՜zU�F�9��0�W�_~�� Q����h�����:k;>���dWsV����ƴ��cؘXݻ�j�O���mh�g	���86V.a]`ݨ�ú�u���;k��o����=�vp|�֏���/�{b��X�Mѭz|ORm�.��-ٿ��U�Б�������E��E�O�*^|n|��rl|6�U�i}gܻo|�i�t��k�ߋ�͟��n7�������R7�%K�a�N����X7��6�j�N��8�{'���;�*fW�����ھ�g�����w�t�bFņ��
�r������j��%M��gN|[��Y>.ώo���;�pN��g�g=����SE�3�?~��\���wrȎ�y:�������ͧ����۵}_X;�k�B�����2ȣ��.&M��e�����]��9BV�kG�ꂙ���nlqQ)�8���zԣ7�_����������j����V�����K��Xr��Mj�T[?��yK�t�������R{'*m�Lɝ8�6�D
�b�ًuxcT@�X�1��H�$w�ؖj3�愱-U�!���z��~7�8���*�J�U'�m�Q���p��A�F�B�QXA:=`���A͙d���T�B��l�����.��w���"����"��ns^ٖ!xu�� �}��. ����Ү_/��,�������T1�)�	&����m %y�|�>!N*�b���� %Y�T��>���l�&A�u�M��mƔ=7�	*���G� '�Ik�+x���$���h^n�v N����|`��9�0��s�6V����eL�B����1>RO�Z�! J]���BY_=B��a�yPj�i&�(%R-k������ m��[��z���e�yg�^'h���g�_'l�ڦ;��3��6�Q����46�x��݀j7
=�Y*�����(�+m_vh�J��E���`��o�8����]�zϣ��6:��p �ޛ�(xk&��s�6֜�Yxoƃ�+�9���,�	����
���Lگ�Yd�y����?j�)� a��9Gm�k��9�%���9j�V��q{�yP����7u��y�xJu���H���m����7>5��C#�K! ʇ����'n���6�v�_�'�)uO��JE�,��ωj�)���&�=i���x��\�a�����j�9�T�    �ю8�#��5�ퟀ'���6J�^ ��Gڝt�����]1&=]$O�W��&��M���ɗ#��ܚ�ȄTi>؄Ui=2�cF�&��	�^�L9�8�m�+S/�'t%�s>)�CM
^��*�7!�w�>:�m��ac�ll3�m��M5�n��آ���lۗE�n:�V�S+��V�%���tiHlI����҅ :!v",�ɗ���)�Źç����{�4��Z Q�E��2 J�E��v2K����I5��n+'U�.�W�\�%ə弶BLZ6�;���� (����s7��v8��d'~�3����y8р��<g<?#�É�H���p�!�&�Ù���g�X�/���a�[�v����Vu�VG�K�N�P'���S~:	�RoϺJ;):.�	�R�/��H�$��<�(���[�M��4�د�2]D`y%-S�RPB� ��ub�(���X�S¯<�(E�&�0�\H��	z��RΣ/�	y���F�3O�1�����7^P�Ή��g�f�P"}��:e�A�%O�L:��<����YJ�N����8IO��[��v� ��Ta+���T� ��YAE�*�#D�<���ftH��r�i#q�ۇ����'�MzL�I(�3�>ߏ��3�>�����<>o(�	n��g���m�p�8����0L�|����'FYL�^�'GYL����,����QS���Q���>?Nn�6�O�sm �n�|�����>�䂉')��2�"�t?���>�S��i����%��������6ލ��S�[Hq�G'�Q�Aꩡ���h�7�*��֧X7_����U�����~ǟ~\|��w�8����G���Y�e�^�{8�:(�U^1��J�q�.�q��#J��sHcխ�-�����$�?T���V�_t��vX���>�ֻn�;h�9��8��C�Ʉ�M`�8�ml��Vt��f��{i-B8���AX���a�3�>��Gk�Ke���mBX:'�}���P�*E�ݕ8���s%&Nq��\��s܆"�ˏ��O�+õ���_�XȒg����>|e�A6��w�ۇ3��ċ~r�O�u���/���Ϥ��f\��V��C��HB��b=Ej���R���a�J՚�Z㖫\,��)Ȫ���Ӌ3�>��+*�c�s�����"Ae�:G��(�D����ڙn��4-�A�rL������}���/���G'���A�䑶�u��J�9��̼�6�lz���eQ�m����f���`K�����9Ȱ`[A�R\Xf�x��������������e����m��V�Ҽ&[���Zľ8��l:�S֏xS'qO&mr��t��)|3�?	�BWL6�žEr� ��͓*vQ��a����w�-
vֻ(���Y�-�uq!0�>�b?�A����Mg����Ͷ���.��vRe�� Q��.j@�绨�(�J�c�0D+]�L񁸂��L}ȡ`�k=i��]�9J;�$`C;�⣇�;���;	��;	���;q?�B|�6�K�}�Dᓶ�]��������k�������K������l��K	˖����l�.�/0�ُ�)�Æ,�Pʲ�uA䡔e3� Q���Ĵ �+[�y8_�j�����_�:����	œ��S[S�ք�\�v!�NI�'��)��HWAZ����R!e��P"v���T<1�5ڐ��r�q�m��32�H�
(&n���Q(���
(��:���QK\�P@Q��OQ)�OZ�>(ԇ5F�k}{��C��;�իI��ܟN�*��4�:�^K�\��"�$��)�s�r��y���[��b��a����6LT|E�I��;��|��N{��3_�L�Ws���Ws���Ws����Jc���&��=�j�!�md�J��+'�?6}z�U�ӧ{
������g�T��"�Ȥ¨�##���V:WY\��x�O��W$�I�aAj8(�^��T|(���N�]�vZ�b��Ӝï��q1���7ï��b3���o6ï�F6���yoB_'lh�:�ʛ���ͩin�_'��&�ur�?9��)��%r(�6{�9�6�a��"��W���0�J| ��W<�9�V��0�Z��+g�C�+��r�^���P�%��"����$���v�s	{�~���3��%��y���\A�t?)�9��<�?�:a���ｩt$P��R�	��t$P����~Ғ�p�����H��$0�@�
fK�M��RőB)�fb���-=r(E@�F�#�r:��I;����R���Y�]���&��DC�8�(_c�P�[�
['�Rz��ȶ��Q?9Z��IMg�OjB8���҅�{�7g�O ��\
W~���Q�cZ�d�k��_G.ER�ZF*�	(�xmc#Gф���̡��,ٙ?3�q�!��(�?4:���z%:l�+*�gE)6�(2���^����D�� Q�p!�U���W\@z�^�hcʊ����Ъ�⸘��J}~��Q{�>A:>4��	ҁI��Fk�bLP�߲֘���[�8�<��}���Ag2}�u �_���Tt��D�?�Sl�����������Gu
OTL��1/�_&�T4��:���_�)>Qq���``���7BC�g�*N���/^a)����+=\���xAuf�K��&j�ж�/�4:�>�\mR�آs��EqI_��>�m���Ĉl��~�H�Ƕ/:���RT��U�L��L�J�����a�M���X�5��,��ؚ����w�3�Ä�|^q@R�q|Rqx3���~�H����C����<)�_,�C"_$Ҝ���*}S�Y�'�y�D�d-��T$�g%4|�*Q�}~��.&��7�Z�é<��*�aB�iϨjU4I��H�S�3��|>qL3�O&�h&���l��|q���@G{�Xs+�ԅ�Ѫae�i)�x��+t����}�4^�+-Rk=R�+��&c�ձfl��#����xΖ��hc��t:5�R��7��*T�(��f>��B�j��:L�n1B�p���TdQ.�k&b���
)-rr���%���8iR����'-½IU4*���U:+ԣ��ī$�J�@�9c��`�f?x+�?'U���ï�[E{RE`n:� ���QXĊ&�i#�5F���tWأ����ǥ��0�o'�9�o'�9
����9"A`���HiSw7u7gZNZc���̆��tS�ד�[���QWn6. �r��(\wu�r)�� �ִ0���Iz�?n�hr�'��u
��&�:�	N�`�����n�NH(�N�	�@��K�A�|���8hÜ/��1І5[�k�]�L�2h?��l��k$�M���Y��V�.���:qP�
�A� ��$�^�u6�Y	w���zN�p�(gdY� ޠ�̺��!T���m r�{d�@��ٶ
P�y�I�`���J1nB ۗ��Smr|Z��1��Q���nSEBʇȗ�ޝ�1r� �=��� �a��-`�G 1j� |=��e�v$�&��<5��X�k�T
�ua*�-v�$� �Ѥ�	��!Э�/c��v	��Hc5DB����  b��h�� Z��m�D�6�ڐ�Un^�Æ��a�-lX[|�Y�6�mF��ha�ڂ�]�6�j��1F�p �Yi1B�\Wzߴ�V5iD��<a-HY�R��3D8c|43��qT	�ףX���H�
}�1�װ��b��k��H��l��c{��"V�{����5M.��_���7���Q�Q�g|��'�i�kL��t�.mu���5�E���^\��^ j���9�N+	� gѤ��7���k,��n��1���3B�Ћ�^֭��
e�t������j!���]C]�K���
ޟ�D�!�ZtJ{R��H<�1y"�@���ة[�ɾe���4���x�.F�~!�v���rY�a�����r:�KĄC������C�s�-~�p������\�Y&@��>�K�o��[�xrl�m�;�.F�WǓ/�e� �@��e
�r��z��%,Z-� z���FրvB�Q'��h��A4�� Zdm26��37J��hxk��\\��5vH�hT    ��j��џxdE���xb�L-
�m
�r�]
�.���"�d	�9&Qi/c�0<#Yk��4�XkDY����u��\@?{j��H�-�������	�Ĉ����G�V5�<�V5�<���8U#ǓX�(U#�3�P5�9�Q5rO��~n#�!�1���Ն�0�B�W84	�j&yF4�T5�<#��ƪ���I��ő�zˡ��)�	zb`:CB���+D:�*�t�&�u�dB:)�#���@V$PY����ă��;�$8P�L���Ɇ)m�`�~�8���S��ٔ�E��A�a)��B����S�7%��q�Ai�o,��.�'�P+��� u�)��$2����� �2����Tc��㏅���ۣ��۸T�vg�y�J5l7�j�o|�I�H5��ά����(�6�0����uȣġc!��Ƥ�:�"1g��P]A�"]A�"�M]�I5�<�~0*���%�;�޺��WFF��L?�jdy�o/{�wgƞ�<�ن���=S���y Idܩi�#���g�m�1����{6#N��f���[��t�1��|{�5#N�E(2��48AZ�HS��$l�HS�޲ �HSs�%Vx?����8��2��X�B�q���=�Vll|����bt�1e	S1���Z��Yj�r`K�J�Uމ1��*g����&�Oj.�D.j��GU⫱���jyU�Ѕ�.���w�	��P�>>g�����R�.�RS�Ca��[����Ƒ��
�s��H�@'��<!H?tn�(t���'�.m��'������	�Vdw68������; J�`�" J�v �ϣ����v��oށO֏x�Һy�����5M�z�d��z>d��g� 5M���e^uJ	�a�*.<>�d�;��Np��=��Iв�M�t՘O����2��7V�[�X��
�BNF;Y|c%v ���� �P�
��>p-�gQ���q��q����e��r?e�=�6*-�'�d�~���hO'�{�,���d3N�؄���fֽn`.pչ�P��i �
\M�@(T�j>B[���m%JƔ��sFq)6�5��?�4ߝ�xRLW��ة0l,��6s_:�����\��8^L��R�� ��}yW���w��qۥ�Q�Ʈ��U�L�]��U#L�M�.56��u�s;��Rc�f1������Ș@�-�*%��%��a��[r�P�Zx��J2�L��vԌ-5���»Ө�.�"�Qݝ�1{�Nc3f'9(͠�|]��ڌ�e2![�A�:(����?��x޷TgT��i�Q�q`��W���R�NV�� '@ʷ.� �dy��dd�yF	�v*��mq�qq�S��I�N!>+� �e2���4�a;�yҟ��� 7�ʈ"��ɢH�C�V�Oׇ�qɤ�i�<�u��ԧʨn(u<�n�E�:Q�~�e��AI(���"X�a:|_9�����c���k�3��f��d��7��DF��	z�*FF��$����
���@���o�UJE��xQ�k=�?��^!�eY|F�����25�za��Hc�t<k�Yt ��r)�o`R�y%�-�y��mv/3�{S�+.R)�9���v��QAv��f�U���Tx{��9D��l�9���ٔs��s(�0#ok���2ws��,�!��9d~�t�_2����X�TtjT��q�TV��R@"W3i�m��i�V��K5m�=r�N�~P]�
}P���,�
��|
qJ���|J=�-	�z���>�VR5F�{!yƘ���1�\�AW���zP��}(��,�߇@+*J.��Wk7�
�ֺ�������6ή��m�[w��������2�*���@+_�}��[��b.��U� ,@l�\���G/�<��mj���1�慟�_�c�+'����d1�T��*������M?mj�t�z3��9�hM�I+�xLz} u��Q��M�RjuƘ�7�TLJEQ&�>��At��UR��1�ڋO�᳣�(��>�Q#L������Ȏ�Uޡ���ɴ����Z�M��'�i���Ё֥äy��i�/�����C�S����l�N΀���_�2��)V����������LU1�I��O�%�\�ScBi������zojߜ��N|�(�o�X=�*��}i��Zu� ��;ё,��v�J;i���_j k�kw�E���P던m�a�ݍ?�o��xe�)㝑&5�Ծ)��ro)+��7��7r.��{��������6��CcO�E���S�fU��l��R��ɱ.ut���JӁ�`z�7s�XTu$��tI&��ǧ���g�E�'<�,:Y��@5,���,:nR�?5lzDݲѧ�M�M�Q���fe�����2�Ծ	C�P﨩�~߈SSU]?���&cNM���җM��lr�;�^Zbo}�v#O�%�N���G'��S���1��tZy�����ĸU�ƞ�F�Q7��~��Ä��دl���8n
���åYڶ=�OQF��T�j���L7�ԌH�.I7��jN�n���dZj�Mr2��ujzo���g�v�)�*k�<�_�t�MͰ�K�n���vR�"�a�k�mjx����W5�'tueMW��v8�r�� ��,���U��L-�ٺ1���	 G�O�BY���ի�A��>��&t���$� l' �|p�N>���HS�Lz��x�`{���`{�!d�=vt2����S�`{�d�=-s�G�Nv퉞K:^�BǭE�H��'`
U���N�r��JI��!F��0�Q�Z�I����\�{�M��\�<d��7#LA��0
�:m��fn� wcQ{�aϫ�=w��e�=,7�I
u��&L�b�]��C��I�Tƻ3r��<�/d�.�O�&ڴ;U�;mڝ�ۃ6mp�tcPM��`�{�Vީx`��@��Q��)m(sߊ1;�y��1��й`#�ҟC�H�w�OMSڹ�@'+�u>�l���h1vz���2��sx[�ܖ���sJ�^ޖ+Gwy[�?{��^ږ�����X�����nMe���tS ��핊ȥ��U.������>#���������P��uV7��~�r籦���w��QA&n��ѧ��ֶ���ARv�O�/9������/����ٷƱ� �wj���S��%~S�[���|y[݈S�T�[�݈S�TO������0U������q2J�}I\��tBT=:C��O��7���ٓ4"F���e��ҍ?5���tcO����t#���k����N���������� ����C5�:�d��� ;���J���{O~g�x�㮥7 �
�9�FR�|g�����ip�a�����6��2�Af��ސF��U����D��ȣ���!��D�-)YA�@�+[� (�G,���� ����|*���ɧ���H�;�=@
e����&{&��zF�4�z=�tf�Z�OM�3���;��&�����PXs:�5��Ԥ�R>��z�
�S�@������<Cis�a��l�3�?[��6�p��ۦ<�	�򳪛M����ʽ�6��
�o2��	Y���Á����]�� R���7���� �no��{m�������=�J}�7$P>W�����H���?m��|R7�E
%o��/9����� M�Xc��	��	*kMu����RB}!1�ۥxR��z\���t1u�C�t'�o*�dz\�ty8��[�nܩi�~��T̨��S��O�a�Ǘd�H���V	��u�@e�.ȟp'C��,[���km՚Ot�~����E�"��S� :�D�b�E�/F!­� ^#6�,��!x��A�"�\���\4j����{�v����%o�XӶ���%Ig>;U��o�6�8Ս5�7-�iǬ�Qr#^$��R�3F�x��
�D��M%��r���l9�X��zIK�A�����e��������i�aJ�5l�O�W7J�xy$ꂗ�Tlgm�����_Dkt�1���ѡ��N��id'G�ct�P����@�s���a�QCg��!�8���w�(�n5�C����Nv� ����D����y҈Oc��\�uj����m/Y��    o{?g�LK�
�.������nF�63@��m2I��$D�L�E��qx՛G�aBbG�3��]o�TA�q��?�>���K�mj�I)���7�0����~�`f��~�Q�%����6q��ͪ,1�(����Fj��:m���B�i��D�h�D�d�K?�HHeuZUŪ:��<qk�;x<t�t�+�J�n���
P����V�߿d�.�C^�K�+^�K�^����m���M�-�u���#���Sq9�}�C�*��?2Z(!{	\߅쥄d/��==R�U8k<������Z�(����ꍓ4����ޙv/��ё�~�O�uc#�KL����L���ZO����	뽇M�e��HI��#r������A")g�r���,̫��$���
��mc�Ҟ.�8���;�d�ib�yH'���ꓡ�����T�>���D���IF	�"�h�����.>���]|���S�8B�s�.���Ri�7|sA;]�ߒ>�;��b��ab�oF��K�h�ݬ�\�Jf��GP��T��$j��CD�K!���C4�Ǉ��	BD`ڈ��n�oM�eA,�al�}�Ǹ������w��A�AC`����2���Mw�p�NYz���zP�@ ��)���+�܎�%ۃO�*��m�r�^��;��A��}��L�n�������Hh�s�曝*�3u��=�?p����#�-�p`�[�x�D������?#����K(�`�S22=�����	F#��"I<�3 NV2n7�2BG�8ݓ���8ݓ)���.*�	�G����f4��
��>]G5 ���B�?���
]H���7�N����Ҧ���M�T#*CC�*B��TiC���RCT^P�Eu)\�"��D��s��b v�&���rӣ�A5!�S��/�cw����W�N6:�^Qe�`U���}$�\��z+��lw��Ň���V!?Xc�t���ƃ��7�k�����W����O��p|5�����|Tt
uķ�|���|���Ud
�d��IXTaE��ǤsE^�Є}SLa��X�R��J^؞D��dC����� />���,�O���X|��.>�Cćq��0�!��|i���|��pqv�[n�>���!b�@�:�A�
Ѥ��h���O����#�����S�é�\F�q�+�=�Nq���@�bLY����|�A�G$�8"��V�h�83l�PV���DL�����R8�إ4��a_J�����}\J�ú����K���j>�x�!hB�#֥L|vqid��Ć�	�^�*�U�d�v�t��xa��N:8�4�b��t�&�&��n|�r������u����tn����y�`������>\���CUJU�:;�>��|��z���uzͧǟ�~���c.!�E�	򗽵���ۄ>A&�y�O�*��J}�\aݝa�a����]L��&>.�
�kׅIRa�0��)�T��x>_�V]t�3mv'}>��6�p1A�NWu�����B�}��R�:�>�(��� ���4?�����<|���H���&)���50�oσ7rpc��Ɵ��3骱��i��?B�FF:���q��Vɼ���>��W���=T76�0�x'ٍ�4�w���Q֛E'�E���^�#M���i���2i��L";!��#��zh��խF��&j��1��K�y��I�_�[�b�i|�%�Q�n��;��@u�(}��|j���~Ȕ���;C��y��l!; �NS�Ҩ�4��<��dȥ޽�fI�k�Aҍ�4}ڄO۷��s㜧t�s��5IC9lk�g<V`��U�[ǣ�Xb5Ϭ%Fٽ��~�fk�MXbߜS랥�N^Z_��~�S����]Z_�Ց^����N�\�pݵ^�8si,�Y�;qi}e��.Z�xP`���$��=�$]ý�\B��]�%ء����	��n��a��Ս�4|K��XL÷
w#2�x�9��L��PǛ�K�C�x�}[� O�
Z�'\�����֞���:W@�ZTk�6G]��Qls$�4�^�_��]_��|��
���+@�7/�����B��~p��v�:���
��R���R����)����Q�>ժ�0JhB���Q,���T!�ˎ e����0d<���<�ݴ�'l|6�Cq9���>�^p�$A��O�|R�T�\H��^&��-���j�s�t�,!�m�˞m. r��@zN���hR$^f�@&�Nj�p1�����md�w���0%���$���TF���ϧR�	P��R_�&U��[�N�2�z{�Jy�,Z�&�"�lBE0&�a�f->Q_��_�kQ���xR�ț�;;]�n�;�cӎ�Z*c=��cGnڑ;nE��46��a���)wn�M�r���r?�[;�	�t�!���p�%�:�î�O�{�2Uv���;�L��Q�Zг)��� (���R��Ÿ�N?�ԍ�4�v��+4��C�ls����>�r ?�S�"�����)��!�=i��)��KW�S�>T)sT��r�R��O���?}�u�u��`�@)��[ŋ�P|ѫo��o=�ӟ>�i*�bv��	P�����Z�S�4�_��N��|�R��:�g�\odRJ�]��,:��MYo �9T��'T��<�f�7�	U��8a��M��Y[>��?}���9���6=;�na�˘����~.r(�G�g��-��ّD��)�����Fa
!�q�P�E7b�nL���k�O�r���N��l9�p��p�$��ʜ�	�r� �|f�JodO�9��P�F�=���`�R
P��%_Sj���Wmb�ʉP��G/΃�|�%��>��CMfR�!36΃�TjFE�fR��'���)�0�z�d�c��e��џ���s #?s���L:jT^=��O���V-n턀'���ݱ�-�8�����*g�."{B���X�ZWn���J��r�?!�!�{�eR'�JP��ܧ[��r��:���I���(�^Ky8�lP}>�8�n�@(E���Ip!�%�Ù�R��LzM�p&}��;�I/e@!e�O^N�W' )�M�ɊWZ10J�|����LJc�"��v����GQ�D|�kсџʦ��Ǥ:9~��U(&��f�>��7�S���*�&�w	���Tűў��ǽW�Ǥ���:�&�V���|�y�J1+*�^*�yQ��v�|bf�nħr�������m�<�zjn���>��1����Q�ʦÅ�0�:�����t*��ihE>O���n�E�\n�����q*��\Z0��kX�8��ш������M���|���xM��z\gޱ:ܟ/ͧ?,��h �E�ѡ�@ti]4RD�V"!m@�i�C4����8K���@`��aq�r���#,��n|�a^	v��4��kF7-0�{����^�Ӓ���R� �1�R9��?�.��ϊMf���3L�Џ��X�Ԉ��м����oK);�u�%����ØI�^ӫh��c%~�/���T���i~2��+c%��}�e���N���j��RHUE���Xe�Q���@�vѢU�h�*Bth!��]tZX�Z-aԲ�h̫�����9*��-�sT�O��q���A�F:�ꚱ�Ω��i��� �XG3��y���m�"|�W�E@�� �A�8c��,/��W�'��j4c��fT����HF����0�Q9��J[���hK�@���z����p
��r����\3��!�Y�+�ϊ�q������d���u�m0u�Т�k�꽬5�-Pd������l�j�^4��CV�%+t�<d�5�B��L"v�Z�<�����o�F$*�MΤ�b��ڎ� 5fK��7`�&`�4`��w����M2H��A�� !:d����B� �@dl�a�%le~�����s��SJ�i"�ve��(���Y�������/�U�_���7���t!j��B$�����-����(Ok�x�i郎^���Ii��~���z�����z-B�сFf��=����)`HH1T=���E�    ��i��w{��,�i�)���N���N3�Wm�������F_6�'�_>b����C�<PՕ��w�TW�w�2�\�+w%^�Xӝ^���Ds��|1p��5$�.'Z��#��iVP�Q,5�C�|�pQ�Ae���(ʠ:{�eq�Q�Tx*� n\y�hjCCQ��(P�A�����Q�Q���3�Q�A%�����W�{R�Ej�Fq��N(��<x_J��җ�Ax�2��/�tEU��U�����u�=YP!_W�U�x��������P�x��c����K�"[B��_D:�~�Fe�W��i|���4�z}yS���4�J��}y�aܟ��?��N��H0,jH������`<�.г���Vh��� ���T�g?��V"���T�H0T	�������VѦ�m�$�.�}�8$b q�:��A��\�P���^��D�&�xP"<��3��R:*@�6 ���8��(=;B���lO$65�8D|�����Q�xF���_ٌg�I<u�T��*Tm��;"{��C��ZNэ͕#���a��	O�a��� �@aS����'l#�L-�VAnaeëF��M�C�4��Q:�î�,��~�c�����x"����[���JhK 
6�MP/DBk��sA6��/����t�(�AϫTQ��.A��_`��l�_����_�F���欺.��<p8��i��u��+�L2K��%D��SNN���L�T�tR�u$��`h��2�*0�?t������#�.FNI�s^��u�p*N�!g�l\��^kKd�ù8�[M3��S��ԤbR&&��).��\�������{�ϋ2�[�>1J=Ok����Ԝ��}���b+�h�᜜�?N7���}�}�9ۗHv8!g�R��l�[��t���/�61s��g�:Sf��0��H5ګ����v8g����`8gc�#��`Ū��~1��I�`�����pxCBv��8�z��G��.�Sw�$������5"y�EfJ�MfJ�Cf���o���[fz���SVzb�V�xb���S��N88g'�l�w���7�*t\��c"���E������&��,N�)�^�	8���9��z='��zMm`�2�P�e��ιʄ��.>i���{�ᴛ�KB5�w����(괛��(��t햔#;��V�ٮ���2�Ł�h8�fX����������6�N/�q��CkL�F��R�:�-��Ը��xФI<Io� �us8�f�r#u�{�/,�>��+�}_Rd��yj�W.�i�P$3�vS�HF�tr4�� GCb�($^d�$��hH|�ѐ�V8�\����,ٖSW~�7���?���겖l�\�U�#"�ҏ*s곯W�ٮ�7*���4o��n�>^ƾ9�"���.<�3�8 9]: �v��E\�-���߳�5E\t�����ܳ�#��;�|8�pLB�?xv�j����C�:Z�ɧ侌{s|J�GŏV�Q��n*�/���!�60���2&��$^�$�D�:�h7ǧ��xs|N��Qon�Y��q��;��-ݎ�@#t���A�q�+ I5i�pNp6�eT����!J�P���$��o���q��%��%�H����9�;Ʀ��K@.���9�ӵ*$���i���N�M>|Bk86�║s��*�{�Y���Ud�p0�.槥r8�Oi�ŷ�Z\�w�'�"���Y�ӷ�/ևL��i��Q�J��j���X`��㻩����%�d19'��9I,�,&�dq�r^�Щ�,��W��c���b��	J���r��m��7]/���	<D��5��@j&#> ���+,Aj&�%�t���jw�b:!}�c��τgϿ����Շ����s�uN��r��^����s�\�7��9�X���+�
mN\�ש���(�Y��^�(ob�ph��'_��O�ܪH�?�|ퟩ=1t�#c���&d�<=)R5�_�'V&���W�4��E&�0�3�N?Rqx���Ϛ������}N�D��>��s~�yUl������~93�����)�bs�S��b�#�r�O������j2�K���*�:j�j��:?��|hM��:I��%y���U(���v��O���*ɷv8�H�)�O/���������/fI�t��T�p$�9SM�s0�'1�o�5/f ���4�m�No|���	���NǤ�1[.?���t|.�t�@1	����H�2�o�$��J���ɕ�,]�ɕs%�ċ+	$�J���@�YP�ŋ�\Y��%QvO��#�$�$�,��K�d�oi�잊�Nִ�(��	��Rɯ_y���PԨ^,��5���ĸU�L3�|���tHBԟʁH��oJ%���k: ��%!D��EF%1�ʡJR�<����6'��1�7�8*���*��!�<T*���y�T���a�C����y�V^��p������=q�XE���K��5�(�P�R	�.c�\]2F�H�Y���m[-w^"��-Ō>�\}�ţˣ<C�Jgtyh�rw[�GL�#M�o���!�F�GAi(�=3c|t9��G�t9�O�dBS����э�/f2���� ]��<�cz^&!��']q�5�UV'R��U8��G*�ՍQ^���UϊN��3�T;r�����r+횖J������e������l�_��.��Tdf��_����_m�
��"65�	�D���O�A_��Ĺ�3:�%N3���^³�A�|t����U-c?;�58��t�V�4���G�`N �����53,$��u�@$�#�Ob^�H�(����8Fŏz�c�OϚT��'kI3=ZK�gђVz\� �i�A��{�Vޤu1X�FH��K?��ŧ�$>Eİ�$~~J��w?$��vݛ�pV��4ĮV���+(�>ũ�ݦ��@�1+�K����2�Z��5]K�����k���a󣝐U5�a%��oHz8H�}�圅/H�T�\j�{���4��^���Ƌ�
�G�sνA�������o|`�:D-rSqq����V\m��W<}��:ܭy�,����X+���-�De�
�FP���Rf���Fy?c0�M�;Ja&
����d�h���w���/��qC��4����S�����T�4<�o����#<U�3p|�`���]����o�M4a=ڞ�n���E�� %a�`��݊e0��(�o�`б�Rq����K�����p#�m;(��*���j�FƏ�\�{�%�{� fw��,�kw���[0�FwR�5@X�I/���+����/�f�:�]i�o6j��4�׉z�J3|#߀�4�7��vV�2�rg�Kk:�ud:`1`����'}�~�Sl�0���[����]����I�4�&�षk{oI�-���ZB4�?w	28���P���2�@'Eֈa�ȏxd �$�@�E`f��������b�gDI!��Wgzqt���ӎƒ�R��mo,!�6"Np��V����μL2��~xC�y��ª$>�UI|	����X�0���dj��&�(�����XX�n,$1K�F�z=z��둘�:�.+F\�?\�k�Q�W��Q�|d��.�q�Cx�2�Y�q;rgi�ۑZo,�ۑ��YB,2�+�4ģ��N�����RcDw�O]��\$�����['o岗�ax��46x+zύ�t|�j׾u���:�3��� a{_I%@Q�{g�K�����PD|zo	�:���
���~��<�ZB��q�S��Q�|��J��M��	��ΰ�xc	1>�r���5��f����mher��J�N���QirxLH��d	yW���%�Ǫ�4�˓t�,O�ҩ� ��3TJ���j�\Y�ՒεI#^U�)M��I�i<Z�Jg9�#�z�)H�9�^^���b�]�'9����;�Z���ɮ�dPoP�.T[��%�j#=���]2��0�,S&���w��ڂ�.�\[)�i+)�9N���S�﹤L��Ïb�Fd�԰x� &�K����+��H�p$��;��h�*y?�r����.�PE ��6    �~ׯ���f8��;��.	T�	��dqFҔh+).Zu�MKho�$���$����7.��f�$�����J�E�A�/E�f>#��j���U�d�hJ{��6��`d����y�"EZ� F�?�4�q���[���
NAP�O�To��(�b}�T�i.��X~���Q��h�Ii���Z���}���F5teO[�)m��=�Qi�����Җ=��&�#-�C�YH�H�Ɨ��$���q����V�8���V�8ݺ�Sڨ��[!I�F���gx��/b�I@Tڨod4S�����H�y+0i��4����iH�|��jH��\0��Y����6Iw�C��n*5��6=ҝ��Tڪ�|Ui��:Ui�>�~�#P��j����}4�hL�P��j�B�D�m����+�*� Qi���x�>��t��4�T�7j�}��!�P[iZ\��H ,Me#l��%���7�+���(K3���[���b-��4�d�RƤhN,�Ҍ�)���6���
[��6�����_��$f��<J���ʼ#"����������n�r`H�I������g����J��IK6�"Uj��'C��T��*�lC�Q�<�K}��GX6C΃�r�&�I��'5}��4b|h�S�U1><��4�Y�PŲ4ç�x�C)��kj�wi�G� /m��y�*R�b�}�O+vi�sHլ�%�N����x���^`0��Ƀ�ʚ<��2.��%�9��|�T�mе��ͲZݴWd�����%�&H�T�R��E��+@�i�N*AT�����ұ�_*��->;��H��и҇�K��R���G\?��%�:��4�%��3�X��:��%i��h:v�c��ZS�w�t"J��LxN�
�$1�-S�>%��ye*S�p[Y��ַ��N��������!x�����&=���8�z?�MSs/�/�(�l�/�G`6M�=�&���iԺ /�5���Q�>��
Ħ��1uG4�}����&OOĜ����qD��}k 7��M�`7m�T�y&uz�4���4��h�E��e���$�i�<y�iZ����4�Xc��T�i��o����^/#�1��P��j3k��:������F��U���X'���Џ��4P���4�Y`�����ڤB_�s�_��"�+8O�/்gWa�,�ħ�W�L�&\2�|�xc%j�i��3��:/9�l6B 
9�c�b���H���:����bb~����0�(�
�O����0���5 �BVl��}��}�u$s���C��b���V�|}���a�ϛ9�a��i�ڤ:���b��L�ԄæO����R)N{u@���מV/ھ),!��A�Ih����3���m)Ԉ�>A2+�B������I��۾E}j�?���L?Mz��՛����Ͷo�A�f�1��b�ъ�_�(;cL6�l�^�6��¯��ލ��ȭ�ݹG�j��F@�=��w���w������[�N
O<�^6�`��Y�;6D UM7­�����j'L�k2�c<���(������_�y=�.��^�!�N��1��x=�5��Z��f`o��t.W(��%�{(��?/�m5#{gd�G��3�_��6�>�E���q���|�U~�f9���h;��J0������?�������R�W#������!����C��qg\�{^����*2k��9�y=&�>�����-j	CT�k������zLy��� �5�?cO�/Y�o���'�{�Z&�P�Lgk:��*��U4I��5׺��kd����2O��9f����5"{$p #k����V�H=4�h�����%�B op�"�?op�B�)�zp�b���)����)�=�P1=� 0�M�f`�B�Ea��3GP�fΈ�@�w�Io8#�_ �6 �.�/�1u��۸�@&5l�~Q��q32y�Q� �k����9��UZ�bQi4���0�7�M�c,�:B�L4����C2��f��&�u�B��5m;��AG���3
~os��lP�4
�E⎱����G���t$�6��3n���P��}e�M8�2�P�~�r��d�@t4��u�GϼCq�s@S	ҧ퐆�&r�\��^	�ZW~^���+@�>-T��2��i�Ҫ	�`ۧY
AHC];v�>����Q��T�N�N��}z�ll��ZS0�0�e1} �p�n^�	۸?F�g��m�c��¶O�
@��^�-���mջ��Xa[]4�[���ϯ���+4��k@���qb�4�<0æ���A�ƪA^'cʹ�d,�O���4�E�ylF���ۥ��n����vj��M��޲׊��a�^K�5��m�æ���g����,��m[;C?�0I�۵�m���5�7)/xx6�	�<��V�!�T��Q�k̹��6
��u�V�j��Cl�V��(��4!��;�$�μ/�*��s���<��'�`�*6�zܭ7�bS�ǣy�2�+�6(cӁ1���9:� ��A�CK=&C���"E�yu��tA��i��(ܱiG=�z޾A ۘh��Ȏ��A#ۘ�B��`�u�2��9U���(^Yޠ�M+�_�ؠ�M+��6�eӊBV�lE�t��%1���Ӽh���h�I�/Ϊӻ�7�f��J�Q�i\�U� ���ʎ�/���s���q11� ��11���p��!NH6h��re����\�� @8�˕M��m�ql޻�p����B��U��c9D���;]�6�h��r%�)��z�a�^<ە�h����M���������O����\@@��C7ﱡt������ۼϭ*���y�?V�U��f����ּ�=�����c�����t܃��}�%�cf���q���|5yln?n��-�Ba��t8C�G���s@C[0�@4�CP���w��DS�ɩ�쐦v�*cD3(��Pc��^Bl�M���g��lG4h���+'����ÙW�;8���1�\�����mp�*:�LUh02�Pt��#��[c Fk=Ҫ�<�O=�`��؀=��؀8�߽��z�����z�@ol�a��z����ܩ���n��z#۩TEd���7��5��w���
�۳��L)98DＴ_ڻ�:��>T��k|�4#��ϱ���?�JӶ�ǱRnqx�{�+Qç�=�޼X�>?�=*����Gz,�y�GO�ٵ�T�G��;~ѻ|.ڵ�6�}���ų�ݨ:�	V_��Mk׬�s�Fj���Yz~�hO� �h��Mfa�)d%�LV��CV�۴�(+Mj���Ë7�ud��6�0�5`��6m4�F��F�h-���H#����`�K��!g��o)~���C���;�6S�l4��vV�X�ޘ<p�Es1���Ds���m>��9�h�)t�m�������0�(��S��>���z�CZ�#�Z'8�B��	��С��sH9�vĈp��z��^���~���v�GU�1��E�����"������*��J��J=��)6�J�;X���ᾃ�-�҂�-���O���_2����	w��	wdU
�M��	�ӽ9�.E@C��g���#V�D�ğ�c���T������%o�����m�?ڿ[��������;��xa�Qs���2=|�ˋ�� �W����s��?�����b�>>��v�]��CA:L5!�F}4^䍅ZT�i��`|�0'����a��{���\���\���P��*���u�Qk��Y4���Ʉ7���T`�QS��)�x����-S�O��.a|
07��y�~G�_�iMw�cp�0�"�\(�X�������|�z�B�G�n8�!�5�C��i�1�Z�#�4L�Q^�^�ajd�0��=[��(�GZ�d��O��e�Q'X��B-�>�J�G�G^f�A��*/��;/��Fv	�ɋ/����#}?��6�o[Ѩ��m+57�����I�m>�o[����)�⿿��VS���~t��mx?6��������p���~����5�n�    �U��#�7->�NqK�B=�#
ʀ�W^��}���&��9��1R'{�L #��M
�o���W����b:A���u��L��6���@�sG�=�JO/�m��j���e3�n����"�ֱ):џ<6C'�	���ͰZ���X���:�M��[�r>��Y���Wq 1���E�b��I8f󫩹�c6w�mP㢵嫶ks+��{�G��1�?Z���a*6�_�<�!c:�m�ꆾ޳����v\�k�g棡�~ϧHC���/������ޮ1.�ޮ1������i�7���g両��5n8K=9Iq;b܈�r4��7BxGS�/n�p6�m��F�\Q!�q#�w��lp�fg_�;nƫw��d:%��$���A�����K;� �iؚ;�̦�?a^:>��f��(�ܬW�j��n��F5��a�Y�j*3�<?z63R�xVX�EƢ�ޙA���m]jS�hE�>�5(��fd��Ou<.����1�r3�c�	��xmK�y@��I���GM��8�rCw�{�6�r3���R������%��	�cnXz�Q�-K��Q_���F9y���^�G���F�zFc�SnLf*[���4�ajA��9�!;{���}�R�u�����/�sލ��M������C�QgX��Mnd��+Hr#�D��Kn��H� ���8#zW6xr#��=�`�͕vD>Un$�=�\��rF������.(> �� �z���P�Z�xp%�_�����t�=t��8kl���+Xs3��kπ0��=���Q�$��t���_c�@0U���)'S#8���1��1Նu	yn���8֍� ��B'�(�Sn�a
0�9�� E��f~��Ka8����A��uOY֢�@|i�i�!��I��*�Q̯�Ŵ��0E)�-���:�b*1���dē�@g(��
�J��U��X���N�J�r��N��!w�A<+�F/�^�a򦌮��!�wM[�`x�¸���~9��c�����	/0Q��V;�n�썗s��u�I�K7���>a9x��v���*v���\��KVj���KT�����[���f"^��73��j�HE�֤���^p�f)u�E&9i�A�yl!�n8f��/����_�D���~y_�R7���vuM^��%�(o=���n�I�t��M�*���rLA��ɉ��n��n����+9�u3��Xv��qsr7j�\��þ� &�@})^�`��Iow��֢����_7�.n�m�릩o�t�t�~n�릻����M�]�'�	> �Ǝ`9��K�:J��*Q+|\�c�x����1�<��_7b�9��nkԲ���R�!e;��(&#�0�rC�Ek0�&H�ʠe�~c����!�֝ҝlZwjh�e�f;�a�j8�	���;�Wѝ	�LU��%�T�S�r�Y/���'':v��x��@��x����[�rx`�	�]o�k!8\�8���1(�r����T������(�T#����ֶ���,i���DG��]«���X�]��T�Ѕ:J:Q�����NT����%B��B"t�!$\�au����%~t܀Z7�(����KLM�_J:YX]~
�qr�r�v��5���A9���sv(L���J.���K�� 9�p7���	�v#�6���,��o7����+�|��$�ݩF!�����|~�S���<2ş�գ���}0ȩ�;4ȭ�;�I�CZ5��C�\��}eo0�HcG1!.��"��`G&Vi �U��"��C�c
�@;�0_Դ�
C`Q�or���0����!䞄A���L�>I%z��`{O*�K�(&��u��܃��{�	�4ٓ��U���J��$��+"	��7C���B#���1��Ǽ�B�-ߎc~�v��q�Џ�k�u�I���of��Oo��On���o{�����㾵�|�1��'�\纫�$3i����tc����7��$�������\���*�6k9�.T�q�����B7vo9����k��%ތ,��M����n�����&�\�wܹ�u�:|V������g����5%��_��$���|�1m �UIЊ[���]��i�H�r�ѝ�[>0��5:�uZ�%o^��m
y�^nS�ț�*�]�	Yz�����x�f�)Z�۰vX���u!bh5�]$����|���14��N/x�xt;^�. �Ƿv����7����6i�fʋ�׭�ֹ�C�|��J���������>E��u�S4σ�7�E ��7"�e���c�^]��|a�T�ɓ,�P��4�Mߕ�v]��x?�����7�F:W�Q�/^�{�i����g��י�0�'�������}R*�o���]WT٭�QB�s�yQ�z��MD+~ᚶ��Q��5tg��q��e��s�te�i�<��.K���Iĝ�;�4)q�:�Ꮂ��E�Z��F�u-�`���,vo|��7-���\��4�.ȂV���>�.�b�B��[B��+~(��W��/X'D ���#��%*��WLmrL�kQ��k����d�=���b�x�PLﴷ߲.�;d�`�Iu��%��M�aL>�T8*�H��_�N�F�bMZZxztH�)_�"Wlb�,M;����zѦ��ͤ�^m&E(�����lO������_h{��K���gU"	 ��iu0f���Y��'O���f�T�ߛQlk�����'��
��}A`��6�!�C`	o�l1�F���!H|	!��2&�j�ᛀI:��$��Et��.*��H�S�l	�BoB��t�^Έ��*$�˔���}}VZ}̪Y�8p�ă�E6^3
�\�� >���,&�ݚ��K1|<��OE��]�8n�[�ދ��$}��c���i�@���]u�*o#���1	)B{l�N�Bh0�nc��չ	Ts�Cm�ZB:��=�~B��s:~�
�����T���Nu��61_!��?�#����WbB?��9���}r�+1�E�2�Pk�x�Po;��<��;u"�<�Q)9�Ia�A�A��}�C�Űv�aBK��/�3�*ɋ0�:��/udۘ�E)��%�?��;p+L��әtL�}�<�z%{�gR�dϕ�p!�g��n����^/�}��T5rg-��!v����T��1��uB���)Qw�GJ^�@~=���9q�+�;'����xD��DWu����#v�ǻb��#s���7���ӽr6U҆/*gS%��t$����l>$��|&س�x6�	f��"v���+1�y'�?i4�|�b�������ɋ���;*hn٪��-1��o'ZbB��]g���i]�㘸�aL������ʍp�*4Ӫ��S;"TKS߽lx<^����킁/Q�9��}�Q���
&��M�6������3`w�Ğ�=M]�����[2Pu�����N���%�0�(�#��&*
؉�ˬT/r=��g�8��"��@Pߥ� �OPߥ�.�C������?H�䋩l�,�9�a���ljU+������%����� x�(^�o��}���K#��!{U(�hvm���t�x|��ar�i�u�q��z;_���į����{W����{u�B_ι�f>�?��+v�C��/��Tr��R[}�a�Xq��"����*q{C!�:x��z� �Ke<%��|�8��^��5���A�$���A]��Խ&CL}��wRyx����f�[�%�腴6�����6�1��U��2lKzg���&���;�Z�����QR��թ����κz��6��Y��*�F�;���661���q��N�������+B�:�^�/[������*�r����)���|yu��[�������p���>:���޿G'�v�L	{�P.�;��}u�
�)%�}tV���L��s6��肴v��b>|4���dRg�1+n����i�l�Xl�ź?���}�-�� ���Ē��������+[�#�W��:���eJ���t��;+S�cP��S��G��8"Q��GG����T烪e~�cQ�C�����9C��z�.gJ��>�H    ���sN���9'��̸�B�s0�/Q�����Ǉ}��ס���Uc�)(9��������̛��{�J̛^A�ʛsK_Wʙo:� �q�YYy��o���囩���z����
��6;���7�x�|K|+#�&��U:ތ1U��*oܸ��l���?�J7����R�ja�t�T�O(�h.P&�W]�f+��M�N�7��M7>a+e�M�CO��M߮�V���L<�����iC�wӷO(E	xӷ�h����5B�NK�w3B3�(�nf��M����7M(��xW��'l��M��ҡ�[�z�U�W�r��ջ��3���l��]!�;��+��+InܱJ�A:��q��M%��!��u�l���9��k7�x����2�fo�'*�n��.ߘ��xG�S��W7��g�8N6��M�Y�R)�n�ܪai�*L2X�f��U���L^�Y�&/*��;�v�Y�u��Wdt��}u����+������Vv�W�gݶK*pW$e���ŝ���q�Syu3pWg��'�1�3�AmtЋ���r��۫��M��3�Q8�� �����cЛ#����mD$�cB��}�V�tmтr�Ǚ�2��ҭ��¹t+nzk.�J���˥[�sS{�t+yn���YJ;��P*��k�5~�rFy����{fy�N�>�
�&��qwzvR�<C?Yǀ�O~97���pƗ��� ����˯�t���t���;����kJ:wUvHJ%���,o�R���݀�P�7G��0�Z���CYr�0RR�V�q>�9�����)ϩpBH�9Հb��ksj��[�Sn\�Fen�f.ڸ��D� x���b���̸֢B"�8*�*|��]e��)��c�tBZ�� �#QK��$��sA����Fu^pTz�F�1�j�S�/=�v�]�3�r�����EJ3�A�?��0��!�j���E>ƅ��3�.�4�^�f�17b���Z�FjQ�?YYf郊33��sâ�%�UQ�E�����bCh��Q���د�Kf/rx<{ʋGIoyq:��ަ]w�jp�����X��6<f�.[��6|x��&���%��Ɖ������x������(�mz�ݖ(�mz�(;�W
���J��V�"kDd-�q9U��6sK)u�
��fY6-�8�f��&\������8�f)gM�����E�A�(�m�����(�m��Ȍ�g��q?B�үG�����A)Ji��=�z]��6�ð�(�m&l�J/�DMe�3�"��Ŭ�A�M*�D��K�4�]��,�X�Dc�o�P�J�*��*��=�U^����eޓW���5N�g`)��|����Ȇ�4hBϷM�i��!	h<_z��x4��x^��f����4��L���*>/�H��F��"���K[�(gH��E���\�"߲˥-�M�\�"�|�[P�����b���#XxӰ�v�Q��MhlR��jMh�{��X=>�'�U	�E6zR 4�,�Ե^�ޏ
k�WV�`���x��'��J{zq����ڟ���t����Ӂ�Y�!���>ޡ��Vikù}`��?FAKR7�!)T5��[.��R���y�j����4��Ie3u��j�T����{�YI��g(�[E���U����=�o)��F����N�J;}T:�@���{gx*]mz��T����L0݋�����L;�U���iG�j���?��=��յDeҭ�A��	[Y�CS&݊d/���4��L�CyjC��yǤ���Z_i�~r��&��U�Vıx3�0ą46�@��Q��Uۊ7�kU+@����&Qde�}���[�p�zP��J�F�R��+�X��U̯ܴo��BL����V��W��0+����{cZ���{m̋J�����hO�q7f��wո5߽K��R�Yi�}�4�0��7M��LV�����6[����w�l�S2ڜA9���v�~�E�*��(m�fQ*�4�6��D�i����S�٩A�e�Ҡ���i��2���2��V�ɞ�ʺ$���-��?��N�ė�E�g�n��9��봽�̳�+#�T]���k�R�����!{�ꦪ�(Uݜ���l7�W����.�R�(�n�R����6��J�'��䰯� K��rξE�JH�uBۯlur9|e��u�d�a+���ͦ��V�d6��d���(vEBFk���P�ٴ�	K)�lZꄞ�^6-�
QvٴT*O�e�R4��t�����n����(���������|�N���uJ�4�*�v�/� �ɞ��)��_yeߢ� ~�g�Hy�*�mu7����#�me�M���P6�c�wmD�dߢ=�@Hyл�D�d�gz��85j�M�rɾOӂ��RɾO�B�K$J%�&�X�Q+�����tU��OωjK�u� �+�lN��g��ţ�*�`���>Ax�.=�b �
��K2�姱h1�����i]m��a0#w�m�L��vy�H]_����x�� �%x��d�B�dΨ�x���D��F3_�PF���7w����ȃC�� ���S�^����M��1��]��
.�r�>*�3JWB����(c'+�F��K1#�-d�����h��@�
��N�k�w���L{̈��2�0�)s�0�!s̀㐹����8d��:d���2�A����Mr�ʸ�߿���ёS͊�೓�[��g'�����,%>;��^��tp����Vn�VnxY�J���V�����|�ϲ�J��W�3s����}+�=3�ΊdZ<Y������h���WnU�f ����W2[��w dzA�&�T8j�Wf�����25w�Rg�Ɠ;���p<�2�K�?�"J �u5���Gl���5�zfQB���ƭD���e���O+h�Ϗ�:ϒ��U��)�V9�"k��Sjuf<Q��T�,�v*��(�O/g�������SԮ6�ͬ��T~:�A'��,�5�-Wұ�w~�u4�9�^�Ra����-2'�vs�ޢQˉ�Ԥ��;�9+��+�2'%���9�J��R�>��IEk��֥�IE�'�4�`�����5¢�5�@E�Ly^ӹ��jyr��ZǷ�d��Z�z0W��3b2W��8j���3a�M�㹚(�k��u��3�S�pF�q]�v�5we�5�i(��\�Ī�UI<ys٫��t�Q�ƭHm���\�����ڰ�k�	5 b8|VM���F�1z|�O&A��
b�1x��R��}j�EÞ�ݵ�u��bL*4��eJ[����M*d�逾(2�-�\ư���5���&2m��pi��K^Ά��d�����ys�:���R�\à��A#�ΘF��K��Y���ǟ�e5׎��͗�\?;Ǔ�¬G�|���ڎ
�*��Ju�Z�zf5?1��r���>*��td�{���W��:�a�aY(��>��Z������aК�aW*1OjW���<i]��� y�:
�-�j�{��W�.��W�^�v=���˚	��h�%]���1�{+�L~����k����۰c����e(�1���2�&WZ�\��`��2�h.k�f�8�Fu��}�j��ƙeM��>����Ux�	 Q�R�f@6(%k|�`��1���d��R��'YY:��-�&��>u���\�IVa��b�@(��V��Yq��_׾bK[y�Gl	��-�Bo�/�n�4B_Z^�b�_xv�b[|qCV|��6�?��븎�0�ħ_x\� q!^�Z|8 XX(>@L�t��yO�͏ �M� 16Ts{�~�QU���}�0z��o�����G�=^6����,��ժ���z�N�	bɪc��q%H�W|r���?�;��+,:eY�f
�
1�ah)�ީ6�
1�:a�[~L#�r��J�ǩ��,֡��q�+��T�gg��N/<��{���(X۷�\��59N��������1�6:g������w)��4n��K﷾]��I>_��.�yo��� W��+3�C�M�\��^�`m�$��x���	9��t��(��N�#�����v��[��y���AJ�O*n7Т��f���`�wɚ��V�GAP�0��p����j��Ri���]�P����J���;jTʘ���ʺ��&e]M���7���FiW���    �����#<DIW3��A�s5�;�J���M#���ѭ�����p�
����=���t��2#�#[m�+�һNx�:�c�id��?�gi��i�1���=lL�탩U�)�V��p�<�1��V�-�6.�q@�j��w�гQ�����5��4Ȥ0�i��(����6�ѐJ):;�m�}?w-ʵ��=���(�j:�}�F�h5}�V}%ZM7�%xۥ؟5N�Y�����S�8`/�`4�)�/O����sOd>��ʏgԿ�����v��c�Y�_5��f�+����(�j��eV�\�����䃤�Y�U6�g9S� F���\Q��ȳ)#9@��>P~b? �OV
��d��� BO�r��ƓK9,����nm�(T����JӢ�P>{�}xO��0���r7C�P��p�Cg�JUx�n@�����C%�j�������u#Vm?"��P�icJ��B�i�B�i�B�A�������/D��՞A��h���0
�L���������ó_��͐2�RvҊ�6�BvR٭T�]�J���'�l!ʧ������f-�A6���(IWe�T�4ʗi!q��~F8���Q�J���m}�V��դW��7��hT�B�{�A��BgF�ڸhx0�"v�Z1�V%��Z��,�I��{)�M�}*�x��k�T����Ӄ7v/��p��GœT�@��+T�M�*����)�<�+�p�ԯ�6���r�ڲfw��ڲ>�
+�|����(�Z�8N��R��xv�O=?�/����
)l6�p��K�P~�� ��hQ~lf��d�P�b�D��_�³��
}Lcm�]�'�'�aq�<���w���zg�wޭ�Y�7%J-�J'�����'cQmt-O�R��*��R�e���� �S�Y�)���)�8E>8E�S>8���8�n��z���Ô/%V#�2
�>(��eE�mW�^貒b����b4��k)z�GNm�%�!�\393Ӯd��B���S�q�}�R�f8����%N��@5�Y�<U�S3��HVF)W���{����{�mR~���l�V��8F�&Ѕ!�|��r��2��jՓq|B��J��W��:9|v�sjx��e�M��SJ{���t�8��XH�'����:?���P�=J*9�i�4�O��Cl̩��RC�UBOF��*35��1�R����k�XҎᔯ�S��O���4��Z��L�k)�<K�=�%�c��_�����0®�-=z1�k�������<zQ�ԌTS�n�\���G��ܩ�A��w�aХ�;�3�Ҵ߆���p��7���mxՃ���%}o�C�G1���"�l�����~�c�p��r�"�{@�*5/�n�{��9eJ���G��o��}<)y�T���.n��f�p�2�����'ʎWK���̨y�tǡ�Yti��j����>���k�U�ܳ���y�4�RDYQ�~o�����]��"�()j�p.��=\6���]��~�\�����W]{]����v�.av�.=J�6�:�CM?��l���R��tuZҏ��NKNWzy��7��2ʑ%&a5J���M���dI���'���C0�IO>�5|�DO����+���:<~�X�O3�$�x(o���S�qꞻdR>tsZ|�ԧ���6J|����b�����d�C�%=���R�%n��إ��e�K7�%��9F�9���/|�C@���$!QpT�ӈ� ��x)F��AOÆyC��G"K������r����S�d����t���:���QDQ��x�Y�V.���clߣe9g��W%8M�ŷ�����q�u��O+�u���C�W�F3��6"��a�ۈ@b�W(�i��Z��4c؎� }�� ����*��Ħ�l��̦��K�Ԧi����x�^+WV���}|�̕�4�e����Q��얯�^�Uv^#�����^;}�����4F���Qu"�(ӌ][:R�d��b���w�Č���$�vxc�4貄�z:���5�ǰ��r�e��o���U�.^މ}g�Ԋ�������I�)o�^'� � ���}K�
{�j�Fio���,��t�E����l�mwQ\J��06��aoz��m�5�A2�]p�.���;%:��jb���|�me؂m�3r��O��u��_�l�*�8�Ͼr��FZ��4��
�@F�2��N^�s�-��Q�5�;p���,<z�^ j����"��/��ݯ��BUYV/�Kϗ7W�����G���Ǆ�&em��B}OM�.��ѓF��_�E:��K#�T�uxAl��[��**l���T8+_�I��_����/��Ђ좳���v�o�J�;������Zz���3x��ȋ�����U��(fy���S�Wx�	�>|1o�2��f}��R*LևK�o<|�����f݆[�#5�;*�R[�����K���dg1����ת��c�x~�R}S�t���F����~^�(α�B:xώ�שp�����{��N���Wx^�x?#��{�"*(K��&�Yz�8d�����Q��m���
�g�"Gx�����Q	?ï��Y�˭�A�������tk�W*, �T�ҩ������O�y-~�g�a�6�/�/(��{9ĠԂgw�Aj%y%=����`,=�<�_�<X9�1��))�O��hW|���BaU�?�H�E��.���tR�N)�������[�-�ƻ��7J6o�&�f
0�����c����.ܞ���+�US��R��?M&ϋ�nk�1��@��I�	m�K�B�ⓙPh[\����$�m�ɰ����P�����ړ�g�Im���^@\z���}�1�d�=U�<�p�D�������R���+$�R+�ҷ#�/�xK/���:�oz����$W������G5`0�K��nm�_��qF5�ߪ��"�e����mV^z2x=WK]��'0������j?��҅#<:*���+?�~�1I�3�-� ��훾�N�6�{˷�Z�o��N�rF\���g.����.�T����}}g-垡㬥��Tʩ#��(_\��z��0��o�*�S���R���:}��R�}�*��m������dz9,�i�����Ү�>k_8���(a)ı��Cw�G)K��̟�V�(e�
�s�(g�
g>�ֆ�>ko�R�Q�R}6�w���Q�R��>)�(s��c'74��R��4vr}k�?�]
�w��vtt�^M�>�P�~ T�6�]C���;U|��(})fxS�K�C�O��])���PsjNe�T�g��HC���vi6�f<R����}�⺚No��Y���?�S� �:Jd���W��'G����e�K��UG�L!����O.�an򛖸��B\%���m���(�MCk�QFSH�v�3�F����v������{��m/w%6-s�x��q,5�E�&��2���-�>=�6W؇b��V�8�Wn?�c=��z|��4�xG�J�8��2*���o�7�x������a �f��;�}�M_M��i�'�T|ʲ�U������	�} �gWA��M�ͯjSGyN1mʌ�~�&əc[�I��s���g�Q�
��|N�w	o��v�s��4�{W����Ž�{#8u�;*�7cSǽ��w�]�T�+��MS���1�rWHo(jc1���}��(�i�� ��#�kd�:�{���A�΃x���R�� z��`�R�6#<��'4��o��䰧�S�s���-k�P�{e�i�X[�[{F6W������3�A��]�GG�}���4��A'�mad��c1�v�`�$ʂZ�m���=J<#�2�V�Q搡���d����R�C�|�}�ǲƪ�zn��J=#��]�;r��]
A�\%���І%��(&��v���TC��Ġ $�
��S�(cֻ�����P�pCcU�ØWcL<}H�����L�7]寔g��ӳQ�ࢦ�q����;����w��>'Eq�
8}.�b�����MQlt?j�)�q����if�2��-���2�IX�yIN_�,��jOw(� Ӿ� C
���)��n�ƎdX��w��(խK��ď|f��FM�*��i+G2��m�Iፇ�QG2�$��-��+�g��J�3V�@�.N�    \������UlO[�Mu@�ur�H�l�ߢ*�l�H�W� ��x���qO�x]���!��&IU�c�,�x6v,�u��X�;�����$�:����{h?qc�9����F��^�9�AL�G=��,鋩��)s3ޖ���e�<���e�<�W���lr�AivPS��ߥe���w�۞r�
��	�9�	�ꗲ� �E��T"w#��^��dj~���V�7k�x蛽U����v�=
�&�n���Ի]���������{x�������o(x4^�o�_��R܍�<>�"n6����mS�XzF
��2ڡ�^�<�]
�(f�G�jl��t��	�b*���T��a*J�
W!����b�]k�M�2:�n,>�Z|\y]��Ng��h�3Q�!�B�i��ug8��U�������i�����cSp� �c���g�C�}#�Ǹ����W�lp�x�ޙ��C�����:�����&��'�1)%CfL
Fɭؘ��Q�u��x���1i��c\��X�jxa&��R��u�脳�Ո��41�|F����`&gh�X��;`h�QH@��xGa)��!�J`�`���M[&�@
(���A;R�؃rPn���l�s�{Q�o �0&6�9�1\��Zt%����,j���hn,v �!\��p����"�z��6���ߖ��+g�)�	���@�Xe�p J@�!��O���?zE���,C����.�-�S��G3�*�|V�qV��	�8��W��EZ�1©@N�0��j�<T5��$థv�i<�C��ۥ$S�a8����F%�!��LF�{^�e2���T��/5�qi��]�g8��ģo�h�7+>��K�Kǣʽľ{<��[V��j�*���Q��x\��h�\
n���!��df�Vl:��̸U�)�33N���.Im��;�g�E����l�#D��c}��+S����Q�51�.��g�:^V�@4��|�L ��9`�A�iF�yp�u�?:w�������i�d'�@�9��4�`�z���'�*}�&�5��f�gߪP����w�����q5�M�ߙ�3�?z>���iH�2l��z��aSL�m��pr�����{M}x��u��i?�:�6?ڰ�-���2������2�9h���������`Z[)����*���P�Zf���aώz��/�f��!k��Z�b7K
sm2tnV�R�!ku�u�[n���n��^�Q��f�F!|3�w�&��g{R�w��܋B���כI�ܜcn�}�7j�9�-G� ܯ��*f�T��)f�R���y�t�)�(���=��yg�$�Bl1�J��b���׾�޵q�<
���i�ن0O��6�F��W�ĳ�;�wV�q�����'Mx����lrͻRҹ��ZTšV���P��h^
D�8��L�۝y']2��"�Kfݻ+G�z?��+H��=ds�I�Ks'�#J*�5��dN��.i�7��4�$�J��-�I�]�$Oj�H��P�����vl����֫5
�V딇��g�Ayh��W��K�g��(;��N�mJC4�P�ˈg9��;�Kŗ��ߦeL��
J�}��z+(�v��ۓ�ΝCQ�-�J�:�b���)��6�.*b[gj��*<�b%�e��-G3�Nk�e�V�r���Ie�Yk���Z|ט�'#y9���Z0ȦH���r0Ð�~�P ��4-G3�5�נDs"�,�3?A�&�ь��x�'`��φe9�I�UѮ�Mrq[�fh�gs4S�����!~�/*��)��"�"He3�-/j��ZG%*O�1Me@�-
�`p8+ eE�H �
�?y�,��5׏GLt~�?�&71��^����a�	S�~ٮp�	S��ۮpm�����M5�hkE�L�1 �i7�Usy�L4��^y�L6�wT>�p��缅K��bv�-�ղ{op�f���N}	�H�+�g	�H���gE����[�ڬ(̴�H�$�j�c����(ԮC^��{��C>��>X��H���C>�b�Cn�������腙�o��O�no��V&�>��6h�KI��\�.%R���Q�'^�K�/�c����m��┩��.�T*3{C3���L�;*35���ya-�8-�`���1��e��~��\�Y�2����2C�e�R���|Ra<܋�M&[*6P��F�jQ1@�2�b�O�qq�.*��h�ёw�32��OM&��ώs&���b���(^�FǄ<��1�3П�蘰^ڍ�	3��NǄ=��阰G�k�%Ŏ1lz��P����k5���QV<�+k1AÕ;:~�ygwn��Ѷ�;�sEZ�l�${Tж#��RG�dz.��Q{<� �������a_�S=EU�W�=�A<��9��l�?��;{І7_�!L��[��l���َ`���_�L6l7����j!:e1y��@�v���ؓ���`x�7:+���A/���J!v��]ҋ\��[�|�����喁��^m��&��x�v�u
V�X��c֝J}���*Xg
��%{s��eEyo.Y��<�K���%���;{sɢ���d_�>�MT�"m/��g�G ��F �Ľ^Зh�G�`��;
3�#^e�Zwfh�V?�����5+b��.�k7~ʺH�Ҵ�����iN�FI82��Ҡj�aL�B��&��q(��k�i)��a�R���L�)kt�Q�I�q �'�.��y/o�ʑ�P��������c.�߿�v�U�/��*ܛ�TVu���y����]�lx��ۻ:�6z%F~�았p+݂m/��J�qn��󳖼�y��Ңٷ��<��K�x�����׆���7�lW��(F>i#�%>*�&�����#����#\��S_���]�`��Zq ��
�ށ��*�TL�*�������'��u8f��@F���V�O�R������j������LU�L�]��Jeb���5���Z[x�kbRy&�w�4�}�ޠ��#��O� �k�.hh�i�'q��:�*�C���B<i֛�'�xt6�Q�H�6�t?i2mvE�x�r�i���� f���lj�#��^���]�Vw2�ޭ�d���O�-�)�:_y��Z�-=ku
��a�����I�{Q�m����]�b��.�ؽ��P��(-���r�;@�+���3٭�'�nw2�6{,�훛u~����h*]�R7��e��t"ۃ�����n��R��}u�����#��X���%��4*���fHʤ��ײ(�oƵl���A��f ˡ׮,.�Ҝ=�|4�,���6觺j�C1(�7)�b��.�P�	��UF�9��^'<r��^�rNe�(�˹�
^!����Ea���-�� ��T���� �K�N2�dt�EF�hbCm�T��QMp����MN-U�_��\��C����6򏷢��V�gW�<N5َ�ڌr;݁?�IIH_'`��$�#fޮUW|�
���M;=�JB;�8��q�����.B'���r��鎚�8����Y'�L�>M�9%b�4G	0s���q������`�}\���1�Ez#���\���p�V�>�q���Iړ���W���~��h���8������x��1��:���q0CG<�'�L��AL�@b�p�oH��3����P���Mk�:põ��p�s�v�|�܎��J>:;3��xf+t!���\�a�3G!�<�9sVd[V\�ph�{�2|����"��w�mf~������Ï�83�Y��+Z�&Ľf��
r�����r<��
��P�Cl���'�x�p)SW�<s��j�9?<h�|j�jW��CUV;��g`�� ��h ��[�<�&��L+ќmS�J���H�Nڦ��̆��m�\j��昕䎨�b�LQz�

�rS+�妾ֽّrS�\!7�AԴBn�E	����"���A�N���r];�����*8s��|z��%È�R�+O��iD��򻏁_�����9ǍX�[��?Ǎ���=f�<�؄ ���A�h�g�����i�t`T� ��C�.����r��p�~|c����ҽ�#�d羊�I�
�Q    ����yw��Q�l=�ț�42%�8�:@2�KA�y>;�Bu`���Yb�c}�i�`��
�r����63`& U�mf����mf�:c���mf�̍��fӣc<k��S<k������J�*{6:��G�O�����;�71�n��I��A�_���������]n��EoTe��i#_��c�:��Z���8�j���୯���G��8��^��R�@�
��a���L�R�H
m�R��)�� ���g���I<�n��I;����:��?]2u��N�)���w:®��!v�w��鶛�S��S���p���+@����u�BS�,��ub��]����G��]T�������^Q�Fwq�"�N�⊅���7�b�3���.�X<˦XgO�����9��N��ž��A��E{�����
��e��bA������F�2�/j�����1n�I_��+�*���٪ը�|��E^�!���/��^�I��6�7PL$���1L�`�.>��Pn:��38�WdK!�yE����D@�� :\�/>�O"�*��YG=�1%E�v`�oR<\��1�h�E��E{����t�*�:UY��{�����ѳ�c»���z5f��٫1�+�̪vJ�Dw)�V%�^*k'5н�ܾs��U����r_���+a?J4����
�n%��V%�jK�^��H��b��w(!尗�N�B�}\-�~$�Z�,��	_�,���^�t���x�뱈�(�p�oZ�{��5��J[�t^��)�^��x������TL�ɼ#�R�
�����w�Ё��~jc�W��K>ȼ���@��0ġ�/�0��/�E7L��:K^t�$���{a2�N���p��s�e��[�/�K�2&�^�ʪQ��e��b�,��3������̢�!�&������^��ךtX�Vn��.�7+�P��s��iMe���6*
H߬����ޤ��ɊT����ph��D�"J5�o�-���(��Ek��C|�Z�rO�V�}&T3���g���@���
_%�j{�_~j8oS�)�o�Q�eC�޶�Q�f�|T���Mx��1��Bx	ݸ��}/�V��K�r�D�m��/�K�Z��]kj%�C�痭��܋�^b<��zsU6r�'�
�׃7����-�����~��[��1!�6��7!���[����~Ǻ���FN��j��-]�E{����xtg������dS�sʘ"���`V*C⺅yΘ�/8cvib��9��cL�L�썼�+4�Q�F�j9{��K�;��e:���=���@�]�ݫ��h�%����[:⭼�Y�j��f^�S�Y�s�®H���K[PK-�6��_ܳ�������p�EGo�o�v�Z��J{�\o�;ֵ�a�Y���y�x��X����]�s��ʥ���l�$5��(��[1lO_�&�k��E�&���������p#�m�`
Z�Kt�/4þ�=|7����RD�^np�r�,^���G����R)�U<炸-�y+����dN�݇,��!f���7h|�E����k�6&��o�1)�'�4?�:�5��6�[3�n��ʏ*�+���tT'��6��ŗ�M�/G�% �os�B�j�*9j/�}騾�Wh_x��Bos�T�}�J߶�gdH���K<kn�%�@�Vk8|I�⥪�"#^��]�bE����8�����P�:�������'�����2�y�-�/�!�P���0���E�e�xBL��He�?z۴��O��e��ۼ���e�bp��p��Gx��
���>�3
]��h�I���5��1�n�=0=&7�,?d�M�T�L����c#N�r\�u�}�-5���|o����/�"����y�/��Ճ���t�	�y��H�d��Z8 �.�h��͋-�#�cG���#q�W\����碮��J�#�.իw���+F�I�*5��5���({77� ު�y���W{t������/��h��SkE�%�ҡ������ԏ���_8B��t=��ۢ���ө{�%U��^x)�.R���S���h����7jъ
��:��}�qMk�^�����WMo�4�N����}/���A8
GP���mm��<������")��u�x���k�Iy�V�U�~f���p;�)����s�BL�R�K��Ae�fT������wm3�z�6kv���Uw-YY���k.T�J�>�J�$Ͽ�(��F=J.�5}zRM�,5�(�tܷO�R^�}rabX�0~�
#�4�0ᵟ�'&�GY�T�2��'U&F����21b�y;�����݁� cf��H[TT�Q���>dη/:�׋
�3J_t�7�1��3���o�����셗"����#7�����iqu��F\G�e�s��L���I1"rs��LG�FIgf�:Ԍ�󮗊H�5��/�r<��2��OC?=pˏ���P��]�?���O��t�ѣw�G����3ֽ��G��]H�{QLπ=���X�������w�p���4rz�\���,Q�L7����"k���劦�W��.���.ӊ�3W���g��o� �E���c�Z�C_'�� �`�m���u0����C8��a���g�5�@,�z}	~�o��=�`�hX�y6�WڔuA���y6����|��~p�����b�yG뤹����洀3�90����&�z��b;	���[=)��YL��~P>F��N�^��ڰ��s���V��""ĽT�RE��m�6c��ۂ��E�� 9����ޤﲫ�����؆����14T�/�#����C�FCH���4�����4�D�d��kI]�E>��d��>�)����_�}(�%�z3�%�\����z�!%Bd����.�z�!���Wof�L�������'��GM)��?���Yo�u�Nd����e��7���L���7����+I�%Օ����˂�c?��<��L��B�#�G�d6�7�{v���{S�+"${S�+�:��ĺ�s�^Ez�co�u{�|����20�z�Ϸ�K\����Y�[o8� �zs�#7k��['ǝ�䑡m-�������b�]�������Qs?��-w{7��^�{yQ�i|�i-�os>lQ�]o.���l·ޥ|�֛��҉6�C�p�O[j�{��yH�-��ږ����}�aq���އz=�����U�� ��U�/+�yN��q�~A����C�
j��5���PL����O��}��[FYE4�����X�"��d������_ʈ-�w)#��ҥ��"T]ʈ~5{�Ù��5G�����8z�K�#�
9`�҅��f^G��X��,G2y���'�Gٷ#�:�X\u(�����ZjR�ckB�����48,I�����p=N�#
���48"\�2��'���R+���?���K���g�|�t<��݁}�g��Ag�����5�0Jy=��$������H�v�>��@3>mQ�s`�K�;ʼKE�q̺��{���9���s�-F��#!��9�љ8�ჩ�{�y�HG3��B¤�Λ��}��tf�1R�����E�-��� 8F>`T�*���� ��W���@F��B����B�� ��׻� y=k���~�p����=k��O[e���V��D}_"��������H�.{���;&<my��I&��wL2�숽c��zߪj���XYd�&V+Zd��u���ޤww0��!&��bBi(�.�W��0�fm;�/�.�z���a������b�]�����#���M�Co��
��Yu�s��Y��Ԭ:�jA�{��Y켡�x^rA�Fҽ�ₛ7T�C+���XBԪ&�y�wU8�s-���<�t��l�ܞx�."SW��K>��X��Fh���'=�?c�}>�����>��k[G~�1�m=TA
�m��o� ݐf���ѯ��>�N�Nz��PLH`�[$D�Ul���(�D��9��-��H�-�D
���5R@������jD��*�3�pS_�в�d�-�M�U�����BG׋P�1L�Z\��S��Ԫ�e���N�7O���K�k��`�hB�c:zIꈭ�    ��t�Ғ$�P�t�R<�S�zL�D@:f܀3 ̋'��@0����A7!3 �G`lFi
	�gf�bŉ�8;������	��,��N@�8��N@�d}pvFj:ʀR:5{���>�&C�2j�3j2I���f�vR���ډj���Njv0R;Q����ډ��9�� ���>��O��S���sj�Ǉ���Kg�d^�,=��_GI��Y蟜�M��yI��Yl2�|4�[���L�W��+]*���Wd�	�tk;���f�Qo��>�d�����.UC�������\r�-mzQ&[��|4{��9EY�Mݹ�ba[1�,�����f����u�T�e���Ĝ7��O�sS_��ln*�K�o����o�J����d���|Ǹ��PC��L6|
�3�2��L'�E�l�P��NX���	]{Ter;�(�d~3���T~Ư�,�J�3�2���xU&�s����=�q�	B������@���H��N��2��A��*:T���E���Lυח�`rF}&�����xsH?bk,�Ml��:!{�C��[Th6�Y �&%�F�J4��,C���~4�/�g�1l[b����<h{��A?*n��81��7Tg�r.8{��.����9tbѺ{���8�އ�(�|�ާ��@������
���d�އ���N��7�8���7�}�����,7�z���7l3�I_o�ˈ�zCmQ|Yo�NU��֛N�{n�uȉG�.����5�zӋ������+�aoz�3'{3�xI�]�1<���7Lh��7]��=G@.�R����tj��
����a���褿����9mgC�0��3!�����ܖ�&�Z��ЇԊ���
����aԽ�V|���P��o��Vg���sof����.�{34��w�?�����S����*��FN�3ٙ/�-μh�/�{zl�V�,�dr������a@5�������R�1`|˖X/E��<�Rƶ�
_�̽�n B����������w�`�x��J��[�sk(ko(4������8��m�C<��5E�0�m,p�b�=����p�Ĵ/Ĭ����&�0���J��V
��g�;����"s[T6짤S�0M��ޱ%��Tj+d%Al+���.���\�8�>&�U�__�����ʳ>=5��+���wTB�gͯ�5�{c�-�ypA�l�̓��t��<�hw���γ(|�����P^!	�>��aOO�c�r@���t�;(�V�q3vm�Zg�5�)�!6&\A�����ް���{Îk��7v������,7�ܠ�}���
m����l�^�����S{�`�}+�b��R$|̽���bY�>�u���އ�,���7ۋV��(���/J~ ����FS�*Ƕl�y�]N�)�S�5�+; ��4 M�"���&���2�n�4�;�M�Рl4TCS��h��fo�iu��L��N��M���jG4���v@S54�ڎh����F���ql��چx�q�ځev��� 3��gNjO+�;�L���m+�M�픡N�l�
*%�v��Ɉ�jp{2b3��2�T�	s(�@�Ԋ����Bh��`2^;�-�.���I�M�kX9Y���k-3��7m���o��xl���Am}A�Cc�sk���;#��#�8S��7ǐx	:�bl�8�p�'�{@5��H��w��)��n8� �I�o�t#��Jo�ЩΌࡪ�'=��&��Mc��x|�=jb�Ik��	�=flb����4$w������]3U�܇�)=��Å����.}�!7�p�C����ۋ3U6�){i����n��|��������^�),d�wW�l{a���6�vA�U���^�!h����L�K�)����p.�d�C�[�to̫�$���	-;���$0���@��Ap���a�rh�!U	}��VMch�͎�J �MיQ ;^�y=!;%Bgֹ�<h]�C��W�Z�Ci0{a�CY�z��JOmu�3��T6���	���Ӹv8#�ƥ��?�Z�~��%��xq��<���-��|�2SM���.��;��ejfV/;�bV�w��Rڜf��S���8r:忙���9����s��g��r�8���DU�Z�1i1������~+����~���tv��=�b���H&zO��O �jr���`{�'�L4�vd�Hf�X�=8��D!'�L�:<�s �x{��y�L΁;������ʁOts�.�A�L2���E��Yu���yU�d�E�3����D4訥��n�Ҭ�D88$�1��lU�����������,΃+5�8�NJc���q�у�Y��vϢ4���p6�A|Z��M9p��ٔw�Ď��hF��|���j|�{�5�m����>�<��G����'�n�b-�ǿn�=jZM<�yk������Gӿ���7c�5���WA~����W�P���S�&dM�'>�T-p���R�d@���kj�� ��k�֊��O\S��O\�M�ܶv����-�������x���y�o���0"l4�T�A����Nh�DM�V�J��J����-4U�Qxr��&�H��;h���zM���mN�_��^>m�MM.���O��I����n|ښG5��N7I���F{�z������>kR=�\5��35���]j���>�d�x{�[5���G�~�
iј{7��Z+��f^����V7�����}�bĽ�o%��jĽ��yě<����F�.Ġb���/o*�E����ʸ{��S����u�n��1��_��4V^�"$�6^������v�]��F8�H�n�U{�AȊ� ��׿	��ܽ�U��M���ZS��?ߵ~��_��D�����������B�Fϕ�6n9���I��j*����M{>գ+��U�h1�����$~9�OT(_��%��k�n��`��{�f�we�׷:s�vD#N�F��N߫���#�?DD6���u�cܽ����kV��5�h2�އ�����`n�r�����c%��]�K��+��cy^*�ޑ7?�govƖz�d���-v!j7��7�K5a�����ͽ��!o������7KC��n�����-2�m=�:Di�I��̳�C���5AO[��=�BL}��!��{�}���+���7�b��RM���솒���ߺ�Нq�����:ty��]�.oZ����M�x�#�,��=J�,⁸7L� �{Ð=��76�.���Wޛ��l���~��"v��7������#4kb���{{�#���Uh�{�Խ�ٮ5דS��fz�_�l�֋��xA���M���p����G�����<F�')\�P'�:�~����^�D=kMh��{;�=_*\���'���T���N���\����B��פ���e&�[>�e���h��ڤ3	Ɍ*\��L�������l�M�"k3���M�&�]�����|2�q22��V�J��������雱�6V�|�B�3�����R�D^3��������"��-�KXx�!��iA�|[iϪOߴ��)�i��ҿiM4�ſjM<�^��m��a4��58F9��H|	�Ֆŗ��9f蟶�w�&n4Õ�,vǕ�_��w昉ٚޙ� �T��ٚ��c3�)����w���{�_�~љ�Ӻ
Ԉ6��O/������l5K/�ē�cEIC�{�U�<�{�v���� ���}��Sִ��T;�_��H�3�oZ�]O�M�ZLjIG�J̫�/^�y����0�O�Ϋ0�e�?u��Iz��W�6/�d�Y����FE��ME'^aBE��=��.��3��E���5^v�JL�����_v���}���Ҙ�!��F�\��N!���Ƞ���2k�G���Z���{�e����9ٱċ/�m�2�]6�<^x�V˅w��Snl��pB�)O��U6(�Y�b5�O�4��x��5&*��=G��U����z�{<Щ܃�JE�숕���L"F��_U 1���)����v~��!�ڄ��Ө6a��rUL�	�k��<�b�^[��u� |u�N^Ob��w�U&�#�*;���    �w}y��Z��'�K���NԺ}`�\�k��K5cW����BN3<_���z�;��Kф����R��1�(��g�I�K�,�ӑ�88��b4���JkWb4���D'F����*O@��+��	S=�0��0�'O ����|& f�\���C�5z��K�4�R��})��K��f;�zy�]�<ܙ��h|;�DW�T���g�eQ$��a�¥�:��L���C��v�����|[y�/����/�,y6���S�<���&�'�]���϶P��xKH\'��tK�v���.~�B~����:���{,�1*߇7������ucy� C'���+�F�6A$�_�+�`����/�?�ݪ��N/��aJ��[SK��qT�y��B����Mp0?2�C�}U̦5��|�`Vʻ�
f��{�`6��$`�͂������7f���BL�.�1n�j죪��4�N0*14�NPM0X�t�9��At�rh��&xi�i!}//�I�bK�t��c�8�;�J�zL�e��ot��r���o��Ol�s�}�,X��<z��Y��9��=|�[�C�ƏGҼCg1��Ӳ��X�>�`�ͷ���4{���W�^K#O�^K�����N�e�Ծf)�t������.�(5yw���7�;)�t�����,������|ÿqXQ�*��ɷ�NV@�.��M���t�?9��}�d4u����� l�.oA��}���
�x=��h|�W3���X����,�o�w�2*����n���8�������^����sg��1��u'�!���t�"�t�>�����~����L���y!���z`�WHuT�>U�]�*�⼍!:�⠢�*ЩK�k$x���Γ���q�-F���c���v* �@���u�,mUu��i���Ё����:�#Ɩ����a����/	��m�Abv� �mv>g����o������"��u~`
W#>�)�1M�-n�
Z�`]�u)Xnq�T���On�� �}�֊4���al/>B^<"}� ��e���L��6���0328���16aQ�B�E��l��<��P��#i�`H���@0���L9�y�#��J����J*�"�	�w��5M��Xe|O�m����
>r˽�]I���z��3��<����'mT�J&����oiJw̒���5�QKe��5N�\�Du���㑮?l�'�?l��SU�>O�}�>O�'}�Sr���js�ģ�q6��m�*���1��bT�RO~,�t�-���a�!�߼H�tz�~�.���pq���7c���·S��݁Q�(�ꝏ�ڔ��)�oX��靲��	�a�'������"��Ì�F}oD����������-���,+>��z �|�޹��1��}���k�[9�f���]������|��Mϓ2 �29	^���>9�(p��K� ��-0��K��{�nnڍ7���顿O�h72DƋ���
Sl;�6r�G��0�b��f��T0�ٵ'?�(sû�*��H_t<����eV�b�wÐZ���Cj51��Z7�R���OFS�~����{���_�7�㫡��ꀆ����=1�W"��TE=0MF"��F�R���?Ԩu��Ճ��9V}xy�U���(�f� �q���q{���;O[�r�b�~G5�`v^��
,������|�bL��o�y�;�Z�\��Q���;��r�b���)v�)�c�dE?����
�ww��.tf�Y~�f�W�)>���2���sH�ش�h���o"�1����7�ӊ��x�a��?�b�W�����+R�x�`�">��Z�i���P�b;vU,��d��W�݀ۨb��0Ҽ�}����7/�Q���(�hķ��L7��������D�Ù��*��U�ဦ��O֩��s��T��?�%�L����Px���uP{�{!_��'�zD!&��ʸ���O��݄��'/�X�C��.���3��18�H�cP�~g���H�^�M>�yT�+�o�M���;���xm�� �r�������*�Z�6u����S�P� �F���:� 1�9�P�D8�F�F�[�9}=* ��vM+����|�g�߻�ba�6)�����(`��Ű�,���]�
�Aݻ�^lS��]�
ʫ-`��D�`�?+��Qk���w��tX.���ix�ڸ}��Žߓ�{���:޶>n\ům�� ���m[!5���T��(/&�k,Ό��EA�-��z&`�����c�W䲉��p6�|7S�l���|=*[&��>����U���w�X|^��_F��q㰎qmy�8�S��<jT���-����B�-�ha`��������[��ͺ�D�'Ưm��ķ&hb]䕚���C�S��1t�Ֆ#��I�.�ЫQ8y}OA@�ڳ��"�l�S�)hه��l{ �'$�$��݇_�|7q�M)@ỉ�O��|w<m��@��n�*e�U��;rc�U�ڢ�[��=�j�VP����J��7���w��z4�{sW2��s�d��&�%�p���	������c�P�n���~��7�`W�)�n��E��[�m�Qפ\��F����+��͏Ht��M�^ A<r�pKDb���"1��|Ϗx:�N�gG�1����om��4��׷:k]`����ӜNc�
�~ UiOȲH{}��0�f-��N�U:�ѡ�ވ�ƚ�U��T�!.��3�Ty�9^@؛�ݹ�uͳU��c7{#���]�9G���М���H@���������ʆ �u�����n���I\��kmZ
����X"ʉ �u�@:sؒ:��8j)����%� vd@h0�p��)�L�-��A4��Mu`��"�����4��:o:]`��逴��X
��z+$3�e=]L@���0�.��<2�г��K�#�8[�}3���7�W"u��7�W�~D��f��8�p��t�¼,�ތ,Y}p��Zj\����'L���"�ȅJ�:D *y��r)��-t��G�v���\��_/�9YICo"G]�c.���Տ������rk�  O���L��[K��@�nmB8�%S�#��K=9o��D��e:j����-�'�2����Ё7����|�#n�ï-���j��C���I�N����C��6<��n��ԃ?v �&H�=��w�G����@�n9;�/Le��n���4z�}���%������2ۢ�6J�����©A� w���7 n���w3��nQ
E�H���݇[�e\y�q��wR$F����4_��w��ʺ�%�
��7��]���%1�[e��ꄇ-�vSe�i5�6�۶�V1|�bX]�4/kS�`2ޜ،��M���@ƛ;<��@ƛ���!d&�=So�m���Fυd��4� ǝ�_s���H�`�Mǝ�n������ ���TP�fT�U�Ż�tPL���R�x��t���W�@>����f�[���t�ˢ�7�,_�B��e���e��C!Vc��C�����S}w��C��SG��xé�6L���tf{�O�(=��w~���ݻ�7U�M��d/YsC�(��Њ{��<���{}���]'r��<��-�$xxs�[Fȳh���@@ě��r�Ρ���&�4�V���&sZ{��w)�e⻝v��K����7v;ޒ��7v;Z��:m�ӻ�
�-	8xw}&�v���p-��
��n"�R�p�)E��37����0Ȥ�C��ʵV�#�r����F�i���V�i�@Ļ�c��'����e
Xx7},Z���˥���A��ͅ��~����"`������ߓ��Q٩I�H���
���������d�&�v���v���Ej���Q��@����������a���߀v�!��nzn�m������曆��� ����_T;*�eݭ��a:�2�aK��[��~g���;+��Jc]=NW��y�F@��j�1�r��Q�� "`�MQ��\�49j�&��^ ����;`����.�rm��N"��    �P ���V����yF�$x�A���tG����T�^g��+�{Tⴚ)�����
ggP���9 ��:�L���c	d��
����*p�n���V���E�8j7k^م
�ݬn��le�B9!�Sz�a�o�Q��N.�,���^g)���x��t��[�`�^b����J5t�^]�^l��k+ԕ�Y���%�(/���L��
+M������%BT��KHyZ ��nV�����R�T+��K,tB�о{u���W��M��Uc�&S����f`)����f��r}sƽ6�vs�gT������G?;n��a�J7�o�YkG�E>_�ޙ!��,�d]��~9�:��J��=��tO@�~:�O��ʌ0�v�8� �g\0��-M�B%��*|�g���Ǘ{#�� �ͪ׈��v3���v��]���˽U��Cτɀn7�^q9q9�:>��V'�vóg������\��p��u/�s�9�Ϭ-�g7fa�4���3�ӸF��u��7魧�;`C>��/1�i�}�!O9�+�-6�t�p��t��������T�~3D �F�g˭�\�lx?sTO�
���N\%
�ߣ
g@C���V�x[
���f��nX@=�8���[���zߊ�f7CNK7�BN�[\��fxiq/����q4�uJw󡨣��.xv3���T�o^�?]�2�tZz�\e��t4w��҅�����8lɳ��@{����"��C�Y�}��b9�|�gU�,�����q�������\��v�{�dys��"�,�K)��-ىˡ��n�n8������R���==lO��x���t]CW�7yi���,8%��av�<��f{:t!�#@���lN`~;u�q���^/����
o�[��.�V��~����W�GțS��!g�2 �R�����8d�wrd�3�hq�b��1:���1 >�N�7�g��ߦ>�g���6��g��%;�֏c�O��q����j��RMsy~;�Ms�%��V����Z��u�a*�V���Ǳ����u����/ue��FΉ�R��FΉfO��FΉg$��ķå���:6���lV��ђ��C��>l�;��>l�ۯ#oc#�vC�]c#M���F�������i���F���N.4���..F���\�������rEН��:r�9�;Zv�n���c�z�<�#�F�9QQ����}W�栅��ħfv2�W7f�/xn'��p(@���"9��wp:�o���~��*]���7�T;�X�-���--�B�Il��M�{Z�1�F�M-�������N.Ǌ������ՍZK
U6j;���B}�]�tN �;�0t�c�N�8yp���ާ�L(�.���nǾY���n4�л�r���FSiR�}-Tu�u/�:Ѯqw[�H:wq���݃��M����Sq���7���ľ��q7W%4��nr�₸��o��l�ex�z�%������2�w���~z��Rp���лPM�7J.��1(�=�P�~y?��_��<'�=�P)안�H��a
�R�A`�ND�OD7�K
u�:���F?ˋ�S���b}�q�7�'��@-E�}r���}*Ǯ�х��y���V��!��3�D�13�	�#�>t����q�����_7�1Ψ�=0��\a��8j2������7����!\h��bj6�h��8vy�?q����Y&��d-эK�U�˃v\�W\���c���;�4塲�=<�q�R�B�����q�É�S��q]&�z��1�Jz��+����^v�\�HE�T�W�FG��yJz5;����W�1�^�N����T�Of�T�_q��N���1�GT�\���X��r��B�h­h��M7<&�n�h�pp�㎐
J��Y�n\?f���q�8��Qw#B�mu71"X� ��&F����0�n�Dhx�F�M�y���(:N�`��D����/ŕ0� ۂQ�-�ނ���t�
_�"�R�(;_��>3H�zB���<T���AH�<���U�_װlmu�2_W��QR��!��ٲ�%��\�����n�� �m^]�`�u��2p�n�k�����.yt�VV�9:超��s����uq�������[��9�����m�s������^T��w����{�W���u�^s�~� unNmf|>���cS�=���	�sS�3c�9d%y_��M�r�2��/���k�!ϝ鹷��N����C7=�l�y.�u�I��A.ċ|t�yZ` �n��S֑P	D�[8<��\��������9�XFWP�rv=-v�D�<m��1��p�; �}��ļ����Xl�hh�g� ���gc�+�kuӵ�� ����ʷ�*L���{Z�_���{Z�U�,G�1/�w���xW��"�Z{w{R�%v_������O��Q�����n:䝶ݻ�U<h��a��7ۤ�o��*_�[tѨ|�M� ����yS�%4(�~/����'�m�����F��q6������y|�� ��e~ѣ�U��yU����	��f-�*���c�s j�~���C���F��y`Ry��d��xT*&��_ػ�(4a�c�l�M��FWk-�?N�uK��`�]�P
Wx��O:��A77
��whk����LrОn;�A��T�jr��Jwͮ�B�P��|k�{�����	��p�SV�V/�2�j3�5Ƚ�W?T>���Ȍ�q�Z@M�s��bb�!7�B��'w��E!�;����$�zDyt�0N��T�i��r���Uy{��Y�-.�T�*v?��xW�������J/��R��;���8�����J�}O�?Z{�} �T��i�|��wO�*_���<"P����\����K��GhQ�(?�}�Y*��Vn1˳.ٯ]!�/�Q��YI���z���"�628��d����T|Ȣ�vHŷ�nW��Bv�C?yࠬ X�(�D�
,����7B�U·�]�.|�8�N�����P~��n�������.t@�bDihys���r{�t�F�P{�vMar�]�����9ܼi�;sV{(}n��u[a��/���fkd<�R����������?(��!_�tTNؑ@Л�U�l��é!v�SC��W��c(�BVjf'��0h|����*����68i9�PT��j�:A�Ol*���'3�zӿOFf�����M�B~���O<�����7��S�9(٥̴���dc��dWPZ�t	�%G3$�	�X�T|*k�0�A�1�SS�T�"5z��&nq$D��z��C굟JZ�^3$GCt���E�Oڝ��d��o:���
����_������O��plŀ��FS��*�Vs\�O{T����
L�[^Y���+�n����E�Y?���˟#����ɋ��; �B��{7��Q��h(+�p!�Bu�d�H(��=��g7Iش�e=�"��sʊ��6�SV9�����N�v`�\�����6�wJ��+�*��M��gŨY*8���wA�{n+�?y*@�U�J/ŧYt������H�/��x�b��*��,�\3���\3�B����o�\5�~���Cat�f_�ѕ�v�GWbP���./ʨ��,W����������g���J-0ѓ�t��&s�8�U���E��m~�8�)��Q���*�OH�T�3�vGE��H�U{Ȇt�g�t��P4J�ѣJD�g�c���6ӣNT�� 曄���r�P�fn�r/�i���O����Q*��8 �˄a�7�NX��F�	+�hot�@��7�+1Fot��2�vGG���[�����o�M>��-�.�gE���n�B��j�o
�V��:D[D�����ZP$��Ԉ�H���َ�~���uӅ��qwz:{�ﮮ�f��X��F��RSX���_����XJ��>x�k)��ak߀Ά��Z�:~��������/M_�QLN�W'�?`�7�v1�`.���v�/�盳�o�T�þ�������͓ğ�'�����|��J�M'^�j�v���m���ǻ�JX�����i�6����ږ�����#��m��g���w�o0    �&���ո�/�ߜ�*�5�<{�%8fx��M��&h�y��>��'�`	u��,�aD��1�yJ�y\�h3N�x�� ~�x ������WK����)?���f�/��-�<�p<:�z� O�Bz����4�L}�*�����������H4�gі�+��g#�?�^;$��I�v�&�L��x^�Ş�CpR,�d�'�b���+��৔b�����{X�s�QϝmP<jh,�l�	@�M���l6YR�p2�7���=W�3x��=5�CF�i	}�|�����W��
�����
�`�PN�e+�����_��A�+_~� wp*Pg!�4^�� ��_���Ek�r�F����o����3��fE8�T�MM�r��4C+N����f���O�ߜ**��� ���O��2�Ԙ�����̰f�O����x_�ǀE����*��<�=Pы�E�p�Lg��^�4*g']���`'��줴������<�Sh�����z�E��$��v#p2�cUs�2i�����;�8��!���凃�:��|��7��o0=GD�[��(����w/��`�!�6���E�]�R�O��n-GᄟN��Dñ"]�Dܪ��:��\d����s��_�6ʨ@at��0
� ��>2}�L���;6X�c"
���Dtt�p��v�����2���U坼�S;.���2[�]Ū�J{1"hwڲ��BzҚ�����7�9�TU,i��Ǌ�9*j��c�D�]mFY!�*'d=6̬ܥ��GE����ҙ�(Y-{0*�[3�[y�
s	dT��\§3Q�0��F$W�lW2*��Tf2]#Q\�G�vū�FJ�+eA¦k��˦T�ϝ+'�3�3���C&��t���l&�h$΍�8n)����%ӂQGZ�T�q���-�OGGI��1���/��hc����8�����8��N���0��s,׹t��ʟ���O>�4�M���~N͚�^b��BM'�[���Ccۨn�����(k�ŒM4f�FZ,�fm��V��( ��j9W����D�j�S[md˶�N��n�[��:���C�Y��[&� dWi���J�)��UZ]���iWy�����Ql�{��08�ߦ�"��ܝ��*L�0�u^�tN�E�@
91�!�b��޴�*��u���*�W�sR�D��I��T�Pԋ�sNrN�ct�EI�;���}k�3�^%.t�^)"�Y��a�^&�o�t��o��A�6N�EDvmb:�܌�s����mtGF=�A6Qy����Ǵ�Տ�,�ŦᢜOU�)��F�c���&r��J�P�M7sD����Uζ)#g��a�v�=�On�C�[��y����*���9\@��a�(�/G�J6�
/9f����)ˌX^����5"J�e�-3��4&�(Ӹ��i��_����ʥTe"J�X�㡂 8��e e��Qv�t�R����%\�����Y������2�|�#K���<�b'=&�eA_
��@���o,����	D�÷7�[��s �Ħ�>�j��{���A��1t@��7U$�C�w#�_d�!�9�C���Ո�T".�Fl���A�s�4ș��%2x�k���rUC�w���dd#C^��}r��Qi��^��CZ�_02�P�GCX;�P��R���I�T��l�����h�����*�v8������H/a�$�-������N�����#���m���	Y�8�;��e_l�R�N5�B�3����*����R!0Qch6�9 "j�����8��N@��GT�f�D��$�\t�BI�f���U䴅���i%�0S̲��1~�l�D�e�ɍh���w���6��6�o*�Ms�5̡)B<m���9T��L�F�i~��w#K�eI��"��i��-+����0�=���݊1(w�Ȳ������*�P�u5�X�H���pY�R~==��r'�5��id,���t2�w7F��^��T��u�cKd�����jr[d��#���D��̡�c��D|?鵽�����T���M���<��zg�~*��r�Y*�Pnt!�)�C�?���%�	܌I��;G��S����Zx�饺��v�V�%앭A�+[��l����$�v�F��/m{�m�@�ҝ�P�烙�X��8�ۋ�W�����H�S���X�q*���cUn�:b@�k�{�=cVnDqg�=�*J�rg�`i��f��ƭ�^
ğl��)pA�q�7�i�cX���Si��Qy�`�S��~{(Wנ�l˝��M,�ZG�� ��`p�A��=L�~�p��@�[�ގ�^�A�V��V޽;�f�I�o���7{�0m�C�a��	0Da �w���;�h�^� %mGCLԌ�v4D�~�s�����{fN�_��Ɲ`�zψa����=#�0�z���H�{>q�n8LgrD�J�����/���q{`\�`��(�ze��h�T.�{9i ��	�R	�n!Jzb#dn��v-
=��^p{���`(t{�Ƿb@��Q2��@�d�B������	�v�>�l�@�0�@�|��>b��vWJpD-`6�KƼ+`:�0�x@!ys3s:��.�8��|�F�Y�!�����Qt~v���-�hn���Q%^H�C����g��v�[��v D'0��3/Y���6�y�0��6�*g��i��M�4u�d��YF���=tF���jΠ�uc,͝��1��A/��Qeg��=��w[(���:��P���	Q���<w>�0���j|SUs��i�tu�Us�ݹ����Ipc\͝ڼ� �����l�M�>9���2������C'���F/��6wn��Gss��l��1�8�[nf���U�q������5��[�E-��A��E�Q8��##e�Ĺ���| �6E�qn/e
䓒�ɤeL���dh�a�?J\�Qr�"R2�B����й�T�QF#�?��	O����X�c��C��h|~�h�յ{}z���J®�s�d�rV�m<;��5^,�ɾ/�����	���3���M ��C�n��šO�ʉ�5����ܵ Dն4m�BJ�f�����lI�.+A�Y|pY�a�����l�Q����E'�(�8�~[�:�f<�<�^�7*�4ˏq�stX��H�s��"u���T>)�e��l���4�͕�Y~��r2�V�Ҧ��
�a�ύ��0���Ɋ��qL�)��CJC�������8z7��C��$�C�l~Q�TJѻʹ�C��1�m���w~cCE^��~?^�g˾-������\�u�;,��=9��D}�E���K�\ �\�B�FpOu��aU� w%�=����g�ˇ�;B�Ԑ|�f+@�qh�Q��8��\��)�ۇ�w���DQH��aYY�>rW�����Jǿ�(8�_\��Co �O��>1�z���2�{!�<�$6b��58��!�N\F�pB�6n��M>MN�b6�rB�>�69=;���׳��-<zx��U
���W�K��ϳ�D/�+�vH	w������L���t�~� f܌�+,ZL>`�>�~д�E?�pAd|��?~��%/B'���"l��}�F#���.e4�.`��v����b�H.6�m��[��<��l����~<ļ%� �C�"�1}���j�6�d��;��pG���~�v���m/?Gc�~q/@*4�Sc�~�e5��<��y�t�P�j#�fz9T=�1����k�rp��%7l��XB^M�F#�T���#g��H?|�&e0�
��%�|_���C��۷���Ǿ���[��^�ڵM��H�؍c��s�J���4lr�D�a�"�?>�ܿ�Z:�_�C<��]�2n��s*2rib7��p�B@>�3r��f ~�����^11��@0����~')q�\v�z���tS2l���4)@4�6��"�L���/y���V*��y=��tF�u �t�*�I:X�Y��U:�MP�E�6�����i�2��� �N�4/����%���]P/ئ�x�_
T3i��D��d�����3�量    EЀ�N?�WF&��x����4N���tq��kU���X�G�O�]e	ȧ�tY�KA>�^D&����zc%��4_�u�����QPӻ~�GE��1���J���Q��(qD��\Dt�qw�Ϯ8"*b���8	L���v.�D��1C�C��L������:��`بӝ₏:CQ1��:�$�g�`��Ԧ�Ǘ��p�82��.�#�]�����v�^ ��Q��Aqi	��ԉR�߇��:A�ʒ�����I�L��y��F�h)�(>aQ��(������8��|�ID���.B�Z������Ny�-�]e��[o3�2�7��z�%�Qt���{��=2��Gٵ�\�af���xe�����1�<�G'tѣ(>��:�r����C�z1!��/?�vBR9�Ny�32�Ջ	9uP��`X����+9lĒ1�K���!9l�� I.�T��l�R�w+���/��~^��'wR���Fh�d'�>�[�ɞ��(�뽞8B��z�<�H�G&�N'��i	&l��L`�ed�a��U]s��f1/��KB5ݖ�K�>F����l���;�Њc�7����L���	)5p�h�l��eˉ^ߵ�[����B�E�l��o�wQ���+�-�N퉶"j��)���z@,e�yj5�	@�v�� z�r{�Z�^e�=^-z�q{Z��<�Aa^+��E6�������D��"�P��zaE:,��\����h�u8~��k����Ԍ��y�P�������t���qe^����G��?|I\d�7��{��)�D���C�c0�Q�iip�k����E��.f2����O��#z]dc�:��4n��x�=�^�c�㨈��`�Q0��1��Q1�GG���O-�8�=�O-���=���@E�b�����[F��@h�������:����U�u��\����)˶a�������:a�t{��k�i�F�i�"R����(���U�g��(��+@���es�m���8��¬=���G���>��Qr����&)�-���һMRh�R�$���$���qH�����`�^2b_?��,^�4�~��Iw����M{�u%��qDT�����~�p��6m�S,��:�>`�=��n���D�(_cC�dG�/"n���9~�`+�`S���p('>�7��!J�B�Щ=B��=B���!$�n���{�bsW�J�^)�G|�R�r��(�;�ּTį�7��R<�l��ֿx�ٚ������3��2�b�6�f�1�a�5�e��LH(0��V��=���i��&��?0Wk�Qa�U�1W��H�LF݌�z|�W��}Uw�΢���}�K7PW���K�4.c`` ��VY�űʺ/�U�ma�Iu[W��۟��r��냪Z�T��u۳W3_S�O���o��I�j�@R�V��~: �˩f�ԓ/������մ׌�z��ΚqS�5��BHw�>5۵7�Z3v��!�nFN=����ة��9�|]o��')�d|3~�7S����z�/T�qSO�P����)�.[�k��k�:�k��e�8t�l�)��]�s�R��|7�5����GlòM�G�8H}7�㣨����h��m��R�6G{)oorܐrܐ]rܐ	9���C�{����f:�B���'��4M�gR��?�h��M�	���C:h�ï�b¯�U255�J�f-��T:wtEk�t���s���Bm0850J�>�y	�r�겥�MK�!u��Ү�LhW]&�j��0d�v:d�r��"Yet� �k ���1�� ��3N]��� O��]��C(t��?����J�+�5c��ykd?=B6)z�l�U�lS����ħ2}Ј5�BG�'�P@�a���a�U*�7򫸕k`�Ιa��5[�6^Mr�;ܨ���:�xgTpA�}�\t*�l	L���%�>j�@#;ؠx��!׺c��ں�w�G�=>_YiF=>�z�;�`�Ѣ�w�OvG�˹�v��������"x���Zw�Aw?6�. �
���\�4c~�ߎ�f������dȸ�w�N��{p��<�&�r�9m�6����C�g7*K�$n,σ�0�U��E��Ƿ:i���E~���H���$,mͲX*[�,�;����^L4#y��e��I����B�c��UVI����O%4�-W���a�^��o��G�@Ȧ�T2͘�'�A�+��-J��ac�kռ~��fw�CV��.#�lg���/7�{����q��H�݇L�'3G��R���|T�;���������)yӌ�y~85' R'0ѿ��Q�� d�of��4/��y�X饪�P��r�5#h^����5p����x�nݿ�NW3<���|9sl~�D��Q��3/��;���_�,rY6j3S63��mHW&�M�֩�=Y!^��H�2���]���%�Iд���
v��᠄)Ľ<�%�f�K�2M�e���f�̓o���w�p(��r��r֔I�?���pT�L�Ngi�����'����~���ur<�1��������N��>6�3Y3������|���p8�f%k����4G$�J�;"�k�) �8�&=�p�k�����8���͑O7���TKY� N:�=�Q�a�I'�[�֘�ݶ`s<�����t3��d����l`�Wn�������౸l�+�/��ʁ��ŕ��q� d��
[�"��ߒϥ�|.eB�,5�,�5�O�:�l�)4d��"d�� m:�<<�d����[~*��Ʀjc^K�!U݇���J�d!T���� >3�!��q7rE~DY�S�H�CW�C��� ��r��q����u\*}������T����jߒN{��MWO�K"﫧�E�z�{]���.�絑��p`��,&��8�߷C�$n�6+vH��P���λ6J�o�!K�N�:��6J�rz��{ ���O�ƌF�J�b�S���܆���&�6=nIi�f<l��8�Vj=��7_+5j\��1i��� 8����h��2�e�����!J�W�t���3�j#�6�EW�&��u�i��W�t�kZ�g�y���_��峉�U�D��t"�b��:]1�L���@�>�f<�N}��^@uvf����x��??����b:�O�-L��745(��ߠ Ԣ�e�ȃ�*6
BuT���Pu��9���ΦM�pA]��0d=cf���P|��M1y^A�{:���ʲ�0'�����mNN�-��tS�nf>�h����-�b��R{���l/�G�F���Q#w�#3wp�⨶A�d����UA��E�A��ϼ)��$�7�P�<b3r��͸ء���[*�3�ySx�Z�7�Qg��W��#�W�b��U:�q�㦋��7�u��l�ؐ��M�����J&� ��d�r�|�զ��~5J��o��-k��x�1Vqo�����N6=zL��ؑ;�o����nV@���|I�ۼ�,�F�|��� �ӹo^ZF��_MQ�c潹�l�Л��`=�~np��y���fy�;ﯶ�IB�
u�ނ ӹ K��W��}��B&�-��vݏ
��HK��L��-aV��D�����U� �
8D=!'�|�t���|�5>9�k�E�̮�}��
HD���.ӑZj�XH��;ޖ#"��7�fA����޵<8���׳|=۟V#2�D�i��Ո��`�jDF�0޸�[�����h���D7�
�:>���IyX0$�:��QM��7�P$G U�d�@�ߤrmI�djT�X�~3�*w��A��U�
n���[�2v��>ެA��Tjd:"a��}C�_6�[��NZ����䴢��YF�)��jhh�=犻%���}�*���J�F<��Nj e��J��Nv<����67;H�iaU%���M\�Ԡ�n+Z[�#ֲ���ٶ��q�5��,*�"fZ^�!�n��W
&-fqOH�|�.ɼ�Sz��䅝���y� ���;4+��wHo06/��8l* ��Լ
�|�����������_    {��K9��������]B�7�FǍ����5(w5���:!�4��2�	�,Z]��j�:�3d�,"d��2n��.���sqŔRl��3�Ε�ϙ�c3�_^�flƓ(�a���cf��n�z<���!j���f��X·��1·�!�[�����÷�#.��Sׂ�K�/�f�!����t����?6�������w�m��C-�E��C�W�/��R�Ռ��^�<���i#0��t�'�$�u՞�A�2K*������,d\=WO]ֹz�F16d�6=d���~ǥ�GJU�Rբ[�hw*Y��4r�AT�R!յp�M.U?@�h����������;�����|_��b~Ȩ� e��Kȸ����XU���U�����B~�T�I��'�=���*�l� �]mC�J��p;a�j�wA����ͥn��#��I�#�'�E���oۋ�MF,�0��Q*�iH�V��p/�{ur�B����|-X��m/��1��|#�����&�n���70d�<9dt��A��I�2n٦�2���y��~��<�mo�n=���
�� �
�m)B��BXQ�Bx¯�ېm!��͐]�ߐ�������� �"5�$:]2��t���8h��H_��Z��x�������~� ���_�-A)9�CA&����w�	���;N�`��QXF��DŅE-+$Qy�m?�V���6̶��[�M�<?Cc�R0����r��K��@>����b�8r[.}��3�����il��.��?`�z�������Gt���>��j�O �A�:Vȯ���\��Vyѵ�m�a�)����3���ʋ��`���9�^X.���D�n�T�rn��C��*�@/|����-���w��:�a�n�L>�fO��,|.�l���Hm�1�7j�T8&7=+�Q8�6s�����n��c�U�	��g�d�	����p�m����õf��A&��^�t��곒���g�:���gf=
��ϲ�qɧ!�6{
H�	�>�4��D���iU�81y�M�K��v�|�4<m??5�
����N3�ϧ�h��}�B@*��@�w(~�{�Z���3K!��j��IA)�Ο�j�N緐	�Y���"�Z��JA*��_�����6q]�}��d�
g`��CJ0d ��V��e�\4.8��EIJ.��F� j��pX@v@�P8�eE|����|�a~������h��/xH�S��QH����ZᴀVvY@��v��4�-���*7�K�N��݊� Τ�`1�>�J��U�"E^�J��;*�r�;)V�(�b�Ѻ�eSo�s�����PE�t���
b;���*�t>�	�TԵ�~?����@IE��8���Wy|�U�}�ǿ4OK����I���.��$���F��q�j7`Re|��2)`}C�N��h��r����p���QA�ǷܾWQ�:H��Ď�>g���Γ����_H��~��b8�;�=7 R9Z�Vڡ��J��R��۶c�	�A��$r$��r{+В5ph!��Q��W-�
�,s;�ޞ���a��a�:P�ӏ��R(�O"��@�O��ס�'-�`8Cs�v��P�J#�H ��[��o ��hD �pXm<��pX��=�L���L9�(�Òw]�]�H���4P��)wB���u���5l�R�[�p��vf������m� ��|�u����@H�_h�0)���:�C�����$�o �}��	>�2������0۝���	��vW�	<�a�;!h��p�i�]�Lw����A��Q��pƊ�h <�[�N��g8c��f�9�nl�Eƾ�0��������5w<p�n��`;���D�c���,Oe��i�'�`�Y��`�]��|�"��r����$~ �pj�Td���Ћr=���O��@8n�,��p��n{��!��������Y�]����t��t$M��L���^�X�����d��"y({��^�7sV�S:e��̤�I�Jz��1R)��u�R��+{99����ϱ�[��>j�E��=!oNe�QQ{�7��W Ċ��z�,�W��̓�<Tn��- ���^�����Y�K��Ju��?g��D�R`��O�3�ù��="�D툊��EF���a�Dñ�+�啣��~��Q=Yn�c)H��&��1��&�N1���aQ�p�R}B�t>���70g��8."6��M3H:� �U(�Ky��4�)o��qQ�������x�#`M�+pGX�o7TO�����L;�4�����2��2����ʬ�qdD��d2�?W?�?Wy�d�4Bv}�$W�y	l��0�fiØ���@�ب^�#�KT�3�c#2:�h��Qq#�KԏҤ�C�~T��Ƞ��"%6���Q�{&���y4����␴2��k8LgզoB�+N��/�:z����F_o>����.ᑁ��SlD:�.7_S��7_S�����)HǇq�J�"��)T��7+JI� �	V�aūH���y�J!�Io�$�G/ }
tr�<���_��W�'$�����L��DY��		���b:�bB������?K$�_�p����	L�	,cĖ�=�\X��b�$D�[&A��3��~m�$ n�[f�3��U���2���������Q�v����p:N�g���Q�>�5�4i��@�lv.�s�Zm��7�AC�h��LV��c�dea�f�S�](u��� �b�-� �ͳ�b_�A2�{A�۟��ᡇf{�����y�H�A4��<4M�i�����@����i8�����%;��Ø����4���0Z�<,p~�pxz�:X���OTj:H����ˤ,����x�A3�n�s�'�SNM��D����fN�]�p�X� ;��s�O�kw0���Š���-�\:��"����SS�Pװez���>�T�g}?���5|�?���b���H�pĀ����'c���c�T��h԰�y)�� Nw?�9���y�:��3.��W���2�5J���:���E�crv�Y�d����fr���CnV�.��o���ٰ�l8v��j8��o�VV�[D�a +�h�p( �ELáń�Dñ��=��3�o\N�F9����A3���A2�f�O$Ù���A2�	Ӿ�:���^q��Qj�=�fn�Y��q���1�)�z;8�#>Mr�s)wϊ
G*pM;tvpG|�����;$���pH����c��}�3	�D{p�������
��d8=�8�.{���GW��@F4~�(�;��(F�
Up7lT����ҪL���]��:*@o�X.�;�#��+�t����y�o��(�{�Ĩ�7�HI��V�$��լ����adN�v!?���z- ib�lW
n��H�y#�2P5GH<y�� �R
qB���'T��V�6E����;H�#D娃�8"�J�7�H�">��8"A~����8"AE���hvñQ��@f���r�
qj��&�8�a���
T�%s��S�����Ck��m^h��Q>�kh��)N��6��s�=�򙶞,fTŞ �vU�������C��#���#l�ШSL1?�6���cC滳ri2������8���8��_�I��m��њ
���F�a�[q��R&m<{� ,��7x��a�����ŉT��W/[���5*T�u`�����v��EQ(q��8�JE�E_Θ���+�}�`���}a}�{sT�L�X��"�QFͱ��wk���w���^5G��Ӟ��(�4��^0��K���?g`d?�p�(����pr�=�w\��	a.��r�x�ߛc����r�6�G�~)X�P����oڌ��"i_�x:8"������b!���[E���z���8��8�k��Y��|�&ߕ�\�6Ω��vxD6�8>"C3�TP��CZ࣌F�� &�1FF �Ό ��қâ�Lrf[��Qo��4��������)��������ON~��MN~����A�F5�������޽zDo�    �b݋GD g�B47�%뀹XdTϰ�ݑQ��v]vGF��]�7>�����\��I�F����E�YF��Q]@Hg5����'�e2��
�_GET�@Q�w>�U��;0�2v5�FU��6�Ax��������wGT�����-�����z�bʍL
�� إ�NŔ�ʁ�nT�;H��0]� �,�Z��D���i�&������8�E��FGE��}��f(<����dޜd�;(�l�M�K���%�� ��I^���â��3)�v%Ȁz��A�OT��ٟ�g����#�`�ZYJP�����������؈��_t)q���]Jܬ��E�40�@x
J�G�z`�&C�}�S�`�Q2b>���ħ����J���f�{K��/%nym`�
O�q���Q4�EQ�_r�[���ϩ�{��vM�퀧�:��l��#U��ݰ:8���řܤ���Q�}���m����:���>ܰf�2*�P��}>�N�1߇/e.�P`t�O�Z%�D|��s_�l��_[��j���Y�խf����B[$w�a��Y��H�eH��G�j�~�!����.����~�"��3�Bҡ�7��V8@��������Yؚ�2�
?�J�w��*�(,�*���cr[&]mm��:�j��ߵ��>#����>�? ��(7�	侩5~��ͳ�@ۜ�;�M���9m���9u�顃�9M��j�"m��f��_�i.䧼<��@�^�D�o/?��A�^��Z�o?���t�7�K���3
)�1�
�4�^
細�l�j�d�z����a`rV�|l��J��k�V���4���:��]��Yg�V��զ�����tU���.� ���A� ��:��# TF �s� �iP�|*��F�~<����|>X�A�1�X{p�s�9S؜Sqz&8x����9��>��Nkӳ���9�����5�Y����Z��y+]�B�,�Z�,bZ��s�z���94�"���9T��Y�;Ǯׅ�c���$ϱ�u��T����eٶ�8�߯�R���4�X�-�Qĩ�|�|9l0��(~$F����V;bu9�fuP>�IYUi���d1��s%.uB��gm��߱�x�tx��T�:��Wv0@�=�Gtx� h� ��4�7�Q�)j��pTZa��6^�A"�5i���3���b��c`S1L�������9�L&�PH'O��I��i �Ld�5:�&��c�ʾ@xŚ���>b�V��mb�Vg��b�Y6�A
�֭�L�n+/qM���t��,�:��c��"��[�'upo-�чπC�&���&g�q#� C���>}9��ٹ	()ŀ:Q�*1t��J��:?��|�V�j:�$����Jn����E���O�
^�0�Lb� By4A�d>���(Hj<���O�{�M�/�ѿc!��o��hH��w�ָ�5̾�ּ�c�}ʶ��z���n0v�B�v���"��bH�
rhW�l~���x��b=���x���΢y,0C������*�u!՛��:ɉ��u+_5�n�[[w�ֽyfͩ[��S�-�}#ȡc��8�������.s�~��B�D���tD�a��s�/���)n���u8��eMGC�&��&�gF�@���7�:x���
�g���P�峵��$U	fh��M�1m� e?��竗[��D����О�@��[����pH03�P%Z�[�,#��;kYY9!��WY�g���.+�~ >e̯n�{u����ЍC�7�w���*��>F����F��������l����qM�U��jr�:��S��>uzI�j:2R�Ď�D��@�#Q0��r�ӣ�Q.����9�\�d��^ৢ��T��@Ho����Fϑ��>��o��N��P�� �C�o��y�T%;o���I�o���r��4���_�n3�_݂`�1�S�v�N� �YdxTaX�-�Q�a�]�x�(� H�)��g�^]N����夕Y �
w���x8y��GnjLh���u��� Mt��϶&'���vkzS�K3H�Y�O�%;o���N\R�%s��Yȶ����X��Nk��1�]+�5�R�(�C�=��Q�ʉK�u��D�� I���R�8޾��@[�U���~��	�_�����v��ã��R����0����]OѠ�9���y�H����ȧXc��,2�G����&ÏF��)�4�!Uu�9@R��j����!�Qc�S����)i��gڔ�-�C��N�]s�T�BID�
�I""��f��(g��}zQ$���5�<�)�:BR�-���p���)��$��x�H�6���y5�t:�Y�e�ʹ{�F���|�W�� 7����D���{�����{��W���<��~��49R!���-����\?w�讧�Y�vi��!ˋ�qt���=r��z�]x<��"�"�F����#;��U�F;�*tSg/J�#K�Fzu��]]�9׺~����4=)Ts���̻�[�����������U�g�L��%f���squ� �� ʹm�<�݂���v��������l}��׽U幂è@9����}�����F���̈́x� ��pͣVn�������:����Wwj17�R�G��l|���Cޞ2q��+���ߔ�2q�gA>����� �e�^17 W���Wn.T�F_]�a��h�`0�"K������]XRXe�k���_evaI��waIq�PN#��d��G��f_�L�)'��@=�g$J$[Ng��v&��
B#u�k;Uݨ�(�<J�Af$' ���ث�Z���9JV�>ʑĎ�\��<K���'�u��J-��JD�'�?^�ФKY�x�S�Z� t8e��j�r:���7�}Lv��SF2����h٭�B3�9��e����8q���ʕa�g�拮�F�����H>���KΉ,�\���y`�L\�_�8�+�d�(�������R"�]ZX��u*���rT�rHe���q���>�*GK��cD +�46��I|�қ �nE+��䞇)��W��vl�s��˼Y��4�S{����Ū��	�&}����$�@��[[�`�N��G��̏u�/g~|W��@I�p�
�G	c����*	\�g��*��-6}kwqH��Axe����Ȍh���*���GIeW�D�+,����
iJ^���$�Q�UQ�"I

�GU�����:$#���ӁTA���t?JB[�rN�� WZ���M�n�%�]U;8��v��+�}Gz-��ݭ1*�}M~qHm/4*�t��@����z�-���$�K�ţI��'�C�D���a�N&숪����;.�#/�dÎ�&(~��ȇQ�d9�K����X�ά��t	�:3��F�D�9�gp%��t��.�8#�����Pn�g�\��_���}��з�)�&~st��'�iߞbSFR��G��:���d�_ګ#��t�������M/e�#Q�����3��o�w3C�#���ƷM��B:�V��F�;�lE�
�#��}P*u�#�FH���ɉ}_�S���W�P8	�δ[{j�g�ȑ�j
U��5�Jǈ��(��'��ۤ�s�up{�Q��Ơ���[������!����(�j����gm���9�?�%�hVjp{�M���482���˜�$LG\���|��[�^�.y��%��d8�{I��^��Ŗ�鈣{nM�(�]|?���^o���Qx]>���5G�,�1������(;'��Z*7����v��紡���uhh!v/{�Q1C(���
��Xg>��G����G»������n�k�>�mi�WK��ݘ+�^y$����)Ƈ������9�:\���Дk|���_�6>��W�����W	�8]�gՔX�7��i5%b6ț�-��Hf"����#�	�CX5���	�P�G��#Y	�>��%Z��'��K�'J�S��ף�׮ZJ�^�fO^4{c:��'�0�C����P�J�Z& O�Ӥhѩ��?��k�^~7J������}�k��W�]�=o�'*��X5�5Q����D�-|�8$Rx�q���UіN�`��.�8(JNo��3�    ��8��R�Y������a�gH�g��D�Q����	\OT�n���h��pHi��|>r ���&f��x�Q����?0GA���b�z'���vJ*�'f=�������ϯ^��(��vX$T��㢛�Ci�_6��ˎ^_6�R��ۖ���)���`�3�b"	v<�J�N�.����B����$��/"9�(�Q�7Kw��e�_vI��_oY�'�_F�N�.Oq(]�֓�A��/b&jq��$fP�������'�-&圭<�@�9��>��ڜn>��9���U����h��R%�ޔ�*;?�����RX#�~���K��;D��_M�t�ǈ�Q{闽L�E�"�F��Gh�p��u8Wy�b�]N5���e�D����4����������r��(�_ק���#�`7i�A3��[�4���U�̷5d��B�)���%raw����K.����y����|�aw}_C�(�����,�wV�N"�^u�n�$� T��gX�Qrawy�C�ݲ��Ȅ�r�F2a�:'3|'v����}�ɂݾ�|$�n_�N즯��ܷo��N즚�<��y�D���-z�M��������� � v�Ȅ�4a�؉��� H��4a0�~����%����:/�`��������T�]��p�脁b�C�3��I��>����n�S�շ�Ő;��L�D6��&�+ƭ号-d�n/:>�d�R`0aw2a���[Y|>n��jRa7y}�M4�[$Wd�n��3�[�Y&7����y�?�y'v���H�ݿT��4�]�Fl>�;�<�H��_�|�M��{�C�.���-��#l9GF���H�]��#	�\�GR,D+
|�>�|$́�R+��ҥVjҿZ�I�[�i2�#��~�����v��b �{�Iy��$�q�Tς �bfV�,�Ql���)A	t� ����::z��^�G���:4H�}z�Yy#�F/b�N��z&��ra����؁�D�ؑ��T���-��,Q`#�"�r���dH���Qj��-��^2A��X�w���W����V^@}���"���ɃO��a�0�/�o{�'���:��2�E�H�x}��8,���yt�f��xI�Q�d7�n�ɚ�s�ws���|7g}�xU)��I�#�vk���5z�q\T�G �+�¶3O/����q`�z$3�&w�W/rw�"�R)E�#���B/]oi7\� 	vל��"9����� v��f��˦̈́� v�&�I���o��k�0�z����$�]���x�Q$��xi�#�2�yF~���I����͇ɉ@�B,�hV]2`7-���O[��j��ֱE��֟O�������~f�Az?P�u9�����-���Z+ƽ1W��%m0]EG�=��2u/-c�[��O~.ʽٰǛ�o�{H�C:�;�p@��0FW��ds�#�{�qp�v
=::zSiRa��z�ͭ�Kw�E�Ջ��VG�.��ut��ut�~1_GG�߻pOO��;�WW�J��N7U{�qXT�û��Ka��^Օ�qD����q8��uO���E��!��A�q�_`q"�6��Q9��i��g�����.`��R�ߍ�QU�p�G�h�|Q���Z�i}�x"dt�����ѕ�D�(zY`�d��o�A*����ϑ�ˉ��ry9����nO�nD��R�zO;��C>��"!��Q��.0�I����{��F��i���Y�{ƨ��F��*ZZ���(�G�`�7c� v�6�`7-��PJ���PJ>>�P�W`�樨�:i��3_��_�Oe� �u��_�b4�E�ʻ��I�^�Fw3�>��:E!ܢ�|�4����sN��,�
��Q:��-�E���E��Nz��kP��%��}�K����1ʗXt�������`����z!�xD1���^%���I�
\$u��y��x�)8*ʘG`�z�9�Д�I�҆�$NB`���V��$����`�$�m�>��94RG��;4R?J���E4y4����=p����"��:ḅE�迸cA�Wь�Ra�����h]�A�?h��ށ��a����zB�>�A����;�a�C�����u��={9+� ���Bo��q�$�K��.γ�(��f=����,�9I����ng�7b��)s����y=.F��Vr,<�G�;t�'�����s� �E���'��r���y9Q6�,�'B�S,4ؘSO	�̲��<e�S��,�S����d��.t�{8S��?p|1c#v^;A��nn����u�CΪq�9v�0��S�d/� ���`��H��j�nn�}�n:����L���~
�w�{��Ȩ.EPe$�BJE�U����u����+A�����M�~~]�G<п��ؘdZ������y�r�_?���A��Y���z�5��uU�8����j����?0v�+�r���~^o<���O`���Ž�Z�A���{!"���u�$��2�R_��S��N�ur��%�D�����b�.�������B^Õ코�O</ˇX��6Y����a��C�]l����������u�=�Y���s˛?��0@~���o�?Z�}�qH}�|�C$�~��g������V����I�����|��=�Y������@�K4nn��b.�2^�a�ɡ��F O�*�AWF�k�r�$�.*�9�kwZQ�\	@J�s�i��[p_/���f�7��9{h<�6�� ��uO���9?w�#�zև�t$�y�7�c!��AZ���!Z��y��A�����������
�mʗH�]V��'~4��+��
}���hh��^bTu��u��-�\G�����4rb��x?b��,�f7�q�fw�e�Olۭ��p���g@���B3p^��!n�5@��Y�聊�����c����?���D�����D�F��3C��P`�Nx�~x(@?J��\����KGEᨦ�M�[.���x�r"me�qi��B�q{��j�V9���pݣ;"��bژc"���E�;*�*���w%�K�`{	�V���V݌E��w?#BE���gcx�(�H�a��sm~���*��/^�x*�K醴��|áW8��Yp�ϭ�cxy�<� �1z?��i���cx�(TtJ���{FD�}ݵT�*lK�򽖢m�������:����!P�G{++�E�F���n1}�����R� k�cE�+vl�����'ʧ*(C+�D�]�#���R���`�R�Z�`��P�F/�����^W$���cH07xw�j���1����Hȷl�a��9�"�e'Z�9#
5��H�M�M�|f�eDy���@Q^$U4�kt^�11���f���ۥ�X�csͨhcJ�o�Z�&F�����Q�gڗift�U���ёq�a�5v���(G��HG��#e�#^`d�tD��8R��k��a����(������=�`�T���C{�xi>��oJ ��]GyQ�٠
���^P�K����b���cc���˚1��u���K˷5�u[���Q��ʢ
�풍�jcl�<IA���oe[C����ѐnˬ�8�mGC�-!Ո�z��ϔ��2�
b�8�Z����̼��"y��s�̼��:�Z��(���j�LZ���D�^c�~-7FD�D��L��D��Y͑�r|W+�[��J��f��+�����N�H�8Ֆ!��������>1���|�P{�|4��s��1� ;[C(%潆'5�=��|�Ƽ�2nLϤɻ�q��L�si�C,���� ��I�yO�2�cMv�%�|��9-1���gӊ�<0b�+ʧ�cv���Rg$��a��Lh�U��u����g��h������ g��h���1�dF�8�qFRe�=�8�֘wǩ��n/�b2C�nj6���?�sh�jOß��=�`{-]�<������՞ޘ|�C|:�m���	$��+Vc��<g�ס ���W<�cR޴�Q�B��a{�O��MϜU�  �I`���;6;���M�<�W����}B��ˉ$��������x��%�����$�/Ŝ����    S��͌r`��@�S �럞C������/�����J_qt��'����\B�����q,ۆ�Q��?�^�׿�E��x��JdAp^������O��m��zf�}s��]c[_��\@�۷h�9Ա}}q���eq&���;/I������j�S�K�)�M�G�69��֞rR��r��U���c��f�S�������U�:��c�Qؐr_����� ���,~��R�GB��!?$���\	�{}�T�חLu	43���ҳ��Hd=^�%����G�X��I`=T�♖�\Ԅ�Ғ��:�+-�*��\=��(W��s���qt��ڍV���4���_~}�	�VǴ���XjK��ZEPIV=�����al�����
���
��?���r�z+/��h��$�Oe'����ɔ�F[��2�t���R,T���B1T����
٩Gt�c�)'��y����N=�t�����ķ�<�0�5�!�$�.�!�����h{�X��4�Q��==��T���R�'�QGUpfp@E�9��uत+� ���8�Qʻ��fٜ��es��'�29�a��|O`s��3�*Bi�ֳ֚���`�����h�[���M`*�4���N�y\��:-؝�jb�!�b�!b���?=^����C�v�Z�;"�@����T�uz|��0E2� �@��5@�'�g�R$���0%3���T���{i���{	D���BLqdR��FQ��#t��r��S��ݜ)�TV3P��_��*U̿_�f�柯�h�f���dB��¡��;1@�*���'�΋m�b��0 ӌ��o��W�A�ģ�?d/�N
-W�e g7�l2��ˈ5ɦ��<&������o��+��&��H��U��K��^r��j�F�#I4���z�|�W�eNZ�A������ �M��U�&�^�@��\��Yf��99���Ҋ˿a/�7z��؇�zd�6����x�ܿa_��Ɓ���9"ʹi���������<��/�����9��O���}�g&I1m�b�C��ucu���!��߯W9�o��L�,����5�X��z�'a.���Ow���6}��!�)fN�;671s~����K�hS�ŜI6�A�7bry�7�3�@��oؿʂI,�ʖ/�����7�Gb��b
�|��_e������C��[�8�S�n��6���X�Y����A���� ��(��;��01|���:)(�?c����k� ��%���t���%\G�^	�߲s|�����!t��J��k���q���ht��+�����O�K���Ū������D����K=�W־���G�]IRI/Mr���e�Iqm�!KO
5v3e����!n�2c�K�D�3a�:�2�@Ri�tC��?{�GQ�����T�ȒS�^�'l��p�(n&��}F$�6Ͳq��;k{�c����&�P�D��|`#��"������#�3c?��2]ďǌ���W�T�y�Heů)����i�:�����W�	9�9���S;����*�4!���2qD�i{A�|��E΃��KsGp���ы�nlGG���vp$�#�o|����HRG轉7����zQ��� �h$Ў�Mα�s�UR�@�����vx�V����C��Ҧ�EE٥��i7H.m�\�2�P����MVԱI��l����u���P	��#~�g��v��L���x�3w�� ��["�S ��%H���C�;�q�����Ȏ��]�&��C~��=Hb*��R���2���I*-`A��� P����C@R�hP2��-ԧ���f{��}G�t���C.����$�	>�c�\��v�y��,�In$@&��L�C�&� ���<rp��O��L� ���,@���Cr�[�����{ħT����ϛK}l�|��G}$�=�X�^�"��٩+�؞��܄�G~�=�J1�=��1��mtnf�b�{W��]`Б�z�!��+��ebK��ɜp|�1:�wf|��i��+~1ސ51ސu1�Y����h�.�Y�H�c_�x����C3�H�5˳���h��W�w��Ȗ�Z_���n ����uG��M�=��^^�jh+d�7�sv5�m��3e��s��'%�|9@�AOq���z�	Φ���.�d�[��N=�G�����y�Բ��<biU�v<1�`�@�ji��z�z��8~�I��%�9��btHM��G虘l[4�#��+��jq�#��m�f��E�癐�-GBY_E��)�Y��K�y��$��U4���AP4�?�����t���,&�s��"���^z��0Q�,Q�8$��6��-� �A�a�@&��S�L��jg�V����'���s��%r�	��{��$�(�LI(�T�=�35��?f�P�YwL&�)���c����X�g��R�You�|n$ǡ��S�8�^G���(�-G6J�ԉlT������N�&��v�Y\������]>���O��ԧ��xZ�[�F����g+�t��q���{�����gk�q�"� �������9�*0�w��/�p�	������<��JF�r٦+τ#�t��rd���9G���K�9�SW�v/�	�D���QVj�0�*B@��šJPc�TLeM��u9\V�4P�ѶS������A4�b���'@JHi�Wn+��*a����׺"?��i���.q�F�^P��^M�sxl��3Z.q�32���H�<^�!slc����6���*����E��<^Ky���v��m���7����M��-]豷�=�v{7�JHq�_�*�~�:ZI�h �e4Rr�������p�Ȯ�VV��ܮ��e4Ż�6)���J����PD�b�$�Ƙ���]!�;4��sz�%fY�v��Rr�RڄP*J�C�0v���C��{���NL>���&��Z����<�rsM0:�-�N�N�a����&�N����Q�Ӵ�U-�r�ED�Tvk�+c$�d�塯T��C�R��Fhm�S��� ��kj���	��kx�\o�z�{�U,�z��sԒ�����,rCwI�����.	8X%5�t���c�:����#�k�]zo�I�5H��X�QGC����U4�¶\�D:��T�z�<���8by�o�r�7ʻ%��7�<k��fy��1�zJy����8v�qF������_���WEMȖأE)̍�Q�f�F>.�ȝ�99L��s@�h�٫pej�.#ِ�)��j�Z9�V�5��+W�j	b�4�����O�O��@��.���y��0��|�6��M�/9��DHEl���qa�g�r�Y\ ��h|��ϣ!�M�*���8V�ͧih]�^C��VC����9������O�кy�b>MB�*��J�?^M��P%]��|��\�|��.=�"��F�OW�������������~{�s�f���-U� D���k��+�U����B��C,s�����!����`??�2�"�l_�f�C.s��j�a���*'��.3+[_�v�Y�ï�N"�*0�]�.�
�����̪/�.L�!O��ix <��<��h�j�yV���S\\�S�]&��Np!�WA�h�4�:���]MNUFI�L_S�]���o���Kc>���vb鍳��5|��Y��
�̬J����	�Y�D%�4ȳ
�z�wmh���J4^O&H�gU��;`�yVX����iAF/�F�,��5@v�,�bGl6_���7q��g*|�@?1�w�R����I�2����g���b�d�x��ϲ�=�µ�n��͹�i7����ƾ��D�穕�5S�%�t~|M]���?h3M��5�A�i�x�<e��ч��G������u���b���}��gY�mb������@�}�e�3O�;��R�����E�d���������	*�T�H�+z��Lȳ�p��A�U��q���AN+�'7�,?"�?H8�����݌@���);Y����f�9wE�]�\�.��2���.�TbO%�9�_�i]�_���=�|ȩݞ��ȹ-~�Q�	B�Y*    �A�<���q�#~��G�'�O_*��F��՛��y�.^�ո�/�j�}�����ؽ��j�޾�K�؋�a��P�����_,��B�È@�8�ǹ2��C�C�"�C����g�kC�Sz��~f7Cu�sh��&�SC5�[k�;>2Ba���}<MwwQ"��8G����ǳ��9��Au��}qSsh�m�w"�����4��v���LQ��C�?F�XH���[n���z�QH�xױρ��LN�*�^����b5�$t�������+���0vGA�c���3
�u���N3p��i�+�W���Ba�<Γlw�0���,�i˖ݵe������fV;y�=�%;yK��-;��P�G6��)��oۮmr��;( P}��*���!��N� �>{@����`�k���v�� �-��}L�� ȧwu��k�.׭�ص�{WĺyW���]5ի�W�rd�Ĥ`?�=�J3����n��o�� @N_�b���.��g����B�1\�#�|�S��i^+�;�\���q�U;���Y%�+�ۿ�(�]��X��((_�&���(�����M���C�ǩ��p]��I����>e�'+����O�GA~�f�����M�����8����؏��<A~��$�����gw���� (�}�:�j'�.w18��������[W;U������t�sS�P<���y����6�p,1���<0����b$G̾<����E���Wy�.��e.ڽyw,��J�8�*%N���<?�T��9od�'���0-�{�a��=�:��ɽ��=�쎃"��0����)�A	u'���tGA��z�����0(���5hz5�X{��IJ����Ph�|0¥w0�%^CI+�Z�<�H��8ܒ�[Z�~T�����:��zC��D��G�j�#�
*�b�[�$�ʾ5J"b�����FI�G5J�r��X���B��[n�uZY�Q��Z�,]�!9��~�0�T<$�8�&;@NΈ	}w�C�
}0Hw8$�>��>e�+��P}�����,}e��;
���;Z%ư���ߩ�۲gVeW��
|�# T0��2ǣ��2��h���z<z+���q���@w��Ā�ؽ��;�P���H�a�g����	�@ەHc4I5�|GB;u�)�����\�c5G��z؋�'f	{Y�O�B��+n2�}�#��&:�0�D������:�C�Ay��u���x�ϭl��U�1Au<�^հK����|gǛ�����W�Y*҂|p��b�	��9^ �rN�
����x<�n��o�S�7t;8׺�r�皷)�6;�[p�U8Au<��Ib�B��$)o�ﯼS~�r�q}6��)�os�\�!��̐S��x���sr9�!��Ar<��=$8��z�	��9?	�Ͻ��s*��71����a����f�YDC���>��vK�u��r�@�����W/+B�!�|jǭ)�Ql�i��!8��ӖO��2Nc>܊�՞��_s���8��Ϩl7�AbT�˚���'��������,#dc�NqC����o�n�1��-QF����_��41��������<C,\&~�X��M,\�K,\�[LY�Gƽ·�+>O�>2{�>���ʛ��-��/���;<����Cu0�*��~����p:H�gUw����|��ԭ��r�ߞcq�����4��\h��8Ug�eq����,:�I�X��^�5`,�U�� �Ź�����__�ޠ-��O���\Q]R@]<�t�n�s,qh���3�6��E���,���-΃X��G�U���\�p�(0�WS�p�W�Z�|�P�in� -N���j�*W�T��Q.�_���|��@��ʻ�]�C����F��ũLmne\+�'�.���8v�C��T᜝u
��mLG;+���v*C�t�#}O�=yO�t�@
�Ź��@]���"-=�|�jn9�T~��մ-xG:��t�SGta��Gi�zA��݀�x����s�őb���T;�TdW���
��8����}��W�L=r�����]�虞��4-���Mf7g�����V�*պVnJ���L)Gwr�$���n���d�i:�	�U��[Y(\� �x�}�a:�	�[�FN�;ɑ�s�����`�-q�-��R�c˥���s�:&9T��@:1���k�K�kM^�ڧTO�S�T5zj�r����}�c����٧�>6��R�x����W���?rn/:��o^=�O}ɭ6�K�����rTr�1��=��uB�������R� ��ݧ�"��)������~�����A���أY��F�,e�,�HyQ̳t�3�����x+��7؎gs�$��?��x-���#9 v�ѝ�ȱ���^k�k6�x�A��?u<��r�z�{OpK�X��e�i]��-n��=���h�,��dhԱ#��g����b^�J�>R$���mH�Z,������li��Xg�� Ṭ:��q*oTL�� ��N)�b��.�$U>�$_�&�Q�zsV�X��Yn�%7�+?zc.�\Ln��GvFHqV�Gv&�����B%71���Enْ�]���gxZ���QP�ft�L�[�]�9ol��5�:4�`��L�[�C=A�p����S0��p�S�${��ݥx$���h��n%��o[�`�R	{�2��a��(̖�= o�Ƒ�ƺ̑NMG׻J�i�{J��ͱΟ^LX{#�N���˖%��S�w��l��M�xG�:��N�����_�{V��XUnb�*_b�*�b���N;�v����r����Z�q T���}�jV�:� <�-���5��W�u�#��P��T�i���9�]�P��~���rE�']�2�t���<�&'{Xe��= `�!����c=R�����fq��#jz�P��?���I|0���O�J> �bO� ���a�G�?q>�&����З�T����"�������	.��?x$��(u�	G����wNS����9M	����`ꠜ�`* C��#�eu�fEX;���]���G���)
,2��Q.H�ɤp�"�������tf������CE_L ��&����9�u���h���9����BV�����59W	Yq%&g+!��=79�Y��+4}���?WT����V>�͹���X��M�C�aʆ�G��e�W����:�@����.�V�?��A���4`�	9���|l�u�6�ì�����*C?��l���G�9�Sv~���r��!{�rZ4���`� �/b7��.[�S�Tא-F�Ñ�)K-F0t��8��\ۚm���#�j���@���D�j�[W|(qs�����d3��Ї����$t���g��a/�?W������Ƅ���ē�+�.�=O��h\&��3�~�e`D%�8�(E.�G��T9���H��+�e��ȱ,=�9�,M��� �$T��ҭ��m�1�ϠS��v�����f^ώ��
�o�� &z�l �-�o8�9��W7W��/��"e� �IǙnț�����pQ�E� ��2�m��@_dY{�h�l��EYm
� �����9nKw�p]�.L,��,XL�v\�z���.��r?oGF�yp��)��
��C#�Sa��C�[:�NrZ��Y"�R�^�TV)�Q�7e�;���p���W>E�+�t<�C+��	��I� �?b�+�l�i��?[��8�{���8�h�0IE��F���Sl��
�l�H
��Ao�;P"�b��܁���0��H����Hj���1�z+��V�H#}�)���s�Í�!Rqb��U�`E,��V��`HG��Q(H��P/(���V�ѷ��7�����Dj��r�KL���t��"��/��S�5ٹ���Ԏ�Xe;D� ,� )������d���-��!eĖ�]����Ɣ�����Q�%��:<��A�QH�7 @܎�� �U����r��^v�̱P%��ꛖ��k�]MU�g-�.ŮmKea���-�    `m[�
�ڵ���j׺��;OmZs39Hux��nn��"���uph�*��G��#�7�A�0)�.GW����O`��NS�n���L9�į�����{{�]�9V�����M�t3���9�.�+/�9�M���啾���RY�{��ʿ��\(����O�W��X��+�Bj�,��>�s�=�	�埻sį�=��{A:Bj�L�{��[���$������LR�	���\&E��r ߾l���m��8��J)�uv���r�]��W�]�~v�].������5�`��!u�uN���u���H����&(='X�������<����_01��׷{�}ə��ϸ�O #)���f�9��A��@���߆��0�!v��S(v���#9�ڡ��:�ʜ��$8<A�x�C�!�Jxqk8<�����#IϠ�� �d�m�����p4z�q���9ЋH�hW�(¸~>��9/lS98Ҙ
Vڡ�-:6R-Bnz_e98�A�}{��asB쓀��q=	�ãP�u�_&Q����$�rR�&Q������`l�v��UN��ND��7�Ύ�"Ր�n͂��O�b�1�����(;�I���������(���)���~G��(�]~V�G��pU9?�ϳ7� Ŧ��D������q�[�(�G���D���3�_?���
�K�	@�[���������ѣ�h@�s$7q�����ĭ��9zU�u@DE�2$�`���	�Rp��D���j�B|����5Eq�\MQ�2�k�d�Gp�-�-����{����s��<����:u�V9lțx���V���g��^H��*r/@	�y�o@��6H��4 D�^�S��Ϳ��?��?N��� A^D$�k�Sk֗QZO�[~�}^�#�?�D\�#��ݨ%�b4�\�Z��P�Yf	"K�n��l�P^���:�����{�^����v�z%ѧ��z)����Z�*��l���S�x��H��0�&��?�o��
�Z�'
2i}��HT��^Q$6��"��p�2���_7>9�+^������{_P��WO�8�:OK�p�FzU����)�o��x�Mo"Xة�9�����Y]� �Miu7iϰ�M��o-�$ ���$oU�����e8��q�b�/�_[�7�G�~�]���_!����i�)n����W����������g	W����߈�E)���(���H����p�H��:}��K�O���U�$PE����)g�[��/�_ѿ�(�h��`<���k��RB�ب<Ͻ/J�S6��F_r�v�2�o���7�������Y9�=�C<k�q�16q�=���
6�R/�x��Y��I���,l,��o�b_8��uh~���ֹ�bJN�LB�r5�Gg!�h�Zt��x?�|V����()ւ���=TBS]���yy�� � i��<'�f���x���o�W�7Ė@�+��J$�R��Z��H�t8��@I�'%����IA�B{���x�g�㡪��Z/������d�o��=���C#�t>�ըV�CU��([ɥE�T�R�� WbW�+��]eOP)J������dcw.!A��F��7dA1]ǭ�dBi��UË�=�́��T]$�-6��О�C�Q�*7���uB�?�uJ�?�J�*ʝJ�*�L�uM�oA1W��疖өGJg����_{�M(k
��F	�l�oV�����8�(.g�n�\dh��CJ���&�]�Z;�Kl���Il���������c�
�Y{<h$,���$���C,���� �쉲#���\{"�҉>��h��]�@ˌX�=^p$l߃��3�hÞ�6�A�p��G��멌k��H���=��բC"�����(����G]�k���Qt[,�F�v�1���S���Ȟ=3�������U�1�P��ђ#~������|{��β�#�3�i�hЈ��I�j/�8����Hd�}L��NY�8j﯂�I�p^�e�g�r��E��b�����;R�l���<�����<���GE�=�����س�O��Hس�uJ���B2M�<��Hp��a� ��������^��=�(��>���l���ڣ�)�@��ɕ�śbɕ�z�W�z�Ԣ����G�^���f���E�����5*3��֚���p��i���%�8�(<J�C�Q�-';�`v��X�c���E���`j]�����.ƚ�C���X�yH�_� ow�ܢ�H�1�>4���!r����j(�N^s�_���L��p:�A�m04��KCû�494��k�����G�����b�^z$4�0b�Oo�p�/b�cG1Y�-�4�a-��$�q�SS&��EivDܠ"���W���ƫ�y�(&�k~B[�V��PI�UX�:�V�dZ��J��u��W4uпI@����)IL65�^Foj��0���Z{���I:����\(z��q���%%�#�����:����OQ��%u�CLlI�(sw�$�������<[�$�1X��e��g�F���#
��mqJ��Z %�<�g+�����eoI���ݒ2��L{ixKʴgT�ڑ�i/�ɘ�?=R�NJ�)E�5R��>�(v�
�-E�����:�l���� L�Q| ���k�k]w��T=�5��,����ڋ���G=�d�Y�l�#
��3�@F=�d����ʫ��Q~)_oy�A�q���W�k����'t�㾞Q@< ��+p����re@ι�˕F+&#��f�3�Q?o�i#�#�kR�h=]i8�ңlo��y��9�b��#'��5�����j��F��U��O�~��>�n8U}��~8٢�G����O�23�m����Ω9V�sES_�q5� Q-pR�+ti���z�7]���z�7o����Q�iN�B�&b32R?�~#u/����1�|�l����U,�Ա�|�M+M'����:�O0��Cy�r:�Q�/��y��㉶��:m�����:m��ct?����]5��xV�8�Ӗ)��:��TcR��'�%u��y7PR�*�Kt##��Ly?�q1�M��5L� ѣ�	(9��7������zORR?/��N�.�{R���U~���{��//�1�]�����Z]�>��y�����0��Ϯ)���L|�l��D�6�ڢ˕�q��Z�#�#�2��jX�܍�ԏP\s�!���Y����QV?#1�S�m���^�̏U�R�ԭ�����J����ydX��dX��E��k�RǾ�Z�3넳��cuF�t���-�x����ӷ���}�;9+���:��(��:�ktw�˒�)�I�X裾 C_E9��(����o`�;��(�s�@C�U&#��2R?�'#�S����6���!7�JC+u��!���by;��R�s�Y��7;���:Y���ԏ2������8l8
���=		�F�@H�@��^�=�� �xk�x�d���0胲@H�����p�qÁ�G���Zu$4�B�S��y�U�D�@���6AG�D��o���nd�a���PT�y6	��t��>�~8*��F���ש1}NR?�M����X4�B�2��8�ቧ�FN�G�����S�
����׌�ԏ�u����l�M;W��4pF>��׸}���X6Iy�O�X�UFg#���nax��v�p�\R\�Y�\RZ>�arrz��E�!yU�mdu˪=u,�o�	XPT�:{��j���ܬu4�T���{��%v��ظ�	
�Տ�g����\p��p��uʩ�SQu� ���[�w��ۭ��7G���I�r�ŖT�ԑ����N,������ִ���8��D,馀���Ê���X�� ��}��£��j/�?�1�:��0M`�^Dz�x�u��k�bC�Q��u��gr!�x�
�����#������'���d���\�Y"4�^\��[!�ߖ���<Z{�i�ۛP�H[�>�\H���V7�Ӄ�&iuS>�1g)�ekNR��7�(�����
�����>�� �-��ޜ�|�    i����`	Ie0�M Y�ʱ�*���ǅ��%U�m��qʽk61N9�f�|��8��B��+���di�t0�����!!��C�
�xu�M�AoR=���W<�����MCUvu1�.ה�;�^Sn��t0���WCZ���C/c�$�2�\�N:I2V?����`t�.�V�H��dX��FO��G��h%��k0�U?��vø�O�Ôxɩ-85^�ߑ�95^r�S�6��K��6��K25��K*5	U�ls,[����M���(|�	&�kz��(������t2d�o�3����M���Cs�5���+����-�����~��Kõ��q��z��P�
���HCֶ�d���o���^߇Q{0H�
���h��k��<���F�@Hu+t �g�[3�F[3�ܭ����@�gsK�!+��LC��-���e��5zD��'}����x@�S�
kw���Ħc 嗅�����G.�V�!�"cȑ���Ta6�	}d�a#(t^��;�߱h]��l�7#��(~�(��y=!g7���	���z��rh�r���rpX��}Bn�G�1g�/�7��!��z��1��zlU�f��Rcc��Av(S2qu��/-������0�W�#wZ�(h��-(��~鐽O��{��c=�|&r�S��7�9� �{0~*�q^��&�_��;T;G'�S�G�F�7�
%���
������P�f�_��mހHf�|p:���阼��
�^v��*A�!n� �9*��_t $@�v$t�}L9=
���|ˠ�9jy'�|���"GV�F���Z�6�\��O�u�j��:[�w��묻�MIw���;�riSҝ�)n�P�4{)>��
d���w'��m��@g�0+o:�N���3���Á�:rQ���-r�Z�␁혆��*J���X�2��b0pZ�a����:�G�А7��د�EV�������œk`�[�-��*����{����c��M�͉�����T�` [���+���ݳ�����=�1�3���V�@>9�u�jE�`ൎ�Qg�O�u���8����Ǒ<<���rq��8���<#���)ª�YǴ��Xy�Z� N�����d�N����# ��`�����50���{a#��u$��[Ǒ\O��q���K�ye�[�ye<GƩ�\�7��
 ���@�M	`ˑ��·�v3���Hsz!��HH_a�r4�f����Msn�|��p�>(	l9z���sw����Ε���W� Q��
5�uq9
���@A�A褝BM���BM��,����(�}�^��.ף�B�����<��{���8<�M��z�sB#������cBx�Sc���+ԊX��\���hP=��w�aؿj���\����g�uH���5p4R
ᕻ��: �p[��O<p�� Ż ��,G@'[S���n�qkYQ ����2�3���˅X����g[+gF�TV��� ���x�+����x�H)F�Ѡ*��^��a6�¦#���q��%��৞\�.�섛8j�bC�DqP��9/�J��^�3,/��Tʵ4Hvϒ������o��]�N�����)�_�t�2H{A�aZ�e�i�Z�(�G.t���G.�rQ!l1�i%�����@�<��y{((^���
۷�;�^�g杅�`��ܞ��[TG�O��Y^$�6�^C{b�G�{3�����n�@hV�a��(�������5���G��R��u�$�~�}R���HW��ϻ��g`�h��w�2_�i�� �g�����ߏԴ�Ī����мJ��q����?;��;j�� �\M��G��gO��u�g���Q,�>�(��X��4X�s���Ţ��91y��y6�Va=Ȭs�xH2(�@mO-FJ,��Z��T�=k{u�<$��&��������a{�m{ (f�vE���`�Z��|�]ݐl���ڍ�'�m/�������`(_W��ڣ2��!A�J��j�H�Izb�=$H��\�C�$=��{H�����l;
�%���e���Xf�o�a����)�u��)�#�Z{JE����R>��AO���˖�3��m;�s�cl;"A#��}N�� 9�)׊��M;bAȲ#�F_;BA�	��tA#�>����2�nT;�0�&��Dp{I�S���$g-s`��M���I��4��y�9N�Q$-�:�"i�l�5��W���4$~��֜J?[kN���Zs������X�Bdd����m�TMu�Y8S�=n��ʙoMI��\�1���I����J=ڑ��x�%<�Ry'L�I���Y��T�^�)=�^��e���J<���6�]���4H�+P��)��}9E�X��s�8����_e4h�����e3�u��,���2�؋�>����A���˃d�b��
/�^�ez���������^�J]�(�h�Dy��ʽ<裭�A�����G޼�����|���&wu���U��?W�o����W�_���T���Щ��W�S���>@��ZC���^����½t�y��XV����t
ɫ�~�uBl���������%Qž���D�S~�7RWOͧ���_~!/�ߵ_���w���r��/�@���i����?�k�V���]�x�3��Ӗ�(:e�\DIQa�S\uLΰt��t�Cu
���
 O|�����s�2=���׀��q�R�)-�S\G��?�XU(J�T��&Ou�aK���K.6�r�լ��%_�쬦��=�O��$$��/�����\��Y���h��ne����J6��W$���MyհR���Kx���M�g�n�V$��/RBL{]�
��kk��/낔>	��P��G�T���	7�`���]�`b3m)�Ǡ'p�}F���Ãw�����R2\�s�d��{�*Z�SX�>U�G�:��e3�<b%-u{%&0ͳ�fN�]�H����Y����v�deV7��$�(��bk;}�W>E��&Чr*\ϻİ��-����ʯv�!ހ?�+�--�j��E�5��P>j������f��j�3�|�|�{���7Oa���V��_�>���~彰ҫ�QX�%�b�?�N��V����]2S�j�e���"uW�� �e�U�L>ꮕ�X������Y������y��à��I��k���s�����=rw9q�]�3S�w���2��TFN�qo��D$��ԣ«\�Ľ�W��
�T�:���G=�c����o�~*�������L{s�#9+L1>po�)��+��r��Q���R �7^��}�z1��
găԳ� t�+)A�������SCF��8�wv��S�P�׸_o�pw�����D���B6���S�T4�%���<�gB�O�W*��#�1�P׿r/�p�Ň�%	���?�ƻ�2g���>�SP��^S����J������6�b�"���K���M�[�]�[�C��G��(�eę��[=�v�j�T&���?y����d��@=���![_P���[�����*��0����,��=~-�*����ܳ��U~�������Cb�����Ka#��ǧm/��yG��3)�����lr޲��H��ݢB��}�Z��������<j��϶�`B.>�A�y��+�H==4��ƂvÁ�@ы��C��|�«|��U~D�*���"o�,�����ժw��C#��è�๪E��!��_�p=�Y�ɷ%^�Qm�Wj�s�~T~��E��7qK*�uH�p��q�?��Př6�KH$�L���CT�v	���DD2��;=>����i����������d�Cs=}*��=���]n�G>Yn�^�#�82sاt=J׎|$%��:�)��y�G�T�}�H0j>�#}=���]������z"賾ru;�w[����zL��3w���v�����q�S�B����2��N�N�hf=�}�[rm���P���=5hW`Hg��"�tػ�˥V&����|�Q �����u��~�8̵���z.}2�W�E�����g�%S�;��)����=&+G    #~�#A
�)�`����?�~˃��	�����=Vo˛�o��s*bY��E����5Y�n��؇<�f�	˓�Y���@�=��I��z"��>Z=��U>�L���0P-��@��A�WBx=�	����oF �>þ� 0"�����m��s�u������6�H/�_���o�t�p-@�%?W&S\{���bع���xm��T����d��%���Ej�X�'m��,�H,�_p��%���K.���6�ez{L3B?������[�[�G��7��nnY��q.�J7�҃P�����*Y0��*��.ً�g3���*��j���~��=���n�x�W���K�����&.[��R�$n�g~��y��U��H*�ߗ�EJ�.���]Ɵ�̋���s��n��U�çƛ�]���,���g��^�&�T�a�SM4�w���ב�<�G<�����$��0�'���ǜ���k?Ct���hR�&����c�4X]�����O�M>��B!�jG�O���V�4ث�l�O��.F�krA��Ze�� m���krA���
/o����滒Y��H(��f��ܽ��HG����ٳ�G]_H%ݾWζ5�����նXh�p�j[,T����d5G�v$��q�Վ$6�nԎ�6� =��̻Q;��5�#�͑{�hn�n$�n�	�����/�D����`
Z_�[�t��`��j�$�j�T�tX�,j�����}B��T��MN��&�p���/ڻ����)�$oM��a_J���ǁ�Gѭ�5_^�(:����W�����E_�=!���tO�%�)-�4ҭ� [e�VA
��;���ұww���t�n���u��Rnם�k�]w�ʇ�Qrk/�H�ޥ�@E��ϒ�I�o9�T~�t��4�o�N�a����{u�C��ˑ��=�ꎆ������S�T�>�Ʋ��%[T�z�\5K�DBz���lQ��G����HX�a�Sw�6�HJ�R�8����g)�!�0���]{<y�W�� U����
oaUb�e�MX��o��V%��5Fmrz���v��������E�&�M�w��O���������N*n)�;���c�ս���yu��T��A���ս$�Sͳ��}��V_�}N$
W_'9�?���N�����<z�h���cB��{�Z�+d�l-#ٹ2	��������S|�W1�Nվ���k:��5�*��5�*���*�듢,��5�ְi?Zæ�%�_5���MKӪ��]=���k��h�+�iP�WSΰ+�i��W"&Ae�����7�{`�V�؈W��y쎃Z�;�J_���y5b���h��R�#�Pq_c�F ���� �4
��?���uNi<r���𺠞bǧܕ%P^T��*�&W��{_#J��h�� %g/C|�E0j4)���5��KD�M�%�����k�D4�4\"���KT����U��!������E��H<�i��|��j��ݺ��◴�2L�-�\3�t���p(�S�.4h"C��P��c)�\�Cb�+���޺�C
1W�sǐB�U{tj!�x)����|}���jc��r�3`O�{���Z�0�]�S�[W���4����h�(a��D'&i� `_�$͐,�k��deL�[�%n�Î��t�c�+v_G���hC�!+�+���{���\�@mŅ�oz-�G[|z�����-��,�}>�0����C?��m�j?(�O�C:�Ʌtd۟���~J���h��Bf	s'(���϶�?0d�ЩC�?(d�����C���9U'��	8�/_.G��'~_���9O��&�9� U�ݮ���dON�p�'^��;�N1�ϻ@m+������}H?{��a(���h[i)���h[i*����o���_~X�V�P3V �A�����s}���6b,pH��!��//pH�	�o<���\�f���On��t9��U��u�����qG�k�������!n2Ɲc�t��4��l��C�t*~��A$�;	��~����F�-6V�Q�H��C>��k���<�\ �k?^Y�@$�~��A#�~���,�aָ;��8K�l�]v~lt�E:����b��Vz׶��o�^�����zپ�DE��s��I��9Y� U�9]�0���R�ý>��J����@8N�I�H�6�p�r�r;?ɸ�&n`�e�M:��N��t�;*�?F;���;�r�`������y��tj�^�r�S���R:������)����l+#�M� �T�� G����-�v�@+�[��?L�- 9�4��N-����b�Х���S[��/0o큒�S{��;�ҩ]N?h����7�����3z\�uzZz`�NW5��%�
r�K�,�^i����tz�䊫�^���~5S���An	{�+��Ҽ�(Oe��,OeeI�jS���*\b�x��}
����!�g��X���n�R+'��J�9F�)�|��5�%]X�o���_\������T�=�G�O{���C��<bZ�L�41�:�A,�����<CL}e��i꫎�cb�e��g�Ik��~?�?�ԎYy�A�	�@.������
p��Bc|��D��� H�;2�*�H�#��-HE�=U,̽b��ʵG1���[ ���� ���l?�X^M�R�Q{h5`�cOŃ_:��J�c�����o����9�-��鰐C#�����~�|���x��f���=g�8)� B;m��nI��@3[�rN��2A�se���=�?���S���{�H)_�7�ǑR���A��9�����ו�Rh�ו�R� '�ϕ�-�7'`�Ȼ���Z��TӾR�e1�k]��M,g]7�-U�ȷ6�Ѝ|�l�>�	fl�>����lԱ�a�F&�����x/��Jc�;ݍ(x�kj�i�e�1~���*�
܊9T�=���/�u���͑����̳�b-2v�o~�"�G`�����	ik���qԷ��$�|�6�m�K��(iK�����M���~&��E:��n�Z-0N[0�B��X�mk=݉q�Y���8[)�A@�Ӷ��L�������غ�,�q�]�9+~��X� %�?��⤼l@�C얉w����wz� x�e�L�˄#�o � �6�P��bs'�`��8g:�5�ag���3u��Ǚ:PRgj�a5z w�����4��K��ا��ݏb�E�w?j��6��*�7�c#�#a��:X��d�ӭy��XilM�\�-�.�c�L=��47���nf����?j�l�C��-�q���M�"����$&`c�L��녑�N�M���X�%�}�&�ܾN��M}oTZ)DO3&���@K���h)���w����<,�t��dh��pyeăM�7�˶��O'�޹f ���#%Uc7�����t�D�"��G3,;��,�a�!���6�&Xv��,�`�Mg�˶�y�@i��c�W���^�0!�W�|tg� J��X� J�mB�R��~��b:T���L�J�#��M'<�R�.�:0򤝝<�N�(%�|�t��xaV��3z��q��0P@�T��b�Gvk�t�<��$�a�v��[9L�¼I�T$�R�8i�M�p'��t�D5����rⶍ��-T�]�'���R��n��\����]ڵrq�ra����?&��䉍�:��B���H��9=��q�����C��Ć�a�8��wF6ɛ���j�ԣe��lA6T�!�*;�	W���A��Aݞ5X�2�CV!s�+e��1����4.�щ�Me��:�<6���.H�� Q$�g�Ilc�-�\�{CC]�=k��4"�QP���\eԣ�!bl��2;!L1-���@�7�i�p���/�a�ؾz��P�&�#���(�3�m��J`V�(��?0Ծ!�MO�p5[�{�3�Ѹ�p|k)��\�������g�S�Yx�1���,����duy�2��t,:����w5��Icɣ����)��u���.{9�Ð��8��![�H(_Nw=��/ ��QP�l�A��A�Cn�У�G��lxY�����H�G�m96    ��e[;�5���H�u��z"�8n��0��z���4��_��G��0��:�>�=���Շv�_�>����+Z^>�ӶAT�R���:8]q'�j��ֵa��WvWe�ӡ�����c�6�x�ʍ��H����M�:���6Kk\g��vq�(�c���܄#=����)���_,����}{OM��CvDsl�5��t��l/��CV����7�f<���ld����L{ �A�֭/z|{|����T���i�
�C$��H"e�H"e-C�װ��;��a��b5��]����&�0=��I$����J;��ԦxFWr��aXsw�gø̈́ԇ23���t�Yc�ř� 9��.�s�����Í"'��N��ٮJ[9o�A:�ڦ�XF�,���3��m��(�^x|��nA�4���5���C�����T��FA�r4�ޚv,#�>[C�2r̲;��_�8�Y/}8��ӊ��>�{��c$ҁ(��c$�i�Ыu�t�6�W�"��}ܴO�v��!��)��%6,�?[\i<�9^䵃�~�"r!������W�-�g��]q%���7��y�t?L�ڳ����&����n���g˵I����F�9p����_��[�nc�N��2��<ҵL�kt��Z��5:Sm-S��H���%����y[��X0I��<|�\eZ�ct�Q���(�������p��n *̄�ervPz�D�����I%�oC����V?��Ws��l*�7����E Tˌ�qF�>�u7O�7�M����ߑ���rԷ8�m���8�Ί;([�6=ҏ;�6�b�#s�V<�㡱G�{4#������R�=:��׷��=��=s#,����2́xd��a蝆�M@�23q����-3�������"��9�,��t�t�IqF]\@4��/��8dol�C�q����%Ԏ�Y���r��yV7�8wuc��f'8ޓEK�#�noʉ����@��0]\��3�	tC�$�$�4历i�k���l=Ϙ{j/[σ�J���r,)4F��$�����$���{��i�������?��\b�5��^�_�i�K�5�ږ�jM���%u��wŵhL{K~�F ܒ]�xq�Z[��U�h���g,���j��moͭ�L�l����{���7�VԶ2Gau��-�:��#M�\�#M��E�#�����^�b�Ƅ�^�ｮ%u�/s$h#�r'~�
b�m@���a�}ҁ�J�h����cBmg��D��$�0�$���'��×�8j�5+��Q�$S�)j���󁌾�2�q�w�8���^s�@F����v�c�tp��y'*[�\�«E�j��(1��fԵRj������T�
oy69U�[F�S��tE��Qڝ_�<U�.ua|�C��ʜ HT��iZ��z퇛�,E����I�OӚ%���,�u�-����a�'R:XS{�����(`���ȥ�TF�ӥߴ2Ğ.��5]���o
wj8�8B���ӥ甇㗾]'���8�H(d&�Θ��<n�x#��N-�o�n�<�
$�':��Mb�:�;��#=v��D�kR���9^�z#{LȽ�����F �'�K�F �J�]>�	���Q�6f����~��b��cK	
RU~�3��?�c���*/B�(x�x�XoCH�G��GCH[	�]�!g�3�����
�F9}�$�7`b���UY���]xY)�X��(���SA���5	���T���ªRa�k*]ycIY�3!��[�2��;�H���^y�5߅B���ەg��c���3��q@@:L�QG6��'���_\&�����&4߈�N����lz���<#�&���cp�n ��OTF6mw0ӽ ���~���%�����)[�=�I|3�a�xO&�~Mh��}Z�s������u.��c�e�k)��b�nr1ϻ��5�_;��.~��SJ���}���˒����t���Sܨ��C�wGwL^h�t�˘����.�7�?}�2��0g�|��}�m�I3wG�q��:�/,[-������(�����6���Ư�ST�M��P������-�k@���
ޙ����*���̀�M�ҭ��̄��#��p��mu��ηoE���	Z���H�I���K�n�eDԩ]�[�6%8�h�]�E��u��Lq��&h�E56�KqvO.��&b/.���^aL?��	*W�Tߵ�x��(#���Q�*3���}��	�$�4}F��+0���� I��0��$���1��$���Z]n*b�l+\�L�Ay��z�_Z�>��2�#����8�0ͮ{���@��-<^���c��D�4��1��N;�ֳe��a�'L�A�|m��Z�$�mar���N��@bC�Y⑲���G��`{Kp��ܶL�#�w��!����9I����8Hb����c��y�$��̱:�I@%u�e��q����e��q��З�F' ��q�^���N��=��S{����2j�th�[�K��~�M�#NY9���(`򊽍%��6�k���;~r��'��i�<��;^�4����61��&<&��d�����W^_�0V��d��@�2R�k�YY�@YFK}��GA�2V�+�7ؖ�Jml>�2fjs)��1n�+'�e�t�YD*�xU!�4{3z�+�W��i�Թ<���ͦT�� θ�?�]��e���ƆI� �X�N�b�i�z�E���(J7c��1uBpSp3y}j�"k�-��U�v���2~�Dd���eչ�������<����#�,x����񻾰�;O,���#�����ɋ�n�2���mj��6T���3'���Tǆ���2���f���2�갴��e,�a�V�y >��n'�i;�?4�+�����H�4�.[�77�o5�e�ԹcD�}���a}�J��S�[�1��+;���8�����+7՜����j�؅��1��&3����9^�z���I'�3�#
��fm�Ʀ�Q�x����J]P�,�n��9v�N{���v1'Oo=����}8%����C��O*�M����qS�}2�9u�g�}�GV�p�W��/?}�gW���܊��W��;7E���2~�X~K�b�k�ݫ-����&�6?����c񑅝�����W.�.�l��*;��dw�PW�Yw��YhW�8
�c��e��J�9����#��/��_��;{�<�K7�R����:c7��ĕ�dT<��	����;�=m���֐L����xqX�|~t�k�3<���>j�^�4�Zf����:�2�f\$�@�e��'~s�k�w\$�Ր/�yM'\M�1�#�䆮���gm������k���S���t,�O�f^�?u�y�����Ǫ�#�fRuE5C�1Tǖ�	���:v
.A��:V��A�0�}5�h~4�h}4��Hgo�`t>+U���,k.#�N�RӑKu��_;�1��pQ�ZFTn���[��g�@ub�e,��*�hu@D7�戈gh���s�zjc�^��Wmr�����:�Y�\�P��&������(�#�h�3�'�S �s��J�K�<�Mu/[P[,�Μ��<>�4J]�'Ϡ�[
Ȩ�3��2�`\ՙ��v�U#e��pɭ�9#��y�(?�u�fdg>���~�K������u�+�<;aI���v	ZޤFq��V�jd���qUO���ye폊��D�ų_UN�2|56=���V|yv���v0�̦�̡�Hɒ�μ���j�V���:��1)���<�h�}R��M�<��M�9�/�͢)�u�hJ�_�M1���ES<m�ߝ��e;�A�G�,yO~�.ۋ������7�5��!�qO�t��g����T�h�x4��3kD֙����<��ɩ`��T��������Ϧ��t���=wO��u��l�\�by�l�[Fb� ���g��:<�,��X�'��uz�NO`<��	��/�N7�F�e4�i�=�x�c�-jlrb���eL֡�e�eL֡�l�^�����ح���}�Պ�i�l�%��|6B��Y���Ȩ�c��Wp���	ŉ�R}�+�t-\�ʕ���_cW�F%o��zi{�9�����Fa���ÒQX�rk    ��/c��]QYlQb[�F�M�bl�&���'�֤0wVk�J�ʋk,�c���%ع�Z��D	6m�(i�Fbn�$�5�pOEtص4!�;L�k����$i4�'	��M
��K�g4���Z��j]���#������@J�P���*q�B %r��V�c %�|�` %���y���z��ghմfM�h���k�e#�N�Ϗ	/#�N�����>� K	<!-4�A�9Nb���u�O�	)Z �wO�jS0{�V��6��W�D����T�N�$:�q*�*m��t��^�T�6�W�TŶ�ʖ*����jK��5��g�
��s������C�~�1��7�%�k�J��N��$e(D��j�ah��4b�)�cR�[7p�ɯ�u��Եu߆��G6�Jzd��֧�5MnI��kY/�[/��5�s�$L����Ak1�8FJ�^�:Z�A��3ʑ����;L"��%���'С</-:��\�;����#����{?RRcv�!GUyvq�N���%���ދ4)�m�'���lݙ��{+����{�$a=�\O�΃������݁R�:�V��Q�����
�4�8�M���j��&�vW�c�u���a���VWƽ�]H�}��ߜ������HBˍǷ�Ϝx��	W6Zt�JB�qk_�O{�%���y����{��س��K.������pI�8�Q�ضd;^J����}H�`Sd4"��m���
�(ӣ��ܐw�V:��lC��k������j�^ִ�?��
����p)ݩ���`�nRiZYٴ҇�ѭ��}H�l�D'���IX^�����d��S�0��lQ�L�҇�Ȩק�\ا��Ȩק�Y���Ҥ�w���z�A���z�vd��O}�����g���<˽8�(՗�YFZϒ<�H+��(ӣ�Z6ť�r�P6����e��'��=/*4�9PҼ�[Y�_�3P=�I�#��8V�D���mۚ�2�ڛ�x��y�6���%�2�v��YF@ž%�2�P޷�YF�¾%�2����L�`��kK�`du�$��Î{���~$u6$��A+ֱ'����I�O��I�&�&P|�Ĝ�x�	�t�kNRR�5ɱK��K�P���$"�5�;���$	���K<�� <"�$���\�@Y�^�n�X�0���&fExdg�-��כ�{�"�gķ�9�b�I8�!_���(Z��1�լ�vT-]�������a|Fkj��q�#p�Ǐ��I�L	��Կb>K̟�u8P�-8-�* x��(�i_��A�!dF���lݲ��ѩ[��|QX)�.��}k�2b�~�A��n_F�b������o�͆ܯ#�����kV���󾵳�l��/4-[?��f�G���t��/p	�����|n6A�H�i��z�Gx�lc<z�$o�ح�W|E��y��l`�����fg�p�=��yNd�ܺ��z~f8��Ԡ	���;H�8���!����J��$|Fj�o��"L�b�o�Fm����-l�X��[.&�7�;.��6Z�~�\r1f�~��܀#�����ipb5�.���l����w1˟�P[�W��rx�.���3���ܓƮ3xT2�iJ0hT~"��0����u��.�]�m�mȎ��nQ.0��{E� 2�R�	����`]?MB�.�%� x!��n<��zS".�W�&Ėپx�{������k�Q[��<�q�WkTi;���_�����~�����8����؎9���+_�O0X����MX����d֯����6��^]���^}9�̿����/�����}�|P�>H5��܇y����b���d!���崝�g��:��vn�WG���<KM.�#���ާ�k
��G����:Ԡ�O��.����ヽ��;��M���=~R�-PW?�,�ג���0�[��])1`�~�Rބ��@�X�O�ēQ]߿.b���7����a���ޞ�ԺgZ!"��[%���>4W?ob���٢��G�Hm��L�!W�X�|��� $kl;������V{H~��{i��^�
3�KΖ����t$\rxKG@2�5�=�����tTߛ�Z)�x���U��(b�����Z��Zf~�=G3w4���=uG�x�@^����iŧ"0)~��}���t,D~A�N�]k���w�	)⚎���˸�8���勐�bM�s����� [�+^�ؿ����I]U�?�����:�_Viu1��'�Z?�3Δwځ�@l]ߗ�h���A~���j]_x6�!��jy�����qn�պj_�{��c�a���u���-<�׋&2��r�@�-��Lv�b��^*�� ��D�������d'j	ʶ������]�pP}�͇Ƿ����#`���G�����S]^ZZ�v��v8��K{��./M�����,����J��nB�,�����Y���6�����)�J��K F�"7t �Ĉ=vम��F`���X�X��Z�������Ki�@O]���B��%)�SWm��\jCe�(��+�2�C��,�5��n� �[�Pw�b�!?"m~�.�v���ic�UYِ5Bwr�-�SmNt������N]^���`t�>�0g���N�[����x����z
�U]~^t �.o��&6�.6�!6�)6�%��-�d�4ة���~�����H��R��2��`Mb��W���a�Z�.�:�U]�
{k,.6��@W]��39�h�18
���:&-��hL2O5�(��zf���6,�V�ZW����!��i1��xvKd*�@XqY�UW�P��*t<%c��&-,a	��-r>L�SBsk����aM��1�%Ψ 5,�)�!ZS4C��L��!�B���������V��5�js!S�ٖ���t\�����J�U�aȚ���'L�P���9����B]$��2: a���r@�&�[�1�Cl�%���	-P�z��#$�l�'j.`��/�uU�&�T��%�'d[�T����VnG&�uͤ�`���tU_�L���骮���.'yk;>I��������緘,;��C�����c;F����y����8LѠe��y ۙ �U�u,�x�y�1LeՏ�`rɒ����G���$�;�W�$G�Yl���꟫H��~x�nxG��I*��?n�i�"���;5I��ʧ_s�^�z��	��b���)�Ԙg؞�y���(9Z�ݞac�	��E[�����CN�TV�r:ny�5��<�([�-3���/�O�+�A�n!�%}9�%��O���#`5?�q{�EZJ},���e(Y�m�³/r���0C��kk�EP,h��(�*Š�~���Kr���g�������co�U����?����>b��b=b�-���~~���*6޹=�&�{n��Ŗ;7�RG��2֑��/3��I�YM��[쀾k�~ưA6���Y�4��~��bS�RT�C$����Oƿ��(���<���	#���X��QO� g�1O���#����G\@�8E\ �E�}:}@�B��O��w����>���|Z���O����O�.���1Pw�L���Si�7�~�=�<�P���}�XҤ8�4K��ҙu�ˡ�t�\��5c��O�jt��t���60���5�اs�\=c��]Ps��}:5\MØ~i�=x���#�%,;^����j>��F?�@e^��lNQǔ������hc��Dy�%p;ɀ�������:�x�S5�_����i*�jᱤ�:�x=:��;t�o�&%��q�$W�i�P�a�Zl�Z��yS��i��i�U�2�����g�%��Q�G75�G��u����D���a�BC��@FA�ϟ�$Ǒ��~��#��}��HI�LŏNZ�k�%�|Y��cj��<���_��G��h��ѡu�_�:�ɉZUZ�	��H�4���G�����gH�Q���D,q����g:� 8S�d
���q��~�7q�5��0�{��8R�KO��|j�x�1V�U��X��)2����p�ϒݻ��S�wݰ��2���ҹ�rF=    ����ǫTyg��a�}:�.������0��w2�L�K&�!S\�X��WDLa[�4��}�fe��Xy��`Z�8ll�����<Ҿ�Uڲ�y�������[ ��.f�9��'���&�?���u�n�t�9U�mo\�̔�=sg�%NcrC�-Nf��uJ��������n0�~��L�y�����6#JC�ah�y�'ԗu�\zBs���HO�/k1��i�0[���m���<:����v?���U&���SAl?��'zy�{o?����cu���S�6�������:l>{?��P���ة��6�l��Ǎ��P�q�K���▶��~���m{�	�4>��D��Z{+�jw�|J���;��r�[���I�9��0���&� �}h?�L���Ŵ��$�>L�H��H�O��
�Hrgk�g��c��O�V��xv����z��DF���l>��>���)%=�A.)�Y��tɵ�TpWC����:��Zꠥv���q�z>�|�w�1r���R�ӣAc?�؜O�۷q��;ؾ-w~�3ؿ�7�l߾���3�L3
0��iF���8Xz�Uڏ����+:� 7ha��#�L�S۷�mD{ g�v^���wD�N�o�Š�L�oC�/:)���O�������[nxK�-#K�Y�m��[�f�~��[F����4�2"}���ٖ��L:Ӂr�~�Z���;��ǩs����3x�$��SlN╚�?�2�iɐ�Lb<�IJb�ǆ	�ĨoS�����"����SҐ�GӾ���SJ�cf���~%���J;F��!��Qg4�l���:�ˤ���m��	�7���т�+ ���@�&��n�P������`�X�8Z�	&ʣ��u�G+�g.���#�7�`��H�b�(�Nz]��ųI�+��i��Se4�`Z���Ό��+n�R��Wۤ�b/�IK�.��z9���O~�9[$v�oԿ	�v���k��bP���v��K��4b��������GZ�v�/�O��~����{�����_�?*��Iu�����]�#��¶���md�w��	������jj�������|�Ǆ��i�i�~�9d��iv�a��W]�y��oǕ��;.��Q�u����:^L�c[ʿPτ�X3ֲL7����N���r��a����m��y�����/&�]�����l�z�j�J��\��K�V�
~os�tq��n�ٱa���ƻ�%�NſS��`����٢����m�$��t��	�ا��)~E ��ܲ1��#��s�G֟���S����>��m��v����|W�6��N��00�Og��t{��J����mQ������T��5>U?T�ĶC�ŶE����WX��Կ>�m��K|\<��_�?S/q��|9b�50q9b�5���%F��.~o�)U�N�B��D���~��Oqη��B�c�l�3��zW�����gJu�����A�4�j�����k��;:Wܺz��F�b��-z�N�g���%�.d�N���[t8!^����&�䶑E���(ulj����i�
�W��ݒ�d��u_�\+��;�Rxz��u#=w���"��>3����*�1/�nD7�V� �nZ��q���)�
ْy��o�W<{d�]������i����kC9tIb4d��:�7ZJvu���<L:i�-vu��b�	7m��'�:z��&��[�fW�//#�6k�'A ݿ%�]��T���>%�ųK����nb��#m0?�wui�����K��]���ƣҜ�]�!��-t�:���c�͹2�U2�&\b˕>�q��B�̡Om�ɰw�Ƒ��(f�Lw�LU�έ�FT��]�� zn��43�m֓vu�"����8bQss�"ts6�C��ò8�9�Ǜ�EM;�Q"��?���dH]z�X�5�Ŀ��~�����hr��G�_|�>z�N�9Sl�-۔�T�:�L1`�[�(@�Կe�NF��5���lݙgX�9w)ރιK	�?�g|�>4dMvrȺ�uQ�ш7�]k��9����C�(-@�%�,����,���������=�{��l݆LC���eų�VB�h�����ܤ�SMzh���A�ܤ&d>d�MkB�W�(?�[Q�����H� O�:+q^�"削�] ��˖�2=�>�#Qǟ�E��ud�m�W��><�BW�M�uV����Z�
+������1˧���c�A|hѨ��m]����V�LZ9�̀6�/�SPkr���hk���?�߭������LMoȏYj���6dE�Z�>�.]�m��!�\瑦�_u�z΅�..s����0�Z[�Â�=���I��]�P�a�I(3�`an�j�����Es́�7І:�xv�]�|
~n��d�E�%�6��Hx��r�� S����v��)�)Y����)9	�w,��)9�!mjNb�}8l��b���n�[b|R��r��G��J`���@v� .������9����Z �	�����9zQ޹�"�"ѺE�e���32�H�l}���������xV=K������t!5Ϲ�aV;����lS��T����F�1p����-���/<��_v4�j&>d
6����L<��Y��x��bI�L�h������X���A������,A������#�Q	�E���pK��lB�QzѴ��w���h�_��������M��3ؙ��4�|�5@�\�c�&�W[F�W��U���l��2��M��\^���EႥ��WR�YG��wď�p��[��~I\�B�P�(�' %W-�V{�Kur6���_4{��m0%ǭI�b���;��m���\_�<�q1d~�n��.�d����U����q;\�=n�/���m��OȦ�'dK��-K�#�t�xd[���nBV�DV��h4����qySV����yYOZꖍ���0R!ߴJv��!��3��,���,�K����ָ ��RM�=�&�ri�L�4�֬�	v��uzq<��y=�΄�)����>ąƛ���K������ctZ.�J���Ep����X��Dv���ߔL�����w2?a���~-�md&R������E�25��Y��e��e�m�!;��7��]��_�#7��ҏ�4&L�~?�C���_�#��5����5쇳=q���*q��a�| ؋������:}����飙�юs^`-.��i�A?3�6�_S�n0Q'&��e��n8.y�n�%�[�Q�B�Ip�p`���Ñ�B�i����s@E�n$�E�n��a5;o]�?lNR��#�ɫwk6��������%#��;��({�ÓO��Ê��)�2e�������,�k�ʊX�'=�:�x�qW�6���G�ф��_��#�ȧ�p&�l܀%b{#`�x��d�L�3BV.��	\Y;��ȐM�������I�m&ۅ�3���!��H��bg:����3���H'�!�����69G(�ڹ񛃫��7��C6a�d�L6a�d�l�&LY��OY�Ƽ2�V�b���ȶyf���j��!�K�ޞ�-N�=���3���؄'#H4ﱈ���zA^����Q8��1�X���?�e�L�m�2��(�քs
��lw�;0Oۋ9!��nnFդS��ل5�D0C��N�LX�c!↻<$�طATS��mǤ�D�p�.(Hk����&GǕ�=�E���0�IEo��t��
r���.�U>_���U�\={D�G��Wz��� ʈj��g=��ie�ѣ"�3�S���~�f�����z�+{j�I�Ŧ>��ZTZ5�d���q�G�]�ho�M��뼦�ؐ_ڄ���-o:��z��6���6ӦL�iS&���-Zq�6�8e�VL/7��3��U�B*�R%rS�J�k�3��ɬ�J)ӃvȖDؐ��<�,���1u611���m^M;���HP�B�lL�ɹm�v4#)2�Y}��i!d�E�f\�5��"�y;S���>�>*'ѝ9���D#!}:����tf0�
ݹ�@"d�6��z�����9y�xB��UB��i� d�<�&B�Ƀ��U�2C�y'��}�`�È��    vF�{;F�R����zƜ��9�͝�!��E�9����S�8E*d�'3s
'���t2)�M�0D���\�48q̞J�w؁2�$M�Ƕ�4�x~��{S^����	�d�6��p�+	����C�r�^�+	}�4�C����ESsKv�D��r�!c�����d��s'h���-��#�Iѧ�����Ó&�}������-���G�����9���9�L9C���!SN��Cb������ͥ�=��]f�ӈŌf_nߦ�
Մ�
��f��R�6!T(Ȩ]v�F>��۰�߷�P᠋���6�)Lc#ؗ޷)�=WxrXkʹ�M��})n_z�F|`_��F��Lj/'
�����-�K�{�����NH.��zيg3.�r�8:�;M�wm_4&�4q͇�>���ܾ����(z�}����_<R'�CtSo�}.�o=�7��Է����������O�v�ľ��4�w�WT͙��+���m���Ц`4{I�ui�[Jc����.m�rl�T���VQ�6M���٥����׸=�����^�.�ImҽR��l��&�}cߴ�[��7�����Hj&/�o#�\,n_iŷ'ԍ��4�f�Ʉ'm�q+�'m���FIs�,��}���a�ƍ!:�ۨ� ����߾��E߃�<�x�s����[�6�B��koMN[��6��Z���#��nԣ�f���v}a�/��j�#�gδ�Fo:�چh��M�nת�8��ߣ�[�zh�-��*9as�v����{���rV˗F@�X=uV�`t}_2tt�|��P���˷���~�J��)��<&��ח����*��e�`�� �VT�l��/o/�o���堸��-f�T�=k�K�K]��"�ʥ.y��K]r�쓺,i1���۰���4ºt���M���:�P�@�2�ާB`[�W�2��-I,�±uc�
�d��Y"WرE�O��a/�oݙق.�oݙĚv��L�ug���e�����q��d�W;���[M�]J�1���^i���7[9lÓbq�;1	��r��!�X\�a�p ��ۡ�w�F s�+��v�B�7�5���b��v�B"8�}��J�_��&���'�>�%}�%��`Y"�]��t�&�~�����"t����� �`y�и��K������qf�u�}#����a�)`��*��v�"ʴ�s�"Z�Ӎ�y��S��l����K�I�P2�]N߄����<�ML���R��7�������K���d0aM�P��7�	��1����p�|������"�˼�B�@�uwٙ#��H��(E�	�P�)��6!�ܱ&7���s���iQ�َT�4{�.�o�r\
�Jj?�l�����{�D����Aar+O�����d��
�P99*��C��jK=Wd����3Gb���y�~\�[3K7�f�$\���|
�)+����_4�[n�a��z:��P�F:�q��?G�22�_����#1��͝<l'��C�>~Lg�I�i5�0��D�%���h��J�Ҹ��Kћ3������f�.Co�쎁g���]f���c��\r��+���<��� �2���<l]b�4ޙ���˛�;� O��N�4��mx&�w������Ҽ�v9yӷL��srgО.#o{��l]>�����VB kc������f�I~�Kƛ1��l�s��Kƛ1gf��\�s������ ��J�<y�\D��B˞HT�״�	r ^�vL�.첚&i�3��lo81������?�G=EN�$�9UR#���b�=-�T��nd�U�
{���Y�T �	6ɭ�A�E���^�{8��)a ǋ�u��w�>�L\��N�����Ӎk}�,_��s��x� �ȥz��gy��wF�w�Z����H������a78��$L03O�H:Ǥ�~G�l{���"9l�.)��nn#�)���r�Fh�i8�)jm0�2ؠ�Y�nq��qM��A㓁i|=ϼg��:�py�����9��S�| \"�9�8 .a�~l> .�pB@.�B)���>v�tG�ܒaقߙ���9�a!����'O�0���8�Aǲj����y�n�:]�Jmz���� �,��{������nbܝ�G��8nyr�gK��<��ے|�G.an���g�K��F�3�m����ܒ|�	⏃z{?G/t��/�1�C���g�Y8%7\6L_���%aeSC�E�^��<��@d���i3df��Ŧ:�ZW�p�R�ޟ|�ٛlYҼ���� �"�w'w�f��(�(R��ߍg���r�	�Av�Xo��41��Ҁ!���^��ua��fl�+�5{5A�x��d�ϕ�M\2��&	q��l�Y�4�sip=�g�sYp=����g����Vd��Υ����Ĺ��v���\\O}��\���|�8�m�Z3��w n���;�+��F�^S�#S�>9[�:#-�'%[���˳��b[&�5���ö�S�?{��lF�\�ۜ�Ѹ̓��:�<#�|.�m���soSs�Y6�h632���ݦ�Nn�vh���C��U���K��3�L�a�+�aoi�q��\���+������\���E3r��R�Ƣ��v,�(��
��^Ȳ�*t�􆪹a麟��GK��
=�G��12�}7��LÞ���Ұ�nǹԶa�q9��6�:N"��چYO3kl�YҬ3�r.�m�o��ϥ����߶{��rچ�[t#��2�d�fsRk�0�X_O��b}}��_��k�i<�p�;pU�u�Z�F�l�`ڢ��-�s��Z� �����kQ}���-q|T���ͽ6B�~r�{:�]�Py�]st�3�K`��[��\��p���.�qs�I�Ӹ����F��M�n2�C�Ө����4j��	σ�P�Oz�1Z��8��K\>�g�<#7mO93�B9+�B���y Xx��� �p1��<4���SZ^�T�)-�GZ� �d���հ��) +�vHg��Ż�M�R����#0� ��dd^
r��l)��{ii�b�/+mZl���J�Kq�i�bf��M��9�;_z�t(	.9ml���S�2�ٹ����q�ꔚ=؊��R����S*�yXF�De9Ϗ���r�=����<0>v<�z�Eϥ�M��:��6�ޡ4��t\�ъ�9ӥ����ю;hE��Mh%�_�0q�iӅ�^z��j,
��Pv��Czr�/x�KQ���/T�Kη2�J�����*�"���wnړa��8�r��.Gm�ԉ�-n�C�;V9*H�V����X%Z��ND���gj��w�y���\��� '=�婍�8t`C�����=7�y����S���0S�lϝ��R����ܞ��������ܴ�V�:V�omNz���Lz��K� �d?��V
�Jl Gb��6�f�&���6����۹�hN��NAG��J����܊�Y�C�ȵcr�Zx0D�u\�~B��y������&���l�8E 
����(�~�s)i�n�a�{�ݺ�0��+��7���N�UG�O�(;y�%����&-�!���dV�[ָ��}D�U�bL42��&����s�H��Ÿš}E< �������S=�B�MZj�+ �z�f�F[�������v�ac�8�����5�^������ć�ޜ7 K\��v���"'��a��?lSĴ��ɜZhw�vW�na�W$W"T9j����e���ʉ?���8�#�w*+�+�F�Ԋ�c�=�CxN�:d�W�9d)�bW9,�S����:9f)�����+�y�<������n�ײl%��i/�jF����r�f,^�x.'mB�eH�������SWRy�r��zr%�l�
��G�M�+�U�|�uI>���$V�%��2p�.ɇ�`�:f���yw�AY�.6�2�^nڴ�E��e�a�e���	�6A�X2�٥�S\�YO�\j��4�hr*�+
�l� �Y�?�+yO��o�4�68���yL� -�[�V��,��p:YB(n�:�]��d[k ECC�h�!� -�C���∆A�v�*&D������"6!��;kާ.&ݩ�Ť    �F쫋w���/l����%)�M
�"�{`�&���s*K����U�Ay�a��%o��$7;����Tt�f��}v�AU���6ĭU=
��n9��Q1떃���Ʒ��j���f���߸����,�U+����,U�y����R�-�r�^��B;��}��
?��KEۊf*�coK�m.흖�W5>�s�h[��3��E۪�	Lm����W}���&j��x;Q�4���G?���J��n�D�~>l��J{_%�v�����J�<��mB���{�O��\>�V�Uk��_)4V�l��je��|�m.m��j�T��~���Ѷ"��m�޶[r�ش�������L����c^��f���D�c��R��"[U�6ե�M�f��2��F��e�m�[�$�i-L�_ڴۙ&w)h�ng���M��i���6���A�v;�۠�fkԹ$��X��AV�
Q.m�j���U�'ج�-�h��e�m�FoK�6�KG��K}x�'��W�fz�Ub�'Z�w�fz�T��i��,�Z5�j��b[�Qh��~;�~qԴ�,�\>ڰ��_9����O����'<&=��o���K�|鱩8�<L�Cӄ=<i=�ؗ�6-�'H����ؖ�l6�l\��c�?;W�g-��цGf��r����Rr�hs�;zn�sψu�hSq��[W����D�+�i>�r�;�g5*+�6�s���B<2D�Qkf�뙪���x�v����t<-���6�L��쇶��-��O��v��i�cv�`۽�p�h�Θ����&��I��\�@��s.!m �0���`��e�� �B�(�vhyYui��ǟ:��,5 �WK�i ,�@`a��|��L�5���H�\�V$[��2Ѧ���п�v�2Ѷ"�����iã�f��MXQhc�I��;�ʧts�h�B	/m+��f�����E�&�����n��6Y���+�M}r�+��\\y����"�HwB����KB����R�&�@m�&Q-9�7�
��ވw: ��;<�i�=�vw�"n�&혅n�&���/m�R���%�v���h�Vݥ��M/B�͓p�2ж"yK��cO ��?�;�Ӟ�|6��{i����~=qn����.m��{�;Xa�k�`�9Kl G+�YN�V��lD
�*�rP�A$�p�W$�	�u�������.5\n�\НY�KO��3��}����ѐ��H�9w"��P��sssF�c���%����i�c��u�Cg�3>_r�t-;�W��6��9�7������UXﲗ��X��@+���=Vbs͙q&X_�%��8#?�9���LvbV(	�{�d��@c_iq��t �ט0÷�k��W��N*���	5l��',���
�ųx���fQ�����f�����/m���ژ��u�dc���Eۊ�R�S�e�͝p�o���A<���+	x��%�]bX=뛧�üUGV�S��H���Am�'��~�$�6&j1�!�D 6������J�f,��,��8��i�'��%�M3>��*������� Pa��<��V]r�V%wj�=���spg�\vڰ7���t�g ��δ���V�j��Q�VcZ����-��!���+K�E�5Ť=_x0�(Rt����"E�)�ȩ`E�g9��0 SXK�-8���`^|T9�� ��6#�J��D��|;�.m+���4*=+���m��[�e�mE�f��rr��9FU��#��0�&Y��qn4I:���&I�P���ʤ�h�t�yH�ae�`8T�Z��hb|+�o41�E��b|���`E��`E1�n� ��p��	x�S+�;��ىa��V)� �h����g�f �d��=��^�3�V��f h �H��b���*Ng�{�?sb��|+�c��?usUQO�;H�]�gJ�]Vu�)hqAfe�^�/om���=�9&��Y����,}��|��o� gc`J�fL&�Yn +_��"�/�4�^��)vj�+2�Mv����|�kҙ6��ٮez����e���KW���xnι�9x���j~���+1�܅���=/�����W����r�ĶA�\�=鹷܄��arr�di�	�1�i���w:�^��6/�����C*�l�$�q��pE�\'a��]f9w ����T�����tw�;�M��;q�Yܝ�IM�gwg�g�B3ώ���#�w� /Ym^Z3�x��ۭ�>��L�ќj��+o_��&n3����?�=qf�y=�|�-��:qz��dx'C���i�`>��֧�s�e��iف�D+���w`qt����@��9�_a�_�t�$O$�&��_~r��/���0FOõ�1��H�5~D��7��/�V�(���.��;9C]��
����\s	ls���.}m.��b62��������H�Y�F���r6�@ �bBr@�fo����لmc��&�	��a�'�[�fq�&�4g�<��h·��w��
'�B�Ü��B�G�5��%�Xo��̎?���wP-�9H���7�n��M\�m��v�"�99(����=�Ņ��ޮ	�8v����7|��܃��R����V|.�m*͚�l�S���r�ms��A4��eq�A��i˺Զ��C�W�-�c�q�YP���'zB&.2��{��/�2iM��8NPŉ�.LPŉ�~@,��Pw9mEd�%�M���J ZDs�[�:�B�#�t�B\
�9p!.m7♕i����i�.s��!\��]$*cޛ�i�����9v��>�.�!��6c�d.�mjP'D��	*�w�<�ܙM��<�[b�!��>d��f����rg&a=t/;��r�ZJ��B����ܪ�L%w���w��z��4,9�nB�@p҅,��v����<��E���6,�'�U;y�X@.�MS�K�p-���� ]�����	X^I�Ҹ?C�JO9˭��C�^x˥A���\S�(���*��5����n�<0���f;�U�ܬ�.�,A@�@�e���S�7f7k���$�Y3U��$8��w��5IpV;s�&�͚g5�n�L�,$X�K�f����ȇ���.]5s}��+�C�/"����}��
5�!z�k���
'gjCj�j;xt�\������c&ޝGڞF�J�.l]+�:���L+AI��јV��Ÿ���V����6�$M�YC*�5*wkHW�-]��e�4t�|������֔<g�1W�qőe�4:,O�H��i�[X�	��qJ�_�%}l�AV<OZq��%�+n��UҊ��1�V%�[-�ْ��]Z�ԅ�з�d-I���u�%�k]R��YR̫��-�<��-�<�:x��b^�|�ڬ��vϔW��%���W˰}	næ��U/�m��+�!��f�Csf׳��ᑆ*Y�#U-ŗ�6�[�2��d>�0�V��c3�o%�����)9�`�$���0�:$���l|?$��h��f=�u���&ny4�s��Kd"�gG gn�DFx?�m���͒���7K&����f�~��s+���g�.T��K��/q���w!Xn���l �?:+��T��~���q��p��e=�&Q����d緕t�eНߖe惇3	!��.��Z�m;���@�B���z�ĄLBHV�)n5��!N�mM8�F�곛�]��M�Έ�;�f�2i��n�J\��&��`O�XZ�K\�MZ�K��خ�7 7���ݥ�/)u���F�L^�cD�/ʹK7qI}�n�C�t-��6]t�R�q݆�.���� �K��붒��Dě\���lR��=��W�r���J&����^�������9����DCjz%�n{H'1j+;��X^zH'�2s�(��)�Ŀ��gO�3����~Fz��~F��9:�<_�Ɇ�<+�)l��0��;�-Kc��6/���C�<�7�w���ͳ��W��e������v�l�6Y�2^1�xInk�u:m�{��Z�a^)x_)�LHϕ�ϳ7���.����6>�g���U�����n<�M;��귽��:�幭M��-�yn��Ή�c�L��mB}����Q/���T�Q-Y{�n�o&�s�n���!���W*D"��+����E,�E�c�a��;��ދ���m���^��+e;*t����b�帽    ba)&����yS����W��01���Y�m��>6V��|��	N���u��	� ��M�*���z����M#��Jr�]�۴��6t�nÈ3�}�nÈ��e�#����m���%�^�db,���ic$�=o?��?V7�������@w|�q��\6�m��͊�j���61����U{G�v��X�tt6D�+�K\&�laMÞY4����>��6;��Q�v�g��2܆a�l�6���!W?Fq����NN8;�/�m8�����q/���.�mj�7�8K�e�M�#Qͥ�M���^��ic4v|�\h��\��\hcs���e�M����p�;��\�z`,�]�#��FK�%��e��u�y��R��:��R��2�l�4�ۖ
�'���/��mc��<�W��k�Ĕ6+]����k�ڙ�6��Rcs���*�LW��@��6mb$�MT���t6��e�M�fɷ��c �\��WfQҧ�"�,	y�Y���hr6j��&�i�KL���Y����E���Y"�ƻ9da��R8f	c��~�Xx{�d#5F±�l�Ƃ �[4����6��ۤ�6KhcԶ�+l�Sh��O�u�m�N�=�8Ƹm���Q�6!N�q�L�2���G,4�_8����X^~��C�)_q%|pv���!\�w����-:��X g��+^4�pxW�i��o~�I����*O����+,i���+��F��_#H���1,d�pp�G���<�?�xq_*xEymLz�m�S������}{��ZiR�<vō�Y��t��}���x��Nj��6~S�
W�q�<-؅�+�Q�Ӽږ`w���*�{8W*���lȕ6.�qOvŲ57�f���S�Mv�v���-���y��&x�ܙ'7a��g�/�^��ܰ��u�5]��t㕶tI�u�t-v���9������ }2�Jw�]W�Xw�\���W��<hl�j���Ѩ1���q�-&��<��T�x]!�l`W6S�H�M�����*B��)�9�n��؄IdX�,i���u�5M�<�0���&���4U�t�I�Ơ'�� �M�v�;�L���]ብ�n��F����#�q��+�c����+m�0ǻ-��3�F�5a�&�OF:?�u~cӌ�x�1���?h�B+Ƴ�w�����v�����Nў�+��8 �z`B9���l�<l�m������>R|����V��
C�����h5�S�;��W�T��<���#vU�m�P��ʤ�*[����R5�� �(�V��8Ba�Ӄ�;�/>+�M�xf4[���j�^�[&ތF�Ny�Գp������l'��a�Jh���@����+�D��"�*�H�A�t�h�H�aY��t5T�nX�o��YT�������?�
�� 4��~�o���j ����+��#�`��������^���"�"�A, �m{����2aK���!l3`v]��17l� �H���_/�����
;�&�t$1&�����T�d��<�P�3��������8T:O:�~u�3�5P�+�ꢼδ��W�d�w7��m��� �@�ɪ�^�����A�ݴ^'o?_�|"EA���'���ۍ��l��S�qA�~�Vi� w8m`�����\�FY��E(�[�x��P���qؖW��Mp��<��I{��6�Ƕ~��b��mm.��.\��9f`Ts8������(�B��9�}�^���_x-�<�){/mj�noC���d7 ��T�Q�����e��X���@lK��a۲�L�-ZB���dۢ�[8�3��p��n�{9��c�{��N�[�`W�������5[%*�)[:�`h,��-��[��cz8��N�.{�����١�����J�}ۀFd[�t1M�r�8�������<�#�O"&�i�r�0��:q�\��mⓖ[�4c�-/��L\�F���hl��0vi�;�:Ε��I��q�#V�<q�������1�Vݹ#o�.[W����m��+^��+���+p��C�i�:Ƞu��bt����#�RAU�c��v7�"�<컱�FWl̊+� $�c����+l|�\�c�"��h�+�3�c[53�Emb�����\���S>\�D��c�����%3����r�y�0���g,R�|m�mc�}]��{�ɕ�AW��c�����{�W��P�2�xҽ�5
����~�5c����6�wόö�Uݠ��ÇI'A�Nm4�k�M� �'�-��W8"	yB��Kj��f�+n�Wn��1��xl���6✉V@��廤3M/�_a�^�F��4=9:T@��- �O��զ����ώTmܠ�ҙ�d{��JW*kGb�Hl�V�O��p7]7P�?l`��烇+wgV�Xl_�?|��H�b4�����^�W)�a�5y4��-
�LC .�MLȅ�r�:�5Q�Δgp��s�-r�:�(gg*���&b��Ϣ9�a���N.ou6�-����a���Қ3 4.�;F�7�1�>�c̰��a��yKw�q�:�͕�<.�D��H>��oE��?8~ŒϬ?�{E�5�͗�6��g�Kc�饚G��c�o\��2k��.�m��*�V$��1��2kƱV%�Ys�7�R��ʗ�6�-���6�^1�8j����OO����&��u�+]<+�4���ć��݆��WyH��Q�+re�� �1�:m�i���37537��6SJ�K�rn\�ʔR�<G��O-4iT�-Z��r���:[[ZCgc˟�<�<�sR�a�對9����6�$C�\⸹p�;U�P4�~���ykJtYx�6���h��M9��o4Y}��d�B�t�b�Ol�!Y���K]���h��������	��!�OI�5n�)���u�,�5Z�d��1e�&�u���b]˓p��׵�m�`���P��[ �(��-��S�x{��iC����N���%=�V��o�NI��U��6�5�xŒb��1�+��9�"��]��У��1m�Mk+i���&BZ[	p�6��%�]�Lk�%�fZ�d����)�n��KD�
o��6s����HrS>��,�tG��%��Hr����n:l��ȝ{A��H�;����#كKag(I\[G��Igs9l�z>f�4�?H)L<[�"kb+})l<2���|2:��6�q�� *�DO�(O�.�m@[�eb�ʓ���ƩO����6��du.�mK%�silz�ş��� �<,Xd"b�E~Q
�%��i.�{��a�J�0�p��|��u�B�4q#���U���=<��]�5Bd���"{e�p���kخ/��-ÄF����A�7:������gi�D��I����M�wTd���(�I��p37�ߙ�/���]	Xp�l@� 5-���}Yl@�8C_���{'�,Teg�pɠ�;[�pzg�bA��]��;���B{x�o�F*��&�و��8��>��P�����w�I>���ab�#���G����LH�&/^��l㘘�H+�\��+~�K/핞����F�I��z���v�r��|}��
��k���������+dmB{7I�Bm��o��R�ޗ��+^�nRU0a���0��/\�z���^�'Z��wc� �_�v�f0E,{�HIL�3�b,ާ��2�^qj�f����Gho'���Q?&�U�T��ж}��϶m��̂�v���R����T}L���S����R)�ؼ����WXbZ��N�iy��X��$����Xi#���G��mK�
��tvVb�	wڬ�Z/�m�,��:p	l�f%�_�0Z�m�\�0��d$��a���u9lé�'�����YHglz�Ka�+
�����(�`��-�]�+�VE�
�2���ݧ(sYl�X� ��щ��.fu�	��'���٦A�^&�4hv�^&۴h�>��}�lӢ�����[�0��c�sN�qylù�G~m�77R�Tަ�*�/3��6�o�S�L�ahM��ў\�����r��r����_qK��_"���½���f�Х�͗^\춨�,9\�\�EŵõfW�e�͵f�e�M}f������{���    e�Ka�m9f�����Y~�������]1�;�.}mb�ŕ�X�U�K`�Xaq���16'7*��9�����Wm@,� =	V� K�dfq�0v"����+�3��[�A.�m����1fZ�-��d�9��3�K�z�#''C�\��4@,���屍��RWz�5�cv�	�/�m����5 YXM�t�X>Մ��SM@,�j� b��6�Eϒ���x卄��I�4 Xd���|�	��1��l�	 ,߲� b�dm�X�G	{%��N�t�;Q"�vZ3;����h���@,��� b�O:��;-�����<Τ�1V��xs@ɶ�+�;��`��<�b�N|��sĝ�sq��5�ff��1i���ܨ�?��I�M|~9�
&Z�v/�}�ě5sT�^���`��K_�������^��ĕ_f�d���Z>���Ǘ��ቖ�o/kFc���k�#��痙X&���њ��xl����kD�KY�����c��vi=�������j��rrwW��>mv�W��� ^�.�4�e�^*�Ӈ�����Y��6�V���z��<m�V�qZ�5~C��úd��� u���o���mo�0K��GX�]�6g��Wo�+���Ƃ�H]6�Pt��x�lc�ȓ����Ke�ӲLz�lÂ[��.�m0k+^fA�zP&��|8�i�]fA�d��ڲ	�R؆ki��M|sY��65|sY�X�Ѹ������&���&���g������Y�4����V��cn���/�6sYl�#��I^���M{i�2ئA�<_M��2Uixb�7�c?�hҚo*�d��eA�i���+>9ך����J���p��?��)�\�y��(zб�HT�c$��=�R&>��$��	���IH�_]f~� G艏.gn�D%��0P��[�g����suYӹ������nO.��&�;�kכ/�J7�=�4s���z�!Γg'}�S�򞗱�Ni4=�<�X"�D= ��I.Q�CE�`�(L�`G���79�^�ZG���,��Yi�l��c��1�r�jǢ�Rf�]�[זmK�*��iL�������A�?U5�\NE�0t�����+萢8FW{Qv�H5�Z�\aP��jiuʭ8Y��Y�����E*�F.R�6r�J�Ġ�& ��ȺH%�b\Q���u�H%� ^�*�ag��� �	��.\!x��� V�GXh��HE�w��Z�x�� TRX�yjB��`՘kcW��ua�W��$e�l�de�8 S�ڼ�S�O#�)Y�>���o4`��"W�A�P��&�IOh̵
���66碫�E������E���\i�V�R�W���kp��6k
���s�[FY'�g5c��Cs��N&�[�~_���\e��ѡYg��H�.�<�W܍���z���T˓Ze�˘j��4c*��"$c�e�K�u)�XMC{]��������I���q�JDdD��4隊���:���!Y5uFS�C2�"�R�s��"k*Ԟ���
�g�!��3�wٟ| p],��	Řj��_+R�տ�K� ��jX��Q�kD<E�a�Е�w�壨We��k��F{8!.�z�lk피k��Nɻ权XɻF������1c�%��ak+w�"	�v��5m+w�@�Z|��XI'W�ʧ�i�|� K.�.���	[ʑgiqOp�,P:fA���t@Y'�1�7�@����,���/+�^�k#��)��H��Otٟ�7�?I�.T�H�c��97B.���9C�އ���-���ji΍A���97���ҜAh�F�,W�B�����]Rv��os�ڑK�!��[Ŗ�Q��ˑ|~�����1ֆ�q[�H�h�1��	��d#�:��%/	�]mL}��0�ژ�Blm|�1^G =Ԙ�#�V�y.�0�������Ѫ��Dl��jC�%�a#����D�PI�I��V^�A��V�2�h��,h�D�-s��Q���0��ΚSH��P��N>I�O��rlԧ��1�%�S�y����B�G6
TR��ҾE*�4�Uq�Tt ���M�K1�/��'�G2<�M��p�#^	�GJ�	��w��k┸Km��]�o)q�cʞ)q'd�gJ�2V�2w��XBl�Y��� ���͞L��8��Z4���@*_@�g��12�% ��]9+�L�[��n���ܲ/RI���]���H%s�tj�.ܡ��v�N�"�T�ȃs/&�/�8��������	Źjg*�u�l���H�S*��Q�G;@)�4�[�*.��)��þA��^u�*i����ʗ*�J"~��59lM-� q-�I���I���9ꍰJb�kh�p�N������@ʾ҉�J9Wz�d��<W|���B�;4;4�*�����6����2�A�pn��z��F��R��O�բ����;�R�WzA�A�J��=�Ԩ�+�����([�kU��q����Y@�*d(6QM�����AiJ">�k����'W�.^��3p�wR��H&7�SS��$�;5�+��:�)�t�s�����${����x���[����7���urT��6ĩ�/�=e���ǽ���#�۸l�hy�ԙi��~�3�����L;>��&N�d�c>=f�dNO�79W[��d�<��[���fO5 �֞J&�߃��gw��L;�̾�":���z3��ATE.���&��穔��o���P��$�:C�S�a�3U:5y�L؃�؄��}r!�0%`��Ϯqf��.������z&�o�)��5K3p��?�=�4��:�S�TS'��J���o��Y
8�8���C*��i7K��3���LM<!>Rh���tt�����?Ν�9E0���\�@)߃����OX� �|�@�$9@*���A]m
�`nUKCG��:M�(������C�&xr�<|���Cg���:��'��83��I��u��)��&�y'���ʃ��a�����[E�G�A�E��qa��R*ϓ��|s6,[�'U��A�Tцq�V�M��]yJ�������&n4%�������2�SR97��T�7b�g�/ �F�`�UyP!��ݪ83�HT��BM1v����t�q����,wck+��+�����0{W�k
��=�"�)�ީҚ��]*�)�ޥҚ��]*b�~�KrB_W�t�B�_7�t����,Õ߄5��s�kp0���->8�Tq1r�xp�Ļ^&>���R�)��F���CSٞ�,�N��J�w�����XJ��X1����Z����0����C-���f�����r�L����Ų��F|��V��{� ��t���&[A#���k8����X�Ua���	c���
��������#�W���}{)F[�W}���y��۴��ߎ�@�9�=t^���Gm�����W��
�y���a� ��j�><��1&��?���WC��-����uPc;��-����[�'�f�}d-��~vɅ���>��ا$ܜZ���C������ǌ"�b�T����]߻�Z�v����w->hz�h�m�I�SF���xD�Q-��:��[:e�[N����<Z��|�֎K7>���)=>w_����R4����2������}vM7:��x`vI���am;/��;5�Z��6�Z�Oh��4i��ki��4��ki�P�2i(u�x�|'��(k�m�o��Ƚ�'���h��|w0���`�J��6,���mm�`4��0�~�Q��|�(X+FY��m�]�8ێ:������3�����"�i�����^P����ŧ�@�R��i���f�k9Ӗ"q��<�amh��}��g-���\���[�"zq��5:C#���Bm���x�oy/�L��1��0�Ρ�N�Ӥ�6�x�Ђ��r�m�c������"t�b��<v�k�%�n�=��nCM�(v됡\ܢ�f�����\�R�1*[�_��{��N�fd���.���%5�k�B�Ԉ��.j�q_tIP�f������%AU��KZe�e��]fcҊ��\��{s.�.�8�h+��0
�K �c�Bû�)�    p� �d�B	r��͡��� ��D�΋X��٭3���6����v�[�	��V!��'\�h��1�@]J\�6�C�K@�(�-tya� .�7�r�#��K���:`9����^t���^�rå����.z��X;�H��,^LZ��6ŉ�W�����U�2��>\�G
jW��ُ��Y�7��y?���T�鸢�N�)���pc�l�-��+⤣¿���z�Q��3q��9�zA���6��Q�Gx\Z5.�.���Q�P��6��vT�v�87������BeFc�L�_kyn��>i����b�U�}�{���_�Q�\���-��&&3[���xh)o:c��;��dFcK�F��[���}��6G#lI���Z�=4���U�;�^m�q5�d������Rׇ�j,�\4���.�/��^\���6ls�4&�k�.�mňl����lFc{���zxZ+6�=!��5W��c-��\L��
�VU�ڿ���V\/j��6��Ȗ��~��m'�kOG��mǨl��n;Fe�m�\���F�����m�"��r��:E�і�S�@,�)�:X���N���y^INc^9�=?��iy������ЀV���ؾc�#E"���54��-�|��o�ՙ�o��mѥkX�Lۥ��Ms��rμ�^�����mL��>�����+b-�-;�i�l{�Kg���^:5�@�����F�3�&o\ +-��E�?o�C���1 �O9�)���聻��\C��єѯ��C԰n�z�� ��n�}7����bP��IHq[!�u��u+��yl�#1C�)� \4��vdvh����H�]�ǖf��*����܀.ږo?�:��Ѕ_��؅�2�ȍ����o6@���K�lfs�i7ܒ����&��t��I�N��n��1�-	,m���(m'����å�yoE� 
�¹��ʭ�8<�
|o%��m���t(r�çC[��M��%9�����Cl%�����J�!�t��_��&��xpm5��7��B�S+N��F^�Cx�zymqn%X2����Vw�9�F����5�Z��Ŷ-��_V�4�f2�.�&J����ZiK�������$�o�t8򹲼��6�x]k1�ڶ�l�F^���x��k�"���}\L.�q%,�)�>��U;Z���uw_B�����f��]_��J�;d�6_1��J���ԨlۖG�{�Qٶ��!_W�3@H��l�~�;8�`�헿��ڶ���G��Ѷ��o�|�ɷݎ��G芏1�<hd� ���m{� ��s�j��mg�O81��I�m��)���>�	�'&R��!��ڲ�>��N9�k��6֣YGޑU�l��ږk�+��ZM
<?���$�F\6!�e�fX���h���@{;l����	+��i���`]|�1�^Ǣ�1�rY����i%�]3z"a��m�U����Q���Q���BO0y�=.�/����;2c��m����}�2FgF}c��6����>��㇯�3i��ߜ%�N��ltv���rO� ��6:�h�Q�^�'PU֝����6�ţ�������xlim��l<��6���c�	^��c�	֖o<����n���#��ӎ�Ƽ��c���9&�6��b�HlCk�Z�1��$/�э�6&ymL�rk�hbS�+V�����/F����;���-��k��>{-�˔�j�{�L߼�X�Gtp0�ڀI8	�Ẉ�����310�C�@M��@aq]w`�|D��R>G������ +�瘠�� bѱ���dwiN[�;�\OC�-�n�����#�Y�v�utA�]|�4�;\c��%mƾ�Z�f�ёI�m-۾d7آ},}˄��-Mx35e��4al�ޑQd�[�qT���H���\�b�qr1�
�jk�"tD�3�'c���U��_�� �Y���b��^�"��~\�"���~1Kr�.�BKS0�Dnc���kO���ЧM���l���m�a�q 1j۰�M�_��k����JS�<t`��ig���Ux�,2�h%.�����ss��hmyn����-��U�S�����w�pV�¾S����g���9:"-qe�ғ�+�YC�[1�J-o�8+#_}+�Yof�oE8k�o��6�[5-zDZJ��q��N�_�IE��#@�%n҇[<C'�"����q������jm�u7����,/��X���¢pLEFu<������+�R����J�W��翵\<V��ƭey�H�<����AG�m����б�)��
���#X���M��� �����-u��'
ȧ&���5P���I����?�R}�/�QS9`c!ϸ�-�����ʂ�{�V���_	��j�`M�؁ �q�d5��g�Z�Ѵ�dMS�'k��-%�jGKI���h)�W�Ӗ�|�5��$_�:ZJ�U���M�+N4���k5�[xe.�Xni���ā���q|tH�81���+���6L���]�W�Nq��/��E1q;:*�ǭl�]U??�[٢��~���0�]i ������/�71�c���t����b/��������@�{��{ظ0�u�Q��ct�ܑ�8�P�� �!5�Mt���]�Ä].?����myp.����U)0@�b��0R�8�YuKǥ�$�a��ڕ$�u�m�F�{u�#_=~y}\4�4�R�'������E�'�Jv�����EY������m@r�~ş��J�[�Ww�-�+���$f���ت=.�j��v���¤�8�=.�����vM���z��0� �q�=.i<��׿�b�T{\�&c�T{\.�8��0�-�ua��oc�/L��'W�E��W��|�,L��'eh��|��+�\=e�
��Q�/)c>��]ݡe�
��I��"d2K�<.J�Β*�Kl����c����F���,��ВfI���e$���G��YR�6�yqL�N�u�|,��8&���b���-��JE�3����D5�7��?<\q���F~ۋ�&�q)
[v4�L�>�m�[�bu#����a��t�^5�d�1���9���4�l��e�����e���G�b�n�Fx��+x�2Fz�˷��8o{IqU,������m=/m<q�8o{́��bb�Z]VM�S���m������>sʚNH�IS�c>�z�k�Zx��ho�����6���h�S��*�|���m�c��>q#����oa:���F����֮��#�V��m/9��>)P!��R04r�?�#�-�wt���<��Y5߇i-#���O��k&ud��	���F`�	�6lwl�
������b���ۍ��TO�}X%�8Փn/�,�q�ҷ�\�8p�2X f�\G������5�Ɓ��K������b2|�cK�Ąl�9.���.{綏��\��7&�^ε��;?�A�f�ۈpî1����]���xpî�o��Q��hpì��qpi4�a�Jnt������[��twsdgHe�q/c¥�-��
����Q����t��4#�h.�y�=.����¥S^ᔍ�������&Q˃�Gδ�q��V�Tc�^w�4ܘ�E�i,�]���g�T&��|�p��1�rkZ����rK%JF�K?�`^�d�t4���p�R\|��K=i�ƀKOcud��Q�ZX����V�ǵF0�c,������4X�m	n����, ����hI9���-b��h����V�^Vs��-�W���S�n��]�ƮaSƔ �(]��{Cӝ2���q�Ғٱd [ܙ.��F�K��lL�4_>�Q�
��+dg\���[����|��.u�K.}��nt��|����R-���Ɣ2��.fI�;�n�E-)w�ɾ��ud�}ׅ-�z��.lI���yaK��z
C$_6�0D�Zc˶�]u#�%��d��zlI���"�X1[�Ѵ�-n���:�\��2��%�%c,s�M�T�h�3M�����.5��z]�6��r��6��z]ݧְKz]��(l������G�����r�A�����i^    Z[��xi[Z�͕�cOi�t��K��m���+1�3��S���}t���#n����W7� ���hq�^-N1S����pñ|y=���6��.z}0�G��/a
Dx}�q���'\�-V2vܞ^�.�aF���G��)���I�5noII.�%t���������Hq���.�`m��6#�Ss��ݬ���f�_)1b����w5n׃��~;ٱ��|�q�v��n�2z�9�ȡ@z�ۦѕGk��i�׊�����;�MF�K���|*�����ݍ���I?d7.ͷ��7.ͷ1�e�4�Fxe�4��l�1�һ�?LV��������iw�9n̨d>w��^}��2Œ0���v�:6�ғ�q�.�/B�ƍ�����+�.9B_+͍�����\i�ƎK{V���qiϩ�.s��0�\����Ҝm�w�m�lM�muZd��Șo}F�ˑ��^�=�y�u�ȱ�F��)����.#��I��#���&~pf��F�vn_��u��0�B�x.�Q£�����pi������	��ІN��E0<�ly��<!� �zaD�{6�ݫ}�F�7���V��	��b�}�˫k�.Yo�ݫ7�;㐇+y.[��I�S!	H}NE$
�\){)"q�-/%n�ǥ�IX����h�1	�[�ǝH���+A�ܲ�16���{�K �}qKڔ!>�3�؍�tQ��B<Ì�ti�_ۍx�0j<��3�C<�g�v�=��m��-�:4(�%���8���GM�y`�(�00ۀ-�s�m�hsGӮ��Y �8�Ne6PK[�? ��:�[�[� .��0= ������EvY!�(BXƔP�&_����<\�%�C��F�6����N|��Yߤ�tI	�ۍ#$Q����]�VR\\��*�c��<�!p2]�b֓�t��m=#��A�����ؠ�<�� )ƛ�Sve����K*�p���W�/����Kڶ�]D_T_1�<U����F�!2��un�Ȓp1FVy;ψs#F�"%��e���m�^T������ѧ�� ��+���Z
�nrF�ˣt	�n��<I��k.O҅(�Xsy���ڸ%x��*(;7��*����/�s�������t���i�u��ŷ��HT�A���K*�{ ��C%-w��j�:�%ކcdh�#�Ѕ�H�><cnt'�����kQYg����&�`���/��
�N�i�3��k�����}d�1�\ocV�\�J���\Ts懮s��*��8���3���7�����Ȍ=7�Ec~� �|ʛ�T��Ɯ�0Y*W4�\�ɰf=hb���@aƝ�0�Y�m���{ri�W����挒C�8E�*\ƟK�V��Ҵs#)+]	��A��]eۻȶ��v���,0��h�q2<H�?��Nɕ���,�2G�;T�|O,G�t�ޔ���aiH�9* ��8�9G��Q0�ݰ�>(y_�:���]�v�.�ޣ����m�����ű�Zl/$�7��<�`{i.���Huqee�V��Ň�419^T1Vp����9�����<���]w�l�%�+��%���B�*��]Dw��]�?�Y�a}���_m��������=_�T|������2�'��n�>���q�O!�%d��[��N�.Uݧ5k�ދ*�[�*,�[5%���ZAi}��E}P�/���_Y�S���E�������V���%���i���~��n죅�?��9�����SzV������~r�i�[Yr�)AG����� �dl��?��N}n����^� #u.� ��N� �Wo@0�B�� �$u���N���#\����<��N�y$Z�5�����.��`L��w��g��\��
�saL�N���>�Fй���p��Y�	$nT��o�(�^�6;��D�=g��	̉��>G���m��A@nOOk}����i}*5/�˻�vn��q���\�H���ň�5w1��L{
U���>YŜO]�b�y�>ň����t�qȩ�RuȈ``} b�2��KGhn��Q�B6e��߸y��\�:���$�a����1��%�@��:�nyi�5���Cjb˓� Wg�-�s-d�s�*_
��H�/��lO���	=��h�'��Q9d�$���Z�$�dPό��\�p6{�>f�ӅnwƥS|���p�ڝܺ�G7��D-��Ka$�2:p�˼��X�i��Х�f;��h��2�)?XP�����3�z/�#��� e��t*�62W-�ɵSJ:�X;F���1�M�+�����W�EE�K)*��~V5:A�*����$e2�-���KMQ�)'X�V�61}@,��;}�F��͚j&�2(��Rj�ǚ�VJMY���������J�)o\R���\ڨ���KK�g��T��4Y^��Pi2�V^��D�������b�2������2���kh +�+2�#TKO��΃����d�=��*z
M��l���r��� ��?-�3m�&Vn�HQ�M�C� e�S�� a��F�|Q���]C�(� M:�<v݄^Q���U��d8#�$H�)"�(��2SD���i
G�[�S�L��k�)Ѹ���MV�+]㱺TYd�E�D�3#:mŐ2�.CE�(q"ý,��.q1�NA�A�Z�~�L�;�R� �|kY�d07�J;Y)(A�Z XU��O�b0pL�
yr�.@,<�y��j�!�ުZ�[Ղ�7����W��Y�N��ɖ��"꿰�6��JK�h'�2���C=����t�K��ȩ���Y�X��:�\Đ�u�J���A�l!=�<�QD��%.Ѥ{ �yBߙ*�
W���?�P�V ��&�M6-��Z�4;���� a1^����f�v8¤�nkF1�c&�e?�nk��k�&�<:ކ�0j�2
�̈́߻(�M��'cNοN�0��>�̔\�;�yg��W'�m�΂�}�H��:{���S�����H��Oa5�M|���U_�E��D���K��Cn\�C���}�j����(.<.d�+֗ݐ�,p�!5�����zk>6���9�nW1�s�fT/]e�xY��n��Zk>�T��>7_bP��ʛ�/U�W��GE��`b��<�=�����.���1��C��t��������貃���u�Ou�__'B1Ʀ{'\�Q#ӽ�fC5.ݻ^Pr�_�G��ڂ������1�^3Vd���5���՘t�G=��b�0c��V��;NE���t۫,�j���/Wg���j>ZMG�iMm�X� �Ӎ4���u#�:�nO7� ��{�W��m=�E��+���/�tE�}p�����C�Yc5*]��a�*þ�b.n2���W�J��%z\͉t��!��֔��Z��f���n�8]U�э1�4��Pm���Yt���<�N��dz;��2��S�vݫ@�sw-���D��s�����ͩ_��}8�K��<���B��N��N�J7�xiw?MӼ���N��_��p gP���Q�K��
|���%C:[{ܒ3<��=��T'�moF��<���!��mߎ9�n���It[�4����6�E�$�-��^իN��^�n�[� ���� _�#_���}-�6��tK������s{���2.J��j,����l����R�l�.�꘶�vqKځ���[Ҿ�wq�gcn�����v���S��`Kޭ�C��i�N�۴-o��v����Ħ�^j�ڀ[tI�5���R�G7���Bly]����<um�:�nK���>/r�XN��%-[���>������j��v���!6 ��}�{�p��Τ����.n���vƽ�ꬺ�J��ao���%ݯv�]�w�6@�!���E�o� Hkby��Ē޹Ě7�2r�����:8�,#�1`���1̛�r:��.Q�N�;9������E�b߲<Nְ��(U�ѝ�*��<��ːU�Gw~k�����CVu���[�h�G�ҳ�.��r5���� �G��Z�*Tn��ytGR���a��j�\˧OEq.�K<lnH˧�3'�U����U!�f�e�������Ƶ1��Xu2ݑ�/�T'���.dcu���e>�T���}|iN=v�L    �3����X��`ՙt�ߖϣ��8:��`�/�y�:�RM��n}RuBݑP�Z+`%�)�ݛ6�XΒ���괺a�M+�Yp�;��`�|,�8��|"Ug֝J��Btbݙȯ��ɹh�N�;^�u�4�Yw(H��YmJ;���c7Ϊ;��['՝��Iu��㴺��ш0{D|����\w�#V�X�ή;����,v0�U��_}�C�V�ܩuǧ2�:��P��u����N�;Y�������	�<4d��hdJoEz���}��7�r������|��\>[Wo:�ޯ!��;\�7�j�6䝸��xI�I��I�q��ŋ ò����)vǛ�:��xqWUg�o�ځZR���+��S��?�v�#ޒ"���.t��Gw]G��#���A�}*�ήή�^��G�sb#WB`Sh�0�P�ǹu{���I�Y��`H�P�cI�G�2��
EwO�pV��),�|Ω;��<N�;R�ӵ�5�NHC>�ځ]R�OB�!=/B�BH�]R��L�9��t�2���Rw�.|]�����V��f�i5V]��B�鼺3W!��ה�.r�+m�%vg�՝���,\=��W�}^�6ݗ��	�j����N�/Iu����F���n�`�%<Q�'�T�x")t��Г�'�\�U�0��H�H��7#0L�Q<	h��Ds#�K/�kg����6���t����Ȼ�}����(mTf�B'�ͷ�!T�3.#T���#�3�~��ׁ�KB���a���(hWO]$�N��3�EEO�n�_��0Q&W��e0g�9�nOX�Me ������PD�����T���z��mc ��o�ׁ����I��|'���E��{
��$��6��4�[ר���A#���ˉu��*Bu^���qr�99�nˈ�W�@ &#V��-U.e\�?��&5q
�䯧zؒx���9>#�e��GS�s���R�M�F�(|�]�
TO��&;jZM����K�tВ�ݏ����5��X�)v���X5VݰW�����k�`^�)~v��];#��� x#��t�#����)����H���HJ�U®1R��F�k��yuۧv�Yu'�̬4��e�u7�.��m4:�;.^Ug�m���|
�o1]�Ψ�81|���g�qug�͌>�����`�uC����u4�2�!��y]���˥B�^��\����
�膼���X��:�{��рL%����Э�'��؊|�⬺�s2qVݖ�"F��,�趏.�~ǏF��Z��ǌl�v�!�gĺ-1����<�`/�+��^J��ݥ�k�	�.�:[��f�*�.�����_N�n�qH�b�(u��
n%�1���P�\�RW�pB]qH�OW�����B����	uI@p�T��%�:�\Qi>�9(�Tfў<['G�{2ra�I�+fѹ=�4�7e(�E�V�׼�7�v&ݞ�cbg�}�`Bʑ�.\VΤ��'r�L�/z"4R�tn$�酀��t�X*d:�Ϫt-O0��t_VgM�����)��u����C��TKپ4���}� dޜQҨ+)��Q�=��F[�[�d��Q@����,<����������}�U{f�����T$�稉�K�O�M �|*QgO��W)&@K�J���«�i �t�J�dL�]ҕhzT��yL]����nװ��h�;�^wr�]���!&*]xc��0���S��3Q��+�� WR���;ӡ�Z��P��6�3-Ϥ���gR�L�3ih�����gR�L���S@��tQ�|��[�q(�������i�ܹ�^�8�%�9p�(u����T�z|lGK	i�@��S�nUB���[e��w�����F�&ρƢ��I/��������H�N���8a\�\+�8qrSiie�T'�)&j]��	�.�]xq�'�]��	,�[���X@G!���O>j]R�*<�I�eF��ucUL&	'B.4>����Z�B���tQю����'����֓
%�N�B�Kt֗�u.I���S��T�n��G�떹|T�n��G�떹�O#%�K�FJ*Ú4�URִ2��ʰBǫ(��t��������K
GT��"���*�벜Ue|(��.�j�;��(�e:��[R!���F��sMf݀/�܎�"�N�Y5!w���"�.ݵT��5W-!wM�/��H�N���O �K*���a5��:C[��}�h�T��s�j�9]<Y-y��l�T��hW^O��M�v�Fwb-���C>h9�#�Y=�#�Y=�# Z=�#���)�,��������"�zr%�d�]-�d�F(x�1d�=jS�HUȝ�q�T�lo�8]U]�UȺa���u�dV!cS�C�E���
t����*L1��*�|�nMV!������yj{_SUȷ���U�|���T-�ߠ��I9��-z�Th��W������b����"��������sK[KUȝ���T����������p|�ފ��8��.M:ݞ[;�Ύ�_T;�{��N��=6�S!dr�;Bv��E/_��S-�H�fɱZ�J7r�M�)�=���N�+� ��O�+�$2�ƣ�WV�ʃ_ze��;��t�ʪ�@8S�}L��2B]��J�к�^�f���CҪ�r�3B�x(��a�u����u�䔇��!^����W��pظg�^��Oz�7�m<%���F�X��Į�[)�^R55M��2J�xH���2F�xH��zF�/�F��t�!Ueo�L7R��~�Q���^�
���	�JdL��.r7�\����kz�8Օ�"�4R�������j��|��uu��4�>�u�d�SU���3���[z�׳f>���t���5�����l�5i%E\V4Ǹk�S���ӅR���.
�����Rs�1��S�"=v��-=s޴�Zz�<��9'�X5B]Z���ܙ��7�1F�Vʲ2cӥk�=j_FxHZ�,#<$��׺l<$��(lt�| f�^'��֧�o=�,_fl����_���}0�	F9DL^`����7^`���xJ��Շ�����N��w���tò#,Ʀ��n��tÄUX��ˋ���U�=�F�~�>��x��m���b��D���:~q���R.Z,JA�/�[풪�\7�RW�OL�[�*����E��\XY��
��-w9�&:�^_��.�6�]"1��//�\�e���2��b�� q8lD�1�����JPƤ{w�|�moղ���b�<�b�<�[�z��\�=y5�Z�W#'j�nm���7�]�(����[��ՅP�a-k��B�ҽ5*��̨t/z*���\>e��J��x��Z�,�hO=����nX���᫙7�o+�L�{�b��� K�H��A��πx����^+�xt�cRfɈt�s�b� ��\�E^��D��҅��ht������K��
ⴳ�3�؈�@�Htcâ��2�͊#�nI�0
�KJ�yI�n����Z�Da����y�`��xt	%��:&
�Q�L��S�,ܴ��.�C�'悗��]Z�%RM���y���p+��ܺ�N̬Ku�tb�"�F��_5 F��E���Ƥ�Ł�/&�"]X�v ���r傕�қV�sXu��iVC��C��ۘhcj��չ�EY����A�V?���ʬg�c�9���8ucߙw�9��h�q�O��p��S�Pl���h�i`َ�@��i�Bu>�(�^.Nt�yfZ��gU#��
M�|a��f��Z�����ek8T�n8�jΔ��(S~t�;�vQ��y:x1���g�{�w��G�x5ZI�i��3/9x3�t|:رɊa�э�M���n��M<����p�̴����W���h�t*�z����.��si�3b�ƠKӝ��}.Mwj��S�����c��4�I״�_���n���y�p�I�L՗�)�M����J�|�A�%��ఎE�I��m'�і�k|�w�#�֮3�.� �(�c+�= /Y���EF=è�뮬T;}6��4�G��Qٕ�8vJl�F]9�3ѓ)�%�K�+�����j�'��<��~����J뵑�ez�{�L��    崧��zXN{sQ��L{���A�%����Ew��k��B����U��o�嶁ݩ=7��J���"�*�7}����:Z��{V{.p�S�s��(� ՙ�_dߌ?��[��s3�p:��}3��:�^��nxb�f��m�_kF�K��*j�/�8O3
]��g?����괧��#=�i),Ѥ���-�����hq�nOOa��HOa�h{.�I�(���LS�:�`��d�=�&��&5��_���"�Ԉ��"%q�����־�0*Ϡ}"��י���4�q��A�%�gl���I�3P�s��A{ `��j�!dz�<�_�4��r2�K�-���hڞ}Iv�=�),�eF3���O��Y�g�$S��to���<6c�D�#(ߌE7|p�ӌD7E�&���=y�%��Y�Ҟ��<s������U��.޲�~7�� �|�#�/ߤZ{�.���<݋Ҽ8�]����@v�5���q3"]ގ�V���n�V�t��ֈ�T{v�j;"�Ӟ��ڎ�Դy���d�؋ʩ�HO�j�IWm�夫���r��t�v��e�磫��Ҟ���Q[֌K���F�ڃ�҂��3򓇷�d|F�Y���bt���%��.�hi��ޔN
�.��5"L�
.J�VpY:�_o^�7���ͭa�V�����\��aŵ���:���ƫ\dnn���d������L�ܑ�|� n��S	`q���K6Z�=id����g<�4cԍ��ӌQ7z�b���t��v�w2��
nI��xz�pIZ�(Ä*�����xg�cӥ�^�.�wF���.���.�2ީ�Ӛ�t�J�L��e���n,��E�&�ϼ��fl���j}�2��]XU����b�.�	�iO$yNz"��L�7�����y�d�(uy�r�2B]^�L�Sv=#mЌQ�v����-�f�Z3J]�5K��Q�ҮS#�ȮӼ�*�M�f����P�G���?94v�e$���O�m��-F�Fأ,��n!�9[�]�?O�x���=��V�.�vN�l�̷ ��.B�q��.'���Q�rН���m��f�k�P$ݵ횫L�ҹw�K�����U���'���r�G�F
�(2��c_Y"�ir�+���������g�r�H�<�O�;Ӻ��m\��αӍp֒5cץj�@�$�|=�v����Dnd	>4M�L�<s���|���$SL�{�)vu�$S��K���F��k�$^��%����	��%}��	Ta\��8'" ٌj7LyD�f��2ܨ]�h�*=k8&���1�ۭ^���G^F'�z9_�e��	�v.�zaL�Z�mD�7& `��/47��K�B7� �*<k8���`w~
Ϛ��\x֜\w~�q����W9���0(]b��N?$Nn0&{��ϛ��r·
�.=k0���]w|�����Uc���������8��H,�~t~�A.ѻ�8�� �(�\�RD���uG�W~|�4�����ܺC���*0LyԲ�'0�D]t��X�&z�4�>������z�����ۋ^��
�g���٦՞Gv SctX�}��l3��_��\t��Uw~������"�i!��U6A�Z���F�V�,V/K]�f7�����r[�p,��.�:��oKI`�˄���|���f <u��ѵ��D3�;s~�����kk9� LfF��i�ˍ�����f���l%0A�<+k�b�_�h�U)��-��.�E��R�VAU��w a���� �:������n��p՝o��Z��7�K����H��阔t����ւ����؞Xl�BZ��&w�f�:�)���\���r]rͷ��u5��&���K�T��^VI�ʭ����&T�uI��d�*���WHU��y�XEYW��`�D����h|r8���}��L9]��;��I&�h��I&��ړLn�Zړ�7Z�mq��	�5����S�[Ti񭟣[��V��(*��֊b�n�(&��ZQLbEX���D��q��,nĭ������NCI���KT���5�2�dZ��
��|�C�l��r��l�#ux�Q�^.\tlΤ;�H���Y�誂�l��5E#�;��p��骵�X2��R8b��J
G��Sk��El���xKjڝTk)�2��q:��_����t�߶{"���^Oėۋ]Z��rӚ[O��[���%��[��R�k�)�p�J��<��x/�l��h��᪍�{��TgJjm��2H�[�ܱ�!��M�ͤ�i�#�^��vIu�R���//�<�)��).�n�?�ř�/�T:���}�u��D#)�L~�p<y����/٧��6��֦��^r��R�b��<b]V]n n�K�{�>n�u/!Ӷ�\��}3Nݞ��@�׌V�Y��b�f����\��,����یVw�.�����U���n{z9�]h�s}�6#�}9x"��tU�Q��o⧰�횹θ����qg��nF���rǿ-���d�NB���ث��,�82��:����L�9���1ڼ}����ﺍ���;��.��'�L\�+���LaJ��6(�%��S3�\�š��kn���F1�\V�]�e���(c�]IQ�e6G�_�
~Tb��*���-|i+���ݦ�-�5aw6�*w�&�Q�NMz�����$��E�|St#7�����_?��D���lbS�tQ�n`�n.���~c����z�ZZ�����qW��~�0vܕ��i�-..#�]مb9�Ac�-L�]nAN��4c�]�+�#�]D6>����0O#����O�(-F(�5���~�1�הoB_W�]fW��z��+�w��}M��{��ϟ��$ij /�}\���n���C������G�
X��'�Jҏf�����ͱu�PYC�4*�ٌ�V	aT�F�e��R5���R1� �5�[�"T���
%.�6�[�'��E��_�I�����H|H��6��W�&uVNjj�qRs������ꮩ��B| s�XRr���u�J��]���}t��%O�c�4����K��$U�F�����SIK��5�Sz���p��(j-�����T֨;��M��
�������y�A G�/e7#����;�����U}Ӟ�iOvh��tE��:��C�������|�"¾b}��K��Wo�RE�č�V��w�:����~��A���;0D��ؠD0W:����DD��]�8�`���Y#�U����m<��>b��#�7���_�3C��`�T�v�p !Źh�VRPw����E��у���g�//k�e�����#�]/����F��� "��>?dG[]:i��V9  �Go ��Cz�S����c>*��o��v��݌ΡɈi7cs��#V�1�Ng��C��Ņx4%us!ǝ��z�:���nF�n�` ?��>�TɑJ  Rhԅ����m��|���&8~4)M��	2n�?�L5�1 �p�p�t���Z|��tt->(�ڵ�L'nƣk���5ڵ��Ff3���ғ��sKJ��\��]?�cŚ�vv���.�wH
rx|����bT�M��ûWC[�Rñ��]�6<0�]���}��
��f<��u��~>ѫ�~�� |�]vJw�c�9�6;���7�1c���mp���Qn�ĀS����!�ױ��f¡�ϵFOb`��'����O��U��<3�`��e7S �f��G�a��5�Eܠ+��u7dӒ��(�S~^�#I=��G>=t� �a��T���p!����x!)����D~�t�eZ�܉�1�.��"Hj���u�{��̻�W{H���a��p��
}����X뾺�xL?/Ƒ)�?�,��Z�a@�v�X����a���o�LN�����ņ�T|n:^��ٝ��p!�c88NG�8�MG��a�阁aM���woA�#ܽwOGS�n:�}w�Yt�X�� v�9�=���=
?��)n����m8x#��9��߱a+81/(��L��ݯЀ�}-���X��p�B��W�ͳ�l�BUGsf��|�h>���R �.nó҈�
b��Ft�ꮽF#�ߺ��_|t�M �Ԁ�����������A�~h��|�~SY;`�t���    :��h�����k���'n�3~�d-��9j:bp��!�bz��1���60�B���.d�ƴ|Nm����e���8�b��H̡#Ŧ��IS�v��ݡ�񶜨��`�x�Ü���M�'���4'����Y	+݁���92|��F7Q�t� G�u� L�9^ �:�Q�����sR�A�h]�m��7N���d��uf�
c\����Wh���������,���r��Xb�\p���eN���[SªX6�F�!�����U�����\�>f���!t���M���L;mG�&��9*52Wh�Am[MoXo<��^���W���?�0�j���m�l���%�$����t��?	�t$��u&ǅ^�l�����Ei��Z��:�L�q��"7�֑�v��ȍ�u����#��=a�L[]��捫5Q���6�V�܍�XZE�Ro��HZEfs}�Q��/25�֙����(Zg���5��獠u&d�\���.ڜ.� ��p��Y_�5}-�6
I������r��jS��=�6G�>�ߌ�u$���i���j�-�Ի_͊���݌�>��F�:��<�nl�#��U�/"$��:���9�cB��)O	g`��j�tH����f:P�.�mH�{O!�����ca�m����Fv���Ƅ�·�Ø����aL������G��O��{�5�U�o��Q�*!_��Rьqu���7+�[iW�����{c[/x���vGZɨV����`�9�o��z��=�������o�߫�s?��j����hU�Tw7�ՙ0>,��j��U�쿙�<��[��ư�*��ʛ��b|��UgNV��;�H8�r�p>�#P��( (b}� �s
?`3���hSw�� m*���.�6�ٱ��a�T7��$�-8��`�Vj7���D�X%�Jr�9�[��׌\u$���Z��6N����\����C�s��v$�/�	���~��'8��c.G�os�����b9���;`����&�H�;�xõ��u;���`#�7�o.k?�hB����J�Oq��"�* \���Fw��͌�]vh��z�Q}�tQ��/Bߞ�P�v.�X�i����}e{ "��F-�F�- �.���E���f�vaa@s����y�v����3`m{������k�Lx��F_�ܕA<�F�=�x&��veϩ oWĳ�>.:?�u�3�F��Mj�vSe@�Qw#��)�����C�}��{�a\�ۣ�_B'}XW�΃�C�m>��k�b���wƍo�.,<��0���d��3������F�/c�p0ʒ~^�I{]���N�^W�Zc�gw��56���f���!7�?��P@��_HY�
 f������=XT�1l{�֫�,<k������e>���Iv:�Y+���ln������s���}.��S>�Y@�olm!�I��S��g�=)j�{*�D�����KMNj)�D�����K|�����R��A_m��%9�5"�Ө�5#��b�3~SQ��B����	�g#7��mAs���&�,�i,�1fӝ|Lt�̦��)	��9;`�%�L^�u�xւ�>���%�g��%Mő)M�Q(���By��p�+�TM���๋�h���0�Wh	����y;����a$�0+qF�M�t#y�Y��z��3��j�"+���y��-����/Ȕy��Ç��<+@d��������1���ց�ذ��0*e���{��8�a��SXZ�~
�KIO�U��[�)�-~fq�V�� ��Dy>
u��C/+M�ДN�)�@Y�ʒJ�k�2_�{����V"�SUZi�������T��֨�9H`h�t�#m���9M�]U3�T���4U�V��i����4U��T�Pq�`�ir�č�t}q�񑮌`|X���*���5���dS	Tv��M%N�+�$�q��Mrg�+�$wv�*A�NW%�����;;Cx���J�"������\�����58n�]���j��3"J����%��i�^�l6�Q��tx4����ӣwj�#-)��6�ɑ��p��kRn���'�i|��M�}�5�Q��Y��q�
d�_�?|�ҍoԄ)��-,Y�C�C��t4�c�).����>����|O��r���L#=Ii��6�Ie�3�v��)�2�ѓ5��m�$��U{<&U9�U��
�R�v��j�e��g�Q׶Qem���?���=6�������Ȧ����}g�`��4|㇟����4��s�V �ѹ}E':���n\��[tCR�×��Dsa�aqa��V�iSa棣�3����Ɗ���t��*����$:VF���`�)�>��~���X�Ww#=/O�+=�����˨��]�u�݈FO��:.�}�2�֦���]l��5G��EOJM�o�������6$)���k�PU�5��_�0��n��'�aV�
�-�/4Zw.3�E�+3᭶���:P}� ���/5�={��.mŇՒ}-�^K��4�-�/���2�F���o��:�By�XDOJ�{�#7˰{7
�A�X��I�!=|�b����zRH��#���ٍ7���#�-����;Է#�Ӎ1������h�Yw���ʽ�uE��B�M���х�.�8�Aw��nd���H�u#�ɉ��Eȭ[(7˴�!X�-�ឿ�0������ ;rF��x�#��C�H>b����@Óp�(R��h��E_�)��]��_��J5���6�����?-4M0e�sQGJT�.�He5��)�����`G.���X[v���֑�n��~d���^A���W�c����0��}m'���v��L���[~i/��8�?�_U{|5	��]s��q�񧿧��:qJ���z���W���㕋��V�i�gӊ��Њw����V�o����T��\Zi�Q�ԋ��7��Fz���rp�����* ߝ�XAK�gT�tc5q:����()6"�^���j��n��&���Ӱ�����I#؍��:A������]���O�0�`�,MJl�H�@�QW�@�Dq�`���-%E�Q��Vl҇U1,�7���G��_wL���j�����1�}i��"�A�����>B��wz�)h��h������·�Wj/�Ї���{��w+�a�^@��6bL�����|Dn@�� A���Y���v�5�q�FzפG�]�Y˱�nD��U��nL��U$��1��W�w�ا��iz��v]�|`}k�0ߍ�:�:�,�Yl���X@i�Mf5���>��e�����4�~���H@�2{�c �7!Cc7
�X2�q�zZ�stc ��l���F j=H����骘�2mR'ZvW���]�\겯���˹#4}���F;��sY�?}"_׍4�YpZs��W�5�ѬFkn���i�톄����2/ۍ�����v]X��4��0�2~�Hki����<�����pG�����[v���TY��j/����"ﺧ|q�tO7�O.�4�^o�un�h����H{.�c�qr��m#��v������G7�Ϙ��p�TZd;gp	��3�+����3���y� ��ӻ�(�{ԯ��;%���IkaܲW �O0�קȦ؂��[�Rh�@+9H3]<�s"�ԍ�3|N�Q�1�fL�g@�B� 㱆�+v��Y��5�	_a0ň?��
�����W���W���]�د���^�QҎ�?>�b�v%���)@Jڇ!��V��j��TB|�������* �;�ҍ�f[額��f8�^l��@��~�>���^�Fum7��X5��H?c�h|�I�pN����^|� TRr�;�"�k���-���$/�H�^�D�v�4�O��"]�*� ��	`�T�����|�.ZI^mO����|���+_hğ��U� Y#�����1�����	`�!rז�!����6RX��?�]#�����yҖ�c��6\>IC��Q��h��}������L�h���ɵh2"���]���I�.t���pU�R�^=p�Fq�@��ԛ�I�I_{7r2?CE�D��=@�Dg�Nt��    �PE��}֨��|]�&rq��NR�ե���+t�(b�q��䳆���bE��:����X���˚BE�G�z�&�q����R/|<�j�S�����*j�Kq���Gc�N7�Ov,5q���i�O�����&mRu�+�(ȐՍ��o�K��d�@&,'c����iÔѐ���(#�<Q^u F�y2�Jwa�<4$l�-|��]���(�Ţ�n��&����Q��{���E���BQ�M.J�I��36�a�?V:CXtm�&l����/�Xc�#�s1n�0b:<c�nQOэ�3����n�h=��5s�vu�V쪖�s�꠪�Z��o�2�B#kUFƚ�ުB�E������X=ϫ@��'mR!��s��Ud�hZ�"�E�m�L�FSd� �5E&	���3��p�2"��`�{�\F�ZWU"��<m��F���#�:Z'�GQF�u�}�?��I��j�%���,t��� ���n���oߵ�y�]��C��w̰	�u�IeWޡ1b�/w5GB�P��!��*u�GȠ�<͔Enimʢ
ӛm2c� \s��J�]֣C5.���hC�f`�����j�Z�6^g��$��a����fj�絗BZi�%F�y����9�Xc����3v��'9�y���""��Z�~J��N�9��Oc]o;��
��m�|�m�dZ�^�LK�I���|�"+�ǡ���h��	&�vA��}���55��ǭdЯm�7��hpU 7�A�F��j�_[��;as�]�hUK1�{wtԆ�׺���l�
�/��#���j�~�\ڹ<�A؍�3����䢝�x�:c}w�ٰ��ۅQ�"�ޱ���%��A��{����{�f��\Z����:�;��w/�1�������H���8�����bjZ�����bj���H��+w��(�F���9���0�ڃƵw�b���U���^��ٶw"l���U2#��w+MWS�d*�8;cۘ��zS`jsD��u��y��uG|��?K��hT+n��� /�޻6_�t/Q���{B�pua�MF	A�R�oA�Z�\��M*��Q�.��^�Z�]�bŦ�R�X��D(�2=Y�f=��o�b�u�ka�SH���>R=�Z��Ҧ!�vKAH�겣m�tн\d��*�� _�vKA��햂|��LU��T�$�6SMR�n3�$9	��׊l���wG"��G_�<�vV:��8��%������BN�K։��R�m'.�Kg�i�~�ħ�~��Pn`㞍-�ჸ��5i�;U�v�x�*ۮ�ީʶk�v����o���?9��΅x���6�
���b���ζ3�ݏ�lɧ���+q�Tg���������9���V�^�"��Q��������ut���ƣ���2�Ig�ǣ�?�s�xt��F���Nϗ�M���'Y���I�����xXf�6���6������_L�b�e��w����6���T�~6�QT77x��e����
�l�7�GQq0�2
�l/�7{���i��zo�>˕�2Fe	q�5�
0��R?�iT�����p��Xyxq��>j��^�\�(�͝l�d����&�m������he�#GR<��P6T�S�T8�jGS�M�o��P�} �|v��S�`�(?g����{���+<��}δ�CfD �.�G��oߥ���>맴��g�U���L�
z���g͵��6���B�ƴ�g��Ռ��|��?i|�S� �A�Z�#��]� ����;1��K7|�1���f����y3nG�oڠ�\�?�KW��x�t��7��W+�;��^�������C�kڰt=�s��Y^���|���V����\`����4��������Y?\�(?k��r��l�i��E���|��������\pa�=*�X�����c��v������ȓ�q�XV��'VBy8����
o�絃�
~c���n���C�֍�.gy���%B��xZXb
d�g�Aώ�!��b4c���j��ꬻ6 CWwL�m\�:F�Y��Lfğ%_).4�</��gɊ�_�����?�g���.t#b��p��PR�|�i^�p�eU|�gy�ΥR��-sz�?�]���|[ha��6H{t��ǺQ��׏��-������ֽg�OÜR寱��|��֏���oᯑ��T��!�wV/�k�*6~�FZ�5|7b���ԍ4�:v��ham��� ���x�# -����@K��ƌ�Bo���h�n3k�%�xC��HX��@���\����'2a���0���.W;��W����Y��C����L�����T2��wv� ��?@���=@�X�@�����[8�?`�"&�a	h���.�4�;L��aШ uh0F�ma�g[=�H-@�8��v���垈�hy_���Z_K��ja�.�a����"ݲ��p�A����l� >`e?����9�"����;YB{��0�����rw߿���w�.mM��&�#��j� >�����>3k�ش�u��w��g����M�GB�>?$��r �/��pB|�C��%���'ۋ~�D�b:��D='0D�o��D��[�wU#���^U͕B�s��M7�ϖ�\f�bʥC���j�r}��~�ƺ�2����^��6n��l������"&�g�����M�Km4�֤@j��h�FTTb��c6�T6Q�/��Ъ��Ԑ�X�����@����Ke8��l����c �9�y�h��L+q\�<o��@c ��V�o�)�Y�T�q�[U�qB)Ն����(������B�%[��+��t�F���u>�Xqb�t�n�b�T�dT���i�g���pr'�j׺�%ݸ@[
zg[��r�=,#�4��ߑ�X_�x�S�_4��({�-#m)�O�d�b�gRs����G0�i�����Ƶ��#�M�;��GsݧI��<�Q���Uu���̦��*׍����3�&(]��І����s��Sݒ2О�~g���}�h��:Ƣ�¥��7���7О]��U���%p�;�5��� �SE�{�)CR��@�+��S<eHE:'�p2��dV�f0OL&�W�����>]%�������Jy[��0���\%�sYx�7�ϞkR|Ů���c����֕d[��qn柠��o��-,9��$d<.7�����\4�g{�/��va *���_��v��S�6}��a�Gd�����}�oy��}�\}Q��	�����l߻ˡ�����!	V��!����>����r��������窈1~vu:��eF�S�Q�(kB��Z��)(��x�!؀���!R�\�:��u��7 �'�h�-�4�1}���7�ظG����$n#��v�j!��<�B�q��o�Q���d7Ж�NA�f\�El`����������C|���8�:������ݙQ���;;�J�'	��Z��OO*���eb+����!��ہ���DcG���S��' >��q���c�q�������W>�@��֣1���~�@_���%z���(@�\�6В��:`F��j7���Kod>��L�%�b������5��uˊ�­X�����|���^�z�	������o��=��tӓ�������-�;�zKR6R�#��Sﻜ{2��/�DǨ?��Ɠ7v�1�d4�+����
�r����Jev��?+=�n8��H>��X�-/���¼.��tS��ّ��G�{�Y�A)��aI� �c�zb܆�H�CX��~=+��a��
�s+�H��Q�Q��o	�Q��\����om�Q���4`�{���&/:����8�H6�V%��D�(I��zx�$�8<db��><g���35�}x���Fϙ�&�gre�����7]a��V��@�Wf����m���>�q���7v�ic�H@�˛ᷘH�t��Q��%$F��0К�wYdL��p����N�1�=�����h�z�)L桚���"��ß����0�'�i�����F����2�L�wh7    ����HDUO���?맬��?�eI��SuN��G�3���/?�ho16z�B�p��B#��c�O]��S�7�j?~h�@���~��?��IWQ�O��Ra-�q��Tr9��m�q��8]���};|֮ˆ���t�P�S&Tx:z���*�~qL5���jd��C�H.? �w�< �Py�t����!�O�-�9R�Z�A#����
>����Éڮ=�z����W<�{�����ȁXd�:�g�->��3@]�8S�KE��:^*<�9S�Kɚ����5M/����c�����V����:
��	��m��"���w��ץ'�L�I����o̻�1$�������:~le��G����C*�mm�ܔ,w��O����T���:�����"�u(G�E��w��:T�{2�t���!Ք��1���A�qȠ�r�1����������r���hr���cҮb�o��>�f���1�T���r��4�������B~�Q���x�#�0?]Kb+�	��ƃ��W��x����?_���= �<�6��z*w�j���i��Z�kQK>��µy1�x�
�f8��x�C:2�O��!��/y�C~�t{�i&��;|N��Sg����AɃN�� !�5��.Z�
H= �th��CK\��pk��y-1�y^K����x�7��x��C�����K��C*dpY9xP��D����^7�BRʦ-�����ѧd����/��q!��㇏/�#��e��.`<H=P2n��T{�_Za��v�y�N���t�۬�bN�R6�`�͊�0�����P��<�2�>���3�Q���a�[�6�!��wϐY�_R=h�����u��\�"t�+���D�x�~Pe:zU5h�{��K�T1h�R��Š����M���囄�jCу��!<Q����x�v����x�A��FՂ�NU�cW6�%Oѥ�K���Ղvُ�OfQy�C	e
�>KP~���x6w�ds��Ɍgs��K"��ܩw��͍z\<��>]�C�m�p��4.���YRi8��К�։b���Qz���ՠ��}8'�AQWq���狪����8�x׺���kx*�fY�FU�(�A�oUo��(�A�ɗ��h�he��K��*�����9��
�(q޸�Q<� ��qf���Srf�5o��b���zP�i��փ�>U���+J�@�(},�e���p^�]U�8ɮ�r-�PU�6������m��s!@��ʵAQ��!_�B��1W���Q��Bw��S'�l\S�Ti8���~���_?}��⯟���G��O��������w��f���Q��i"A�F=|ڈA��s|N�G����Us�4-����N����/���VF�Os�����F��.~׳�U*�W���Q�Tm~�s׸<>�zs�7�g�en�9#ޜ��!���Ó<]F�9S�+=�t��֘|��t�׹�3�:w������q�u�ԃ���>�F�9^6���Yϖ��|X2���Y�&�{XX�asus��m����Gua?P�K�����5;}�ja����mv⯠�2����ob�Q�	TE^x���}�r��n}�a�F���O��叟~�p7>�$�;�L�ۃ�z��7 c�g�U:�/Wܕ�3�a��3���wN����a���]9�Ms~N�qi������u���%��N�Q���M�L�)YԆ1i�\����Sf��pTZ���*��o�3r]w�h��u�>�U�Th	F�9^���V���l{�x��Woգ�Şz�W���Xe?H��Y�h(1���T���]3�?��>[��@}3��Ϩ'��P<��Q��7��0�̑s���0��Mje��-��F���g���#DAШ��r�Ҩ��r�0�Z�2w��Q����D�+|G	��~��haTG��+qm|��0.զ��Y�5j��ܠ��oz�;[Fu1?2n��4�&+*��0z��^CC ".���p��P��	0�g�K �`YD�`���!,�~������_�WԿ���3��Xi��a�����Q�����ll*�C��D�����߼M��`h��Q3A�
� ��VA$�B���39 ��ʼ �D%䍕1=w�ӓ/����x�)�g`ީ��1b�� �
[�'݆�a�T��B�'l��E�ad��U�9����Or�º�A�Q7�섇����}�<jz?}J ���)���ӧt��C&����:����`����pw��0Cl,���ڜ�h�͛�q�0�?��,:�!�d��AYt��O�MX��y�KL�W�)���9�lL)4Og��H��|�xC�w�x�9t���P�$�CśT�Cœ���Sd��r|���r��}Fs��Q{�\M�7�Gl�I']�F{��=Mo�Oz��Q�T�V�����
�QWR�9���z�0����3-2��d,��gw�EAwQ��r�����D�`l4��4�%��ȇ
X�Zh����j�-.jY��Q�kYs0GeC{Z�����P��p�N�l7�~��H��L�=�K{ &�Q��\�?�d4���4��r��fFs9�x�8��f���W|����Ὥ��5�<W�'�X��É�7�0�hha��L��?0��=�?�kg;4ɮM{�6�i�]&E��цL�3�� "_�綡�`�wF�\D���\Dچ���H\7�8��]� ��9�
 ��n���*��HdB�5�(�]�Hb~�E.����6�����5�ڱ�'�G���("��e=�*$��H⏰V��H�Z5�a�3�0��Ҷ=�ji�n#F�1�˙R�{nۙ�7�˙����v�*��|��iN#6kmӢR�j�.{���A�AD����~Fs��Pr:kh��.���)��ǳJ�{�y�-o"r�����m�C0Y�K�,D"���/���F;���� "`�]�y�G@O�?��&��&�ǅ��aZ9�F����otGIJ�pFh�(���2O��������,G:��EI�����It�(�,�u�����@:i(�a�6�;������
s��m��T�eqL�+鸌!�,U-��2Sk�͑�zh��2�k��:��~��(8cW֫.
����8֘�nw\ra�L��*�b��xOp2A��	Ψ#�h��u+��'DFh�?���-�g?b�����9���Lh�O�{�ͼ�h��=[�?��b�Dk��*��h�҈��{�+�7Vf\Aݯqf��?��Th��lIF_�.4�06���426����>�Qx����?5*Ϸ�6�Ҳ�Y8�˙z�A�eδ�6�6������"s�0:˖w �w3B����eK; �󩻂�Fe�2؇F��ZL�����K���i�&m1�h���W:��mqy��簞�:�W#�l���w�j����q�X��S��,�����������?Z�t�=���U��4���'_�u�=y���쩇�����Fc�+�v ����"�Ȩ��N�k���*Q��N]�?~�,Uld��1�x/�.:�S9ϡy�a�ԑ]u���y�<2,u\���F�� pe?�0��"�쟫.��*��vo�}A�e���/�ou_P ��*{�����V�	mc��!���N�7z=�Ԭ����qw��xc�Q�{!YMe{�jz��E�����3(���GX~��S��f��T���}�I¯����h�0��i��N����O�����1�[�9�%���iSDأҦ�HF�M�HT�c��c	����v�`�$R���!H�P��@"�X��I|t��6;��1S�d~��G$X��d��^h�����0E�B$ ��r*�2~ʞ66�UXϠ/����'����D(%c�J;
�����f��Еvn�Ѝ���Ø)�w#`Ĕ-m�\�c�:��� ��� ���X"y]�D�� N|������{ht���
��#�친�';g��bɏQS�R9�)t*�峝R��`,��
.����T�H��!�+��sD$�M�pB�  ��~��D�D�s�    �(���|�����>��K��� �d�c8�x�{���|q���JO�r,�GV[��4U�E�ա�%a9]�a9�H�d�&L�:]D��(Z{��C�p��:��&BW�a��E�1u�é���6�'�����m����,�_P�No���U�O&��Ac&k�v3@L~9�3�l��q_M ��m<W8R�Y�uL����Wa����SwFHB�B~��$@�Co映���^i�Y� ą:f�};$�6�5P��g|���B%�������tċ�tLOFHx�o(�4��kR��qz#+,/a�OI�$rS�.=���if�9;��9=��X꓎1���'u3�q�=�0�t&<�]*y]�BS�艒�ٴ�=�K��0�ܾG��Þ@�7fc�־�1��ԛ�}�R]�I�IH���D"!�6�sL������l�ͮ����>���`!sh羵�C;�������i�Ã��cnw8�9��X����.�٢O'�Jg� �.���bEa�QR�o�aNQ�-���
j�1��Vϩ];/Yͩm��sjۮ��9�mW�bNmەa�K�v%>�Ҷ=����ۃO��������>q�ݸQTFL���ě�;]�VtI1���E@���[t�h�n7	����=�ILS�s�d�S��I��s�}��H7��N���&��c(�!����'QT��#��$�#�9���yD?�L�(��쏶���=�.���<�
�1��pm�aRy�4b=L*o&a�ä2��z�T�L���I�Mз����X���6��zĚ�y�
���nI8*��|�V���^���/�+�.26�ʒ9L����	2��h*]=�<\��Bj�1�Jk����?�=�����3^�x7V<&�Y���TYvh�"FTYv�~Au��oؓO�u��ہ��o�+����G=�x������È*#�Ƿ���B��1U^�,����V>�#�,+;���!_���&��/��iXn��U�%,���C�>$Ic�8�������v��y��1+	l�p;f%�,���j ����;�c����a��eq1G�eS�h�>Wy�,u����+Ol���2F&N�a����/��_�x�T�K#�qL�M��x,�\DV?��2��4�Ø,�f/߽[�|h� @�@`l��f�C��5lV㳼�i��"_R���0:�P	l����)�,���V��czo²�P ���K�꿶���dmX��y%�?�|f�����#����!�?Wͅ�m{ߕ^3,9�VX2[v���Z�my�����f|w��4��_)�����-�Kl7�ӜZ�]�pfwS��Cs�8>�ؖ��dpv��a����5��&��Fu�eƕ~_�S��K+t����]WN�Ʃ��t������e^Z�35u��������w6���C��#��e���P�)��g̗��7S�F}�p��ywF��S��9s�(�%Ñ&m���!��/�������<��������gb�U���y�bF��W��3� ��3��bF�x0 �mcrSc���������Pw����~fD�M�3̈�[����㍝�� �:��B	 :cX���ܻo b��7����F�I�����"�ݑ�0NLZ/I�b�z㽁a��4�-�iC��͞63jLZ)��N��Fv��M} �wn���87��l�70J��@��D� ':h�3�xf_�"�y}_�¡b1�Q� }-��I��[�0ɦ���	[n��}�J��O�"��Y��d̛v���R��7+`�����(w_�����u찅̌9�=����+��Y���;�>��Ӑ�v��GwQ+��g>��O�ߛ��OW�����5�N�/����u7�E�	�|��#��i�́����f�M7���x4��Nr���w�����+���x4���6v|��nRǷ^��[�������0.�����A�v6��Wr�_]̒�QlD���2~�ccr�#�}7��Kh���y�n��N�D+��ƕІ�їc��]��Nv1�o8���?�։5^�����%��N�o���Y\wE�����r�{|Zq�u ���q��e��g'�|�_�������ZO�8�����^��5^��=8�擔�yc��43���.�m�:��l>���u��'�W���d�޼�u�hU�y6�:���SOsN)T�v���J��646-�%�n#�,�ҌY_7Iӱ�snJ�#ir�)�v�Ӏ]�,EW�]��yV�W��ʖ��M��Z�l�1�'����8g3:��%[n���3����:�y�nofI���E�\�Sr�A �&��������ǲ=���`��]l��mq�����us��R��T;��%\��r��E_tЋ���b�j�ƹa�&���w��7/�1��`h�8��ڛ�^���=�{K@��j����5`<�K���iNӹ��WG����E�J<@9��s焉;t'��)�]fI�b1����k����-�q��)�᜝�����M%!n��JB�N�U9�ۇ��T�s�� �N�&���S#��L����p����B���]Ȓ:�V�\-8 � �0"C7V�¤F���\.�5i>�k�-8���1�� ��@� ,Bn��+�K���W��G>g˷TD�������L+t�J2T�a)�X�?	�B��h<�fj`��A�\�'��N%�(#`x�G��6����F�:�0���t��,� -��t��Eq [�v�~:���M:�n�8��*s����Mj�0ּ�k�Ɣ�|���[ؙN�#ފA������}`�~pgm�`�P)�hs+ӝ�~m5�7��&i���~�C���)ߔ�N��9����ڬ�ܷ|6��M��̧0�	�(�Fa���/�)Lp���!���b���Ue,j���KLɔ���������^BïMm��1�t��泫[[dna�s�.m�b�7�"Wѽ�Ɣ;��4���"����,�q�&��,�b��k�v1�T㱏� �"R�㍇	�v}惓����Oi�G3Ә@c\-�pӘ@c\��5у/Q\��o:�θ½'A����ЙC�|�a�4�X%�
�] ��g����a_�$��)1�����$���/F=���%��%�h(G�r�:�kҔ��\�Ň0�A�˳�9��n� �� o>3�%q�8�8q�ŉ�-�N�GF�Ĺu�*2��ɪ2�kc(��7��ӆ���Py�y��{�"+�Ț<�J9��m>8J�Ðz�n���8�ϖ����Dg���4�P�w�M��r9�=���i��۞`r;�9��{�4g�S�)��L�iDc�K�~���b�����E�?M�,7	;�O�*@;�Q4T�I�� Wˎ�7�&b���3�h�MV*��.�T��T�����.?��O�-��77��Zh��e�^P��[�.��4f�2�.�~}׃�}��i�Ќ]��Du�,8?���`��$9�(%�_I���~;��5)E�Wt���.��|g]\U,Эw�-�Y_��C3" z%1�Zd煎��X(j_��h�5N�gh�"h�A�S�[� K�HXf@�Aȫx���,S*-��B/[Z:�M�n:�+�d�Y^�� �����"y5�4T�� gBr�A
�� }��?B��Ҍ�mf\��s] Zy=v8������V��ï-��$�{&�6�f�L����v+��U���,C���³�4(ne^7^p�2CKq�2�U����0�풚9�,�Y��f8Ԭ����,@*���"�9�t.RѥS��"�:u,Pf�$̈Ge�M��&������B���Y;�Q�\��F_�%���e%���e%���Ky%�wKZV�̸I6�J�9$��(7��֒��8ƛ�FJ��FHJA��i��t'C��[�w�%�&�8~�%�;{�w:i"�Yv*c��Ne�S!P�/�	�,{kx����>�����S���TUt�=(���.�%�J8e�o�����h����+�r�����z%՞4�    �W�o�kYq�s�[���	�]Z48���K�9כ_A��X23���FV�@L�:+�*o��Y�U���T�|�mc���Ȭ�&��  N>~��d��.R�=M-��j��S]�d��5�U���T;�Qn�9+��l�Z�=rU�ah�k2�N�Zk29����%�����eѳ�����-ں�0�
���g��V�1`]J��U���{T���T5�	%jS�pg.�6U�0�zV��0��%գ��E/5�[�ӽ{��ӎ�OF�z�ku���U7�i(]uéQn�O^��U7�	*�V$3��d��\������+�Vb�
x��C�)�(\@#%z$�H	�ˤ?�H	�.�)��e�)�#Y\�Su��dp���3`ԩ�a��:U<��p�*N��N�Z�����.�SRF��ƿpK3��T����>���a�[�gjg]Je���Y��y�|����'ާ�u����-���Q�<�Ri������R#$S����+�ٓ��)��%ϝ�=]�ܩ�8w�j�;�=u�D{'@�;%ڻUUt�D�Ĵ�hv[0SR�J��8a֣JFy�JF��zT�8�QR���*eqL;+��r�+Ź96|��N�_��I[\�h)j�y����xQkչ���֚*ü�z��U.cF�5��
���]:�s}3r�Zs��O���c]]Aś�h2+4��K���7��t��a����U-M�I��]�2�&���2��+X���|~J9�ȊOO��>d~����ܠ�$���<23Ɇ��#���}9��H��>����r�H!a�}&���ѥ�ôц��c�F�Zk� _��+�9��F���p`�G�6UP�$��<���7�ڻ�kkj��`����y�X�Ycy�4�ԪG�q�=�5��T�t�=T/g� �C�5J��L���iܩ�����1��n�8l�*�EI�4U�.�p#�S�ۤ�}�J�ԧ'�Ң�s�jh�,i<Ҟ�`��:�I�K�d��W�]l7�L��t����z)y�񛱩��)�F�ZK�����>�\F�ZK�hq�X޻<l�*�:ItYu<%2�R�F�wX�q�Ҩ�,ovu�Q�!�N�2�̷�3̥a{m/$=�<\��h�ʴ�/9�%���i�UO>�0�d�C���]d=kp�Y6�a��T�\pk�!Z��TK_z��B���E�Ͷx)�Q�R\^��U�\�ʻi���4�U.s�
�!1)�Z���؋����E���V�l�ը^�eM���`y��8������u�b��1���+�(�������B�����(ȉZ��x�jQ1���9*F��#�A�ޒ.�t�U+:����[���歕���1�Y��Pgxs2�xYa+�N�jd��;��0��,�t�er'GPF:�/fё��Ѳ���af���1�V.M�e����Nˉr�i������ZRH���FU��,+���@,H�j�.��ʣDY���ך���1������ ��6���2�B�eet*�Ãa,F2:�����Euaf{���D��4rր�s�b��!��2r�����7MT���A�Ph�p��&�p%��;�-7�V����G~�F��HZoiNm]�/ZI��[��\aǨZCg�q�j�E�H�O�����1�֪�8Yk��!�~������lk�����O������))���\$�0��Ȑ�H[v�>�Ag5}(�ͫ:�$vP�L{�a��E�1��Trp=��rqk�+���ok�+�w�g���Ɂ(e��THǸ$NR�ҙGz:��둹��*�<�0�V:�!��!�V�9�5��s'Sc��WPR����R���(���ۏƗ��E�߮_�$^�K�S��C�b*�̤��R��o!��>#t�EWOa���z���<�����O�ҵ�Y髆��x!�q�V=�=��y�D�t�j(�o��T�dygiY'�1�2�'i��(#i�+�̴(�	�qs+�8�+���@j��p���(3��H�$OؽuҢ�D1�� �<��Т���&�	b��&�	��7�r��c0�A�]��|�ww3���#j��(J8J�$��\�Q��u��(��~/`����մo������C����΁�Jf�Eߝ��4�)�freH����uQrv���t�4b��K����N�`�I} ��[���v=�6�4�W����� @���
��ɸ_Õ��SF�vܩ��пH��'I^�i/��|�e�k-I�iF��3{3��If]ن�D��)���r�=ez����S���4�P�[�#\G�4XZ^���dm�_��'��cX�({"e\���.�(�?�|�𸉔w�(@��n���M�|��y���:e0��(N��G!(���u�8��v��<J�wS'�Θ���j ���Tc�캛�w�R~=��J��N8.L���S�Np�{r�+%�s')����J	��w����n+�>h�ȧ|7�)<���Ӥ7S��<�� (�!i�^�ߚ�@�\��b�h=��
�@� 6�K��,?l�U��,\Uf�~લ����qt�5�ϣK�~~k�r��<���P4�&��=�:p3�X��A�5�X^����1�X^��L��']v�f?�7�X^�DeJ���]���8cy/+��m6n��N�p6��}�>�5q[Yv�lⲲ܌���]�����{]99ArA~����#���0���Β.df�gI��|.�&����5I0���� Ą�V��#���rr�X�{cY���u2-m��5��ԩ�M��T��˝lojxGK����=������żw��y�I�,e7��M+��ąe�9T�����"�`Ҁ�U��c+r���}emE����{���VV���dJ�)�W),�G�E�������}e�r^-�bx����f��Ghd�qrjĢx���7�ٸ�22�7� ��rT���)�7f=	��o6�zj-ƐYO���2��*���Uϸi�����3c��I�,]�ҐA���� ����Lv�e33^g��gix������BqG���k��}W������FAK��y/*Ƿ�'�)_{0��������t�S�.�v�6�҅W�>�D�B<��"ԏ/�_�}fZ�.�siU������4���8c��[���^v�1w:$0�$y�h���������`�[��;���ц��fd�P`�T��]>�$f�β�	����؅:[8%���5�[��<��zl%�I��	@.RI�_�`���ꍉ�B{�u�J
s�I����b���7	�&+דxU����:�i=[�:h�FQKsU]����\ǿ��iӹ.V	��ʾ�T��y��KЯ��*J���p%^�K�U��V낔d,ݛ���7cG�K�k]��q0 %�?�E)�����#fPq�q�4��Y-�����Ԥ^�V����MkN���<X�D���Vd�L~�qj�h7�c�d�E�=@]�G�CZo�|pv�<�&�%�<R���hIZROt�ߓz���%�R�CM��*�]��Qi�;e��^k`��LI��F��J�?v��J	Qܻ0%$q�"v8�H�����I�#���5�.a�;��z���M���b������:x�t�ړ���^�280��)�<�v�ړd���|>Mۻ� ��P>d�s].�;� 
I�/�X�R�z:|Z�JE={Z@(ѳw|�n�|-�;T2x\K��+`�K��r�߾�T*���=�T����E���%&Қ�.RI��,��xI��TI�!>�-ʭ_�nW�.�ʿHs��A�R?�r,���`����׸�A��·d���aM�P�f㿇�����QI��M�%�������|�sQ�<sQm�$� �	�����τ�uqK���y�H��K�5���XìS2j���UA�=X���[��wro-'G�˹��3�te���muҼ9��v�R�.� �%���$\̂쒲���o�e�y��k��j�����߾�ů��$�p��~X�vI�ŌӮ)�`���J�����*�j4v4&r��ȼ����쪍�"��Ȯ(��i��hM�+�ٮi/����.Yp    �47���%��ʳ%��%k)AV�[JB�@oR�hlM� �Fb�aQ�MI7�m�	�s��6����*X���me�Պv�&ܜ��0wW�h#�x�6��h�0�3�/j\���Z�����}��u׹���i�nv���|���r�>��^aG*�s"^�X�j�ا�Y��]ｇ*��컬=T�wݍ�|��O���E]�g�|&H�Ⱥ\C.u���f��A��k{*�Y���C;���d��E-�g9fJ�o��홒�;���YK�I�e����-k^)��R�=��X&��_�er3�X�r'K�e%��ͽ��M��m�T1���#�������W��b�t�ڞ/@4��8��W;wm� *�v2�r����cl��۸�V�7���I740���5�s������Z+�m�Ķ%^!���q%
"����D�8xM�ö�0�����'Q˦�ؖe���ؖ�b��;X�b[�!��0���م+��ؖ��Y�f`&��k�4¹���lKW��_u3"[1_9��W_�D�eK_��'7��^�(lKg�ꕙ1ؖ�UGl�m�,Qś��9l�j��f�@�������a�Y>����5��9�1���r���W.с�8/}8pȩ_僓�ԅoΘE>���_n�.��׆���̓Rs�h���Fkq�ڞ/@}��6&���6��n��6�����6�ѓ9�mgJw��
�$ǒ���ܮq�A-j������=�(���<a5�Gy��5 ��=�f�y���x�^(d��ײ@�/j�˹u��?g�m�ٗl��j���h���'/��M�H��k�h��j����]۔���]�R�1$�܅L���eӹk�ʥ�pF�=Ḅ��B���?��6��Vƙkc���yV:�r|�N\�r9;v�r�͡IMzN�M���6�HS��*��<Z��o��N�#���ވkc��yU�BO���Ϝ��~����֖�ʵො,�Z|z˯eg�.��r&��T�0��|��x�m,ÿv�� *%`��Fm�2-����2ղ���4��Z�xm��9um r�9qmˌj��rhf�X&��LN�~dr��[��0Jc�� 
ooz�������< (
�.T�E��.7�D�L��C����2�ZZkO��F`�q���`ƛ�L���.�(a�5Luy����_�f�B�� �������z��*����(���y= (2��D�9�؈�̐�z0y��o;C	4�����nS��C�z@� �W0�-�P.C�zJ��p˚/DI^�Et!Jr.ݛ�0C��(����.JI��g^�<h�t9cmMot�h���-��m��H7g<l�����:�Fk���@���j����r����I1�.��s4�
/�e�R�PTZ]������(*��|1������`F�YOO��y����)ԑ�h=�(Q�������O) J��u9om�91[���k�=�'K:�3�~�p��I?�f=�~����u(ґi9im�g��Cn`Hrc�i�\z9km�I�/��_����[�V�<�����k�,��4�Yۃ�m9gm�^�fg�I_*Ȼ��r3z���A�/ev+vu�:IY.����7��l
�����!��%c<9��]]��F/'�mʇ�gr9Qmc>�z9Om���o���-y�%��)Ì=�r��P�)���V3�(�)�b��n��n�z���d��M�!#�"G�\��JI�����T�F��#��J���*K�؝.�����2��eΐ)'�)SN�,�ў����\�Jc��ٕ'���͙j;�0z�����������Q&��U�IQ���qy�]헹f���ո&�����ū�|�򝈫 �vѽ�h��ZR�UX�$�*7�%G�Ե�=]�HE�(��H%jH���U.PI\Rśӆ�G4*�Ʀ��9�7��*���Z�GI⩍���V;�L���*�HjO��� S S�0�˨�\C�D��ԔlH�k)��#W�JK�����R��G�f���,Z�b�=�:Y^�:Y^�:Y^�zk|#ur4�AWX.ZI1�Er3*�xW.Zy��4�͠�-7���:��_�^׆���qp%p��(�9�qo������L~� T	A�"��q����|�����ބJܖ�#)���t��z�7u���2@��(Nw~��fT8f�y*�>����A���Tv=�r*�>�.�*Ssp7W��냱� ��m��U�z�X��:uq3)�OH���>�N +�8����S:�ݗ��J�l���]��-���f����ƻ��Q�����+E^���X���_���$p1o�F��>��ʤ�>��1$c|=�����k	�kK�M �����Yw�ȫE��1�)����[4�Ӟ���?`�	k_7׫�t�n�*z�q�&�r����u(�YLq�V�r����u�����<L��a��!��r�ښ��l�3�&����r�hz���6 �5_��n�9k�� �✵�UB;��'���8km�����&C�⭅�Z��R9�ERa�i�S�IT����a||�(���Ms�.}�8g�;����y`���;��*���sՖW�K�vY��r�ڒ�EØԂŵ�K*3�	��o��qj�/Y��{k�%��b~ͧ,s�*�>=���M>p��N?���������9mm"��ӖYN�yk����r�����9s���֋,^�Gݫ,y�7p��0Y􁮕b��rhA�j��YN^+���zr����c�*��4�.��q�������a�4���:*W��F�B;������4N[[��i��U�4�N�[����Yk_�ra6��*�چ�b�r���*Av��lZ��ş��{A'�-��w4Oz��w����$�Y��r�ڒwm>�yd�ih둿鲏U�o��p��*�v�c^M~H]]��iΪ�$���.XI2�`E���ao��$�u�,���%�Uw2�.����:�o'��]w�o�T��o(��|���+ߐ���=���Ww��t��N^���� u
M��g��	8����=�pE��`�� +B��.��B=r-ܭT��2�Zj����rI�q�f��%�i�N�\Ao�n�*W�W���d�aL�nK��7��~@����Y�&���I��$jE0�<�Zi�ڃ4����n�+y����z܇�V^I���K>]�<���
���]آ�R7�U~�B>�*�<�0-x���(kCU�$\;U���e|���G�1Pԏ_`� YxG��`7���\���2����W����2�7U���g�Vkisp��-m]fk�y��vъ*�]�-�}���auY�(k����8kC+S�]Z��֥���.#��<a�FYK�<�랴��<���P���S�zb�j��4�C_n��tIr=��C?l�����~�Hk��GZ<��y�$E�sk�Vh���P�A�>�\�t�8>� Y�[㻥��p|���;�2>U,#�i��asm�7�����,�L������^���q����C���^��`wXoJl[����k�u=a�(2�i-��_�݋6 �\P�Z�J*?��^��Z_PԪ|n���Z���F��{F��W(4�4�XE��1 ��)�AdVx{���/��R$G4@�����_%y��o7Y*ϝ��Ry��7@ym2*�ʁ�hg~�Ńe|����${[rl'�[
H'ٛ���,�q�$ma��9Z��o�;2*߭t�H�Zw�rG:�E���锨o��)�M\�R�>��K�K،U��6�k�Z7���u��w��^�hq�۱��>�nPL"����فJ��Т� :@���q��C|u�eon�V`U���N�s�ܫ��'Y�*밈�zU�aq7�k�:�.R�aų8�הuX���(�۳,k��,��m7D��e�ܑ������#Fw�J\�%�q��/JIW��ln�;`�T
�k�����[f�{$�ߤ�n2��z�an�mGR%�d��!��F�����U�}�=y���i����g��cn�Ǝ��,��۔f������fOi볹���̏0.JyߵY}�<��#4�Z��~�Xk�7q���Ҩ    7��}$���˹�f(�.߆���� ��>��v�	:2+!
d�:�Ic@p�܂��} 3U�-�X�[Q�8�p�xkr��#���ת|$�3����>�嗤R:�X��)�\n6E'2�֏�R�����S%f��e�J!|��Q�+����r=����R4���w��-0u�ޘ`���N6C*p��l�� гO�]kd���rp��am�쇖�.��N#����SN�(k����9�����tvj���O;?x*��	_���v��͝o߯]q����7���ɷ��L�ˈkM�WJނ׵r���7IbS3�Z�����J��]e����ʪ.Cc�`���{��6�b�nL�1}Qm,�(Q>ָ��64C�0㾣���DdT�a)���ߙ�a|�Q�S������Pb���DCֺ�Zt(U������F��������|l�W�5�2M���[_�O	����Z�y�Ϯ�����Q����*Ƿ�:�Z�=�q��wg_=l���|�1�Ҁ�0ګp���<c�`T�4�y�4����+�bd��_����Z��&i�!K��=c�堇�ݖ��J?�9�AHc��!��sVc����f��6�ڰ7V�am�\jl\����V�y�il���<�1�Z��.��˯S㪍��w�FW��I���Rf*i2�Z.�j�����i5�i�{��cmH2w<����c-Q�5��h�,�O{lz�$�q�mz$��6�
�ߌ�6�����j��t����6\�*�����T�ZFVK�e6s���k �|
��������t�}\�����\�
{<N������C����従�j��R��G.JI�࢔j}]/JI�Λ�R�x�"�!��@)_�0�Rr`���N�� Ny���w�� H��/����Eg��'9�a�M�� (	��5!L��;o��� |�M_ŧ���<�U=K���9��;����9����͋Sn����ܹk�(��I�RgY(�SO7S�������V��޸e�#��TIx�@*��	���+�1'�Jڱ��ϒ�QI�y�a#��qg���(&��@�x>�}>���Ý������L!C\�J�I�Dk�lR0E'	�K&`J:J��Դ8�̚�'έgU�;�u���Ӵ�9t]�R˵8�NR;��\@M��3+���sԎ40|)�<q���#��S�;tz�P;� ����q��̀'�C㨥�hz���Zo�� �[NP�;ɱ�(wh+}*��	��{vMF������)���������û{C&w�F�G���y��_�
�>�Sn��x�l��eZW�5[�S�5��8��\�{x������f0k�(�<4�)r��E�b��o
~^���?���>����[��H�3����2��NVQ|9��FU��r��_��/�+r)!Ooa�I��':|A�<y�bM���)�6���L@��@��L���O�~�$'����<۞;�6O�N�������mf'�˿w��K��m�o:E����7��ݳ��4-}��7r7�v��Jx'��!Τ�6W�����<��#��;�ةjg>�p�[�&J�|�v=�1��E�VW��EI"CC!I"[O��H�%ox�<p�ݯ�:��v*�qc�Ig~{�Q��|�*�8,
�(ᰨګ(��z�(�๛�V%��Y@):B#M)y��J�K�`���R�~�|r�E=�>o��L�"�@#.B���F��Hd.@)���fRx���(:+�MԺ��tV�]����	ɺ���ͮ�JI�/>�6d�:X-���8Q��ǘ>�&�y�n!��h���=�ӓ0{J�C�>��v;;6��f��]��nO�.p��PO��)��$�SB}˞{����5RB=u����H��YY�u�)o�!�g/�2�A�%�گ!g���J4�
w���k�N<��F�9v!�g>���#��;�p#Y':>�������k�b�6�<��ɫT*s����hr�ڞ�3:NW[	ˍ���O�7�uIS��g���`���s�~�nC
�5t����k�t�ux~���E�����$7�%�-�5�3EC�/w�)�+/Hq��ܴ��|\;ݹ֖n�2%��ڔ�����G>{�2��ܜ�6��8Σ#A芯?�ݴ�������M7�L��LN��[��8T�V͋����u�Ҩ�������~�K��ڗI%m��7Wڎ�s�����^d�v�ʭ�7������E`�[WX�݋``;���_� �b{��;mh��Y���h{k嚖��'��NN����.=jYwĉײ���)ޜ�ο1�v!�D��($rP�ڮ$rPE����Ak�N���ɉu�%��ɟ����ɩ�C�aԩ�lW�i����n^�<;v�r|�!�f��\��톤9���[2<Uf�L�B-������9���br�>���..s��ȋ�����/�~��V�0��Pap���N���O��j�fnxw]m݌x;ѧl^+��:�E�c8���]�L�Q;��b_:�2nZ:�$�Q��\͐OyH)�6n&��mƥP��:	T��}Ǿ��u�Vݷ^�)6.%��d�W쑪��S�*�ڀ*�����*¼_����C�f1!�'�.mH��ե�䞉�n��i��.��#7!�^��"��+���v�J*ۀ*	A��N����9i�^�����4zN�d�]�I�_�mq%�H!�y��^��}�;9�:�@@��D��o��%�����x�����#�0w"s����މ�A[�}�y�e��#L�eazk�rH���Y��4SSּ��N��F*��r�)'xDb�#�]BZ9�卅>���@���^H0���V�����hc�\�Uv>��*�+w9#ml�+�NI[�ʭ�yR3uRR3u��JI�t>�[Y���5s�d�\�0�	2`|�4&]F�iL�n|�4&�N��4��DŰ��|�N� edUD���k�਱�>Ȫ�����I;�J�v�+���J;^�q�שq�c��&V�������ėG)�
�1���i�Р��8Ȫ$BbtИI���9�DH���L�hO.N�]� �����ƀp�j�\'gUH0�jSR�i(N9��)����ZhR���r��gq�~�3��E�%��~��ZP�R{��V�$:b�4�:%������Ğ��S�1�znB��:�ci�3rFڙ��}�C� _g(ֈ��H'w.KO���N��u���!��tt�^�LGw�#�a�tv��q�~��g?I��dɦ�G�jT�H5*Y�޼R�C�KI�)8K�SrzK�S-�=�R�Tc�Y)�^5��Wy�pr��C�-OIQ�[e}��9}���Dr��d|��<;�4���N)��g��f���	P���!(���fWd=;e4�d�SF�Hr;e4S''e4���I�"�}KT��3<*����ӎġ��?��օqy�tb	�:"������6B��s� &ɛ-��w?��$��ڷ��i�\	po&�[�P6g?���>��}(�P��~�~�?L�1�MQn�۞XA,{�O*U)����G��n��SxZr��7*].^�d�>?�����OQ2��K��U���;>���û`��OUM_���~���1��;���5vE��*���� ����N��f�K����zOַ3�&�lg�/���C/��az-L�8)�O+�a%��~Z�xd���
�ț:CU����R�]�0�J�G�"�(I���hK@����'yE|�ӑd�\�~nmxEoJ'���sa���������ZraJr���)H�$y�T�Nq?]S��+��8���*�1̕~�`���s���8i���g���za����7J(�;�FJ�T�Z����R�+9�FI�T�گ0~Xe�Ӄ�;�q��]�m��+���zk����J���m��q���hiW������m̴+�F�8�O��o���h���0�B�S������zNt�0ǯ@�O��r-���Z>K.��,�6��%�X�/�U��E�5��?��9Z�T�6��%P�G���Ԯ��9�m$�+a����Hj����m�+�u.�v5M���>��t���XD�b������%I��gQ�J��8�1�.    Ղ���CU����=�B��u�R����4i:���UK3���t�����_#П)]IxgIׇ�w�t}H�H���j)T^l�FWK��㫥1�VFXKgC�m��tM=�!����Ut�Xki��%���F'�& �e��kf���z%=[�F\�frMnD�[�f��F[K�\F���	xs��!��m̵��䰌�� �5�Zz��|d�Rj#��k"��6�Z��<����2�֥6)�<��R��Ȍ�v�b�m�wn^wI?�\jI�>Ґ"i���h��H�%I�Uie�O5�% x}�Q�.!A�ӻ��v	�Ų�I��Z z�޺� *���.A�^���G���Lz��N���R)���)2fq���I�=E��<�i���F��[�"�z?����G�W�ģ����!�. 81��X��T�7vz�ǒ�1�.�M��1�.��0t���3�x��U. ��8%}�r��,R�*k�Uʞ�nR�*e�]J]��sH��=���n&c�Z۹i�c!8�b��&��&��MB
�
�M��m㳥DL0F�5�h���k��V�y�M��ZGPe ��$!��R�`W@�I��"�.-K�|��3���I�Z��h UpoZ@��C�&�'~Z ���)�*���Hmi��L>������m��i
޼�V����L�,�z��O���9����>�7�P/���/T��Fgdі��Ӱm��T�͝���R	r�i�,)�5�mؾ�o�M�T��O5?�}��l6����R��l�:v)�.CބL�o�F��%}�M��_�Z/�y��Z���\ب����F��y�E�U<I�6�]�o�����}9إ���
�_�>�z��n���v�?�U�X�)@M����[2ô�-��w�ؒ�{kzז�𤞇T�EN�Ԥ�'�:PM�}� ֤�'�����z���IO�X2� �İ��3�ۥ�c׳���ձzշ*���)���/���0f��a��mNR_@Ux��#e4е��H���F�'�T��e�d������ƒ�%w��M�
����˚��`��JU$n�g�����E@�fi�C�c�r�]��9� ��*�"\L���-6��CYے;M] �C����&(.,��4A��~�7��#ԕv���Șo�J��6��%ꗻ�0�[��Lj�͔�d@3e��Ly�hr'	��N��4�&l�:A�Ԣ�f���[�tJ� h��n�C�k��*�R���r�󕮩'��?���I��!�p���z�rŃQ�$H�l���Ҳss��knԷ�lހ�F}K�V'F}K�Ν4���Kj�R����(�8i�F~b�E4����-��`y󑖯Y�<R����� ـi2:@��pq��.�I���Xh�0\*�Ё��Rх����:x�}7��4`��^��,�n��Y���ʉ.:�V�SkU���Tj�����ɉNB�v�4rTw��҈��?�x���4���*��h�|����䬌�&���2�I�k��t)Skضz����-�5���b��r��Dv�(qi|~u�.--taƉ�ڟ,�q��'Kk�����}�4���*���,_��0H����=��]�k2J�����m���<��kh�'�;]�fz��>�2��&���/׋��ou��P<�2=��|��.7��Unc�]-{d�!����ƅ���\�q�����/N�����qW�sJa����r��ħ���.���&�d�|]�^>�$�d�?AҨqWK���C�ִZ�e������Z��.\E�gY�����֪�#A'uޏ�<��]���"ƎK5����!�F�N�pw��Ԡ���O�ݝl0y�������-E4�\*����tAA��G.Uz&-8U2M9M2���Ny���L�g�)%I�%�p� H���&���5�\�����R�骺י�U�qqQ�jy�ک��uP�'Mw�X��F��Z�Q��-��4 �ʥ�OfA�,��Kh\�4䴄F�K�$�].W���.���F�K����M;���r��Izc���۽���8�o���i���"Qиs�NM�M�U���s��,?�F�K�ϝLY���%�o�8Ǧ�7n��;7l_��Ɲ�ߢb��si�i~���[��n#�mJ��:��1'aܹt�m�hN+�͐PK��6�\�AMKՎ��r��#/���q�a ����U�W鐍:��z�p���;������ī�����7b�@�����O�������w�霷UI�us.U<�����I�!W	�1�R���9�z������G2�l�@4�`����sr�z�F��"� 4��.Ϥo�f�`J��Js&��a�~��L�U=ƝK_�\�L>~��W�޷@kƞK��.j%'��uu)�<�����1�R�t��/�����?.
p���#�oV�|�@3i��g��T��3܍���i�NV�����|2�\Jԟ��}�m}I�� ?ee 2������HO2�s��$�#���ļn���l,��-���F��zN��%��	-uF1�zds8�a����8����<�c����"y��p��OZ�!41J��s*����[���r�?J2�C�?.��bU�E1�����Hu��b�"�}�J�U����Y�.�j�0i���[Mk��ʿ���0�åjW��|�fTZ�_G�&�߽Îi~w�0��_V���ꆖ�e��K�8�ׇ�Z_R�9�
�\7��h2�Z�Q���kL�u*��̓J�k�Y��T�P4���1N���M*G@1��y����yc��y� _$|Eg���D�����s�k���͓������]?���~'�1i�|2���_H:��^7T��I��g9��YA��X����@R&y)��
+���v�?���?w�tg����ܧdd�a�{����ן�@g��.+��> ����e���Kǔ]����^~�6��uW�if�ȍ,���(�-Fё̊M��f%��F��LnNF�!�N�ǅ0J�C�oB�|�N.tI���}��w���"�i�������/���|���f^y𱵙_��0��z��p���;��œ9��e\cE�1�2���:i/�Dw�"GnNy�$ћ�y>S<)k���qR�pɎeh��:����9N �xo,T�M�O��(x[�m��͋b��p߸vWOu�S�%��$��c�n�j�{���v��9��p���cp�ܥ<7�o���g���g���>�e6��,����,)M�z�.7��h��j�Y�Dw��'0LZ<�0LZ=o�ҬMH7o&����$���\�@1W
I�?�)_���T��.�	#�Q�ҎS'H���ו=�0 U����{�����t�������D)�5���b�9�M����B�=�q�9���GaX樬�L��o��5w�N<�:��mx��މ��7����k�b*뽝�S����3"^kV����}�Q�[�q%50Mc�~��FR�/Z}������&@��C�� ����/7�_k��>3[��f鍅w��*�>��F}F���l٘xAbr���m\�k�@>�	&��>��t�S̠����>�����/T�N�-KZ ��X��'�nD���d>�U���xx���\/U�fU_CJ���1�R��a4J^�:�d�¬d���7V^� �{���G��N��]���O��t�����6n�Pj��5o�T���2�ҽ�L����M��ٛ��r�d̼�(�]����9�6R^���I�)�~��c+�~����>��㮎O/;&����7�!kN����lo����J���ub��y����>ͯ�2�^kU�w��[�5���*w���S��B��Q�������S������_�4��0����`���kPRc��������@��;*o4:�o��(z�)a�C+�����l�f�̑7�x�=:v�i$��e�T.i,�k) �s4�^�+0 ��է��2�}~)G�>Ӹz�9U���O1����U���}O��<����Oj�bU�&�V�ڵ'��<﹕P�Wk��j���7���9�J��n�����w�O�M������!�	    :�ml��|t�^�0�d���1��\"_t�V���,no����.II�`��c�}��ߤ�+B_��)�c��W�SZ�({C��0��P��6�Q���W�@	b�m����)�eo���yP�Sd7�����;ƢH_�l�4a��c�H��{�j�������+� ��Y��d�	T��Ss�&�iJ/�y,0MrX޸�`J�/���|���h�/��P�W��g���?������<�S��֔5'Ǵ�S�dS�ŘJ�8�X|iD�ⶢ~�@wQxw��D��6[�/a�`\6*_�"<��N�`Ѱ�1�=��Ϡ��7P�
� M**���C�4�TV�5Cz�a�`.weX3kV�5�K�a�H�feX��u��غ�f~?���O��^-��Ig�h-�@=}\�:a�q�u�����xf{�t�6>�@@�zc��Dߚ��g�����t0�o���]����9c��VB��F�˭�Ha���J��2N� ����%�D���KQK��ӗ�����Km�j�㴳�h}C�g $��mW�����Z�J�x}C��e�m����Fd��4D� ���L`�^�ؗ�>cfľ���eW�t]����K�N��L'SF�K�NBx#��5NZ���ߠ4wy�)�k)����2ٗ�����K#����;�6�UD�&՚�*"e���פU��!_���v�@�b�f��}�5*5÷K�ZE�k��"�5y]En�f} ٚT���*=�\��
�|�H>�������L�����ޤ#���t�1�RI��y�TˁNf�pIkb��T��0h��� �����t �,3��� �Jm���Tg���A����k����4�(6R6��*��WE�Ky�o�+���Ո��9mcܿ��]�/�!7�-����寲零IE�>K@��.As��W�ŝ���Nzp�g���k˶�ԕ��6&üQ�;v�T�*{��$C�b�(�(on�ٲ�7���E��8���Y#n���@8RS�t3�j i�O���a��.�h��ީ<k�u	H���NؠCu:�M:����9pFcg �t#M�8�-�#�K%�%ݝ#�#)�g��t�U�ѩ�l?s*��l��G�[��Īiˀq��h�N�A���T��s*���xͩ�3Z�ѩ���u�N?kiG��Ā�����xe�̀q���
#Yr��AO�g�������Lv1g��+��N��M�ix`��Y�a9�P���%���EY�uR�e��P�a�6@��S�3�nd�.vJ7�^��s�\���m�l�MZ�xr'Z��S�*���NT�Ɲh�ͺ��3o�}���R^� �r�|i���g��&״nSi�m�=Yz(Y50Tn�0UΚ��p��6b�pf�m>���͟��.^��5�pj6Lu�6_S�No9�T�h�?��,L���њ�>]��_��J&v���"x�CG(�H�5��>N�a(�(��c�\��DO�&׽��� l���S������)�0�Kʧ�n~�6�D���@�Fr�{Q��*C(Q�#3�4f�����_�>k�f�?����>q�s_�g��_ח&]���;/p:V˽���緻�tZ��|5�Iy_�`R�W���E����b=��$:�`��l25����[A�3$HA����徟9=pzW�a��[�%?�d����l߳����Ӿ�B���l߳�!��7`�P�=O�	�ɞ�����l4�
�=O�	�����ǘ���GHIdr��C�D&��c��K�lOac�?{3��޻�ОCG#%��w�_b�(�ǖ;�FU+ў�^U���r_��dI��
)��k v7g�w��j��f�=5�Yٷ`�h�\!�{���}�C#3�광Ȅ�����+��=iMǳd1�i4���K7����՜:8T�����u��2e��=@3�i�0���I�e��R9�*7y���U��A��B�ɐ	�닫[�iN��X�I̜<8���˛$�Q��&[�=C��tI��S{��36�߫m��kN���j�����vlH#�⑮�hĊ�z��'S�5׸rk�� Ј���s��n*��f���u��Y���aƹ�Ή��q��9��fZ`��K~g�^-g��shTϡ��,�P�n��P����P:��꧃H��d���u�d���2�*�GP͢#�Y\�5�l��ApM�fۣ�������h���x���G�¤�w��A6_md�5�I(���l���揹Lt��<��@x�1by�L�rm���"�gٸ0O�̂=xKA�A�tz��V�Z�&���?��OI.*�<�+5���Ֆ�"Q��B7_�� ���)����k�����a���'�J�4kA{A
�Ȧ�[)lZ8���p�����VNsM6l�
UNoͧ��Je�k(V*�p_
+U�(���|-P���b� �̃��X�����
�_ޤU:���I��O�Qݲ�س���ƞ�E+6�,0ӔƦ�u��4v-0Y�&]�F�Ҷ�n牕.m�d�J�����?骠H#����=l��%�Ny=�Ӣ���l���b��F�4�\�K��ܐ=~�e�i_�J��Uh��9&3�L�`;��i�$v�z��(=>mE@M�[P������F��玕�T��Ti�g֓�ag��b���Z���bvXe�F�R�[���W[-��B�w� ����`��K�������(K���ҵ�n���Y�\�Nk����YJ�Q��f)q�Z<��1����C��;}p�{��U,���N�}iP{{��u�đk P���c�&���N��iޤ~V i��,V�y�k;��0���{M'#E�4�f5�3�V?����䜲
8#��!-������^��RI�b ^L���	+P�ׄ�����d=�ƾ�@)@)RN^�!� LK��ɪC�w/He��݋���_���L�op�j<^"o���ip�Ә���l'Obˌ&s��ɳ�r��7�)'�� s��)�ʘ��L9#/������#.A�3O�'������{|�gcv?43�����6s��)�%����3�)1xJ?CE>q��)��-��ҹPQlr��)-824���KP����S���#�x�x��6u�cX�u�{S��08��_J��˻�|���pro�^��=fx'xM�uZ���)�Sy�٠/*�&�u���R�k����fM`D���SO!�)�+�����Ӳ1n,�N��mۙ��9n�w���߁;������9Y�a��a5v�B����ǝ�|R��P5g��9XN��J����9G����0{��g�v���狵�$����w�\�Bdf������x!2��`�.& ���I��#b&+�$�<L�h�x������t�K�Hm���Ű�MHWjͥO�v�m5,KU�P%,��Te?��3��r"�#�'N<�H��-̴U���Ypv�6��,2�h4���d@���"��]b���]"cm�k�0��=������S��EVS{"�kN<�*W�����L!���/?a�B��z�	[>������Cܕ�]�\*'k���\J��<y�(����'/>���Y8?�����l�9܊흲�O���f�<�w �����s�g3>doN<y/�7vi�P>��`�Pj��]N<_W38?�OL������S.^T�z��r+$���'o����x�N��oxt�ؾ�x5��;��g�%�w57Qrj๨�P[���QP��)�Kh����%ޢ����%ޢ�ծrXP�o�?35���[��Ձ/��]Iہ/�� i��|:�_k��a���KV������Z9�5�3%�C|�Ӥ��W�z�pk\��]�Z��O��:��J_�\+W���R�����AtvI��&��}��p�A.wnw����uu��0�},���uun3"��I?#2�2��l|9o}��/Wխ6� �����C�6����E�$�o��`a����*&W���MY�s�Y�7#3�c&@0�;)�
�t�)���j(&	����ہ0��x;�Z�� ������I�&�K`X܀_dU¯�H�o1    ? ɢxx^5c��[��2�����$)��:H
����;(&kp�Yۂ�,�};fq���iw�o��y�Y��NÚ1����iD*m�=�/q �!��0���j�F�ԜIz�L/&�3��&-4gC@���)�3zLp�Y�D�����BwR�;8ġ�`~4�����E�(-��y�O��.�<=C�����p��'w�3A��\&����[�k��2b��kO�0&ȵg�0��kODq0?����cn���.KLL:)� �X.&`^��t��`��������i�WV( �rY��c��qe�OO��3-B���ȴ�N|��,>�ŕ
-g�d���=�1K}�[u
*�G��x���d�Q[�NV��!�x����Z�O���~���.���.��7Ă`�kmoR��s��n�s�'ĝ���j�R�3� ��l?ŗ[������<����`晹a��0y:]���]V��CG�̧�
觬G�=W@�\U�+���u��<�Ó����Wс^���g�Ɣ�D\ɊE���ˤ�^����5��de��{;�KVv+�:�K2��>	�f&�_�.�3"��^k<�4��Mf���o����o�C�\�&QC�\�f�H�s}��x��<�%���`��Ӝ��@.FӅ�����[��@.�T���Jv�S�'d��a��@���Pc��{�%�^nK�Qہ.R�5��c���@?�� t��Ν5�~�/<���O��]p
�����'���h�� /�������΄�r'x�lR�_����
��P�I��\�x�	 H��r3@�����|>dR���y��Ha~�5)̯T�� �B1���W&=�θ^��؜8�z����}|f`��9���}a�� � ������1���p;O���mub��$����0�m��ra��0�������p���E�^	���<��/��L�)o�9���Z�$��S���W��w�W9����)z�Ϸ���eN|�a>Îs5� �[K��������ʒ��|F�\�Y	�e�� ��i`@bg��O�Y���z{���-���C׭ݚ�y�x'�K�si#�_Z�w�[�;o��\Y�:j�=�� ߲���x��c��de�����1+t�kV�Ϋ�%{�B����,��ħ��t��u_0�̺��RJ0��7�o� ���Y\s:�|��I�B�ONrT�[it��T�9p*p��`@MtHg��̬2V���0�ɀoH�wS�ɀ���%�ـoH�[p2��������f�y��@F�-�kvzR M�;h���oǛ͙51u���&�V�;kb�Ә�3�ѝ������ͮ���:�� �jw`����)�q��왉q���=�3 OvZ��C4^9�\
� 5��6g ��,(;�}e��8�܊��H�)���=�t૲qр g�5�̖�k4і�k�����k�>� 1sc��<S�����Vi�������{��zN��S�H��79�������Xi��$l;�-xgHyLa��s���.Uy��&ΐ��!���&?�a� ���~�8sJ�q	�̈���������Q�t��q��&���'�O^��	���Ҙ�.�A��z��(M��1�Z֜' [HZ��	���o_��t�H�Ag��Élx���3�'d���3�΃_d��A0�aO���\�E2��숇��y'���9C�7=c��3�{���f�ӭ0�{fj'�������==�uFj����M2�=�C:s���^o�Y�q��)����x��	:� qR��y(M�����<ԗ�<T}�A(�3�9�oB��i��3�u��T*�K6��N&Ώ�z���Q��vԌo���$��	Ԓ{���"!�3��dѪG����n��E��Cŀ-��r�pQ.��ۃ\�;H���\���7k3��;������-��\�"�k���e'ˋN�{����8ֈ�]�>X"g��x��M�q��Ș��\�h5u�ߴ��~`.qM����-qM*h�knG�;T��;]s�5w*s����n���:l-��w
����˻�37
FX6��Κ�ra�	��E'��qd���~��c&��j�<v��l�c��:O���P<�_���~� w�&hal,kh��a���o��^�I��!���x!c�uZ`��oqg[F�:0���[ 0ߋ��@^��ВathK�H,��!�������B�������q�(I�"�"��х�?�C��1Y\vD�F4��0.;2�b�ct��FCw��|`�Ve��R�b�j�h!���j�h!��Tj�h��i~󍷼�CO�۪�sg:ZM�\PVc.�Vc��$V�8\�Ej0,��c�&��u�R5)�.<�2A/F�:%G�	����u@og�����,(��΂�t�H,�M��D~���A1�;��',O�1wJ�b��9Z���u�˻����� ̷�� b��g���3�9�C�d`�hcK�b��(f��!%��[���;���.d��{ĺ�V��� b��q�|Qk2W��d��@�d�K �\��2� `�u� �u8~��+`X�Eg$OT^����S�����~6<�̓��Y�/�c2z�Ph-�~��~!Ј��:B��ک�|0��L.��OW:�J��?%xb�D�f�~��6��Ũ��9b*��ts5#��o���n�T�5g�T�q(41�T�M0�ʠ6)�K�36Hj�16H����G���a>�p���Z8��m%���mt�d�i���~ɋ8�pb�U����	b���*!�!�_�
�c�o�k�-����_�&̂۷�Sv-�k��ç�ܾU)���JLQ�,�1�[�9�g���Iyc&R������~�w��)Y����ꯘ��K�y}�З�̜�7U�,�뛦*4U�4U�:렩T<i*�x8�o�����s�KVN����C}���T�g*L��4U��Y��V����z����A�[�V^b�_d~mQ��~�Μ�7�·z,}+k�W����x�48}���iL�W*�SI���t�N�� �����f7h}��",�^�ҤI� ��Bk&8}�޿s�x��tۜ�7}@F����挾��q/�;���` I22�1��N��ő��_���Ze?cU$�)&l��6j�	u1�"&�AĄN<���L[uf��RM��?iAކ2����4��&��uKL�3����6D�j���9q.ߴʠU����;f�D��劼�oz���sߟ�]$Gj.Un�x9�M]^����)��ץ�������Bu�����b��x��{��O�oy���"1E��B�p��u�IJ
�m1�,Du�oyђ����o��0�(�!aC��7�y�7���&���bwIP��� ���>�)|�X�p�;�ok"Gs�;��8�o�pфN⛺X�'�Mcq'�,�i���
ߢ���h��f��oy�2G��n#��'�_�5'���ܚs�^U�w��)|��J~Q؜��j�<�`�|�����S��v��n��
��÷�̼0K~�؂ŷ�.��VkI�� ���
�ۂǷ����ʅ�S[G�G�4'=�oy���L6�!�@���÷觻-|������˓��·���b�$�%Kmg��²Zگ�~��/�h������W���W]-ޯ?t��~:H���8^��~>�c��Gϡ�>nP2�/nA�}e -��r?rI6�eF���_����0Qm3|[���=|[�-f7�pᄳ�o�N8�1�dߖ��X�ŷ)��� ?�6L2�p~��l��g���� � �$L5;?�Z�C�L�MA>�x&���zl�3�!�0 �8F�݅$�@�Ax�@#f�apÄϐ�������ف3L�bBR�$���]�\k'�|�餪��$��7�%����5�|"��y�w�9�oW�D<�,���.40��ds��Ԋ/d�;��F��������p��|#����1�3�f��ly���+��>�Tb.6���i	"ߦ{���mj���%c��ǖ=��    ���o��Gm!f�� U.����ץ�����piO~�����F�������v�5�K?�}Ӝ�7��]o���_�>��?iy���L./���M%.������}�����q�c?~�[���\�b��9�!�ϛb�V�����0#d;%ą۹�Y5˞���F�q�V�r/�xp�)c3�F������_:9���B��x*3�3p�$���.e�i�oP�I,�Ų+���4C,o1m�f�G�t��⽨$�f���I�������S��kmfPZ��kmf�ز��Adc�LO����OZ�e��҈��ʥ���.Q!řK��R��ϓ�],;�y�q�b)�L�X]*;�IkH��\*��E�.��Tg�c�!�����8#��G�f�WZ����7R�T��ͧ����$����R{:_�u�⌊�Q��^�gT\�P�BOm�����a��-���ON�-s_Dkj�%en1����ʰi�}�Ki�Ml�pӆ�6܇̥4"�!��ӭ�xRG�F�b���%n�7m�ņ�9�Xl(�J��iQ�aBG�km�7�8( �mȏ׶�{��4��x���=Bv���g|��m���������"��6�?������7�]�xCc��;�c�x����g���M��������9�v!��G���U�u�?�f�|f�k�i�r����|z g�y�v{�]ک9|70����E�xRG!n#ċ/�ABϸq-�PǋU�4��S�����E�lAq9����T����:�-�7����⑗��isu���x���<�� ��4����-\V�	q�Z�Q0.n[����b�gʅ-��k�.�9ľ��.i��S��I�Y�����Sq��ϥ��9�;��0�w�q��>��Z�b�c�1H�3n:c�t�Mg�θ�]�z����+}c�*���tX��Ү�?�xR�+�2
z[�\,��>;5��+����]�p�>�9&��3�>!�M}.&�a�g�C��:m2cc��2�1�hm/���{�FS�_y��M����\���)\�t���A�ڋQ����"v(i�4xc�F�7Z|uZ7"&'i��t��\�,��_�6�|�BظnYշe!l\w��#h7j�&w���'�q՘s��:�\\����s�0s��?�Q���_�����]E��K�W�=��^MW7Ҏ��&�*�!d�V��BX�~���r!4Y+B���Aa���A3�z-�����%���,��@l�vǪYFb��(Ã�@8P���(Xf�
����R�#X�w�bɫE�LO���2rS��i��y���\��Z���Zd-�g��r���a���2�4`��U�h��V��`���1ҡ���=��AV �#	U�_?u}���\���.^�� ��ݺ�Y*ĄW�s�˅�
v&��d̫\.�e��e�6Y{.۵�:�3�k�u�߸�M�������낧�~X/҅�hGT�	C-�Y��Z�u�15?��)1=�Ӭ.�#��2ۻt���ڀَ���K����t�������I��#��"2Ðv��W��Yv�g|.o<����ŝg|*�~�~+��ۛG���X<�{�̓�&#G��χ
����Ņƪ4֬4��^=��m���i+���e���{s*x���o��K������������O���|�Х��jb�Ui,�ݒ~����z��A��eפ�iY4�Ȃ����)��,u7���4#Ř��Y6+�u�Y6�}u�Y6�u3Y���e&ˌ��-Gٓ��r��b9ʞ��-Gٓn`r�-��e� &�8Ŷ&�fx���J=��1�S��>�>9H���o�RS��ɥ���rY5Zq��=4�8_�q)M8҄��,|�GL����J;¸��H; ��#6D)C�G��=bCj�=bÑ6lEl(��eF�� ��� �?Yo�"�T��f�$[�f���/��8�H2C!%�ڹM겄j�9��5�Z�c�T��%`i5����'�	��f��V���G�oM�v�4]H�:M�_Z%Ry�j�*�Zy�.s5��&XͲ�ך`5�=Wk��d�X����5b5����Yn[#T����RB5;P�5B�ñ�vb� d����4�Y�ry���zZ�f�ƕ���o.�&���,�O�s;_���s���0<n��kV��%�vqɡ!�W$#g�16����"އ�O�6 �<�8�m@2<�xvՄ_� 9B��Ϛթ#���,`Ln��B���c��cxG����̖@2z�"�4�Yy�]����$�$3?���t(�ł̅9.F ��ł�1.�n��� a����?�ҽ{Y9�-��M	mь%_���,�G6�%q�O�i�B+n��i�h[LXΗ�],&,�-&,bBd��CO"�H�?(�_����l9�0�9��=��'���]c���=�]�D�C2&��������W��ě���=�F��J`�F~8�^%���[!������^��c��^��r6_c�f+:��6�%�V����R�P�<��a�<@3���!�J3^b�����R+d2��c������}K�폥��wۻʥ1�Os.�m/����*~�m�c)�����RN����J�1;�6�N�kϷc��|��qg�ݦ{4�R�-����7��2;�o�y��I?o�����3�����L5z�����CG�A��aC��K+�_�n�~����x��w۷r᤾��{��:t�'��*���ѥ;U��Mb�Ѳ]�L;ͪ..�Jb�^3GHh��h�#�.vs�S�f��d�����Ӵ}��D��b��0d�Q2ȨQ�h���s�ڣ�*�#SJ���hfE��}�Ѱ�;-�T�W3�Eg^�O:Gͅә}�}���M�1�����?@��t��h�H:n!�.�vz��r�t����3����i������+0�wqL��ITҝ�w�>Dug�ݯ� ^WLP{n�WLR{n1v�R>j��Y�=��b��W������^o�$�V��pq�i+D��쥶
as��G�l?cx_hp�p�}��`f������k��U�a�ܿ__���-����Y[�,f��VS�N퐆���f��]o`g���۴3?�,#]��Zf@>;�d�&@��fr-������v$��d��ɋ��{�Fɬ1 e� ��V2�9�o�m%�S���V�ɝ�71�<\? ��K��]�PH�\{94���"�K�B��39�9��3�X���<
�hI��2bAև�U�|��\������I@��<`f�uQ6�N.l�2�X\�Jb;��ͅM�0�kb��p]��
%�u��@y)�Ų��th �Խo�.s)K\<�2�+�5���p`�α�8�����}���&�q�<�d���^�|E21pTm�"��O
�Y2g�1+M�(F��bVaF�U��; ��	����9��2�aD��3&=cHN���� ����p~�����L^�q9jOz�|2��l wj�L�2ƬL�3Ⴓ�fn�����}3�N1֔,:�E�d�R���?�Sb����b�%1�����\TVM'�y���ۤ1a�z�3[k��7=isYZ�^�ϕR3 7Cbm��N�.����~��"���f>ڕ��лQ�2F�>u��-�ݒb��J�н���і��b�l��������=�JP6P��4�gk���]�@UF.:�ˡ(c�q���q�2LSQb�(����13��f*��<P杪�A2��v�+#�Ii,cߡ;M;9����H�N�1eXɢ���/�)5�p�ż)M],Y����{z��QJS<ݩP�Ht�1C�1<��cF��)0f�q
��L�12��0f��02�v�Z�;�oz����dވ`VY��*����b�*{�Nh��K5�ˋ��@�~�l(��qh�,<��*�^��g��G�1���3�Y��M���O��t��SG���D7D�]�#Uz`�|v �N/���|d�s��t���Y~ӝ�g�8מ�2���-��+���fZ��E�9��&� �� i�&x"(�$��3&�)���~S��0R�A���aF.�sJ���    ���iV�����F�MI�2Ơ���>>��sѽ~��8�r�_{4@j��lKp�_{4I����#G����AۭF�wIs���#��b�x0��=is��LF@0
{��R���Ų�ܜ=N�b�^�
��s#�L�<<�	xг�rF%�#	����
�灓�Q�������|�.��c�u,9�!���O9F��O9F���Q�Q+ҳ�%=MN�{+@�7�Y
�����脿{}�N��[�&C�-]J����n���섿{K�5�|�{��N�{��Rw��[��z���f��������T�l��v��Q.�Y�<�b�������7ݴ������.u�R��:H��jh�x�����t��{�����f��S�����lz�ȣ��Mo��#g�M��v�ЛxA�)��U�ht���
���>�������Eog�����[�g�6ĖJ%�s�߫Ty�&�8h��|3���Yoj:�b��{�����*ޫe��������= N�om��<�t������8�o��$&��� `:p�� �x/���~3 ����7����~3 {:�o攞ub���hF^��yI�:�4k�U�9oT��t���H�!$�Jt��w."fq[�>��7_�;N�L��p���U^�ґ*_z>�h�YfI_���ʥ�q_s�s�ߌ��M����q_�1���f�Wq�9�����q_��v�}���1�+u�$�:�9e���/Y_d��^K�D����h{�x�;eyz��S�v���=K�c��z�x�]�完�}�N �Q^2M��(/b�=墼=3	��9h��0��6��-�����/�|C�F:Gb��5DHo�5���j�ယw{��3&��԰oZ�4�u&4�n� ���	�8]]ŅN���N �*%Bp�T)�~$��*�Gr{��#ٔ���|��� :�1 �F>V�]�Oyn��k$a����'����Ҟ�_3�6�7[m@�Vx�E�ϖ��M��l�������0���0�E��2/1,60�;p��h��edY�)�s�#CW	�-�(��eh	Ĭ�:p����tq��w�d��%!I*]��ӛ
i=�`g(n6;�� �Pl6��?��}ϑ�����-�\�7�頚��\oew&��؃����%\�J ��c\�|1��7�JX�� �M��h�9=؃Д��|�,�=B�m	ǲ�N&Y҃�!��lK�Y����2�8�O�Cq�)q(Q1%yL�k��c�F�0ۄ2�<#�6x���d��r���=�{J���W|�@���y�ƙ��WuK��&�xg��&�g��cO�T�O�$;{I�ѱ�y�eg�4!co���n�3]��IW�5���lhE8:�5b���~� 0�X��q�;���~>sj54b��j>6<�3lx:g�6<�f~œ���b+}�[��R�2|A,�tݣD;����jH�z���>�B,�TŲ˟�N��4ӗ�8�:s�w"�tH[P/�����\�j��PC2/Tëw&�f��4��2A��ϥ>gNgb��P���1�,���.�� Z�Q
5Ve��j�U��X��,�X%����0ͷ@bU���!�|j�8#|�~�?���b`��Y�5V��V@4ZE
�7))�^�5))��@<��>�L��苺k�����N�~Nt�D���O'N_":p"��s�'�@�A�?�W���:a�J���.�p����ew)���F�Ǒ<:{I1r�q&���ؐL��y�d�<M�!� ڐ=���!{`�C���#@�yl����a��IW ��� l��+ �LW�rg 6��ɪ"�&�v9�	P���V��T�"*5_�I;���tG'v��#(�F����f"�#��b�Ϡޤ����b��\��BU�~�����دm\�s2`{1a�a�_\���Ɣ �i�|{��	ؔ�6�A+ڷ�� ���䣷�ـ�|�y�؄L~>hKv>`Bt�{����7&�1��Q�*e��b��a1�?�2L�k/�I������{Y�2�u�y����xz�X <��Y�{"+�lJC酈�E/S�i���7CG�7BG%%�ᧄ��5%9�-�+5q���t���.����K˓�^���)�����qѧ8+�)	������$��	3�v��a�`)N	|��D��ojFnv��rQ�/{ܐ.�l�Z�(NlBq~��'�7{�]cvv����&a���1�����*ӌZ��LJ��:3E�Y�Y�J����UG~����_{��a��r}�����<ۮ�#����O�
Ḏ�X��:>o.'��������h�2�H�*�|c8�4'�)I5�k�	�8��o�|q2���^O3�����d�.Js�ҷ��<K��L*��pɞ�����-:Ө�'��'΅O��\��gT\<��ŧ'~r��ն��V>ŗ�����N��o���·��Lp����j_�8���qg�<�.�Iq>��ʐ��ٌ��+��Ts�V:���$��4�m����\VNd.x82Ǫ\���j\�VF��\���p��7�+�� ����z�H揳,K�q� C̀2R��w���j�ܮt�͑w��of����.�}Y�t�M��E�_dr�Ġb����ob	�H-]`�/( �P3�y��� #� #j�n��<����$�l�\c" �(e���3��� N��O-9p����g ;p�:��\��e:�dЌh�k�4#�2��e�\�
��{�P���@6���)������SwЙ�N�:5�i����M�r ��O)�3z�1�rP�W�)9������μ?���FPt?���NP���ـ/2����l����ݾ8!pNQ��%�a.ƙ/μ�se��4�n�a�X���Ґ��5Y�8%��������05�#�3�]���|���M3M���%�/��i���T�i�)f�f�4S'�^ �������>����S�8m$/�h�F]�s�F�h���bg<i�u���NW}xHk�=B2��_%G�S�N�᠇1�6��I��6ud9���r�i��4��cL	>�wsάT��<5���-�g>��ǘ�z!�vhc�k~^���H�p�N���5%�6]�Z�cƜW����b�U�R�>��q�ȓȇ�SC��8pp��0��� �w*�7�n��ʂ�m%��JE�b�ń� ��w�0?XQ��&���ӄ�(L�ӄ@H{�4��+���/\�������x]c����8���S����nK�3��F0��h��M����,i�>�Cl*�z��;Hg��-|��$����[�L�8�ozKv���Mwi�N���	��]nKNq����vO����ItdI���R�8G�9r���~��Ŵ`ͥ�J-&�a�T���̕Uj1�
3!��^3=T�b>�2��a��R���-�T1��c���T-�TK�15��VY�+�u���t��R�_>L ��� s{Zĵ2�-����4��%�q�� ^��#+��=�;x���!l,U��ȴQ�_�R
�6hE��ϭ�R���.�FҌ��Rb��1y-J��#]�3� �ڙ@�(&?���>Y;ZO���c���i����v��g)��_�LU��/���nC��AY��u1߁1r�.f}`��K�T�د_�T����|Çc��:�7�|�����f��'�[��n�׽�Q*�1<C��l�
7I��c��C/n<U���,��S���^�`=�x� K*��T-��Ч s�qb�Rq�$����Ų}��P�ao��i����Τ$)u��K��$��͆�ROA�MtQ*���8	𽓅",�� EX��*�|o��"l���{�f��ީ;uj�����a��䔤�)�dh���x�	W����Ba-�)�[X*
2b*��H�շ�������Z��j��[��rۭ}���ٙE{��f��=rv�ݥ=rv�u�=rvV���9;�|��=rtV� v�a	C�Ggy場�Gg5�+����~ ԥ<:��}��V���{*�99�J+��-    �H��y�ŤTE�e3Cԛ!�8+�Y�q�t2�8pV:�_++��K�.����Y���l����'2K��R��N��ӥ����p��UΖĂ10ΖĂግU��,��qL�ƔT�7x]̋�دB�8]�k\���R����1c���`UZ�5���+��={���u�goǭ]P׶.tAƄ�:�+Y7n�oFJ�H]��,f���"D��U�E�օ���6���{�vX��d�uF���I�=Z6eܶ��pO���Z������5a��oL�&�y;�Em�=+�m�M�M�Na>ܹ�)̎����I�y�)�][L8�`uS�nF
x����,8�)�Y�hS�J6>��1�8����o����J,O��"Y�Qыd%�q\��M�d]+m�=�A[�k���-޵�2�݌��-�\ζ�ًR�!�����]|�=6� �8&sg�d���a�qZ���N��P�@3�����/<i;󌜎;��I� =^͒)��=� 3��m{K�ܶG�
u�ܶ�4��m{MT�nFRRH��E�B��s��y�?Bǒc�G�X,�w(�5O�w�s�����%�*K���XC$�SyR����A���������u9V��˩ҸX�˱R~2�t=VJ��ҁd��cH½_]���g(N�+��JڋL��^e�I�Qz�����(��(HF��U�����F��}�2É�[h����IbQ���3�%���إ˼��޲���Jo�1��5��+�����*<������6	.�m|q��D���&*����0���းp� �t���so��4蓑��xo��Y\gj9?a]��y�b�1���[z�Ӽ�6}Ȏi�˝&i΍)iCP1�p�7�#��#�"C�t�B�4��.�y��s �COw��tɼ�ǊS _�.�� �4�d�����3[�Ns:��#��>��0��/Q���YOQ�}��S���a�S�y5����A��!�#������������ђ�D�&3Ւ���t���`��$��0��SF�! -t���]�C���B*y�s�[6�MĲ�oC6���=}3�6��蓑��1��=}c�4)��X�Y��3��N�a�׳�� g��� �ɮ��n'ΰo�56��"�a�bV�#ˤx�ߨ+�t��Qq�Gr��Ⱦ�� ������Y����4D����_l)�ᾞV��-�1x�_`�D4��/����z*)H��#p�cĠ.bD(C�dĈC�3�R@5 F��0I�PƁ4_+Vi9�Aj�Œǭ��}�/)��e�*�f[�_	���O�Q�K��2P��ϊ��d5�:p@�5��J�46ƾĕ*��F�;��kr�B���SP�~�/��(j�x�C�7�ilt���7t5�j��Re Լ��� �Yi����{_ �8A�^���L���=[�
8����F�W���/��fT\��:�hLa�E�2�P�h�5�����o
�2���7�Gq:��n��L���E�W�`O:!�~5"2�B�n� ��z���x�Vn��=^��2�	C���6¼~��O�`�W�WEe0��_��a~�I;�6���ߎ��f3c�X)��ճY!����[1�o),��=��b�ڊ:]1Emp�8�wC!;&���77fi�A6�(U�x���>@��i؊^6�A�jE��ӥb��w���jEǳ�ҋ����s'��.�x����[gNM��Y���{��׍��A_7yz�׍�E��j���;�@(�8+��N� �8C�R��
���Q�E�x��=����u6I%��{O�	����ט�i^�Z`�Lvu����u++��k�:��̤��4�C����xcl��tF 3�3g� ��Ό���N����@=8؁�<rS��o�7؁�~cr`mTX���kgZ��X���l��l	r�"�!�mѶyI&ȁ�_�T����^��u�"Fn%ߙ5��w�wyj�׶�aɦ�V�85pz��h��4��m�F����ف�!�ac�O�L��t/��I�땑?�3�1���ݜx�o���3�'ݱ���$_�x�o!ȁ_�Ƴ\��N-N�_��ʊ�TE%��_��@�A�b��H9��5�XcdLp3��#YpvLA��y���2�|�2�,�Ac�� ��N��4f �gΛL�s��gxN���0�\t���Eqv��s���~�%�	�ӣU\�H����%81��y`�}���5�y�����JG�4T#%�P@��ـi��)���)sK�asS��ڣ��v�$��6$��*(r����;�`	���� �V���R k��k�����o�:�曮�3$]�`H�����ҁ��F��,J5˴�2*�#������!~ �zd��B��j� -X��~�?8\ʞ�8�	��=a��=!�!cG��Bʖ7H�w�ˋ�u<ӥ��h��L�]��']�`��)'���i�
☞Rb���rB�)�� 1�[�:ۧ����t8����x��Q���m?
rআ��p8��P�N���r�b�*�$��ٷ���<,�����?���_U���[���[���[���[���[5L��5-�(l��ѶSύ��,��j�������]��^���b;p����r�ӑg�=]Ō���l	v��D��o:��attyz<ttz�W;��QS��.6Z���L��]k�x���r�n����d	j���N/Aܴ Q5,�?��Z�����Y��-�6��i�7z�pv���o���'�_�<�_��t�_Y��C�g��j���g	f���}b���U��A��V���^��^E�b%�8�$c' b�I0��ᇇ�/��OO�ڹ��N�`�C���e��C��N�~�`cDt���,��� �K�L<|��,��!X�%. K�:Pf}4j-qj�OJ�u*4��������X�do������t��,˭����i?��I��e.�\Ԉ3�	H���h	Z��2V1���Z+����ZA\�Z+x��km�h�k���2��?x����(�|�b�H>��]�l�v��ߛ�_KB�4�i��̰�Rc���# �Hx�X [�����U_�Ӎ���;�1����C1<=���xq�-�7l<S�CK&�4ߡw�LVA\��α�nL�y%a�̏]gh����������f����0�@3���L~��ă��5a�\�D�q}Sq��&
��>=�9�������&����J��"��̫o�`���e�X��<W
���8�:�yuƖ?x�_���9C��1�0�J����x�B���Ϙ�ͼ����������Y��AH�K?�d��dى�׫�?3R���X�>�n@��cz?@��O��/�G�������VL���V�P�/�b�R��4f�F��@Ͳ�������H��}+f��� 1�d[.���a�O�v0�i~vb�e�f�?���׫�z�#��W<=��	���u
S�;��`7v��� �z��U'b%���4dL'-l-� �j��6���X0U���$Jcɻ�nd�H�\��h��I����lB{A�����k��s7▩��qf�e�T"W;1���!�^�f���^�n�#���G�#ĖN&Ftj��j�K� 8�X�k���n�ӰC�`�W]?��E���h_yh���%i'K�ko��n����WƱ�.��p-7�L�L��zA���j̢�̅+}.������dʚo���UC���-:Q��&+1��yU˃��]U��U�n*n\�V&��~}X���b�	|�������W:s�\�V���g]�lg���ʥ�i���sݐ�z�NuR�eRD�k���_YhpR��B�5���������"�\��8�=�0�׍+�K!��
'��+�֜8ײ���KLZ�K��l��y�i 5ɞ|�<�x�>͌_ܩl^w2`��%(��v~+��KP�
�������r݉�;�����d$��4�9;?p���s���a��o&��бC��B��uwܪ7��a�%�C3׈�V��    �k13U��� j�1(D;#`��h�IT�F ���8]��mv����e��hĀ1�h��ړ��o�b ��P����ӊb#z37Yanr���߆��R�K ������!~^��K#�0����w���br�ԙ��ԩ��r�F�
5�:B���K����XB����������q( �n{�^��H�B�ӎ��g�*�<������ѫ��C �\�Ǳ>���>cK��L2�.r��d�Γ^��nW���ah���YD}
����_��n�Fk6)���T�8�{�3�YOdtuf�<��/Ug�C�v�F�s�!ms��CZ���SIK����4��qK8N5[�q�CM��4�inrEu�>-+�h��l�M#W�B����U}�t&��B�sZi�T�r�&�{�>�a8D�����Q�.m
���Ҧ�6���.m
���Y�~ҩ�A�viS��.��c0��N��t�	{�'���A/��*FY��qR}�����Y�ώ����F�����4�GA�Ґ�~���3�G����Q�l5�GARǔK��8���VSzt�Q�A-+�5'��ȋ�n�o���O�lJm��K:h_|Im��K���"�.����D�LbjVǞ��m��,v}�U�>��������?[�8�a��t=4��7�b�����P�?	3E�ܖ&�*FْLer[�KK&�-+�>,-oY�T&m{2�IÐbҶW2M�^�?.X#DV�	��i�"3��p�������;üp9C��jD���L�=���ׂf�7�Q-8�ֆ�FU��S���Vz��Y�'����Ղ�l i����agb~��9���j�-�����sa�l������ӳߡ`=.�Ѓ����$�<p~�Th����R3	�J�,S�uK�d*j��%�<%}"m{��$S{��iº�����TP@��$H-�?{ujA0۲�r8~zw�Ԃ�'�U����+��uzM����Q��(���,i�i�3-��N���A�E���J���9���H�)�y���3�Ҡ�H�KX���L��7�.��t03?$�)�����MG3��m�R3�m�!*!�Ю�h׮��rAT1��S�q�N�;z1=E|;���T ��1��;�����3���#}̘�}���|��N��~M�`f|���RHS�pƫ}'�@f(�~-������Kx[Qg	v�k9���b~��b���b��"��ÆԿ&�,~����a:��_7�'v������[�!���䲱�9[����Gߪ���s�^����d��A�=�擊ۋn>����擮���,��C�� V��I��ġ�$#ӱ΄���Q��y��O���t~x����&f��37��y��gƔ��o�N���_��N��?O�:Kp����i����!�	N�W��S�N�ل&t���d�<LI2H)�lqeL�U����ehɤ:��k7O�v�k���n2�dR���5��l��L���-lS�*Cך�H�\�7�[������;'
����6u2���{u����v���g�t�j�١Y�)8��35I�2�V��Ԃ�Sg��򰘐 ĩ�3KTڪM����K:�f���U3ƆN�?t�jC�t����U��8w���-�5N|�<���g��,@9YpF9�y���?I�P�.8������@��Q��D��щ�28ݱ����H�C���N7�����.8�P"|���NH�uz:���X�]��?�J����Q��"�S�*f$�E�Z�d�NNE���������.I�b�U��V��{�����|�w�| � ��� H����7 �Ip�� , :ap�q?�~{��gR����PV#���$*��c�ld#����/F�^9q��׺%M�^�P�&ah� ���w;�ƾ�4�[�tj}ؠ݀��<4Q�k%�9=-�Ӝ�YWP#��?o�i��;Upz���п�S�4�T�:��n;Qpf��O������4K�G"P��x�� ��R��L)�-�-��YW�Pj�-A�������ޫ���jTtY�^��0��T,S�m|�����������1���Wŕ�w�c���w�xz�{�����첮PFz�������xF�dm�Y�첮�1�td|W{~��Q�r��Z�(�~T8Ҫ��G��Q|�xsɳ�v��̓a��]蒟��~w���	��{ 10��w.^�����W� �ǆ�|m$�ǆ@2lxJ3����$�)R��$��v�!m�4�f4���Q��S�y�S��l�q�2�v���y�%׆���(�zNI�y!�:�|���èF��"�&%HΒ�%�t�){�%�1'��E�Ε�de��9�s�Ӈ�,��yZ̢�(�Yt0������l��2�?uF�i��g�m1}���Ն�̻���-���c�4��j˖~�;���׽�Qۖ=�B�ݲ�WCmb�EOt�c�- T4�e?/�ڲ��AL��2��b(����׏��q�աY6�'��5�}�Akpb�P�-����lӿ��͌����8=YŅ��)��(�J��Γ�~ �}�h�So�<�i`-����L^���+yx�N%WQvyw�׮��_���o~�%s�i�w(,T��SV{�M�M�NLԋ��KaT;
1L�Q��cB �?&<�kb�&< �k����:��ńu��s���ځa�Y/#�[&�䢯N�J�69����Vy�ݛ8)������E���X�ګX��4�yHś�L�kB��U+}���^�p���u���"�{M�}��`�i��kMB����֭�u�j�JQ�p���>���	���բ�pI[����ɡ�ܵu�k���]{�]{d�B��/N�T'�[�:��%Ǥ۩�|�3N��,y�qf���댓�?�3�$����"O2�"8��H��|�Y��R��ў��;ڲ6�"pG[��xz�-{yz�-{Y5T�x�^2��-����zzѦ��.��ͅ�����o�85p����/�������ߛN�=yS�R�*nԦ$�-�l�ՙ�3����d2R��$ȼ�\�ק�j�<mr�^�r��C>,e�cO����׻$��X��Ȓ���������&���N�^���������.V��<����Ԑ�g2R���"��-�A�Ч%�N������B`�n&�Fw���֝#I�*.tV��a��ߧ���M�pƾƼ=�>���IV�s��nzZ�($+a+��ߠ�Y�\��j=��<�U2�������UV��IuT1_��0�,b%\��c�C5�ҹ���R+,5�C5�.f���f��ҡ�I+a� 0����)a�� |�s�k�:�r]�J��dt�)y�;��dw��r�~��r��m
���z}Z.׫xыt�MW�D�r�f�g�t��L| �|�� ���H{�2�� ~I4pzJ��R��+���1$���$:2MaH@+xH�v�1�v��hJU�ܮ=O�]�OO����_�D�1|z�ъ]6(�Y
�L3��aW�l��B�3��>L��јs/a{"��1��Y���h����0�\V�:�0�rKԡ�I����IE���h�X��z;NE����>NEF�w��|��H�Aɍ�0i>(�&�rJ�
#�<I
����(�hq�3��*f����O�t`��$��������A3���u>�+��ċL%��\B�d��,��l������w1s�k����̥/1��p��DY�}���փ�q8��kHYF�`I����
��'.0��y:O]f~ś�ή�Y�א���e>��J��"�KZ̘(�|[:fe���gemT��?H��:++�5�z�,c����}��I��d:����:�~k��C&��r�b�I�^��lR�g�Nt�|��D��7Y�&�{f�٥r/�t�� ]*�LV�K�܃��Y�g���l=�������D��փ�������2� h^�Su��ivL%�7�J&�'P�bg�;z����-��&�̛ΠN`�o�o��� 	O��Ɨ��dw��S���L2eK�b���|    {N��Ӄ˷�'�oD6�d>�b�x؈R���S�y�b���{�5�6�sqK�%�������;��|���g�'��T=B&����A��_������1A����c�������o�S�	���g3?��b���6�%�ཅz�Y&�Tq��iy�@�c��AI.�ܿ��
��>0y�zz��c�BQK��sZ��Y1|=���_b�E��L���&�oS��B�trr��&^C$���/d�[����>���2u�,=�r�v�%u��z~����+=1��C�(H;������~�ס��&�C��M^��w�6����d�d�W\���VV%#�Q�?�2���E6p๴c��=f����9�x]�s,��0C�2J�T'/Z�8�p�!t���P�����!e���EN��q=�f'B�Q�Ko�I��2j[�!��,��u��3��8�'�x�����#4^�Y�~����/�4f�+J9z�!�.�@��mH1�Kq�1���e�J� wS�a���@������8p�����zL15�6�����x�ǀs�	�0����	���H�g ��K�3 �$�W<]���h�w�A��ұ,8	����,�?�u$�'���(�C��L'����aD��z���a�@��|_���=��,^a;'�n1I�䢪�����9��3-�������2'c�s�qe0G
�1�j�x��:�9���k0�1H8Ú[�O.�6I�W�x���j�D�ס�.�O�a�ÊT�� ;�H����ߪ�I-�~�K�.��~�(�����c�;�Q�����Ԙ�6�e���qn̲V��p��8p�9ƈIZc����G��3�-��� �h��8S���+�����n�9=4jF��T�� w2mE�s���!�$�]�����8�!:ͽ��������5C<3Q��(��`�:4�"��^in�Ή�1��6U�N���-&^
ͽ���8ͽrAt�4��ѩӕ!�D���t�u��r�\���.}�&H���3E{Ř���M�Sg��,��
8<N�78pf��p©�3�tt�������L���P��Ó	hR{�5ٹp�\��GM�5���_�S_4�ݏN|�ĺ�1�����Fm�y�3/��r�m�1�T։����ӆ [��&���L��2b��n@.��N/�.;���\�Y�Sip��� ,����3��X�B��ir��:����Qܱ�&���0j�$t<��y�;���j��i4�����ҕ�����Y k2�ώ\��
L�1��m�d����aj<�H#�1vJN<͎�U�a,�'�G� ���So1?��&3!��Ō~0��x1�W.�@Ɏ��iL���̘�5�1 M�n��ó� �ch�wX�)��`Rӎg�	��t����1&�^.���y��[��Lbs�V��p�\q�A���vN�!�0vT� <����� <^�$�2;�(���u
�񢿀�혢�ڱoL�A�s��tLRJ���;&y_��n�9&�ҝ�V8Z~^
��狅��>���2ߤ	���~����i���9.�C5CHun&���"�xǃ,O�n��ǋ�h��9��x��;��x��ĝ x�H��%�:� �Q�X�I�� ��9�x�0��Ær���������>� �Q8og��w���:�o��p������[�v��k�vs�S�^ߏ�W�����r�輿�6w����E6����9�����n��9��F��c}������a�q'��2Z�nx�Q�ba�s���&X��ҋ�M+m�H���	��0�uWd9���	yx��ˤ/��-3�9�p�L�'��wTM����w�L�F��GՄ�����p�8g,�o1�kbs'�͠����Ɛ�ĖҙG}�������|��� eJ��/��̼̋�T_��dM���gn���� g�=��x;���a�W����5���n����j�i5`�[�-��e@1�@}�������1Yn���!<{	�K���`���0ʁ1w�특]B�j�-�\�H���E�b�9.���b�r���/'���˧��E��7R0L*�C�������2�\H|F���-�-B>M����_h�<β��i�lQ���:��Yx���Ū��)�؉��$� �(���ڌ���%Q �dm�p����,#K4�8\��`Y1�$��:�:XF���LB��A`�\�
�v��]���!.���t4`�ٌw�ia��]��l�1WH�&k|�#�(�f�EK���׽rݷ9��E�'���u|H}G�<��ӱ+�h��:c���D��R���e֌�m�Lj�@�X���mqѩP��6�\`�\1��$�(��m@�$���Y�P����Uab��� \�@&G���`�r��A���fnf�mO�p�ڜ��,7'������椿���n�9�o,c)��=I{��G}Q��Ӝ��n��̲N֩bx���[�i����X��t�>�'~�I���}e{�H�]����fi,�5�FmF���ӝ%��?�U����������+|�-n����ͩsC]���l��1Hg6=�Y+�̦'�Q��D��X{ ldӉ��R�/�\��l}]�,)�����rk�10��ݴ�2w�Cu-*6	�C#zP�VjD;q�څ6=�k�mg���I�(<��9v��24���s�� ��̿��̟�!6�$f��cԈ���=8�:�dj�sD�t��AD��+�^�����Iv)*�t����� ����,ÝJ&d�,��
~pЅ{������Q���Z\������4S�F�1z���["%#�RS2k��S2k�]��3y^���L����3y^��D&�_���y�{QX&�߂��9`�F}�A3��A�?p�����sc�҃�<10g����փ���G	=˱_�����B���5h��XO��֒
sܒ[k"��J�X䰌�QӒmR0����R�8�i�?_�h���=�~c3��v����4˦ a N�&i;<=�=��u �����ʞ%5|�1���k�Nv-������߻����|q7�r��3��o�ގ;�W���׬'2t�)���c�;������kSŗq((m��R�A�kS�dh��,ۖDx��kS6(�6r����O*�::������K�Q6-gx���ޜ�

�9G}�*G����	Cys�Ryǧ=Xm���M������߾��?}˱hA����Z�Se��ĳ�ȯ�jt=D3�����9q�Z�����9��V����s��Ov(��\��� i��-����c�ެ���8��Fp�D3�������i*�M�i�5�������������z�e��qh�|����ڢ�E'r5���^H�]�Կi`��9�A@� `��� ���ni�6�O'�H��	Ro@7��_�;��tI���E�6w�E��'�xSh`���G�Zo��	��5�8}]���M�|�-,-�,�F��{���,Mݍ�Ƶ��u�;c��{��3ㆮ��3��O�̽�t����n��C��+ƿ�֔}Ÿe?�s9�\s'�#�x��dԶ�����q�EC�ڢ��+���2S�����eۢ���E����ix��8�3�W�(�)�� �ҙ\?�`K�ۂ��bh��SB{КGmȚЈ<��l��p0^�"$���[r��diNז͍�������ۋ'g8>�����քW�)��.���@h3U�OhP~~kJ��	:��62���Iz����~���&@��_�����~�=�y�|ٹ����Nc|���up���ᵮ���������4��n_�8�����o�y_�CA��ZJ0�r�(�������jJR�-$jJ�����!��E��2MC�fJ�Y���������Y|�u����Xb��e^��z��yI����/�� G�Awc�6����K��3�9�5ޠ����	�]���M����T=cK�삹��l��#�w4�ρ�Mm��.�ƪg� �5V=K#�    ����^cճ��;X�l��^@���~L���#�� I/�̽�^�RX�ϑ}ܨ梇#�T됢�^��wH�s����1���^�=G�c�9��{Y�I֠�:ϩ������^!��O9s]��3��	^g���8s�{�י�`~�u�at��/ 3W���:��8�'4��ɪg�V=u"E�FX�N�z��lS���vG�m�M�ꖛ�xf�oI%�U��]R�lt�%E�V��wI��
��K����ᒢg�}ݻ�f���Ělv4��ۈv�MpjU E�`�>����%�o�j�۲{�[���\�E�[Ɬ���1�m����>����S��hJ��G��Vy��H��g�~��\V��k.��o_�� �Â��?_<���3�Q<�D��t�?�2��I��{�/	���Z��r�lz��� ��a�7E9Vl���߼d۟h�e�t�� ]Ӵ�w�?�\���xGk%M��`.�yC�
Pc�2���Ͷ����T�Ø���X���������Mݍ�s~�F�	���W6�ϼ_�P�-gs��khh��kh)����	����_�$�b��t���H8�G��Lj+R4]u��s�"�3����xg�(+����(��D�*��F�(�r~b3�����,�7����e��)[e�V`����J�A|�9_[�=R� _;������;�6�{(5ς�lOf�Ǡ�w�p�y��Ƥ�w�c,�t�ٍM�v��ʴ���x!䔭f:-�;�������<h���i��P� ��~�c�����t���;��|ʾ�[�����5��/m_�`v�~��O��w:�4��t���pNF
i.:r/���A��e� .�6in�
��zi�����ᡔF�ͣ�JN9���]���/h�O�Kժ� >�.<
J�S�"����$,�BT���(�*��J��QG�]������[��>�-����~���(�g+�C���E��'U��!����	3�#�[� �x���
)H�OmJ�V6V�,D�f㸥-���E���=���fϼc�A3<��h޷��H4�5�Ξ^r�a���Gz����!�sg�����ԁ��FN�:�G!�SI��jU��5�6����3�-#=gIƮ2��q~�>tO�aW$8b�
���x�W�%6^�ZF>J��J]v��;��Y�^%�V��ek��ΐ�'�H��<�PFh~�L� e�a
{�\�Ccp�շ:�"�.A�Q� e�GǬ�?���<���\�U�Nu0�0Et��%�� Q��1�h�j�O9���ض!���wy�)��Z� =d���jyV໼A��a���������I�qyk�Ã���V38�s��^%�)�k�.M�!+=�B�9[�2;X�d/���e���j��aMC8`F(Es.͐�8���� 
V�i��[�'yM\ьs��f>��G݂ffay43��M�L�Hw(�$����{-�0��c�dv:��~.X�oH�Ϝ�2�D�X� QUZ�'TU,�f~U�F/�js�/��W��J��*�Z���3�u��Z���A<��#^��y�+pE}���Ի=8�K�I�k�����[<p�g��OZ����5i;�yR�{ҔfIn/���䶸`=�Ѓ�L�@����W� _d���a9Z��yՊ��%�U���%����5�L=[%�L�m�WD������,�Mv��K�]�H	��%/p�?h��̩à.�.&�_�X4~|;�y��??��+$f$Je����1df��:����O�H��T�#.��sN�ҭ`L%��~y��AFQ�8�f��Ր����T���K�
��˦\�)�>hGF��^�b�E+�>o����V�6���]����\�*n7p���Ϗ���"�2�F�8� j�np�߮�
V�aԟѸWAh*���S�C6����c�n�*����
a��1�j�Cp��a�f�j���͋���[������_{q&G�ڇ3i8h�>Jʗv� ����^�&>
+%p���THP�5�|hE����
�������1$8�QO�h�6>m\>v���N�0#�Z���@0 b���|̠b#̨��X���٘+	4Y�=�<E	��T�$x�ZXפ�o͋&.> ����U��0�#��a~��;M_$�%a�}����v�WA32��#�wVV��_�ػ�J���`�㫎�h?w�/�̓������鎄��4�d��!:�2w�m���߃�^�"�`�W��=��7}Q���h����^{��X�𽩊�+C3/�����+L��/��2m�M.��2���[��偎<LՅ�e?����6�qO[,	|��lT��qS��;ߺ6n������^�;aR�,��N��1v� �;a*�w���o߫bR�L�u��y��������\�DJ��-� �tz���V�ׁ䨲�����` 9�sW�Uq͓��������u�כ�c�T�����X���L��T�_C"�)nV�p�M6��N���������+����ʨtC'��$���A
|]��ׂ��rթ'�ue�+��|���|�bdr'S�e�w�Z+�U�3g��u-^��X�BwOL�r��T��Ũ�4��+:Wf�
�A� ������[�����)Q�r�#Q���B����҉`�.�,�F���*��ʗ�8���`G���v.!e2_��\�R��QlS��������\^�����Tw������\����M�9pE�R�`.�~��f�\0��f�鋗|
\��k����V˥�X�(%pɒ��	|-@�g\�fpu���^K�\�V�
;"�-t:�N�%���1;�i�s⸧���כa'�ۓz�^���;�=:�c��3N0Ͱ*l�S+����`� B�1�a��W�����)��Ա �Z·�g�����7���]V�>[>�����hPg#����Vg��#�� z�F�f�8����1���y6����`A
\^lu�����Vki����i��
\^_g�х��q�n@3b�Wb��B�	��V��)�3��3�1Ϳ���	�|p�C����|޹l[-�� ��A\a��p��B��"������,x�
WL�	��dfs��[m&��dH`z�2�(�u��å�-X0�h9��?G��	|�Y���P~���ͽ��+M����!V��t��E���U!9y���f3�8�e������j�� 3�����r�1OqG�7rCCPNj�}KɃ��8q��Znv!�y0�ù�c�)6���~���g��~�DX�M�:�(#RC�'�x�c ]�$�RK� ʔ�ց-h�����蜴��#�k`�&aY���J�	#���@1?��}����V1�� ��7_r���>��c��9O+��B�]p�\��]���1�=8�7%,� �Q��2*dĸ �-(�8��<�y$/�BsH�H^>�y$/�!m!&�|�)�!~������$��%��
��a�b�b�n��M���d:�ʅtL	����
�D��|�/�v_�l���,X���]Qqi��-�Қ�]K�Қ_�t�">-�)�i���ȈH�7�N';�Y�uRZ9��B���?���U!d������2�Uی�ʮ~Q$�D��J&��K_$�s_/����}���I/_/S�R��:x��o��=+�h>��rg8s�vF�i�����$�-'�%s����.��Yɪ�%s1+e��d.f-���B	]��f�W��wޫ �s�tyj��9�;p|�R�7a�LX:���2�:9K�	I���eB��t%�N/��LX:��U�$wᵣZ&����.�ʟ��`�g�]8�qH����!�G�O63uq���B��t�ٖd	�%|���<7�+� G�t*4��.�䞹]���I�YK�r�t^��T��dѐ���s�Y.y�Y�n9�:��C_N��%��L�.n���s�Z���u��![S�:���΅��2/�L���I��B�9k/�����w�x2O=Mn    ��i�L���\d�\��b�[.��� ���a��!/��:��$�ZK���y�����qq	W��¾��srM�B�9y\2`�Q�P��w�o�{HT���rY�e���2���Kzq���l?�w�q�i�!>2`���zd���b
5Ȁ��=�<�2`���.V	6`�z'��������r[�gk�R��_�$'y����2�O�=��w����)JP��K�y�*��?�^E��Cz���-�[~l���
aG��f�N�)���W!�����
�&ǂ�TU�Y��߾9Ca�J�
J���e�YC��!�#6�W���ocB�`�4����Y�^�Vj���8m*`��r
>��)���T蛳c�y�(�͵�,A
l��%,?8���)J�;:�1����;��=�������{����.��W	�ߍ{���\2� .����d����x����g�x譎Lp90�^p9j��D�fF�,ø��=���	�%��.��EG$���\}'?�=1ٜv�2���'r��DZJ�
�'������ǐ8����cѳ%$�MϮ;^�\���T�7:�����G�cK�t�K@��Q7ځ;#]�_��z��I,��io6�h���%�<ɗc�K�|t�׸�mzQ#7���WG�)�K&`y�;�$6����=��l ���l򧋜E� [ݣ8�͙�=���ʩ�=��a��j?[szZ����:��鍚���	�J�*� l������$8f�Xc$wм�YD�f3��HAl�k��3`�k�(��h,��9�A��0H���4��)I�y�Ezp��3�+�;g�w�uNQ�z�"`�z�!6����W����똎�טi�a��k����� ��K�=�|-���[`�B6�s�s>οoO!�v��p�{A��l.��]E[�r��N�Д=�0B��W�'W\���T�it�#\��s�/�c{ b�lZ`ʞbz�O�P�u���@$@2*P�
?�nS��0ߌq���|��M���
�?)Z�DK�_��~�2�鄠��|� ¨0g�&0�_6z�^^�8�9��]Pм�����u���,#WmSU�2rR��kZ�D�d�u�xj�+���Q;M��k�C�μlw��m�펋8,���X�6����0F��8�c���|�r�-�3{�aE��Z���7{��#Fŗd���p��9��n�8�����h�4T��i����i����%���s1�#������E`�[m3,�K�l�㧈̒x|	�ٌ�9%Ҍ��S�)d�ә/��J��A^�0z��w��0=�@PG7����ѹ��9:�ѭ����b�:��^
��M����e7����b�Q-X��������,��{�X��^'�:A����_y����N'ޭ�U�،0^��;m�)�m� ��A�:5���\�gO�>��{B��n�+�!�~�!��ll��x�?ig�Ć,��s�k+���ր`D�_{y63�րa�Y_�}ѳ��o���CS���I���:<����SZC>F����Z�~~�1�%������֚x_/�nM���Uk�}���
�֚lyy H�e���XC6F��qK���WA�5dcXЂF�I��5���z"k@2R���`��!Ҳ0�Áex�4i�@3_EF֐��TC���\�zyO��Rs<f����IK�_g���˱{�I�uz� �?HKY��ւ�=��E��x��
�~V�ט�8�#k&�
ҚY3�TTrŚI���Jik&���
|�._�f�z�Eک׫Z$K��!�Y��84t����؊{�ځ0,gB8:��L��AA�͐Z;F�(�l�y~��`�9�uN6f��t��3a�.�P4�� s�>;�\X�A1�𭕯�2F��b�/�ev��	A�;�!#�h�d�HtJ�^$:e�d���Q ��s�SR�"�)�zcH���7��)���i����׹��9��g�[O97��j�ܗ5Β���D ��D,z���5hp.�\L��<��2O!��i�k��X�\���79���~nr� ��n77���Ĩ���X6gn�����������g~͒�Wn�ExHml��XX�1�Y	2ޗ�`�5��/�a�J��������d��ͭa����淒w��!�Y����l7-��7���Y)4�l��;�ç�rcGt��I���%�\Ń�׺.GўJ�Ek&�,�~��.�n�M�����/y��*9��k��cGr-���cGF��Yɱ�	�*9v.:�lޕr��ԉ���k_��0�Uv�t�������%�w�<�@�d�b��cK��/f���Kf�OPD:+	��z��U����w��x�?�S|��yxQ�Vr�
��1��˙�#��4��w.��(O�ߧ;�{��|�*�����h�o��=���d�B�F�b�9C��J���,��d	�Y�di��qâ�O��WU�>�7�Bٶn6���}��G����_�"F�_��&8~˯!H�lt��W�F���m�^����t`T�dk��O�h^q�k�2h������9���~�r�^	���-����A�{�X>M�G�m^*�r��ȝ�Wi�jAj_��+��Ԍ	r�ط]��A̷m�z��N=ߍGp����^1�9J�Fo�N��;�='H��a�=GH�3g�"Ն$��#��TƇ����-p�T��G��8KN��(:ŋ�#�ԓS[^���z��2�xc/P�w���-�!9bn���\�_��օO_��Y�T [����\t����Lz�y.�'�&
����ѡ����"�+��Q��a�`�\1��Srv���蕋ltݺ�a��[�t X?��PD��U�;��u@B�h`��kT���: ����\`�q_$�f?��O�ΤSp����b�������<��5��a�/�<�;Mq���!�n�[�!b\�ށ+Ֆc{	9�����z��{��&�	��%4�r�Ud0�"���T��H)�v��=���rׯ�K;@�.U9�.f���}Ū�������N&�5^�:�k��^s�e��)uk�j]iѕ S��>QFH�b�/0ʭT?���}E�:@J�ox�}�Ɲ�.�l��q�����u@�������[�DO��h�Ȫ ̽�$��`��{mr� mr�(({�M���0�A�\��_��SN�Y�� �.S�Mh؟�zv�"��Xi�KHA�k��%^��5�h��.A�[����-�z�Ǡ66ϑ�����}�]T����ͥ7#�cT��. :�]����]��l��\gP�"W+ �)+��Q?/K�ʾE%ցW�nō���n�@��[a~�z��Y�[�`�=΋Ɣ���KQ��mWvm��,
�����X�S�X\�7W�S�"�U�u��gs�>�.��;�.��ޝk���@�Z�
��Zn0��c�c��v`�
{gU�?�y���r��Rk��
� Ι�U�uD��q��p�������"[4����=����f���<�c�x`�
��!8��dY�Jΰ��.�X����D��$Z�*|��L��z���܉��ͅ�N
��ï��W�{^}W���<8"�z����4b�����r�t�A�[��dBːa�����h .?�Q̀\~s�J������&N�>�^4�~l�|ꒊ�˝�1T fz�hg"b^����EkA�{��Y�s��^�����i�Y�U�LD�*��I��oE���A#��k�&<Vot?^X0r)��� ]���(������%Xt� n���%�Qb�`��Y���N���g�e'�"ks�J�s����\�Cǃ޻h�'�"7��Ėuc�Xe�ldrz2�X����	x�˃f&�g�Y�e'4���@�
6�dv�˽r{���h�\���%?k�3���e˦��Y ަd��n��d!�F��BȧH��&!O,��ԥ��'�"�"��#�<q�B�6�جh��pH$7���G�y@��g;A�y���h� V��8g	A��-�o8kMS��ǚ�� �y���(a�8��}:�TKak��ϸn�v=�|j���0z��O�    �hA�{p����ƵT��k������o-��ǃOAK�s{�]-7�YO'�%t��Χ����g-�����I�fL
����C8�������E{�ך����f��� {6�z9YJ͂�������Q�}��ͫ�p�:�q���2	������zgs���O3�~�i)�������X��|�[6�z-��o� �j��y�Z
���� �b�k�,K5�kY��Pz��*-� Z�<wM �Y����>iA�{=�o��x�Yv�8��5�z���X��g�O1SF� �.}O�����.Ku��7Z�2�}iKE�mA�[���b�Y�}�Y������|�s&�����y|��{_��d����1�����Y�L��j/���H��Gڅ̂������O�u�Y�7d���Q�}�_,z˹QWv��S짮���E7v���29-�zo�Ð3U�6+�����-�+-�z��:}o�ﱚ+�z��XlL��g�dRϝz�z
V�6A�[1�5hA�{�_EZ��{5-�oM�
K���۸��I1e��{Um�*��6*[N�,q���/�A�[�Ug���N����F�6_�����
�q�ryY��^���Q�F��M����۹f�U1���]��sg1�a�|,[[l����A�8F)� z )-������Yp�����[.��vqA�������S�q��V�es���u��V���d�*HF*�2�2 e�
����\�5Z��[N�ʦ`�-'f�X���[���-'��A�[NlQ�>��\��kI��V�ў���B����98z�f�a����N�)+���bD�9��b~���ȸW6��h��NDA�[o!
�{tq�j@��V�z���.^�dA�{]�J΂���8�|�rq�%0xz�/���^Xc��z;'��� /6~��N:��b���>2H F��ɦ�JY썶�*�Soy֢�N����IU���F� �b�%�^,"8@ɼ��T2���;w���;��7��I�d#�V���niU0	��Rɪ��X�*T�&։���u��1Z�u��M~�k����:/����EpуG+�-����W���Č�CB2���Nu@=��dl_����W�E �w����y~�
�E)���F�k�5FIvCo��l�"~��q0N�o�AA�[f�/}� adи]{o?p�Z����ɻ<y���]pρf�B����J` @4�Tb�|r�|J�ѾgY�*�F<�{��7/aUoA�}�A0��2te�d��銫s���l\�6#�\�D�������)����Ǧ�oz�~�����]�4s&x�{�������_�2s��FC��^��3�3B�ݲ�_�+�-)Y6}q2�2�E��|�'tHs����﬋��Iq�Mdf~8�l��̨�}O�|��0�ٳuS��l}�T��l����G���bƕ�ם����y3g���q�D$�@4?,J6�a�sbԋa���$�01�k4ђ����io���58m��.C�<�F�Ch�̅S|�d.���\~���E{��.h{/��wa��'�u����pbV��<��'E5;=�[����y�����Y��	@�E>f��{���3\�ϰV�y�L�!�� g~��l�I����E��������h��wЋ�FW����f~��<�AQiY'3#�Ca�YL�L �r���H;Jg��	d��N�ǟ�q��D�$Θ�{�I�0n�.l��	>�L�� �!��)�uI�ς�A�[^��6]�H�l>���A6�� 	�9�l s�;�ٿ�,FY�����)S\��[��W���C�t���B��ǝ7���l��[�q�V�B�l����#g>�f�KΙ2���I�l�u�k�-�\���+T,^��t�ū�����75�A�s�^���p0��k���=x�?��)���%FO�x�i��޺ɵ�B�ƛ\[������7>�^7��[ق��^�Z�]��%���5��l6/�/Y7o�-YG���u���^�#A�]o]�� ��u4yv3��g��[��޻f�g��k��!��-�C���D�z�<e���:�0��v����@q'|���Z�.y/0��Π��#���+��.�\�Z5qw�7�Cr��^��]i�k��i�A�[NA(�u���.sy%����k#�(3{�E�a�U�lU�+*g'�^�O��BK�}�z��B�х��c�BK�-��BK�}/�$1*Б̓���|q��v2�.\����,\����`Ĥ�)�l[F?�-�C�(�[݊/bL�ޚ3���-�iℲ�	��rB��Yo9amc�����Uz�\ΝT��˹��i��{C�l�����ྒjA�[
���!�s�c���=�z�B(�����m��"f�\�E���cv�Pb^B#������B��=LNЅ��%ΐ�c��i�fe�pi�%e�pij���ʄ �����l�>�O�v�M��zk�M��d���͚dӜ��_�l��d]�apA���ya����y�;�E'�~cz-�)�E&�u�\�E�E���4%��4%��4%��&ed���{m:��P�Ϸ�i��|Z[�4Wm�������cma�$rZ[�4%�l	3�<��^�����0�-}��-�"����L���i��c?d�8�#��P!qg?BR!����
DƄ'���60I���9cFM��K��c�>A��4��d�9����&��wJ�Jmp��&�� �d�r�c�����S/#�gě�mP����J��-Ķ_I�����W2�o�}�+�ҷ��I�;I�-�{o��-$�̽w����^ɔ�ǒ)-^TۯdJ�ڲ�.����/�vg����p��.��+msɊmw֖��m�h�w�(�Qn�s��Q7
�/�'�7w��L�޻�.NgK�޻��N���S<w`�IF��9�{ov���������Zݲ�+7��`#E��\��z��hK��m9�n��ۃE�� {��R:���W�E{R|;jh��l{0=ڹ	L���2���J�r�u��I�,0:�U^H�Y��=�v���=ۨ����A�H�
��-�ג��Uڍ���A�D)&�'/�=�))����'�NI������aQj۞<;|	���١:���!�mO9>|�2������n8IpDϛ	��V�n���	�(��O��@4K���%E^�Kj��Ba{I��[��$��j��L�����@N��B��8�sՂ����G������u�]�C�w���
��l+��l;Ց{����@M�ض7��'�#��<�_�0F1�����_Rō�a�l������;��hF�$�9l�-�k�[s�*����x��t<��8`��W���Lmh��ӕi���Q��c2k
��x�5��c���Y3��m�^DGv�fCeژ�82����8_�h��u�2�q�f&0D//�څ<����H�݇2FR����9�����+G���')�v=|<]�-�ˑa��g<]�[��e+����x�\���ȑ�t<��E�<���e�w�������x:�[a���IH<c=b+_
i����cr �n|�9t���e=,d6��z���)o�N�xΙS�Sٵ�'��H�����4d)ng�;� �,=G5x��)�e��r�@�ѱ�f���E�����ft5��6!�]:�s*h��t}!���U=�	z�(�s؎gj��#�ҋ���n=����δQ�4���y����zwz/��qSڞ�iG�:o0�Ya1���ym���D#x{��8j��?X��r�ݯ{6��l�Ãf��im�xxw�wݶ�Dg�P��=U3s��mu4�$e�
�9��o��WNSv��{a�LK����og߫S*���q/��>+̸}�Rno��KaNQ�Y�+�Vi�Sc���9��������.v�8<āu�ԘS��Sc��F�!هn5V�	#�|Kc�j������^���Ma�&^�d�����5�N�:*�����P� ��%�l�#�|kx�Šw~�)��Ⱦ�����X��5�|K~�Tl�%�]�:߲�*�A�[���t�U;    1�uS�M\r����ׅ F|�����(^A�[}�����b'�S�Z�A�뼗{L��:o澙�A��r17�1�`�-5f�V��R��X��h
�*�]Ӣ�^��*?V���*�^��cU� �-=j'�G�d��T�h��#x:6�|��V�E�?�޷8h#}����7��r�&���T#~k��$Ek/m$�G��:�m,���[�}i����+���5�^��Y��i;��v:��6m�W�
��2�lN�$�e<��hp�^���D֮5$����1��+}��h#C��j��RXe�FP��¤��Fcp���_N��	�h2^^�����Vbyk}j ;���L`�U��u*r?�N%A�	.8 �s{� wF��Q �H�J8p��"X��;�����F �ȝ�DRX�X���� vx=v��U�4@��w�R�c�.�Ϊ.RK�\t�m��!��C ȹ�|�Y��-_Z`���M�׀s�/�Т7��bےpc��-A�h���6t��uƬtXo!m�a���S�[t�����6Uh4�MZ�oS�V|q��$/ �f���A+��>�D+��>�Dt��E�&Z|Ѣ�_�@9rG:��������Z�3�M 87�p���f2�|��ŉ�kt0������e3ǖ��`��/��'��F�w���__�� ��^V~�@�tv4��8 na�A��&@?�]���)�Oy�9úd�5*�~�ECxoN�h�q$6������L�z��b�CF�)i��2�7��(��R_FQ�N}�Ս��N�����E����L��M��ׅ\<>A��$��篓\�c��nK�㯛1��u���d̨ ��J�E�攴S�6)�I�٢�&eg�B®;'3pr��Qv3�	_�%;PV�$������HW-�A �&`~�[/�Ek6M��9�EX�@�oIt��+�K�KD�t��$U�j&C�w�R�n�zj׃�8��N!9��D�]������)Jt*q����>�D���
�I�t�Up�I.	0�����&U8��)Z��p)i�A�8�D�a��k`��1��_vm�
:`'���PA|[�����:��9�s3t�5�vlw-�F�X���hJ�)y�02�t�eH��_ZL���N�i��m��F�ك�h1�i1�ߎ��Y��~+�ˢ�kFҷ�J�_Cj���|;n����L�@�+k����Ľ>��s���<�蒲A�-[���A�?A|��n��'c�Û�8]26hN��%c�f������dlZ=�9�dl�<�̭~D=I�޸E��Hs7Z5t n�&����h�W����u ���^K��4��JK� 5��:�M�a�F�����sCn�ʃj�o߂�z�z�)R����@55�:@��4w��4��^@��<��]����ۻ��^{��K;}��������&���M!��-Ej�FqO5��F5�T��_��D�/�h�Ɨj<�f�v2)���2Q�K5����S�>��^��>��^��>�Ǝ]p?��j3�H����jCN��+5�M�ڍ��Ͳ�0����]�,�M+Ŀ�ּ�?HA�y��db�l_[%��L+dS<�G�м�]�)�y��8������cgs%��9�-8�e/�ӛ9="��_p|�?✘w�0�@4�t�sԍHpb��A�9K�q��s�����������E#�?��U���w2�j��]�)��os��ə�iT�In_p4S_�*��h���D��k���oS�P�( ��%-v�_JZ&�;��?������;�
A|����
���D{��
٫��)�-��5��.��><����y��e�N�epٹ�1Z��|m!���@u(N�z	4��s��C����gp�$���0FH׭���Z�\�O�gp�D��V+���(~p��{�N�|���n-؂���݄U�}�0\�e�^Kd���{-�A\�OS
��}���PD��ёE��`T�N�2�cp9�v�h&�MMF���k{��;G�_���-H�k�WA\&�i"}Р��l;`|0�Ijǋ��� 4s���^Q3������<��~���-��O����y������ʲA�?�!؃˖z�0{�G�`�(�r���W�4d1|���h�f�\��ṛ3H�T�0zF�}p��v��o�zҿC,8�3��T:(Ќ���d���@�zw����6��xg��2@��-���uw8W�`.'2�2p�A�4f�d>�$mn�qË#����������6A|��{��9C0��ʁdӤ)��/
�(P MĤ@��g�$��vI� ,�{8��e��֠�KxYF%��I���\;�42�z��?� p憩v
�A�Z?���{Y����� V��� V{�`��D�'��*a����Ŝ��)3p��<�h����� ����9��8�c��#�ZQ�#��i
� V��N�|���<�t�e����K&A\����"�D�6�ٟ��t]�����M�2�4h$q��e�2A`���!h��3�B:�|����4�ai�| ���)�Gܵ��g�(� �_e�� &sy����x�hqpE^�+�T� �Q]���f��
h��
�%G��`�+�w�d2�e~M��+J�yt��9�D3]�S�+�f <�I�a� 4�f��h$͈��H��tbe�9h�Ŵӫ^����<�n�އ��͍�ܭvJc�9�1x��j)C��[e�Ơ~��F-Q��`s3�� �?���`
#�D����9��� i�(��l�(��22U�I��*ѭ\૓Q.������m�@�F�Uz?5r���C�F�U4"Iøi G�dL�Ss9 i~��)�,�&�v���F���qV��� 5<�:�2�V	�X��g�s���6k�<�7���#X�k���P@3i-
hx�8�LJHͤ�(��N��\s��Lh_{>�����@:?V��Ig0־�����*d������2���=�SJ���y\{�ޞ�

�Z8	׼Yk^P��&��Zܨ�`�ōr�Z��7���,µ�i'��oY�?�B�'Ozc����!|ż�I;�L��1����p���ڠI���`.CZ"��g$]���Q��ٺ� ̭�+xmQxo���ײ?\e��%\����(�)� �@|+��=3Ұ`��
�R��/S�;�s��
/x��wU��wFQ��I����$f~ux�LJd���U���*���_U�M�)��E��h�҉��'"g�tg���B��Z���.t��IF=�y��
5���o������H��/��\[�Oؿhֲ">���f-S#A(�_<kw��q���h-�O�
{1�5���Q؋i�a�
:a'���e�&�dZ;W��L؅h�AL�(#Tk�؂I؅j�řBP	{Q�5ܞ	a'�Z�[6�Dؿ��͞��������Dh��a'�Z�q��9�w�q �^Lk՘��՘���\]�:�<SqBz���$[����?؅lm`?��.dk�E@![;�(�\k���������N�5?>�r��~�ٯ®<ky%|��+�ڍ&+�w7������	Ly9�w�������F��������`.a�w+��L6'`aw1��b���M�ܝN�.гщ7�l:1�	|�t�M{ܓN��L�%f��E�
��1�9����U�uP�0�9*��A�*76۠���1���xd3%�78�?����1��l��ַV�?ؿ��2�1�%n��rqt2������'���u�}�9胯�g�Aǣ\�Be��KTė�\y�iP׀�u���	A\sF�΁��r[T�+n8���9���`P�+n8�sp�;������T�;��Y17��]x�p�;���&��{�2��5u4g�K��Z�?������A������Qڞ!�bw
u�1�Y6�����
|N	C�C��w�.K� \��+��p��wL]�D������i�1�x������Zx'ޠ���4&[>1����f��xj�N)e[+� �f��vҬ�EoÝ�B3=p^\����q�A��Yk��U�    �{�p��/�_j-~a��/\�L�^j}��e���Ka��G9�:oԏ`
.�= ;'��S�!�9���[|�#���y��R���M%�L	.A�,��ú���>��6�C(F�t��eg�́a�Z�Q�q=�@̮.RS�<죣�Q`��\/�� 
�$�Rw�G0�_O��*���})ԃa$栓E��\�aH�� �HL�*������9��=��@h�k���CR���(�ڵ�@0ʺf���`'��5��0$]ˡo�1�=hn�\�G/�-�6ؿ���{6�x;^��_�kY���M?=���]���Z*<8���r-KB�6�I��L,�����-���^Llik�K@u�;9�.�H�;��;�ղ���G6˭+4{6�v�E������\��Zΰ*���e��a�d��Ͳ��1d\�����b	)�M�2,��E�vr�;)���93�s�eΌ}����3�zΪ�<b쭆��5x����n��אz-ZA�Z��$�W�=�!xI���Y��eO}�Ϝ'�м黽Pc���vn�(�\�3�lz�QҊ��A\�˳�	.�4>c\�g��2�7L!�Jp�݈�٢>��-U'.���޵T,;A�K��ќ&)��i���.�t��e'8�]Y��1��;s�_�\�ֲ���,f	���h	|�У%�y�GK(�-��;}W;1�������]פ�9u���<�.�F�j��`wn���H���@j!	#���킾�p!B���F3p�B����X 0�b��q�G؋f/��� G� 0d�#���9ӻr;Y�;�����y8L����ù��#~�S�'�z�s$��3��['�L�wu���~1/d�o�żGvp�U��#���S���Q�~
U~)��ރ��6z���`(�3G����Q�H6sGR9��őH�2Gq�@3s���d��C���I�������Y�gx+��І��>Fg|�6\���0�o'�ЦU���0wy�ST�m���
ڴ��Q�g�q�s�����������%���R`�n��" {Q]���vw���Gp-d����e/B�B�|��Qk�f&����^n��6:(�6LIm�g/`!���E;�$؁�tj�(;ȁ�|j7.9�����|�4cD��V� 2���f��K���Z���H�g=	v�ZL[��`�����c�]R�x�:W�m�t��5�i_m��ƶ%+o�!�vݱRC�f�uǉ����Mw7��$�,ԙ����'j��ڊ���I�p��ೇ$�
>H�&Jx�)���F&��r}�Nj=$"��J�������=��Y��<t���|C�`	��u��\����v���\39
�����@Mp��[�<�'�v��;M`F�`��#%	��.��8�l����ܢl�t/73��=�F:�g����H�"�r�c�f�� �f��މ 3�P9���r?�F�����s`�'���|��%@,��}Jx�$��,O;�P��]�� �ܽ��6`L�^��6P�l^�$����{`+��8o�/��f����r��G�[��p�l�3{/��8G*{��f%��y��*a������oa:p��[�X;�9��8Z%�T[9�]����g6�Iu�>�h�t6:�����&�ܨkH���y�x�=��9���o��aK��5��5�[��#q��F�Ż��ɟ&��ь~�6������6=*� �iu���������E�]l)�Es�� oe��&�ܒ��^׃[]���%1�F��%3I~�����d&ay�?�&��>b|��j>��ɮ|�r؇^F�n��^ͩ�C/#�$h��ע��el@_]Ht"%��e��2�a ��WE�?]8�L��HnIp[J���s���P�	�J��G�}���Ǆ�d���Ul��N��6F�2S3B0sԈ>�Mr�S�B0��zș`��k��	6��C�v��?C�	��P0j��X�����3�PK=�y*�N~��&����u�4#�hFP7Zy.�k�����1�ɞd�r��wo�������m7���9]�F`�8o�ea�M���y���oͼ�B��&Ձs%J�?�PB�0���C6�9���N�s�P����L�Y�B gt��ht�hoD�$"�9�1_�yNjF�b-������}@/���w��8X�,��h��b��}�{��Ϧyl���4�!��4Nt��u���X�H����u�WA������<���.� ��K�\@����<��i�����S|�Y���K{X3R3l����̱���p'1.h(������BZo�(F���z�!��ߣ,��[9�Ss�r޾���@�5Ф�׌�d2K:�R��I�5������<�57����Z�l*7����l���q�r�$-s��;�1RB���ë:o/ӣ�V"���-THA�:�J(z�MF�c8�m��S#���*C?;O2����f�'��j���I�u7~C��ʓ��T���og�	���u;~�o_��qy뒮�u�3y���.����\��b�p��Ac��ߥ��+��<�����"���r1�rhhf���D*F$��[�団~�(S<C
+��TR��d�Ð�-/�R��l�ۨ�}��np~��M
{����b�ב��w��Ysq�N��8�O�%Wѩh�ґRg��(E[��f���{;��[C#�g��3]a�gǓ�
;�ko��
��0����ly|��C�����[�����9Vb���*}�%���4�� ��4W��!o�\�4���S%�[��q��sk4�
@Ey�m�t�W����LE�ج-f*:#�b�=��\E/Y.IUtY��*pQ�%U��m���ۜ�{5��y�r�彷#�mI7�v:��s�t��r,o����u�-9m��x*���}���nق���ZY��HI�ݕ�}$Sa�j�'C>��N���j�Ʉ֍u�����HIal6��Ò�q�÷��p\O��x�������A*F���/�>�ռM��Vn�6��Hx�i �����t}�O����V��}%_o����W��V�������3ao��ؓ��%.�c/���\�.�r�u��<�~�Lyv�o j��˃�w|=�����eo��^\���l��%�0�s#Z�h6y4UN$���-�5!�Ѽ�Y"�ag�M�x�tz0�׫B�����+���.&��-��� ~�rn��C'99���Nr�zy��oq ����������мb�<r��#�|,�?[ޠ��7M��chr(��.c�&���wM^mt��,ɫ�����]G���?�<~������,<��GW���+��ѕ����s�t���x�Q�)�8g����N��OY����c���\��oYy4_!�^���׎��Gٹ$u������E��,�?_�y��u������������r��k���q�{4	� ����[]x�݃�w��\�F���k��!f�k�*����C}	3���~{���[&��۽K'��,��y��෗���,���d��u�������$��"�}k��n��ҭ�����;�v!����
oV�{������o��%#�t�)͋�o~�1�]qg����:���`E�ف��`A;�Z<x}+��=o^�!�Ì�ַ�z�̃շ�z�̓շ�Y���~խu^�Z6��}������%w��S���j'}%�j��0�}�"��,Jt�[��A h}�\4�]첤Y�՟Z�ꦊ�o�2���j7k�R�Z�D�ݸ�e�\�&�;�0QW�\8Dv}qe��]�n�Jx��# �?�$��-�d���{����oG���O�y��7����)յY���u4�q!.����*Hmq�㥥ks��/JcMZb��v��v"����!���ǉ��"�̟����#Z�� eDL)g$ ��k96�y�!�̫m�S����@2�Cx>�~��l�~�|i���PDD���"�9���@׿S���M|    �b�'�H�H�۝y���y38K �[̎LO�b@���h6�̪L�g�鴠��p��g�����X��k�xP2���A���;&S
7���Lw���`����W�;�Z�������wL݈?h�jo��6V�1u{�!o�o�4���D��:'�G�e�l��w�/ey6�$UYo6�,��F����w,%�\�j9>���ͷ����Xʀ9�5�(D��ʎI��`;��A����;V��n�V߱���
Z߱��3�G�����TA������(^ߒ5�ľe���^�����:��A�*S��yP�^�.Vkf�kL ��o�V�>8�W����ַ\`�	Z߲�"2���-c"��4S�ݠ�wҬ�]�騃���-�ʴ�7���[�F�_�&�@���nt�H���9[�>`QE3�|�
���5v���.�������t��^�O�y����:�0=x}���Ӄ׷@"Vp�VXa�
n��f��-�&�t�ݷt+ae<4�F�$�}���EG�ț�G�8�18�F�UY+��e"�&2��7e�3H~�*�� �f�<x~�܌̦^��2H�������r����z��^w=����ʢ}Q+o٣K8m����w��S�����Dw��)��h-@�;�Ht4�!Bt4'ݢ�< iTs�`�_Q���O��H� i�z8̀h��ϩ �|@z0��%�����INo9C]����gR����_��ڴq?���A��}���~i�/G�;��-�m�y���#'ܾ�/u���^@�&h���n�4�� �F�A��x$�TxF	�w6�)�=X}����͎GbK]_��H������~�e��)� �-���Է��n;z����S�A�[6�V( 4?�3�A;i�̿��8S1*�f�>OC��w,	Pq�ȃ�w�
�I��o�p���A�[��-�_ʓx4�J�㕭/3.�}� }{m9�.J����v�W	�x�m���y|6���-V���g��q��(��GT��g�!�����{L��g$��k�S�'���h����aȅ쐟�wز{�'n�{�ҷ��ҷa�R��Z8�]y��8÷��9~J�a��E�G+����2�3?)���_�9�@2�	��6��W�o~8�4-ݎ��î��2��b�'�A�;���s*#'(���	j=y
i�%���A�;���;F"Q�3��!��+�-��ÙH|/ͥ$h~}	�?>��?'MJ�����4v5!�kO�F�nv}�4���)�җ�8I��ۉd���L��A�~",�E���/j�6"��\�[��qΚJY�=��|B�`��Q��w�D	^����1�ɒ�$����U���Bѱ$�(�jI6_ʒl�4o��K@ٍ�^�y��yi'�9N�Ę6�NF���`���B��@3_��ue-�8'~p�Ϟ{!���_�=Z��
@�)[IϷ�ͽV���ဂA�;���s��ވ�={|[�ٔ��z����ׇ;��s�#�|G�:&�ٌJ9&_��f�=��28}G��iFߪ�(v|F�BIs���{qf�뜠���s�O�����_����}�D�o++RE�o�m����-}� �'��;�7)������wm#î��0M���U��S4���w�@�;#L0���M��A�;LS���Jd�B�r�󦜻Qμ~��e��o�����y����������Z����Z��� ��<+����vi��k�ҭ�2s9w�y��A��\AA��^�P|�W������JTPA��^G�c{a��fD���f���G��KF����Ӝ�N�߱�����9w 9DG�l�%=A�;��{�����`2߲�]y� �-���g:߲��|0�h�r�KH�ɐċ��{�z����G�>�ʿ������v�5Ջ��z�4�g9j��5i'N(��H]�v�ʴ�M�@�D��NQ�̨X'8fT��Ȍ�M�1ҁ��q��p_\�����~���t��S7#=���H�͍�G��2�(�-�yF)o���N�(A;�M1�I(�]����7�J�aQB��
2�1DQ�o��AO���$�Z��ͷP��_q�!�|[R�e3�d�%fm^�q��M��o�8s��[�w2�3Q:S5��o���(���t��"���٤"�+o<�T$ds��y
g^�#;9�3��ɕ�+5	"��>��� #�{Y��Շ�-��i�C:ae��`�-�徛�/��.�t�I��US�Κ�x0;k����IpT@�΂��[��Hp�(�K9B=��K9�|�]��QP�&���S�[�@��-�*�����$!8L�j����Ep�֨;GmR�e�F�O��JR�ͭD��߉�_}</2��K���X�Z�c;�����pwe�h����.Vy=����]?;�{A�錣�>����\�L	^��a� �o���r��n�o�2A�[�����g�ߞ�ߟ4Ȝ��g*eN���ײ�g>fN��k'����d�׳橙��=��u��M&�1bt�ó�lk4q����Ĺl�o��6K�!W���-+���e�ډ���6�c���f��������7���)��2v�����|��Q�@�O��%Ġn'[%���g�o���7�W&2(}���R�� �P����ҷl\���� ����� ���ɤ�k'����Hư��@槎a�\�7^�(����V�տ���٭&ǟ( �lvM�C�<�AL�Ѽ�}������̈́�����Q4��U��>��N%��X�tҳ�X[�!p]������(�"	��������]��[�*E�#[w�����]4�l����# ��"p���X
zs�bL���}���9�?��9E�WJ��U_h�I�їa�������+��R�
��INSԘI���}�q��㜤�v��J�Q�h�O�N�o3��S=`~#�WF3��xK�K㥨�)6����k��O
o������'�4&�z�7�l^eP�0N��^cg�ur�>Z���{+�'�<(~���Z��ᷬ�����[֎J]4��:Q̝"]"R���:]qU�]R�>Z��͇�I=�d��L���"�i� ���I7�NS%�I3�IR�q���%`��I��<�����ш��Xo��<�_��?������X��<��u7%	~���kR��֢O,�dm��m��[���n��Zr�>_�w���s�ZN�߇��F������iL��K��G��NKߋɂ�d�}�TE&���|��%���*�&i~?�!����Q`��{=�i�$�}��a���|֠&���)���ѷ���5����ҢU��ߊVkbP��)�o��+h~+P�\2�~}�?�>+����q+���q�z��y�;�~��E]o��[���rm�����D�	���F����®D���<z�4�!�{������|U��C�4��hN���/�];w���@7
F�,��:O����2��]z^e�L�l �������fc≃�5�7�lSɈ��k$��B�A'��>�~���C��j�}@�h��>h� 7�u����_-l�E`UWj`�`\h�����=h��@6�cc�?�Z���QǈF(R��k�H������8�F�sη�qI4㦏z(�5I�۴F��虜:�J���Uǜ��ͧR�����۪��F����*�+6^��"��{v�������"���K�ډ��b8���mU�n��v͗jY�̿M���U4�ڳ����4�h#�d��U^(�d�`��nt���h~�Ǭ�H-�̿M�>vE���|v��#���@��9����Ɂ6<���o���I
�FKgR����v�d�zV�u�I����~O��WK�>�7���ehn�u��d�mL^C�I��~��3�A�W�_˓����<���L����ִl^�t�d�ҥ�=��7����V&���}>x[�����(�\*@���̰1�T	�L��&'�que&�oӎ�q���퀆��װ����缆-7v67��]�j>x�7V��a�D���9�`0����
V)�� �w����	V�    �����������>���ױR��-*̹�M�_�|W������j��D#1�;ĪT�I��h�Z1��g#��|�h�P#�D#���=>nf0���q�C�� �h���`��ˠJ#ǥ`�Ha�`fW`����������b�����zNv>�|ב����A3�(��s5�r�C���,{�u�����>eٙ�����gp�����=��w��n1/�{�?���њ��cv��[1����D�����h�'7�s����5rk??��Q��xJFq�����
&|ܯ��"SCtp:��:H1!O#t-h�eL��1�U�t s��@2�)'h#�je�\{g��^c'8�A�{���`��j����W�_�x��������ܿOU��{����-�Qז=p�a9��=pU��gW0==[���/�Z��fye;�`�e^��˝�8n�W�w:i(>A	��о���.ZAh�X�p���B-Y��E t#t�V�񐠈k�P�by������>�,M#���ۈ�DxUO���S�-\[@���`�ؽ]�A�|��%u�b�Y�:�2��~�c�-C�c�-����1�a,��͂�d��#�C�f��,n����j�=C{E��c2q���(�c�!V÷ pݵ�<_ya�{nԷ�o���|L'n�Zis:&.��ӓ��kP�-��ײB�u�+�U�FǢ�J��w��z�����U�u,TN�R,�� �ˎ��_�[��	��ڬ����emR�c��vW$���~�����]}k ����H�#ok���]����E�M��]r�M�o浾5����n�_���=�Ʊ��E����v8�6��>|Q����dx1�qO��8�B�����M�Ѱ6��t�����ŠRBx_^4����"��6��n��	�%tᾦV�(�TB��i��mO+��m�XM���ђ�T����	�R)U���J�>K��x�Y.V*E}��׵�F�N;�o][�Ah.�
�]Q�'Y~���{w��ߞt�]m�)1\���cW�[��a�b/}��T�+&����Qz�>6{�Yy�R0���<����#K�|���T�D�J��R����O�����l�Bs\��fF�[�!t�g�ߑ�I8�÷p������;1���_t>�hz�g�Ӧ�oq�`i�b�UI��YvR���/"�Ptfݙ�(�3��|?m茲3,��`*X��P�-ܑ͍,<���T�D^�v�iT,�>(́�%Ҭ`ܵW�ߤ���)�t�>X��<�}��̛ڿ�`&���s"�u��eg�)������8'Ճ ��Qv�9�w:���r/���~�*�-ӧo5��DЍ�����s�Kh�>ԹP,�ut.\�j���>�\XB���M��;�D���Ҍc&����P=Z榚	X��zX��z,Xj�z,X��z,��Ś"�AzY`��K>���ȟ=bg���FYX��:fMl�N�gfљr6�T�I��&b�>9M�>9���3�Yv
�)t���ɩS^g�4%����JS�8tN�4�	��f�gJ4T�"�i8t�>tN�<����|J<��#��+\�Ӥ���æҗ5�n�^��<�B��ҧ"y/? L�ɷ+_�T��,��,r#��B:��D|<�FY
��,�-�,|=�)���@�D�SxD�Q�͖A���,n%���?���_�-��Ph:7�M#��_��U�u�I׼����e-�ݥ�Ecw����3��}t=���u�t=�����P��j,��DondcDW��z(Ca��$%��]SK׃�����z����
��[iP��>��I?R�uH��RtE"����4��u�
��3��I��� I�fp���L��A�>�$Iu �I�	4�����O�9���@s8�e4kT�Y �A=Q�f`j'-��:�>��kᄏ"�N���z�`��<���t)���	�����T�O�]E�t������}�W�I_�Q��um�C���|o]�Jbӵ�s_����^#X�QǽH������i�m����Z�#�m�6��*��CG|j���9tć��]��9}���y�B��9�}��:}�Ϗn�>O�A�h�W�%��3�&��(�<���s���%�Nʓ�:�o�}\� ���!v�r:0r��k�����uS�=; ��3���bC��K�ڨR��V��[!�7�A˦�w���T��6��)%AS�"AiHnX3�������W�l��p��NC?��hD�i�F����`7t�eHc-�,C� �#G�j��^1�޷�$��@���>��{�����h�r�1ߗ��y/e��^J���b ��o��Wk�A��@�	~ն���%�_a��Pø�X9�#o���N)��e��vd�j</aW�=Ѱ#�v��<W���y���߭�;b�-��ʢu[8�����[*����x�ł��(_}lG8Z~J��[)�5�����
�_ K{~�yk��f��U�V~͛_8��t0�l�{�]�꒾��ݑ�z������^�<n�4�]�Q�J�xz��BppH��7�u&��]~B���	n礵��Lp?,��"���[䟴��;�����>��%��� ���y�8�V	��g"�F��vL����v�*�������t��"\���ބ���Z|r�!\��*�[0���E��m��ۊOо�u���[.�q}^\o�L�A��Y>�����B�!���ga8�&"��N�Q����4*���Fef�!�+��<�I�*�21�2��D�� ��d#���/n<��?��[�0;�D)F��Ƿb0�a ���� �
��7��v0�` tQ0�n
�[��P0�{��a`�W�h�K�����ԏw�c�����9�{Kë�l�['�j�z?�G{K5�6~23��킷�ޮ��afg��l���.w���.�|9J#������-&�8���;���$�&�@�&6P�<��z �ȿ��eSnR�|5ʯ��O�߷�0P>��y�Q�[�3ⳉ�I�C��O�I�C����)�h��Фφ�)��'B�O �EV�(�� ��_�NҞ>/f���=8^�[���4^�Q��^%�nx��Tr;{��U��]�$�)i>�|�����I
ԟ��'iPyO�ح%�$	R��?I���\�O��r3���4h��x��7)�I�.&�O��QV_�Or��f�x.��NaNҠ�����Z����i�Μ�T��[����,6��4H���V�fn����/��q�uƨ�����l�k��.&����)"��Ώ���?P |�>��A�����D�
l�ݗ������~��/4s{8ɁJ���Ǔ����#'IP)�Ƀ��hd7�]��.&jO�率�[.z�����FN�����mh��m'$'��(�zar`Xc�Ɂ5A;��%�R�ԅϬG��X	�^J�'��r+t����盧	� ����A�(N�#[�p�7nⓉ�O&��V>��Z�_Nc��O&��V>��G�dB��L��J�1>�po|2��'��d"E����D�p�^N�0kt2��ǘ�+m ���1�S?&��R�v���PB�sѓܧ�j�I�����Fk���t*�E&�Iԙ��~r�Z��I�*��zx};��A�V�< }k��C�s�[y�|�?���D�����hPz�&!�k�_��P~ƍ��_��H	?��p��`Ү�~)�hG�>	��΢�מ~)���`I�\el���ʠ���Zq��S/�}��_̊3���h�i���hĉϧ���+�ԇ�A%��>��R����S�*F����Hy�;Iya���Hy2��_)OV�ZR^&.�
oK��Ȓ窇�pj^R=Y�o�y���
w!�5�m_}�VxZ�����%OV8��2z���b�gL��y�ߞx�E4���u���Co;�/t��P�orOҽM�A��M�a��C�?�꟟���>�HC�,�v�yr6A�`�8��}�h!h@�(Ab�#hs;��QP�!h��2EA��+���b��"d��/D6!����B����i�E������Z��ꃩևf��c    �A3�	 ���ca��E��m'�����3��!�'�W!�'u[�#�J���l1S���Yqm��������>���?�ē���b� v��0�p'�4��ČFz`M�K���&�!���<#w�! ʝ|ng�!���|}r��h�_��!��Uj~"��Qw&u�R�goI�<4P��4��_�I��N}�w�[�l��jD�C~�{��a��ƕ��<}���<���ϥ X8��x ��t��*�U۝����T�Z�q�FA���t(b�e=7B�U=-
�Q�4)�yh� ����A�&��4iPO}�u��v��o�:i��-�sN6WqV�ɮ����]����b?o/�����-���IX��!�C��d@,�E�J�4�I�Z�K9D�	���ڒ��O&��0�d�|-�%��w�i�ҋ��2�|�ɇ ����5Q�}n"��R���g���5M2� �`K	���v�!�DP�T+vu7U�Z��ԧ��s��C_nG�pj@�3�{$�+{���Ѥu>:��ΧB���T�\B:�����
K��б�d;:�d3a�c	?�t,y�[�*�J����T�T*�'w�ҩ�%:UIJ�S�y$$�f��T	�
�N��&�ٿ�K+�h2 ����I�X:��H���&"-�3�5Y�7��&��}�P���S� �|n4Ds�9qXEl$���|-�~��&���{z���&���{���彼���yX�s|�!yχ�8�{�<s驲�<�mǊ���{V����v&�\�&�R����Jj�0�Sj��v����N�뗆�z,+��~R�_Eْ���,�u�[Ҟ��"�-iOI|�mB��>�X��{I7,�OT�A����5+�*il���GѼǔ%����`I~H��F ��e���=7
��%���{�l�ս��%�+fl1Q��_Lԡ��b���g�"�
���vŶԮ�����G����❯��C�+�f(?��q�C?�?���Z�zk��8籖�Ff�S��54�s�c��v�!䬇~y<�r҃�)�7us҃�4��l�5�4�p����}e�Z�v�O�����5�섧�h~/��~�5�섏]��qt��E��u;�b���pq�d��xNh������Q�i���S3��� ��5�85��^���>����$ao]���&�������8����_����V��o�!���Apw��M'���B��EP�2�T����JR�=��Щ �U�_�N�������o�=��?���?Ɔk ?�I�l�h+�ۀ������wa܄f���x�����!��Q#��5���k��	x�.]���ښ'cb����}��X�'���=��w4b7��ј� `WɑQ�\���O(��K1��/�s�������3(���q�� ��Y���z�MA���QAp� ���GiF��}���݁$۹	ؙ�x	�#ᎁH��z~��c"FO��?z߸c��فs�!�V��7Έ�hZ���;���p�]�3�I����;���o}�i�!?�����;0����)�����S�U �� ��'%�;/�|&��z�B��A��B�5	�( y�"�O_��kȹ�C �v|�K� /��8܊e�:�-"Tr^.6��P��%��Ʉ��O��&� =��VZ���nZ�$'(	PY�O2 ��c��������@%d��|~����F�� YP�Y��C^ކa��R�F2؛D�����dB�d�h� ��4��$'Js��ɀ��8���?a���&�)�*��ܧ��)L���BMAR��
�I}��w��CH�f��l@rQ��bb������mZo�W��]�6�} K:W�J�f�U<O���|��}�'$���oJ}�<��w��L����nR �Dm�Hnr�R���7"	�Sq�<����8wi�&��� M2T�&ғ��#~�@oҡ�5�%�P_6�.���gc�L2T~�q�&J?��ѕ�l(��%$�Pz���&*�>�\�{���M:Ԛ}M\����p/뙸�+����b&�N�1m�j�CǱ���y�<��=-��˭=�$����#�d����"y�����g��P�<|B߸��|D�f�����|B�t{�P��o��#��:o��S���Q|J�W��g�AE�p�3�"/���A�q���tN�6�"�'�3�",�L:�H�ߞID^�4�!�زg��
f{&�T؅���u�IZ��
륆f̛��Kf�?�$5�nō�XE�=ɋj�N�fO򢌭�~r�ڼ�ň�Z�O>TJWBa	��g���|�%�+�_d�����eO�B5���Sdy���"�k�Jҡbw���k16{���W�/FTN�E�r�%z �x���k#?�P�������e��{���}#�=Ά��1�'t>R\"�O}��Ht$�ܿJ�=Ί���XXNf�����85RV��<����qr�$���:;R�d�ѕp���ѕp�u����R57�E�ׯ�g��������g�JGK����E���1���ڣ��G�G�=V�<5��|����"s����hS#P���㤗^�����#7���+��c��{T��2v��n��6�1v� ��<NSr'�=wӌ0~�q�����>rz����$���g�]��r7A���a�=�����
~ Ϋ�xH|37@� ��⧷��<�8�� �@>�	��"�6� ��x6Mujz6�Csъ��Gh.V�[�Q���s1�������������ɿm��'��O��1�/e����~��9��Kٳ1��dԘ%���7�{F�o��Ƹ�R�l̇�?z�sА�@hcNrjfш�Z[���QB���ގQ�B�u=7*a?D=��1��
1��[ճ�L� k��	��I��\��Zԙ	;֦�ώܑ��HI�>�2Vm��u�F���}>^uU�+���\Z����s��	��s/@�9����)�k��-�~�I66݃[X�����/!��Cȟuhg�'�3	�-��8���r����Td��h_��M�!�};�7����O2�2���
DPZj�2�	�a'gM��[�aG/L���!.���v��Ma7[��\�G��\�U���$D�ȓA�fP~[���H�7����&#:�/T���QOBTR`AZiBoM(��[gb-�F�!�(ņ�ƴ�x΍I��fb�Ç.6�"D	c�7�X0��1z����,�[~^��U��Y�32�����xg㒟(�e㒟)z/V>��a%��x�N+�|����J����͇N+����*��f6:���(��UJ���ऒ�]���JH�n�9��)%p#�()7�#8����<pJ��gs�����n0xW����$A��y�JӺ���A��=Zk�ך�\����3�	4a�o43���~2��<�f��7��5a?'����fr ���i�8k�n��簟��c.�9�ɂH��0�I�H��I�~տY<�G��ńJ��ml~x� �ŃH��n���fr���\fb�$��D�S���iK�s��
��|�`���vsI�����$C�9��1
��C��E�&���f�!���ۓ���=OB���f���y_R�V�rg�I�Z��)>��%݄ �d���+��d@���h����҃��E�J�����I�o1o"�/߄|~-Swł/�ݵ��,&�/&_�۶*:+]�j�x@T�4]'���H�;�޺Ӡ	m��N�f�e3g�yЄX6��{��\hRV\<.n�x:�����f�	N�&��0^7
V�;��=�D���	��!c�e]m�+�6-|L��l��Ų�1oZ�G���p��2�z�i�`kee�Z++sxؒX�oؼ�a�;�󆋭[��;�n�e�����y�q�w������=ԌCB��Wh��D<�ԝM�1rk�[��[�yhtN���A���}��ݕ��,�ϩQ^�&�N#}=�F����GhtN��z�F��(�x�Q�D�z."� �k<������6��f�D��5b�$��؈�ʭ�5B����8o�Q���zl��k\� ����=��    �Y��f9��i�<�����c6��M�4�y0�k˚��H��5����4z�O#�b莆�b� w1r���ׁ1X�\j��Kf� pw�ҽwĀ�/��H.�� �n�n�}(7Aw�Z�6!�߄\6vGcwvG�~��	����	��e�Ǉ����Y��j�<�`u�0�x�	Z�]gaV�gc�9hb�	� ���Qta�(��6�u.�a�0�Ch��v���ГIh���,��|e�'d9w~^��^%�O [� ?���%��^��!�Ϛ
��Is�L�$�D�`v������W�|�Z��W��s���2��`*�d?���X'�iU��:P�r5�ɁZ�Kn���\$`%*�mfo�\DM� �oe7�5�/��b��)��t� i�)(�ɭ�@�hy#�nDu���/G���q;�.O��^�<����i���2��a�u�鱞��pL^4�iڭCx'�i5���GQ-z�J��!jE�Zj�z��$<t՛H��*]҄���e��'	O�t�ם��e����IzZ�+G���L��&�i�.��]�g�~�n���#�=u'�i=��C;���$r;�OW��lܻ1��쉭$��c7I{���t����)|�'6��F������f�'6���h/�Q�Pq�����]�g��N.>hZ-�{�vf��""���EL@�kf���\�b]��oP{3��^N�f+�g����*l���p|J��#��껓��v�A�\��r�h6S��I|��Ŋ���H�3�Oy�to'���&e6� �?����?��,�f�ey'�9��$?%�t�%�iE7{!�\�kV�����I����qb�F����ƨ���G|�j�k���V���>�"�窝�'c*6�]̧�N{�H�&��(�{Zd�u���{4/��H��"[��O�*�� 9P�l��ڰ� ��g@	 �	�ł�GLEҠ� s��B[����{�ȷ����B��LB�|B�d�=��F���z��p��$:sm��u�e��!8
G���t4Z��z��Y�H4^B�	����G��҃
����e:X�ң��<��ь]4#�G��㦣3�G*��qbQ=:g&�;�>��,=�M2��T�P��D���)T��d�P��Q?z|
��c�����z�D��i�>���@����E��A�u}c��?�`Q:z@]Ay�v��}aW�F��Q<z@^��'�z^W��s�K'�9[����6B���у2��QAzPM���@�K�żN�?��~*^���h�-��("=�`�q ���á5�V㢂�(�-�|�w�|����@9��
-A�EkPֈ�YX�&����C�����iX�VD-��bJ<���l���병�y?��-����[������U�|�%��?A`ϠQ�3	���.XT���g�ǻ�{��C�=Gh���(����Ø��s�������~4�e��&�:ң%��l����oQEz�I2�QDz����$�vg�!�.L��E�Q}�Kk�3p�j<;��.���Z���ѣչ~����q�W��
��Z�4�g���]���nT�.�O`�a��[�d�d_��Y�%�FHs];�	�'�ኂaW���hw������R�iWG���O��HO�2�I�S���K�ӎ��������Ҩ�~�{y��]1�O�y9y��ї�T��uy�ћ蔇ܼ8y�M���ؒ��D �=R��b������'��QIړ��R.Ez��ה|���I��Bو��t��aE#� la<��3(�C�A��Q��5��"<�R�0O��/2��[x��:7�@����(�w���MI���8u�$7�x�)���cG4Cn~�'?���8��"?=+@��H�w@o<OG�����DYwí8���0^8�|���$�Ⱦ�D��k�p�g݆���u�8����]L�~��d�a��7F\B/6Fn"d�a�b63�ۇ��й# _�pѝ���;�cqH�SQuCZ�";�Ys�d:��� $ϡRl�H*�/�C�ؼ��tP���y��^��0{�v��P)���;\�-����q+|�B@���)I��R[@ 	��hD1��,M�����҄YI�S>�J��g�<;��KqQ"�R��(t�uM��i��E��iG��ӎs%>W�t��G`��_2܅�;_�L��Ԓ�oK�S�h��$:�>�&Z4'��s!�k�<N�A��%ɡ�k�b9Tr�I��yy��yP�-PQ������/����>�)�t	�P$yN�g�&$�A6��.����wI�Sξ6}���R�ۈ�kF�>D�����j�Qz��B�"��3K�#�A�>��ïw��o�J������DI�������������v��ϰ#�J¾�N�y>XNx6���'�(���͋���'7/*C��ܼ��97O���}����yQz��#?�Ud�yhEq������7���Qz�WV�h!|��ϋ���7?/�B�L���Düj)NQ�������o�TT���\�(�{a�aZ�@���fD1�]�rƢ��M-�J���I�/�����|�����vؽ�{��M��v���Ǭ��?	y�%�wB^�ޔ��K��Ļ(�����?���t&P�m��s��}�(�)��f�4�����(	�?I�>�Ҩ"94�B�?��M�@�)�%jCo�Tܸ�g�,Qz�NS��d�S�2�Q��h\�.GN��Ѕ�N{��Л��N��8�n����$�EI���v��)�Boζ���r �*�ѰT%X|k�����t����n�����
� 2��i��ա7����V&?~�։O�Qz#I�PdJN}��hԐ{��7�;u�J����(�����b��7[/�EoN��^���f�Fz^#V�`�Z��M͋bѻ�?��{�罰G��l=������D����m�/
�o������Tԉޟ���v���v�"�wV<���ܭ�A��[n4Ǆ+�[x�۠���#�%	�D�l��+@�쟞�#?�྽�W��iP6x��wh�ڙ06y�DʝM�?��g��O����v�?��g��O�������tL�?#/�u��,=[��Vޝ-�?/[��~촇~촉"Q�m�H�E�h���)|�]���%)*u�5���YҡR�2�7�[��2�i/�s�?���3���߃0���_�0���ߚ0��yj5�q��?�d�?�1��#��C�ݳC�P��9@�C�sg��!��C�P>��u���C�vh�$�x��!��C���Z�2��orXR"$��ڒ���Nv�"��	�i����BP>�X>���O+d/���,�ΤG�dɾ'=2�6Ɏ���i�E�A[2�%/�̜��E���&C�jm,��7}όc���,����˒q���p����=K:���gI�8}/�Z��-:���@���ِ4B?lHp����C��gɆ��tr��,$h�0��tH2"N�(�����х�|t��>|t���n�#N�n��}�$�����T�;z~�C�fA\Pr�Mr���v�����7��&9j�"<n����s�DxT2�-m(�(7m���ػI�([m�Y(4��w�!�o��F�L�\�cz<9%�E�,�nrՌ�"�	C}��#�=�%����p2��d��ǙOѕ�j�$ٙi�=���?XQ;zQ��G)W/�F�O��E+��SvBy��ыS���p�|̏�Q4z!W/��(�Z���=JF��\���~s��`��2s�����F����1�f�E���O�^T�^~3��Z��'S/�Eפ�OZT�^�d<E��������C���5�E�������[H��Q+z�� D�����}�;z��Î8z�Q��E������.��ڑ��ݿ�jQ:zq��;%� �izQ9z!M�m� ~��@��^��W��O��Q3z�&�E���	zэxW� �u'6JE��2t�'��J��I͋ыS�<��$��̪��TUi�W.,j4�8��'>�Eu��G�QpUȢ4�BU"�����/*C�?}Zwu/�B����}BX�ļ��>�y>"f����e���|�(]P�� �  +JC���8�������E>�{o��^����L���<��+��:WEq��I��^�}`A�h5s7�C/J�{��Fq����w�<�ja��'oԇ^��ţ�:2��C]�D+q���+���eo4j$SA��ǥ�v.��������WKu�7�C/��U#�_�����*��(�Z����FQ��MӻQz�'��V����n��^�<���'Oo�}�ga�FQ�������E؟���(�8O/�9����O��^ܨ|~����\��S�'=͉/��R0N�'c$4��,Z`%��$㡻@�,{��,�Y��E�{x+���ɋY�dda�g���\L��	z�d���%*��t���p}�+�}��pn�}����CT���\瓙w�d:��w��9-�]㡩���?�
�Iq87/�X�g��Pr^����|��s3�r:��>�r�I{�9��8��������U�>B��(wV�ٰG/��E��}�������v��B'�O;t�=���)��W:��^y�}��!o�����	z޴�1d<�Y(�!�0w�!$��'��!$���C���%��(N �仏�	z��3H$�y'ܼ��c��o�{1���+�C����d=������=]�+7ĵb�O��,v��<�嶁�ָ$u�$;y�/���5#;y�'+�>�i�TF�}��pV�F�oV�}�� +/^�,I�wG���󠑖�'�#i�OJ��s����Eu��h�YN����2�洣����9���;�甗�u�;��R$�A�\�q� 2�x� .�u��A\N;�`:���C�[��,���%Vj��#�N-���Z�͑dGI�C)za��|�\�#���}x����H��.&�GrJj��`#)���$ǡ4=o�H�7Kv(ϛ)y��Ҷ��^��";��������5�����ׯ'߱��G���Q�Ŵ5����3R#J�����������      x   �   x�m�MO�0���������8O��hAB�:oD$��L�Rd?��$���fu��H�&$��u@Q��UA��#D�E	+�u�}���X
lQ�ʺ7tI�0�6����A!�A�iQ�b�qm���2���s��NU�Aņ�*v1��Gǽݝd ����t��\�ض�,���}+zGT���В9��gk�%�s��ud��&��-
�j��)�+x�-����J����aT��      �   �  x���Ko�@���_ѽ�;03@"E�0<L�`�ʆǸ&�r}��TV��ԫ#ݣs7�|"(x��=?t;^�i��������sC�OՇ��]���
�.]R��5g�s�P��cP���ϛnWp;0���isv�6C�i�65����&k39b���:��Q�(�j_ӻ;@�y����G7��[���\y�BD�CI1��s��	r����H�BeQ��⍔������}� �?��g���Z�� B(
}���" M�Dw�˼l�ײ?_st����u�0��T�t���h��x7u��]M-���&�Ϊݟ��������\-��?���0�~u��z1N#J�r����X��K�?�+-��Űf�6T�q2��	X�Xȶ�-�e�"������4��y6�L~����      �      x�3�4�4�2�4�1z\\\ 	      �   >   x�˱�@��W������
���T�ٺX�غ�\zq�ţ7y:,�1��� �U	�      z   N   x�3��-��L�,H��,�T�M�KLO-�.h�e�阒��%��8�KR��3RS�0E��L8��r�L(e����� aU(      �      x�3�4�4�2�4�1z\\\ 	      �   �  x��W9�#9�ُ����$x�����;�"U��,�%�R	 $��*A�� D�����:�d��/�>��(\Tkbmb�-##�U��V�0��'�2'��Q�UǺ?��0��ƅ3<8II�L6��/{�瞡���G�\�d�D3�T��O�� @~Ep���/
@��Z��Z�Z�V;'*m�ڷWX����$�0��+�,������zt�oJ�F�nֱ93�Ɲ��t�Ƨ���L���"S�����6���\&�c�w��b�$;��b��`@����X=�ʩ�f) ����A���c־�
���i�"A��LJ_���EEHr��ZWQ��r*�4�o��^9|d���Db����>��x4�8�l�Ԣfieԍ�:(�5�7NH�%��~읲��x���?�9[�6�R�n\y�A[�b�8ʐ���b u��s������'������`���0%ݸ4V�(�y,��-~�_I��<�R`g�5�_��㬗�8e����T���%�5#Ӑ�������q�X���qU����m�K&M�T����TZ��jo���������<~�j/r�:��./�c	���2�Z\�Ve�if�r��Α<T-k���,�����|��^ˣO5u�����^����J/dt9}�.��~����)��{ҥ ���ǜ�}dϱ��m��B��ϋ��Q{F��+f��?rv��d��5ޡ0��s��>D��NN��>9=H]�����N�����RsZ�N��T���_��פ�*���$y����诤�����w��%�>�'��(;�6�G�4�,��!Ř��������|�ާ��S�Y�i��R��'��O���.t��3��k���=h�����b��~�^�щ�����E/�Qw��__ ���t�����N�:���q�Ȑp���J\B�@�VЧT;���&:����*y������׿�s%      �      x������ � �      |   �
  x���K�.�F�����!�-���6�i&�<��`��s��l������}�/I��cɗ ��b�*��+��A~���*�+���%�\���(I�J\+��Z�Z���<����\���sw�J֮e�4��im�?F�[�v�{������#�*rCY/��m̭;c��p>�ܳO���V$HLџ��\�p�{��v�k�1�%μvL�b~ڮ��}�<X�y�yq�Y���������%����O҇�4^��j��j'1ϴrM�奯8Rh�Ӎ�i���-=�Zc�꨽G}y���B��\C�M>y/�����϶N�[��?�H��FڈHu^Fޫ5I��/��t䖖>�[��T���G8�k,zFZ���{"�����K�ˢ���0�V�g��4f���/iW��g��TJ�Nw>4�fr�B{\ў�e|K�j��m�5չ�1RJ꼭 �}>Ua�����ͪ)[���{pN��9�I�￱��w|����⾍Wn�����IFiN���-���t�u[p�Ʋ�|�[Ԛ=��v��;}��������|W�nBb���Y���b�q�%�|�mv�ظT�ʓ��|n���5�~eN��^�Ըn����\o���]Y����g�c����>9:?9_A(���e��-�I�1,?Q1��5^.�D�F�Ϸ��&��� b��<��rN08��+�L��/�Gu�$��O�G!��[��u���	Yn�_����}9�.mP���c�ZVw~oH���w�>z�����"��*��M�q�Ε�\���T�tt����i8Ϛd�ќ�SS��_~/�[�V�G{j������a�c�?":��a��Է>��+4>���#r��=��ڃ??����Y�>d�4o�����w���|j�B�}��:��X~�}RC�>y'���9��f4_����r��+�1I��@Tbڝ��
������M��Gو��,��MRk��+J2���0����>ƥy<��Q���Kg������m~���ww�bY����f�-&��M���?�8����S�λ�<U��ɾ�8˯�����ވ����[Y����������"���z�u8o(S#C���q0���]%$�KF�n�)~\������ŏ�E9�:���I����Q�dI���'h��KPz'��6,e�	���X��k������y�ê��Ҭ�Y��x�N��;?�JQ�瓞5s$[���M�p㻬�fͻ����zD�=�Qwd�:�S7�P����ޛ_>��~x��	�ntL��@���o�|?j��/�������j�id�9���K�9����o�Y��g�|�%��v�8�]��g?8�i�vn?v�%h�����o���s�y�Sq�E�a��×�Ǽ�q��������"�Z�9'�I���!gkKx�=_�ys��7	�u�!_^)��t�-��x��Ղ�_�1�~������Ml���p�:�2���z�ݲ���������d���a����O�z���'������l�aH�	��O��1P�͗�.e�.�3+z�r��Z��O�D�Gt�DxQ��|ЙF��|��������h�#l�9�	�-�\�T:��h��z���$���;E��뽿�־8D籣������������)��� HZd: �ŷ�N�of.�ϐKcq~k����Yf>���X}�����?ᕥ�O^� 3\����C����n��lBS�O��Ol������<[�����,��w=P0�U�������	EI~����X�j���vF��|�Q�9gZ���_O��{��j���8�R�.hƹ�3���J��/�??�ܦ<1��y* 3x��6m�/�u��v����盳�5��.g�=�q���D@��{kA �#UeV�S��"k϶��H^zJ��ݡi����1�K��+C8�՗��ki��Na$�V�M҈s��4M̐��x�H7�?�/Z�9�02������
�[}g.
ӥ�q4�z���y��hy�Yj��^��D�s^0����y��t�����5 G�p�)�R���~�p\�yC����G��݅�����Q�	��ӊ����?����+������??��i�	K'9�Kb=/��GC�F���"�a���80RM���Ya���� �H[�_�fR��8�<��5���of���"��n$��_f�#�ٓ�?�#�g縋�8K���7��LZܟ�t��s:�%���1��CQ�v>�
��ě_��³�[_�3+����4\�/��;f踿��e�0^q���y_��t(��s�����]�4J����0]P�����J���Ld��9��qg��hjo�C����|1z�GL��yU�6iϟ�czu��|�wu�%s���翉������2�O[_�� �t'�/�KW�/�3U��]�w&��1��<�\bҽ�?�.ϐ���ď���̌�+�b��>Q��g&����OUW�t�g�]�������>�ood �~1�D��T��n��+V�\�D? �b�I�����]�K����;ݍl`̷������(���V�s~��Y9��*�2�z��hf�P�ߓ�Қs���y��čT-��\-���O�O\t��������4��7Z����~w�PG�yaaZHqN��>01�����yFx�k����]�ǈݡ�|�y��Q(��ȼ�au~���<����gG��/��yۨ�޼�c��o4Vź�O��Q���d�ǀ���������?������o���/?���_����7�������_~������/_F�.I,_����V·R������c���V�ې���_��Ȥ��      �   7   x�5ƹ� D���?vms�B�u M4����X��q;�����َZ}?q i	�      T   5   x�320�4404Bc�����tN#=#=3��Ԣ���̼t]CsK�=... &�      �      x������ � �     