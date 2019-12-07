using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.ScenarioModelResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils
{
    interface IScenarioResulToObjectConverter : ITableToObjectConverter
    {
        ScenarioModelResult BuildScenarioModelResult(int scenarioId, int modelComponentId, int scenarioModelResultTypeId, int year, decimal value);
    }
}
