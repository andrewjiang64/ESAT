using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    public class Farm : BaseItemWithBoundary
    {
        public string Name { get; set; }

        public int OwnerId { get; set; }
    }
}
