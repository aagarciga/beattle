import { Injectable } from "@angular/core";

/**
 * JSON Web Token Helper
 * JSON Web Token is a JSON-based open standard for creating access tokens
 * that assert some number of claims.
 */
@Injectable()
export class JwtHelper {

  /**
   * 
   * @param str URL to decode
   */
  public base64Decode(str: string): string {
    let result = str.replace(/-/g, '+').replace(/_/g, '/');
    switch (result.length % 4) {
      case 0: { break; }
      case 2: { result += '=='; break; }
      case 3: { result += '='; break; }
      default: {
        throw 'Illegal base64url string!';
      }
    }
    return this.base64DecodeUnicode(result);
  }

  /**
   * 
   * @param token
   */
  public decode(token: string): any {
    let parts = token.split('.');

    if (parts.length !== 3) {
      throw new Error('JWT must have 3 parts');
    }

    let decoded = this.base64Decode(parts[1]);
    if (!decoded) {
      throw new Error('Cannot decode the token');
    }

    return JSON.parse(decoded);
  }

  /**
   * 
   * @param token
   */
  public getExpiryDateFrom(token: string): Date {

    let decoded: any = this.decode(token);

    if (!decoded.hasOwnProperty('exp')) {
      return null;
    }

    // The 0 here is the key, which sets the date to the epoch
    let date = new Date(0); 
    date.setUTCSeconds(decoded.exp);

    return date;
  }

  /**
   * 
   * @param token
   * @param offsetSeconds
   */
  public isTokenExpired(token: string, offsetSeconds?: number): boolean {

    let date = this.getExpiryDateFrom(token);
    offsetSeconds = offsetSeconds || 0;

    if (date == null) {
      return false;
    }

    // Token expired?
    return !(date.valueOf() > (new Date().valueOf() + (offsetSeconds * 1000)));
  }

  /**
   * 
   * @param str
   * @see https://developer.mozilla.org/en/docs/Web/API/WindowBase64/Base64_encoding_and_decoding#The_Unicode_Problem
   */
  private base64DecodeUnicode(str: any) {
    return decodeURIComponent(Array.prototype.map.call(atob(str), (c: any) => {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
  }
}
