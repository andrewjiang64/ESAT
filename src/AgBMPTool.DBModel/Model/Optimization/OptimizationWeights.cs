using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Optimization
{
    public class OptimizationWeights : BaseItem
    {
        [ForeignKey(nameof(Optimization))]
        public int OptimizationId { get; set; }

        public Optimization Optimization { get; set; }

        [ForeignKey(nameof(BMPEffectivenessType))]
        public int BMPEffectivenessTypeId { get; set; }

        public Scenario.BMPEffectivenessType BMPEffectivenessType { get; set; }


        /// <summary>
        /// The target for the constraint
        /// </summary>
        public int Weight { get; set; }
    }
}
