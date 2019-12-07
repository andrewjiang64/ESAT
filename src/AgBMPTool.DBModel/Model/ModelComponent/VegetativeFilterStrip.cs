using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class VegetativeFilterStrip : ModelComponentStructurePolygon
    {
        /// <summary>
        /// Vegetative filter strip width (meter)
        /// </summary>
        [Column(TypeName = "numeric(5,0)")]
        public decimal Width { get; set; }

        /// <summary>
        /// Vegetative filter strip length (meter)
        /// </summary>
        [Column(TypeName = "numeric(5,0)")]
        public decimal Length { get; set; }

        /// <summary>
        /// Max. surface area, ha
        /// </summary>
        [Column(TypeName = "numeric(12,4)")]
        public decimal Area { get; set; }

        /// <summary>
        /// Drainage area to vegetative filter strip area ratio
        /// </summary>
        [Column(TypeName = "numeric(12,0)")]
        public decimal AreaRatio { get; set; }

        [Column(TypeName = "geometry (polygon)")]
        public Polygon DrainageArea { get; set; }
    }
}
