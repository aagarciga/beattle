"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var AlertMessage = /** @class */ (function () {
    function AlertMessage(severity, summary, detail) {
        this.severity = severity;
        this.summary = summary;
        this.detail = detail;
    }
    return AlertMessage;
}());
exports.AlertMessage = AlertMessage;
var AlertDialog = /** @class */ (function () {
    function AlertDialog(message, dialogType, okCallback, cancelCallback, defaultValue, okLabel, cancelLabel) {
        this.message = message;
        this.dialogType = dialogType;
        this.okCallback = okCallback;
        this.cancelCallback = cancelCallback;
        this.defaultValue = defaultValue;
        this.okLabel = okLabel;
        this.cancelLabel = cancelLabel;
    }
    return AlertDialog;
}());
exports.AlertDialog = AlertDialog;
//# sourceMappingURL=alerts.js.map