﻿using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for TradeShippingInfo
/// </summary>

/// <summery>
/// Trade Shipping info management
/// </summery>
class TradeShippingInfo {

    /// <summery>
    ///  Add trade shipping info
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> int
    /// </summery>
    public function addTradeShippingInfo(userID){
        global db;

        if(!is_numeric(userID))
            return; // failed

        //Get shipping info from trade_user table
        query = sprintf("SELECT tradeUser.* FROM %s AS tradeUser WHERE tradeUser.userID=%d", TABLE_TRADE_USERS, userID);
        query = db.prepare(query);
        data = db.getRow(query);

        if(!empty(data) && data["shippingCountryID"] > 0){
            //it means there are shipping info

            param = [//                    "fullName"=>data["shippingFullName"],
                "address" => data["shippingAddress"], "address2" => data["shippingAddress2"], "city" => data["shippingCity"], "state" => data["shippingState"], "zip" => data["shippingZip"], "countryID" => data["shippingCountryID"],];

            newID = db.insertFromArray(TABLE_TRADE_SHIPPING_INFO, param);

            return newID;

        }

        return; //failed

    }

    /// <summery>
    /// Update trade shipping information by user id
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// </summery>
    public function updateTradeShippingInfo(userID, data){
        global db;

        query = db.prepare("UPDATE " + TABLE_TRADE_SHIPPING_INFO + " AS tsi " + "LEFT JOIN " + TABLE_TRADE + " AS t ON tsi.shippingID=t.sellerShippingID " + "SET address=%s, address2=%s, city=%s, state=%s, countryID=%d, zip=%s " + "WHERE t.sellerID=%d", data["shippingAddress"], data["shippingAddress2"], data["shippingCity"], data["shippingState"], data["shippingCountryID"], data["shippingZip"], userID);

        db.query(query);

        query = db.prepare("UPDATE " + TABLE_TRADE_SHIPPING_INFO + " AS tsi " + "LEFT JOIN " + TABLE_TRADE + " AS t ON tsi.shippingID=t.buyerShippingID " + "SET address=%s, address2=%s, city=%s, state=%s, countryID=%d, zip=%s " + "WHERE t.buyerID=%d", data["shippingAddress"], data["shippingAddress2"], data["shippingCity"], data["shippingState"], data["shippingCountryID"], data["shippingZip"], userID);

        db.query(query);
    }

}