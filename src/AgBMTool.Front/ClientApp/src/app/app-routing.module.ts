import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { UserAuthGuardService } from './AuthGuard/UserAuthGuardService';
import { SelectivePreloadingStrategyService } from './selective-preloading-strategy.service';

const appRoutes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'login', redirectTo: "login", pathMatch: 'full' },
  { path: '', redirectTo: "login", pathMatch: 'full', canActivate: [UserAuthGuardService] },
  {
    path: 'home',
    component: HomeComponent,
    loadChildren: () => import('./home/home.module').then(mod => mod.HomeModule),
    canActivate: [UserAuthGuardService]
  }
];

@NgModule({
  imports: [
    RouterModule.forRoot(
      appRoutes,
      {
        enableTracing: false, // <-- debugging purposes only
        preloadingStrategy: SelectivePreloadingStrategyService,
      }
    )
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule { }
