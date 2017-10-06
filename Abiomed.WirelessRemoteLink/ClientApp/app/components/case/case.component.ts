/// <reference path="../../../../node_modules/@types/jwplayer/index.d.ts" />
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'case',
    templateUrl: './case.component.html',
    styleUrls: ['./case.component.css'],    
})
export class CaseComponent implements OnInit {
    constructor() { }
    ngOnInit() {
        this.startVideo();           
    }

    startVideo()
    {        
        var playerInstance = jwplayer("playerElement");
        playerInstance.setup({
            playlist: [{
                sources: [
                    { file: "rtmps://rlv.abiomed.com:443/live/RL00015" },
                    { file: "https://rlv.abiomed.com:443/live/RL00015/playlist.m3u8" },
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




