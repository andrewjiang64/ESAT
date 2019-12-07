using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPTypeLocationsDTO
    {
        public int BMPTypeId { get; set; }
        public IEnumerable<int> LocationIds { get; set; }
    }
}
