using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class CostEffectivenessInput
    {
        public int BMPCombinationTypeId { get; set; }
        public int LocationId { get; set; }
        public OptimizationSolutionLocationTypeEnum LocationType { get; set; }
        public decimal Cost { get; set; }
        public List<UnitEffectiveness> EffectivessValues { get; set; } = new List<UnitEffectiveness>();
    }
}
