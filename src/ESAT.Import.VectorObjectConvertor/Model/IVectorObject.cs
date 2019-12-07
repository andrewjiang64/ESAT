using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.VectorObjectConvertor.Model
{
    public abstract class IVectorObject
    {
        public Dictionary<string,string> Attributes { get; set; }

        public Geometry Geometry { get; set; }
    }
}
