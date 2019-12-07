using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class OptimizationSettingDTO
    {
        public OptimizationTypeEnum OptimizationType { get; set; }
        public decimal BudgetTarget { get; set; }
        public List<EcoServiceValueWeight> EsWeights { get; set; } = 
            new List<EcoServiceValueWeight>
            {
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 1, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 2, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 3, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 4, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 5, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 6, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 7, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 8, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 9, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 10, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 11, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 12, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 13, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 14, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 15, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 16, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 17, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 18, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 19, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 20, Value = 0 },
                new EcoServiceValueWeight { BMPEffectivenessTypeId = 21, Value = 100 }
            };
        public List<ConstraintInput> Constraints { get; set; } = 
            new List<ConstraintInput> {
                new ConstraintInput {
                    BMPEffectivenessTypeId = 21,
                    OptimizationConstraintValueTypeId = OptimizationConstraintValueTypeEnum.Percent,
                    Value = -20
                }
            };
    }
}
