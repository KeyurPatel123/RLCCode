import { Component, OnInit} from '@angular/core';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { AuthenticationService } from "../../shared/authentication.service";
import { ActivatedRoute, Params } from "@angular/router";
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
    showResetMessage: boolean;
    resetMessage: string;
    messageCSS: string;
    
    constructor(private authenticationService: AuthenticationService, private route: ActivatedRoute) { }

    ngOnInit(): void {
        
        this.route.paramMap.subscribe((paramMap: Params) => {
            this.id = paramMap.params.id;
            this.token = paramMap.params.token;
        });        
        this.validateForm();
    }

    private validateForm() {
        this.passwordForm = new FormGroup({                                     
            password: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[~`!@#$%^&*()_\-+={}|[\]\\;':",./<>?]).{8,}$/), Validators.pattern(/^\S*$/)]),
            passwordVerify: new FormControl('', [Validators.required, Validators.pattern(/^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[~`!@#$%^&*()_\-+={}|[\]\\;':",./<>?]).{8,}$/), Validators.pattern(/^\S*$/)]),
        }, this.areEqual);
    }

    private areEqual(group: FormGroup) {        
        var password = group.controls['password'].value;
        var passwordVerify = group.controls['passwordVerify'].value;

        return password === passwordVerify ? null : { notSame: true }         
    }

    private ResetPasswordForm() {
        this.authenticationService.resetPassword(this.id, this.token, this.password).subscribe(result => {
            this.showResetMessage = true;
            this.messageCSS = "ServerSuccess";
            if (result) {
                this.resetMessage = "Your password has been updated.";
                this.messageCSS = "ServerSuccess";
            }
            else {
                this.resetMessage = "Error processing, please try again.";
                this.messageCSS = "ServerError";
            }
        });                
    }

}

