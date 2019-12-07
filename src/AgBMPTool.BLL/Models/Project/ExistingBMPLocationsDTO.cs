using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class ExistingBMPLocationsDTO : BMPLocations
    {
        public int BMPTypeId { get; set; }

        public List<int> ModelComponentIds { get; set; }
    }
}
