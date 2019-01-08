import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { ConfigurationService } from '../configuration.service';
import { Subject, Observable, throwError } from 'rxjs';
import { mergeMap, switchMap, catchError } from 'rxjs/operators';
import { AuthorizationService } from '../authorization.service';

@Injectable({
  providedIn: 'root'
})
export class EndPointFactoryService {

  static readonly apiVersion: string = "1";

  constructor(
    protected httpClient: HttpClient,
    protected settings: ConfigurationService,
    private injector: Injector
  ) { }

  private readonly _httpParamsScope: string = 'openid email phone profile offline_access roles';
  private readonly _httpHeaderContentType: string = 'application/x-www-form-urlencoded';
  private readonly _loginUrl: string = "/connect/token";
  private get loginUrl() {
    return this.settings.baseUrl + this._loginUrl;
  }
  private taskPauser: Subject<any>;
  private isRefreshingLogin: boolean;
  private _authorizationService: AuthorizationService;
  private get authorizationService() {
    if (!this._authorizationService) {
      this._authorizationService = this.injector.get(AuthorizationService);
    }
    return this._authorizationService;
  }

  getLoginEndPoint<T>(username: string, password: string): Observable<T> {
    let params = new HttpParams()
      .append('username', username)
      .append('password', password)
      .append('grant_type', 'password')
      .append('scope', this._httpParamsScope);
    let requestBody = params.toString();
    let header = new HttpHeaders({ 'Content-Type': this._httpHeaderContentType })
    return this.httpClient.post<T>(this.loginUrl, requestBody, { headers: header });
  }
  getRefreshLoginEndPoint<T>(): Observable<T> {    
    let params = new HttpParams()
      .append('refresh_token', this.authorizationService.refreshToken)
      .append('grant_type', 'refresh_token')
      .append('scope', this._httpParamsScope);

    let requestBody = params.toString();
    let header = new HttpHeaders({ 'Content-Type': this._httpHeaderContentType });


    return this.httpClient.post<T>(this.loginUrl, requestBody, { headers: header }).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRefreshLoginEndPoint());
      }));
  }

  protected getRequestHeaders(): { headers: HttpHeaders | { [header: string]: string | string[]; } } {
    let headers = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authorizationService.accessToken,
      'Content-Type': 'application/json',
      'Accept': `application/vnd.iman.v${EndPointFactoryService.apiVersion}+json, application/json, text/plain, */*`,
      'App-Version': ConfigurationService.appVersion
    });

    return { headers: headers };
  }
  protected handleError(error, continuation: () => Observable<any>) {

    if (error.status == 401) {
      if (this.isRefreshingLogin) {
        return this.pauseTask(continuation);
      }

      this.isRefreshingLogin = true;

      return this.authorizationService.refreshLogin().pipe(
        mergeMap(data => {
          this.isRefreshingLogin = false;
          this.resumeTasks(true);

          return continuation();
        }),
        catchError(refreshLoginError => {
          this.isRefreshingLogin = false;
          this.resumeTasks(false);

          if (refreshLoginError.status == 401 || (refreshLoginError.url && refreshLoginError.url.toLowerCase().includes(this.loginUrl.toLowerCase()))) {
            this.authorizationService.reLogin();
            return throwError('session expired');
          }
          else {
            return throwError(refreshLoginError || 'server error');
          }
        }));
    }

    if (error.url && error.url.toLowerCase().includes(this.loginUrl.toLowerCase())) {
      this.authorizationService.reLogin();

      return throwError((error.error && error.error.error_description) ? `session expired (${error.error.error_description})` : 'session expired');
    }
    else {
      return throwError(error);
    }
  }

  private pauseTask(continuation: () => Observable<any>) {
    if (!this.taskPauser)
      this.taskPauser = new Subject();

    return this.taskPauser.pipe(switchMap(continueOp => {
      return continueOp ? continuation() : throwError('session expired');
    }));
  }
  private resumeTasks(continueOp: boolean) {
    setTimeout(() => {
      if (this.taskPauser) {
        this.taskPauser.next(continueOp);
        this.taskPauser.complete();
        this.taskPauser = null;
      }
    });
  }

}
