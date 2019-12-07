using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    public class UnitOptimizationSolutionEffectiveness : BaseItem
    {
        [ForeignKey(nameof(UnitOptimizationSolution))]
        public int UnitOptimizationSolutionId { get; set; }

        public UnitOptimizationSolution UnitOptimizationSolution { get; set; }

        [ForeignKey(nameof(BMPEffectivenessType))]
        public int BMPEffectivenessTypeId { get; set; }

        public BMPEffectivenessType BMPEffectivenessType { get; set; }

        public int Year { get; set; }

        public decimal Value { get; set; }
    }
}
