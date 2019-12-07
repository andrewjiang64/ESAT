using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.BMPScopeAndIntelligenceData
{
    public class BMPScopeSummaryGridDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// which location is selected (blue) and the unselected locations (white)
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// which location is included in baseline (grey) and the unselected locations (white)
        /// </summary>
        public bool IsIncluded { get; set; }

        public int SummarizationType { get; set; }

        public double BMPArea { get; set; }

        public double OnsiteEffectiveness { get; set; }

        public double OffsiteEffectiveness { get; set; }

        public double Cost { get; set; }

        public double OnsiteCostEffectiveness { get; set; }

        public double OffsiteCostEffectiveness { get; set; }
    }
}
