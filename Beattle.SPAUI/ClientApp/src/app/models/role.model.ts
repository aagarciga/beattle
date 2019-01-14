import { AuthorizationModel } from "./authorization.model";

export class RoleModel {
  public id: string;
  public name: string;
  public description: string;
  public usersCount: string;
  public authorizations: AuthorizationModel[];

  constructor(name?: string, description?: string, authorizations?: AuthorizationModel[]) {
    this.name = name;
    this.description = description;
    this.authorizations = authorizations;
  }
}
