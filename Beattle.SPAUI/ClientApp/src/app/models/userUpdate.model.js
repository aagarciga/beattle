"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var user_model_1 = require("./user.model");
var UserUpdateModel = /** @class */ (function (_super) {
    __extends(UserUpdateModel, _super);
    function UserUpdateModel(currentPassword, newPassword, confirmPassword) {
        var _this = _super.call(this) || this;
        _this.currentPassword = currentPassword;
        _this.newPassword = newPassword;
        _this.confirmPassword = confirmPassword;
        return _this;
    }
    return UserUpdateModel;
}(user_model_1.UserModel));
exports.UserUpdateModel = UserUpdateModel;
//# sourceMappingURL=userUpdate.model.js.map