import { Component, OnInit, Input, OnDestroy } from '@angular/core';

import { UserLoginModel } from "../../../../models/user-login.model";
import { AlertService } from 'src/app/services/alert.service';
import { AuthorizationService } from 'src/app/services/authorization.service';
import { ConfigurationService } from 'src/app/services/configuration.service';
import { MessageSeverity } from 'src/app/enums/message-severity.enum';
import { AppUtilities } from 'src/app/appUtilities';
import { DialogType } from 'src/app/enums/dialog-type.enum';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {

  userLoginModel = new UserLoginModel();
  isLoading = false;
  formResetToggle = true;
  modalClosedCallback: () => void;
  loginStatusSubscription: any;

  @Input()
  isModal = false;

  constructor(
    private alertService: AlertService,
    private authorizationService: AuthorizationService,
    private configurations: ConfigurationService
  ) { }

  ngOnInit() {
    this.userLoginModel.rememberMe = this.authorizationService.rememberMe;

    if (this.getShouldRedirect()) {
      this.authorizationService.redirectLoginUser();
    } else {
      this.loginStatusSubscription = this.authorizationService.loginStatusEvent.subscribe(isLoggedIn => {
        if (this.getShouldRedirect()) {
          this.authorizationService.redirectLoginUser();
        }
      });
    }
  }

  ngOnDestroy() {
    if (this.loginStatusSubscription)
      this.loginStatusSubscription.unsubscribe();
  }

  getShouldRedirect() {
    return !this.isModal && this.authorizationService.isLoggedIn && !this.authorizationService.isSessionExpired;
  }



  showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  closeModal() {
    if (this.modalClosedCallback) {
      this.modalClosedCallback();
    }
  }

  login() {
    this.isLoading = true;
    this.alertService.startLoadingMessage("", "Attempting login...");

    this.authorizationService.login(
      this.userLoginModel.email,
      this.userLoginModel.password,
      this.userLoginModel.rememberMe
    )
      .subscribe(
        user => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;
            this.reset();

            if (!this.isModal) {
              this.alertService.showMessage("Login", `Welcome ${user.userName}!`, MessageSeverity.success);
            }
            else {
              this.alertService.showMessage("Login", `Session for ${user.userName} restored!`, MessageSeverity.success);
              setTimeout(() => {
                this.alertService.showStickyMessage("Session Restored", "Please try your last operation again", MessageSeverity.default);
              }, 500);

              this.closeModal();
            }
          }, 500);
        },
        error => {

          this.alertService.stopLoadingMessage();

          if (AppUtilities.checkNoNetwork(error)) {
            this.alertService.showStickyMessage(AppUtilities.noNetworkMessageCaption, AppUtilities.noNetworkMessageDetail, MessageSeverity.error, error);
            this.offerAlternateHost();
          }
          else {
            let errorMessage = AppUtilities.findHttpResponseMessage("error_description", error);

            if (errorMessage)
              this.alertService.showStickyMessage("Unable to login", errorMessage, MessageSeverity.error, error);
            else
              this.alertService.showStickyMessage("Unable to login", "An error occured whilst logging in, please try again later.\nError: " + AppUtilities.getResponseBody(error), MessageSeverity.error, error);
          }

          setTimeout(() => {
            this.isLoading = false;
          }, 500);
        });
  }

  offerAlternateHost() {

    if (AppUtilities.checkIsLocalHost(location.origin) && AppUtilities.checkIsLocalHost(this.configurations.baseUrl)) {
      this.alertService.showDialog("Dear Developer!\nIt appears your backend Web API service is not running...\n" +
        "Would you want to temporarily switch to the online Demo API below?(Or specify another)",
        DialogType.prompt,
        (value: string) => {
          this.configurations.baseUrl = value;
          this.alertService.showStickyMessage("API Changed!", "The target Web API has been changed to: " + value, MessageSeverity.warn);
        },
        null,
        null,
        null,
        this.configurations.fallbackBaseUrl);
    }
  }

  reset() {
    this.formResetToggle = false;

    setTimeout(() => {
      this.formResetToggle = true;
    });
  }

}
