using Microsoft.VisualStudio.TestTools.UnitTesting;
using ESAT.Import.Utils;
using AgBMPTool.DBModel.Model.Scenario;

namespace ESAT.Import.Test.EsatDb
{
    [TestClass]
    public class EsatDbWriterTest
    {
        [TestMethod]
        public void ExecuteNonQuery()
        {
            // Arrange
            string tableName = "Reach";
            string sql = $"UPDATE public.\"{tableName}\" SET \"Id\" = \"Id\" * 1;";
            int expectResult = 6; // error correct = 6

            // Act
            int resultString = new EsatDbWriter().ExecuteNonQuery(sql);
            int resultSb = new EsatDbWriter().ExecuteNonQuery(new System.Text.StringBuilder().AppendLine(sql).ToString());

            // Assert
            Assert.AreEqual(resultString, expectResult);
            Assert.AreEqual(resultSb, expectResult);
        }

        [TestMethod]
        public void AddBaselineScenario()
        {
            // Tested in GetBaselineScenarioId

            // Assign
            int watershedId = 1;
            int scenTypeId = 1;

            int expectScenId = 1; // correct = 1

            // Act
            int scenId = new EsatDbWriter().AddBaselineScenario(watershedId, scenTypeId);

            // Assert
            Assert.AreEqual(expectScenId, scenId);
        }

        [TestMethod]
        public void AddUnitScenario()
        {
            // Arrange
            string type = "GWW_MI48H";
            int scenTypeId = 1;
            int watershedId = 1;
            int subAreaId = 29;

            int expectComboId = 41; // correct = 41
            int expectScenId = 1; // correct = 1
            int expectMCId = 35; // correct = 35

            // Act
            EsatDbRetriever r = new EsatDbRetriever();
            EsatDbWriter w = new EsatDbWriter();
            int mcTypeId = r.GetModelComponentTypeId("SubArea");
            int mcId = r.GetModelComponentId(watershedId, mcTypeId, subAreaId);
            int id = w.AddUnitScenario(mcId, r.GetBaselineScenarioId(watershedId, scenTypeId), r.GetBmpCombinationId(r.GetBmpIdsFromText(type)));

            // Assert
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                UnitScenario us = db.UnitScenarios.Find(id);
                Assert.AreEqual(expectComboId, us.BMPCombinationId);
                Assert.AreEqual(expectScenId, us.ScenarioId);
                Assert.AreEqual(expectMCId, us.ModelComponentId);
            }
        }
    }
}
