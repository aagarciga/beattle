import { Component, ViewEncapsulation, OnInit, AfterViewInit, ViewChildren, QueryList } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { LoginComponent } from './components/feature/security/login/login.component';
import { LocalStoreManagerService } from './services/local-store-manager.service';
import { AlertService } from './services/alert.service';
import { AuthorizationService } from './services/authorization.service';
import { ConfigurationService } from './services/configuration.service';
import { Router, NavigationStart } from '@angular/router';
import { I18nService } from './services/i18n.service';
import { ApplicationTitleService } from './services/application-title.service';
import { NotificationService } from './services/notification.service';
import { AccountService } from './services/account.service';
import { ToastaService, ToastaConfig, ToastOptions, ToastData } from 'ngx-toasta';
import { MessageSeverity } from './enums/message-severity.enum';
import { AlertDialog, AlertMessage } from './common/alerts';
import { DialogType } from './enums/dialog-type.enum';

var alertify: any = require('../../node_modules/alertifyjs/build/alertify.js');

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  encapsulation: ViewEncapsulation.None // Use only on development
})
export class AppComponent implements OnInit, AfterViewInit {

  isAppLoaded: boolean;
  isUserLoggedIn: boolean;
  shouldShowLoginModal: boolean;
  removePrebootScreen: boolean;
  newNotificationCount = 0;
  title = 'Beattle';

  stickyToasties: number[] = [];

  dataLoadingConsecutiveFailurs = 0;
  notificationsLoadingSubscription: any;

  @ViewChildren('loginModal,loginControl')
  modalLoginControls: QueryList<any>;

  loginModal: ModalDirective;
  loginControl: LoginComponent;

  get userName(): string {
    return this.authorizationService.currentUser ? this.authorizationService.currentUser.userName : "";
  }

  get fullName(): string {
    return this.authorizationService.currentUser ? this.authorizationService.currentUser.fullName : "";
  }

  get notificationsTitle() {

    let gT = (key: string) => this.translationService.getTranslation(key);

    if (this.newNotificationCount)
      return `${gT("app.Notifications")} (${this.newNotificationCount} ${gT("app.New")})`;
    else
      return gT("app.Notifications");
  }

  constructor(
    storageManager: LocalStoreManagerService,
    private toastaService: ToastaService,
    private toastaConfig: ToastaConfig,
    private accountService: AccountService,
    private alertService: AlertService,
    private notificationService: NotificationService,
    private appTitleService: ApplicationTitleService,
    private authorizationService: AuthorizationService,
    private translationService: I18nService,
    public settings: ConfigurationService,
    public router: Router) {

    storageManager.initialiseStorageSyncListener();

    translationService.addLanguages(["en", "nl", "de", "es"]);
    translationService.setDefaultLanguage('en');


    this.toastaConfig.theme = 'bootstrap';
    this.toastaConfig.position = 'top-right';
    this.toastaConfig.limit = 100;
    this.toastaConfig.showClose = true;

    this.appTitleService.applicationName = this.title;
  }


  ngAfterViewInit(): void {
    this.modalLoginControls.changes.subscribe((controls: QueryList<any>) => {
      controls.forEach(control => {
        if (control) {
          if (control instanceof LoginComponent) {
            this.loginControl = control;
            this.loginControl.modalClosedCallback = () => this.loginModal.hide();
          }
          else {
            this.loginModal = control;
            this.loginModal.show();
          }
        }
      });
    });
  }  
  ngOnInit(): void {
    this.isUserLoggedIn = this.authorizationService.isLoggedIn || true;

    // 1 sec to ensure all the effort to get the css animation working is appreciated :|, Preboot screen is removed .5 sec later
    setTimeout(() => this.isAppLoaded = true, 1000);
    setTimeout(() => this.removePrebootScreen = true, 1500);

    setTimeout(() => {
      if (this.isUserLoggedIn) {
        this.alertService.resetStickyMessage();

        //if (!this.authService.isSessionExpired)
        this.alertService.showMessage("Login", `Welcome back ${this.userName}!`, MessageSeverity.default);
        //else
        //    this.alertService.showStickyMessage("Session Expired", "Your Session has expired. Please log in again", MessageSeverity.warn);
      }
    }, 2000);


    this.alertService.getDialogEvent().subscribe(alert => this.showDialog(alert));
    this.alertService.getMessageEvent().subscribe(message => this.showToast(message, false));
    this.alertService.getStickyMessageEvent().subscribe(message => this.showToast(message, true));

    this.authorizationService.reLoginDelegate = () => this.shouldShowLoginModal = true;

    this.authorizationService.loginStatusEvent.subscribe(isLoggedIn => {
      this.isUserLoggedIn = isLoggedIn;


      if (this.isUserLoggedIn) {
        this.initNotificationsLoading();
      }
      else {
        this.unsubscribeNotifications();
      }

      setTimeout(() => {
        if (!this.isUserLoggedIn) {
          this.alertService.showMessage("Session Ended!", "", MessageSeverity.default);
        }
      }, 500);
    });

    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        let url = (<NavigationStart>event).url;

        if (url !== url.toLowerCase()) {
          this.router.navigateByUrl((<NavigationStart>event).url.toLowerCase());
        }
      }
    });
  }
  ngOnDestroy() {
    this.unsubscribeNotifications();
  }


  onLoginModalShown() {
    this.alertService.showStickyMessage("Session Expired", "Your Session has expired. Please log in again", MessageSeverity.info);
  }
  onLoginModalHidden() {
    this.alertService.resetStickyMessage();
    this.loginControl.reset();
    this.shouldShowLoginModal = false;

    if (this.authorizationService.isSessionExpired)
      this.alertService.showStickyMessage("Session Expired", "Your Session has expired. Please log in again to renew your session", MessageSeverity.warn);
  }
  onLoginModalHide() {
    this.alertService.resetStickyMessage();
  }


  showDialog(dialog: AlertDialog) {

    alertify.set({
      labels: {
        ok: dialog.okLabel || "OK",
        cancel: dialog.cancelLabel || "Cancel"
      }
    });

    switch (dialog.dialogType) {
      case DialogType.alert:
        alertify.alert(dialog.message);

        break
      case DialogType.confirm:
        alertify
          .confirm(dialog.message, (e) => {
            if (e) {
              dialog.okCallback();
            }
            else {
              if (dialog.cancelCallback)
                dialog.cancelCallback();
            }
          });

        break;
      case DialogType.prompt:
        alertify
          .prompt(dialog.message, (e, val) => {
            if (e) {
              dialog.okCallback(val);
            }
            else {
              if (dialog.cancelCallback)
                dialog.cancelCallback();
            }
          }, dialog.defaultValue);

        break;
    }
  }
  showToast(message: AlertMessage, isSticky: boolean) {

    if (message == null) {
      for (let id of this.stickyToasties.slice(0)) {
        this.toastaService.clear(id);
      }
      return;
    }

    let toastOptions: ToastOptions = {
      title: message.summary,
      msg: message.detail,
      timeout: isSticky ? 0 : 4000
    };


    if (isSticky) {
      toastOptions.onAdd = (toast: ToastData) => this.stickyToasties.push(toast.id);

      toastOptions.onRemove = (toast: ToastData) => {
        let index = this.stickyToasties.indexOf(toast.id, 0);

        if (index > -1) {
          this.stickyToasties.splice(index, 1);
        }

        toast.onAdd = null;
        toast.onRemove = null;
      };
    }


    switch (message.severity) {
      case MessageSeverity.default: this.toastaService.default(toastOptions); break;
      case MessageSeverity.info: this.toastaService.info(toastOptions); break;
      case MessageSeverity.success: this.toastaService.success(toastOptions); break;
      case MessageSeverity.error: this.toastaService.error(toastOptions); break;
      case MessageSeverity.warn: this.toastaService.warning(toastOptions); break;
      case MessageSeverity.wait: this.toastaService.wait(toastOptions); break;
    }
  }
  markNotificationsAsRead() {

    let recentNotifications = this.notificationService.recentNotifications;

    if (recentNotifications.length) {
      this.notificationService.readUnreadNotification(recentNotifications.map(n => n.id), true)
        .subscribe(response => {
          for (let n of recentNotifications) {
            n.isRead = true;
          }

          this.newNotificationCount = recentNotifications.filter(n => !n.isRead).length;
        },
          error => {
            this.alertService.logError(error);
            this.alertService.showMessage("Notification Error", "Marking read notifications failed", MessageSeverity.error);

          });
    }
  }
  logout() {
    this.authorizationService.logout();
    this.authorizationService.redirectLogoutUser();
  }
  getYear() {
    return new Date().getUTCFullYear();
  }


  initNotificationsLoading() {

    this.notificationsLoadingSubscription = this.notificationService.getNewNotificationsPeriodically()
      .subscribe(notifications => {
        this.dataLoadingConsecutiveFailurs = 0;
        this.newNotificationCount = notifications.filter(n => !n.isRead).length;
      },
        error => {
          this.alertService.logError(error);

          if (this.dataLoadingConsecutiveFailurs++ < 20)
            setTimeout(() => this.initNotificationsLoading(), 5000);
          else
            this.alertService.showStickyMessage("Load Error", "Loading new notifications from the server failed!", MessageSeverity.error);
        });
  }
  private unsubscribeNotifications() {
    if (this.notificationsLoadingSubscription)
      this.notificationsLoadingSubscription.unsubscribe();
  }
}
