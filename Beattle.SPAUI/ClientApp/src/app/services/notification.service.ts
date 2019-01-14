import { Injectable } from '@angular/core';
import { map, flatMap, startWith } from 'rxjs/operators';
import { AuthorizationService } from './authorization.service';
import { NotificationEndPointService } from './endpoints/notification-endpoint.service';
import { interval, Observable } from 'rxjs';
import { NotificationModel } from '../models/notification.model';


@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  private lastNotificationDate: Date;
  private _recentNotifications: NotificationModel[];

  get recentNotifications() {
    return this._recentNotifications;
  }

  set recentNotifications(notifications: NotificationModel[]) {
    this._recentNotifications = notifications;
  }

  constructor(
    private notificationEndPointService: NotificationEndPointService,
    private authorizationService: AuthorizationService
  )  {

  }


  getNotification(notificationId?: number) {

    return this.notificationEndPointService.getNotificationEndPoint(notificationId).pipe(
      map(response => NotificationModel.Create(response)));
  }


  getNotifications(page: number, pageSize: number) {

    return this.notificationEndPointService.getNotificationsEndPoint(page, pageSize).pipe(
      map(response => {
        return this.getNotificationsFromResponse(response);
      }));
  }

  getUnreadNotifications(userId?: string) {

    return this.notificationEndPointService.getUnreadNotificationsEndPoint(userId).pipe(
      map(response => this.getNotificationsFromResponse(response)));
  }

  getNewNotifications() {
    return this.notificationEndPointService.getNewNotificationsEndpoint(this.lastNotificationDate).pipe(
      map(response => this.processNewNotificationsFromResponse(response)));
  }

  getNewNotificationsPeriodically() {
    return interval(10000).pipe(
      startWith(0),
      flatMap(() => {
        return this.notificationEndPointService.getNewNotificationsEndpoint(this.lastNotificationDate).pipe(
          map(response => this.processNewNotificationsFromResponse(response)));
      }));
  }

  readUnreadNotification(notificationIds: number[], isRead: boolean): Observable<any> {

    return this.notificationEndPointService.getReadUnreadNotificationEndpoint(notificationIds, isRead);
  }

  deleteNotification(notificationOrNotificationId: number | NotificationModel): Observable<NotificationModel> {

    if (typeof notificationOrNotificationId === 'number' || notificationOrNotificationId instanceof Number) { //Todo: Test me if its check is valid
      return this.notificationEndPointService.getDeleteNotificationEndpoint(<number>notificationOrNotificationId).pipe(
        map(response => {
          this.recentNotifications = this.recentNotifications.filter(n => n.id != notificationOrNotificationId);
          return NotificationModel.Create(response);
        }));
    }
    else {
      return this.deleteNotification(notificationOrNotificationId.id);
    }
  }

  private processNewNotificationsFromResponse(response) {
    let notifications = this.getNotificationsFromResponse(response);

    for (let n of notifications) {
      if (n.date > this.lastNotificationDate)
        this.lastNotificationDate = n.date;
    }

    return notifications;
  }

  private getNotificationsFromResponse(response) {
    let notifications: NotificationModel[] = [];

    for (let i in response) {
      notifications[i] = NotificationModel.Create(response[i]);
    }

    notifications.sort((a, b) => b.date.valueOf() - a.date.valueOf());
    notifications.sort((a, b) => (a.isPinned === b.isPinned) ? 0 : a.isPinned ? -1 : 1);

    this.recentNotifications = notifications;

    return notifications;
  }

  pinUnpinNotification(notificationOrNotificationId: number | NotificationModel, isPinned?: boolean): Observable<any> {

    if (typeof notificationOrNotificationId === 'number' || notificationOrNotificationId instanceof Number) {
      return this.notificationEndPointService.getPinUnpinNotificationEndpoint(<number>notificationOrNotificationId, isPinned);
    }
    else {
      return this.pinUnpinNotification(notificationOrNotificationId.id);
    }
  }

  get currentUser() {
    return this.authorizationService.currentUser;
  }
}
