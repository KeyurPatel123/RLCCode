import { Component, OnInit, OnDestroy} from '@angular/core';
import { Router } from '@angular/router';
import { CaseService } from "../../shared/case.service";
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'summary',
    templateUrl: './summary.component.html',
    styleUrls: ['./summary.component.css'],    
})
export class SummaryComponent implements OnInit, OnDestroy {    
    cases: any;
    subscription: Subscription;

    constructor(private router: Router, private caseService: CaseService) { }

    ngOnInit() {
        this.cases = this.caseService.GetActiveCases();

        this.subscription = this.caseService.caseUpdate$.subscribe(
            event => this.cases = this.caseService.GetActiveCases()            
        );
    }  

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    getCases() {

    }    
}


