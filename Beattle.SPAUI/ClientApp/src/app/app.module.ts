import { BrowserModule } from '@angular/platform-browser';
import { NgModule, ErrorHandler } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './components/example/nav-menu/nav-menu.component';
import { HomeComponent } from './components/example/home/home.component';
import { CounterComponent } from './components/example/counter/counter.component';
import { FetchDataComponent } from './components/example/fetch-data/fetch-data.component';
import { LoginComponent } from './components/feature/security/login/login.component';
import { AppRoutingModule } from './modules/app-routing/app-routing.module';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateLanguageLoader, I18nService } from './services/i18n.service';
import { ToastaModule } from 'ngx-toasta';

import { NgxDatatableModule } from '@swimlane/ngx-datatable';


import { TooltipModule } from "ngx-bootstrap/tooltip";
import { PopoverModule } from "ngx-bootstrap/popover";
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ModalModule } from 'ngx-bootstrap/modal';
import { AlertService } from './services/alert.service';
import { ConfigurationService } from './services/configuration.service';
import { ApplicationTitleService } from './services/application-title.service';
import { NotificationService } from './services/notification.service';
import { NotificationEndPointService } from './services/endpoints/notification-endpoint.service';
import { AccountService } from './services/account.service';
import { AccountEndPointService } from './services/endpoints/account-endpoint.service';
import { LocalStoreManagerService } from './services/local-store-manager.service';
import { EndPointFactoryService } from './services/endpoints/endpoint-factory.service';
import { ApplicationErrorHandler } from './app-error.handler';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    LoginComponent
  ],
  imports: [
    // Add .withServerTransition() to support Universal rendering.
    // The application ID can be any identifier which is unique on
    // the page.
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    }),
    NgxDatatableModule,
    ToastaModule.forRoot(),
    TooltipModule.forRoot(),
    PopoverModule.forRoot(),
    BsDropdownModule.forRoot(),
    ModalModule.forRoot()  
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl },
    { provide: ErrorHandler, useClass: ApplicationErrorHandler },
    AlertService,
    ConfigurationService,
    ApplicationTitleService,
    I18nService,
    NotificationService,
    NotificationEndPointService,
    AccountService,
    AccountEndPointService,
    LocalStoreManagerService,
    EndPointFactoryService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}
