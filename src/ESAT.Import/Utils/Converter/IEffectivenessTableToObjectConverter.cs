using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils
{
    interface IEffectivenessTableToObjectConverter : ITableToObjectConverter
    {
        UnitScenarioEffectiveness BuildUnitScenarioEffectiveness(int unitScenarioId, int bmpEffectivenessId, int year, decimal value);
    }
}
