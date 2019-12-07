using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Overview
{
    public class LsdParcelBMPSummaryDTO
    {
        public int ProjectSpatialUnitTypeId { get; set; }
        public int LsdOrParcelId { get; set; }
        public int FarmId { get; set; }
        public string LsdOrParcelBmp { get; set; }
        public string StructuralBmp { get; set; }
        public decimal Cost { get; set; }
    }
}
