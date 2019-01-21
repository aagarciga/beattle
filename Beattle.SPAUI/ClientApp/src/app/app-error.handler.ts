import { Injectable, ErrorHandler } from "@angular/core";

@Injectable()
export class ApplicationErrorHandler extends ErrorHandler {

  constructor() { super(); }

  handleError(error: any) {
    if (confirm("Fatal Error!\nAn unresolved error has occured. Do you want to reload the page to correct this?\n\nError: " + error.message)) {
      window.location.reload(true);
    }
    super.handleError(error);
  }
}
