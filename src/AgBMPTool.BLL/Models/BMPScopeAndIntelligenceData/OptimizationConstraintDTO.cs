using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
    public class OptimizationConstraintDTO
    {
        /// <summary>
        /// BMP Effectiveness Type Id
        /// </summary>
        public int BMPEffectivenessTypeId { get; set; }

        /// <summary>
        /// Optimization Constraint Value Type Id
        /// </summary>
        public int OptimizationConstraintValueTypeId { get; set; }
        
        /// <summary>
        /// The target for the constraint
        /// </summary>
        public decimal Constraint { get; set; }
    }
}
