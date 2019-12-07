using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ESAT.Import.Utils;
using System;
using AgBMPTool.DBModel.Model.ModelComponent;
using AgBMPTool.DBModel.Model.Scenario;

namespace ESAT.Import.Test.EsatDb
{
    [TestClass]
    public class EsatDbRetriverTest
    {
        [TestMethod]
        public void ContainsTable()
        {
            // Arrange
            string schemaName = "public";
            string tableName = "Watershed";

            bool expectedResult = true;

            // Act
            bool result = new EsatDbRetriever().ContainsTable(schemaName, tableName);

            // Assert
            Assert.AreEqual(result, expectedResult);
        }

        [TestMethod]
        public void GetModelComponentTypeId()
        {
            // Arrange
            Type t = new SubArea().GetType();
            int expectedResult = 1;

            // Act
            int id = new EsatDbRetriever().GetModelComponentTypeId(t.Name);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetWatershedId()
        {
            // Arrange
            string name = "Sample watershed";
            int expectedResult = 1;

            // Act
            int id = new EsatDbRetriever().GetWatershedId(name);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetScenarioTypeId()
        {
            // Arrange
            string type = "Existing";
            int expectedResult = 2;

            // Act
            int id = new EsatDbRetriever().GetScenarioTypeId(type);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetUnitTypeId()
        {
            // Arrange
            string npName = "N/P Yield";
            string tssName = "TSS Yield";
            string tempName = "Temperature";
            string scName = "Soil carbon"; // error
            string unitless = "Unitless";
            int expectedNpResult = 10;
            int expectedTssResult = 9;
            int expectedTempResult = 4;
            int expectedScResult = 14;
            int expectedUlResult = 15;

            // Act
            int npId = new EsatDbRetriever().GetUnitTypeId(npName);
            int tssId = new EsatDbRetriever().GetUnitTypeId(tssName);
            int tempId = new EsatDbRetriever().GetUnitTypeId(tempName);
            int scId = new EsatDbRetriever().GetUnitTypeId(scName);
            int ulId = new EsatDbRetriever().GetUnitTypeId(unitless);

            // Assert
            Assert.AreEqual(npId, expectedNpResult);
            Assert.AreEqual(tssId, expectedTssResult);
            Assert.AreEqual(tempId, expectedTempResult);
            Assert.AreEqual(scId, expectedScResult);
            Assert.AreEqual(ulId, expectedUlResult);
        }

        [TestMethod]
        public void GetModelComponentId()
        {
            // Arrange
            int watershedId = 1;
            int modelComponentTypeId = 1;
            int modelId = 1;
            int expectedResult = 7; // correct = 7

            // Act
            int id = new EsatDbRetriever().GetModelComponentId(watershedId, modelComponentTypeId, modelId);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetScenarioModelResultVariableTypeId()
        {
            // Arrange
            string tp = "TP";
            string sc = "Soil carbon";
            string pcp = "Precipitation";
            string sm = "Soil moisture";
            int expectedTpId = 13;
            int expectedScId = 14;
            int expectedPcpId = 1;
            int expectedSmId = 3; // error correct = 3

            // Act
            int tpId = new EsatDbRetriever().GetScenarioModelResultVariableTypeId(tp);
            int scId = new EsatDbRetriever().GetScenarioModelResultVariableTypeId(sc);
            int pcpId = new EsatDbRetriever().GetScenarioModelResultVariableTypeId(pcp);
            int smId = new EsatDbRetriever().GetScenarioModelResultVariableTypeId(sm);

            // Assert
            Assert.AreEqual(tpId, expectedTpId);
            Assert.AreEqual(scId, expectedScId);
            Assert.AreEqual(pcpId, expectedPcpId);
            Assert.AreEqual(smId, expectedSmId);
        }

        [TestMethod]
        public void GetScenarioModelResultTypeId()
        {
            // Arrange
            int utId = 10;
            int mctId = 2;
            int smrvTypeId = 13;
            int expectedResult = 23; // error TP reach loading, correct = 23

            // Act
            int id = new EsatDbRetriever().GetScenarioModelResultTypeId(utId, mctId, smrvTypeId);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetScenarioModelResultTypeId_UseName()
        {
            // Arrange
            string name = "TP reach loading";
            int expectedResult = 23; // error TP reach loading, correct = 23

            // Act
            int id = new EsatDbRetriever().GetScenarioModelResultTypeId(name);

            // Assert
            Assert.AreEqual(id, expectedResult);
        }

        [TestMethod]
        public void GetBaselineScenarioId()
        {
            // Arrange
            int watershedId = 1;
            int scenarioTypeId1 = 1;
            int scenarioTypeId2 = 2;
            int expectedResult1 = 1; // error correct = 1
            int expectedResult2 = 2; // error correct = 2

            // Act
            int id1 = new EsatDbRetriever().GetBaselineScenarioId(watershedId, scenarioTypeId1);
            int id2 = new EsatDbRetriever().GetBaselineScenarioId(watershedId, scenarioTypeId2);

            // Assert
            Assert.AreEqual(id1, expectedResult1);
            Assert.AreEqual(id2, expectedResult2);
        }

        [TestMethod]
        public void GetUnitScenarioId()
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
            int id = r.GetUnitScenarioId(mcId, r.GetBaselineScenarioId(watershedId, scenTypeId), r.GetBmpCombinationId(r.GetBmpIdsFromText(type)));

            // Assert
            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                UnitScenario us = db.UnitScenarios.Find(id);
                Assert.AreEqual(expectComboId, us.BMPCombinationId);
                Assert.AreEqual(expectScenId, us.ScenarioId);
                Assert.AreEqual(expectMCId, us.ModelComponentId);
            }

        }

        [TestMethod]
        public void GetBMPComboId()
        {
            // Arrange
            string type = "GWW_MI48H";
            int expectId = 41; // correct = 41

            // Act
            int id = new EsatDbRetriever().GetBmpCombinationId(new EsatDbRetriever().GetBmpIdsFromText(type));

            // Assert
            Assert.AreEqual(expectId, id);
        }

        [TestMethod]
        public void GetBmpIdsFromText()
        {
            // Arrange
            string type = "GWW_MI48H";
            int expectSumId = 24; // correct = 6 + 18 = 24

            // Act
            List<int> ids = new EsatDbRetriever().GetBmpIdsFromText(type);
            int sumId = 0;
            foreach (int id in ids)
            {
                sumId += id;
            }

            // Assert
            Assert.AreEqual(expectSumId, sumId);
        }


        [TestMethod]
        public void GetBmpEffectivenessTypeId()
        {
            // Arrange
            string type = "TP offsite";
            int expectId = 21; // correct = 21

            // Act
            int id = new EsatDbRetriever().GetBmpEffectivenessTypeId(type);

            // Assert
            Assert.AreEqual(expectId, id);

        }

        [TestMethod]
        public void GetBMPTypeId()
        {
            // Arrange
            string bmpType = "MI48H";
            int expectId = 18; // correct = 18

            // Act
            int id = new EsatDbRetriever().GetBMPTypeId(bmpType);

            // Assert
            Assert.AreEqual(expectId, id);
        }
    }
}
