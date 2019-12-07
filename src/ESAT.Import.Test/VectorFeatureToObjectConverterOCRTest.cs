using ESAT.Import.Utils.Converter;
using ESAT.Import.VectorObjectConvertor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace ESAT.Import.Test.Application
{
    [TestClass]
    public class VectorFeatureToObjectConverterOCRTest
    {
        [TestMethod]
        public void ToOCRObjects()
        {
            // Arrange
            var mock = new Mock<VectorFile>();
            mock.Setup(m => m.GetMockVectorFile()).Returns(() =>
                new VectorFile
                {
                    Path = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190911_import_shape\Watershed.shp",
                    Driver = "ESRI Shapefile",
                    ProjectionCode = 3700
                });

            VectorFile v = mock.Object.GetMockVectorFile();
            string expectResult = "Sample watershed";


            // Act
            Dictionary<int, Dictionary<string, string>> objs = new VectorFeatureToObjectConverterOCR().ToOCRObjects(v);

            // Assert
            Assert.AreEqual(expectResult, objs[1]["name"]);
        }
    }
}
