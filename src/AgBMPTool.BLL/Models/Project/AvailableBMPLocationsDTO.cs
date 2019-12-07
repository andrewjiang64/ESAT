using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class AvailableBMPLocationsDTO : BMPLocations
    {
        public int BMPTypeId { get; set; }

        public List<int> ModelComponentIds { get; set; }
    }
}
