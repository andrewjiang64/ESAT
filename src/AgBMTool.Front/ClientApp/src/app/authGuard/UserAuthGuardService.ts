import { Injectable } from '@angular/core';
import { Router, CanActivate } from '@angular/router';

@Injectable()
export class UserAuthGuardService implements CanActivate {

    constructor(private router: Router) { }

    canActivate()
    {
        if (localStorage.getItem('ManagerUser'))
        {
            return true;
        }

        // not logged in so redirect to login page
        this.router.navigate(['/login']);
        return false;
    }
}
