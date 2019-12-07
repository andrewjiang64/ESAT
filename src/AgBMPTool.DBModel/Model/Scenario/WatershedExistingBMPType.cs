using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    public class WatershedExistingBMPType : BaseItem
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }
        
        public ModelComponent.ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(ScenarioType))]
        public int ScenarioTypeId { get; set; }

        /// <summary>
        /// The base scenario, either conventional or existing
        /// </summary>
        public ScenarioType ScenarioType { get; set; }

        [ForeignKey(nameof(BMPType))]
        public int BMPTypeId { get; set; }

        /// <summary>
        /// All bmp types at this location.
        /// </summary>
        public BMPType BMPType { get; set; }

        [ForeignKey(nameof(Investor))]
        public int InvestorId { get; set; }

        public Investor Investor { get; set; }
    }
}
