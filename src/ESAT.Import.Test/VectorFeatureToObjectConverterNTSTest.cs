using ESAT.Import.VectorObjectConvertor.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using VectorFeatureToObjectConverter;

namespace ESAT.Import.Test.Application
{
    [TestClass]
    public class VectorFeatureToObjectConverterNTSTest
    {
        [TestMethod]
        public void ToNTSObjects()
        {
            // Arrange
            var mock = new Mock<VectorFile>();
            mock.Setup(m => m.GetMockVectorFile()).Returns(() =>
                new VectorFile
                {
                    Path = @"C:\Users\Shawn\OneDrive - University of Guelph\Documents\Geography\Research\Projects\ESAT_dev\03.project_on_going\190911_import_shape\Watershed.shp",
                    Driver = "ESRI Shapefile",
                    ProjectionWKT = "PROJCS[\"NAD_1983_Transverse_Mercator\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137.0,298.257222101]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Transverse_Mercator\"],PARAMETER[\"False_Easting\",500000.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",-115.0],PARAMETER[\"Scale_Factor\",0.9992],PARAMETER[\"Latitude_Of_Origin\",0.0],UNIT[\"Meter\",1.0]]",
                    ProjectionCode = 3700
                });

            VectorFile v = mock.Object.GetMockVectorFile();
            string expectResult = "Sample watershed";


            // Act
            Dictionary<int, VectorObject> objs = new VectorFeatureToObjectConverterNTS().ToNTSObjects(v);

            // Assert
            Assert.AreEqual(expectResult, objs[1].Attributes["name"]);
        }
    }
}
