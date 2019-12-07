//Angular Packages
import { BrowserModule } from '@angular/platform-browser';
import { NgModule, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router, NavigationEnd } from '@angular/router';


//App Components
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { FooterComponent } from './footer/footer.component';
import { HeadComponent } from './head/head.component';
import { LoginComponent } from './login/login.component';
import { RightComponent } from './right/right.component';
import { TreeviewComponent } from './treeview/treeview.component';


// Modules
import { ToastrModule } from 'ngx-toastr';
import { TreeViewModule } from '@progress/kendo-angular-treeview';
import { AppRoutingModule } from './app-routing.module';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { DialogComponent } from './dialog/dialog.component';

// Services
import { TreeviewService } from './treeview/treeview.service';
import { UserAuthGuardService } from './AuthGuard/UserAuthGuardService';
import { NotificationService } from '../app/services/utility/notificationService';
import { MapService } from '../app/map/map.service';

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        FooterComponent,
        HeadComponent,
        LoginComponent,
        RightComponent,
    TreeviewComponent,
    DialogComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        HttpClientModule,
        FormsModule,
        TreeViewModule,
        NoopAnimationsModule,
        ToastrModule.forRoot({
            progressBar: true,
        }),
        AppRoutingModule,
        LayoutModule
    ],
    providers: [TreeviewService, UserAuthGuardService, NotificationService,MapService],
    bootstrap: [AppComponent]
})
export class AppModule implements OnInit {

    constructor(
        private router: Router,
    ) { }

    ngOnInit() {
        //this.router.events
        //    .subscribe((event) => {
        //        if (event instanceof NavigationEnd) {
        //            this.headerFooter = (event.url !== '/login')
        //        }
        //    });
    }

}
