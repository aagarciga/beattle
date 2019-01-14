import { AuthorizationValue, AuthorizationName } from "../common/application-types";

export class AuthorizationModel {
  public static readonly authorizationUsersView: AuthorizationValue = "users.view";
  public static readonly authorizationUsersManage: AuthorizationValue = "users.manage";

  public static readonly authorizationRolesView: AuthorizationValue = "roles.view";
  public static readonly authorizationRolesManage: AuthorizationValue = "roles.manage";
  public static readonly authorizationRolesAssign: AuthorizationValue = "roles.assign";

  public name: AuthorizationName;
  public value: AuthorizationValue;
  public groupName: string;
  public description: string;

  constructor(
    name?: AuthorizationName,
    value?: AuthorizationValue,
    groupName?: string,
    description?: string
  ) {
    this.name = name;
    this.value = value;
    this.groupName = groupName;
    this.description = description;
  }


}
