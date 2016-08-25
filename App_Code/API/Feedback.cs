using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Feedback
/// </summary>

/// <summery>
///  Feedback management
/// It will have  feedbacks for trade section and shop section
/// </summery>
class Feedback {

    const ACTIVITY_TYPE_TRADE = 1;
    const ACTIVITY_TYPE_SHOP = 2;

    /// <summery>
    /// Add feedback
    /// 
    /// <typeparam name=""></typeparam> mixed writerID
    /// <typeparam name=""></typeparam> mixed score
    /// <typeparam name=""></typeparam> mixed feedback
    /// <typeparam name=""></typeparam> mixed activityID
    /// <typeparam name=""></typeparam> mixed activityType
    /// <returns></returns> int|null|string|void
    /// </summery>
    public function addFeedback(writerID, score, feedback, activityID, activityType = Feedback.ACTIVITY_TYPE_TRADE){
        global db;

        if(!is_numeric(activityID) || !is_numeric(writerID) || !is_numeric(score) || feedback == ""
        )
            return; // failed

        //If you left feedback already, then return    
        if(this.hasFeedback(writerID, activityID, activityType))
            return; //exists

        param = ["activityID" => activityID, "activityType" => activityType, "writerID" => writerID, "score" => score, "comment" => feedback, "createdDate" => date("Y-m-d H:i:s")];

        otherUserID = null;

        switch(activityType){
            case Feedback.ACTIVITY_TYPE_TRADE:

                tradeIns = new Trade();
                tradeData = tradeIns.getTradeByID(activityID);
                if(!tradeData)
                    return; //no such trade

                if(tradeData["sellerID"] == writerID){

                    otherUserID = param["receiverID"] = tradeData["buyerID"];
                    param["itemID"] = tradeData["buyerItemID"];
                }else if(tradeData["buyerID"] == writerID){
                    otherUserID = param["receiverID"] = tradeData["sellerID"];
                    param["itemID"] = tradeData["sellerItemID"];
                }else{
                    return; //no rights
                }

                break;

            case Feedback.ACTIVITY_TYPE_SHOP:

                shopOrderIns = new ShopOrder();
                orderData = shopOrderIns.getOrderByID(activityID);

                if(!orderData || orderData["buyerID"] != writerID)
                    return; //no such trade

                otherUserID = param["receiverID"] = orderData["sellerID"];
                param["itemID"] = orderData["productID"];

                break;

            default:
                return; //no such cases
                break;
        }

        newID = db.insertFromArray(TABLE_FEEDBACK, param);

        if(newID && otherUserID){
            //Update TradeUser Table for rating calculation
            this._updateRanking(otherUserID);
        }

        //Create notification
        tradeNotificationIns = new TradeNotification();
        tradeNotificationIns.createNotification(otherUserID, writerID, TradeNotification.ACTION_TYPE_FEEDBACK, newID);

        return newID;
    }

    /// <summery>
    /// Get Feedback By ID
    /// 
    /// <typeparam name=""></typeparam> feedbackID
    /// <returns></returns> array|void
    /// </summery>
    public function getFeedbackByID(feedbackID){

        global db;

        if(!is_numeric(feedbackID))
            return;

        query = sprintf("SELECT * FROM %s WHERE feedbackID=%d", TABLE_FEEDBACK, feedbackID);

        data = db.getRow(query);

        return data;

    }

    /// <summery>
    /// Get Feedback By Activity type and id
    /// 
    /// <typeparam name=""></typeparam> mixed activityID
    /// <typeparam name=""></typeparam> mixed activityType
    /// <returns></returns> stdClass
    /// </summery>
    public function getFeedbackByActivityID(activityID, activityType = Feedback.ACTIVITY_TYPE_TRADE){

        global db;

        if(!is_numeric(activityID))
            return;

        query = sprintf("SELECT * FROM %s WHERE activityID=%d AND activityType=%d", TABLE_FEEDBACK, tradeID, activityType);

        data = db.getRow(query);

        return data;

    }

    /// <summery>
    /// Get feedback data by userID
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> string  type
    /// <returns></returns> Indexed|void
    /// </summery>
    public function getFeedbackByUserID(userID, type){

        global db;

        if(!is_numeric(userID))
            return;

        query = sprintf("
                SELECT tFeedback.*, tItem.title AS tradeItemTitle, tShopProd.title AS productTitle, rUser.totalRating AS receiverRating, rUser.positiveRating AS receiverPositiveRating, wUser.totalRating AS writerRating, wUser.positiveRating AS writerPositiveRating 
                FROM %s AS tFeedback 
                    LEFT JOIN %s AS tItem ON tItem.itemID=tFeedback.itemID AND tFeedback.activityType=%d 
                    LEFT JOIN %s AS tShopProd ON tShopProd.productID=tFeedback.itemID AND tFeedback.activityType=%d 
                    LEFT JOIN %s AS rUser ON rUser.userID=tFeedback.receiverID 
                    LEFT JOIN %s AS wUser ON wUser.userID=tFeedback.writerID 
            ", TABLE_FEEDBACK, TABLE_TRADE_ITEMS, Feedback.ACTIVITY_TYPE_TRADE, TABLE_SHOP_PRODUCTS, Feedback.ACTIVITY_TYPE_SHOP, TABLE_USERS_RATING, TABLE_USERS_RATING, userID);

        switch(type){
            case "received":
                whereCond = sprintf("WHERE tFeedback.receiverID=%d ORDER BY tFeedback.feedbackID DESC", userID);
                break;
            case "given":
                whereCond = sprintf("WHERE tFeedback.writerID=%d ORDER BY tFeedback.feedbackID DESC", userID);
                break;
            default:
                return; //something goes wrong.

        }

        query += whereCond;

        feedbackList = db.getResultsArray(query);

        return feedbackList;

    }

    /// <summery>
    /// get Received Feedback Count
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> boolean isPositiveOnly
    /// <returns></returns> int
    /// </summery>
    public function getReceivedFeedbackCount(userID, isPositiveOnly){

        if(!is_numeric(userID)){
            return 0;
        }

        global db;

        if(isPositiveOnly){
            query = sprintf("SELECT count(*) AS ratingCount FROM %s WHERE receiverID=%d AND score > 0", TABLE_FEEDBACK, userID);
            result = db.getRow(query);
        }else{
            query = sprintf("SELECT count(*) AS ratingCount FROM %s WHERE receiverID=%d", TABLE_FEEDBACK, userID);
            result = db.getRow(query);
        }

        if(result)
            return result["ratingCount"];else
            return 0;

    }

    /// <summery>
    /// It will update user feedback ranking when you leave feedback;
    /// </summery>
    public function _updateRanking(userID){

        global db;

        if(!is_numeric(userID))
            return;

        totalFeedback = this.getReceivedFeedbackCount(userID, false);
        positiveFeedback = this.getReceivedFeedbackCount(userID, true);

        param = ["totalRating" => totalFeedback, "positiveRating" => positiveFeedback];

        //if this is first feedback
        ratingVal = this.getUserRating(userID);
        if(ratingVal){
            //update
            db.updateFromArray(TABLE_USERS_RATING, param, ["userID" => userID]);
        }else{
            param["userID"] = userID;
            db.insertFromArray(TABLE_USERS_RATING, param);
        }

        return true;

    }

    /// <summery>
    /// Get user Rating
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> stdClass
    /// </summery>
    public function getUserRating(userID){

        global db;

        if(!is_numeric(userID))
            return;

        query = sprintf("SELECT * FROM %s WHERE userID=%d", TABLE_USERS_RATING, userID);

        data = db.getRow(query);

        return data;
    }

    /// <summery>
    /// Check if the writer writes already.
    /// 
    /// <typeparam name=""></typeparam> mixed writerID
    /// <typeparam name=""></typeparam> mixed activityID
    /// <typeparam name=""></typeparam> mixed activityType
    /// <returns></returns> bool
    /// </summery>
    function hasFeedback(writerID, activityID, activityType){

        global db;

        if(!is_numeric(writerID))
            return false;

        query = sprintf("SELECT * FROM %s WHERE writerID=%d AND activityID=%d AND activityType=%d", TABLE_FEEDBACK, userID, activityID, activityType);

        data = db.getRow(query);

        if(!data){
            return false;
        }else{
            return true;
        }

    }

    /// <summery>
    /// Get Trade Feedbacks
    /// 
    /// <typeparam name=""></typeparam> mixed tradeIDs
    /// <returns></returns> array|Indexed
    /// </summery>
    public function getTradeFeedbackList(tradeIDs){

        global db;

        feedbackList = [];

        if(!is_array(tradeIDs) && tradeIDs){
            tradeIDs = explode(",", tradeIDs);
        }

        if(is_array(tradeIDs) && count(tradeIDs) > 0){
            query = sprintf("SELECT * FROM %s WHERE activityType=%d AND activityID IN (%s)", TABLE_FEEDBACK, Feedback.ACTIVITY_TYPE_TRADE, implode(",", tradeIDs));
            feedbackList = db.getResultsArray(query);
        }

        return feedbackList;

    }

    /// <summery>
    /// Get feedback;
    /// 
    /// <typeparam name=""></typeparam> tradeID
    /// <returns></returns> Indexed
    /// </summery>
    public function getTradeFeedback(tradeID){

        global db;

        result = null;
        if(is_numeric(tradeID)){
            query = sprintf("SELECT * FROM %s WHERE activityType=%d AND activityID=%d", TABLE_FEEDBACK, Feedback.ACTIVITY_TYPE_TRADE, tradeID);
            result = db.getResultsArray(query);
        }
        return result;

    }

}