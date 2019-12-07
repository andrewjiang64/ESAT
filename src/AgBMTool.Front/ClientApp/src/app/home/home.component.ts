import { Component } from '@angular/core';
import { MessageService } from '../services/index';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  subscription: Subscription;
  constructor(private messageservice: MessageService) {
    this.subscription = this.messageservice.getshowtreeviewMessage().subscribe(message => {
      if (message) {
        this.hiddentreeview = message.show;
        console.log("home = " + this.hiddentreeview);
      }
    });
  }
  private hiddentreeview: boolean = false;
  public hiddentreeviewfunction(): void {
    this.hiddentreeview = !this.hiddentreeview;
    this.messageservice.sendHiddentreeviewMessage(!this.hiddentreeview);
  }
}
