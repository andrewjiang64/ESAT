using System;
using System.Collections.Generic;
using System.Text;
using static AgBMPTool.BLL.Enumerators.Enumerators;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPCostAreaDTO
    {
        public int LocationId { get; set; }
        public int FarmId { get; set; }
        public int BMPCombinationTypeId { get; set; }
        public decimal BMPArea { get; set; }
        public string BMPCombinationTypeName { get; set; }
        public decimal Cost { get; set; }
        public bool IsSelectable { get; set; }
        public bool IsSelected { get; set; }
        public int OptimizationSolutionLocationTypeId { get; set; }
    }
}
