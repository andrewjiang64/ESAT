using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class EffectivenessDTO
    {
        public int ScenarioModelResultVariableTypeId { get; set; }
        public int BMPEffectivenessLocationTypeId { get; set; }
        public int BMPEffectivenessTypeId { get; set; }
        public decimal EffectivenessValue { get; set; }
        public decimal CostEffectivenessValue { get; set; }
    }
}
