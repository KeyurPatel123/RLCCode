import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { sharedConfig } from './app.module.shared';
import { DOCUMENT } from '@angular/common';
import { AuthGuard } from "./shared/authguard.service";
import { AuthenticationService } from "./shared/authentication.service";
import { StorageService } from "./shared/storage.service";
import { AgmCoreModule } from '@agm/core';

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    providers: [AuthGuard, AuthenticationService, StorageService],
    imports: [
        ServerModule,
        FormsModule,
        ReactiveFormsModule,
        NgbModule.forRoot(),
        AgmCoreModule.forRoot({
            apiKey: 'AIzaSyBcQ2QU5FUJS9r3zoGIiSxskY0kqWLtycc'
        }),        
        ...sharedConfig.imports
    ]
})
export class AppModule {
}
