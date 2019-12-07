using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Optimization
{
    /// <summary>
    /// The selected location for optimization. All locations (LSD or parcels) are selected by default.
    /// </summary>
    public class OptimizationLegalSubDivisions : BaseItem
    {
        [ForeignKey(nameof(Optimization))]
        public int OptimizationId { get; set; }

        public Optimization Optimization { get; set; }

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
