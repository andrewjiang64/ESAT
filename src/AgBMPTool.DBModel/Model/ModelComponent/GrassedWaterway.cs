using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class GrassedWaterway : ModelComponentStructurePolygon
    {
        /// <summary>
        /// grassed waterway width (meter)
        /// </summary>
        [Column(TypeName = "numeric(5,0)")]
        public decimal Width { get; set; }

        /// <summary>
        /// grassed waterway length (meter)
        /// </summary>
        [Column(TypeName = "numeric(5,0)")]
        public decimal Length { get; set; }
    }
}
