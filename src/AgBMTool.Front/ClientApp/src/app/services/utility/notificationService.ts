import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class NotificationService {

    constructor(private toastr: ToastrService) { }


    showSuccess(message, title) {
        this.toastr.success(message, title);
    }

    showError(message, title) {
        this.toastr.error(message, title);
    }

    showWarning(message, title) {
        this.toastr.warning(message, title);
    }

    showInfo(message, title) {
        this.toastr.info(message, title);
    }


    showMessageOnTopLeft(message, title, position: any = 'top-left') {
        this.toastr.info(message, title, {
            positionClass: position
        });
    }
}
