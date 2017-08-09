import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from "rxjs/Observable";
import 'rxjs/add/operator/map'

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

    login(username: string, password: string): Observable<boolean> {
        return this.http.post('/api/Login/UserLogin', JSON.stringify({ Username: username, Password: password }))
            .map((response: Response) => {
                // login successful if there's a jwt token in the response

                // Finish up!
                /*let token = response.json() && response.json().token;
                if (token) {
                    // set token property
                    this.token = token;

                    // store username and jwt token in local storage to keep user logged in between page refreshes
                    localStorage.setItem('currentUser', JSON.stringify({ username: username, token: token }));

                    // return true to indicate successful login
                    return true;
                } else {
                    // return false to indicate failed login
                    return false;
                }*/
                return true;
            });
    }

    logout(): void {
        // clear token remove user from local storage to log user out
        this.token = null;
        localStorage.removeItem('currentUser');
    }

}