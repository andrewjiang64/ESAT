using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Utility
{
    // DTO used to bind dropdown 
    public class DropdownDTO
    {
        public int ItemId { get; set; }

        public string ItemName { get; set; }

        public string Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsDefault { get; set; }
    }
}
