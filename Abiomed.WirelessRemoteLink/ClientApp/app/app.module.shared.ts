import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { LoginComponent } from './components/login/login.component';
import { EnrollmentComponent } from './components/enrollment/enrollment.component';
import { ForgotPasswordComponent } from "./components/forgotPassword/forgotPassword.component";
import { AdminComponent } from "./components/admin/admin.component";
import { FooterMenuComponent } from "./components/footermenu/footermenu.component";
import { SummaryComponent } from "./components/summary/summary.component";
import { AuthGuard } from "./shared/authguard.service";
import { ResetPasswordComponent } from "./components/resetPassword/resetPassword.component";
import { AuthenticationService } from "./shared/authentication.service";
import { MapComponent } from "./components/map/map.component";
import { CaseComponent } from "./components/case/case.component";
import { SubNavMenuComponent } from "./components/subnavmenu/subnavmenu.component";
import { DeviceManagementComponent } from "./components/deviceManagement/devicemanagement.component";

export const sharedConfig: NgModule = {
    bootstrap: [AppComponent],    
    declarations: [
        AppComponent,
        NavMenuComponent,
        SubNavMenuComponent,
        FooterMenuComponent,
        LoginComponent,
        EnrollmentComponent,
        SummaryComponent,
        ForgotPasswordComponent,
        ResetPasswordComponent,
        AdminComponent,
        MapComponent,
        CaseComponent,
        DeviceManagementComponent
    ],
    imports: [
        RouterModule.forRoot([
            { path: 'login', component: LoginComponent },
            { path: 'enrollment', component: EnrollmentComponent, canActivate:[AuthGuard]},
            { path: 'forgot-password', component: ForgotPasswordComponent },
            { path: 'reset-password/:id/:token', component: ResetPasswordComponent },
            { path: 'admin', component: AdminComponent, canActivate: [AuthGuard] },
            { path: 'device-management', component: DeviceManagementComponent, canActivate: [AuthGuard] },
            { path: 'summary', component: SummaryComponent, canActivate: [AuthGuard] },
            { path: 'case/:serial', component: CaseComponent, canActivate: [AuthGuard] },
            { path: 'map', component: MapComponent, canActivate: [AuthGuard] },
            { path: '', redirectTo: '/login', pathMatch: 'full' },
            { path: '**', redirectTo: '' }
        ])
    ]
};
