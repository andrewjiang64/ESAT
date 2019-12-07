using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    /// <summary>
    /// A unit scenario is the application of one or more bmps on a single location. It's different from the regular
    /// scenarios where multiple locations will be used. The unit scenario is only used for optimization purpose.
    /// </summary>
    public class UnitScenario : BaseItem
    {
        public int ModelComponentId { get; set; }

        [ForeignKey("ModelComponentId")]
        public ModelComponent.ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(Scenario))]
        public int ScenarioId { get; set; }

        /// <summary>
        /// The base scenario of the unit scenario, either conventional or existing
        /// </summary>
        public Scenario Scenario { get; set; }

        [ForeignKey(nameof(BMPCombination))]
        public int BMPCombinationId { get; set; }

        /// <summary>
        /// All bmp types at this location. This is the bmp combo in Shawn's design, i.e. combination of bmps
        /// </summary>
        public BMPCombinationType BMPCombination { get; set; }
        
        /// <summary>
        /// Effectiveness
        /// </summary>
        public List<UnitScenarioEffectiveness> UnitScenarioEffectivenesses { get; set; }
    }
}
