using NUnit.Framework;
using ESAT.Import.Utils;
using AgBMPTool.DBModel.Model.Scenario;
using ESAT.Import.Model;
using System.Collections.Generic;

namespace ESAT.Import.Test.AggregationTest
{
    public class BMPEff_Count_UnitOptimizationSolutionBuilder
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

        [Test]
        public void Count_SoilMoisture_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_ET_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 2; // ET
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Runoff_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 4; // runoff
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOnsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOffsite_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Carbon_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 12; // carbon
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Biodiversity_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Cost_2001_Conv_ISWET_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 22; // Cost
            int bmpCombId = 1; // Isolated wetland
            int year = 2001;
            int scenId = 1;
            int locationId = 36;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }


        [Test]
        public void Count_SoilMoisture_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_ET_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 2; // ET
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Runoff_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 4; // runoff
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOnsite_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOffsite_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Carbon_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 12; // carbon
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Biodiversity_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Cost_2001_Conv_VFST_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 22; // Cost
            int bmpCombId = 4; // VFST
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_SoilMoisture_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 1; // soil moisture
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_ET_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 2; // ET
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Runoff_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 4; // runoff
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOnsite_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 11; // tp onsite
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_TPOffsite_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 21; // tp offsite
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Carbon_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 12; // carbon
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Biodiversity_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 13; // biodiversity
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }

        [Test]
        public void Count_Cost_2001_Conv_GWW_Parcel_GetUnitOptimizationSolutionsByWatershedIdParallel()
        {
            // Arrange

            // Act
            int exp = 1;
            int bmpEffId = 22; // Cost
            int bmpCombId = 6; // GWW
            int year = 2001;
            int scenId = 1;
            int locationId = 4;
            int locationTypeId = 2;

            // Assert
            Assert.AreEqual(exp,
                UOS.Find(x => x.LocationId == locationId && x.OptimizationSolutionLocationTypeId == locationTypeId && x.ScenarioId == scenId && x.BMPCombinationId == bmpCombId)
                    .UnitOptimizationSolutionEffectivenesses.FindAll(x => x.Year == year && x.BMPEffectivenessTypeId == bmpEffId).Count);
        }
    }
}
