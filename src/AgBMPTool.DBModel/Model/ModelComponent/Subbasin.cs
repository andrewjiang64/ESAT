using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class Subbasin : BaseItemWithBoundary
    {
        [ForeignKey(nameof(SubWatershed))]
        public int SubWatershedId { get; set; }

        public SubWatershed SubWatershed { get; set; }

        public List<SubArea> SubAreas { get; set; }

        public Reach Reach { get; set; }
    }
}
