using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class ConstraintInput : UnitEffectiveness
    {
        public OptimizationConstraintValueTypeEnum OptimizationConstraintValueTypeId { get; set; }
    }
}
