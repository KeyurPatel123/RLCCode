import { Component, OnInit} from '@angular/core';
import { Router} from '@angular/router';

@Component({
    selector: 'map',
    templateUrl: './map.component.html',
    styleUrls: ['./map.component.css'],    
})
export class MapComponent implements OnInit {
    
    constructor() { }
    ngOnInit(): void {
    }       
    title: string = 'My first AGM project';
    lat: number = 42.5769634;
    lng: number = -70.9191885;    
}


