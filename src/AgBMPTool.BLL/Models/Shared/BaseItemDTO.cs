using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Shared
{
    public class BaseItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BMPTypeDTO : BaseItemDTO {
        public int watershedId { get; set; }
        public int scenarioTypeId { get; set; }
        public int modelComponentId { get; set; }
        public int modelComponentTypeId { get; set; }

        public int projectId { get; set; }
    }

    public class ModelCompoentDTO {
        public int modelComponentId { get; set; }
        public bool isSelected { get; set; }

        public int modelCompoentTypeId { get; set; }
    }
}
