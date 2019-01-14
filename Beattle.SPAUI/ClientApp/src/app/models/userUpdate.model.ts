import { UserModel } from "./user.model";

export class UserUpdateModel extends UserModel {
  public currentPassword: string;
  public newPassword: string;
  public confirmPassword: string;

  constructor(currentPassword?: string, newPassword?: string , confirmPassword?: string) {
    super();

    this.currentPassword = currentPassword;
    this.newPassword = newPassword;
      this.confirmPassword = confirmPassword;
  }
}
