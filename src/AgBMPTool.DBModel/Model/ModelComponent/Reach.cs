using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class Reach : BaseItem
    {
        [ForeignKey(nameof(ModelComponent))]
        public int ModelComponentId { get; set; }

        public ModelComponent ModelComponent { get; set; }

        [ForeignKey(nameof(Subbasin))]
        public int SubbasinId { get; set; }

        public Subbasin Subbasin { get; set; }

        /// <summary>
        /// The field boundary
        /// </summary>
        [Column(TypeName = "geometry (multilinestring)")]
        public MultiLineString Geometry { get; set; }
    }
}
