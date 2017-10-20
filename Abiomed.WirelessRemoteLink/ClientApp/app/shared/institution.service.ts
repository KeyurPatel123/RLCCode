import { Injectable, Inject, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";

@Injectable()
export class InstitutionService {
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) {}    

    GetInstitutions(): Observable<InstitutionInterface[]> {
        return this.http.get('/api/Institution/GetInstitutions')
            .map((response: InstitutionInterface[]) => {
                console.log(response);
                return response;
            });
    }
}

export interface InstitutionInterface {
    Id: string;
    DisplayName: string;
}