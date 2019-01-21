import { AuthorizationValue } from "../application-types";

export interface TokenId {
  sub: string;
  name: string;
  fullname: string;
  email: string;
  phone: string;
  role: string | string[];
  authorizations: AuthorizationValue | AuthorizationValue[];
  settings: string;
}
