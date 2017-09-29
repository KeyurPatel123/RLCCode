import { Component, OnInit, ViewChild, Inject} from '@angular/core';
import { Router} from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { NgbModal, ModalDismissReasons, NgbTooltip, NgbModalRef} from "@ng-bootstrap/ng-bootstrap";
import { DOCUMENT } from "@angular/common";
import { AuthenticationService } from "../../shared/authentication.service";

@Component({
    selector: 'login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css', '../../assets/css/AbiomedBase.css'],    
    providers: [AuthenticationService]
})

export class LoginComponent implements OnInit  {
    username: string;
    password: string;   
    errorMessage: string;
    closeResult: string;
    loginForm: FormGroup;
    loginError: boolean;
    modalRef: NgbModalRef;

    constructor(private authenticationService: AuthenticationService, private router: Router, private modalService: NgbModal) { }

    ngOnInit() {
        this.loginError = false;
        this.authenticationService.logout();
        this.validateForm();        
    }
   
    private validateForm() {
        this.loginForm = new FormGroup({
            username: new FormControl('', [Validators.required, Validators.email]),           
            password: new FormControl('', [Validators.required]),           
        });
    }

    public LogIn(modal) {       
        // Reset Error flag and try to login
        this.loginError = false;
        this.authenticationService.login(this.username, this.password)
            .subscribe(result => {                
                if (result.isSuccess) {                    
                    if (result.viewedTermsAndConditions !== true)
                    {
                        this.OpenTermsAndConditionsModal(modal);
                    }
                    else
                    {
                        this.routeUser();
                    }                    
                }
                else 
                {
                    this.loginError = true;
                    this.errorMessage = result.response;
                }
            });                
    }

    public Enroll() {
        //this.router.navigate(['/admin']);
    }

    public Demo() {
        // Todo update!
        //this.router.navigate(['/summary']);
    }

    public OpenTermsAndConditionsModal(modal) {
        this.modalRef = this.modalService.open(modal);        
    }

    public AcceptTAC() {
        this.authenticationService.acceptTAC()
            .subscribe(result => {
                this.modalRef.close();
                this.routeUser();
            });                
    }

    private routeUser()
    {
        this.router.navigate(['/summary']);
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
