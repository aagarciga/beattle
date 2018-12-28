import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Route } from '@angular/compiler/src/core';

import { LoginComponent } from 'src/app/components/feature/security/login/login.component';
import { CounterComponent } from 'src/app/components/example/counter/counter.component';
import { FetchDataComponent } from 'src/app/components/example/fetch-data/fetch-data.component';
import { HomeComponent } from 'src/app/components/example/home/home.component';

const routes: Routes = [
  { path: "login", component: LoginComponent, data: { title: "Login" } },
  { path: 'counter', component: CounterComponent },
  { path: 'fetch-data', component: FetchDataComponent },
  { path: '', component: HomeComponent, pathMatch: 'full' },
];

@NgModule({
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ],
  exports: [
    RouterModule
  ],
  providers: [],
  declarations: []
})
export class AppRoutingModule { }
