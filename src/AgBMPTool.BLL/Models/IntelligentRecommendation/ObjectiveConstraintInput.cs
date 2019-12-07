using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class ObjectiveConstraintInput
    {
        public int BMPCombinationTypeId { get; set; }
        public int LocationId { get; set; }
        public OptimizationSolutionLocationTypeEnum LocationType { get; set; }
        public decimal ObjectiveValue { get; set; }
        public List<UnitEffectiveness> ConstraintValues { get; set; } = new List<UnitEffectiveness>();
    }
}
