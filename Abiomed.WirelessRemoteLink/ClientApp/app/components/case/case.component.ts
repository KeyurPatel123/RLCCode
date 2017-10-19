/// <reference path="../../../../node_modules/@types/jwplayer/index.d.ts" />
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { CaseService } from "../../shared/case.service";

@Component({
    selector: 'case',
    templateUrl: './case.component.html',
    styleUrls: ['./case.component.css'],    
})
export class CaseComponent implements OnInit {
    serial: string;
    device: any;

    constructor(private route: ActivatedRoute, private caseService: CaseService) { }
    ngOnInit() {
        this.route.paramMap.subscribe((paramMap: Params) => {
            this.serial = paramMap.params.serial;
            this.getDevice();
            this.startVideo(this.serial);
        });                   
    }

    getDevice() {
               
    }

    startVideo(serial:string)
    {        
        var playerInstance = jwplayer("playerElement");
        playerInstance.setup({
            playlist: [{
                sources: [
                    { file: "rtsp://rlv.abiomed.com:443/live/" +  serial},
                    { file: "rtmps://rlv.abiomed.com:443/live/" + serial },
                    { file: "https://rlv.abiomed.com:443/live/" + serial + "/playlist.m3u8" },
                ],
            }],
            width: "100%",
            aspectratio: "4:3",
            autostart: true,
            stretching: 'exactfit',
            preload: "none",
            androidhls: true,
            primary: "flash",
            rtmp: {
                bufferlength: 1
            }
        });
       
    }
}




