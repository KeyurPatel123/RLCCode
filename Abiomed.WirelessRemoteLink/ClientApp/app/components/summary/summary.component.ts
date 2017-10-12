import { Component, OnInit} from '@angular/core';
import { Router } from '@angular/router';
import { DeviceService } from "../../shared/device.service";

@Component({
    selector: 'summary',
    templateUrl: './summary.component.html',
    styleUrls: ['./summary.component.css'],    
})
export class SummaryComponent implements OnInit {
    deviceList:any;
    constructor(private router: Router, private deviceService: DeviceService) { }

    ngOnInit() {
        this.deviceService.GetDevices().subscribe(result => {
            this.deviceList = Array.from(result);    
        });           
    }  
}


