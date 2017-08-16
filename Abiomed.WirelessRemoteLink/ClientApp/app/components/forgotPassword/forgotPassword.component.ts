import { Component} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";

@Component({
    selector: 'forgotPassword',
    templateUrl: './forgotPassword.component.html',
    styleUrls: ['./forgotPassword.component.css'],
    providers: [AuthenticationService]
})

export class ForgotPasswordComponent{        
    
}
