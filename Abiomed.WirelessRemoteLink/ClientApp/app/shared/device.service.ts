import { Injectable, Inject } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";

@Injectable()
export class DeviceService {
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) { }

    public GetDevices(): Observable<any> {
        return this.http.get('/api/Device/GetDevices')
            .map((devices: any) => {                
                return devices;
            });
    }
}