using AgBMPTool.DBModel.Model.Boundary;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    /// <summary>
    /// A sub area is the interseciton of subbasin and lsd. It's the smallest unit in the system.
    /// </summary>
    public class SubArea : BaseItemWithBoundary
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }
        
        public ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(Subbasin))]
        public int SubbasinId { get; set; }

        public Subbasin Subbasin { get; set; }

        [ForeignKey(nameof(LegalSubDivision))]
        public int LegalSubDivisionId { get; set; }
        public LegalSubDivision LegalSubDivision { get; set; }

        [ForeignKey(nameof(Parcel))]
        public int ParcelId { get; set; }

        public Parcel Parcel { get; set; }

        public decimal Area { get; set; }

        public decimal Elevation { get; set; }

        public decimal Slope { get; set; }

        public string LandUse { get; set; }

        public string SoilTexture { get; set; }


    }
}
