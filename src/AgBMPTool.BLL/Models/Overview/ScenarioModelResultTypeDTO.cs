using AgBMPTool.DBModel.Model.ScenarioModelResult;
using AgBMPTool.DBModel.Model.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Overview
{
    public class ScenarioModelResultTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int SortOrder { get; set; }

        public int UnitTypeId { get; set; }

        public string UnitSymbol { get; set; }

        public int ModelComponentTypeId { get; set; }

        public int ScenarioModelResultVariableTypeId { get; set; }


        public ScenarioModelResultTypeDTO(ScenarioModelResultType entity, UnitType unittypeEntity)
        {
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Description = entity.Description;
            this.SortOrder = entity.SortOrder;
            this.UnitTypeId = entity.UnitTypeId;
            this.UnitSymbol = unittypeEntity.UnitSymbol;
            this.ScenarioModelResultVariableTypeId = entity.ScenarioModelResultVariableTypeId;
        }
    }
}
