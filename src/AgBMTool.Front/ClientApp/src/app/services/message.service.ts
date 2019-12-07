import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private hiddentreeviewsubject = new Subject<any>();
  private clikedlayersubject = new Subject<any>();
  private showtreeviewsubject = new Subject<any>();
  private headname = new Subject<any>();
  private summarilizationlevel = new Subject<any>();
  private selectedgraphic = new Subject<any>();
  private selectedoverviewgraphic = new Subject<any>();
  private createnewproject = new Subject<any>();

  sendHiddentreeviewMessage(hiddentreeview: boolean) {
    this.hiddentreeviewsubject.next({ show: hiddentreeview });
  }

  sendshowtreeviewMessage(showtreeview: boolean) {
    this.showtreeviewsubject.next({ show: showtreeview });
  }

  sendheadnamemessage(headname: String) {
    this.headname.next({ name: headname})
  }

  getheadnamemessage(): Observable<any> {
    return this.headname.asObservable();
  }

  sendSummarilizationlevelMessage(summirilization) {
    this.summarilizationlevel.next(summirilization);
  }

  getSummarilizationlevelMessage(): Observable<any> {
    return this.summarilizationlevel.asObservable();
  }

  sendClickedSummaryLayerMessage(layername: string, Id: string) {
    this.clikedlayersubject.next({ "layername": layername, "Id": Id });
  }

  getClickedSummaryLayerMessage(): Observable<any> {
    return this.clikedlayersubject.asObservable();
  }

  sendSelectedGraphicMessage(isSelected: boolean, Id: string) {
    this.selectedgraphic.next({"isselected": isSelected,  "locationid": Id });
  }

  getSelectedGraphicMessage(): Observable<any> {
    return this.selectedgraphic.asObservable();
  }

  sendSelectedandoverviewGraphicMessage(isSelected: boolean, Id: string) {
    this.selectedoverviewgraphic.next({ "isselected": isSelected, "locationid": Id });
  }

  getSelectedandoverviewGraphicMessage(): Observable<any> {
    return this.selectedoverviewgraphic.asObservable();
  }

  sendcreatenewproject(success: boolean) {
    this.createnewproject.next({ "success": success });
  }

  getcreatenewproject() {
    return this.createnewproject.asObservable();
  }

  clearMessages() {
    this.hiddentreeviewsubject.next();
    this.showtreeviewsubject.next();
  }

  getHiddentreeviewMessage(): Observable<any> {
    return this.hiddentreeviewsubject.asObservable();
  }

  getshowtreeviewMessage(): Observable<any> {
    return this.showtreeviewsubject.asObservable();
  }
}
