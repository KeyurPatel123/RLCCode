import { Component, OnInit } from '@angular/core';
import { Router} from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { UserRegistrationInterface } from "../../shared/authentication.interface";
import { AuthGuard } from "../../shared/authguard.service";
import { InstitutionService } from "../../shared/institution.service";
import { DeviceService } from "../../shared/device.service";

@Component({
    selector: 'devicemanagement',
    templateUrl: './devicemanagement.component.html',
    styleUrls: ['./devicemanagement.component.css'],
    providers: [InstitutionService, DeviceService]
})

export class DeviceManagementComponent implements OnInit {
    records: {
        Editable: boolean;
        RLMSerialNo: string;
        RLMPassWord: string
        Institution: string;
        AICSerialNo: string;
        AICSoftwareNo: string;
    }[];
    institutions: any;
    created: boolean;
    creationResponse: any;
    creationStatus: any;
    role: any;
    createForm: FormGroup;
    rlmsn: string;
    rlmpw: string;
    institution: string;
    aicserialnumber: string;
    aicsoftwarenumber: string;
    isCollapsed = true;
    constructor(private institutionService: InstitutionService, private deviceService: DeviceService) { }

    ngOnInit() {
        this.GetInstitutions();
        this.ValidateForm();
        this.created = false;
        this.creationStatus = "";
        this.creationResponse = "";
        this.records = [
            { Editable: false, RLMSerialNo: "RL12345", RLMPassWord: "poi0mnj", Institution: "Danvers", AICSerialNo: "#098765", AICSoftwareNo: "#A56788"},
            { Editable: false, RLMSerialNo: "RL23456", RLMPassWord: "eqwr$35", Institution: "Steward", AICSerialNo: "#987654", AICSoftwareNo: "#A89076"},
            { Editable: false, RLMSerialNo: "RL34567", RLMPassWord: "iyuty7&87", Institution: "Childrens", AICSerialNo: "#876543", AICSoftwareNo: "#A56784"},
            { Editable: false, RLMSerialNo: "RL45678", RLMPassWord: "kjiuu^78", Institution: "MGH", AICSerialNo: "#765432", AICSoftwareNo: "#A98098"}
        ];


     // this.sort(this.column);
    }

    private Edit(item)
    {
        item.Editable = "contenteditable";
        console.log(item);
    }

    private GetInstitutions() {
        this.institutionService.GetInstitutions().subscribe(institutions => {
            this.institutions = institutions;
        });
    }

    private ValidateForm() {
        this.createForm = new FormGroup({
            rlmsn: new FormControl('', [Validators.required]),
            rlmpw: new FormControl('', [Validators.required]),
            institution: new FormControl('', [Validators.required]),
            aicserialnumber: new FormControl('', [Validators.required]),
            aicsoftwarenumber: new FormControl('', [Validators.required])
        });
    }   

    _drop(event: any) {
        event.preventDefault();
    }

    _keyPress(event: any) {
        const pattern = /[0-9]/;
        let inputChar = String.fromCharCode(event.charCode);

        if (!pattern.test(inputChar)) {
            // invalid character, prevent input
            event.preventDefault();
        }
    }

    private Register() {
        this.deviceService.CreateDevice(this.rlmsn, this.rlmpw, this.institution, this.aicserialnumber, this.aicsoftwarenumber);
    }
}

