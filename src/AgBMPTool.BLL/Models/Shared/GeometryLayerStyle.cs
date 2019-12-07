using System;
using System.Collections.Generic;
using System.Text;
using AgBMPTool.DBModel.Model.Boundary;

namespace AgBMPTool.BLL.Models.Shared
{
    public class GeometryStyle {
        public string color { get; set; }
        public string type { get; set; }
        public static GeometryStyle getGeometryStyle(GeometryLayerStyle style, string geometrytype) {
            switch (geometrytype)
            {
                case "MultiPoint":
                    return new PointStyle(style);
                case "MultiLineString":
                    return new SimpleLineStyle(style);
                case "MultiPolygon":
                    return new PolyGonStyle(style);
                default:
                    return null;
            }
        }
    }
    public class SimpleLineStyle : GeometryStyle
    {
        public string width { get; set; }
        public SimpleLineStyle()
        {

        }
        public SimpleLineStyle(GeometryLayerStyle geometryLayerstyle) {
            this.color = geometryLayerstyle.color;
            this.width = geometryLayerstyle.simplelinewidth;
            this.type = geometryLayerstyle.type;
        }
    }

    public class PointStyle : GeometryStyle {
        public string style { get; set; }
        public SimpleLineStyle outline { get; set; }

        public string size { get; set; }

        public PointStyle(GeometryLayerStyle geometrylayerstyle)
        {
            this.style = geometrylayerstyle.style;
            this.type = geometrylayerstyle.type;
            this.color = geometrylayerstyle.color;
            this.size = geometrylayerstyle.size;
            this.outline = new SimpleLineStyle()
            {
                color = geometrylayerstyle.outlinecolor,
                width = geometrylayerstyle.outlinewidth,
                type = "simple-line"
            };
        }
    }

    public class PolyGonStyle : GeometryStyle {
        public string style { get; set; }
        public SimpleLineStyle outline { get; set; }

        public PolyGonStyle(GeometryLayerStyle geometrylayerstyle) {
            this.style = geometrylayerstyle.style;
            this.type = geometrylayerstyle.type;
            this.color = geometrylayerstyle.color;
            this.outline = new SimpleLineStyle()
            {
                color = geometrylayerstyle.outlinecolor,
                width = geometrylayerstyle.outlinewidth,
                 type = "simple-line"
            };
        }
    }
}
