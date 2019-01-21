import { Injectable } from '@angular/core';
import { RoleModel } from '../models/role.model';
import { RoleOperation, AuthorizationValue } from '../common/application-types';
import { Subject, forkJoin, Observable } from 'rxjs';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthorizationService } from './authorization.service';
import { AccountEndPointService } from './endpoints/account-endpoint.service';
import { UserModel } from '../models/user.model';
import { UserUpdateModel } from '../models/userUpdate.model';
import { mergeMap, tap } from 'rxjs/operators';
import { AuthorizationModel } from '../models/authorization.model';


export type ChangeEventArg = {
  roles: RoleModel[] | string[], operation: RoleOperation
};

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  public static readonly RoleOperationAdd: RoleOperation = "add";
  public static readonly RoleOperationUpdate: RoleOperation = "update";
  public static readonly RoleOperationDelete: RoleOperation = "delete";

  private _changeEvent = new Subject<ChangeEventArg>();

  get authorizations(): AuthorizationValue[] {
    return this.authorizationService.userAuthorizations;
  }

  get currentUser() {
    return this.authorizationService.currentUser;
  }

  constructor(
    private router: Router,
    private httpClient: HttpClient,
    private authorizationService: AuthorizationService,
    private accountEndPointService: AccountEndPointService
  ) { }

  getUser(id?: string) {
    return this.accountEndPointService.getUserByUserIdEndPoint<UserModel>(id);
  }

  getUserAndRoles(id?: string) {
    return forkJoin(
      this.accountEndPointService.getUserByUserIdEndPoint<UserModel>(id),
      this.accountEndPointService.getRolesEndPoint <RoleModel[]>()
    );    
  }

  getUsers(page?: number, pageSize?: number) {
    return this.accountEndPointService.getUsersEndPoint<UserModel[]>(page, pageSize);
  }

  getUsersAndRoles(page?: number, pageSize?: number) {
    return forkJoin(
      this.accountEndPointService.getUsersEndPoint<UserModel[]>(page, pageSize),
      this.accountEndPointService.getRolesEndPoint<RoleModel[]>());
  }

  updateUser(userUpdateModel: UserUpdateModel) {
    if (userUpdateModel.id) {
      return this.accountEndPointService.getUserUpdateEndPoint(userUpdateModel, userUpdateModel.id);
    }
    else {
      return this.accountEndPointService.getUserByUserNameEndPoint<UserModel>(userUpdateModel.userName).pipe<UserModel>(
        mergeMap(foundUser => {
          userUpdateModel.id = foundUser.id;
          return this.accountEndPointService.getUserUpdateEndPoint(userUpdateModel, userUpdateModel.id)
        }));
    }
  }

  newUser(userUpdateModel: UserUpdateModel) {
    return this.accountEndPointService.getUserNewEndPoint<UserModel>(userUpdateModel);
  }

  getUserPreferences() {
    return this.accountEndPointService.getUserPreferencesEndPoint<string>();
  }

  updateUserPreferences(setting: string) {
    return this.accountEndPointService.getUserPreferencesUpdateEndPoint(setting);
  }

  deleteUser(userOrUserId: string | UserUpdateModel): Observable<UserModel> {

    if (typeof userOrUserId === 'string' || userOrUserId instanceof String) {
      return this.accountEndPointService.getUserDeleteEndPoint<UserModel>(<string>userOrUserId).pipe<UserModel>(
        tap(data => this.onRolesUserCountChange(data.roles)));
    }
    else {

      if (userOrUserId.id) {
        return this.deleteUser(userOrUserId.id);
      }
      else {
        return this.accountEndPointService.getUserByUserNameEndPoint<UserModel>(userOrUserId.userName).pipe<UserModel>(
          mergeMap(user => this.deleteUser(user.id)));
      }
    }
  }

  unblockUser(id: string) {
    return this.accountEndPointService.getUserUnblockEndPoint(id);
  }

  userHasPermission(authorizationValue: AuthorizationValue): boolean {
    return this.authorizations.some(p => p == authorizationValue);
  }

  refreshLoggedInUser() {
    return this.authorizationService.refreshLogin();
  }

  getRoles(page?: number, pageSize?: number) {
    return this.accountEndPointService.getRolesEndPoint<RoleModel[]>(page, pageSize);
  }

  getRolesAndAuthorizations(page?: number, pageSize?: number) {
    return forkJoin(
      this.accountEndPointService.getRolesEndPoint<RoleModel[]>(page, pageSize),
      this.accountEndPointService.getAuthorizationsEndPoint<AuthorizationModel[]>());
  }

  updateRole(role: RoleModel) {
    if (role.id) {
      return this.accountEndPointService.getRoleUpdateEndPoint(role, role.id).pipe(
        tap(data => this.onRolesChanged([role], AccountService.RoleOperationUpdate)));
    }
    else {
      return this.accountEndPointService.getRoleByRoleNameEndPoint<RoleModel>(role.name).pipe<RoleModel>(
        mergeMap(foundRole => {
          role.id = foundRole.id;
          return this.accountEndPointService.getRoleUpdateEndPoint(role, role.id)
        }),
        tap(data => this.onRolesChanged([role], AccountService.RoleOperationUpdate)));
    }
  }

  newRole(role: RoleModel) {
    return this.accountEndPointService.getRoleNewEndPoint<RoleModel>(role).pipe<RoleModel>(
      tap(data => this.onRolesChanged([role], AccountService.RoleOperationAdd)));
  }

  deleteRole(roleOrRoleId: string | RoleModel): Observable<RoleModel> {

    if (typeof roleOrRoleId === 'string' || roleOrRoleId instanceof String) {
      return this.accountEndPointService.getRoleDeleteEndPoint<RoleModel>(<string>roleOrRoleId).pipe<RoleModel>(
        tap(data => this.onRolesChanged([data], AccountService.RoleOperationDelete)));
    }
    else {

      if (roleOrRoleId.id) {
        return this.deleteRole(roleOrRoleId.id);
      }
      else {
        return this.accountEndPointService.getRoleByRoleNameEndPoint<RoleModel>(roleOrRoleId.name).pipe<RoleModel>(
          mergeMap(role => this.deleteRole(role.id)));
      }
    }
  }

  getAuthorizations() {
    return this.accountEndPointService.getAuthorizationsEndPoint<AuthorizationModel[]>();
  }

  getChangeEvent(): Observable<ChangeEventArg> {
    return this._changeEvent.asObservable();
  }

  private onRolesUserCountChange(roles: RoleModel[] | string[]) {
    return this.onRolesChanged(roles, AccountService.RoleOperationUpdate);
  }
  
  private onRolesChanged(roles: RoleModel[] | string[], op: RoleOperation) {
    this._changeEvent.next({ roles: roles, operation: op });
  }
}
