import { Component, OnInit} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';


@Component({
    selector: 'resetPassword',
    templateUrl: './resetPassword.component.html',
    styleUrls: ['./resetPassword.component.css'],
    providers: [AuthenticationService]
})

export class ResetPasswordComponent implements OnInit {        
    loginForm: FormGroup;

    ngOnInit(): void {
        this.validateForm();
    }

    private validateForm() {
        this.loginForm = new FormGroup({
            password: new FormControl('', [Validators.required, Validators.email]),
            passwordVerify: new FormControl('', [Validators.required, Validators.email]),
        });
    }

}

