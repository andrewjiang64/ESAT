import { Component, ViewEncapsulation } from '@angular/core';
import { BMPScopeAndIntelligenceService } from '../bmpScopeAndIntelligence/bmpScopeAndIntelligence.service';
import { Options } from 'ng5-slider';
import { ArcgisApiService } from '../../map/arcgis-api.service';
import { MapService } from '../../map/map.service';
import { MessageService } from '../../services/index';
import { MapComponent } from '../../map/map.component';
import { BmpSelectionAndOverviewComponentService } from './bm-selection-and-overview.service'
import { NotificationService } from '../../services/utility/notificationService';
import { ActivatedRoute } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { RowClassArgs } from "@progress/kendo-angular-grid";
import { aggregateBy } from '@progress/kendo-data-query';

const createFormGroup = dataItem => new FormGroup({
  'value': new FormControl(dataItem.value, Validators.compose([Validators.required, Validators.pattern('^[0-9]{1,3}')])),
});

@Component({
    selector: 'app-bmp-selection-and-overview',
    templateUrl: './bmp-selection-and-overview.component.html',
    styleUrls: ['./bmp-selection-and-overview.component.css']
})
/** bmpSelectionAndOverview component*/
export class BmpSelectionAndOverviewComponent {
  private map: MapComponent;
  private projectId: any;
  private bmptype: string;
  private bmptypeId: string;
  private bmptypeIds: any[];
  private sub: any;
  private _notificationService;

  public buttonText = '>';
  public displayMiddleDivClass = 'col-lg-9';
  public displayRightDivClass = 'col-lg-3';

  public formGroup: FormGroup;
  private editedRowIndex: number;

  private tabStripHeight: number;
  private tabStripGridHeight: number;
  private tabStripChartHeight: number;
  private baseLineInfoMapHeight: number;
  private constraintGridHeight: number;

  public showLocationType: boolean = false;

  private scenarioModelResultVariableTypeChart: any;
  private scenarioModelResultVariableTypeChartSelectedValue: number;

  private bmpEffectivenessLocationType: any;
  private bmpEffectivenessLocationTypeSelectedValue: number;

  public checkedItems = {};

  private summaryGridColumns: any[];
  private summaryGridData: any[];
  private nonFilteredSummaryGridData: any[];
  private selectRowById: any[] = ["17"];

  private fieldBMPSummaryGridData: any[];
  private fieldBMPSummaryGridDataAll: any[];
  private fieldBMPSummaryGridDataNext: number;
  public fieldBMPSummaryLevel: string = "LSD";

  public baselineBMPSummaryGridData: any[];
  public baselineBMPEffectivenessGridData: any[];

  public municipalityList: any[];
  municipalitySelectedItem: any = -1;

  public watershedList: any[];
  watershedSelectedItem: any = -1;

  public subWatershedList: any[];
  subWatershedSelectedItem: any = -1;

  public intelligentSettingList: any[];
  intelligentSettingSelectedItem: any = 0;

  public showBudget: boolean = false;
  public showEcoService: boolean = false;

  public displayCustomizeWeightModel = 'none';
  public displayAddConstraintModel = 'none';

  public budget: number;
  public targetValue: number = 0;

  public bmpEffectivenessTypeList: any[];
  bmpEffectivenessTypeSelectedItem: any = 0;

  public addConstraintBMPEffectivenessTypeList: any[];
  addConstraintBMPEffectivenessTypeSelectedItem: any = 0;

  public optimizationConstraintValueTypeList: any[];
  optimizationConstraintValueSelectedItem: any = 0;

  public bmpsummarygriddata: any[];
  public bmpprojectEffectivenessSummary: any[];
  public bmpdatasum: Number;

  public deleteConstraintValueTypeid: number;
  public displayConstraintDeleteModel = 'none';

  public displayRunIntelligentRecommendationModel = 'none';
  public displayRunIntelligentRecommendationWarningModel = 'none';
  public displayRunIntelligentRecommendationResultModel = 'none';

  // chart data
  public effectinessData: Array<{ category: number, color: string, value: number }> = [];
  public acumulatedData: any[] = [];
  public transitions: boolean = false;
  public navigatorStep: number;
  public step: number;
  public min: number = 1;
  public max: number = 20;
  public navigatorData: any[];
  public categories: any[];
  public valueAxis: any[];

  public locationType: string;  // LSD or Parcel or Model Component Name
  public locationTypeSelectionColor: string;
  public investorList: any[];
  public selectedinvestorId: number;

  private progressBarSummaryGridData: boolean = true;
  private isBMPTypeList: Boolean = false;
  private tabGridHeight: number;
  public line = {
    color: 'white'
  };
    /** bmpSelectionAndOverview ctor */
  constructor(private bmpScopeAndIntelligenceService: BMPScopeAndIntelligenceService, private route: ActivatedRoute, private arcgisService: ArcgisApiService, private mapService: MapService, private messageservice: MessageService,
    private notificationService: NotificationService, private bmpSelectionAndOverviewComponentService: BmpSelectionAndOverviewComponentService) {

    this.baseLineInfoMapHeight = window.innerHeight - 305;
    this.tabStripHeight = window.innerHeight - (window.innerHeight - 342) - 150;
    this.tabStripGridHeight = (window.innerHeight - (window.innerHeight - 342) - 150) - 65;
    this.tabStripChartHeight = this.tabStripGridHeight + 10;
    this.constraintGridHeight = (window.innerHeight / 2) - ((window.innerHeight * 20) / 100);
    this.tabGridHeight = window.innerHeight * 0.22;
    this.map = new MapComponent(arcgisService, mapService, messageservice);
    this._notificationService = notificationService;
  }

  public ngOnInit(): void {

    this.sub = this.route.params.subscribe(params => {
      this.projectId = params['id']; // (+) converts string 'id' to a number
      this.bmptype = params['bmptype'];
      this.bmptypeId = params['bmptypeId'];
      this.isBMPTypeList = !(params['bmptypeIds'] == null || params['bmptypeIds'].length == 0);
      this.map.getBMPTypeGeometryData(this.projectId, this.bmptypeId, params['bmptypeIds'], false);
      if (!this.isBMPTypeList) {
        this.tabGridHeight = window.innerHeight * 0.22;
        this.LoadGridData();
      }
      else {
        this.tabGridHeight = window.innerHeight * 0.35;
        this.tabStripGridHeight = (window.innerHeight - (window.innerHeight - 342) - 150) - 20;
        this.loadLsdParcelStructuralBMPSummary();
      }
      if (!this.isBMPTypeList)
        this.LoadLocationType();
      
      this.LoadBMPSummaryGridData();
      this.LoadProjectEffectivenessSummaryGridData();
      this.getInvestorList();
     
    });

    this.messageservice.getSelectedandoverviewGraphicMessage().subscribe(data => {
      this.onselctionandoverviewGridCheckboxValueChange(data);
      this.syncgridwithmap(data);
    })

    this.bmpScopeAndIntelligenceService.GetProjectMunicipalitiesByProjectId(this.projectId).subscribe(rdata => {
      this.municipalityList = rdata;
      if (rdata.length > 0) {
        this.municipalityList.splice(0, 0, { name: "All", id: -1 });
        this.municipalitySelectedItem = this.municipalityList[0].id;
        this.municipalityChange(this.municipalityList[0]);
      }
    });

    this.bmpScopeAndIntelligenceService.GetOptimizationTypeList(this.projectId).subscribe(rdata => {
      this.intelligentSettingList = rdata;
      if (rdata.length > 0) {
        var intelligentSetting = this.intelligentSettingList.filter(x => x.isDefault)[0];
        this.intelligentSettingSelectedItem = intelligentSetting.itemId;
        this.intelligentSettingChange(intelligentSetting);

        this.GetBMPEffectivenessTypesByProjectId();
      }
    });

    this.bmpScopeAndIntelligenceService.GetScenarioModelResultVariableTypes().subscribe(rdata => {
      this.scenarioModelResultVariableTypeChart = rdata;
      this.scenarioModelResultVariableTypeChartSelectedValue = this.scenarioModelResultVariableTypeChart.filter(x => x.isDefault == true)[0].itemId;

        this.bmpScopeAndIntelligenceService.GetBMPEffectivenessLocationType().subscribe(rdata => {
            this.bmpEffectivenessLocationType = rdata;
            this.bmpEffectivenessLocationTypeSelectedValue = this.bmpEffectivenessLocationType.filter(x => x.isDefault == true)[0].itemId;

            this.LoadChart();
        });
    });

  }

  public LoadGridData() {
    this.progressBarSummaryGridData = false;

    this.bmpSelectionAndOverviewComponentService.GetSummaryGridData(this.projectId, this.bmptypeId).subscribe(rdata => {

      rdata.summaryTableColumns.forEach(p => p.fieldName = p.fieldName.toLowerCase());   // Set first character to lower case to match with datatable column

      this.summaryGridColumns = rdata.summaryTableColumns.filter(x => x.fieldName !== "isselectable" && x.fieldName !== "isselected");
      this.nonFilteredSummaryGridData = rdata.summaryTableData;
      this.FillSummarygridData();
      this.LoadChart();
      this.progressBarSummaryGridData = true;
    });
  }

  public loadLsdParcelStructuralBMPSummary() {
    this.progressBarSummaryGridData = false;

    this.bmpSelectionAndOverviewComponentService.getLsdParcelStructuralBMPSummaryDTOsForBMPSelectionAndOverview(this.projectId).subscribe(rdata => {
        this.fieldBMPSummaryLevel = rdata.projectSpatialUnitType;
        this.fieldBMPSummaryGridDataAll = rdata.summary;
        this.fieldBMPSummaryGridDataNext = this.fieldBMPSummaryGridDataAll.length > 10 ? 10 : this.fieldBMPSummaryGridDataAll.length;
        this.fieldBMPSummaryGridData = this.fieldBMPSummaryGridDataAll.slice(0, this.fieldBMPSummaryGridDataNext);

      this.progressBarSummaryGridData = true;
     
    });
  }

    private loadMoreFieldBMPSummary(): void {
        if (this.fieldBMPSummaryGridDataNext < this.fieldBMPSummaryGridDataAll.length) {
            this.fieldBMPSummaryGridDataNext = this.fieldBMPSummaryGridDataAll.length > this.fieldBMPSummaryGridDataNext + 10 ? this.fieldBMPSummaryGridDataNext + 10 : this.fieldBMPSummaryGridDataAll.length;
            this.fieldBMPSummaryGridData = this.fieldBMPSummaryGridData.concat(this.fieldBMPSummaryGridDataAll.slice(this.fieldBMPSummaryGridDataNext - 10, this.fieldBMPSummaryGridDataNext));
        }
    }


  public LoadBMPSummaryGridData() {
    this.bmpSelectionAndOverviewComponentService.getProjectBMPSummaryData(this.projectId).subscribe(data => {
      this.bmpsummarygriddata = data;
      this.sumCost();
    })
   
  }

  public LoadProjectEffectivenessSummaryGridData() {
    this.bmpSelectionAndOverviewComponentService.getProjectEffectivenessSummary(this.projectId).subscribe(data => {
      this.bmpprojectEffectivenessSummary = data;
    })
  }

  public sumCost() {
    var sum = 0;
    for (var i = 0; i < this.bmpsummarygriddata.length; i++) {
      sum = sum + this.bmpsummarygriddata[i].totalCost;
    }
    this.bmpdatasum = Number(sum.toFixed(3));
  }

  public FillSummarygridData() {
    this.summaryGridData = this.nonFilteredSummaryGridData.filter(x => x.scenariotypeid == this.scenarioModelResultVariableTypeChartSelectedValue);
    this.selectRowById = this.summaryGridData.filter(x => x.isselected == true).map(t => t.locationid);
  }

  public LoadLocationType() {
    this.bmpScopeAndIntelligenceService.GetLocationTypeByProjectIdAndBMPTypeId(this.projectId, this.bmptypeId).subscribe(rdata => {
      this.locationType = rdata.toString();
    });
  }

  // Select grid row (send single location id to this function)
  public SelectGridRowByIds(locationId) {
    if (this.selectRowById.filter(x => x == locationId).length > 0) {
      this.selectRowById = this.selectRowById.filter(x => x !== locationId);
    }
    else {
      this.selectRowById.push(locationId);
    }
  }

  // highlight area in map on row selected
  public onSelectedKeysChange(e) {
    var optHighlight = { "layername": this.bmptype, "Id": this.selectRowById };
    this.map.highlightfeaturelayergeogemtry(optHighlight);
  }

  public FilterSummaryGrid() {
    this.summaryGridData = this.summaryGridData.slice(1);
    this.summaryGridData = this.nonFilteredSummaryGridData.filter(x => x.scenariotypeid == this.scenarioModelResultVariableTypeChartSelectedValue);
    this.bmpEffectivenessLocationTypeChange(this.bmpEffectivenessLocationTypeSelectedValue);
  }

  public CheckIfColumnisHidden(column) {
    if (column.isHidden)
      return true;
    else
      return false;
  }


  public ShowLocationTypeDropdown(value) {
    if (value.title == "Chart") {
      this.showLocationType = true;
      this.LoadChart();
    }
    else {
      this.showLocationType = false;
    }
  }

  public LoadChart() {
    this.bmpEffectivenessLocationTypeChange(this.bmpEffectivenessLocationTypeSelectedValue);

  }

  public onScenarioTypeClicked(dataItem) {
    if (this.checkedItems[dataItem.isSelectable]) {
      this.checkedItems[dataItem.isSelectable] = false;
    } else {
      this.checkedItems[dataItem.isSelectable] = true;
    }
  }

  public municipalityChange(value: any): void {
    this.bmpScopeAndIntelligenceService.GetProjectWatershedsByMunicipality(this.projectId, this.municipalitySelectedItem).subscribe(rdata => {
      this.watershedList = rdata;
      if (rdata.length > 0) {
        this.watershedList.splice(0, 0, { name: "All", id: -1 });
        this.watershedSelectedItem = this.watershedList[0].id;
        this.watershedChange(this.watershedList[0]);

      }
    });
  }

  public watershedChange(value: any): void {

    this.bmpScopeAndIntelligenceService.GetSubWatershedsByWatershedId(this.municipalitySelectedItem, this.watershedSelectedItem).subscribe(rdata => {
      this.subWatershedList = rdata;
      if (rdata.length > 0) {
        this.subWatershedList.splice(0, 0, { name: "All", id: -1 });
        this.subWatershedSelectedItem = this.subWatershedList[0].id;

        this.subWatershedChange(this.subWatershedSelectedItem);
      }
    });
  }

  public getInvestorList(): void {

    this.bmpScopeAndIntelligenceService.GetInvestorList(this.projectId, this.bmptypeId).subscribe(rdata => {
      this.investorList = rdata;
      if (this.investorList.length > 0) {
        this.selectedinvestorId = this.investorList[0].id;
      }
    });
  }

  public subWatershedChange(value: any): void {

    var summarizationLogicForMapLayers = this.GetSummarizationLogicForMapLayersWithLocationFilter();
    //    this.map.getUserGeometryData(summarizationLogicForMapLayers);
  }


  // Will be called when location filter values are changed
  public GetSummarizationLogicForMapLayersWithLocationFilter() {
    if (this.locationType == "LSD") { //LSD
      return {
        "LSD": true, "Reach": true, "Parcel": false, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
        "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "LSD"
      };
    }
    else if (this.locationType == "Parcel") {  // Parcel
      return {
        "Reach": true, "Parcel": true, "Farm": false, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
        "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Parcel"
      };
    }
    else
      return {
        "Reach": true, "Farm": true, "Municipality": true, "SubWaterShed": false, "WaterShed": true,
        "MunicipalityId": this.municipalitySelectedItem, "WatershedId": this.watershedSelectedItem, "SubwatershedId": this.subWatershedSelectedItem, "selectedsummarzationType": "Farm"
      };
  }

  public onTabSelect(e) {

  }

  public selectAll() {

    this.UpdateAllSelections(true);
  }

  public deselectAll() {
    this.UpdateAllSelections(false);
  }

  private UpdateAllSelections(isSelected) {
    this.bmpScopeAndIntelligenceService.ApplyQuickSelection(this.projectId, this.bmptypeId, false, isSelected, this.municipalitySelectedItem, this.watershedSelectedItem, this.subWatershedSelectedItem).subscribe(rdata => {
      var locationids: any[] = rdata;
      this.nonFilteredSummaryGridData.forEach(function (part, index) {
        if (locationids.indexOf(+this[index].locationid) > -1) //location id is string, need to convert to number
          this[index].isselected = isSelected;
      }, this.nonFilteredSummaryGridData);

      this.FillSummarygridData();
      this.map.updatebmpselectedstatus(locationids, isSelected);
      //here call map function and send the list of locaton ids.
    });
  }

  public intelligentSettingChange(optimizationType: any): void {

    if (optimizationType != null && optimizationType != undefined) {
      this.bmpScopeAndIntelligenceService.SaveOptimizationType(this.projectId, optimizationType.itemId).subscribe(rdata => {
        // 2 is Budget
        if (optimizationType.itemId == 2) {
          this.showBudget = true;
          this.showEcoService = false;
        }
        else {
          this.showBudget = false;
          this.showEcoService = true;
        }
      });
    }
  }

  // open "Run Intelligent Recommendation" warning model popup
  onOpenRunIntelligentRecommendationWarningModel() {
    this.displayRunIntelligentRecommendationWarningModel = 'block';
  }

  // open "Run Intelligent Recommendation" model popup
  onOpenRunIntelligentRecommendationModel() {

    if (this.selectRowById.length > 0)
      this.onOpenRunIntelligentRecommendationWarningModel();
    else
      this.displayRunIntelligentRecommendationModel = 'block';
  }

  // close "Run Intelligent Recommendation" model popup
  onCloseRunIntelligentRecommendationModel() {
    this.displayRunIntelligentRecommendationModel = 'none';
  }

  // open "Run Intelligent Recommendation" result model popup
  onOpenRunIntelligentRecommendationResultModel() {
    this.displayRunIntelligentRecommendationResultModel = 'block';
  }

  // close "Run Intelligent Recommendation" model popup
  onCloseRunIntelligentRecommendationResultModel() {
    this.displayRunIntelligentRecommendationResultModel = 'none';
  }

  private GetBMPEffectivenessTypesByProjectId() {
    this.bmpScopeAndIntelligenceService.GetBMPEffectivenessTypeListByProjectId(this.projectId).subscribe(optimization => {
      this.bmpEffectivenessTypeList = optimization.bmpEffectivenessTypes;
      this.addConstraintBMPEffectivenessTypeList = optimization.bmpEffectivenessTypes;

      // set existing Intelligent Setting
      if (this.addConstraintBMPEffectivenessTypeList.length > 0) {
        this.addConstraintBMPEffectivenessTypeSelectedItem = this.addConstraintBMPEffectivenessTypeList[0].id;
        this.budget = optimization.budgetTarget;

        for (var i = 0; i < optimization.addedOptimizationConstraint.length; i++) {
          this.targetValue = optimization.addedOptimizationConstraint[i].defaultConstraint;
          this.optimizationConstraintValueSelectedItem = optimization.addedOptimizationConstraint[i].defaultConstraintTypeId;
          this.addConstraintBMPEffectivenessTypeSelectedItem = optimization.addedOptimizationConstraint[i].id;
        }
      }

    });
  }

  // open "Customize Weight" model popup
  onOpenCustomizeWeightModel() {
    this.displayCustomizeWeightModel = 'block';
  }
  // close "Customize Weight" model popup
  onCloseCustomizeWeightModel() {
    this.displayCustomizeWeightModel = 'none';
  }

  // open "Customize Weight" model popup
  onOpenAddConstraintModel() {
    this.displayAddConstraintModel = 'block';
    this.targetValue = 0;

    this.bmpScopeAndIntelligenceService.GetOptimizationConstraintValueTypeList().subscribe(rdata => {
      this.optimizationConstraintValueTypeList = rdata;
      if (rdata.length > 0) {
        this.optimizationConstraintValueSelectedItem = rdata.filter(x => x.isDefault)[0].itemId;
      }
    });
  }
  // close "Customize Weight" model popup
  onCloseAddConstraintModel() {
    this.displayAddConstraintModel = 'none';
  }

  public addCustomizeWeight() {

    var optimizationWeights = [];

    for (var i = 0; i < this.bmpEffectivenessTypeList.length; i++) {
      optimizationWeights.push({ BMPEffectivenessTypeId: this.bmpEffectivenessTypeList[i].id, Weight: this.bmpEffectivenessTypeList[i].defaultWeight });
    }

    this.bmpScopeAndIntelligenceService.SaveWeight(this.projectId, optimizationWeights).subscribe(rdata => {
      this.displayCustomizeWeightModel = 'none';
    });
  }

  public IsValidAddConstraint() {
    var isValid = true;
    if (this.addConstraintBMPEffectivenessTypeSelectedItem == null || this.addConstraintBMPEffectivenessTypeSelectedItem == 0) {
      this._notificationService.showError("Please select Constraint.", "");
      isValid = false;
    }
    else if (this.targetValue == null || this.targetValue == 0) {
      this._notificationService.showError("Please enter Target.", "");
      isValid = false;
    }
    return isValid;
  }


  // open "Delete Constraint" model popup
  onOpenDeleteConstraintModel(item) {
    this.deleteConstraintValueTypeid = item.dataItem.id;
    this.displayConstraintDeleteModel = 'block';
  }
  // close "Add Constraint" model popup
  onCloseConstraintDeleteModel() {
    this.displayConstraintDeleteModel = 'none';
  }


  public editHandler({ sender, rowIndex, dataItem }) {
    this.closeEditor(sender);

    this.targetValue = dataItem.defaultConstraint;

    this.formGroup = createFormGroup(dataItem);

    this.editedRowIndex = rowIndex;

    sender.editRow(rowIndex, this.formGroup);
  }

  public cancelHandler({ sender, rowIndex }) {
    this.closeEditor(sender, rowIndex);
  }

  private closeEditor(grid, rowIndex = this.editedRowIndex) {
    grid.closeRow(rowIndex);
    this.editedRowIndex = undefined;
    this.formGroup = undefined;
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

    public bmpEffectivenessLocationTypeChange(itemId) {

        var yaxisTitle = this.scenarioModelResultVariableTypeChart.filter(x => x.itemId == this.scenarioModelResultVariableTypeChartSelectedValue).map(y => y.itemName);
        yaxisTitle = yaxisTitle + " " + this.bmpEffectivenessLocationType.filter(x => x.itemId == this.bmpEffectivenessLocationTypeSelectedValue).map(y => y.itemName);

        this.valueAxis = [{ name: 'TPoffsite', title: { text: yaxisTitle, color: 'white' } },
        { name: 'valueNavigatorAxis', pane: 'navigator', visible: false },
        { name: 'accumulatedBMP', title: { text: 'Accumulated BMP (%)', color: 'white' } }
        ];

        if (this.nonFilteredSummaryGridData !== undefined) {
            this.effectinessData = [];

            // 1 is OnSite and 2 is OffSite
            if (itemId == 1) {
                for (let i = 0; i < this.nonFilteredSummaryGridData.length; i++) {
                    if (this.nonFilteredSummaryGridData[i].scenariotypeid == this.scenarioModelResultVariableTypeChartSelectedValue) {
                        var val = parseFloat(this.nonFilteredSummaryGridData[i].onsiteeffectiveness);
                        var barColor = this.GetBarColor(this.nonFilteredSummaryGridData[i].locationid);

                        this.effectinessData.push({ category: this.nonFilteredSummaryGridData[i].locationid, color: barColor, value: val });
                    }
                }
            }
            else {
                for (let i = 0; i < this.nonFilteredSummaryGridData.length; i++) {
                    if (this.nonFilteredSummaryGridData[i].scenariotypeid == this.scenarioModelResultVariableTypeChartSelectedValue) {
                        var val = parseFloat(this.nonFilteredSummaryGridData[i].offsiteeffectiveness);
                        var barColor = this.GetBarColor(this.nonFilteredSummaryGridData[i].locationid);

                        if (!isNaN(val))
                            this.effectinessData.push({ category: this.nonFilteredSummaryGridData[i].locationid, color: this.locationTypeSelectionColor, value: val });
                    }
                }
            }

            if (this.effectinessData.length > 0) {

                this.min = 0;
                if (this.effectinessData.length > 20)
                    this.max = 20;
                else
                    this.max = this.effectinessData.length;

                // sort Effectiness Data by desc order
                this.effectinessData = this.effectinessData.sort(function (a, b) { return a.value - b.value });

                this.categories = this.effectinessData.map(d => d.value);
                this.navigatorStep = Math.floor(this.categories.length / 10);

                // get sum of Effectiness Data
                var sum = 0;
                for (let i = 0; i < this.effectinessData.length; i++) {
                    sum = sum + this.effectinessData[i].value;
                }

                var previousValue = 0;
                this.acumulatedData = [];
                for (var i = 0; i < this.effectinessData.length; i++) {

                    var value = ((this.effectinessData[i].value / sum) * 100) + previousValue;
                    this.acumulatedData.push(value.toFixed(1));
                    previousValue = value;
                }
            }
            else {
                this.categories = [];
                this.navigatorStep = 0;
                this.min = 0;
                this.max = 0;
                this.acumulatedData = [];
            }


        }
    }


    public GetBarColor(locationId) {
        if (this.selectRowById.filter(x => x == locationId).length > 0) {
            return this.locationTypeSelectionColor;
        }
        else {
            return "white";
        }
    }

  public onSelectEnd(args: any): void {
    // set the axis range displayed in the main pane to the selected range
    if (args.to - args.from > 20) {
      this.min = args.to - args.from;
      this.max = this.min + 20;

    }
    else {
      this.min = args.from;
      this.max = args.to;
    }

    // stop the animations
    this.transitions = false;

    // set the main axis ticks and labels step to prevent the axis from becoming too cluttered
    this.step = Math.floor((this.max - this.min) / 10);
  }

  private rowCallback = (context: RowClassArgs) => {
    switch (context.dataItem.isselectable) {
      case false:
        return "grayRow";
      default:
        return {};
    }
  }

  public onselctionandoverviewGridCheckboxValueChange(data) {
    var locationId = data.locationid;
    if (this.locationType == "LSD") {

      var optimizationLSD = {
        BMPTypeId: this.bmptypeId,
        LegalSubDivisionId: data.locationid,
        IsSelected: !data.isselected
      };

      this.bmpScopeAndIntelligenceService.SaveLegalSubDivisions(this.projectId,false ,optimizationLSD).subscribe(rdata => {
        data.isselected = !data.isselected;
      });
    }
    else if (this.locationType == "Parcel") {

      var optimizationParcels = {
        BMPTypeId: this.bmptypeId,
        ParcelId: data.locationid,
        IsSelected: !data.isselected
      };

      this.bmpScopeAndIntelligenceService.SaveParcels(this.projectId, false, optimizationParcels).subscribe(rdata => {
        data.isselected = !data.isselected;
      });
    }
    else {

      var optimizationModelComponents = {
        BMPTypeId: this.bmptypeId,
        ModelComponentId: data.locationid,
        IsSelected: !data.isselected
      };

      this.bmpScopeAndIntelligenceService.SaveModelComponents(this.projectId, false, optimizationModelComponents).subscribe(rdata => {
        data.isselected = !data.isselected;
      });
    }
    this.map.updatesinglebmpselectedstatus(locationId,!data.isselected);
    if (this.selectRowById.filter(x => x == locationId).length > 0) {
      this.selectRowById = this.selectRowById.filter(x => x !== locationId);
    }
    else {
      this.selectRowById.push(locationId);
    }
  }
  public syncgridwithmap(data) {
    var locationId = data.locationid;
    this.nonFilteredSummaryGridData.forEach(function (part, index) {
      if (locationId == this[index].locationid) //location id is string, need to convert to number
        this[index].isselected = !data.isselected;
    }, this.nonFilteredSummaryGridData);
    this.FillSummarygridData();
  }

}
