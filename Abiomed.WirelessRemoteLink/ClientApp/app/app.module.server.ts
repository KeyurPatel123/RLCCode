import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { sharedConfig } from './app.module.shared';
import { DOCUMENT } from '@angular/common';

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    imports: [
        ServerModule,
        FormsModule,
        ReactiveFormsModule,
        NgbModule.forRoot(),
        ...sharedConfig.imports
    ]
})
export class AppModule {
}
