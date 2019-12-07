using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model
{
    public class BaseItemWithBoundary : BaseItem
    {
        [Column(TypeName = "geometry (multipolygon)")]
        public virtual MultiPolygon Geometry { get; set; }
    }
}
