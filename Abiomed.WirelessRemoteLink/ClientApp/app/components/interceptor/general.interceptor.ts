import { Injectable} from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable } from "rxjs/Observable";
import { StorageService } from "../../shared/storage.service";

@Injectable()
export class GeneralInterceptor implements HttpInterceptor{    
    token: string;
    constructor(private storageService: StorageService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        // Check if token exist
        if (this.token === null || this.token === undefined)
        {
            this.token = this.storageService.SessionGetItem("token");
        }

        var request = req.clone({
            headers: req.headers.set('Content-Type', 'application/json').set('Authorization', 'Bearer ' + this.token)
        });

        return next.handle(request);
    }
}