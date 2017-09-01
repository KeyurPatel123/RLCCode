import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs/Observable";
import 'rxjs/add/operator/map'
import { AuthenticationInterface } from "../../shared/authentication.interface";

@Injectable()
export class AuthenticationService {
    public token: string;

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
                return response;
            });
    }

    logout(): void {
        // clear token remove user from local storage to log user out
        this.token = null;
        //localStorage.removeItem('currentUser');
    }

    acceptTAC(): Observable<boolean > {
        return this.http.post('/api/Authentication/AcceptTAC','')
            .map((response: boolean) => {
                return response;
            });
    }

    createUser(): void {
       // return this.http.post('/api/Authentication/Login', JSON.stringify({ Username: username, Password: password }))
       //     .map((response: Response) => {
       //
       //         return true;
       //     });
    }
}