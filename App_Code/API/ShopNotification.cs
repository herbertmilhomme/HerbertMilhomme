using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for ShopNotification
/// </summary>

/// <summery>
/// Shop Notification
/// </summery>
class ShopNotification {

    const ACTION_TYPE_PRODUCT_SOLD = "sold";

    var objectType = "shop";
    var objectID = 0;

    /// <summery>
    /// It will create Notification on Activities table
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer senderID   (the man who creates this alert)
    /// <typeparam name=""></typeparam> string  actionType : one of action types (const defined for this class)
    /// <typeparam name=""></typeparam> integer actionID   : related shop order ID
    /// </summery>
    function createNotification(userID, senderID, actionType, actionID){

        //Check if this user will get this notification. (it will be set by Notification setting page)
        userIns = new TradeUser();
        userData = userIns.getUserByID(userID);

        flagEnabled = 0; // user checked that he didn"t want to have this notification

        switch(actionType){
            case Activity.ACTION_TYPE_PRODUCT_SOLD:
                flagEnabled = userData["optProductSoldOnShop"];
                break;
        }

        if(flagEnabled == 1){
            //Create Notification.
            activityIns = new Activity();
            activityId = activityIns.addActivity(userID, senderID, this.objectType, actionType, actionID);

            activityIns.addNotification(userID, activityId, Activity.NOTIFICATION_TYPE_PRODUCT_SOLD);
        }

    }

    /// <summery>
    /// get Number of new notification
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam>       string type
    /// <returns></returns> one
    /// </summery>
    function getNumOfNewMessages(userID, type = null, isNew = 1){

        global db;

        if(type == null)
            query = db.prepare("SELECT count(*) FROM " + TABLE_MAIN_ACTIVITIES + " WHERE userID=%d AND isNew=%d AND objectType=%s", userID, isNew, this.objectType);else
            query = db.prepare("SELECT count(*) FROM " + TABLE_MAIN_ACTIVITIES + " WHERE userID=%d AND isNew=%d AND activityType=%s AND objectType=%s", userID, isNew, type, this.objectType);

        num = db.getVar(query);

        return num;
    }

    /// <summery>
    /// get all notification which this user has received.
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed limit
    /// <returns></returns> array
    /// </summery>
    function getReceivedMessages(userID, type = null, isNew = 1, limit = 5){

        global db;

        whereCond = "";
        if(type == null)
            whereCond = sprintf(" WHERE act.userID=%d AND act.isNew=%d AND act.objectType="%s" ORDER BY act.createdDate DESC ", userID, isNew, this.objectType);else
            whereCond = sprintf(" WHERE act.userID=%d AND act.isNew=%d AND act.activityType="%s" AND act.objectType="%s" ORDER BY act.createdDate DESC ", userID, isNew, type, this.objectType);

        if(is_numeric(limit) && limit > 0){
            whereCond += " LIMIT " + limit;
        }

        query = sprintf("
                SELECT act.objectID AS senderID, act.activityType, act.createdDate, act.actionID, user.firstName, user.lastName   
                    FROM %s AS act 
                    LEFT JOIN %s AS USER ON USER.userID = act.objectID
                %s 
            ", TABLE_MAIN_ACTIVITIES, TABLE_USERS, whereCond);

        messageList = db.getResultsArray(query);
        newMessageList = [];

        if(count(messageList) > 0){
            foreach(messageList as msgData){
                data = [];
                data["senderID"] = msgData["senderID"];
                data["senderName"] = trim(msgData["firstName"] + " " + msgData["lastName"]);
                data["activityType"] = msgData["activityType"];
                data["createdDate"] = msgData["createdDate"];
                data["actionID"] = msgData["actionID"];

                newMessageList[] = data;
            }
        }

        return newMessageList;

    }

    /// <summery>
    /// Mark message as read
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> string  actionType one of value of this action type (offer_received, offer_accepted,offer_declined,feedback)
    /// </summery>
    function markAsRead(userID, actionType = null){

        global db;

        if(!is_numeric(userID))
            return;

        if(actionType != null){
            query = sprintf("UPDATE %s SET isNew=0 WHERE userID=%d AND objectType="%s" AND activityType="%s"", TABLE_MAIN_ACTIVITIES, userID, this.objectType, actionType);
        }else{
            query = sprintf("UPDATE %s SET isNew=0 WHERE userID=%d AND objectType="%s"", TABLE_MAIN_ACTIVITIES, userID, this.objectType);
        }

        db.query(query);

        return;

    }

}
