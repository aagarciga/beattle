"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var AuthorizationModel = /** @class */ (function () {
    function AuthorizationModel(name, value, groupName, description) {
        this.name = name;
        this.value = value;
        this.groupName = groupName;
        this.description = description;
    }
    AuthorizationModel.authorizationUsersView = "users.view";
    AuthorizationModel.authorizationUsersManage = "users.manage";
    AuthorizationModel.authorizationRolesView = "roles.view";
    AuthorizationModel.authorizationRolesManage = "roles.manage";
    AuthorizationModel.authorizationRolesAssign = "roles.assign";
    return AuthorizationModel;
}());
exports.AuthorizationModel = AuthorizationModel;
//# sourceMappingURL=authorization.model.js.map