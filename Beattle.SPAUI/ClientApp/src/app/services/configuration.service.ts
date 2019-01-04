import { Injectable } from '@angular/core';
import { AppUtilities } from '../appUtilities';
import { environment } from 'src/environments/environment';
import { LocalStoreManagerService } from './local-store-manager.service';

type UserConfiguration = {
  language: string,
  homeUrl: string,
  theme: string,
  showDashboardStatistics: boolean,
  showDashboardNotifications: boolean,
  showDashboardTodo: boolean,
  showDashboardBanner: boolean
};

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  public static readonly appVersion: string = "2.6.1";

  public baseUrl = environment.baseUrl || AppUtilities.baseUrl();
  public loginUrl = environment.loginUrl;
  public fallbackBaseUrl = "#";

  //***Specify default configurations here***
  public static readonly defaultLanguage: string = "en";
  public static readonly defaultHomeUrl: string = "/";
  public static readonly defaultTheme: string = "Default";
  public static readonly defaultShowDashboardStatistics: boolean = true;
  public static readonly defaultShowDashboardNotifications: boolean = true;
  public static readonly defaultShowDashboardTodo: boolean = false;
  public static readonly defaultShowDashboardBanner: boolean = true;
    //***End of defaults***

  private _language: string = null;
  private _homeUrl: string = null;
  private _theme: string = null;
  private _showDashboardStatistics: boolean = null;
  private _showDashboardNotifications: boolean = null;
  private _showDashboardTodo: boolean = null;
  private _showDashboardBanner: boolean = null;

  constructor(private localStorage: LocalStoreManagerService, private translationService: AppTranslationService) {
    this.loadLocalChanges();
  }
}
