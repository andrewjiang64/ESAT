import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

@Injectable()
export class UserOverviewService {

    private BASE_URL = 'api/OverviewData';
    private userdata: any;
    constructor(private http: HttpClient) {

    }

    public GetBaseLineOptions() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getBaseLineOptionsUrl = `${this.BASE_URL}/GetBaseLineOptions`;

        let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
        headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);

        return this.http.get<any>(getBaseLineOptionsUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetWatershedRange(baselineValue) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetStartAndEndYearByUserIdAndUserType/${this.userdata.userId}/${baselineValue}/${this.userdata.userTypeId}/false`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
    }

    public GetMunicipalitiesByUserId() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetMunicipalitiesByUserId/${this.userdata.userId}/${this.userdata.userTypeId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        //headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);
        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }


    public GetWatershedsByMunicipality(municipalityId) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetWatershedsByMunicipality/${this.userdata.userId}/${this.userdata.userTypeId}/${municipalityId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        //headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);
        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetSubWatershedsByWatershedId(municipalityId, watershedId) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetSubWatershedsByWatershedId/${this.userdata.userId}/${this.userdata.userTypeId}/${municipalityId}/${watershedId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        //headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);
        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }


    public GetScenarioResultSummarizationType() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getBaseLineOptionsUrl = `${this.BASE_URL}/GetScenarioResultSummarizationType`;

        let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
        headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);

        return this.http.get<any>(getBaseLineOptionsUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetSummaryGridData(baselineId, summerizationLevelId, locationFilter_MunicipalityId, locationFilter_WatershedId, locationFilter_SubwatershedId, startYear, endYear) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getBaseLineOptionsUrl = `${this.BASE_URL}/GetOverviewSummaryResultByParameters/${baselineId}/${summerizationLevelId}/${locationFilter_MunicipalityId}/${locationFilter_WatershedId}/${locationFilter_SubwatershedId}/${startYear}/${endYear}/${this.userdata.userId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*' });
        
        return this.http.get<any>(getBaseLineOptionsUrl, { headers: headers }).pipe(tap(data => data),
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
