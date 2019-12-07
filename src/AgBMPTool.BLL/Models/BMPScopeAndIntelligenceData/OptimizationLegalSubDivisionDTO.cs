using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
   public class LegalSubDivisionDTO
    {
        public int BMPTypeId { get; set; }

        public int LegalSubDivisionId { get; set; }

        /// <summary>
        /// If the parcel or lsd is selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
