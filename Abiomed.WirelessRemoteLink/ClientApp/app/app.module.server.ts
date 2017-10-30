import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { sharedConfig } from './app.module.shared';
import { DOCUMENT } from '@angular/common';
import { AuthGuard } from "./shared/authguard.service";
import { AuthenticationService } from "./shared/authentication.service";
import { StorageService } from "./shared/storage.service";
import { MomentModule } from 'angular2-moment';
import { AgmCoreModule } from '@agm/core';
import { CaseService } from "./shared/case.service";
import { InstitutionService } from "./shared/institution.service";
import { DeviceService } from "./shared/device.service";

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    providers: [AuthGuard, AuthenticationService, StorageService, CaseService, InstitutionService, DeviceService],
    imports: [
        ServerModule,
        FormsModule,
        ReactiveFormsModule,
        NgbModule.forRoot(),
        MomentModule,
        AgmCoreModule.forRoot({
            apiKey: 'AIzaSyBcQ2QU5FUJS9r3zoGIiSxskY0kqWLtycc'
        }),        
        ...sharedConfig.imports
    ]
})
export class AppModule {
}
