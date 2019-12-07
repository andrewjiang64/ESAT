using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.VectorObjectConvertor.Model
{
    public abstract class IVectorFeatureToObjectConverter
    {
        // Default EPSG standard code is 4326 for WGS 84
        public static int EPSG_STANDARD_CODE = 4326;

        // Default geometry key in the object
        public static string GEOMETRY_KEY = "ST_GEOMETRY";

        // Default Id key
        public static string ID_KEY = "Id";

        /** Doc
         * This method convert vector file into <Id,<key,value>> objects with Id as feature key
         * Geometry information are saved in the "ST_GEOMETRY" key
         * Vector MUST have a "Id" column in the attribute table 
         */
        public abstract Dictionary<int, Dictionary<string, string>> ToOCRObjects(VectorFile v);

        /** Doc
         * This method convert vector file into <Id,VectorObject> objects with Id as feature key
         * Geometry information are saved in the VectorObject
         * Vector MUST have a "Id" column in the attribute table 
         */
        public abstract Dictionary<int, VectorObject> ToNTSObjects(VectorFile v);
    }
}
