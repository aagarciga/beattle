import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { ModuleMapLoaderModule } from '@nguniversal/module-map-ngfactory-loader';
import { AppComponent } from './app.component';
import { AppModule } from './app.module';

@NgModule({
  /*
    Angular Universal generates static application pages on the server
    through a process called server-side rendering (SSR).
    When Universal is integrated with your app, it can generate and serve
    those pages in response to requests from browsers. It can also
    pre-generate pages as HTML files that you serve later.

    In order to use Angular Universal
    read documentation https://angular.io/guide/universal
  */
  imports: [
    // The AppServerModule should import AppModule followed
    // by the ServerModule from @angular/platform-server
    AppModule,
    ServerModule,
    // Important to have lazy-loaded routes work
    ModuleMapLoaderModule
  ],
    bootstrap: [AppComponent]
})
export class AppServerModule { }
