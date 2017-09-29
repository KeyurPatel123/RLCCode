import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs/Observable";
import 'rxjs/add/operator/map'
import { AuthenticationInterface, UserRegistrationInterface } from "./authentication.interface";
import { StorageService } from "./storage.service";

@Injectable()
export class AuthenticationService {   
    constructor(
        private http: HttpClient,
        @Inject('ORIGIN_URL') private originUrl: string,
        private storageService: StorageService
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
                    this.storageService.SessionSetItem("loggedIn", "true");
                    this.storageService.SessionSetItem("role", response.role);
                }
                return response;
            });
    }

    logout(): void {
        this.storageService.SessionSetItem("loggedIn", "false");
    }

    getLoggedIn(): boolean {
        var isTrue = false;        
        isTrue = (this.storageService.SessionGetItem("loggedIn") == 'true');
        return isTrue;
    }

    getRole(): string {        
        return this.storageService.SessionGetItem("role");
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

    forgotPassword(username: string): Observable<boolean> {
        return this.http.post('/api/Authentication/ForgotPassword', JSON.stringify({ Username: username, Password: '' }))
            .map((response: boolean) => {
                return response;
            });
    }

    resetPassword(id: string, token: string, password: string): Observable<boolean> {
        return this.http.post('/api/Authentication/ResetPassword', JSON.stringify({Id: id, Token: token, Password: password }))
            .map((response: boolean) => {
                return response;
            });
    }
}