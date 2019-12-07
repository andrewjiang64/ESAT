using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class EffectivenessBoundDTO
    {
        public int BMPEffectivenessTypeId { get; set; }
        public decimal UpperPercentage { get; set; }
        public decimal LowerPercentage { get; set; }
        public decimal UpperAbsoluteValue { get; set; }
        public decimal LowerAbsoluteValue { get; set; }
    }
}
