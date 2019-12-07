import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserOverviewComponent } from './userOverview/userOverview.component';
import { ProjectComponent } from './project/project.component';
import { MapComponent } from '../map/map.component'
import { HomeRoutingModule } from './home-routing.module';
import { ArcgisApiService } from '../map/arcgis-api.service';
import { MapService } from '../map/map.service';
import { UserOverviewService } from './userOverview/userOverview.service';
import { ProjectService } from './project/project.service';
import { ChartComponent } from './projectBMPCostEffectivenessChart/chart.component';
import { ProjectBMPCostEffectivenessChartService } from './projectBMPCostEffectivenessChart/chart.service';
import { BaselineInfoComponent } from './baselineInformation/baselineInfo.component';
import { BaselineInfoService } from './baselineInformation/baselineInfo.service';
import { BMPScopeAndIntelligenceComponent } from './bmpScopeAndIntelligence/bmpScopeAndIntelligence.component';
import { BMPScopeAndIntelligenceService } from './bmpScopeAndIntelligence/bmpScopeAndIntelligence.service';
import { BmpSelectionAndOverviewComponentService} from './bmp-selection-and-overview/bm-selection-and-overview.service';
import { BmpSelectionAndOverviewComponent } from './bmp-selection-and-overview/bmp-selection-and-overview.component';
import { NotificationService } from '../services/utility/notificationService';

// Modules
import { GridModule } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { Ng5SliderModule } from 'ng5-slider';
import { TooltipModule } from '@progress/kendo-angular-tooltip';
import { LayoutModule } from '@progress/kendo-angular-layout';

@NgModule({
    imports: [
        CommonModule,
        HomeRoutingModule,
        GridModule,
        ChartsModule,
        DropDownsModule,
        FormsModule,
        Ng5SliderModule,
        TooltipModule,
        LayoutModule
    ],
    declarations: [
        UserOverviewComponent,
        ProjectComponent,
        MapComponent,
        ChartComponent,
        BaselineInfoComponent,
      BMPScopeAndIntelligenceComponent,
      BmpSelectionAndOverviewComponent
    ],
  providers: [ArcgisApiService, MapService, UserOverviewService, ProjectService, ProjectBMPCostEffectivenessChartService, BaselineInfoService, BMPScopeAndIntelligenceService, NotificationService, MapComponent, BmpSelectionAndOverviewComponentService],
})
export class HomeModule { }
