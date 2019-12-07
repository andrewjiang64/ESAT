using AgBMPTool.DBModel.Model.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ScenarioModelResult
{
    public class ScenarioModelResultType : BaseType
    {
        [ForeignKey(nameof(UnitType))]
        public int UnitTypeId { get; set; }

        public UnitType UnitType { get; set; }

        [ForeignKey(nameof(ModelComponentType))]
        public int ModelComponentTypeId { get; set; }

        public ModelComponent.ModelComponentType ModelComponentType {get;}

        [ForeignKey(nameof(ScenarioModelResultVariableType))]
        public int ScenarioModelResultVariableTypeId { get; set; }

        public ScenarioModelResultVariableType ScenarioModelResultVariableType { get; set; }
    }
}
