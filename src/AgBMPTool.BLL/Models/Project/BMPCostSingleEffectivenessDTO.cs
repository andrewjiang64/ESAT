using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPCostSingleEffectivenessDTO : BMPCostAreaDTO
    {
        public int ScenarioModelResultVariableId { get; set; }
        public decimal OnsiteEffectiveness { get; set; }
        public decimal OffsiteEffectiveness { get; set; }
        public decimal OnsiteCostEffectivenessValue { get; set; }
        public decimal OffsiteCostEffectivenessValue { get; set; }
    }
}
