import { Component, OnInit} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { Router} from '@angular/router';

@Component({
    selector: 'admin',
    templateUrl: './admin.component.html',
    styleUrls: ['./admin.component.css', '../../assets/css/AbiomedBase.css'],    
    providers: [AuthenticationService]
})
export class AdminComponent{
    username: string;
    password: string;    

    constructor(private authenticationService: AuthenticationService, private router: Router) { }
    
}


