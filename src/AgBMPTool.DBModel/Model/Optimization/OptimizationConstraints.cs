using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Optimization
{
    public class OptimizationConstraints : BaseItem
    {
        [ForeignKey(nameof(Optimization))]
        public int OptimizationId { get; set; }

        public Optimization Optimization { get; set; }

        [ForeignKey(nameof(BMPEffectivenessType))]
        public int BMPEffectivenessTypeId { get; set; }

        public Scenario.BMPEffectivenessType BMPEffectivenessType { get; set; }

        [ForeignKey(nameof(OptimizationConstraintValueType))]
        public int OptimizationConstraintValueTypeId { get; set; }
        public OptimizationConstraintValueType OptimizationConstraintValueType { get; set; }

        /// <summary>
        /// The target for the constraint
        /// </summary>
        public decimal Constraint { get; set; }
    }
}
