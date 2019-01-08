export interface LoginResponse {
  tokenAccess: string;
  tokenId: string;
  tokenRefresh: string;
  tokenExpiresIn: number;
}
