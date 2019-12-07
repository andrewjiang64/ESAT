using AgBMPTool.DBModel.Model.Scenario;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPSummaryDTO
    {
        public BMPType BMPType { get; set; }

        public int BMPTypeId { get; set; }
        public string BMPTypeName { get; set; }

        public int ModelComponentCount { get; set; }

        // Total BMP area in hectare
        public decimal TotalArea { get; set; }
        
        // Total BMP cost in $
        public decimal TotalCost { get; set; }
    }
}
