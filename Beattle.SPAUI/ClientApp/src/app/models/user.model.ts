/*
 *
 * */
export class UserModel {

  public id: string;
  public userName: string;
  public fullName: string;
  public email: string;
  public phoneNumber: string;
  public isEnabled: boolean;
  public isLockedOut: boolean;
  public roles: string[];

  // Note: Using only optional constructor properties without backing store disables typescript's type checking for the type
  /**
   * 
   * @param id
   * @param userName
   * @param fullName
   * @param email
   * @param phoneNumber
   * @param roles
   */
  constructor(
    id?: string,
    userName?: string,
    fullName?: string,
    email?: string,
    phoneNumber?: string,
    roles?: string[]) {

    this.id = id;
    this.userName = userName;
    this.fullName = fullName;
    this.email = email;
    this.phoneNumber = phoneNumber;
    this.roles = roles;
  }

  get friendlyName(): string {
    let name = this.fullName || this.userName;

    /*
     if (this.jobTitle)
      name = this.jobTitle + " " + name;
      */

    return name;
  }
}
