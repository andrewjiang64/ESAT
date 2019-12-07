using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Test.Application
{
    [TestClass]
    public class WatershedExistingBMPTypeFactoryTest
    {
        [TestMethod]

        public void BuildWatershedExistingBMPTypeDTOsTest()
        {
            // Assign
            int watershedId = 1; // Existing_LSD_MI48H_2001_2010_Watershed Manager
            int investorId = 4;
            int bmpTypeId = 18;

            int expect = 2;

            //// Act
            var res1 = Services.WatershedExistingBMPTypeFactory.BuildWatershedExistingBMPTypeDTOs(watershedId, 2, investorId);

            //// Assert
            Assert.AreEqual(expect, res1.FindAll(x => x.BMPTypeId == bmpTypeId).Count);
        }

    }
}
