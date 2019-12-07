import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LoginModel } from '../models/LoginModel';
import { LoginService } from './LoginService';
import { NotificationService } from '../services/utility/NotificationService';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
/** login component*/
export class LoginComponent implements OnInit{

    ngOnInit(): void {
        localStorage.clear();
    }


    private _loginservice;
    output: any;

    actionButtonLabel: string = 'Retry';
    action: boolean = false;
    setAutoHide: boolean = true;
    autoHide: number = 2000;

    /** login ctor */
    constructor(private _Route: Router, loginservice: LoginService) {
        this._loginservice = loginservice;
    }

    LoginModel: LoginModel = new LoginModel();

    onSubmit() {
        this._loginservice.validateLoginUser(this.LoginModel).subscribe(
            response => {
                if (response.token == null && response.userTypeId == "0") {
               
                    //this._Route.navigate(['Login']);
                }

                if (response.userTypeId == 1) {
                   
                    this._Route.navigate(['/Admin/Dashboard']);
                }

                if (response.userTypeId == 2) {
                   
                    this._Route.navigate(['/home']);  // It should be Manager/Dashboard
                }
            });

    }
}
