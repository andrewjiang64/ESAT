using AgBMPTool.BLL.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Project
{
    public class ProjectsCostEffectivenessChartModel
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        public double BMPCost { get; set; }

        public int EffectivenessTypeId { get; set; }

        public double Effectiveness { get; set; }

        public ProjectsCostEffectivenessChartModel(ProjectsCostEffectivenessChartDTO dto)
        {
            this.ProjectId = dto.ProjectId;
            this.ProjectName = dto.ProjectName;
            this.Description = dto.Description;
            this.BMPCost = dto.BMPCost;
            this.EffectivenessTypeId = dto.EffectivenessTypeId;
            this.Effectiveness = dto.Effectiveness;
        }

    }
}
