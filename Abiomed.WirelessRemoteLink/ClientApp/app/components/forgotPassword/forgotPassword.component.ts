import { Component, OnInit} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
    selector: 'forgotPassword',
    templateUrl: './forgotPassword.component.html',
    styleUrls: ['./forgotPassword.component.css'],
    providers: [AuthenticationService]
})

export class ForgotPasswordComponent implements OnInit {
    loginForm: FormGroup;

    ngOnInit(): void {
        this.validateForm();        
    }

    private validateForm() {
        this.loginForm = new FormGroup({
            username: new FormControl('', [Validators.required, Validators.email]),
        });
    }
}
