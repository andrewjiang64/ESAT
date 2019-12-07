using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class ProjectsCostEffectivenessChartDTO
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        public double BMPCost { get; set; }

        public int EffectivenessTypeId { get; set; }

        public double Effectiveness { get; set; }

    }
}
