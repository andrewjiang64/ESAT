import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

@Injectable()
export class ProjectBMPCostEffectivenessChartService {

    private BASE_URL = 'api/ProjectData';
    private userdata: any;

    constructor(private http: HttpClient) {

    }

    public GetProjectBMPCostEffectivenessChartData(costEffectivenessSelectedValue) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var url = `${this.BASE_URL}/GetProjectsBMPCostByEffectivenessTypeId/${this.userdata.userId}/${costEffectivenessSelectedValue}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(url, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetBMPEffectivenessType() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var url = `${this.BASE_URL}/GetBMPEffectivenessType`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(url, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }


    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            // A client-side or network error occurred. Handle it accordingly.
            console.error('An error occurred:', error.error.message);
        } else {
            // The backend returned an unsuccessful response code.
            // The response body may contain clues as to what went wrong,
            console.error(`Backend returned code ${error.status}, ` + `body was: ${error.error}`);
        }
        // return an observable with a user-facing error message
        return throwError('Something bad happened; please try again later.');
    };
}
