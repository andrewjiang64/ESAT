using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Overview
{
    public class UserLocationInfo
    {
        public int locationid { get; set; }

        public decimal area { get; set; }

        public decimal elevation { get; set; }

        public decimal slope { get; set; }

        public string landuse { get; set; }

        public string soiltexture { get; set; }
    }
}
