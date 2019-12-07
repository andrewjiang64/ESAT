using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ESAT.Import.Utils;
using AgBMPTool.DBModel.Model.Scenario;

namespace ESAT.Import.Test.Utilities.Converter
{
    [TestClass]
    public class EffectivenessTableToObjectConverterTest
    {
        [TestMethod]
        public void BuildUnitScenarioEffectiveness()
        {
            // Arrange
            int unitScenarioId = 1;
            int bmpEffectivenessTypeId = 21; // tp offsite
            int year = 1;
            decimal value = 10;

            int expectId = 0; // error correct = 0

            // Act
            UnitScenarioEffectiveness unitEff = new EffectivenessTableToObjectConverter()
                .BuildUnitScenarioEffectiveness(unitScenarioId, bmpEffectivenessTypeId, year, value);

            // Assert
            Assert.AreEqual(unitEff.Value, value);
            Assert.AreEqual(unitEff.UnitScenarioId, unitScenarioId);
            Assert.AreEqual(unitEff.BMPEffectivenessTypeId, bmpEffectivenessTypeId);
            Assert.AreEqual(unitEff.Year, year);
            Assert.AreEqual(unitEff.Id, expectId);
        }

        [TestMethod]
        public void RecordToObjects()
        {
            Dictionary<string, object> record = new Dictionary<string, object>()
            {
                ["Scenario"] = "Conventional",
                ["Watershed"] = "Sample watershed",
                ["LocationType"] = "SubArea",
                ["BMPType"] = "GWW_MI48H",
                ["SubArea"] = 29,
                ["Year"] = 1,
                ["Soil carbon onsite"] = 3.57,
                ["TP offsite"] = -0.45643,
            };
            int expectCount = 2; // error correct = 1
            int expectBETypeId = 21; // error correct = 21
            decimal expectValue = (decimal)-0.45643;


            // Act
            List<object> output = new EffectivenessTableToObjectConverter().RecordToObjects(record);
            UnitScenarioEffectiveness uses = (UnitScenarioEffectiveness)output[0];

            // Assert
            Assert.AreEqual(expectCount, output.Count);
            Assert.AreEqual(expectBETypeId, uses.BMPEffectivenessTypeId);
            Assert.AreEqual(expectValue, uses.Value);
        }
    }
}
