import { Component, OnInit} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { Router} from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
    selector: 'admin',
    templateUrl: './admin.component.html',
    styleUrls: ['./admin.component.css'],    
    providers: [AuthenticationService]
})
export class AdminComponent implements OnInit {
    createForm: FormGroup;
    firstname: string;
    lastname: string
    phone: string
    email: string;
    role: string;

    constructor(private authenticationService: AuthenticationService) { }

    ngOnInit() {
        this.ValidateForm();
    }

    private ValidateForm() {
        this.createForm = new FormGroup({
            firstname: new FormControl('', [Validators.required]),
            lastname: new FormControl('', [Validators.required]),
            phone: new FormControl(''),
            email: new FormControl('', [Validators.required]),
            role: new FormControl('', [Validators.required]),
        });
    }   

    private Register() {
        //this.authenticationService.            
    }
}


