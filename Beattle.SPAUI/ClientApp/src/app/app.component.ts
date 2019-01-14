import { Component, ViewEncapsulation, OnInit, AfterViewInit, ViewChildren } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { LoginComponent } from './components/feature/security/login/login.component';
import { LocalStoreManagerService } from './services/local-store-manager.service';
import { AlertService } from './services/alert.service';
import { AuthorizationService } from './services/authorization.service';
import { ConfigurationService } from './services/configuration.service';
import { Router } from '@angular/router';
import { I18nService } from './services/i18n.service';
import { ApplicationTitleService } from './services/application-title.service';
import { NotificationService } from './services/notification.service';
import { AccountService } from './services/account.service';

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

  @ViewChildren('loginModal,loginControl')
  loginModal: ModalDirective;
  loginControl: LoginComponent;

  constructor(storageManager: LocalStoreManagerService,
    private toastaService: ToastaService,
    private toastaConfig: ToastaConfig,
    private accountService: AccountService,
    private alertService: AlertService,
    private notificationService: NotificationService,
    private appTitleService: ApplicationTitleService,
    private authService: AuthorizationService,
    private translationService: I18nService,
    public configurations: ConfigurationService,
    public router: Router) {

    storageManager.initialiseStorageSyncListener();

    translationService.addLanguages(["en", "fr", "de", "pt", "ar", "ko"]);
    translationService.setDefaultLanguage('en');


    this.toastaConfig.theme = 'bootstrap';
    this.toastaConfig.position = 'top-right';
    this.toastaConfig.limit = 100;
    this.toastaConfig.showClose = true;

    this.appTitleService.appName = this.appTitle;
  }


  ngAfterViewInit(): void {
    throw new Error("Method not implemented.");
  }
  
  ngOnInit(): void {
    // Wait for 1 sec for loading animation
    setTimeout(() => this.isAppLoaded = true, 1000);
  }

  get notificationsTitle() {

    let gT = (key: string) => this.translationService.getTranslation(key);

    if (this.newNotificationCount)
      return `${gT("app.Notifications")} (${this.newNotificationCount} ${gT("app.New")})`;
    else
      return gT("app.Notifications");
  }

}
