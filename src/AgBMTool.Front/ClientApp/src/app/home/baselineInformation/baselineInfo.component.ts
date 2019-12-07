import { Component, ViewEncapsulation, ElementRef } from '@angular/core';
import { BaselineInfoService } from './baselineInfo.service';
import { Options } from 'ng5-slider';
import { ActivatedRoute } from '@angular/router';
import { ArcgisApiService } from '../../map/arcgis-api.service';
import { MapService } from '../../map/map.service';
import { MapComponent } from '../../map/map.component';
import { BaselineBMPSummaryDataModel } from '../../models/BaselineBMPSummaryDataModel';
import { PageChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { process, State, aggregateBy } from '@progress/kendo-data-query';
import { MessageService } from '../../services/index';
import { Subscription } from 'rxjs';
interface Item {
    ItemName: string,
    ItemId: number
};

@Component({
    selector: 'app-baselineInfo',
    templateUrl: './baselineInfo.component.html',
    styleUrls: ['./baselineInfo.component.css'],
    encapsulation: ViewEncapsulation.None
})

export class BaselineInfoComponent {
    private map: MapComponent;

    bmpSummaryDataModel: BaselineBMPSummaryDataModel = new BaselineBMPSummaryDataModel();

    public buttonText = '>';
    public displayMiddleDivClass = 'col-lg-9';
    public displayRightDivClass = 'col-lg-3';

    private projectId: any;
    private sub: any;

    private baseLineInfoGridHeight: number;
    private baseLineInfoMapHeight: number;

    private baselineBMPSummaryGridHeight: number;
    private baselineBMPEffectivenessGridHeight: number;

    private summaryGridColumns: any[];
    private summaryGridData: any[];
    private summaryGridDataAll: any;
    private next: number;
    private previousSelectRowById: number = 0;
    private selectRowById = [];

    public effectivenessGridView: GridDataResult;
    public baselineBMPEffectivenessGridData: any[];

    public summarizationDropdownSelectedValue: number = 2;

    public showLocationFilter: boolean = true;

    public summarizationTypeList: any[];
    summarizationTypeSelectedItem: any = 0;

    public municipalityList: any[];
    municipalitySelectedItem: any = -1;

    public watershedList: any[];
    watershedSelectedItem: any = -1;

    public subWatershedList: any[];
    subWatershedSelectedItem: any = -1;

    public baselineBMPSummaryGridData: any[];

    public totalCostBMPSummaryGridData: any = 0;
    subscription: Subscription;
    private progressBarSummaryGridData: boolean = true;

    constructor(private baselineInfoService: BaselineInfoService, private route: ActivatedRoute, private arcgisService: ArcgisApiService, private mapService: MapService,
        private messageservice: MessageService, private elRef: ElementRef) {

        this.baseLineInfoMapHeight = window.innerHeight - 305;
        this.baseLineInfoGridHeight = window.innerHeight - (window.innerHeight - 342) - 150;
        this.map = new MapComponent(arcgisService, mapService, messageservice);

        this.baselineBMPSummaryGridHeight = ((this.baseLineInfoMapHeight + this.baseLineInfoGridHeight) * 30 / 100) - 14;
        this.baselineBMPEffectivenessGridHeight = ((this.baseLineInfoMapHeight + this.baseLineInfoGridHeight) * 30 / 100) - 14;

      this.subscription = this.messageservice.getClickedSummaryLayerMessage().subscribe(message => {
        if (message) {
          this.SelectOverviewGridRowById(message.Id);
        }
      });
    }


    public ngOnInit(): void {

        this.sub = this.route.params.subscribe(params => {
            this.ParamsChanged(params['id']); // (+) converts string 'id' to a number
        });

        // Get summarization types And Load BMP Summary and Effectiveness Summary grids
        this.baselineInfoService.GetSummarizationTypeList().subscribe(rdata => {
            this.summarizationTypeList = rdata;
            if (rdata.length > 0) {
                this.summarizationTypeSelectedItem = rdata[0].itemId;
            }
        });

        // Get Municipalities 
        this.baselineInfoService.GetProjectMunicipalitiesByProjectId(this.projectId).subscribe(rdata => {
            this.municipalityList = rdata;
            if (rdata.length > 0) {
                this.municipalityList.splice(0, 0, { name: "All", id: -1 });
                this.municipalitySelectedItem = this.municipalityList[0].id;
                this.municipalityChange(this.municipalityList[0]);
            }
        });

        this.LoadLegend();
    }

    private ngOnDestroy() {
        this.sub.unsubscribe();
    }

    public ParamsChanged(id) {
        this.projectId = parseInt(id);
        this.LoadBMPSummaryGridData();
        this.LoadEffectivenessGridData();
    }

    public LoadBMPSummaryGridData() {
        this.baselineInfoService.GetBaselineBMPSummaryGridData(this.projectId).subscribe(rdata => {
            this.baselineBMPSummaryGridData = rdata;
            var totalCost = 0;
            this.baselineBMPSummaryGridData.forEach(function (num) { totalCost += num.cost || 0; });
            this.totalCostBMPSummaryGridData = totalCost;
        });
    }

    public LoadEffectivenessGridData() {
        this.baselineInfoService.GetBaselineBMPEffectivenessGridData(this.projectId).subscribe(rdata => {
            this.baselineBMPEffectivenessGridData = rdata;
        });
    }

    public LoadLegend() {
        var summarizationLogicForMapLayers = this.GetMapLayerLegendItemsToShow();
        this.map.drawlegend(summarizationLogicForMapLayers);
    }

    public LoadSummaryGridData() {

        this.progressBarSummaryGridData = false;

        this.baselineInfoService.GetSummaryGridData(this.summarizationDropdownSelectedValue, this.municipalitySelectedItem,
            this.watershedSelectedItem, this.subWatershedSelectedItem, this.projectId).subscribe(rdata => {

                rdata.summaryTableColumns.forEach(p => p.fieldName = p.fieldName.toLowerCase());   // Set first character to lower case to match with datatable column

                this.summaryGridColumns = rdata.summaryTableColumns;
                this.summaryGridDataAll = rdata.summaryTableData;

                this.next = this.summaryGridDataAll.length > 10 ? 10 : this.summaryGridDataAll.length;
                this.summaryGridData = this.summaryGridDataAll.slice(0, this.next);
                
                this.progressBarSummaryGridData = true;
            });
    }

    private loadMore(): void {
        let scrollTop = this.elRef.nativeElement.getElementsByClassName('k-grid-content')[0].scrollHeight;

        if (this.next < this.summaryGridDataAll.length) {
            this.next = this.summaryGridDataAll.length > this.next + 10 ? this.next + 10 : this.summaryGridDataAll.length;
            this.summaryGridData = this.summaryGridData.concat(this.summaryGridDataAll.slice(this.next - 10, this.next));
        }

        setTimeout(() => document.getElementsByClassName("k-grid-content").item(0).scrollTo({ top: scrollTop }));
    }

    // Select grid row
    public SelectOverviewGridRowById(resultId) {
        this.selectRowById = [];
        this.selectRowById.push(resultId);
    }

    public onSelectedKeysChange(e) {

        var spacialScope = this.summarizationTypeList.filter(x => x.itemId == this.summarizationDropdownSelectedValue)[0].itemName;
        var optHighlight = { "layername": spacialScope, "Id": this.selectRowById };
        this.map.highlightgeogemtry(optHighlight);

        if (this.previousSelectRowById > 0) {
            var optUnhighlight = { "layername": spacialScope, "Id": this.previousSelectRowById };
          //  this.map.unhighlightgeogemtry(optUnhighlight);
            this.previousSelectRowById = this.selectRowById[0];
        }
    }

    public summarizationTypeChange(value: any): void {

        if (this.summarizationDropdownSelectedValue != null && this.summarizationDropdownSelectedValue != undefined) {

            // As we need to visible 'Location Filter', when 'Summarization level' is only  in LSD (1), Parcel (2), and Farm level (3)
            if (this.summarizationDropdownSelectedValue <= 3) {
                this.showLocationFilter = true;
            }
            else {
                this.showLocationFilter = false;
            }

            this.LoadSummaryGridData();
            this.LoadLegend();
        }

    }

    public municipalityChange(value: any): void {

        this.baselineInfoService.GetProjectWatershedsByMunicipality(this.projectId, this.municipalitySelectedItem).subscribe(rdata => {
            this.watershedList = rdata;
            if (rdata.length > 0) {
                this.watershedList.splice(0, 0, { name: "All", id: -1 });
                this.watershedSelectedItem = this.watershedList[0].id;
                this.watershedChange(this.watershedList[0]);
            }
        });
    }

    public watershedChange(value: any): void {

        this.baselineInfoService.GetSubWatershedsByWatershedId(this.municipalitySelectedItem, this.watershedSelectedItem).subscribe(rdata => {
            this.subWatershedList = rdata;
            if (rdata.length > 0) {
                this.subWatershedList.splice(0, 0, { name: "All", id: -1 });
                this.subWatershedSelectedItem = this.subWatershedList[0].id;
                this.subWatershedChange(this.subWatershedSelectedItem);
            }
        });
    }

    public subWatershedChange(value: any): void {
        var summarizationLogicForMapLayers = this.GetSummarizationLogicForMapLayersWithLocationFilter();
        this.map.getUserGeometryData(summarizationLogicForMapLayers);

        this.LoadSummaryGridData();
    }

    // Will be called when location filter values are changed
    public GetSummarizationLogicForMapLayersWithLocationFilter() {
        if (this.summarizationDropdownSelectedValue == 1) { //LSD
            return {
                "LSD": true, "Reach": true, "Parcel": false, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "LSD"
            };
        }
        else if (this.summarizationDropdownSelectedValue == 2) {  // Parcel
            return {
                "Reach": true, "Parcel": true, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Parcel"
            };
        }
        else if (this.summarizationDropdownSelectedValue == 3) {  // Farm
            return {
                "Reach": true, "Farm": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Farm"
            };
        }
        else if (this.summarizationDropdownSelectedValue == 4) {  // Municipality
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Municipality"
            };
        }
        else if (this.summarizationDropdownSelectedValue == 5) {  // SubWatershed
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": true, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "SubWatershed"
            };
        }
        else if (this.summarizationDropdownSelectedValue == 6) {  // Watershed
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
                "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Watershed"
            };
        }
    }


    // Will be called on Summarization change
    public GetMapLayerLegendItemsToShow() {
        if (this.summarizationDropdownSelectedValue == 1) { //LSD
            return {
                "LSD": true, "Reach": true, "Parcel": false, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true
            };
        }
        else if (this.summarizationDropdownSelectedValue == 2) {  // Parcel
            return {
                "Reach": true, "Parcel": true, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true
            };
        }
        else if (this.summarizationDropdownSelectedValue == 3) {  // Farm
            return {
                "Reach": true, "Farm": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true
            };
        }
        else if (this.summarizationDropdownSelectedValue == 4) {  // Municipality
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true
            };
        }
        else if (this.summarizationDropdownSelectedValue == 5) {  // SubWatershed
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": true, "WaterShed": true
            };
        }
        else if (this.summarizationDropdownSelectedValue == 6) {  // Watershed
            return {
                "Reach": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true
            };
        }
    }

    public showHideRightPanel() {
        if (this.buttonText == '>') {
            this.buttonText = '<';
            this.displayMiddleDivClass = 'col-lg-12';
            this.displayRightDivClass = 'noneDisplay';
        }
        else {
            this.buttonText = '>';
            this.displayMiddleDivClass = 'col-lg-9';
            this.displayRightDivClass = 'col-lg-3';
        }
    }

}
