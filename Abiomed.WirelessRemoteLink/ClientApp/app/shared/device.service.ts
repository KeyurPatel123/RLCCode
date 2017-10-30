import { Injectable, Inject, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";

@Injectable()
export class DeviceService {
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) {}    

    GetDevices(): Observable<any[]> {
        return this.http.get('/api/Device/GetDevices')
            .map((response: any[]) => {
                return response;
            });
    }

    CreateDevice(rlmsn: string, rlmpw: string, institutionSAPId: string, aicserialnumber: string, aicsoftwarenumber:string ): Observable<any> {
        return this.http.post('/api/Device/CreateDevice', JSON.stringify({ Rlmsn: rlmsn, Rlmpw: rlmpw, InstitutionSAPId: institutionSAPId, Aicserialnumber: aicserialnumber, Aicsoftwarenumber: aicsoftwarenumber }))
            .map((response: any) => {                
                return response;
            });
    }
}