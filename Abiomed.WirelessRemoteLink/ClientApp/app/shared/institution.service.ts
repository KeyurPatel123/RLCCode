import { Injectable, Inject, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";

@Injectable()
export class InstitutionService {
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) {}    

    GetInstitutions(): Observable<any[]> {
        return this.http.get('/api/Institution/GetInstitutions')
            .map((response: any[]) => {
                console.log(response);
                return response;
            });
    }
}