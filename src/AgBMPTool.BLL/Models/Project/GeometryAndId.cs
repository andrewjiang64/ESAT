using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;

namespace AgBMPTool.BLL.Models.Project
{
    public class GeometryAndId
    {
        public int Id { get; set; }
        public IGeometry geometry {get;set;}
    }
}
