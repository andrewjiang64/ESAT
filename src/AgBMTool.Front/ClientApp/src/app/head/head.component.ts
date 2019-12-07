import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from '../services/index';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-head',
    templateUrl: './head.component.html',
    styleUrls: ['./head.component.css']
})
/** head component*/
export class HeadComponent {
    @Input() name: string;
    @Input() userName: string;
    @Input() userCompany: string;
    subscription: Subscription;
    private userdata = JSON.parse(localStorage.getItem("ManagerUser"));
  /** head ctor */
  constructor(private router: Router, private messageService: MessageService) {
        this.name = "Overview";
        this.userName = this.userdata.username;
    this.userCompany = this.userdata.organizationName;
    this.subscription = this.messageService.getheadnamemessage().subscribe(message => {
      if (message) {
        this.name = message.name;
        console.log("head = " + this.name);
      }
    })

    }

    onLogout() {
        localStorage.removeItem('token');
        this.router.navigate(['/login']);
    }
}
