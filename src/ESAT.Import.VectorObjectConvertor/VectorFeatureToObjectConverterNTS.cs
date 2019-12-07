using ESAT.Import.VectorObjectConvertor.ESATException;
using ESAT.Import.VectorObjectConvertor.Model;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VectorFeatureToObjectConverter
{
    public class VectorFeatureToObjectConverterNTS : IVectorFeatureToObjectConverter
    {
        private GeometryFactory gf = new GeometryFactory();

        public override Dictionary<int, VectorObject> ToNTSObjects(VectorFile v)
        {
            // check if file exists
            if (!File.Exists(v.Path))
            {
                Console.WriteLine($"Can't find file @{v.Path}\n----Skipped this file ----");
                return null;
            }

            using (var r = new ShapefileDataReader(v.Path, gf))
            {
                var res = new Dictionary<int, VectorObject>();

                bool checkedHeader = false;

                var csFact = new CoordinateSystemFactory();
                var ctFact = new CoordinateTransformationFactory();
                var projIn = csFact.CreateFromWkt(v.ProjectionWKT);

                var trans = ctFact.CreateFromCoordinateSystems(projIn, GeographicCoordinateSystem.WGS84);

                CoordinateArraySequenceFactory factory = CoordinateArraySequenceFactory.Instance;

                while (r.Read())
                {
                    if (!checkedHeader)
                    {
                        // flag has Id column
                        bool flag = false;
                        for (int i = 0; i < r.DbaseHeader.NumFields; i++)
                        {
                            if (r.DbaseHeader.Fields[i].Name.ToLower().Equals(GEOMETRY_KEY.ToLower()))
                            {
                                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                                    // following is the message send to console
                                    $"Shapefile has invalid header name \"{GEOMETRY_KEY}\"\n\n{v.Path}");
                            }
                            else if (r.DbaseHeader.Fields[i].Name.ToLower().Equals(ID_KEY.ToLower()))
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
                    }

                    // Define a directionary to save attributes and geometry
                    Dictionary<string, string> obj = new Dictionary<string, string>();

                    // First, save feature attributes to the directionary
                    for (int i = 0; i < r.DbaseHeader.NumFields; i++)
                    {
                        obj[r.DbaseHeader.Fields[i].Name.ToLower()] = r.GetString(i+1);
                    }
                    
                    // Add to output dictionary
                    res.Add(Convert.ToInt16(obj[ID_KEY.ToLower()]), new VectorObject { Geometry = this.Transform((Geometry) r.Geometry, trans, factory), Attributes = obj});

                }

                return res;
            }

        }

        public override Dictionary<int, Dictionary<string, string>> ToOCRObjects(VectorFile v)
        {
            throw new NotImplementedException();
        }

        private Geometry Transform(Geometry geomIn, ICoordinateTransformation trans, CoordinateArraySequenceFactory factory)
        {
            var corIn= geomIn.Coordinates;

            var coordinates = new Coordinate[corIn.Length];

            for (int i = 0; i < corIn.Length; i++)
            {
                coordinates[i] = trans.MathTransform.Transform(new Coordinate(corIn[i].X, corIn[i].Y));
            }

            ICoordinateSequence sequence = CopyToSequence(coordinates, factory.Create(coordinates.Length, Ordinates.XY));

            if (geomIn.GeometryType.Contains("Polygon"))
            {
                return (Geometry)gf.CreatePolygon(sequence);
            }
            else if (geomIn.GeometryType.Contains("Point"))
            {
                return (Geometry)gf.CreateMultiPoint(sequence);
            }
            else if (geomIn.GeometryType.Contains("LineString"))
            {
                return (Geometry)gf.CreateLineString(sequence);
            }

            throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                                    // following is the message send to console
                                    $"Geometry type not recognized \"{geomIn.GeometryType}\"");
        }

        private static ICoordinateSequence CopyToSequence(Coordinate[] coords, ICoordinateSequence sequence)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                sequence.SetOrdinate(i, Ordinate.X, coords[i].X);
                sequence.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }
            return sequence;
        }
    }
}
