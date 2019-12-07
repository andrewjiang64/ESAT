using AgBMPTool.DBModel.Model.Scenario;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class EffectivenessSummaryDTO
    {
        public BMPEffectivenessType BMPEffectivenessType { get; set; }

        public string UnitSymbol { get; set; } = "";

        public decimal PercentChange { get; set; } = 0;

        public decimal ValueChange { get; set; } = 0;

        public string Parameter { get
            {
                return UnitSymbol.Equals("-") ?
                    $"{BMPEffectivenessType.Name}" :
                    $"{BMPEffectivenessType.Name} ({UnitSymbol})";
            }
        }

        public decimal Value { get
            {
                return ValueChange;
            }
        }
    }
}
