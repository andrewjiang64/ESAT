using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    /// <summary>
    /// This is the lsd. The column name will match the names in the shapefile. 
    /// We inheritate from parcel to avoid duplicate column definition.
    /// </summary>
    public class LegalSubDivision : BaseItemWithBoundary
    {
        public short Meridian { get; set; }

        public short Range { get; set; }

        public short Township { get; set; }

        public short Section { get; set; }

        public string Quarter { get; set; }

        public short LSD { get; set; }

        public string FullDescription { get; set; }

    }
}
