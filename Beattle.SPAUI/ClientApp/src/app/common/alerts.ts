import { MessageSeverity } from "../enums/message-severity.enum";
import { DialogType } from "../enums/dialog-type.enum";

export class AlertMessage {
  constructor(
    public severity: MessageSeverity,
    public summary: string,
    public detail: string
  ) { }
}

export class AlertDialog {
  constructor(
    public message: string,
    public dialogType: DialogType,
    public okCallback: (val?: any) => any,
    public cancelCallback: () => any,
    public defaultValue: string,
    public okLabel: string,
    public cancelLabel: string
  ) {

  }
}
