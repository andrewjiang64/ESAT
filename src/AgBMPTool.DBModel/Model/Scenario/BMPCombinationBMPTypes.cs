using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Scenario
{
    public class BMPCombinationBMPTypes : BaseItem
    {
        [ForeignKey(nameof(BMPCombinationType))]
        public int BMPCombinationTypeId { get; set; }
        public BMPCombinationType BMPCombinationType { get; set; }

        [ForeignKey(nameof(BMPType))]
        public int BMPTypeId { get; set; }
        public BMPType BMPType { get; set; }
    }
}
