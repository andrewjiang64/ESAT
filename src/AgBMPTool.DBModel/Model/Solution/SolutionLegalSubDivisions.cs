using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Solution
{
    public class SolutionLegalSubDivisions : BaseItem
    {
        [ForeignKey(nameof(Solution))]
        public int SolutionId { get; set; }

        public Solution Solution { get; set; }

        [ForeignKey(nameof(BMPType))]
        public int BMPTypeId { get; set; }

        public Scenario.BMPType BMPType { get; set; }

        [ForeignKey(nameof(LegalSubDivision))]
        public int LegalSubDivisionId { get; set; }

        public Boundary.LegalSubDivision LegalSubDivision { get; set; }

        /// <summary>
        /// If the parcel or lsd is selected
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
