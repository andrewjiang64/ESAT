import { Component } from '@angular/core';
import { TreeviewService } from './treeview.service';
import { Router } from '@angular/router';
import { MessageService } from '../services/index';
@Component({
    selector: 'app-treeview',
    templateUrl: './treeview.component.html',
    styleUrls: ['./treeview.component.css']
})
/** treeview component*/
export class TreeviewComponent {

    data: any[];
    /* treeview ctor */
    constructor(private treeviewService: TreeviewService, private router: Router, private messageService: MessageService) {

    }

    public ngOnInit(): void {
      this.loadtreeview();
      this.messageService.getcreatenewproject().subscribe(rdata => {
        if (rdata.success) {
          this.loadtreeview();
        }
      })
  }

  public loadtreeview() {
    this.treeviewService.getTreeView().subscribe(rdata => {
      this.data = rdata;
      console.log(this.data);
    });
  }

    public iconClass(dataItem) {

        const is = (dataItemIconclass: string, iconclass: string) => dataItemIconclass == iconclass;
        return {
            'projects-icon': is(dataItem.iconclass, 'projects-icon'),
            'project-icon': is(dataItem.iconclass, 'project-icon'),
            'overview-icon': is(dataItem.iconclass, 'overview-icon'),
            'bso-icon': is(dataItem.iconclass, 'bso-icon'),
            'bsair-icon': is(dataItem.iconclass, 'bsair-icon'),
            'baseline-icon': is(dataItem.iconclass, 'baseline-icon'),
            'ASNL-icon': is(dataItem.iconclass, 'ASNL-icon'),
            'ASPL-icon': is(dataItem.iconclass, 'ASPL-icon'),
            'CLDR-icon': is(dataItem.iconclass, 'CLDR-icon'),
            'CRRO-icon': is(dataItem.iconclass, 'CRRO-icon'),
            'CSTL-icon': is(dataItem.iconclass, 'CSTL-icon'),
            'CVCR-icon': is(dataItem.iconclass, 'CVCR-icon'),
            'DGOT-icon': is(dataItem.iconclass, 'DGOT-icon'),
            'FDLT-icon': is(dataItem.iconclass, 'FDLT-icon'),
            'FERMG-icon': is(dataItem.iconclass, 'FERMG-icon'),
            'FLDV-icon': is(dataItem.iconclass, 'FLDV-icon'),
            'FRCV-icon': is(dataItem.iconclass, 'FRCV-icon'),
            'GWW-icon': is(dataItem.iconclass, 'GWW-icon'),
            'IRRMG-icon': is(dataItem.iconclass, 'IRRMG-icon'),
            'ISWET-icon': is(dataItem.iconclass, 'ISWET-icon'),
            'LAKE-icon': is(dataItem.iconclass, 'LAKE-icon'),
            'MASB-icon': is(dataItem.iconclass, 'MASB-icon'),
            'MCBI-icon': is(dataItem.iconclass, 'MCBI-icon'),
            'MI48H-icon': is(dataItem.iconclass, 'MI48H-icon'),
            'MSCD-icon': is(dataItem.iconclass, 'MSCD-icon'),
            'MTHS-icon': is(dataItem.iconclass, 'MTHS-icon'),
            'NAOS-icon': is(dataItem.iconclass, 'NAOS-icon'),
            'OFSW-icon': is(dataItem.iconclass, 'OFSW-icon'),
            'PSTPS-icon': is(dataItem.iconclass, 'PSTPS-icon'),
            'PTSR-icon': is(dataItem.iconclass, 'PTSR-icon'),
            'RDMG-icon': is(dataItem.iconclass, 'RDMG-icon'),
            'RESV-icon': is(dataItem.iconclass, 'RESV-icon'),
            'RIBUF-icon': is(dataItem.iconclass, 'RIBUF-icon'),
            'RIWET-icon': is(dataItem.iconclass, 'RIWET-icon'),
            'RKCH-icon': is(dataItem.iconclass, 'RKCH-icon'),
            'ROGZ-icon': is(dataItem.iconclass, 'ROGZ-icon'),
            'SAFA-icon': is(dataItem.iconclass, 'SAFA-icon'),
            'SAMG-icon': is(dataItem.iconclass, 'SAMG-icon'),
            'SMDM-icon': is(dataItem.iconclass, 'SMDM-icon'),
            'SUNA-icon': is(dataItem.iconclass, 'SUNA-icon'),
            'TERR-icon': is(dataItem.iconclass, 'TERR-icon'),
            'TLDMG-icon': is(dataItem.iconclass, 'TLDMG-icon'),
            'VFST-icon': is(dataItem.iconclass, 'VFST-icon'),
            'WASCOB-icon': is(dataItem.iconclass, 'WASCOB-icon'),
            'WDBR-icon': is(dataItem.iconclass, 'WDBR-icon'),
            'WSMG-icon': is(dataItem.iconclass, 'WSMG-icon'),
            'AOPANoMASB-icon': is(dataItem.iconclass, 'AOPANoMASB-icon'),
            'k-icon': true
        };
    }

    public handleSelection(item: any): void {
        var typeId = item.dataItem.typeId;
        if (typeId == 1) {
            this.router.navigate(['home/overview']);
            this.messageService.sendheadnamemessage("Overview");
        }
        if (typeId == 2) {
            this.router.navigate(['home/project']);
            this.messageService.sendheadnamemessage("Add/Open project");
        }
        if (typeId == 4) {
            this.router.navigate(['home/baselineInfo', { id: item.dataItem.projectId }]);
            this.messageService.sendheadnamemessage(item.dataItem.projectName + " - Baseline information");
        }
        if (typeId == 5) {
            this.messageService.sendheadnamemessage(item.dataItem.projectName + " - BMP scope & intelligent recommendation - " + item.dataItem.name);
            this.router.navigate(['home/bmpScopeAndIntelligence', {
                id: item.dataItem.projectId,
                bmptype: item.dataItem.name,
                bmptypeId: item.dataItem.id,
                bmptypeIds: item.dataItem.bmptypeIds
            }]);
        }
        if (typeId == 6) {
            this.messageService.sendheadnamemessage(item.dataItem.projectName + " - BMP selection & overview - " + item.dataItem.name);
            this.router.navigate(['home/bmpSelectionAndOverviewComponent', {
                id: item.dataItem.projectId,
                bmptype: item.dataItem.name,
                bmptypeId: item.dataItem.id,
                bmptypeIds: item.dataItem.bmptypeIds
            }]);
        }
        console.log(item.dataItem.name);
    }
}
