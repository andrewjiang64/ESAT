import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserOverviewComponent } from './userOverview/userOverview.component';
import { ProjectComponent } from './project/project.component';
import { BaselineInfoComponent } from './baselineInformation/baselineInfo.component';
import { BMPScopeAndIntelligenceComponent } from './bmpScopeAndIntelligence/bmpScopeAndIntelligence.component';
import { BmpSelectionAndOverviewComponent } from './bmp-selection-and-overview/bmp-selection-and-overview.component';
import { UserAuthGuardService } from '../AuthGuard/UserAuthGuardService';

const homeRoutes: Routes = [
    { path: 'overview', component: UserOverviewComponent },
    { path: 'overview', redirectTo: "overview", pathMatch: 'full' },
    { path: '', redirectTo: "overview", pathMatch: 'full', canActivate: [UserAuthGuardService] },
    { path: 'project', component: ProjectComponent, canActivate: [UserAuthGuardService] },
    { path: 'baselineInfo', component: BaselineInfoComponent },
    { path: 'bmpScopeAndIntelligence', component: BMPScopeAndIntelligenceComponent },
    { path: 'bmpSelectionAndOverviewComponent', component: BmpSelectionAndOverviewComponent}
];

@NgModule({
    imports: [
        RouterModule.forChild(
            homeRoutes
        )
    ],
    exports: [
        RouterModule
    ]
})
export class HomeRoutingModule { }
