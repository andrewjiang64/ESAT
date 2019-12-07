using AgBMPTool.DBModel.Model.Scenario;
using AgBMPTool.DBModel.Model.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Shared
{
    public class BMPEffectivenessTypeDTO
    {
        public int id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public int sortOrder { get; set; }

        public int unitTypeId { get; set; }

        public string unitTypeSymbol { get; set; }

        public int defaultWeight { get; set; }

        public int? scenarioModelResultTypeId { get; set; }

        public int? scenarioModelResultVariableTypeId { get; set; }

        public int bmpEffectivenessLocationTypeId { get; set; }

        public int? defaultConstraintTypeId { get; set; }

        public decimal? defaultConstraint { get; set; }

        /// <summary>
        /// create constructor
        /// </summary>
        public BMPEffectivenessTypeDTO() { }

        public BMPEffectivenessTypeDTO(BMPEffectivenessType bmpType, UnitType unitTypeEntity)
        {
            this.id = bmpType.Id;
            this.name = bmpType.Name;
            if (unitTypeEntity != null)
            {
                this.name += " (" + unitTypeEntity.UnitSymbol + ")";
                this.unitTypeSymbol = unitTypeEntity.UnitSymbol;
            }
            this.description = bmpType.Description;
            this.sortOrder = bmpType.SortOrder;
            this.unitTypeId = bmpType.UnitTypeId;
            this.defaultWeight = bmpType.DefaultWeight;
            this.scenarioModelResultTypeId = bmpType.ScenarioModelResultTypeId;
            this.scenarioModelResultVariableTypeId = bmpType.ScenarioModelResultVariableTypeId;
            this.bmpEffectivenessLocationTypeId = bmpType.BMPEffectivenessLocationTypeId;
            this.defaultConstraintTypeId = bmpType.DefaultConstraintTypeId;
            this.defaultConstraint = bmpType.DefaultConstraint;
        }
    }
}
