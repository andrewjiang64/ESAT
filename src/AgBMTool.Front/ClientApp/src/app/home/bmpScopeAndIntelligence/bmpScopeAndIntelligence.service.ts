import { Injectable } from '@angular/core';import { HttpClient, HttpErrorResponse, HttpHeaders, HttpResponse, HttpParams } from '@angular/common/http';import { Observable, throwError } from 'rxjs';import { catchError, tap } from 'rxjs/operators';@Injectable()export class BMPScopeAndIntelligenceService {    private BASE_URL = 'api/BMPScopeAndIntelligenceData';    private BaselineInfo_BASE_URL = 'api/BaselineInformationData';    private Project_BASE_URL = 'api/ProjectData';    private userdata: any;    constructor(private http: HttpClient) {    }    public CheckIfBMPsSelectedinProject(projectId, bmptypeId, locationType) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/CheckIfBMPsSelectedinProject/${projectId}/${bmptypeId}/${locationType}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token        });        return this.http.get<boolean>(getUrl, { headers: headers }).pipe(catchError(this.handleError));
    }    public GetBMPDefaultSelectionColorByBMPType(bmptype) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetBMPDefaultSelectionColorByBMPType/${bmptype}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token        });        return this.http.get<string>(getUrl, { headers: headers, responseType: 'text' as 'json' }).pipe(catchError(this.handleError));    }    public GetLocationTypeByProjectIdAndBMPTypeId(projectId, bmptypeId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetLocationTypeByProjectIdAndBMPTypeId/${projectId}/${bmptypeId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token        });        return this.http.get<string>(getUrl, { headers: headers, responseType: 'text' as 'json' }).pipe(catchError(this.handleError));    }    public GetProjectMunicipalitiesByProjectId(projectId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetProjectMunicipalitiesByProjectId/${projectId}/${this.userdata.userId}/${this.userdata.userTypeId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetProjectWatershedsByMunicipality(projectId, municipalityId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetProjectWatershedsByMunicipality/${projectId}/${this.userdata.userId}/${this.userdata.userTypeId}/${municipalityId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetSubWatershedsByWatershedId(municipalityId, watershedId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BaselineInfo_BASE_URL}/GetSubWatershedsByWatershedId/${this.userdata.userId}/${this.userdata.userTypeId}/${municipalityId}/${watershedId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }  public GetInvestorList(projectId, bmptypeId) {    this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));    var getUrl = `${this.BASE_URL}/GetInvestorList/${this.userdata.userId}/${projectId}/${bmptypeId}`;    let headers = new HttpHeaders({      Authorization: 'Bearer ' + this.userdata.token,      'Content-Type': 'application/json',      Accept: 'application/json, text/plain, */*'    });    return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),      catchError(this.handleError)    );  }    public GetSubAreaModelResultType() {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetSubAreaModelResultType`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetOptimizationTypeList(projectId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetOptimizationTypeList/${projectId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetSummaryGridData(projectId, bmptypeId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetSummaryGridData/${projectId}/${bmptypeId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetScenarioModelResultVariableTypes() {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetScenarioModelResultVariableTypes`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetBMPEffectivenessLocationType() {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetBMPEffectivenessLocationType`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetBMPEffectivenessTypeListByProjectId(projectId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetBMPEffectivenessTypeByProjectId/${projectId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public GetOptimizationConstraintValueTypeList() {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/GetOptimizationConstraintValueTypeList`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),            catchError(this.handleError)        );    }    public SaveOptimizationType(projectId, optimizationTypeId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/SaveOptimizationType/${projectId}/${optimizationTypeId}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json;charset=utf-8',            Accept: 'application/json, text/plain, */*'        });

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public SaveWeight(projectId, optimizationWeights) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/SaveWeight/${projectId}`;
        let optimizationWeightsStr = JSON.stringify(optimizationWeights);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationWeightsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public SaveConstraint(projectId, optimizationConstraints) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/SaveConstraint/${projectId}`;
        let optimizationConstraintsStr = JSON.stringify(optimizationConstraints);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationConstraintsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public DeleteConstraint(projectId, bmpEffectivenessTypeId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/DeleteConstraint/${projectId}/${bmpEffectivenessTypeId}`;
        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public SaveBudget(projectId, budget) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/SaveBudget/${projectId}/${budget}`;        let headers = new HttpHeaders({            Authorization: 'Bearer ' + this.userdata.token,            'Content-Type': 'application/json',            Accept: 'application/json, text/plain, */*'        });

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public RunIntelligentRecommendation(projectId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/RunIntelligentRecommendation/${projectId}`;
        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public SaveLegalSubDivisions(projectId, isOptimization,LSD) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));      var getUrl = `${this.BASE_URL}/SaveLegalSubDivisions/${projectId}/${isOptimization}`;
      let optimizationLSDStr = JSON.stringify(LSD);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationLSDStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }  public SaveParcels(projectId, isOptimization, parcels) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));    var getUrl = `${this.BASE_URL}/SaveParcels/${projectId}/${isOptimization}`;
    let optimizationParcelsStr = JSON.stringify(parcels);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationParcelsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }  public SaveModelComponents(projectId, isOptimization, modelComponents) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));    var getUrl = `${this.BASE_URL}/SaveModelComponents/${projectId}/${isOptimization}`;
    let optimizationModelComponentsStr = JSON.stringify(modelComponents);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationModelComponentsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    public ApplyQuickSelection(projectId, bmptypeId, isOptimization, isSelected, municipalityId, watershedId, subwatershedId) {        this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));        var getUrl = `${this.BASE_URL}/ApplyQuickSelection/${projectId}/${bmptypeId}/${isOptimization}/${isSelected}/${municipalityId}/${watershedId}/${subwatershedId}`;

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );    }    private handleError(error: HttpErrorResponse) {        if (error.error instanceof ErrorEvent) {            // A client-side or network error occurred. Handle it accordingly.            console.error('An error occurred:', error.error.message);        } else {            // The backend returned an unsuccessful response code.            // The response body may contain clues as to what went wrong,            console.error(`Backend returned code ${error.status}, ` + `body was: ${error.error}`);        }        // return an observable with a user-facing error message        return throwError('Something bad happened; please try again later.');    };}