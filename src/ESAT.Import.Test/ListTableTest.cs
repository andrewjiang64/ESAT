using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ESAT.Import.Utils;

namespace ESAT.Import.Test.Utilities.ListTableTest
{
    [TestClass]
    public class ListTableTest
    {
        [TestMethod]
        public void Size()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record = new List<object>() { 1, 2, "text" };

            int expectResult = 1; // error correct = 1

            // Act
            table.AddRecord(record);

            // Assert
            Assert.AreEqual(expectResult, table.Size);
        }

        [TestMethod]
        public void Headers()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            List<object> record = new List<object>() { 1, 2, "text" };

            string expectResult = "header2"; // error correct = header2

            // Act
            ListTable table = new ListTable(name, headers);

            // Assert
            Assert.AreEqual(expectResult, table.Headers[1]);
        }

        [TestMethod]
        public void AddRecord()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = "number"; // error correct = "number"

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);

            // Assert
            Assert.AreEqual(expectResult, table.GetRecord(1)["header3"]);
        }

        [TestMethod]
        public void DeleteRecord()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = "text"; // error correct = "text"
            int expectSize = 3; // corect = 3

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);
            table.AddRecord(record1);
            table.AddRecord(record2);
            table.DeleteRecord(1);

            // Assert
            Assert.AreEqual(expectSize, table.Size);
            Assert.AreEqual(expectResult, table.GetRecord(1)["header3"]);
        }

        [TestMethod]
        public void GetRecord()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = "number"; // error correct = "number"

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);

            // Assert
            Assert.AreEqual(expectResult, table.GetRecord(1)["header3"]);
        }

        [TestMethod]
        public void GetValue()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = "number"; // error correct = "number"

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);

            // Assert
            Assert.AreEqual(expectResult, table.GetValue(1, "header3"));
        }

        [TestMethod]
        public void SetValue()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = "text"; // error correct = "text"

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);
            table.SetValue(1, "header3", "text");

            // Assert
            Assert.AreEqual(expectResult, table.GetValue(1, "header3"));
        }

        [TestMethod]
        public void ValidateHeaders()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<string> testHeader = new List<string> { "header1", "header2", "error" };

            bool expectResult = false; // error correct = false

            // Act
            bool result = table.ValidateHeaders(testHeader);

            // Assert
            Assert.AreEqual(expectResult, result);
        }

        [TestMethod]
        public void ValidateRecordNum()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            int testNum1 = -1;
            int testNum2 = 3;
            int testNum3 = 0;

            bool expectResult1 = false;
            bool expectResult2 = false;
            bool expectResult3 = true;

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);
            table.AddRecord(record1);
            table.AddRecord(record2);
            table.DeleteRecord(1);

            bool result1 = table.ValidateRecordNum(testNum1);
            bool result2 = table.ValidateRecordNum(testNum2);
            bool result3 = table.ValidateRecordNum(testNum3);

            // Assert
            Assert.AreEqual(expectResult1, result1);
            Assert.AreEqual(expectResult2, result2);
            Assert.AreEqual(expectResult3, result3);
        }

        [TestMethod]
        public void ValidateColumnCount()
        {
            // Arrange
            string name = "TestTable";
            List<string> headers = new List<string>() { "header1", "header2", "header3" };
            ListTable table = new ListTable(name, headers);
            List<object> record1 = new List<object>() { 1, 2, "text" };
            Dictionary<string, object> record2 = new Dictionary<string, object>()
            {
                ["header1"] = 3,
                ["header3"] = "number"
            };

            string expectResult = null; // error correct = null

            // Act
            table.AddRecord(record1);
            table.AddRecord(record2);
            string result = table.ValidateColumnCount();

            // Assert
            Assert.AreEqual(expectResult, result);
        }
    }
}
