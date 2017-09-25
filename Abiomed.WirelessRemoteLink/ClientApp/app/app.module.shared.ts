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
import { ResetPasswordComponent } from "./components/resetPassword/resetPassword.component";

export const sharedConfig: NgModule = {
    bootstrap: [ AppComponent ],    
    declarations: [
        AppComponent,
        NavMenuComponent,
        FooterMenuComponent,
        LoginComponent,
        EnrollmentComponent,
        SummaryComponent,
        ForgotPasswordComponent,
        ResetPasswordComponent,
        AdminComponent
    ],
    imports: [
        RouterModule.forRoot([
            { path: 'login', component: LoginComponent },
            { path: 'enrollment', component: EnrollmentComponent },
            { path: 'forgot-password', component: ForgotPasswordComponent },
            { path: 'reset-password', component: ResetPasswordComponent },
            { path: 'admin', component: AdminComponent },
            { path: 'summary', component: SummaryComponent},
            { path: '', redirectTo: '/login', pathMatch: 'full' },
            { path: '**', redirectTo: 'login' }
        ])
    ]
};
