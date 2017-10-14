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
        setInterval(() => {    //<<<---    using ()=> syntax
            this.getDevices();
        }, 10000);

        this.getDevices();        
    }  

    getDevices() {
        this.deviceService.GetDevices().subscribe(result => {
            var devices:any = Array.from(result); 
            this.deviceList = devices.filter(function (element) {
                return element.value.pumpSerialNumber !== ""; 
            });            
        });      
    }
}


