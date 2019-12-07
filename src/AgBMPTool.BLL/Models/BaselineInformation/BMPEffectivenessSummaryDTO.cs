using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BaselineInformation
{
    public class BMPEffectivenessSummaryDTO
    {
        public int Id { get; set; }
        public string Parameter { get; set; }
        public decimal Value { get; set; }
    }
}
