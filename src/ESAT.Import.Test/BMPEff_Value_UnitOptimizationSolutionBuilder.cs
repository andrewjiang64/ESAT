using AgBMPTool.DBModel.Model.Scenario;
using ESAT.Import.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Test.AggregationTest
{
    class BMPEff_Value_UnitOptimizationSolutionBuilder
    {
        private List<UnitOptimizationSolution> _uos;

        public List<UnitOptimizationSolution> UOS
        {
            get
            {
                if (_uos == null)
                {
                    int watershedId = 1;

                    _uos = new UnitOptimizationSolutionBuilder(Services.UoW, Services.USS).GetUnitOptimizationSolutionsByWatershedIdParallel(watershedId);
                }
                return _uos;
            }
        }

        [Test]
        public void A_Init_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            int cnt = this.UOS.Count;

            Assert.Pass();
        }

        /** SQL script to get the result of 2001_Conv_ISWET in test db
         -- *** Intelligent & Manual selection - reach BMP ***
            SELECT * FROM
            (

            -- *** Intelligent & Manual selection - reach BMP onsite based on reach ***
            SELECT 
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId", 
 	            'Onsite' as "BMPLocationType",
	            'Reach' as "SpatialUnit",
	            trunc(avg("PercChange"/100*"ReachValue"),6) as "ValueChange"
            FROM
	            (
	            select 
		            mc."ModelId" as "BmpId", 
		            b."ReachId", 
		            r."ModelComponentId" as "ReachMcId", 
		            b."SubAreaId", 
		            sa."ModelComponentId" as "SubAreaMcId",
		            u."ModelComponentId" as "BmpMcId", 
		            ue."Year", 
		            ue."Value" as "PercChange",  
		            u."ScenarioId", 
		            ue."BMPEffectivenessTypeId",
		            smrReach."Value" as "ReachValue"
	            from 
		            public."UnitScenario" as u
		            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		            inner join public."ModelComponent" as mc on u."ModelComponentId" = mc."Id"
		            inner join public."IsolatedWetland" as b on b."ModelComponentId" = mc."Id"
		            inner join public."SubArea" as sa on b."SubAreaId" = sa."Id"
		            inner join public."Reach" as r on b."ReachId" = r."Id"
		            inner join public."ScenarioModelResult" as smrReach on smrReach."ScenarioId" = u."ScenarioId" and smrReach."ModelComponentId" = r."ModelComponentId" and smrReach."ScenarioModelResultTypeId" = ue."BMPEffectivenessTypeId" + 12 and smrReach."Year" = ue."Year"		
	            WHERE
		            ue."Year" = 2001
		            and
		            u."ScenarioId" = 1
		            and
		            mc."ModelId" in (1) -- BMP id
		            and
		            mc."ModelComponentTypeId" = 3  -- Isolated wetland
		            and
		            ue."BMPEffectivenessTypeId" not in ( 1, 2, 3, 4, 12, 13 ) and ue."BMPEffectivenessTypeId" <= 13
	            ORDER BY
		            ue."Year"
	            ) as foo
            GROUP BY
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId"
	
            UNION

            -- *** Intelligent & Manual selection - reach BMP onsite based on subarea ***
            SELECT 
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId", 
 	            'Onsite' as "BMPLocationType",
	            'SubArea' as "SpatialUnit",
	            trunc(avg("PercChange"/100*"SubAreaValue"),6) as "ValueChange"
            FROM
	            (
	            select 
		            mc."ModelId" as "BmpId", 
		            b."ReachId", 
		            b."SubAreaId", 
		            sa."ModelComponentId" as "SubAreaMcId",
		            u."ModelComponentId" as "BmpMcId", 
		            ue."Year", 
		            ue."Value" as "PercChange",  
		            u."ScenarioId", 
		            ue."BMPEffectivenessTypeId",
		            smrSA."Value" as "SubAreaValue"
	            from 
		            public."UnitScenario" as u
		            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		            inner join public."ModelComponent" as mc on u."ModelComponentId" = mc."Id"
		            inner join public."IsolatedWetland" as b on b."ModelComponentId" = mc."Id"
		            inner join public."SubArea" as sa on b."SubAreaId" = sa."Id"
		            inner join public."ScenarioModelResult" as smrSA on smrSA."ScenarioId" = u."ScenarioId" and smrSA."ModelComponentId" = sa."ModelComponentId" and smrSA."ScenarioModelResultTypeId" = ue."BMPEffectivenessTypeId" + 2 and smrSA."Year" = ue."Year"
	            WHERE
		            ue."Year" = 2001
		            and
		            u."ScenarioId" = 1
		            and
		            mc."ModelId" in (1) -- BMP id
		            and
		            mc."ModelComponentTypeId" = 3  -- Isolated wetland
		            and
		            ue."BMPEffectivenessTypeId" in ( 1, 2, 3, 4, 12, 13 ) and ue."BMPEffectivenessTypeId" <= 13
	            ORDER BY
		            ue."Year"
	            ) as foo
            GROUP BY
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId"
	
            UNION

            -- *** Intelligent & Manual selection - reach BMP offsite ***
            SELECT 
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId", 
 	            'Offsite' as "BMPLocationType",
	            'Reach' as "SpatialUnit",
	            trunc(avg("PercChange"/100*"ReachValue"),6) as "ValueChange"
            FROM
	            (
	            select 
		            mc."ModelId" as "BmpId", 
		            r."ModelComponentId" as "ReachMcId", 
		            u."ModelComponentId" as "BmpMcId", 
		            ue."Year", 
		            ue."Value" as "PercChange",  
		            u."ScenarioId", 
		            ue."BMPEffectivenessTypeId",
		            smrReach."Value" as "ReachValue"
	            from 
		            public."UnitScenario" as u
		            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		            inner join public."ModelComponent" as mc on u."ModelComponentId" = mc."Id"
		            inner join public."Watershed" as w on w."Id" = mc."WatershedId"
		            inner join public."Reach" as r on w."OutletReachId" = r."Id"
		            inner join public."ScenarioModelResult" as smrReach on smrReach."ScenarioId" = u."ScenarioId" and smrReach."ModelComponentId" = r."ModelComponentId" and smrReach."ScenarioModelResultTypeId" = ue."BMPEffectivenessTypeId" + 2 and smrReach."Year" = ue."Year"		
	            WHERE
		            ue."Year" = 2001
		            and
		            u."ScenarioId" = 1
		            and
		            mc."ModelId" in (1) -- BMP id
		            and
		            mc."ModelComponentTypeId" = 3  -- Isolated wetland
		            and
		            ue."BMPEffectivenessTypeId" > 13 and ue."BMPEffectivenessTypeId" < 22
	            ORDER BY
		            ue."Year"
	            ) as foo 
            GROUP BY
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId"

            UNION

            -- *** Intelligent & Manual selection - reach BMP cost ***
            SELECT "ScenarioId", "BmpId", "BMPEffectivenessTypeId", trunc(avg("YearlyCost"),6) as "ValueChange" from
	            (select foo."BMPCombinationId", foo."ScenarioId", foo."Year", foo."BmpId", foo."BMPEffectivenessTypeId", trunc(sum("Cost"),6) as "YearlyCost" from
	            (select UET."BMPCombinationId", UET."BMPEffectivenessTypeId", UET."ScenarioId", UET."BmpId", UET."Year", UET."Cost"  from
	            (select 
			            mc."ModelId" as "BmpId", ue."Year", ue."Value" as "Cost", u."ScenarioId", ue."BMPEffectivenessTypeId", u."BMPCombinationId"
		            from 
			            public."UnitScenario" as u
			            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
			            inner join public."ModelComponent" as mc on u."ModelComponentId" = mc."Id"
		            WHERE
			            ue."Year" >= 2001 and ue."Year" <= 2001
			            and
			            mc."ModelId" in (1) -- BMP id
			            and
			            mc."ModelComponentTypeId" = 3
		 	            and
		 	            ue."BMPEffectivenessTypeId" = 22
		            order by
			            u."ModelComponentId", ue."Year") as UET	
	            ) AS foo
	            group by
		            foo."ScenarioId", foo."Year", foo."BmpId", foo."BMPEffectivenessTypeId", foo."BMPCombinationId") as foo
            group by
	            "ScenarioId", "BmpId", "BMPEffectivenessTypeId"
	
            ) AS FOO
            ORDER BY
	            "BMPEffectivenessTypeId"
         */
        [Test]
        public void Value_SoilMoisture_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 11.732550m;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_ET_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 13.372722m;
            int bmpEffId = 2; // ET
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Runoff_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -4.625800m;
            int bmpEffId = 4; // runoff
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOnsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -1.141950m;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOffsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -1.620646m;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }
                
        [Test]
        public void Value_TNOnsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -5.089128m;
            int bmpEffId = 8; // tn onsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TNOffsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -7.397001m;
            int bmpEffId = 18; // tn offsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Carbon_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 4.067270m;
            int bmpEffId = 12; // carbon
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Biodiversity_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 0.153000m;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Cost_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 200;
            int bmpEffId = 22; // Cost
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        /** SQL script to get the result of 2001_Conv_VFST or GWW in test db
         -- *** Intelligent & Manual selection - field BMP ***
            SELECT * FROM
            (
            -- *** Intelligent & Manual selection - field BMP onsite without biodiversity ***
            SELECT "ScenarioId", "ParcelId", "BMPCombinationId", 'Onsite' as "BMPLocationType", "BMPEffectivenessTypeId", trunc(avg("YearlyValueChange"),6) as "YearlyAvg" from
	            (select foo."BMPCombinationId", foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", trunc(sum("ValueChange"),6) as "YearlyValueChange" from
	            (select UET."BMPCombinationId", UET."BMPEffectivenessTypeId", SMT."ScenarioId", SMT."ModelComponentId", SMT."ScenarioModelResultTypeId", SMT."ParcelId", SMT."Year", SMT."Value", UET.PercChange, SMT."Value" * UET.PercChange / 100 AS "ValueChange" from
	            (SELECT
		            s."ModelComponentId", s."Year", s."Value", s."ScenarioId", sa."ParcelId", s."ScenarioModelResultTypeId"
	            from
		            public."ScenarioModelResult" as s
		            inner join public."SubArea" as sa on sa."ModelComponentId" = s."ModelComponentId"
	            where
		            s."Year" >= 2001 and s."Year" <= 2001
		            and
		            sa."ParcelId" in (4)
	            Order by
		            s."ModelComponentId", s."Year") as SMT
	            inner join
		            (select 
			            u."ModelComponentId", ue."Year", ue."Value" as PercChange, u."ScenarioId", ue."BMPEffectivenessTypeId", u."BMPCombinationId"
		            from 
			            public."UnitScenario" as u
			            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		            WHERE
			            u."BMPCombinationId" = 6
		 	            and
		 	            ue."BMPEffectivenessTypeId" < 13
		            order by
			            u."ModelComponentId", ue."Year") as UET 
		            on SMT."ModelComponentId" = UET."ModelComponentId" 
	 		            and SMT."Year" = UET."Year" 
	 		            and SMT."ScenarioId" = UET."ScenarioId"
			            and UET."BMPEffectivenessTypeId" = SMT."ScenarioModelResultTypeId" - 2
	            ) AS foo
	            group by
		            foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", foo."BMPCombinationId") as foo
            group by
	            "ScenarioId", "ParcelId", "BMPEffectivenessTypeId", "BMPCombinationId"
	
            UNION

            -- *** Intelligent & Manual selection - field BMP onsite biodiversity ***
            SELECT "ScenarioId", "ParcelId", "BMPCombinationId", 'Onsite' as "BMPLocationType", "BMPEffectivenessTypeId", trunc(avg("YearlyValueChange"),6) as "YearlyAvg" from
	            (select foo."BMPCombinationId", foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", trunc(sum("ValueChange" * "SAArea")/sum("SAArea"),6) as "YearlyValueChange" from
	            (select UET."BMPCombinationId", UET."BMPEffectivenessTypeId", SMT."ScenarioId", SMT."ModelComponentId", SMT."ScenarioModelResultTypeId", SMT."ParcelId", SMT."Area" as "SAArea", SMT."Year", SMT."Value", UET.PercChange, SMT."Value" * UET.PercChange / 100 AS "ValueChange" from
	            (SELECT
		            s."ModelComponentId", s."Year", s."Value", s."ScenarioId", sa."ParcelId", s."ScenarioModelResultTypeId", sa."Area"
	            from
		            public."ScenarioModelResult" as s
		            inner join public."SubArea" as sa on sa."ModelComponentId" = s."ModelComponentId"
	            where
		            s."Year" >= 2001 and s."Year" <= 2001
		            and
		            sa."ParcelId" in (4)
	            Order by
		            s."ModelComponentId", s."Year") as SMT
	            inner join
		            (select 
			            u."ModelComponentId", ue."Year", ue."Value" as PercChange, u."ScenarioId", ue."BMPEffectivenessTypeId", u."BMPCombinationId"
		            from 
			            public."UnitScenario" as u
			            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		            WHERE
			            u."BMPCombinationId" = 6
		 	            and
		 	            ue."BMPEffectivenessTypeId" = 13
		            order by
			            u."ModelComponentId", ue."Year") as UET 
		            on SMT."ModelComponentId" = UET."ModelComponentId" 
	 		            and SMT."Year" = UET."Year" 
	 		            and SMT."ScenarioId" = UET."ScenarioId"
			            and UET."BMPEffectivenessTypeId" = SMT."ScenarioModelResultTypeId" - 2
	            ) AS foo
	            group by
		            foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", foo."BMPCombinationId") as foo
            group by
	            "ScenarioId", "ParcelId", "BMPEffectivenessTypeId", "BMPCombinationId"

            UNION
	
            -- *** Intelligent & Manual selection - field BMP offsite ***
            SELECT "ScenarioId", "ParcelId", "BMPCombinationId", 'Offsite' as "BMPLocationType", "BMPEffectivenessTypeId", trunc(avg("YearlyValueChange"),6) as "YearlyAvg" from
	            (select foo."BMPCombinationId", foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", trunc(sum("ValueChange"),6) as "YearlyValueChange" from
	            (select UET."BMPCombinationId", UET."BMPEffectivenessTypeId", SMT."ScenarioId", SMT."ModelComponentId", SMT."ScenarioModelResultTypeId", UET."ParcelId", SMT."Year", SMT."Value", UET.PercChange, SMT."Value" * UET.PercChange / 100 AS "ValueChange" from
	            (SELECT
		            s."ModelComponentId", s."Year", s."Value", s."ScenarioId", s."ScenarioModelResultTypeId"
	            from
		            public."ScenarioModelResult" as s
	            where
		            s."Year" >= 2001 and s."Year" <= 2001
	            Order by
		            s."ModelComponentId", s."Year") as SMT
	            inner join
		            (select 
			            r."ModelComponentId" as "OutletReachMcId", u."ModelComponentId", sa."ParcelId", ue."Year", ue."Value" as PercChange, u."ScenarioId", ue."BMPEffectivenessTypeId", u."BMPCombinationId"
		            from 
			            public."UnitScenario" as u
			            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		 	            inner join public."SubArea" as sa on sa."ModelComponentId" = u."ModelComponentId"
			            inner join public."ModelComponent" as mc on mc."Id" = u."ModelComponentId"
			            inner join public."Watershed" as w on w."Id" = mc."WatershedId"
			            inner join public."Reach" as r on r."Id" = w."OutletReachId"
		            WHERE
			            sa."ParcelId" in (4)
		 	            and
			            u."BMPCombinationId" = 6
		 	            and
		 	            ue."BMPEffectivenessTypeId" > 13
		            order by
			            u."ModelComponentId", ue."Year") as UET 
		            on SMT."ModelComponentId" = UET."OutletReachMcId" 
	 		            and SMT."Year" = UET."Year" 
	 		            and SMT."ScenarioId" = UET."ScenarioId"
			            and UET."BMPEffectivenessTypeId" = SMT."ScenarioModelResultTypeId" - 2
	            ) AS foo
	            group by
		            foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", foo."BMPCombinationId") as foo
            group by
	            "ScenarioId", "ParcelId", "BMPEffectivenessTypeId", "BMPCombinationId"

            UNION

            -- *** Intelligent & Manual selection - field BMP cost ***
            SELECT "ScenarioId", "ParcelId", "BMPCombinationId", 'Offsite' as "BMPLocationType", "BMPEffectivenessTypeId", trunc(avg("YearlyCost"),6) as "YearlyAvgCost" from
	            (select foo."BMPCombinationId", foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", trunc(sum("Cost"),6) as "YearlyCost" from
	            (select UET."BMPCombinationId", UET."BMPEffectivenessTypeId", UET."ScenarioId", UET."ParcelId", UET."Year", UET."Cost"  from
	            (select 
			            sa."ParcelId", ue."Year", ue."Value" as "Cost", u."ScenarioId", ue."BMPEffectivenessTypeId", u."BMPCombinationId"
		            from 
			            public."UnitScenario" as u
			            inner join public."UnitScenarioEffectiveness" as ue on u."Id" = ue."UnitScenarioId"
		 	            inner join public."SubArea" as sa on sa."ModelComponentId" = u."ModelComponentId"
		            WHERE
			            ue."Year" >= 2001 and ue."Year" <= 2001
			            and
			            sa."ParcelId" in (4)
		 	            and
			            u."BMPCombinationId" = 6
		 	            and
		 	            ue."BMPEffectivenessTypeId" = 22
		            order by
			            u."ModelComponentId", ue."Year") as UET	
	            ) AS foo
	            group by
		            foo."ScenarioId", foo."Year", foo."ParcelId", foo."BMPEffectivenessTypeId", foo."BMPCombinationId") as foo
            group by
	            "ScenarioId", "ParcelId", "BMPEffectivenessTypeId", "BMPCombinationId"

            ) AS FOO
            ORDER BY
            "ScenarioId", "ParcelId", "BMPCombinationId", "BMPEffectivenessTypeId", "BMPLocationType"
         */
        [Test]
        public void Value_SoilMoisture_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 10.098753m;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_ET_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 20.114009m;
            int bmpEffId = 2; // ET
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Runoff_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -11.759600m;
            int bmpEffId = 4; // runoff
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOnsite_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -0.577857m;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOffsite_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -0.874862m;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Carbon_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 13.865320m;
            int bmpEffId = 12; // carbon
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Biodiversity_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 0.084701m;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Cost_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 133.750000m;
            int bmpEffId = 22; // Cost
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_SoilMoisture_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 9.335117m;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_ET_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 17.652508m;
            int bmpEffId = 2; // ET
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Runoff_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -11.805180m;
            int bmpEffId = 4; // runoff
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOnsite_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -0.775565m;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_TPOffsite_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = -1.190386m;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Carbon_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 16.607907m;
            int bmpEffId = 12; // carbon
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Biodiversity_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 0.048962m;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }

        [Test]
        public void Value_Cost_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            decimal exp = 176.290000m;
            int bmpEffId = 22; // Cost
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.Zero((int)(1e6m * (exp -
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.Find(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Value)));
        }
    }
}
