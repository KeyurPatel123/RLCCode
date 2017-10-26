import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { DOCUMENT } from '@angular/common';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { sharedConfig } from './app.module.shared';
import { GeneralInterceptor } from "./components/interceptor/general.interceptor";
import { AuthenticationService } from "./shared/authentication.service";
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { MomentModule } from 'angular2-moment';
import "./assets/css/AbiomedBase.css";
import { AuthGuard } from "./shared/authguard.service";
import { StorageService } from "./shared/storage.service";
import { AgmCoreModule } from "@agm/core";
import { CaseService } from "./shared/case.service";
import { InstitutionService } from "./shared/institution.service";

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    imports: [
        BrowserModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        NgbModule.forRoot(),
        MomentModule,
        AgmCoreModule.forRoot({
            apiKey: 'AIzaSyBcQ2QU5FUJS9r3zoGIiSxskY0kqWLtycc'
        }),        
        ...sharedConfig.imports
    ],
    providers: [
        AuthenticationService,
        AuthGuard,
        StorageService,
        CaseService,
        InstitutionService,
        { provide: 'ORIGIN_URL', useValue: location.origin },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: GeneralInterceptor,
            multi: true,
        },        
    ]
})
export class AppModule {
}
