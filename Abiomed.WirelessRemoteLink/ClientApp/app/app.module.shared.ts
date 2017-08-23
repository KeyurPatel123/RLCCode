import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AppComponent } from './components/app/app.component'
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { LoginComponent } from './components/login/login.component';
import { EnrollmentComponent } from './components/enrollment/enrollment.component';
import { ForgotPasswordComponent } from "./components/forgotPassword/forgotPassword.component";
import { AdminComponent } from "./components/admin/admin.component";

export const sharedConfig: NgModule = {
    bootstrap: [ AppComponent ],    
    declarations: [
        AppComponent,
        NavMenuComponent,
        LoginComponent,
        EnrollmentComponent,
        ForgotPasswordComponent,
        AdminComponent
    ],
    imports: [
        RouterModule.forRoot([
            { path: 'login', component: LoginComponent },
            { path: 'enrollment', component: EnrollmentComponent },
            { path: 'forgot-password', component: ForgotPasswordComponent },
            { path: 'admin', component: AdminComponent },
            { path: '', redirectTo: 'login', pathMatch: 'full' },
            { path: '**', redirectTo: 'login' }
        ])
    ]
};
