import { Injectable, OnInit } from '@angular/core';


interface LayoutConfiguration {
    homeUrl: string;
    themeId: number;
    showTreeview: boolean
    showManagerUserOverview: boolean;
    showManagerUserProjectsList: boolean;
    showManagerUserProject: boolean;
    showManagerUserProjectBaseline: boolean;
}


export class LayoutConfigurationService {
    constructor(
        private localStorage: Storage) {

        this.loadLocalChanges();
    }

    ngOnInit() {

        if (localStorage.getItem('token') != null) {
            if (localStorage.getItem('ManagerUser') != null) {
                var loggedInUser = localStorage.getItem('ManagerUser');
                
            }
        }
    }

    private loadLocalChanges() {

    }
}
