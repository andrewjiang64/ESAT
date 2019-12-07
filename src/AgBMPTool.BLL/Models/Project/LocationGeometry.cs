using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.BLL.Models.Project
{
    public class LocationGeometry
    {
        public int LocationId { get; set; }
        public int ParcelId { get; set; }
        public int LegalSubDivisionId { get; set; }
        public decimal Area { get; set; }
        public int FarmId { get; set; }
        public Geometry Geometry { get; set; }
    }
}
