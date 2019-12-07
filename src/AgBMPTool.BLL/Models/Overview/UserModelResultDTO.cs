using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Overview
{
    public class UserModelResultDTO
    {
        public int locationid { get; set; }

        public int resulttype { get; set; }

        public decimal resultvalue { get; set; }
        
        public decimal stdvalue { get; set; }
    }
}
