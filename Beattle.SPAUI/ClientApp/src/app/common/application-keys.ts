import { Injectable } from "@angular/core";

@Injectable()
export class AppKeys {
  public static readonly LANGUAGE = "language";
  public static readonly URL_HOME = "url_home";
  public static readonly THEME = "theme";

  public static readonly USER_SETTINGS_LANGUAGE = "user_settings_language";
  public static readonly USER_SETTINGS_URL_HOME = "user_settings_url_home";
  public static readonly USER_SETTINGS_THEME = "user_settings_theme";

  public static readonly USER_SETTINGS_SHOW_DASHBOARD_STATISTICS = "user_settings_show_dashboad_statistics";
  public static readonly USER_SETTINGS_SHOW_DASHBOARD_NOTIFICATIONS = "user_settings_show_dashboad_notifications";
  public static readonly USER_SETTINGS_SHOW_DASHBOARD_TODO = "user_settings_show_dashboad_todo";
  public static readonly USER_SETTINGS_SHOW_DASHBOARD_BANNER = "user_settings_show_dashboad_banner";

  public static readonly TOKEN_ID = "token_id";
  public static readonly TOKEN_ACCESS = "token_access";
  public static readonly TOKEN_REFRESH = "token_refresh";
  public static readonly TOKEN_EXPIRES_IN = "expires_in";

  public static readonly CURRENT_USER = "current_user";
  public static readonly USER_AUTHORIZATIONS = "user_authorizations";
  public static readonly REMEMBER_ME = "remember_me";
}
