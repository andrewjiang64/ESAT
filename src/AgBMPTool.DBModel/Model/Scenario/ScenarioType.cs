using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    public class ScenarioType : Type.BaseType
    {
        /// <summary>
        /// If it's a base line scenario. Two baseline scenarios are conventional and existing.
        /// </summary>
        public bool IsBaseLine { get; set; }

        public bool IsDefault { get; set; }
    }
}
