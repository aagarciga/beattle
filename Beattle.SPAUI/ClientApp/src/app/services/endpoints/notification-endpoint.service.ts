import { Injectable } from '@angular/core';
import { HttpResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { HttpStatusCodes } from 'src/app/enums/http-status-codes.enum';

@Injectable({
  providedIn: 'root'
})

  

export class NotificationEndPointService {

  private demoNotifications = [
    {
      "id": 1,
      "header": "20 New Products were added to your inventory by \"administrator\"",
      "body": "20 new \"BMW M6\" were added to your stock by \"administrator\" on 5/28/2017 4:54:13 PM",
      "isRead": true,
      "isPinned": true,
      "date": "2017-05-28T16:29:13.5877958"
    },
    {
      "id": 2,
      "header": "1 Product running low",
      "body": "You are running low on \"Nissan Patrol\". 2 Items remaining",
      "isRead": false,
      "isPinned": false,
      "date": "2017-05-28T19:54:42.4144502"
    },
    {
      "id": 3,
      "header": "Tomorrow is half day",
      "body": "Guys, tomorrow we close by midday. Please check in your sales before noon. Thanx. Alex.",
      "isRead": false,
      "isPinned": false,
      "date": "2017-05-30T11:13:42.4144502"
    }
  ];

  constructor() { }

  getNotificationEndPoint<T>(notificationId: number): Observable<T> {

    let notification = this.demoNotifications.find(val => val.id == notificationId);
    let response: HttpResponse<T>;

    if (notification) {
      response = this.createResponse<T>(notification, HttpStatusCodes.OK);
    }
    else {
      response = this.createResponse<T>(null, HttpStatusCodes.NotFound);
    }

    return of(response.body);
  }

  getNotificationsEndPoint<T>(page: number, pageSize: number): Observable<T> {

    let notifications = this.demoNotifications;
    let response = this.createResponse<T>(notifications, HttpStatusCodes.OK);

    return of(response.body);
  }

  getUnreadNotificationsEndPoint<T>(userId?: string): Observable<T> {

    let unreadNotifications = this.demoNotifications.filter(val => !val.isRead);
    let response = this.createResponse<T>(unreadNotifications, HttpStatusCodes.OK);

    return of(response.body);
  }

  getNewNotificationsEndpoint<T>(lastNotificationDate?: Date): Observable<T> {

    let unreadNotifications = this.demoNotifications;
    let response = this.createResponse<T>(unreadNotifications, HttpStatusCodes.OK);

    return of(response.body);
  }

  getPinUnpinNotificationEndpoint<T>(notificationId: number, isPinned?: boolean, ): Observable<T> {

    let notification = this.demoNotifications.find(val => val.id == notificationId);
    let response: HttpResponse<T>;

    if (notification) {
      response = this.createResponse<T>(null, HttpStatusCodes.NoContent);

      if (isPinned == null)
        isPinned = !notification.isPinned;

      notification.isPinned = isPinned;
      notification.isRead = true;
    }
    else {
      response = this.createResponse<T>(null, HttpStatusCodes.NotFound);
    }


    return of(response.body);
  }

  getReadUnreadNotificationEndpoint<T>(notificationIds: number[], isRead: boolean, ): Observable<T> {

    for (let notificationId of notificationIds) {

      let notification = this.demoNotifications.find(val => val.id == notificationId);

      if (notification) {
        notification.isRead = isRead;
      }
    }

    let response = this.createResponse<T>(null, HttpStatusCodes.NoContent);
    return of(response.body);
  }

  getDeleteNotificationEndpoint<T>(notificationId: number): Observable<T> {

    let notification = this.demoNotifications.find(val => val.id == notificationId);
    let response: HttpResponse<T>;

    if (notification) {
      this.demoNotifications = this.demoNotifications.filter(val => val.id != notificationId)
      response = this.createResponse<T>(notification, HttpStatusCodes.OK);
    }
    else {
      response = this.createResponse<T>(null, HttpStatusCodes.NotFound);
    }

    return of(response.body);
  }

  private createResponse<T>(body, status: number) {
    return new HttpResponse<T>({ body: body, status: status });
  }
}
