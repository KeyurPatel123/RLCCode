import { Component, OnInit} from '@angular/core';
import { Router} from '@angular/router';
import { GoogleMapsAPIWrapper, MarkerManager, AgmMarker } from "@agm/core";
import { MarkerOptions } from "@agm/core/services/google-maps-types";


@Component({
    selector: 'map',
    templateUrl: './map.component.html',
    styleUrls: ['./map.component.css'],
    providers: [GoogleMapsAPIWrapper , MarkerManager]
})
export class MapComponent implements OnInit {
    
    constructor(private markerManager: MarkerManager, private googleMapsAPIWrapper: GoogleMapsAPIWrapper) { }
    ngOnInit(): void {
        /*
        let marker: AgmMarker = new AgmMarker(this.markerManager);
        marker.latitude = 40;
        marker.longitude = -70;
        this.markerManager.addMarker(marker);

        let m: MarkerOptions;// = new MarkerOptions();
        m.position.lat = 35;
        m.position.lng = -70;
        this.googleMapsAPIWrapper.createMarker(m);
        */
    }       

    title: string = 'My first AGM project';
    lat: number = 42.5769634;
    lng: number = -70.9191885;    


     

    click(data): void {
            console.log(data);
    }
}


