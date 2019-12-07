using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class ConstraintTarget
    {
        public int BMPEffectivenessTypeId { get; set; }
        public decimal LowerValue { get; set; }
        public decimal UpperValue { get; set; }
    }
}
