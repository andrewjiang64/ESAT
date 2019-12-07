using AgBMPTool.DBModel.Model.ModelComponent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    /// <summary>
    /// This is the regular model scenario with bmps applied at different places, e.g. baseline
    /// </summary>
    public class Scenario : BaseItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey(nameof(Watershed))]
        public int WatershedId { get; set; }

        public Watershed Watershed { get; set; }

        [ForeignKey(nameof(ScenarioType))]
        public int ScenarioTypeId { get; set; }

        public ScenarioType ScenarioType { get; set; }

        /// <summary>
        /// List of model results
        /// </summary>
        public List<ScenarioModelResult.ScenarioModelResult> ScenarioModelResults { get; set; }

        /// <summary>
        /// All unit scenarios base on this scenario
        /// </summary>
        public List<UnitScenario> UnitScenarios { get; set; }
    }
}
