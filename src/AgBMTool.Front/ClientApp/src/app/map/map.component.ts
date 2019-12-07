import { Component, OnInit, Input, Injectable } from '@angular/core';
import { ArcgisApiService } from './arcgis-api.service';
import { MapService } from './map.service';
import { MessageService } from '../services/index';
import { UserOverviewComponent } from '../home/userOverview/userOverview.component'
import { loadModules, loadScript } from 'esri-loader';
import { FilterModel } from '../models/LayerFiler';
import { LayerLoad } from '../models/LayerLoad';
import { BehaviorSubject } from 'rxjs';
import { Subscription } from 'rxjs/Subscription';
import html2canvas from 'html2canvas';
import * as jsPDF from 'jspdf';

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css'],
  providers: [MapService]
})

export class MapComponent implements OnInit {
  public sceneView: any;
  private map: any;
  private layerproperties: Object = new Object();
  private currentsummarizationtype: String;
  private currentcompoentlayer: String;
  public hiddentreeview: boolean = true;
  private previousselectedgraphicId: any = null;
  private previoussymbol: any;
  private hightlightgraphic: any;
  subscription: Subscription;
  private layersymbol: Map<String, String> = new Map<String, String>();
  constructor(private arcgisService: ArcgisApiService, public mapService: MapService, private messageservice: MessageService) {
    this.subscription = this.messageservice.getHiddentreeviewMessage().subscribe(message => {
      if (message) {
        this.hiddentreeview = message.show;
        console.log("map = " + this.hiddentreeview);
      }
    });
  }

  ngOnInit() {
    // this.sendMessage();
  }

  public showtreeviewfunction(): void {
    this.hiddentreeview = !this.hiddentreeview;
    this.messageservice.sendshowtreeviewMessage(!this.hiddentreeview);
  }
  getUserGeometryData(opts) {
    const properties = Object.keys(opts);
    this.layerproperties = new Object();
    this.currentsummarizationtype = opts.selectedsummarzationType;
    var summarzationType = opts.selectedsummarzationType;
    var tmpmessageservice = this.messageservice;
    this.previousselectedgraphicId = null;
    properties.forEach(prop => {
      var visible = opts[prop];
      if (visible === true || visible === false) {
        this.layerproperties[prop] = visible;
      }
    });
    if (this.map == null || this.sceneView == null) {
      this.arcgisService.loaded$.subscribe(loaded => {
        if (loaded) {
          this.arcgisService.constructMap({ basemap: "satellite", elevation: true }).then(map => {
            this.map = map;
            this.arcgisService.constructMapView({ map: this.map, container: "map", center: [1, 45], zoom: 13 }).then(sceneView => {
              this.sceneView = sceneView;
              var tmp = this.sceneView;
              sceneView.on("click", function (evt) {
                var screenPoint = evt.screenPoint;
                // the hitTest() checks to see if any graphics in the view
                // intersect the given screen point
                sceneView.hitTest(screenPoint)
                  .then(function (response) {
                    console.info(response.results);

                    var graphic = response.results[0].graphic;
                    var layer = graphic.layer;
                    var layerId = layer.id;
                    if (layerId == summarzationType) {
                      var graphics = layer.graphics.items;
                      for (var i = 0; i < graphics.length; i++) {
                        if (graphics[i].attributes["hightlight"] != null) {
                          graphics[i].attributes["hightlight"].remove();
                          graphics[i].attributes["hightlight"] = null;
                          break;
                        }
                      }
                      var highlight;
                      sceneView.whenLayerView(layer).then(function (layerView) {
                        highlight = layerView.highlight(graphic);
                      });
                      graphic.attributes["hightlight"] = highlight;
                      tmpmessageservice.sendClickedSummaryLayerMessage(summarzationType,
                        graphic.attributes.Id);
                    }

                  });
              });
              this.drawGemetryData(opts);
            });
          })
        }
      })
    } else {
      this.drawGemetryData(opts);
    }
  }

  drawGemetryData(opts) {
    this.map.allLayers.find(function (layer) {
      if (layer.type == "graphics") {
        layer.removeAll();
      }
    });
 //   this.map.removeMany(layers);
    const properties = Object.keys(opts);
    var filter = new FilterModel();
    filter.MunicipalityId = opts["MunicipalityId"];
    filter.WatershedId = opts["WatershedId"];
    filter.SubwatershedId = opts["SubwatershedId"];

    properties.forEach(prop => {
      var visible = opts[prop];
      if (visible == false || visible == true) {
        this.generateMapLayerByname(prop, visible, filter);
      }
    });
    this.setMapExtent();

  }

  generateMapLayerByname(LayerId, visible, filter) {
    switch (LayerId) {
      case "LSD":
        this.getLSDGeometryData(visible, filter);
        break;
      case "Reach":
        this.getReachGeometryData(visible, filter);
        break;
      case "Parcel":
        this.getParcelGeometryData(visible, filter);
        break;
      case "Farm":
        this.getFarmGeometryData(visible, filter);
        break;
      case "Municipality":
        this.getMunicipalityGeometryData(visible, filter);
        break;
      case "SubWaterShed":
        this.getSubWaterShedGeometryData(visible, filter);
        break;
      case "WaterShed":
        this.getWaterShedGeometryData(visible, filter);
    }
  }

  updatebmpselectedstatus(graphicIds, selected) {
    var layer = this.map.findLayerById(this.currentcompoentlayer);
    var graphics = layer.source.items;
    var updatefeatures = [];
    for (var i = 0; i < graphics.length; i++) {
      if (graphicIds.indexOf(graphics[i].attributes.ObjectID) > -1) {
        graphics[i].attributes.selected = selected == true ? "Selected" : "Unselected";
        updatefeatures.push(graphics[i]);
      }
    }
    layer.applyEdits({
      updateFeatures: updatefeatures,
    });

  }

  updatesinglebmpselectedstatus(graphicId, selected) {
    var layer = this.map.findLayerById(this.currentcompoentlayer);
    var graphics = layer.source.items;
    var updatefeatures = [];
    for (var i = 0; i < graphics.length; i++) {
      if (graphics[i].attributes["hightlight"] != null) {
        graphics[i].attributes["hightlight"].remove();
        graphics[i].attributes["hightlight"] = null;
        break;
      }
    }
    for (var i = 0; i < graphics.length; i++) {
      if (graphicId == graphics[i].attributes.ObjectID) {
        graphics[i].attributes.selected = selected == true ? "Selected" : "Unselected";
        updatefeatures.push(graphics[i]);
        break;
      }
    }
    layer.applyEdits({
      updateFeatures: updatefeatures,
    });

  }

  getBMPTypeGeometryData(projectId, bmptypeId, bmptypeIds, isOptimization) {
    if (this.map == null || this.sceneView == null) {
      var tmpmessageservice = this.messageservice;
      this.arcgisService.loaded$.subscribe(loaded => {
        if (loaded) {
          this.arcgisService.constructMap({ basemap: "satellite", elevation: true }).then(map => {
            this.map = map;
            this.arcgisService.constructMapView({ map: this.map, container: "map", center: [1, 45], zoom: 13 }).then(sceneView => {
              this.sceneView = sceneView;
              var tmp = this.sceneView;
              sceneView.on("click", function (evt) {
                var screenPoint = evt.screenPoint;
                // the hitTest() checks to see if any graphics in the view
                // intersect the given screen point
                sceneView.hitTest(screenPoint)
                  .then(getGraphics);
              });

              function getGraphics(response) {
                // the topmost graphic from the click location
                // and display select attribute values from the
                // graphic to the user
                console.info(response.results);

                var graphic = response.results[0].graphic;
                if (graphic.attributes.hasOwnProperty("selectedstatus"))
                  return;
                var layer = graphic.layer;
                var updatefeatures = [];
                if (layer.type == "feature") {
                  if (graphic.attributes["selected"] == "Selected") {
                    graphic.attributes["selected"] = "Unselected";
                  }
                  else if (graphic.attributes["selected"] == "Unselected") {
                    graphic.attributes["selected"] = "Selected";
                  }
                  updatefeatures.push(graphic);
                  if (isOptimization)
                    tmpmessageservice.sendSelectedGraphicMessage(graphic.attributes["selected"] != "Selected",
                      graphic.attributes.ObjectID);
                  else
                    tmpmessageservice.sendSelectedandoverviewGraphicMessage(graphic.attributes["selected"] != "Selected",
                      graphic.attributes.ObjectID);
                  layer.applyEdits({
                    updateFeatures: updatefeatures,
                  });
                  var graphics = layer.source.items;
                  for (var i = 0; i < graphics.length; i++) {
                    if (graphics[i].attributes["hightlight"] != null) {
                      graphics[i].attributes["hightlight"].remove();
                      graphics[i].attributes["hightlight"] = null;
                      break;
                    }
                  }
                }
              }
              if (bmptypeIds == null || bmptypeIds.length == 0) {
                this.drawBMPTypeGeometries(projectId, bmptypeId, isOptimization);
              } else {
                this.drawBMPTypeListGeometries(projectId, bmptypeIds, isOptimization);
              }
              this.setMapExtent();
              this.getBMPProjectWaterShedsGeometry(projectId);
              this.getBMPProjectMunicipilitiesGeometry(projectId);
              this.getBMPProjectProjectReachesGeometry(projectId);
            
            });
          })
        }
      })
    } else {
      if (this.map != null) {
        var layers = [];
        this.map.allLayers.find(function (layer) {
          if (layer.type == "feature") {
            layers.push(layer);
          }
        });
        this.map.removeMany(layers);
      }
      if (bmptypeIds == null || bmptypeIds.length == 0) {
        this.drawBMPTypeGeometries(projectId, bmptypeId, isOptimization);
      } else {
        this.drawBMPTypeListGeometries(projectId, bmptypeIds, isOptimization);
      }
    }
    
  }

  drawBMPTypeGeometries(projectId,bmptypeId, isOptimization) {
    this.mapService.getUserBMPTypeGeometryData(projectId, bmptypeId, isOptimization).subscribe(rdata => {
      if (rdata != null) {
        var layername;
        layername = rdata.layername;
        this.currentcompoentlayer = layername;
            if (rdata.geometrytype == "MultiPoint") {
              this.arcgisService.addGeometryBMPTypeMutiPointsGeomeryLayer({
                layname: layername,
                geometries: rdata.geometries,
                style: rdata.geometryStyle,
                map: this.map,
                view: this.sceneView,
                visible: true
              });
            }
            if (rdata.geometrytype == "MultiPolygon") {
              this.arcgisService.addGeometryBMPTypeMutiPolyGonGeomeryLayer({
                layname: layername,
                geometries: rdata.geometries,
                style: rdata.geometryStyle,
                map: this.map,
                view: this.sceneView,
                visible: true
              });
            }
            if (rdata.geometrytype == "MultiLineString") {
              this.arcgisService.addGeometryBMPTypePloyLineGeomeryLayer({
                layname: layername,
                geometries: rdata.geometries,
                style: rdata.geometryStyle,
                map: this.map,
                view: this.sceneView,
                visible: true
              });
            }
        var opts = {
          "Municipality": true,
          "Watershed": true,
          "Reach": true
        };
        opts[layername] = true;
       
        const properties = Object.keys(opts);
        properties.forEach(prop => {
          console.log("drawlegend " + prop);
        })
        this.drawlegend(opts);
      }
    });
  }

  drawBMPTypeListGeometries(projectId, bmptypeIds, isOptimization) {
    this.mapService.getBMPTypeGeometrystyledic(bmptypeIds).subscribe(styledic => {
      this.mapService.getUserBMPTyeListGeomtries(projectId, bmptypeIds, isOptimization).subscribe(rdata => {
        if (rdata != null) {
          var opts = {
            "Municipality": true,
            "Watershed": true,
            "Reach": true
          };
          for (var i = 0; i < rdata.length; i++) {
            var layername;
            layername = rdata[i].layername;
            opts[layername] = true;
            if (rdata[i].geometrytype == "MultiPoint") {
              this.arcgisService.addGeometryBMPTypeListMutiPointsGeomeryLayer({
                layname: layername,
                geometries: rdata[i].geometries,
                style: rdata[i].geometryStyle,
                map: this.map,
                view: this.sceneView,
                styledic: styledic,
                visible: true
              });
            }
            if (rdata[i].geometrytype == "MultiPolygon") {
              this.arcgisService.addGeometryBMPTypeListMutiPolyGonGeomeryLayer({
                layname: layername,
                geometries: rdata[i].geometries,
                style: rdata[i].geometryStyle,
                map: this.map,
                view: this.sceneView,
                styledic: styledic,
                visible: true
              });
            }
            if (rdata[i].geometrytype == "MultiLineString") {
              this.arcgisService.addGeometryBMPTypeListPloyLineGeomeryLayer({
                layname: layername,
                geometries: rdata[i].geometries,
                style: rdata[i].geometryStyle,
                map: this.map,
                view: this.sceneView,
                styledic: styledic,
                visible: true
              });
            }
          }
          const properties = Object.keys(opts);
          properties.forEach(prop => {
            console.log("drawlegend " + prop);
          })
          this.drawlegend(opts);
        }
      });
    });
  }

  getBMPProjectWaterShedsGeometry(projectId) {
    this.mapService.getProjectWaterShedsGeometry(projectId).subscribe(rdata => {
      if (rdata != null)
        this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
          layname: "Watershed",
          geometries: rdata,
          map: this.map,
          view: this.sceneView,
          visible: true,
        });

    });
  }

  getBMPProjectMunicipilitiesGeometry(projectId) {
    this.mapService.getProjectMunicipilitiesGeometry(projectId).subscribe(rdata => {
      if (rdata != null)
        this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
          layname: "Municipality",
          geometries: rdata,
          map: this.map,
          view: this.sceneView,
          visible: true,
        });

    });
  }

  getBMPProjectProjectReachesGeometry(projectId) {
    this.mapService.getProjectReachesGeometry(projectId).subscribe(rdata => {
      if (rdata != null)
        this.arcgisService.addGeometryPloyLineGeomeryLayer({
          layname: "Reach",
          geometries: rdata,
          map: this.map,
          view: this.sceneView,
          visible: true
        });
    });
  }

  highlightgeogemtry(opts) {
    var layer = this.map.findLayerById(this.currentsummarizationtype);
    var Id = opts.Id; 
    var graphics = layer.graphics.items;
    var graphics = layer.graphics.items;
    for (var i = 0; i < graphics.length; i++) {
      if (graphics[i].attributes["hightlight"] != null) {
        graphics[i].attributes["hightlight"].remove();
        graphics[i].attributes["hightlight"] = null;
        break;
      }
    }
    for (var i = 0; i < graphics.length; i++) {
      if (graphics[i].attributes["Id"] == Id) {
        var highlight;
        this.sceneView.whenLayerView(layer).then(function (layerView) {
          highlight = layerView.highlight(graphics[i]);
        });
        this.sceneView.extent = graphics[i].geometry.extent.expand(1.5);
        graphics[i].attributes["hightlight"] = highlight;
        break;
      }
    }
 

  }

  highlightfeaturelayergeogemtry(opts) {
    var layer = this.map.findLayerById(this.currentcompoentlayer );
    var Id = opts.Id[0];
    var graphics = layer.source.items;
    for (var i = 0; i < graphics.length; i++) {
      if (graphics[i].attributes["hightlight"] != null) {
        graphics[i].attributes["hightlight"].remove();
        graphics[i].attributes["hightlight"] = null;
        break;
      }
    }
    for (var i = 0; i < graphics.length; i++) {
      if (graphics[i].attributes["Id"] == Id) {
        var highlight;
        this.sceneView.whenLayerView(layer).then(function (layerView) {
           highlight = layerView.highlight(graphics[i]);
        });
        this.hightlightgraphic = highlight;
        graphics[i].attributes["hightlight"] = highlight;
        if (graphics[i].geometry.extent != null)
          this.sceneView.extent = graphics[i].geometry.extent.expand(1.5);
        else {
          this.sceneView.center = [graphics[i].geometry.latitude, graphics[i].geometry.longitude];
          this.sceneView.zoom = 17;
        }
          
        break;
      }
    }

  }

    setMapExtent() {
        this.mapService.getUserMapExtent().subscribe(extent =>
        {
            this.arcgisService.setExtent({ view: this.sceneView, extent: extent });
        });
  }

  getParcelGeometryData(visible, filter) {
    this.mapService.getParcelGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "Parcel",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
    });
  }

  getLSDGeometryData(visible, filter) {
    this.mapService.getLSDGeometryData(filter).subscribe(rdata => {
      if(rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "LSD",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
    });
  }

  getReachGeometryData(visible, filter) {
    this.mapService.getReachGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryPloyLineGeomeryLayer({
        layname: "Reach",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
    });
  }

  getFarmGeometryData(visible, filter) {
    this.mapService.getFarmGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "Farm",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible,
      });
     
    });
  }

  getMunicipalityGeometryData(visible, filter) {
    this.mapService.getMunicipalitiesGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "Municipality",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
    });
  }

  getSubWaterShedGeometryData(visible, filter) {
    this.mapService.getUserSubWaterShedGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "SubWaterShed",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
      
    });
  }

  getWaterShedGeometryData(visible, filter) {
    this.mapService.getWaterShedGeometryData(filter).subscribe(rdata => {
      if (rdata != null)
      this.arcgisService.addGeometryMutiPolyGonGeomeryLayer({
        layname: "WaterShed",
        geometries: rdata,
        map: this.map,
        view: this.sceneView,
        visible: visible
      });
    });
  }
  drawlegend(opts) {
    loadModules(["dojo/dom-construct", "dojo/on", "dojo/dom","dojo/domReady!"])
      .then(([domConstruct, on, dom]) => {
        domConstruct.empty("legenddropdown");
        const properties = Object.keys(opts);
        properties.forEach(prop => {
          var visible = opts[prop];
          console.log(prop);
          if (visible) {
            var legnednode = dom.byId("legenddropdown");
            if (this.map == null || this.map.findLayerById(prop) == null || (this.map.findLayerById(prop).type == "graphics" && this.map.findLayerById(prop).graphics.length == 0) ) {
              var counter = 5;
              var t = setInterval(() => {
                if (this.map != null && this.map.findLayerById(prop) != null && (this.map.findLayerById(prop).type == "graphics" && this.map.findLayerById(prop).graphics.length > 0)) {
                  if (counter == 0)
                    clearInterval(t);
                  var map = this.map;
                  var linode = domConstruct.toDom("<li></li>");
                  domConstruct.place(linode, legnednode);
                  var labelnode = domConstruct.toDom("<label class='checkbox' style='padding:5px;'></label>");
                  domConstruct.place(labelnode, linode);
                  var checkboxnode = domConstruct.toDom("<input type='checkbox' checked value =" + prop + " />");
                  on(checkboxnode, 'click', function (element) {
                    var layer = map.findLayerById(element.target.value)
                    layer.visible = element.target.checked;
                  });
                  var layer = map.findLayerById(prop);
                  layer["visible"] = true;
                  domConstruct.place(checkboxnode, labelnode);
                  if (layer.type == "graphics") {
                    var symbol = layer.graphics.items[0].symbol;
                    this.drawgraphic(symbol, prop, labelnode);
                  } else if (layer.type == "feature") {
                    this.drawfeaturelayerrenderer(layer.renderer, prop, labelnode);
                  }
                  clearInterval(t);
                  counter--;
                }
              }, 500);
            }
            else {
              var map = this.map;
              var linode = domConstruct.toDom("<li></li>");
              domConstruct.place(linode, legnednode);
              var labelnode = domConstruct.toDom("<label class='checkbox' style='padding:5px;'></label>");
              domConstruct.place(labelnode, linode);
              var checkboxnode = domConstruct.toDom("<input type='checkbox' checked value =" + prop + " />");
              on(checkboxnode, 'click', function (element) {
                var layer = map.findLayerById(element.target.value)
                layer.visible = element.target.checked;
              });
              var layer = map.findLayerById(prop);
              layer["visible"] = true;
              domConstruct.place(checkboxnode, labelnode);
              if (layer.type == "graphics") {
                var symbol = layer.graphics.items[0].symbol;
                this.drawgraphic(symbol, prop, labelnode);
              } else if (layer.type == "feature") {
                this.drawfeaturelayerrenderer(layer.renderer, prop, labelnode);
              }
            }
          }
          else {
            var legnednode = dom.byId("legenddropdown");
            if (this.map == null || this.map.findLayerById(prop) == null || (this.map.findLayerById(prop).type == "graphics" && this.map.findLayerById(prop).graphics.length == 0)) {
              var t = setInterval(() => { 
                if (this.map != null && this.map.findLayerById(prop) != null && (this.map.findLayerById(prop).type == "graphics" && this.map.findLayerById(prop).graphics.length > 0)) {
                  var map = this.map;
                  var linode = domConstruct.toDom("<li></li>");
                  domConstruct.place(linode, legnednode);
                  var labelnode = domConstruct.toDom("<label class='checkbox' style='padding:5px;'></label>");
                  domConstruct.place(labelnode, linode);
                  var checkboxnode = domConstruct.toDom("<input type='checkbox' value =" + prop + " />");
                  on(checkboxnode, 'click', function (element) {
                    var layer = map.findLayerById(element.target.value)
                    layer.visible = element.target.checked;
                  });
                  var layer = map.findLayerById(prop);
                  layer["visible"] = false;
                  domConstruct.place(checkboxnode, labelnode);
                  if (layer.type == "graphics") {
                    var symbol = layer.graphics.items[0].symbol;
                    this.drawgraphic(symbol, prop, labelnode);
                  } else if (layer.type == "feature") {
                    this.drawfeaturelayerrenderer(layer.renderer, prop, labelnode);
                  }
                  clearInterval(t);
                }
              }, 500);
            }
            else {
             var map = this.map;
             var linode = domConstruct.toDom("<li></li>");
             domConstruct.place(linode, legnednode);
              var labelnode = domConstruct.toDom("<label class='checkbox' style='padding:5px;'></label>");
             domConstruct.place(labelnode, linode);
              var checkboxnode = domConstruct.toDom("<input type='checkbox' value =" + prop + " />");
             on(checkboxnode, 'click', function (element) {
                var layer = map.findLayerById(element.target.value)
                layer.visible = element.target.checked;
             });
              var layer = map.findLayerById(prop);
              layer["visible"] = false;
              domConstruct.place(checkboxnode, labelnode);
              if (layer.type == "graphics") {
                var symbol = layer.graphics.items[0].symbol;
                this.drawgraphic(symbol, prop, labelnode);
              } else if (layer.type == "feature") {
                this.drawfeaturelayerrenderer(layer.renderer, prop, labelnode);
              }
            }
          }
        }); 
      })
  }

  drawgraphic(symbol,nodename, parentnode) {
    loadModules(["dojo/dom-construct",'dojox/gfx', "dojo/domReady!"])
      .then(([domConstruct, gfx]) => {
        var node = domConstruct.toDom("<span style='font-size: 15px; padding-left: 5px;'></span>");
        if (symbol == null) {
          console.log("nodename = " + nodename);
        }
        var symboltype = symbol.type;
        if (symboltype == "point") {
          domConstruct.place(node, parentnode);
          var mySurface = gfx.createSurface(node, 10, 10);
          mySurface.createCircle({ cx: 5, cy: 5, r: 5 }).setFill(symbol.color).setStroke( symbol.outline.color);

        }
        else if (symboltype == "simple-line") {
          domConstruct.place(node, parentnode);
          var mySurface = gfx.createSurface(node, 10, 10);
          mySurface.createLine({ x1: 0, y1: 0, x2: 10, y2: 10 })
            .setStroke(symbol.color);
        }
        else {
          domConstruct.place(node, parentnode);
          var mySurface = gfx.createSurface(node, 10, 10);
          mySurface.createRect({ x: 0, y: 0, width: 10, height: 10 })
            .setFill(symbol.color).setStroke( symbol.outline.color);
        }
        var text = domConstruct.toDom("<span style='font-size: 15px; padding-left: 5px;'>" + nodename + "</span>");
        domConstruct.place(text, parentnode);
      })
  }

  public takescreenshot() {
       html2canvas(document.getElementById('map')).then(function (canvas) {
         var img = canvas.toDataURL("image/png");
         var doc = new jsPDF();
         doc.addImage(img, 'JPEG', 5, 20);
         doc.save('testCanvas.pdf');
       });
  }

  public downloadImage(dataUrl) {
    // the download is handled differently in Microsoft browsers
    // because the download attribute for <a> elements is not supported
    if (!window.navigator.msSaveOrOpenBlob) {
      // in browsers that support the download attribute
      // a link is created and a programmatic click will trigger the download
      const element = document.createElement("a");
      element.setAttribute("href", dataUrl);
      element.setAttribute("download", "map");
      element.style.display = "none";
      document.body.appendChild(element);
      element.click();
      document.body.removeChild(element);
    } else {
      // for MS browsers convert dataUrl to Blob
      const byteString = atob(dataUrl.split(",")[1]);
      const mimeString = dataUrl
        .split(",")[0]
        .split(":")[1]
        .split(";")[0];
      const ab = new ArrayBuffer(byteString.length);
      const ia = new Uint8Array(ab);
      for (let i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
      }
      const blob = new Blob([ab], { type: mimeString });

      // download file
      window.navigator.msSaveOrOpenBlob(blob, "map");
    }
  }

  drawfeaturelayerrenderer(renderers, nodename, parentnode) {
    loadModules(["dojo/dom-construct", 'dojox/gfx', "dojo/domReady!"])
      .then(([domConstruct, gfx]) => {
        var text = domConstruct.toDom("<span style='font-size: 15px; padding-left: 5px;'>" + nodename + "</span> <br/>");
        domConstruct.place(text, parentnode);
        var uniquevalues = renderers.uniqueValueInfos;
        for (var i = 0; i < uniquevalues.length; i++) {
          var node = domConstruct.toDom("<span style='font-size: 15px; padding-left: 15px;'></span>");
          var value = uniquevalues[i].value;
          var symbol = uniquevalues[i].symbol;
          var symboltype = symbol.type;
          if (symboltype == "point") {
            domConstruct.place(node, parentnode);
            var mySurface = gfx.createSurface(node, 10, 10);
            mySurface.createCircle({ cx: 5, cy: 5, r: 5 }).setFill(symbol.color).setStroke(symbol.outline.color);

          }
          else if (symboltype == "simple-line") {
            domConstruct.place(node, parentnode);
            var mySurface = gfx.createSurface(node, 10, 10);
            mySurface.createLine({ x1: 0, y1: 0, x2: 10, y2: 10 })
              .setStroke(symbol.color);
          }
          else {
            domConstruct.place(node, parentnode);
            var mySurface = gfx.createSurface(node, 10, 10);
            mySurface.createRect({ x: 0, y: 0, width: 10, height: 10 })
              .setFill(symbol.color).setStroke(symbol.outline.color);
          }
          var text = domConstruct.toDom("<span style='font-size: 15px; padding-left: 5px;'>" + value + "</span><br/>");
          domConstruct.place(text, parentnode);
        }       
      })
  }
}
