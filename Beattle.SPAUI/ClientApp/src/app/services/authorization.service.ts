import { Injectable } from '@angular/core';
import { ConfigurationService } from './configuration.service';
import { Router } from '@angular/router';
import { EndpointFactoryService } from './endpoint-factory.service';
import { LocalStoreManagerService } from './local-store-manager.service';
import { Observable } from 'rxjs';
import { AppKeys } from '../appKeys';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {

  constructor(
    private router: Router,
    private settings: ConfigurationService,
    private endPointFactory: EndpointFactoryService,
    private localStoreManager: LocalStoreManagerService
  ) {
    // TODO: Initialization for login status
  }

  get refreshToken(): string {
    //TODO: implement
    return "";
  }

  get accessToken(): string {
    //TODO: implement
    return "";
  }

  refreshLogin(){
    return this.endPointFactory.getRefreshLoginEndpoint<LoginResponse>().pipe(
      map(response => this.processLoginResponse(response, this.rememberMe)));
  }

  reLogin() {

    this.localStoreManager.deleteData(AppKeys.TOKEN_EXPIRES_IN);

    if (this.reLoginDelegate) {
      this.reLoginDelegate();
    }
    else {
      this.redirectForLogin();
    }
  }
}
