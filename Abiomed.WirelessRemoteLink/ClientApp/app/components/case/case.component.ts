/// <reference path="../../../../node_modules/@types/jwplayer/index.d.ts" />
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { CaseService } from "../../shared/case.service";
import { Subscription } from "rxjs/Subscription";

@Component({
    selector: 'case',
    templateUrl: './case.component.html',
    styleUrls: ['./case.component.css'],    
})
export class CaseComponent implements OnInit {
    pumpSerial: string;
    case: any;
    subscription: Subscription;
    videoPlaying: boolean = false;

    constructor(private route: ActivatedRoute, private caseService: CaseService) { }
    ngOnInit() {
        this.route.paramMap.subscribe((paramMap: Params) => {
            this.pumpSerial = paramMap.params.serial;
            this.getCase();

            this.subscription = this.caseService.caseUpdate$.subscribe(
                event => this.getCase()
            );
        });                   
    }    

    getCase() {        
        // Get Case
        var caseTry = this.caseService.GetActiveCase(this.pumpSerial);

        if (caseTry !== undefined) {            
            this.case = caseTry;
            console.log(caseTry.value.connectionStartUtc);
            // Check if video playing
            if (!this.videoPlaying) {
                this.videoPlaying = true;
                this.startVideo(this.case.value.remoteLinkSerialNumber);
            }
        }
    }

    startVideo(serial:string)
    {        
        var playerInstance = jwplayer("playerElement");
        playerInstance.setup({
            playlist: [{
                sources: [
                    { file: "https://rlv.abiomed.com:443/live/" + serial + "/playlist.m3u8" },
                    { file: "rtsp://rlv.abiomed.com:443/live/" + serial },
                    { file: "rtmps://rlv.abiomed.com:443/live/" + serial }                    
                ],
            }],
            width: "100%",
            aspectratio: "4:3",
            autostart: true,
            stretching: 'uniform',
            mute: 'true',
            preload: "none",
            androidhls: true,
            primary: "flash",
            rtmp: {
                bufferlength: 1
            }
        });
       
    }
}




