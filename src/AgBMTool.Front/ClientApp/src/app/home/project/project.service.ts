import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

@Injectable()
export class ProjectService {

    private BASE_URL = 'api/ProjectData';
    private userdata: any;

    constructor(private http: HttpClient) {

    }

    public GetProjectsListByUserId() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getProjectsUrl = `${this.BASE_URL}/GetProjectListByUserId/${this.userdata.userId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        //headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);
        return this.http.get<any>(getProjectsUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }


    public GetSliderRange() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `api/OverviewData/GetStartAndEndYearByUserIdAndUserType/${this.userdata.userId}/${this.userdata.userTypeId}/0/true`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
    }


    public GetSummarizationTypeList() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetSummarizationTypeList`;

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
        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetWatershedByMunicipalityId(municipalityId) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getUrl = `${this.BASE_URL}/GetWatershedByMunicipalityId/${this.userdata.userId}/${this.userdata.userTypeId}/${municipalityId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetBaseLineOptions() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getBaseLineOptionsUrl = `api/OverviewData/GetBaseLineOptions`;

        let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
        headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);

        return this.http.get<any>(getBaseLineOptionsUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public GetProjectSpatialUnitType() {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getProjectsUrl = `${this.BASE_URL}/GetProjectSpatialUnitType`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });
        return this.http.get<any>(getProjectsUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)

        );
    }

    public SaveProject(addProject) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getProjectsUrl = `${this.BASE_URL}/SaveProject/${this.userdata.userId}`;
        let _body = JSON.stringify(addProject);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getProjectsUrl, JSON.stringify(_body), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
    }

    public DeleteProjectById(projectId) {
        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
        var getProjectsUrl = `${this.BASE_URL}/DeleteProject/${projectId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.delete<any[]>(getProjectsUrl, { headers: headers }).pipe(tap(data => data),
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
