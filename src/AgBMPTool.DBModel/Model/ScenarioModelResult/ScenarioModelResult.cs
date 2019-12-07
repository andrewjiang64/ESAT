using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ScenarioModelResult
{
    /// <summary>
    /// Model result for a model component
    /// </summary>
    public class ScenarioModelResult : BaseItem
    {
        [ForeignKey(nameof(Scenario))]
        public int ScenarioId { get; set; }

        public Scenario.Scenario Scenario { get; set; }

        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }

        public ModelComponent.ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(ScenarioModelResultType))]
        public int ScenarioModelResultTypeId { get; set; }

        public ScenarioModelResultType ScenarioModelResultType { get; set; }

        public int Year { get; set; }

        public decimal Value { get; set; }
    }
}
