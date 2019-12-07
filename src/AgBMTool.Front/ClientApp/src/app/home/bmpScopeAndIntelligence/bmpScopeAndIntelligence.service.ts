import { Injectable } from '@angular/core';
    }

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
        let optimizationWeightsStr = JSON.stringify(optimizationWeights);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationWeightsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
        let optimizationConstraintsStr = JSON.stringify(optimizationConstraints);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationConstraintsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
      let optimizationLSDStr = JSON.stringify(LSD);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationLSDStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
    let optimizationParcelsStr = JSON.stringify(parcels);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationParcelsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );
    let optimizationModelComponentsStr = JSON.stringify(modelComponents);

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.post<any[]>(getUrl, JSON.stringify(optimizationModelComponentsStr), { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );

        let headers = new HttpHeaders({
            Authorization: 'Bearer ' + this.userdata.token,
            'Content-Type': 'application/json;charset=utf-8',
            Accept: 'application/json, text/plain, */*'
        });

        return this.http.get<any[]>(getUrl, { headers: headers }).pipe(tap(data => data),
            catchError(this.handleError)
        );