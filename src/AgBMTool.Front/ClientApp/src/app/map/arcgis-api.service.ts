import { Injectable } from '@angular/core';
import { loadModules, loadScript } from 'esri-loader';
import { BehaviorSubject } from 'rxjs';
import { geometry } from '@progress/kendo-drawing';

@Injectable({
  providedIn: 'root'
})

export class ArcgisApiService {
  loaded$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  constructor() {
    this.loaded$.subscribe(loaded => {
      if (!loaded) {
        loadScript({
          // use a specific version of the JSAPI
          url: 'https://js.arcgis.com/4.12/'
        })
          .then(() => {
            this.loaded$.next(true)
          }).catch(err => this.loaded$.next(true))
      }
    });
  }

  constructMap(opts: { basemap: any; elevation: boolean }): Promise<any> {
    return new Promise((resolve, reject) => {
      loadModules(['esri/Map']).then(([Map]) => {
        const map = new Map({
          basemap: opts.basemap
        });
        if (opts.elevation) {
       //   map.ground = 'world-elevation';
        }
        resolve(map);
      });
    });
  }

  constructMapView(opts: {
    center: number[];
    zoom: number;
    container: string;
    map: any;
    padding?: any;
  }): Promise<any> {
    return new Promise((resolve, reject) => {
      loadModules(['esri/views/MapView', "esri/widgets/BasemapToggle"])
        .then(([MapView, BasemapToggle]) => {
          const view = new MapView({
            center: opts.center,
            zoom: opts.zoom,
            map: opts.map,
            container: opts.container,
            padding: opts.padding ? opts.padding : {},
            highlightOptions: {
              color: [255, 0, 0, 1],
              haloColor: "red",
              haloOpacity: 0.9,
              fillOpacity: 0.2
            }
          });
          var basemapToggle = new BasemapToggle({
            view: view,
            nextBasemap: "topo"
          });
          // Add the widget to the top-right corner of the view
          view.ui.add(basemapToggle, {
            position: "bottom-left"
          });
          view.ui.move("zoom", "bottom-left");
          view.when(() => {
            resolve(view);
          }); 
        });
    });
  }

  addGeometryPloyLineGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/GraphicsLayer"])
        .then(([Graphic, GraphicsLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          if (layer == null) {
            var layer = new GraphicsLayer({ id: opts.layname });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var paths = geometry.coordinates;
            var attributes = geometry.attributes;
            var object = new Object();
            for (var k = 0; k < attributes.length; k++) {
              object[attributes[k].name] = attributes[k].value;
            }
            var polyline = {
              type: "polyline", // autocasts as new Polyline()
              paths: paths
            };
            var polylineGraphic = new Graphic({
              geometry: polyline,
              symbol: geometry["style"],
              attributes: object
            });
            layer.add(polylineGraphic);
          }
          resolve(layer);
        });
    });
 }

  addGeometryMutiPolyGonGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/GraphicsLayer"])
        .then(([Graphic, GraphicsLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          if (layer == null) {
            var layer = new GraphicsLayer({ id: opts.layname });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var rings = coordinates[j];
              var polyGon = {
                type: "polygon",
                rings: rings,
              };
              var attributes = geometry.attributes;
              var object = new Object();
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
              }
              object["objectID"] = object["Id"];
              var polyGonGraphic = new Graphic({
                geometry: polyGon,
                symbol: geometry["style"],
                attributes: object
              });
              layer.add(polyGonGraphic);
            }
          }
          resolve(layer);
        });
    });
  }

 addGeometryMutiPointsGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/GraphicsLayer"])
        .then(([Graphic, GraphicsLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          if (layer == null) {
            var layer = new GraphicsLayer({ id: opts.layname });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var points = coordinates[j];
              var point = {
                type: "point",
                latitude: points[0],
                longitude: points[1],
              };
              var attributes = geometry.attributes;
              var object = new Object();
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
              }
              var pointgraphic = new Graphic({
                geometry: point,
                symbol: geometry["style"],
                attributes: object
              });
              layer.add(pointgraphic);    
            }
          }
          resolve(layer);
        });
    });
  }
  addGeometryBMPTypeMutiPolyGonGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selected == 'Multiple', 'M', '') ",
            }
          };
          var polygonnotselectedsymbol = {
            type: "simple-fill",  // autocasts as new SimpleFillSymbol()
            color: "rgba(255,255,255,0.1)",
            style: "solid",
            outline: {  // autocasts as new SimpleLineSymbol()
              color: "grey",
              width: 1
            }
          };
          var polygondisabledselectedsymbol = {
            type: "simple-fill",  // autocasts as new SimpleFillSymbol()
            color: "grey",
            style: "solid",
            outline: {  // autocasts as new SimpleLineSymbol()
              color: "white",
              width: 1
            }
          };
          var fields = [
            {
              name: "ObjectID",
              type: "oid"
            }, {
              name: "selected",
              alias: "selected",
              type: "string"
            }
          ];
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "selected",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [{
              // All features with value of "East" will be green
              value: "Selected",
              symbol: opts.style
            },{
              // All features with value of "East" will be green
                value: "Existing",
                symbol: polygondisabledselectedsymbol
              },
              {
                // All features with value of "East" will be green
                value: "Unselected",
                symbol: polygonnotselectedsymbol
              }]
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var rings = coordinates[j];
              var polyGon = {
                type: "polygon",
                rings: rings,
              };
              var attributes = geometry.attributes;
              var object = new Object();
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
              }
              object["ObjectID"] = geometry.id; 
              var polyGonGraphic = new Graphic({
                geometry: polyGon,
                attributes: object,
              });
              graphics.push(polyGonGraphic);
            }
          }
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              source: graphics,
              labelingInfo: labelClass,
              labelsVisible:true,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              renderer: renderer
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          resolve(layer);
        });
    });
  } 
 
  addGeometryBMPTypePloyLineGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          var linenotselectedsymbol = {
            type: "simple-line",
            color: "blue",
            width: "2px",
            style: "dot"
          };
          var linedisabledselectedsymbol = {
            type: "simple-line",
            color: "grey",
            width: "2px",
            style: "solid"
          };
          var fields = [
            {
              name: "ObjectID",
              alias: "ObjectID",
              type: "oid"
            }, {
              name: "Selected",
              alias: "Selected",
              type: "string"
            }
          ];
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selected == 'Multiple', 'M', '') ",
            }
          };
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "Selected",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [{
              // All features with value of "North" will be blue
              value: "Selected",
              symbol: opts.style
            }, {
              // All features with value of "East" will be green
              value: "Existing",
                symbol: linedisabledselectedsymbol
            },
            {
              // All features with value of "East" will be green
              value: "Unselected",
              symbol: linenotselectedsymbol
              }]
          }
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              labelingInfo: labelClass,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              renderer: renderer,
              outFields: ["*"]
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var paths = geometry.coordinates;
            var attributes = geometry.attributes;
            var object = new Object();
            for (var k = 0; k < attributes.length; k++) {
              object[attributes[k].name] = attributes[k].value;
            }
            var polyline = {
              type: "polyline", // autocasts as new Polyline()
              paths: paths
            };
            var polylineGraphic = new Graphic({
              geometry: polyline,
              attributes: object,
              symbol: geometry["style"],
            });
            graphics.push(polylineGraphic);
          }
          layer.source = graphics;
          resolve(layer);
        });
    });
  }
  addGeometryBMPTypeMutiPointsGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var pointnotselectedsymbol = {
            type: "simple-marker",  // autocasts as new SimpleMarkerSymbol()
            style: "square",
            color: "rgba(255,255,255,0.1)",
            size: "8px",  // pixels
            outline: {  // autocasts as new SimpleLineSymbol()
              color: [255, 255, 0],
              width: 3  // points
            }
          };
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selected == 'Multiple', 'M', '') ",
            }
          };
          var pointdisabledselectedsymbol = {
            type: "simple-marker",  // autocasts as new SimpleMarkerSymbol()
            style: "square",
            color: "grey",
            size: "8px",  // pixels
            outline: {  // autocasts as new SimpleLineSymbol()
              color: [255, 255, 0],
              width: 3  // points
            }
          };
          var fields = [
            {
              name: "ObjectID",
              alias: "ObjectID",
              type: "oid"
            }, {
              name: "Selected",
              alias: "Selected",
              type: "string"
            }
          ]
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "Selected",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [{
              // All features with value of "North" will be blue
              value: "Selected",
              symbol: opts.style
            }, {
              // All features with value of "East" will be green
              value: "Existing",
                symbol: pointdisabledselectedsymbol
            },
            {
              // All features with value of "East" will be green
              value: "Unselected",
              symbol: pointnotselectedsymbol
              }]
          }
          var layer = opts.map.findLayerById(opts.layname);
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              labelingInfo: labelClass,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              outFields: ["*"],
              renderer: renderer
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var points = coordinates[j];
              var point = {
                type: "point",
                latitude: points[0],
                longitude: points[1],
              };
              var attributes = geometry.attributes;
              var object = new Object();
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
              }
              var pointgraphic = new Graphic({
                geometry: point,
                attributes: object,
                symbol: geometry["style"]
              });
              graphics.push(pointgraphic);
            }
          }
          layer.source = graphics;
          resolve(layer);
        });
    });
  }


  addGeometryBMPTypeListMutiPolyGonGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    styledic: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selectedstatus == 'Multiple', 'M', '') ",
            }
          };
          var polygonnotselectedsymbol = {
            type: "simple-fill",  // autocasts as new SimpleFillSymbol()
            color: "rgba(255,255,255,0.1)",
            style: "solid",
            outline: {  // autocasts as new SimpleLineSymbol()
              color: "grey",
              width: 1
            }
          };
          var polygonMultiplesymbol = {
            type: "simple-fill",  // autocasts as new SimpleFillSymbol()
            color: "rgba(112,48,160,1)",
            style: "solid",
            outline: {  // autocasts as new SimpleLineSymbol()
              color: "grey",
              width: 1
            }
          };
          var polygondisabledselectedsymbol = {
            type: "simple-fill",  // autocasts as new SimpleFillSymbol()
            color: "grey",
            style: "solid",
            outline: {  // autocasts as new SimpleLineSymbol()
              color: "white",
              width: 1
            }
          };
          var fields = [
            {
              name: "ObjectID",
              type: "oid"
            }, {
              name: "selectedstatus",
              alias: "selectedstatus",
              type: "string"
            }
          ];
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "selectedstatus",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [ {
                // All features with value of "North" will be blue
                value: "Existing",
                symbol: polygondisabledselectedsymbol
              },
              {
                // All features with value of "North" will be blue
                value: "Unselected",
                symbol: polygonnotselectedsymbol
              },
              {
              // All features with value of "East" will be green
                value: "Multiple",
                symbol: polygonMultiplesymbol
            }]
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var rings = coordinates[j];
              var polyGon = {
                type: "polygon",
                rings: rings,
              };
              var attributes = geometry.attributes;
              var object = new Object();
              var newstyle = false;
              var styletypename;
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
                if (attributes[k].name == "selectedstatus") {
                  newstyle = this.checkifneedaddstyle(attributes[k].name, attributes[k].value);
                  styletypename = attributes[k].value;
                }
              }
              object["ObjectID"] = geometry.id;
              if (newstyle) {
                var style = opts.styledic[styletypename];
                this.addnewstyletorenderer(styletypename, style, renderer)
              }
              var polyGonGraphic = new Graphic({
                geometry: polyGon,
                attributes: object,
              });
              graphics.push(polyGonGraphic);
            }
          }
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              source: graphics,
              labelingInfo: labelClass,
              labelsVisible: true,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              renderer: renderer
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          resolve(layer);
        });
    });
  }

  addGeometryBMPTypeListPloyLineGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    styledic: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var layer = opts.map.findLayerById(opts.layname);
          var lineMultiplesymbol = {
            type: "simple-line",
            color: "rgba(112,48,160,1)",
            width: "2px",
            style: "dot"
          };
          var linedisabledselectedsymbol = {
            type: "simple-line",
            color: "grey",
            width: "2px",
            style: "solid"
          };

          var linenotselectedsymbol = {
            type: "simple-line",
            color: "rgba(255,255,255,0.1)",
            width: "2px",
            style: "dot"
          };

          var fields = [
            {
              name: "ObjectID",
              alias: "ObjectID",
              type: "oid"
            }, {
              name: "selectedstatus",
              alias: "selectedstatus",
              type: "string"
            }
          ];
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selectedstatus == 'Multiple', 'M', '') ",
            }
          };
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "selectedstatus",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [ {
                // All features with value of "North" will be blue
                value: "Existing",
                symbol: linedisabledselectedsymbol
              },{
              // All features with value of "East" will be green
                value: "Multiple",
                symbol: lineMultiplesymbol
              },
              {
                // All features with value of "East" will be green
                value: "Unselected",
                symbol: linenotselectedsymbol
              }
			  ]
          }
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              labelingInfo: labelClass,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              renderer: renderer,
              outFields: ["*"]
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var paths = geometry.coordinates;
            var attributes = geometry.attributes;
            var object = new Object();
            var newstyle = false;
            var styletypename;
            for (var k = 0; k < attributes.length; k++) {
              object[attributes[k].name] = attributes[k].value;
              if (attributes[k].name == "selectedstatus") {
                newstyle = this.checkifneedaddstyle(attributes[k].name, attributes[k].value);
                styletypename = attributes[k].value;
              }
            }
            if (newstyle) {
              var style = opts.styledic[styletypename];
              this.addnewstyletorenderer(styletypename, style, renderer)
            }
            var polyline = {
              type: "polyline", // autocasts as new Polyline()
              paths: paths
            };
            var polylineGraphic = new Graphic({
              geometry: polyline,
              attributes: object,
              symbol: geometry["style"],
            });
            graphics.push(polylineGraphic);
          }
          layer.source = graphics;
          resolve(layer);
        });
    });
  }
  addGeometryBMPTypeListMutiPointsGeomeryLayer(opts: {
    layname: string;
    geometries: any;
    style: any;
    map: any;
    view: any;
    styledic: any;
    visible: boolean
  }): Promise<any[]> {
    return new Promise((resolve) => {
      loadModules(["esri/Graphic", "esri/layers/FeatureLayer"])
        .then(([Graphic, FeatureLayer]) => {
          var pointMultiplesymbol = {
            type: "simple-marker",  // autocasts as new SimpleMarkerSymbol()
            style: "square",
            color: "rgba(112,48,160,1)",
            size: "8px",  // pixels
            outline: {  // autocasts as new SimpleLineSymbol()
              color: [255, 255, 0],
              width: 3  // points
            }
          };
          var pointdisabledselectedsymbol = {
            type: "simple-marker",  // autocasts as new SimpleMarkerSymbol()
            style: "square",
            color: "grey",
            size: "8px",  // pixels
            outline: {  // autocasts as new SimpleLineSymbol()
              color: [255, 255, 0],
              width: 3  // points
            }
          };
          var pointnotselectedsymbol = {
            type: "simple-marker",  // autocasts as new SimpleMarkerSymbol()
            style: "square",
            color: "rgba(255,255,255,0.1)",
            size: "8px",  // pixels
            outline: {  // autocasts as new SimpleLineSymbol()
              color: [255, 255, 0],
              width: 3  // points
            }
          };
          const labelClass = {
            // autocasts as new LabelClass()
            // autocasts as new LabelClass()
            symbol: {
              type: "text",  // autocasts as new TextSymbol()
              color: "black",
              haloSize: 1,
              haloColor: "white"
            },
            labelExpressionInfo: {
              expression: "IIF($feature.selectedstatus == 'Multiple', 'M', '') ",
            }
          };
          var fields = [
            {
              name: "ObjectID",
              alias: "ObjectID",
              type: "oid"
            }, {
              name: "selectedstatus",
              alias: "selectedstatus",
              type: "string"
            }
          ]
          var renderer = {
            type: "unique-value",  // autocasts as new UniqueValueRenderer()
            field: "selectedstatus",
            defaultSymbol: opts.style,  // autocasts as new SimpleFillSymbol()
            uniqueValueInfos: [ {
                // All features with value of "North" will be blue
                value: "Existing",
                symbol: pointdisabledselectedsymbol
              }, {
              // All features with value of "East" will be green
                value: "Multiple",
                symbol: pointMultiplesymbol
              },
              {
                // All features with value of "East" will be green
                value: "Unselected",
                symbol: pointnotselectedsymbol
              }]
          }
          var layer = opts.map.findLayerById(opts.layname);
          if (layer == null) {
            var layer = new FeatureLayer({
              id: opts.layname,
              fields: fields,
              labelingInfo: labelClass,
              objectIdField: "ObjectID",  // field name of the Object IDs
              geometryType: "polygon",
              outFields: ["*"],
              renderer: renderer
            });
            layer.visible = opts.visible;
            opts.map.add(layer);
          }
          var graphics = [];
          for (var i = 0; i < opts.geometries.length; i++) {
            var geometry = opts.geometries[i];
            var coordinates = geometry.coordinates;
            for (var j = 0; j < coordinates.length; j++) {
              var points = coordinates[j];
              var point = {
                type: "point",
                latitude: points[0],
                longitude: points[1],
              };
              var attributes = geometry.attributes;
              var object = new Object();
              var newstyle = false;
              var styletypename;
              for (var k = 0; k < attributes.length; k++) {
                object[attributes[k].name] = attributes[k].value;
                if (attributes[k].name == "selectedstatus") {
                  newstyle = this.checkifneedaddstyle(attributes[k].name, attributes[k].value);
                  styletypename = attributes[k].value;
                }
              }
              if (newstyle) {
                var style = opts.styledic[styletypename];
                this.addnewstyletorenderer(styletypename, style, renderer)
              }
              var pointgraphic = new Graphic({
                geometry: point,
                attributes: object,
                symbol: geometry["style"]
              });
              graphics.push(pointgraphic);
            }
          }
          layer.source = graphics;
          resolve(layer);
        });
    });
  }

  checkifneedaddstyle(attributename, attributevalue) {
    if (attributename == "selectedstatus"
      && attributevalue != "Existing"
      && attributevalue != "Unselected"
      && attributevalue != "Multiple")
      return true;
    return false;
  }

  addnewstyletorenderer(styletypevalue, style, renderer) {
    var uniqueValueInfos = renderer.uniqueValueInfos;
    for (var i = 0; i < uniqueValueInfos.length; i++) {
      if (uniqueValueInfos[i].value == styletypevalue) {
         return;
      }
    }
    var newrenderervalue = {
      value: styletypevalue,
      symbol: style
    }
    uniqueValueInfos.push(newrenderervalue);
  }
  
  centerMap(opts: {
    view: any;
    centerPoint: any;
  }) {
    opts.view.center = opts.centerPoint;
  }

    setExtent(opts: {
        view: any;
        extent: any;
    }) {
        opts.view.extent = opts.extent;
    }
}
