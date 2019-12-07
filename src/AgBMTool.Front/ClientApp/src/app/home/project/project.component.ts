import { Component, ViewEncapsulation, ViewChild } from '@angular/core';
import { ProjectService } from './project.service';
import { Alert } from 'selenium-webdriver';
import { Options } from 'ng5-slider';
import { Router } from '@angular/router';

import { PageChangeEvent, GridDataResult } from '@progress/kendo-angular-grid';
import { TooltipDirective } from '@progress/kendo-angular-tooltip';
import { NotificationService } from '../../services/utility/notificationService';
import { ChartComponent } from '../projectBMPCostEffectivenessChart/chart.component';
import { MessageService } from '../../services/index';

@Component({
    selector: 'app-project',
    templateUrl: './project.component.html',
    styleUrls: ['./project.component.css'],
    encapsulation: ViewEncapsulation.None
})
/** project component*/
export class ProjectComponent {

    @ViewChild(TooltipDirective, { static: false }) public tooltipDir: TooltipDirective;

    private chart: ChartComponent;
    private _notificationService;

    public gridView: GridDataResult;
    public skip = 0;
    public pageSize = 20;

    gridData: any[];
    private projectGridHeight: number;
    displayModel = 'none';
    displayDeleteModel = 'none';
    public deleteProjectId: any;

    public title: string;
    public description: string;

    addProject: any;

    summarizationTypeList: any[];
    summarizationTypeSelectedItem: any = 0;

    scopeList: any[];
    scopeListSelectedItem: any = 0;

    spatialUnits: any[];
    spatialUnitSelectedItem: number = 0;

    baselineList: any[];
    baselineSelectedItem: number = 0;

    // slider range
    minYear: number;
    maxYear: number;
    sliderOption: Options = { floor: 2015, ceil: 2020 };

  constructor(private projectsService: ProjectService, private router: Router, private notificationService: NotificationService, private messageService: MessageService) {
        this.projectGridHeight = window.innerHeight - 188;
        this._notificationService = notificationService;
    }

    public ngOnInit(): void {

        this.bindProjectList();
    }

    bindProjectList() {
        this.projectsService.GetProjectsListByUserId().subscribe(rdata => {
            this.gridData = rdata;
            this.loadProjects();
        });
    }

    protected pageChange({ skip, take }: PageChangeEvent): void {
        this.skip = skip;
        this.pageSize = take;
        this.loadProjects();
    }

    private loadProjects(): void {
        this.gridView = {
            data: this.gridData.slice(this.skip, this.skip + this.pageSize),
            total: this.gridData.length
        };
    }
    public showTooltip(e: MouseEvent): void {
        const element = e.target as HTMLElement;
        if (element.nodeName === 'TD' || element.offsetWidth < element.scrollWidth) {
            this.tooltipDir.toggle(element);
        } else {
            this.tooltipDir.hide();
        }
    }

    public getItem(anchor: any): string {
        const index = parseInt(anchor.nativeElement.parentNode.getAttribute('data-kendo-grid-item-index'), 10);
        const item = this.gridData[index];
        return item.description == null ? item.name : item.description;
    }

    // open "Add Project" model popup
    openModal() {

        this.projectsService.GetSummarizationTypeList().subscribe(rdata => {
            this.summarizationTypeList = rdata;
            if (rdata.length > 0) {
                this.summarizationTypeSelectedItem = rdata[0].itemId;
                this.SummarizationTypeChange(rdata[0]);
            }
        });

        this.projectsService.GetProjectSpatialUnitType().subscribe(rdata => {
            this.spatialUnits = rdata;
            if (rdata.length > 0)
                this.spatialUnitSelectedItem = rdata[0].id;
        });

        this.projectsService.GetBaseLineOptions().subscribe(rdata => {
            this.baselineList = rdata;
            if (rdata.length > 0)
                this.baselineSelectedItem = this.baselineList[0].itemId;
        });

        this.title = "";
        this.description = "";

        this.displayModel = 'block';
    }

    // close "Add Project" model popup
    onCloseHandled() {
        this.displayModel = 'none';
    }

    public SummarizationTypeChange(value: any): void {

        if (value != null && value != undefined) {
            if (value.itemId == 4) {
                this.projectsService.GetMunicipalitiesByUserId().subscribe(rdata => {
                    if (rdata.length > 0) {
                        this.scopeList = rdata;
                        this.scopeList.splice(0, 0, { name: "All", id: 0 });
                        this.scopeListSelectedItem = [{ name: "All", id: 0 }];
                    }
                    else {
                        this.minYear = 0;
                        this.maxYear = 0;
                        this.scopeList = [];
                        this.scopeListSelectedItem = 0;
                    }
                    this.SetSliderRange();
                });
            }
            else {
                this.projectsService.GetWatershedByMunicipalityId(-1).subscribe(rdata => {

                    if (rdata.length > 0) {
                        this.scopeList = rdata;
                        this.scopeList.splice(0, 0, { name: "All", id: 0 });
                        this.scopeListSelectedItem = [{ name: "All", id: 0 }];
                    }
                    else {
                        this.minYear = 0;
                        this.maxYear = 0;
                        this.scopeList = [];
                        this.scopeListSelectedItem = 0;
                    }
                    this.SetSliderRange();
                });
            }
        }

    }

    public SetSliderRange() {
        this.projectsService.GetSliderRange().subscribe(rdata => {
            this.minYear = rdata.startYear;
            this.maxYear = rdata.endYear;
            this.sliderOption = { floor: this.minYear, ceil: this.maxYear };
        });
    }

    public scopeListChange(value: any): void {
        // if there is 'All' scope
        var findAllInScopeList = value.filter(x => x.id == 0);
        if (findAllInScopeList.length > 0)
            this.scopeListSelectedItem = findAllInScopeList;
    }

    // add project
    public AddProject() {
        if (!this.IsValidAddProject()) { return; }

        var filterdScopeList = this.scopeListSelectedItem.filter(x => x.id == 0);
        if (filterdScopeList.length > 0)
            filterdScopeList = this.scopeList.filter(x => x.id != 0);
        else
            filterdScopeList = this.scopeListSelectedItem;

        this.addProject = [{
            Name: this.title, Description: this.description, Scope: filterdScopeList, ScenarioTypeId: this.baselineSelectedItem,
            SpatialUnitId: this.spatialUnitSelectedItem, StartYear: this.minYear, EndYear: this.maxYear
        }];

        this.projectsService.SaveProject(this.addProject[0]).subscribe(rdata => {
            this.bindProjectList();
          this.onCloseHandled();
          this.messageService.sendcreatenewproject(true);
        });
    }

    public IsValidAddProject() {
        var isValid = true;

        if (this.title == "") {
            this._notificationService.showError("Please enter Title.", "");
            isValid = false;
        }
        else if (this.description == "") {
            this._notificationService.showError("Please enter Description.", "");
            isValid = false;
        }
        else if (this.scopeListSelectedItem.length == 0) {
            this._notificationService.showError("Please enter Scope.", "");
            isValid = false;
        }
        else if (this.baselineSelectedItem == null || this.baselineSelectedItem == 0) {
            this._notificationService.showError("Please select Baseline.", "");
            isValid = false;
        }
        else if (this.spatialUnitSelectedItem == 0) {
            this._notificationService.showError("Please select Spatial Unit.", "");
            isValid = false;
        }
        else if (this.minYear == 0 || this.maxYear == 0) {
            this._notificationService.showError("Please select Start and end year.", "");
            isValid = false;
        }

        return isValid;
    }

    // open "Delete Project" model popup
    onOpenDeleteModel(index) {
        this.deleteProjectId = this.gridData[index].id;
        this.displayDeleteModel = 'block';
    }

    // close "Add Project" model popup
    onCloseDeleteModel() {
        this.displayDeleteModel = 'none';
    }

    // delete project by id
    DeleteProject() {
        this.projectsService.DeleteProjectById(this.deleteProjectId).subscribe(rdata => {
            this.bindProjectList();
            this.chart.LoadChartData();
            this.onCloseDeleteModel();
        });
    }

    CallBaselineInfo(projectId) {
        this.router.navigate(['home/baselineInfo', { id: projectId }]);
    }
}

