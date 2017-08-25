import { Component, OnInit, ViewChild, OnDestroy} from '@angular/core';
import { AuthenticationService } from "../service/authentication.service";
import { Router} from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { NgbModal, ModalDismissReasons, NgbTooltip} from "@ng-bootstrap/ng-bootstrap";

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css', '../../assets/css/AbiomedBase.css'],    
    providers: [AuthenticationService]
})

export class LoginComponent implements OnInit, OnDestroy  {
    username: string;
    password: string;    
    closeResult: string;
    loginForm: FormGroup;

    constructor(private authenticationService: AuthenticationService, private router: Router, private modalService: NgbModal) { }

    ngOnInit() {
        document.querySelector('body').style.backgroundColor = '#0E355A';
        this.authenticationService.logout();
        this.validateForm();        
    }
    
    ngOnDestroy() {
        document.querySelector('body').style.backgroundColor = '';
    }
   
    private validateForm() {
        this.loginForm = new FormGroup({
            username: new FormControl('', [Validators.required]),           
            password: new FormControl('', [Validators.required]),           
        });
    }

    public LogIn(modal) {      
        this.OpenTermsAndConditionsModal(modal);
        // Fix up
        //this.router.navigate(['/admin']);
        /*this.authenticationService.login(this.username, this.password)
            .subscribe(result => {
                // Fix up!
                if (result === true) {
                    this.router.navigate(['/']);
                } else {
                    //this.error = 'Username or password is incorrect';
                    //this.loading = false;
                }
            });        
        */
    }

    public Enroll() {
        this.router.navigate(['/admin']);
    }

    public OpenTermsAndConditionsModal(modal) {
        this.modalService.open(modal).result.then((result) => {
            this.closeResult = `Closed with: ${result}`;
        }, (reason) => {
            this.closeResult = `Dismissed ${this.getDismissReason(reason)}`;
        });
    }

    private getDismissReason(reason: any): string {
        if (reason === ModalDismissReasons.ESC) {
            return 'by pressing ESC';
        } else if (reason === ModalDismissReasons.BACKDROP_CLICK) {
            return 'by clicking on a backdrop';
        } else {
            return `with: ${reason}`;
        }
    }
}
