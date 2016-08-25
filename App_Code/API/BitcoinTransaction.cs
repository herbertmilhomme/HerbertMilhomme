using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BitcoinTransaction
/// </summary>

/// <summery>
///  Bitcoin Transaction
/// </summery>
class BitcoinTransaction {

    const ACTIVITY_TYPE_LISTING_PRODUCT = 1;
    const ACTIVITY_TYPE_PRODUCT_PURCHASE = 2;
    const ACTIVITY_TYPE_LISTING_TRADE_ITEM = 3;

    const STATUS_PENDING = 0;
    const STATUS_ACTIVE = 1;

    const BITCOIN_RECEIVER_ID = 0;

    /// <summery>
    /// Add transaction of bitcoin
    /// 
    /// <typeparam name=""></typeparam> mixed receiverID
    /// <typeparam name=""></typeparam> mixed payerID
    /// <typeparam name=""></typeparam> mixed activityType
    /// <typeparam name=""></typeparam> mixed activityID
    /// <typeparam name=""></typeparam> mixed amount
    /// <returns></returns> int|null|string|void
    /// </summery>
    public static function addTransaction(receiverID, payerID, activityType, activityID, amount){
        global db;

        if(!is_numeric(receiverID) || !is_numeric(payerID) || !is_numeric(activityType) || !is_numeric(activityID) || !is_numeric(amount) || amount < 0
        )
            return; // failed

        param = ["receiverID" => receiverID, "payerID" => payerID, "activityType" => activityType, "activityID" => activityID, "amount" => amount, "createdDate" => date("Y-m-d H:i:s"), "status" => BitcoinTransaction.STATUS_ACTIVE];

        newID = db.insertFromArray(TABLE_BITCOIN_TRANSACTION, param);

        return newID;
    }

}