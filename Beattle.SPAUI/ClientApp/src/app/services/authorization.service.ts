import { Injectable } from '@angular/core';
import { ConfigurationService } from './configuration.service';
import { Router, NavigationExtras } from '@angular/router';
import { EndpointFactoryService } from './endpoint-factory.service';
import { LocalStoreManagerService } from './local-store-manager.service';
import { Observable, Subject } from 'rxjs';
import { AppKeys } from '../common/application-keys';
import { AppUtilities } from '../appUtilities';
import { UserModel } from '../models/user.model';
import { PermissionValues } from '../common/application-types';
import { LoginResponse } from '../common/interfaces/login-response';
import { map } from 'rxjs/operators';
import { JwtHelper } from '../common/helpers/jwt.helper';
import { TokenId } from '../common/interfaces/token-id';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private previousLoginStatus = false;
  private _loginStatus = new Subject<boolean>();

  public loginRedirectUrl: string;
  public logoutRedirectUrl: string;
  public reLoginDelegate: () => void;

  constructor(
    private router: Router,
    private settings: ConfigurationService,
    private endPointFactory: EndpointFactoryService,
    private localStoreManager: LocalStoreManagerService
  ) {
    // Initializing Login Status
    this.localStoreManager.getInitEvent().subscribe(() => {
      this.checkLoginStatus();
    });
  }

  get loginUrl() {
    return this.settings.loginUrl;
  }
  get homeUrl() {
    return this.settings.homeUrl;
  }
  get currentUser(): UserModel {

    let user = this.localStoreManager.getDataObject<UserModel>(AppKeys.CURRENT_USER);
    this.checkLoginStatus(user);
    return user;
  }
  get userPermissions(): PermissionValues[] {
    return this.localStoreManager.getDataObject<PermissionValues[]>(AppKeys.USER_PERMISSIONS) || [];
  }
  get accessToken(): string {

    this.checkLoginStatus();
    return this.localStoreManager.getData(AppKeys.TOKEN_ACCESS);
  }
  get accessTokenExpiryDate(): Date {

    this.checkLoginStatus();
    return this.localStoreManager.getDataObject<Date>(AppKeys.TOKEN_EXPIRES_IN, true);
  }
  get isSessionExpired(): boolean {

    if (this.accessTokenExpiryDate == null) {
      return true;
    }
    return !(this.accessTokenExpiryDate.valueOf() > new Date().valueOf());
  }
  get tokenId(): string {

    this.checkLoginStatus();
    return this.localStoreManager.getData(AppKeys.TOKEN_ID);
  }
  get refreshToken(): string {

    this.checkLoginStatus();
    return this.localStoreManager.getData(AppKeys.TOKEN_REFRESH);
  }
  get isLoggedIn(): boolean {
    return this.currentUser != null;
  }
  get rememberMe(): boolean {
    return this.localStoreManager.getDataObject<boolean>(AppKeys.REMEMBER_ME) == true;
  }
  get loginStatusEvent(): Observable<boolean> {
    return this._loginStatus.asObservable();
  }

  /**
   * 
   * @param page
   * @param preserveParams
   */
  gotoPage(page: string, preserveParams = true) {

    let navigationExtras: NavigationExtras = {
      queryParamsHandling: preserveParams ? "merge" : "", preserveFragment: preserveParams
    };
    this.router.navigate([page], navigationExtras);
  }  
  refreshLogin(){
    return this.endPointFactory.getRefreshLoginEndPoint<LoginResponse>().pipe(
      map(response => this.processLoginResponse(response, this.rememberMe)));
  }
  reLogin() {

    this.localStoreManager.deleteData(AppKeys.TOKEN_EXPIRES_IN);

    if (this.reLoginDelegate) {
      this.reLoginDelegate();
    }
    else {
      this.redirectForLogin();
    }
  }
  redirectLoginUser() {
    let redirect = this.loginRedirectUrl && this.loginRedirectUrl != '/' && this.loginRedirectUrl != ConfigurationService.defaultHomeUrl ? this.loginRedirectUrl : this.homeUrl;
    this.loginRedirectUrl = null;

    let urlParamsAndFragment = AppUtilities.splitInTwo(redirect, '#');
    let urlAndParams = AppUtilities.splitInTwo(urlParamsAndFragment.firstPart, '?');

    let navigationExtras: NavigationExtras = {
      fragment: urlParamsAndFragment.secondPart,
      queryParams: AppUtilities.getQueryParamsFromString(urlAndParams.secondPart),
      queryParamsHandling: "merge"
    };

    this.router.navigate([urlAndParams.firstPart], navigationExtras);
  }
  redirectLogoutUser() {
    let redirect = this.logoutRedirectUrl ? this.logoutRedirectUrl : this.loginUrl;
    this.logoutRedirectUrl = null;

    this.router.navigate([redirect]);
  }
  redirectForLogin() {
    this.loginRedirectUrl = this.router.url;
    this.router.navigate([this.loginUrl]);
  }
  /**
   * 
   * @param userName
   * @param password
   * @param rememberMe
   */
  login(userName: string, password: string, rememberMe?: boolean) {

    if (this.isLoggedIn)
      this.logout();

    return this.endPointFactory.getLoginEndPoint<LoginResponse>(userName, password).pipe(
      map(response => this.processLoginResponse(response, rememberMe)));
  }
  logout(): void {
    this.localStoreManager.deleteData(AppKeys.TOKEN_ACCESS);
    this.localStoreManager.deleteData(AppKeys.TOKEN_ID);
    this.localStoreManager.deleteData(AppKeys.TOKEN_REFRESH);
    this.localStoreManager.deleteData(AppKeys.TOKEN_EXPIRES_IN);
    this.localStoreManager.deleteData(AppKeys.USER_PERMISSIONS);
    this.localStoreManager.deleteData(AppKeys.CURRENT_USER);

    this.settings.clearLocalChanges();

    this.checkLoginStatus();
  }

  /**
   * Check if the current user is already logged in
   * @param currentUser
   */
  private checkLoginStatus(currentUser?: UserModel) {

    let user = currentUser
      || this.localStoreManager.getDataObject<UserModel>(AppKeys.CURRENT_USER);
    let isLoggedIn = user != null;

    if (this.previousLoginStatus != isLoggedIn) {
      setTimeout(() => {
        this._loginStatus.next(isLoggedIn);
      });
    }
    this.previousLoginStatus = isLoggedIn;
  }

  /**
   * 
   * @param response
   * @param rememberMe
   */
  private processLoginResponse(response: LoginResponse, rememberMe: boolean) {

    let accessToken = response.tokenAccess;

    if (accessToken == null)
      throw new Error("Received accessToken was empty");

    let idToken = response.tokenId;
    let refreshToken = response.tokenRefresh || this.refreshToken;
    let expiresIn = response.tokenExpiresIn;

    let tokenExpiryDate = new Date();
    tokenExpiryDate.setSeconds(tokenExpiryDate.getSeconds() + expiresIn);

    let accessTokenExpiry = tokenExpiryDate;

    let jwtHelper = new JwtHelper();
    let decodedIdToken = <TokenId>jwtHelper.decode(response.tokenId);

    let permissions: PermissionValues[] = Array.isArray(decodedIdToken.permission) ? decodedIdToken.permission : [decodedIdToken.permission];

    if (!this.isLoggedIn)
      this.settings.import(decodedIdToken.settings);

    let user = new UserModel(
      decodedIdToken.sub,
      decodedIdToken.name,
      decodedIdToken.fullname,
      decodedIdToken.email,
      decodedIdToken.phone,
      Array.isArray(decodedIdToken.role) ? decodedIdToken.role : [decodedIdToken.role]);
    user.isEnabled = true;

    this.saveUserDetails(user, permissions, accessToken, idToken, refreshToken, accessTokenExpiry, rememberMe);

    this.checkLoginStatus(user);

    return user;
  }

  /**
   * 
   * @param user
   * @param permissions
   * @param tokenAccess
   * @param tokenId
   * @param tokenRefresh
   * @param tokenExpiresIn
   * @param rememberMe
   */
  private saveUserDetails(
    user: UserModel,
    permissions: PermissionValues[],
    tokenAccess: string,
    tokenId: string,
    tokenRefresh: string,
    tokenExpiresIn: Date,
    rememberMe: boolean) {

    if (rememberMe) {
      this.localStoreManager.savePermanentData(tokenAccess, AppKeys.TOKEN_ACCESS);
      this.localStoreManager.savePermanentData(tokenId, AppKeys.TOKEN_ID);
      this.localStoreManager.savePermanentData(tokenRefresh, AppKeys.TOKEN_REFRESH);
      this.localStoreManager.savePermanentData(tokenExpiresIn, AppKeys.TOKEN_EXPIRES_IN);
      this.localStoreManager.savePermanentData(permissions, AppKeys.USER_PERMISSIONS);
      this.localStoreManager.savePermanentData(user, AppKeys.CURRENT_USER);
    }
    else {
      this.localStoreManager.saveSyncedSessionData(tokenAccess, AppKeys.TOKEN_ACCESS);
      this.localStoreManager.saveSyncedSessionData(tokenId, AppKeys.TOKEN_ID);
      this.localStoreManager.saveSyncedSessionData(tokenRefresh, AppKeys.TOKEN_REFRESH);
      this.localStoreManager.saveSyncedSessionData(tokenExpiresIn, AppKeys.TOKEN_EXPIRES_IN);
      this.localStoreManager.saveSyncedSessionData(permissions, AppKeys.USER_PERMISSIONS);
      this.localStoreManager.saveSyncedSessionData(user, AppKeys.CURRENT_USER);
    }

    this.localStoreManager.savePermanentData(rememberMe, AppKeys.REMEMBER_ME);
  }
}
