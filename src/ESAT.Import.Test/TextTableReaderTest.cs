using Microsoft.VisualStudio.TestTools.UnitTesting;
using ESAT.Import.Utils;
using System;
using ESAT.Import.Utils.TextFile;

namespace ESAT.Import.Test.Utilities.ListTableTest
{
    [TestClass]
    public class TextTableReaderTest
    {
        [TestMethod]
        public void CsvToTable()
        {
            // Arrange
            string csvFile = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190913_Test_DB_results\BmpEffectiveness.csv";

            int expectHeaderSize = 26;
            int expectRecordSize = 1800;
            double expectValue = -0.36; // TP offsite (%) record #8 (row 10)
            string expectName = "BmpEffectiveness.csv";

            // Act
            ListTable table = new TextTableReader().CsvToTable(csvFile);

            // Assert
            Assert.AreEqual(expectName, table.Name);
            Assert.AreEqual(expectHeaderSize, table.Headers.Count);
            Assert.AreEqual(expectRecordSize, table.Size);
            Assert.AreEqual(expectValue, Convert.ToDouble(table.GetValue(8, "TP offsite (%)")));
        }
    }
}
