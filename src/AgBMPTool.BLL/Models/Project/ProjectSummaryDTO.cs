using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class ProjectSummaryDTO : ProjectDTO
    {
        public List<BMPSummaryDTO> BMPSummaryDTOs { get; set; } = new List<BMPSummaryDTO>();
        public List<EffectivenessSummaryDTO> EffectivenessSummaryDTOs { get; set; } = new List<EffectivenessSummaryDTO>();
        public decimal Cost { get; set; } = 0;
    }
}
