import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot } from "@angular/router";
import { AuthenticationService } from "./authentication.service";

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private authenticationService: AuthenticationService, private router: Router) { }    
     
    canActivate(next: ActivatedRouteSnapshot) {        
        // Get path and check if logged in
        var status = false;        
        var loggedIn = this.authenticationService.getLoggedIn();        
        
        if (loggedIn) {
            var path = next.routeConfig.path;
            status = this.CheckRole(path);
        }

        // If status bad, redirect to login screen
        if (status === false)
        {
            this.router.navigate(['/login']);
        }
        return status;
    }

    private CheckRole(path: string): boolean
    {
        var status = true;
        var role = this.authenticationService.getRole();

        switch (path)
        {
            case "admin":                
                status = this.Admin(role);
                break;
        }
        return status;
    }

    /* Page Specific Checks */
    private Admin(role: string): boolean
    {
        var status = false;
        if (role.includes("ADMIN"))
        {
            status = true;
        }
        return status;
    }
}