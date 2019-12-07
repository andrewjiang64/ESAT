using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
    public class OptimizationWeightDTO
    {
        /// <summary>
        /// BMP Effectiveness Type Id
        /// </summary>
        public int BMPEffectivenessTypeId { get; set; }

        /// <summary>
        /// The target for the constraint
        /// </summary>
        public int Weight { get; set; }
    }
}
