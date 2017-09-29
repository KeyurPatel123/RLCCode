import { Component, OnInit} from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { AuthenticationService } from "../../shared/authentication.service";

@Component({
    selector: 'forgotPassword',
    templateUrl: './forgotPassword.component.html',
    styleUrls: ['./forgotPassword.component.css'],
    providers: [AuthenticationService]
})

export class ForgotPasswordComponent implements OnInit {
    forgotForm: FormGroup;
    username: string;
    showForgotMessage: boolean;
    forgotMessage: string;
    messageCSS: string;

    constructor(private authenticationService: AuthenticationService) { }

    ngOnInit(): void {
        this.validateForm();        
    }

    private validateForm() {
        this.forgotForm = new FormGroup({
            username: new FormControl('', [Validators.email]),
        });
    }

    private ForgotPassword()
    {
        this.authenticationService.forgotPassword(this.username).subscribe(result => {
            this.showForgotMessage = true;
            if (result) {
                this.forgotMessage = "An email will be sent to you shortly.";
                this.messageCSS = "ServerSuccess";
            }
            else {
                this.forgotMessage = "Error processing, please try again.";
                this.messageCSS = "ServerError";
            }
        });                
    }
}
