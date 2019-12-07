"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var LayoutConfigurationService = /** @class */ (function () {
    function LayoutConfigurationService(localStorage) {
        this.localStorage = localStorage;
        this.loadLocalChanges();
    }
    LayoutConfigurationService.prototype.ngOnInit = function () {
        if (localStorage.getItem('token') != null) {
            if (localStorage.getItem('ManagerUser') != null) {
                var loggedInUser = localStorage.getItem('ManagerUser');
            }
        }
    };
    LayoutConfigurationService.prototype.loadLocalChanges = function () {
    };
    return LayoutConfigurationService;
}());
exports.LayoutConfigurationService = LayoutConfigurationService;
//# sourceMappingURL=layoutConfiguration.js.map