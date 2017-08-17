import { Component, OnInit} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { Router} from '@angular/router';

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css', '../../assets/css/AbiomedBase.css'],
    providers: [AuthenticationService]
})

export class LoginComponent{
    username: string;
    password: string;    

    constructor(private authenticationService: AuthenticationService, private router: Router) { }
    
    ngOnInit() {
        // reset login status
        this.authenticationService.logout();
    }

    public LogIn() {               
        this.authenticationService.login(this.username, this.password)
            .subscribe(result => {
                // Fix up!
                //if (result === true) {
                //    this.router.navigate(['/']);
                //} else {
                //    this.error = 'Username or password is incorrect';
                //    this.loading = false;
                //}
            });        
    }

    public Enroll() {
        this.router.navigate(['/enrollment']);
    }
}
