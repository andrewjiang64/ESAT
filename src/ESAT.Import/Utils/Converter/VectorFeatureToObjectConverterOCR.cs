using ESAT.Import.ESATException;
using ESAT.Import.Utils;
using ESAT.Import.VectorObjectConvertor.Model;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ESAT.Import.Utils.Converter
{
    public class VectorFeatureToObjectConverterOCR : IVectorFeatureToObjectConverter
    {
        public override Dictionary<int, VectorObject> ToNTSObjects(VectorFile v)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<int, Dictionary<string, string>> ToOCRObjects(VectorFile v)
        {
            // check if file exists
            if (!File.Exists(v.Path))
            {
                Console.WriteLine($"Can't find file @{v.Path}\n----Skipped this file ----");
                return null;
            }

            // Register all OGR methods
            Ogr.RegisterAll();

            // Get OGR shapefile driver for ESRI Shapefile
            var drv = Ogr.GetDriverByName(v.Driver);

            // Open shapefile and get the layer
            var ds = drv.Open(v.Path, 0);
            OSGeo.OGR.Layer layer = ds.GetLayerByIndex(0);

            // Define a working feature
            OSGeo.OGR.Feature f;

            // Check if layer has {GEOMETRY_KEY} or {ID_KEY} column
            while ((f = layer.GetNextFeature()) != null)
            {
                // flag has Id column
                bool flag = false;
                for (int i = 0; i < f.GetFieldCount(); i++)
                {
                    if (f.GetFieldDefnRef(i).GetName().ToLower().Equals(GEOMETRY_KEY.ToLower()))
                    {
                        throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                            // following is the message send to console
                            $"Shapefile has invalid header name \"{GEOMETRY_KEY}\"\n\n{v.Path}");
                    } else if (f.GetFieldDefnRef(i).GetName().ToLower().Equals(ID_KEY.ToLower()))
                    {
                        flag = true;
                    }
                }

                if (!flag)
                {
                    throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                            // following is the message send to console
                            $"Shapefile doesn't have essential column \"{ID_KEY}\"");
                }

                break;
            }

            // Reset reading record to the beginning
            layer.ResetReading();

            // Create a dictionary to save all imported objects
            Dictionary<int, Dictionary<string, string>> output = new Dictionary<int, Dictionary<string, string>>();

            // Loop through all features
            while ((f = layer.GetNextFeature()) != null)
            {
                // Define a directionary to save attributes and geometry
                Dictionary<string, string> obj = new Dictionary<string, string>();

                // First, save feature attributes to the directionary
                for (int i = 0; i < f.GetFieldCount(); i++)
                {
                    obj[f.GetFieldDefnRef(i).GetName().ToLower()] = f.GetFieldAsString(i);
                }

                // Second, save geometry to the directionary under key "Geometry"
                var geom = f.GetGeometryRef();

                if (geom != null)
                {
                    string pgisGeom;
                    geom.ExportToWkt(out pgisGeom);

                    // Transform layer projection to standard EPSG WGS84
                    pgisGeom = $"ST_Transform(ST_Multi(ST_geomFromText('{pgisGeom}',{v.ProjectionCode})),{EPSG_STANDARD_CODE})";

                    obj[GEOMETRY_KEY] = pgisGeom;
                }

                // Add to output dictionary
                output.Add(Convert.ToInt16(obj[ID_KEY.ToLower()]), obj);
            }

            return output;
        }

    }
}
