using AgBMPTool.BLL.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Common
{
    public class BMPEffectivenessTypeModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int SortOrder { get; set; }

        public int UnitTypeId { get; set; }

        public int DefaultWeight { get; set; }

        public int? ScenarioModelResultTypeId { get; set; }

        public int? ScenarioModelResultVariableTypeId { get; set; }

        public int BMPEffectivenessLocationTypeId { get; set; }

        public int? DefaultConstraintTypeId { get; set; }

        public decimal? DefaultConstraint { get; set; }

        public BMPEffectivenessTypeModel(BMPEffectivenessTypeDTO bmpType)
        {
            this.Id = bmpType.id;
            this.Name = bmpType.name;
            this.Description = bmpType.description;
            this.SortOrder = bmpType.sortOrder;
            this.UnitTypeId = bmpType.unitTypeId;
            this.DefaultWeight = bmpType.defaultWeight;
            this.ScenarioModelResultTypeId = bmpType.scenarioModelResultTypeId;
            this.ScenarioModelResultVariableTypeId = bmpType.scenarioModelResultVariableTypeId;
            this.BMPEffectivenessLocationTypeId = bmpType.bmpEffectivenessLocationTypeId;
            this.DefaultConstraintTypeId = bmpType.defaultConstraintTypeId;
            this.DefaultConstraint = bmpType.defaultConstraint;
        }
    }
}
