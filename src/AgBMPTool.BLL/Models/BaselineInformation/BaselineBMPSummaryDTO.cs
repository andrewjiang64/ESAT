using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BaselineInformation
{
    public class BaselineBMPSummaryDTO
    {
        public int Id { get; set; }
        public string BMP { get; set; }
        public int Count { get; set; }
        public decimal Area { get; set; }
        public decimal Cost { get; set; }
    }
}
