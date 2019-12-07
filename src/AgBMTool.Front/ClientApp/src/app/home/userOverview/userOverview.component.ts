import { Component, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { UserOverviewService } from './userOverview.service';
import { Options } from 'ng5-slider';
import { ArcgisApiService } from '../../map/arcgis-api.service';
import { MapService } from '../../map/map.service';
import { MapComponent } from '../../map/map.component';
import { MessageService } from '../../services/index';
import { Subscription } from 'rxjs';

interface Item {
    ItemName: string,
    ItemId: number
};

@Component({
    selector: 'app-userOverview',
    templateUrl: './userOverview.component.html',
    styleUrls: ['./userOverview.component.css'],
    encapsulation: ViewEncapsulation.None
})

export class UserOverviewComponent {
   private map: MapComponent;
    private overviewGridHeight: number;
    private overviewMapHeight: number;

    private summaryGridColumns: any;
    private summaryGridData: any;
    private summaryGridDataAll: any;
    private next: number;
    private previousSelectRowById: number = 0;
    private selectRowById = [];

    // slider range
    minYear: number = 0;
    maxYear: number = 0;
    sliderOption: Options = { floor: 0, ceil: 100 };

    private progressBarSummaryGridData: boolean = true;


    public baselineDropdownSelectedValue: number = 2;
    public summarizationDropdownSelectedValue: number = 2;

    public baselineDropdown: any[];
    public summarizationDropdown: any[];

    private showLocationFilter: boolean;

    public listItems: Array<{ text: string, value: number }> = [
        { text: "Parcel", value: 1 },
        { text: "Stream", value: 2 },
        { text: "Watershed", value: 3 },
        { text: "Municipality", value: 4 }
    ];

    public municipalityDropdown: any[];
    municipalitySelectedItem: any = -1;

    public watershedDropdown: any[];
    watershedSelectedItem: any = -1;

    public subwatershedDropdown: any[];
    subWatershedSelectedItem: any = -1;
    subscription: Subscription;
    constructor(private userOverviewService: UserOverviewService, private arcgisService: ArcgisApiService, private mapService: MapService, private messageservice: MessageService,
     private elRef: ElementRef) {
        this.overviewMapHeight = window.innerHeight - 305;
        this.overviewGridHeight = window.innerHeight - (window.innerHeight - 342) - 150;
      this.map = new MapComponent(arcgisService, mapService, messageservice);
      this.subscription = this.messageservice.getClickedSummaryLayerMessage().subscribe(message => {
        console.log("message = " + message);
        if (message) {
          this.SelectOverviewGridRowById(message.Id);
        }
      });
    }

    public ngOnInit(): void {
        this.userOverviewService.GetBaseLineOptions().subscribe(rdata => {
            this.baselineDropdown = rdata;
        });
  
        this.userOverviewService.GetScenarioResultSummarizationType().subscribe(rdata => {
            this.summarizationDropdown = rdata;
        });
        this.LoadLocationFilters();

        this.LoadRange();
        this.LoadLegend();
    }

    // Load slider range
    public LoadRange() {
        this.userOverviewService.GetWatershedRange(this.baselineDropdownSelectedValue).subscribe(rdata => {

            this.minYear = rdata.startYear;
            this.maxYear = rdata.endYear;
            this.sliderOption = { floor: this.minYear, ceil: this.maxYear };
            //this.LoadGridData();
        });
    }

    // Load location filter dropdowns
    public LoadLocationFilters() {
        // Get Municipalities 
        this.userOverviewService.GetMunicipalitiesByUserId().subscribe(rdata => {
            this.municipalityDropdown = rdata;
            if (rdata.length > 0) {
                this.municipalityDropdown.splice(0, 0, { name: "All", id: -1 });
                this.municipalitySelectedItem = this.municipalityDropdown[0].id;
                this.municipalityChange(this.municipalityDropdown[0]);
            }
        });
    }

    public sliderChangeEvent() {
        this.LoadGridData();
    }

    public municipalityChange(value: any): void {
        this.userOverviewService.GetWatershedsByMunicipality(this.municipalitySelectedItem).subscribe(rdata => {
            this.watershedDropdown = rdata;
            if (rdata.length > 0) {
                this.watershedDropdown.splice(0, 0, { name: "All", id: -1 });
                this.watershedSelectedItem = this.watershedDropdown[0].id;
                this.watershedChange(this.watershedDropdown[0]);

            }
        });
    }

    public watershedChange(value: any): void {

        this.userOverviewService.GetSubWatershedsByWatershedId(this.municipalitySelectedItem, this.watershedSelectedItem).subscribe(rdata => {
            this.subwatershedDropdown = rdata;
            if (rdata.length > 0) {
                this.subwatershedDropdown.splice(0, 0, { name: "All", id: -1 });
                this.subWatershedSelectedItem = this.subwatershedDropdown[0].id;
                this.subWatershedChange(this.subWatershedSelectedItem);
            }
        });
    }

    public subWatershedChange(value: any): void {

      var summarizationLogicForMapLayers = this.GetSummarizationLogicForMapLayersWithLocationFilter();
      this.map.getUserGeometryData(summarizationLogicForMapLayers);

        this.LoadGridData();
    }

    // Load Summary grid data
    public LoadGridData() {

        this.progressBarSummaryGridData = false;

        this.userOverviewService.GetSummaryGridData(this.baselineDropdownSelectedValue, this.summarizationDropdownSelectedValue, this.municipalitySelectedItem,
            this.watershedSelectedItem, this.subWatershedSelectedItem, this.minYear, this.maxYear).subscribe(rdata => {

                rdata.summaryTableColumns.forEach(p => p.fieldName = p.fieldName.toLowerCase());      // Set first character to lower case to match with datatable column

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
        
        var spacialScope = this.summarizationDropdown.filter(x => x.itemId == this.summarizationDropdownSelectedValue)[0].itemName;
        var optHighlight = { "layername": spacialScope, "Id": this.selectRowById[0] };
        this.map.highlightgeogemtry(optHighlight);

        if (this.previousSelectRowById > 0) {
            var optUnhighlight = { "layername": spacialScope, "Id": this.previousSelectRowById };
           // this.map.unhighlightgeogemtry(optUnhighlight);
            this.previousSelectRowById = this.selectRowById[0];
        }
    }

    // Load grid, map and legend on baseline value change
    public BaselineChange(value: any): void {

        this.LoadGridData();

    }

    public SummarizationChange(value: any): void {
        if (this.summarizationDropdownSelectedValue <= 3) {
            this.showLocationFilter = false;
        }
        else {
            this.showLocationFilter = true;
        }
        this.LoadGridData();

        var summarizationLogicForMapLayersForLocation = this.GetSummarizationLogicForMapLayersWithLocationFilter();
      this.messageservice.sendSummarilizationlevelMessage(summarizationLogicForMapLayersForLocation);

        this.LoadLegend();

    }

    public LoadLegend() {
        var summarizationLogicForMapLayers = this.GetMapLayerLegendItemsToShow();
        this.map.drawlegend(summarizationLogicForMapLayers);
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

}
