using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.IntelligentRecommendation
{
    public class ScenarioModelResultDTO
    {
        public int OnsiteModelComponentId { get; set; }
        public List<UnitScenarioModelResult> OnsiteScenarioModelResults { get; set; }
        public int OffsiteModelComponentId { get; set; }
        public List<UnitScenarioModelResult> OffsiteScenarioModelResults { get; set; }
    }
}
