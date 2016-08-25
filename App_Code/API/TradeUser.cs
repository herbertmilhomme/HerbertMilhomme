using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for TradeUser
/// </summary>

/// <summery>
/// Trade User Info Management
/// </summery>
class TradeUser {

    /// <summery>
    /// Get Trade User Info
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> array|void
    /// </summery>
    public function getUserByID(userID){

        global db;

        if(!is_numeric(userID))
            return;

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_USERS + " WHERE userID=%d", userID);
        data = db.getRow(query);

        if(!data){
            this.addUser(userID);
            query = db.prepare("SELECT * FROM " + TABLE_TRADE_USERS + " WHERE userID=%d", userID);
            data = db.getRow(query);
        }

        return data;
    }

    /// <summery>
    /// Check duplication
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> bool|void
    /// </summery>
    public function checkDuplication(userID){
        global db;

        if(!is_numeric(userID))
            return;

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_USERS + " WHERE userID=%d", userID);
        data = db.getRow(query);

        if(data)
            return true;else
            return false;
    }

    /// <summery>
    /// Add Trade user
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> array   data
    /// <returns></returns> int|null|string|void
    /// </summery>
    public function addUser(userID, data = []){

        global db;

        userIns = new User();
        if(!is_numeric(userID) || !userIns.checkUserID(userID, false))
            return;

        if(this.checkDuplication(userID))
            return;

        user_base_info = user.getUserBasicInfo(userID);

        if(!user_base_info){
            return;
        }

        data["userID"] = userID;
        data["shippingFullName"] = trim(user_base_info["firstName"] + " " + user_base_info["lastName"]); //When adding address, put your full name to this record

        newID = db.insertFromArray(TABLE_TRADE_USERS, data);

        return newID;

    }

    /// <summery>
    /// Update shipping Info
    /// It has 2 logic in it. Update your own shipping info, and update already created trade records which has no shipping info.
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> array   data
    /// <returns></returns> bool
    /// </summery>
    public function updateShippingInfo(userID, data){

        if(!is_numeric(userID) || //            data["shippingFullName"] == "" ||
            data["shippingAddress"] == "" || data["shippingCity"] == "" || data["shippingState"] == "" || data["shippingZip"] == "" || data["shippingCountryID"] == "" || !is_numeric(data["shippingCountryID"])
        )
            return false;

        //Update my shipping info
        global db;

        db.updateFromArray(TABLE_TRADE_USERS, data, ["userID" => userID]);

        //Update trade table which has no shipping info with this info.
        //It will check trade table, and create records in trade_shipping_info
        tradeIns = new Trade();
        tradeShippingInfoIns = new TradeShippingInfo();

        //---------------- Update for seller ----------------------//
        requiredList = tradeIns.getShippingInfoRequiredTrade(userID, "seller");
        if(!empty(requiredList) && count(requiredList) > 0){
            foreach(requiredList as tradeData){
                //Add shipping info
                shippingRecID = tradeShippingInfoIns.addTradeShippingInfo(userID);
                if(!empty(shippingRecID) && is_numeric(shippingRecID)){
                    //update trade table
                    tradeIns.updateTrade(tradeData["tradeID"], ["sellerShippingID" => shippingRecID]);
                }

            }
        }

        //---------------- Update for buyer ----------------------//
        requiredList = tradeIns.getShippingInfoRequiredTrade(userID, "buyer");
        if(!empty(requiredList) && count(requiredList) > 0){
            foreach(requiredList as tradeData){
                //Add shipping info
                shippingRecID = tradeShippingInfoIns.addTradeShippingInfo(userID);
                if(!empty(shippingRecID) && is_numeric(shippingRecID)){
                    //update trade table
                    tradeIns.updateTrade(tradeData["tradeID"], ["buyerShippingID" => shippingRecID]);
                }

            }
        }

        //-------------------- Update Buyer Shipping Info -----------------------//
        tradeShippingInfoIns.updateTradeShippingInfo(userID, data);

        return true;

    }

    /// <summery>
    /// Update Trade User
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> array   data
    /// <returns></returns> bool|void
    /// </summery>
    public function updateTradeUser(userID, data){

        global db;

        if(!is_numeric(userID))
            return false;

        res = db.updateFromArray(TABLE_TRADE_USERS, data, ["userID" => userID]);

        return;

    }

    /// <summery>
    /// Get users who are at top, by having items
    /// 
    /// <typeparam name=""></typeparam> integer limit
    /// <returns></returns> Indexed|void
    /// </summery>
    public function getUsersTopByItems(limit = 10){

        if(!is_numeric(limit))
            return;

        global db;

        avaiableTime = date("Y-m-d H:i:s");

        query = sprintf("
                        SELECT tUser.*, user.firstName, user.lastName, (SELECT COUNT(*) FROM %s AS tItem WHERE tUser.userID=tItem.userID AND tItem.expiryDate >= "%s" AND tItem.status=%d) AS itemCount 
                        FROM %s AS tUser 
                            LEFT JOIN %s AS USER ON tUser.userID=USER.userID
                            WHERE USER.status=%d ORDER BY itemCount DESC LIMIT %d
                            
                    ", TABLE_TRADE_ITEMS, avaiableTime, TradeItem.STATUS_ITEM_ACTIVE, TABLE_TRADE_USERS, TABLE_USERS, User.STATUS_USER_ACTIVE, limit);

        result = db.getResultsArray(query);

        return result;

    }

    /// <summery>
    /// Check if you have credits
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> bool
    /// </summery>
    public function hasCredits(userID, minAmount = 1){

        userIns = new User();
        userInfo = userIns.getUserBasicInfo(userID);

        if(!userInfo)
            return false;

        return userInfo["credits"] >= minAmount;

    }

    /// <summery>
    /// Get all users

    /// </summery>
    public function getAllUsers(){

        global db;

        query = sprintf("SELECT * FROM %s", TABLE_TRADE_USERS);

        result = db.getResultsArray(query);

        return result;

    }

    /// <summery>
    /// Use one credits
    /// 
    /// <typeparam name=""></typeparam> integer   userID
    /// <typeparam name=""></typeparam> float|int credits
    /// <returns></returns> float|int
    /// </summery>
    public function useCredit(userID, credits = 1){

        //Update credit activity table (credit has been used)
        transactionIns = new Transaction();
        transactionIns.useCreditsInTrade(userID, credits);

        return credits;

    }

}
