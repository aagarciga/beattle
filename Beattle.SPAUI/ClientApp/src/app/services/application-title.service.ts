import { Injectable } from '@angular/core';
import { Subscription } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { Router, NavigationEnd } from '@angular/router';
import { filter, map, flatMap } from 'rxjs/operators';
import { AppUtilities } from '../appUtilities';


@Injectable({
  providedIn: 'root'
})
export class ApplicationTitleService {

  subscription: Subscription;
  applicationName: string;

  constructor(
    private titleService: Title,
    private router: Router
  ) {
    this.subscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      map(_ => this.router.routerState.root),
      map(route => {
        while (route.firstChild)
          route = route.firstChild;

        return route;
      }),
      flatMap(route => route.data))
      .subscribe(data => {
        let title = data['title'];

        if (title) {
          let fragment = this.router.url.split('#')[1]

          if (fragment)
            title += " | " + AppUtilities.toTitleCase(fragment);
        }

        if (title && this.applicationName)
          title += ' - ' + this.applicationName;
        else if (this.applicationName)
          title = this.applicationName;

        if (title)
          this.titleService.setTitle(title);
      });
  }
}
