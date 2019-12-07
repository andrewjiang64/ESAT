using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public interface BMPLocations
    {
        int BMPTypeId { get; set; }

        List<int> ModelComponentIds { get; set; }
    }
}
