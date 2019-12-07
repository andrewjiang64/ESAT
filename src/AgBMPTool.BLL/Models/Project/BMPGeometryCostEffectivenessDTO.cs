using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace AgBMPTool.BLL.Models.Project
{
    public class BMPGeometryCostEffectivenessDTO : BMPCostAllEffectivenessDTO
    {
        public Geometry Geometry { get; set; }
    }
}
