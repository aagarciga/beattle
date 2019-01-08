import { PermissionValues } from "../application-types";

export interface TokenId {
  sub: string;
  name: string;
  fullname: string;
  email: string;
  phone: string;
  role: string | string[];
  permission: PermissionValues | PermissionValues[];
  settings: string;
}
