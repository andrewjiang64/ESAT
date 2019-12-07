import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs'
import { catchError, tap } from 'rxjs/operators'
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse } from '@angular/common/http';
import { LoginModel } from '../models/LoginModel';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { NotificationService } from '../services/utility/notificationService';

@Injectable({
    providedIn: 'root'
})

export class LoginService {
    public token: string;
    private _notificationService;

    constructor(private _http: HttpClient, private _Route: Router, private notificationService: NotificationService) {
        this._notificationService = notificationService;
    }
    private apiUrl = window.location.origin + "/api/Authenticate/";

    public validateLoginUser(loginmodel: LoginModel) {

        const headers = new HttpHeaders().set('Content-Type', 'application/json')
            .set("Access-Control-Allow-Origin", "*");

        return this._http.post<any>(this.apiUrl, loginmodel, { headers: headers })
            .pipe(tap(data => {

                if (data.token != null) {
                    if (data.userTypeId == 1) {
                        // store username and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('AdminUser', JSON.stringify({ username: loginmodel.Username, userId: loginmodel.UserId, token: data.token, userTypeId: data.userTypeId, organizationName: data.organizationName   }));
                    }
                    else if (data.userTypeId == 2) {
                        // store username and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('ManagerUser', JSON.stringify({ username: loginmodel.Username, userId: data.userId, token: data.token, userTypeId: data.userTypeId, organizationName: data.organizationName   }));
                    }
                    else if (data.userTypeId == 0) {
                        this._notificationService.showError(data.loginFailedMessage ,"");
                    }
                    // return true to indicate successful login
                    return data;
                } else {
                    this._notificationService.showError(data.loginFailedMessage, "");
                    // return false to indicate failed login
                    return null;
                }
            }),
                //catchError(this.handleError)
            );
    }

    LogoutUser() {
        localStorage.removeItem('ManagerUser');
    }

    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            // A client-side or network error occurred. Handle it accordingly.
            console.error('An error occurred:', error.error.message);
            //this.notificationService.showError('An error occurred:' + error.error.message, "Login Error");
        } else {
            // The backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong,
            //this.notificationService.showError(error.message, "Login Error");
            console.error(`Backend returned code ${error.status}, ` + `body was: ${error.error}`);
        }
        // return an observable with a user-facing error message
        return throwError('Something bad happened; please try again later.');
    };
}
