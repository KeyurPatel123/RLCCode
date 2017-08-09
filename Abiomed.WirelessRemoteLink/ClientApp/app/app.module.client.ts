import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { sharedConfig } from './app.module.shared';
import { GeneralInterceptor } from "./components/interceptor/general.interceptor";
import { AuthenticationService } from "./components/service/authentication.service";

@NgModule({
    bootstrap: sharedConfig.bootstrap,
    declarations: sharedConfig.declarations,
    imports: [
        BrowserModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        ...sharedConfig.imports
    ],
    providers: [
        AuthenticationService,
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
