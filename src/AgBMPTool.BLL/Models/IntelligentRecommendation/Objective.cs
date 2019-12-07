using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class Objective
    {
        public OptimizationObjectiveTypeEnum GetOptimizationObjectiveType { get; set; }
        public decimal Value { get; set; }
    }
}
