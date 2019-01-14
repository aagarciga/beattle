import { AppUtilities } from "../appUtilities";

export class NotificationModel {

  public static Create(data: {}) {    

    let result: NotificationModel = new NotificationModel();
    (<any>Object).assign(result, data);    

    if (result.date)
      result.date = AppUtilities.parseDate(result.date);

    return result;
  }

  public id: number;
  public header: string;
  public body: string;
  public isRead: boolean;
  public isPinned: boolean;
  public date: Date;
}
