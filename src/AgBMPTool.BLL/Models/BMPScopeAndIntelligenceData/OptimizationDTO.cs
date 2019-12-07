using AgBMPTool.BLL.Models.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
   public class OptimizationDTO
    {
        public int Id { get; set; }

        public int projectId { get; set; }

        public int optimizationTypeId { get; set; }

        /// <summary>
        /// The budget target for budget mode
        /// </summary>
        public decimal? budgetTarget { get; set; }

        public List<BMPEffectivenessTypeDTO> bmpEffectivenessTypes { get; set; }
        public List<BMPEffectivenessTypeDTO> addedOptimizationConstraint { get; set; }
    }
}
