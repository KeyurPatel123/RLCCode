import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs/Observable";
import 'rxjs/add/operator/map'
import { AuthenticationInterface, UserRegistrationInterface } from "../../shared/authentication.interface";

@Injectable()
export class AuthenticationService {   
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
    ) {        
        // set token if saved in local storage
     //   var currentUser = JSON.parse(localStorage.getItem('currentUser'));
  //      this.token = currentUser && currentUser.token;
    }

    login(username: string, password: string): Observable<AuthenticationInterface> {
        return this.http.post('/api/Authentication/Login', JSON.stringify({ Username: username, Password: password }))
            .map((response: AuthenticationInterface) => {
                if (response.isSuccess)
                {
                    if (typeof window !== 'undefined') {
                        sessionStorage.setItem("loggedIn", "true");
                        sessionStorage.setItem("role", response.role);
                    }
                }
                return response;
            });
    }

    logout(): void {
        if (typeof window !== 'undefined') {
            // clear token remove user from local storage to log user out
            sessionStorage.setItem("loggedIn", "false");
        }
    }

    getLoggedIn(): boolean {
        var isTrue = false;
        if (typeof window !== 'undefined') {
            isTrue = (sessionStorage.getItem("loggedIn") == 'true');
        }
        return isTrue;
    }

    getRole(): string {
        var role = '';
        if (typeof window !== 'undefined') {
            role = sessionStorage.getItem("role");
        }
        return role;
    }

    acceptTAC(): Observable<boolean > {
        return this.http.post('/api/Authentication/AcceptTAC','')
            .map((response: boolean) => {
                return response;
            });
    }

    registerUser(userRegistration: UserRegistrationInterface): Observable<any> {
        var json = JSON.stringify(userRegistration);
        return this.http.post('/api/Authentication/Register', userRegistration)            
            .map((response: any) => {
                return response;
            });
    }
}