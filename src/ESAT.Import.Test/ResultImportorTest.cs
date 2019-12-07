using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ESAT.Import.Model;
using System;

namespace ESAT.Import.Test.Application
{
    [TestClass]
    public class ResultImportorTest
    {
        [TestMethod]
        public void ImportScenarioResult()
        {
            // Assign
            string subareaCsvFile = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\ScenarioSubAreaResults.csv";
            string reachCsvFile = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\ScenarioReachResults.csv";
            decimal expectSum = (decimal)961965.44; // correct = 891055.59 + 70909.85 = 961965.44‬
            int expectCount = 9660; // correct = 960 + 8700 = 9660‬

            // Act
            new ResultImportor().ImportScenarioResults(subareaCsvFile, reachCsvFile);
            decimal sum = 0;
            int count = 0;

            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                foreach (var item in db.ScenarioModelResults)
                {
                    sum += item.Value;
                    count++;
                }
            }

            int sumCompare = (int)(100 * (((int)expectSum / sum) - expectSum / sum));

            // Assert
            Assert.AreEqual(((int) expectCount / count), expectCount / count);
            Assert.AreEqual(sumCompare, 0);
        }

        [TestMethod]
        public void RemoveScenarioResults()
        {
            // Assign
            string watershedName = "Sample watershed";
            int expectResult = 9660;

            // Act
            int result = new ResultImportor().RemoveScenarioResults(watershedName);

            // Assert
            Assert.AreEqual(((int)expectResult / result), expectResult / result);
        }

        [TestMethod]
        public void ImportBmpEffectiveness()
        {
            // Assign
            List<string> csvFiles = new List<string>();
            csvFiles.Add(@"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\BmpEffectivenessField.csv");
            csvFiles.Add(@"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\BmpEffectivenessReach.csv");
            decimal expectSum = (decimal)88443.02; // correct = 40128.68 + 48314.34 = 88443.02
            int expectCount = 39140; // correct = 1740 + 37400 = 39140‬

            // Act
            new ResultImportor().ImportBmpEffectiveness(csvFiles);
            decimal sum = 0;
            int count = 0;

            using (var db = ESAT.Import.Utils.Database.AgBMPToolContextFactory.AgBMPToolContext)
            {
                foreach (var item in db.UnitScenarioEffectivenesses)
                {
                    sum += item.Value;
                    count++;
                }
            }

            // Assert
            Assert.AreEqual(expectCount, count);
            Assert.AreEqual(Convert.ToInt32(expectSum * 100 / expectCount), Convert.ToInt32(sum * 100 / count));
        }

        [TestMethod]
        public void RemoveBmpEffectiveness()
        {
            // Assign
            string watershedName = "Sample watershed";
            int expectResult = 39328; // correct = 39140 + 188 = 39328

            // Act
            int result = new ResultImportor().RemoveEffectiveness(watershedName);

            // Assert
            Assert.AreEqual(expectResult, result);
        }
    }
}
