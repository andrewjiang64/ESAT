using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.ModelComponent
{
    public class ModelComponentStructurePoint : ModelComponentStructure
    {
        [Column(TypeName = "geometry (multipoint)")]
        public virtual MultiPoint Geometry { get; set; }
    }
}
