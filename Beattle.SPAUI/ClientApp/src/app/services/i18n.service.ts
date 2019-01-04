import { Injectable } from '@angular/core';
import { Subject, Observable, of } from 'rxjs';
import { TranslateService, TranslateLoader } from '@ngx-translate/core';
import { browser } from 'protractor';
import { setTimeout } from 'timers';

@Injectable({
  providedIn: 'root'
})
export class I18nService {

  readonly defaultLanguage = "en";
  readonly languages = /en|es|nl|de/;
  private onLanguageChange = new Subject<string>();
  languajeChange$ = this.onLanguageChange.asObservable();

  constructor(private translateService: TranslateService) {

    this.setDefaultLanguage(this.defaultLanguage);
  }

  setDefaultLanguage(language: string) {
    this.translateService.setDefaultLang(language);
  }

  addLanguages(languages: string[]) {
    this.translateService.addLangs(languages);
  }

  getDefaultLanguage() {
    return this.translateService.defaultLang;
  }

  getBrowserLanguage() {
    return this.translateService.getBrowserLang();
  }

  changeLanguage(language: string = this.defaultLanguage) {
    if (!language) {
      language = this.translateService.defaultLang;
    }

    if (language != this.translateService.currentLang) {
      setTimeout(() => {
        this.translateService.use(language);
        this.onLanguageChange.next(language);
      }, 1000);
    }
  }

  useBrowserLanguage(): string | void {
    let browserLanguage = this.getBrowserLanguage();

    if (browserLanguage.match(this.languages)) {
      this.changeLanguage(browserLanguage);
      return browserLanguage;
    }
  }

  getTranslation(key: string | Array<string>, interpolateParams?: Object): string | any {
    return this.translateService.instant(key, interpolateParams);
  }

  getTranslationAsync(key: string | Array<string>, interpolateParams?: Object): Observable<string | any> {
    return this.translateService.get(key, interpolateParams);
  }
}

export class TranslateLanguageLoader implements TranslateLoader {
  public getTranslation(lang: string): any {
    //Note Dynamic require(variable) will not work. Require is always at compile time

    switch (lang) {
      case "es":
        return of(require("../assets/locale/es.json"));
      case "nl":
        return of(require("../assets/locale/nl.json"));
      case "de":
        return of(require("../assets/locale/de.json"));
      default:
        return of(require("../assets/locale/en.json"));
    }
  }
}
