import { Component, OnInit} from '@angular/core';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { AuthenticationService } from "../../shared/authentication.service";
import { ActivatedRoute } from "@angular/router";
import { Observable } from "rxjs/Observable";

@Component({
    selector: 'resetPassword',
    templateUrl: './resetPassword.component.html',
    styleUrls: ['./resetPassword.component.css'],
    providers: [AuthenticationService]
})

export class ResetPasswordComponent implements OnInit {        
    id: string;
    token: string;
    password: string;
    passwordForm: FormGroup;    
    
    constructor(private authenticationService: AuthenticationService, private route: ActivatedRoute) { }

    ngOnInit(): void {
        //this.route.params.subscribe(params => {
        //    this.id = params['id'];
        //    this.token = params['token'];
        //    console.log(this.token);
        //});        
        this.validateForm();
    }

    private validateForm() {
        this.passwordForm = new FormGroup({
            password: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,}$/)]),
            passwordVerify: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{8,}$/)]),
        }, this.areEqual);
    }

    private areEqual(group: FormGroup) {        
        var password = group.controls['password'].value;
        var passwordVerify = group.controls['passwordVerify'].value;

        return password === passwordVerify ? null : { notSame: true }         
    }

    private ResetPasswordForm() {
        this.id = "U_AAGNELLO_40ABIOMED.COM";
        this.token = "CfDJ8GnGsS+kgw1Kksy/YWg2WMZXHPH6TgtxLPF0Fb+fwYi1dDm/1RJAQwbrc1AtcykShX17B9/ioUKTCoLXTyXGyZQU7sjWgrtvc4KyQUlP7ea5ajBvgp06Ge+ZqL04GpLTylxgadj2O1UjrBJz/r7gIIw="
        this.authenticationService.resetPassword(this.id, this.token, this.password).subscribe(result => {
            // todo display message
            console.log(result);
        });                
    }

}

