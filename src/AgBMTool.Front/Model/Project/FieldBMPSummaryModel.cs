using AgBMPTool.BLL.Models.Overview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Project
{
    public class FieldBMPSummaryModel
    {
        public string ProjectSpatialUnitType { get; set; }

        public List<LsdParcelBMPSummaryDTO> Summary { get; set; }
    }
}
