import { Component, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { AuthenticationService } from "../../shared/authentication.service";

@Component({
    selector: 'nav-menu',
    templateUrl: './navmenu.component.html',
    styleUrls: ['./navmenu.component.css'],
    providers: [AuthenticationService]
})

export class NavMenuComponent implements OnInit {
    private role: string;
    private links: NavMenuLink[] = new Array();    

    constructor(private authenticationService: AuthenticationService, private router: Router) { }    

    ngOnInit() {
        this.role = this.authenticationService.getRole();

        if (this.role === "ADMIN")
        {
            var Admin = new NavMenuLink();
            Admin.name = "Admin";
            Admin.link = "/admin"
            this.links.push(Admin);

            var DeviceManagement = new NavMenuLink();
            DeviceManagement.name = "Device Management";
            DeviceManagement.link = "/device-management";
            this.links.push(DeviceManagement);
        }
    }

    public openLink(link: NavMenuLink) {        
        this.router.navigate([link.link]);
    }
}

class NavMenuLink {
    name: string;
    link: string;
}
