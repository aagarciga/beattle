import { Component, ViewEncapsulation, OnInit, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  //encapsulation: ViewEncapsulation.None // Use only on development
})
export class AppComponent implements OnInit, AfterViewInit {

  isAppLoaded: boolean;
  title = 'Beattle';

  ngAfterViewInit(): void {
    throw new Error("Method not implemented.");
  }
  
  ngOnInit(): void {
    // Wait for 1 sec for loading animation
    setTimeout(() => this.isAppLoaded = true, 1000);
  }

}
