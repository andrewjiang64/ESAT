using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ESAT.Import.Utils;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using ESAT.Import.Utils.TextFile;

namespace ESAT.Import.Test.Utilities.Converter
{
    [TestClass]
    public class ScenarioResultToObjectConverterTest
    {
        [TestMethod]
        public void BuildScenarioModelResult()
        {
            // Arrange
            int scenarioId = 1;
            int modelComponentId = 2;
            int scenarioModelResultTypeId = 23; // TP reach loading
            int year = 2001;
            decimal value = 10;

            int expectId = 0; // error correct = 0

            // Act
            ScenarioModelResult smr = new ScenarioResultToObjectConverter()
                .BuildScenarioModelResult(scenarioId, modelComponentId, scenarioModelResultTypeId, year, value);

            // Assert
            Assert.AreEqual(smr.Value, value);
            Assert.AreEqual(smr.ModelComponentId, modelComponentId);
            Assert.AreEqual(smr.ScenarioId, scenarioId);
            Assert.AreEqual(smr.ScenarioModelResultTypeId, scenarioModelResultTypeId);
            Assert.AreEqual(smr.Year, year);
            Assert.AreEqual(smr.Id, expectId);
        }

        [TestMethod]
        public void RecordToObjects()
        {
            // Assign
            Dictionary<string, object> record = new Dictionary<string, object>()
            {
                ["Scenario"] = "Conventional",
                ["Watershed"] = "Sample watershed",
                ["SubArea"] = 1,
                ["Year"] = 1,
                ["TN Yield"] = -999,
                ["TP Yield"] = 4.14,
            };
            int expectCount = 1; // error correct = 1
            int expectModelComponentId = 7;
            int expectScenarioId = 1;
            int expectSmrTypeId = 13; // error correct = 13
            decimal expectValue = (decimal)4.14;


            // Act
            List<object> output = new ScenarioResultToObjectConverter().RecordToObjects(record);
            ScenarioModelResult smr = (ScenarioModelResult)output[0];

            // Assert
            Assert.AreEqual(expectCount, output.Count);
            Assert.AreEqual(expectModelComponentId, smr.ModelComponentId);
            Assert.AreEqual(expectScenarioId, smr.ScenarioId);
            Assert.AreEqual(expectSmrTypeId, smr.ScenarioModelResultTypeId);
            Assert.AreEqual(expectValue, smr.Value);
        }

        [TestMethod]
        public void TableToObjects_SubArea()
        {// Arrange
            string csvFile = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\ScenarioSubAreaResults.csv";

            int expectObjectSize = 8700; // correct = 8700
            decimal expectSum = (decimal)891055.59; // correct = 891055.59

            // Act
            List<object> output = new ScenarioResultToObjectConverter().TableToObjects(new TextTableReader().CsvToTable(csvFile));
            decimal sum = 0;
            foreach (var item in output)
            {
                sum += ((ScenarioModelResult)item).Value;
            }

            // Assert
            Assert.AreEqual(expectObjectSize, output.Count);
            Assert.AreEqual(expectSum, sum);
        }

        [TestMethod]
        public void TableToObjects_Reach()
        {// Arrange
            string csvFile = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\ScenarioReachResults.csv";

            int expectObjectSize = 960; // correct = 960
            decimal expectSum = (decimal)70909.85; // correct = 70909.85

            // Act
            List<object> output = new ScenarioResultToObjectConverter().TableToObjects(new TextTableReader().CsvToTable(csvFile));
            decimal sum = 0;
            foreach (var item in output)
            {
                sum += ((ScenarioModelResult)item).Value;
            }

            // Assert
            Assert.AreEqual(expectObjectSize, output.Count);
            Assert.AreEqual(expectSum, sum);
        }
    }
}
