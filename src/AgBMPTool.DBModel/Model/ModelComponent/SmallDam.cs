using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class SmallDam : ModelComponentStructurePolygon
    {
        /// <summary>
        /// Maximum surface area, ha
        /// </summary>
        [Column(TypeName = "numeric(12,4)")]
        public decimal Area { get; set; }

        /// <summary>
        /// Maximum volume, m3
        /// </summary>
        [Column(TypeName = "numeric(12,0)")]
        public decimal Volume { get; set; }
    }
}
