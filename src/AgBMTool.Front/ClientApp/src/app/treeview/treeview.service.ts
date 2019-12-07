import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class TreeviewService {
    private BASE_URL = 'api/ProjectData';
    private userdata: any;
  constructor(private http: HttpClient) {

  }
  public getTreeView(): Observable<any[]> {
    var treeviewurl = this.BASE_URL + "/1";
      //return this.http.get<any[]>(treeviewurl);

      this.userdata = JSON.parse(localStorage.getItem("ManagerUser"));
      
      let headers = new HttpHeaders({
          Authorization: 'Bearer ' + this.userdata.token,
          'Content-Type': 'application/json',
          Accept: 'application/json, text/plain, */*'
      });
      //headers = headers.append('Authorization', 'Bearer ' + `${this.userdata.token}`);
      return this.http.get<any[]>(treeviewurl, { headers: headers });
  }
}
