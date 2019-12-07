using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class SubAreaResultDTO
    {
        public int subareaid {get;set;}

        public int modelresulttypeid { get; set; }

        public int resultyear { get; set; }

        public decimal resultvalue { get; set; }
    }
}
