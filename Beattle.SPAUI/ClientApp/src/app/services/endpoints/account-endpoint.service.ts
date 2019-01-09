import { Injectable, Injector } from '@angular/core';
import { EndPointFactoryService } from './endpoint-factory.service';
import { HttpClient } from '@angular/common/http';
import { ConfigurationService } from '../configuration.service';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AccountEndPointService extends EndPointFactoryService {

  private readonly _usersUrl: string = "/api/account/users";
  private readonly _userByUserNameUrl: string = "/api/account/users/username";
  private readonly _currentUserUrl: string = "/api/account/users/me";
  private readonly _currentUserPreferencesUrl: string = "/api/account/users/me/preferences";
  private readonly _unblockUserUrl: string = "/api/account/users/unblock";
  private readonly _rolesUrl: string = "/api/account/roles";
  private readonly _roleByRoleNameUrl: string = "/api/account/roles/name";
  private readonly _authorizationsUrl: string = "/api/account/authorizations";

  get usersUrl() { return this.settings.baseUrl + this._usersUrl; }
  get userByUserNameUrl() { return this.settings.baseUrl + this._userByUserNameUrl; }
  get currentUserUrl() { return this.settings.baseUrl + this._currentUserUrl; }
  get currentUserPreferencesUrl() { return this.settings.baseUrl + this._currentUserPreferencesUrl; }
  get unblockUserUrl() { return this.settings.baseUrl + this._unblockUserUrl; }
  get rolesUrl() { return this.settings.baseUrl + this._rolesUrl; }
  get roleByRoleNameUrl() { return this.settings.baseUrl + this._roleByRoleNameUrl; }
  get authorizationsUrl() { return this.settings.baseUrl + this._authorizationsUrl; }

  constructor(
    httpClient: HttpClient,
    settings: ConfigurationService,
    injector: Injector) {

    super(httpClient, settings, injector);
  }

  getUsersEndPoint<T>(page?: number, pageSize?: number): Observable<T> {
    let endpointUrl = page && pageSize ? `${this.usersUrl}/${page}/${pageSize}` : this.usersUrl;

    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUsersEndPoint(page, pageSize));
      }));
  }
  getUserByUserIdEndPoint<T>(userId?: string): Observable<T> {
    let endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;

    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserByUserIdEndPoint(userId));
      }));
  }
  getUserByUserNameEndPoint<T>(userName: string): Observable<T> {
    let endpointUrl = `${this.userByUserNameUrl}/${userName}`;

    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserByUserNameEndPoint(userName));
      }));
  }
  getUserNewEndPoint<T>(userObject: any): Observable<T> {

    return this.httpClient.post<T>(this.usersUrl, JSON.stringify(userObject), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserNewEndPoint(userObject));
      }));
  }
  getUserUpdateEndPoint<T>(userObject: any, userId?: string): Observable<T> {
    let endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;

    return this.httpClient.put<T>(endpointUrl, JSON.stringify(userObject), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserUpdateEndPoint(userObject, userId));
      }));
  }
  getUserUpdatePatchEndPoint<T>(patch: {}, userId?: string): Observable<T>
  getUserUpdatePatchEndPoint<T>(value: any, op: string, path: string, from?: any, userId?: string): Observable<T>
  getUserUpdatePatchEndPoint<T>(valueOrPatch: any, opOrUserId?: string, path?: string, from?: any, userId?: string): Observable<T> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = userId ? `${this.usersUrl}/${userId}` : this.currentUserUrl;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrUserId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrUserId }];
    }
    else {
      endpointUrl = opOrUserId ? `${this.usersUrl}/${opOrUserId}` : this.currentUserUrl;
      patchDocument = valueOrPatch;
    }

    return this.httpClient.patch<T>(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserUpdatePatchEndPoint(valueOrPatch, opOrUserId, path, from, userId));
      }));
  }
  getUserPreferencesEndPoint<T>(): Observable<T> {

    return this.httpClient.get<T>(this.currentUserPreferencesUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserPreferencesEndPoint());
      }));
  }
  getUserPreferencesUpdateEndPoint<T>(settings: string): Observable<T> {
    return this.httpClient.put<T>(this.currentUserPreferencesUrl, JSON.stringify(settings), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserPreferencesUpdateEndPoint(settings));
      }));
  }
  getUserUnblockEndPoint<T>(userId: string): Observable<T> {
    let endpointUrl = `${this.unblockUserUrl}/${userId}`;

    return this.httpClient.put<T>(endpointUrl, null, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserUnblockEndPoint(userId));
      }));
  }
  getUserDeleteEndPoint<T>(userId: string): Observable<T> {
    let endpointUrl = `${this.usersUrl}/${userId}`;

    return this.httpClient.delete<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUserDeleteEndPoint(userId));
      }));
  }

  getRolesEndPoint<T>(page?: number, pageSize?: number): Observable<T> {
    let endpointUrl = page && pageSize ? `${this.rolesUrl}/${page}/${pageSize}` : this.rolesUrl;

    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRolesEndPoint(page, pageSize));
      }));
  }
  getRoleEndPoint<T>(roleId: string): Observable<T> {
    let endpointUrl = `${this.rolesUrl}/${roleId}`;
    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRoleEndPoint(roleId));
      }));
  }
  getRoleByRoleNameEndPoint<T>(roleName: string): Observable<T> {
    let endpointUrl = `${this.roleByRoleNameUrl}/${roleName}`;
    return this.httpClient.get<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRoleByRoleNameEndPoint(roleName));
      }));
  }
  getRoleNewEndPoint<T>(roleObject: any): Observable<T> {
    return this.httpClient.post<T>(this.rolesUrl, JSON.stringify(roleObject), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRoleNewEndPoint(roleObject));
      }));
  }
  getRoleUpdateEndPoint<T>(roleObject: any, roleId: string): Observable<T> {
    let endpointUrl = `${this.rolesUrl}/${roleId}`;
    return this.httpClient.put<T>(endpointUrl, JSON.stringify(roleObject), this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRoleUpdateEndPoint(roleObject, roleId));
      }));
  }
  getRoleDeleteEndPoint<T>(roleId: string): Observable<T> {
    let endpointUrl = `${this.rolesUrl}/${roleId}`;
    return this.httpClient.delete<T>(endpointUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getRoleDeleteEndPoint(roleId));
      }));
  }

  getPermissionsEndPoint<T>(): Observable<T> {
    return this.httpClient.get<T>(this.authorizationsUrl, this.getRequestHeaders()).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getPermissionsEndPoint());
      }));
  }
}
