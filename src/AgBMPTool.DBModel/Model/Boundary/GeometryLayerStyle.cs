using System;
using System.Collections.Generic;
using System.Text;

namespace AgBMPTool.DBModel.Model.Boundary
{
    public class GeometryLayerStyle : BaseItem
    {
        /// <summary>
        /// Name of the geometry layer
        /// </summary>
        public string layername { get; set; }

        /// <summary>
        /// type of symbology.
        /// simple-fill for polygon and multi-polygon
        /// simple-line for linestring
        /// simple-marker for point and multiple-point
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// style of polygon file, line type or marker type
        /// polgyon - backward-diagonal, cross, diagonal-cross, forward-diagonal, horizontal, none, solid, vertical
        /// https://developers.arcgis.com/javascript/latest/api-reference/esri-symbols-SimpleFillSymbol.html#style
        /// 
        /// line - dash, dash-dot, dot, long-dash, long-dash-dot, long-dash-dot-dot, none, short-dash, short-dash-dot, short-dash-dot-dot, short-dot, solid
        /// https://developers.arcgis.com/javascript/latest/api-reference/esri-symbols-SimpleLineSymbol.html#style
        /// 
        /// marker - circle, cross, diamond, square, triangle, x
        /// https://developers.arcgis.com/javascript/latest/api-reference/esri-symbols-SimpleMarkerSymbol.html#style
        /// </summary>
        public string style { get; set; }


        /// <summary>
        /// polygon - color of fill
        /// line/point - color of line/marker
        /// 
        /// rgba (rgb(158, 0, 0, 0.6)) or color name (purple) 
        /// a value: 0 indicates the color is fully transparent and 1 indicates it is fully opaque.
        /// https://developers.arcgis.com/javascript/latest/api-reference/esri-Color.html
        /// </summary>
        public string color { get; set; }

        public string size { get; set; }

        /// <summary>
        /// Width of line. Only applied to line
        /// </summary>
        public string simplelinewidth { get; set; }

        /// <summary>
        /// color of outline. Only for polygon and points. 
        /// </summary>
        public string outlinecolor { get; set; }

        /// <summary>
        /// width of outline. Only for polygon and points.
        /// </summary>
        public string outlinewidth { get; set; }

        public string outlinestyle { get; set; }
    }
}
