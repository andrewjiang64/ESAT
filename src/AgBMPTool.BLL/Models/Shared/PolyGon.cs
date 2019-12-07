using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace AgBMPTool.BLL.Models.Shared
{
    public class ExtentModel
    {
        public double xmin { get; set; }
        public double ymin { get; set; }
        public double xmax { get; set; }
        public double ymax { get; set; }
        public SpatialReferenceModel spatialReference { get; set; }

        public override string ToString()
        {
            return string.Format("xmin = {0}, ymin = {1}, xmax = {2}, ymax = {3}", xmin, ymin, xmax, ymax);
        }
    }

    public class SpatialReferenceModel
    {
        public int wkid;
    }

    public class GeometryDTO {

        public int Id { get; set; }
        public string selected { get; set; }
        public List<GeometryAttriBute> attributes { get; set; }
        public string type { get; set; }
    }

    public class modelCompoentGeometryLayer {
        public List<GeometryDTO> geometries;
        public string layername;
        public string geometrytype;
        public GeometryStyle geometryStyle;
        public modelCompoentGeometryLayer() {
            this.geometries = new List<GeometryDTO>();
        }
    }
   public class PolyGon : GeometryDTO
    {
        public List<Double[]> coordinates { get; set; }
        public PolyGonStyle style { get; set; }
        public static List<Double[]> convertPolygonString(Polygon geometry)
        {
          
                List<Double[]> list = new List<Double[]>();
                for (int j = 0; j < geometry.Coordinates.Length; j++)
                {
                    Double[] coordinate = new Double[2];
                    coordinate[0] = geometry.Coordinates[j].X;
                    coordinate[1] = geometry.Coordinates[j].Y;
                    list.Add(coordinate);
                }
              
            return list;
        }
    }

   public class MutiPolyGon: GeometryDTO {
        public List<List<Double[]>> coordinates { get; set; }
        public PolyGonStyle style { get; set; }
        public static List<List<Double[]>> convertPolygonString(MultiPolygon geometry)
        {
            var simplifiedGeo = DouglasPeuckerSimplifier.Simplify(geometry, 0.0005) as GeometryCollection;
            if (simplifiedGeo == null)
                simplifiedGeo = geometry;
            var geometries = simplifiedGeo.Geometries;
            List<List<Double[]>> res = new List<List<Double[]>>();
            for (int i = 0; i < geometries.Length; i++) {
                List<Double[]> list = new List<Double[]>();
                for (int j = 0; j < geometries[i].Coordinates.Length; j++) {
                    Double[] coordinate = new Double[2];
                    coordinate[0] = geometries[i].Coordinates[j].X;
                    coordinate[1] = geometries[i].Coordinates[j].Y;
                    list.Add(coordinate);
                }
                res.Add(list);
            }
            return res;
        }

    }
    public class PolyLine : GeometryDTO
    {
        public List<Double[]> coordinates { get; set; }
        public SimpleLineStyle style { get; set; }
        public static List<Double[]> convertPolygonString(MultiLineString geometry)
        {
            var simplifiedGeo = DouglasPeuckerSimplifier.Simplify(geometry, 0.0005);

            var coordinates = geometry.Coordinates;
            if(simplifiedGeo != null)
            {
                if (simplifiedGeo is MultiLineString)
                    coordinates = (simplifiedGeo as MultiLineString).Coordinates;
                else if(simplifiedGeo is LineString)
                    coordinates = (simplifiedGeo as LineString).Coordinates;
            }

            List<Double[]> res = new List<Double[]>();
            for (int i = 0; i < coordinates.Length; i++)
            {
                var coordinate = coordinates[i];
                Double[] ds = new Double[2];
                ds[0] = coordinate.X;
                ds[1] = coordinate.Y;
                res.Add(ds);

            }
            return res;
        }
    }

    public class MutiPointDTO : GeometryDTO
    {
        public List<Double[]> coordinates { get; set; }
        public SimpleLineStyle style { get; set; }
        public static List<Double[]> convertMutiplePoints(MultiPoint point)
        {
            var coordinates = point.Coordinates;
            List<Double[]> res = new List<Double[]>();
            for (int i = 0; i < coordinates.Length; i++)
            {
                var coordinate = coordinates[i];
                Double[] ds = new Double[2];
                ds[0] = coordinate.X;
                ds[1] = coordinate.Y;
                res.Add(ds);
            }
            return res;
        }
    }

    public class GeometryAttriBute
    {
        public String Name { get; set; }
        public String Value { get; set; }

        public static GeometryAttriBute getGeometryIdAttribue(int Id) {
            return new GeometryAttriBute()
            { Name = "Id",
              Value = Id + ""
            };
        }
        public static GeometryAttriBute getGeometryIsSelectedAttribue(string selected)
        {
            return new GeometryAttriBute()
            {
                Name = "selected",
                Value = selected
            };
        }
    }
}
