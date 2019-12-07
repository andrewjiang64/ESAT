using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    /// <summary>
    /// We don't use all columns in the municipality shapefile. Only name and region. 
    /// </summary>
    public class Municipality : BaseItemWithBoundary
    {
        public string Name { get; set; }

        public string Region { get; set; }
    }
}
