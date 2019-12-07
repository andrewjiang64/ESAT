import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, } from 'rxjs';
import { FilterModel } from '../models/LayerFiler';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class MapService {
  private BASE_URL = 'api/UserGeometryData';
  private userdata = JSON.parse(localStorage.getItem("ManagerUser"));
  private userId = this.userdata.userId;
  private showtreeview = new BehaviorSubject<any>(true);
  private headers = new HttpHeaders({
     Authorization: 'Bearer ' + this.userdata.token,
     'Content-Type': 'application/json',
     Accept: 'application/json, text/plain, */*'
   });
  constructor(private http: HttpClient) {

  }

  public sendchangeTreeviewmessage(show: boolean) {
    this.showtreeview.next({ show: show });
  }
  public getchangeTreeviewmessage() {
    return this.showtreeview.asObservable();
  }
  public getReachGeometryData(filter: FilterModel): Observable<any[]> {
    var reachGeometrywurl = this.BASE_URL + "/getReachData/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(reachGeometrywurl, { headers: this.headers });
  }
  public getLSDGeometryData(filter: FilterModel): Observable<any[]> {
    var lsdGeometrywurl = this.BASE_URL + "/getLSDData/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(lsdGeometrywurl, { headers: this.headers });
  }

  public getParcelGeometryData(filter: FilterModel): Observable<any[]> {
    var parcelGeometrywurl = this.BASE_URL + "/getParcelData/" + this.userId  + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(parcelGeometrywurl, { headers: this.headers });
  }

  public getFarmGeometryData(filter: FilterModel): Observable<any[]> {
    var farmGeometrywurl = this.BASE_URL + "/getFarmData/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(farmGeometrywurl, { headers: this.headers });
  }

  public getMunicipalitiesGeometryData(filter: FilterModel): Observable<any[]> {
    var farmGeometrywurl = this.BASE_URL + "/getMunicipalitiesData/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(farmGeometrywurl, { headers: this.headers });
  }

  public getWaterShedGeometryData(filter: FilterModel): Observable<any[]> {
    var farmGeometrywurl = this.BASE_URL + "/getUserWaterShedData/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(farmGeometrywurl, { headers: this.headers });
  }

  public getUserSubWaterShedGeometryData(filter: FilterModel): Observable<any[]> {
    var farmGeometrywurl = this.BASE_URL + "/getUserSubWaterShed/" + this.userId + "/" + filter.MunicipalityId + "/" + filter.WatershedId + "/" + filter.SubwatershedId;
    return this.http.get<any[]>(farmGeometrywurl, { headers: this.headers });
  }

  public getProjectWaterShedsGeometry(projectId: number): Observable<any[]> {
    var projectWaterShedsGeometryurl = this.BASE_URL + "/getProjectWaterShedsGeometry/" + projectId;
    return this.http.get<any[]>(projectWaterShedsGeometryurl, { headers: this.headers });
  }

  public getProjectMunicipilitiesGeometry(projectId: number): Observable<any[]> {
    var projectMunicipilitiesGeometryurl = this.BASE_URL + "/getProjectMunicipilitiesGeometry/" + projectId;
    return this.http.get<any[]>(projectMunicipilitiesGeometryurl, { headers: this.headers });
  }

  public getProjectReachesGeometry(projectId: number): Observable<any[]> {
    var projectReachesGeometryurl = this.BASE_URL + "/getProjectReachesGeometry/" + projectId;
    return this.http.get<any[]>(projectReachesGeometryurl, { headers: this.headers });
  }

  public getUserBMPTypeGeometryData(projectId: number, bmptypeId: number, isOptimization: boolean): Observable<any> {
    var bmptypeGeometryDataGeometrywurl = this.BASE_URL + "/getUserBMPGeomtries/" + this.userId
      + "/" + projectId + "/" + bmptypeId + "/" + isOptimization;
    return this.http.get<any[]>(bmptypeGeometryDataGeometrywurl, { headers: this.headers });
  }

  public getUserBMPTyeListGeomtries(projectId: number, bmptypeIds: Array<number>, isOptimization: boolean): Observable<any> {
    var bmptypeGeometryDataGeometrywurl = this.BASE_URL + "/getUserBMPTyeListGeomtries/"
      + projectId + "/" + this.userId + "/" + JSON.stringify(bmptypeIds) + "/" + isOptimization;
    return this.http.get<any[]>(bmptypeGeometryDataGeometrywurl, { headers: this.headers });
  }

  public getBMPTypeGeometrystyledic(bmptypeIds: Array<number>): Observable<any> {
    var bmptypeGeometrystyledicurl = this.BASE_URL + "/getBMPTypeGeometryLayerDic/" + JSON.stringify(bmptypeIds);
    return this.http.get<any[]>(bmptypeGeometrystyledicurl, { headers: this.headers });
  }

  public getUserMapCenter(): Observable<any[]> {
    var userCenterPointwurl = this.BASE_URL + "/getUserCenterPoint/" + this.userId;
    return this.http.get<any[]>(userCenterPointwurl, { headers: this.headers });
  }

    public getUserMapExtent(): Observable<any> {
        var useExtenturl = this.BASE_URL + "/getUserExtent/" + this.userId;
        return this.http.get<any[]>(useExtenturl, { headers: this.headers });
    }
}
