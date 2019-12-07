import { Component } from '@angular/core';
import { ProjectBMPCostEffectivenessChartService } from './chart.service';

@Component({
  selector: 'app-chart',
  templateUrl: './chart.component.html',
  styleUrls: ['./chart.component.css']
})
/** chart component*/
export class ChartComponent {

    public projectBMPCostEffectivenessData: any;
    
    public costEffectivenessSelectedValue: number = 2;

    public bmpEffectivenessTypes: any;

    constructor(private projectBMPCostEffectivenessChartService: ProjectBMPCostEffectivenessChartService) {

    }

    public ngOnInit(): void {

        this.LoadBMPEffectiveness();
        this.LoadChartData();
    }

    public LoadBMPEffectiveness() {
        this.projectBMPCostEffectivenessChartService.GetBMPEffectivenessType().subscribe(rdata => {
            this.bmpEffectivenessTypes = rdata;
            this.costEffectivenessSelectedValue = rdata.filter(x => x.defaultConstraintTypeId == 1)[0].id;
        });
    }

    public LoadChartData() {
        this.projectBMPCostEffectivenessChartService.GetProjectBMPCostEffectivenessChartData(this.costEffectivenessSelectedValue).subscribe(rdata => {
            this.projectBMPCostEffectivenessData = rdata;
        });

    }

    public BmpEffectivenessTypeChange(value: any): void {
        this.LoadChartData();
    }

    public labelContent = (e: any) => {
        return e.dataItem.projectName;
    };


}
