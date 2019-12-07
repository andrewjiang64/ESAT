using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.VectorObjectConvertor.Model
{
    public class VectorFile
    {
        public string Path { get; set; }

        public int ProjectionCode { get; set; }

        public string ProjectionWKT { get; set; }

        public string Driver { get; set; }

        public virtual VectorFile GetMockVectorFile()
        {
            throw new NotImplementedException();
        }
    }
}
