import { Injectable, Inject, OnInit, OnDestroy, EventEmitter } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class CaseService implements OnDestroy {
    private activeCases: any;
    private caseTimer: any;
    private caseUpdate = new BehaviorSubject<number>(0);
    caseUpdate$ = this.caseUpdate.asObservable();

    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) {
        this.caseTimer = setInterval(() => {    //<<<---    using ()=> syntax
            this.GetCasesServer();
        }, 10000);
        this.GetCasesServer();
    }    

    ngOnDestroy(): void {
        clearInterval(this.caseTimer);
    }

    private GetCasesServer() {
        this.http.get('/api/Case/GetCases')
            .subscribe((devices: any) => {
                this.activeCases = devices;
                this.caseUpdate.next(0);
            });
    }

    getUpdatedCasesEmitter() {
        return this.caseUpdate;
    }

    GetActiveCases() {
        return this.activeCases;
    }

    GetActiveCase(pumpSerial:string) {

    }
}