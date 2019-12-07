using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    /// <summary>
    /// We will use quater section for parcel. So 
    /// </summary>
    public class Parcel : BaseItemWithBoundary
    {
        public short Meridian { get; set; }

        public short Range { get; set; }

        public short Township { get; set; }

        public short Section { get; set; }

        public string Quarter { get; set; }

        public string FullDescription { get; set; }

        public int OwnerId { get; set; }
    }
}
