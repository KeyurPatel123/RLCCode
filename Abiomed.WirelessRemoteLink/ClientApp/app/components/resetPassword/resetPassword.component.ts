import { Component} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";

@Component({
    selector: 'resetPassword',
    templateUrl: './resetPassword.component.html',
    styleUrls: ['./resetPassword.component.css'],
    providers: [AuthenticationService]
})

export class ResetPasswordComponent{        
    
}
