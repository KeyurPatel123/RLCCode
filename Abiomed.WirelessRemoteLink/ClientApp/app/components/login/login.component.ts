import { Component, Inject} from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent{
    constructor(private http: Http, @Inject('ORIGIN_URL') private originUrl: string) {       
    }

    public LogIn() {

        this.http.post(this.originUrl + '/api/Login/Login', "Alex").subscribe(result => {
            console.log(result);
            //this.forecasts = result.json() as WeatherForecast[];
        });
    }
}
