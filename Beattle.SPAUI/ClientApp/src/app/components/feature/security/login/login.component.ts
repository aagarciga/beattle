import { Component, OnInit, Input } from '@angular/core';

import { UserLoginModel } from "../../../../models/user-login.model";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  userLoginModel = new UserLoginModel();
  isLoading = false;
  formResetToggle = true;
  modalCloseCallback: () => void;
  loginStatusSubscription: any;

  @Input()
  isModal = false;

  constructor() { }

  ngOnInit() {
  }

}
