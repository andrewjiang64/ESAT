using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.Project
{
    public class UnitSolution
    {
        public int BMPTypeId { get; set; }
        public OptimizationSolutionLocationTypeEnum LocationType { get; set; }
        public int LocationId { get; set; }
    }
}
