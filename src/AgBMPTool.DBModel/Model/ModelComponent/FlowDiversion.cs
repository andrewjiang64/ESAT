using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class FlowDiversion : ModelComponentStructurePoint
    {
        /// <summary>
        /// flow diversion length (meter)
        /// </summary>
        [Column(TypeName = "numeric(6,0)")]
        public decimal Length { get; set; }
    }
}
