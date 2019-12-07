using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
   public class ModelComponentDTO
    {
        public int BMPTypeId { get; set; }

        public int ModelComponentId { get; set; }

        /// <summary>
        /// If the parcel or lsd is selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
