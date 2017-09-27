import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { Router } from "@angular/router";

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
            var temp = new NavMenuLink();
            temp.name = "Admin";
            temp.link = "/admin"
            this.links.push(temp);
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
