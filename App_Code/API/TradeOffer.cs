using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for TradeOffer
/// </summary>

/// <summery>
/// Trade offer management
/// </summery>
class TradeOffer {

    const STATUS_OFFER_INACTIVE = 0; // Offer Inactive, When user has been banned, then all pending offers will be changed to inactive, and it won"t be displayed on other"s offer list
    const STATUS_OFFER_ACTIVE = 1; // Offer Active
    const STATUS_OFFER_DECLINED = 2; // Offer Declined

    /// <summery>
    ///  Add Offer
    /// 
    /// <typeparam name=""></typeparam> integer targetItemID
    /// <typeparam name=""></typeparam> integer offeredItemID
    /// <returns></returns> int
    /// </summery>
    public function addOffer(targetItemID, offeredItemID){
        global db;

        if(!is_numeric(targetItemID) || !is_numeric(offeredItemID))
            return;

        //If same offer exists with this item?
        oldOfferID = this.existsSameOffer(targetItemID, offeredItemID);
        if(oldOfferID !== false)
            return oldOfferID;

        //Add new offer
        dateTimeStamp = date("Y-m-d H:i:s");

        data = ["targetItemID" => targetItemID, "offeredItemID" => offeredItemID, "createdDate" => dateTimeStamp, "lastUpdateDate" => dateTimeStamp];

        newID = db.insertFromArray(TABLE_TRADE_OFFERS, data);

        //Create notification 
        if(newID){

            tradeItemIns = new TradeItem();
            targetItemData = tradeItemIns.getItemById(targetItemID);
            offeredItemData = tradeItemIns.getItemById(offeredItemID);

            if(targetItemData){
                tradeNotificationIns = new TradeNotification();
                tradeNotificationIns.createNotification(targetItemData["userID"], offeredItemData["userID"], TradeNotification.ACTION_TYPE_OFFER_RECEIVED, newID);
            }

        }

        return newID;
    }

    /// <summery>
    /// Get offer by ID
    /// 
    /// <typeparam name=""></typeparam> integer offerID
    /// <returns></returns> array|void
    /// </summery>
    public function getOfferByID(offerID){

        global db;

        if(!is_numeric(offerID))
            return;

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_OFFERS + " WHERE offerID=%d", offerID);

        data = db.getRow(query);

        return data;

    }

    /// <summery>
    /// Check if same offer exists
    /// 
    /// <typeparam name=""></typeparam> integer targetItemID
    /// <typeparam name=""></typeparam> integer offeredItemID
    /// <returns></returns> bool
    /// </summery>
    public function existsSameOffer(targetItemID, offeredItemID){

        global db;

        if(!is_numeric(targetItemID) || !is_numeric(offeredItemID))
            return false;

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_OFFERS + " WHERE targetItemID=%d, offeredItemID=%d", targetItemID, offeredItemID);

        data = db.getRow(query);

        if(empty(data))
            return false;else
            return data["offerID"];
    }

    /// <summery>
    /// If the target Item owner declined this person"s offer before, then it will return true. If not, it will return false
    /// 
    /// <typeparam name=""></typeparam> integer targetItemID
    /// <typeparam name=""></typeparam> integer offeredItemID
    /// <typeparam name=""></typeparam> integer userID : if this param set, then offeredItemID will be ignored
    /// <returns></returns> bool
    /// </summery>
    public function checkDeclinedOffer(targetItemID, offeredItemID, userID = null){

        global db;

        if(!is_numeric(targetItemID) || (!is_numeric(offeredItemID) && userID == null))
            return false;

        //Get offer item data
        tradeItemIns = new TradeItem();

        if(userID == null){
            offerItemData = tradeItemIns.getItemById(offeredItemID);
            if(empty(offerItemData))
                return false;

            userID = offerItemData["userID"];

        }

        offerItemList = tradeItemIns.getItemList(userID, null, TradeItem.STATUS_ITEM_ACTIVE);
        offerItemIdList = [];

        if(count(offerItemList) > 0){
            foreach(offerItemList as tmpData){
                offerItemIdList[] = tmpData["itemID"];
            }
        }
        if(count(offerItemIdList) == 0)
            return false;

        if(in_array(targetItemID, offerItemIdList) && in_array(offeredItemID, offerItemIdList)){
            //same account item.
            return false;
        }

        //Run query

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_OFFERS + " WHERE targetItemID=%d AND STATUS=%d AND offeredItemID IN (" + implode(",", offerItemIdList) + ")", targetItemID, TradeOffer.STATUS_OFFER_DECLINED, offeredItemID);

        data = db.getRow(query);

        if(empty(data))
            return false;else
            return true;//exists

    }

    /// <summery>
    /// Get offer list by target Item ID
    /// 
    /// <typeparam name=""></typeparam> integer targetItemID
    /// <returns></returns> bool|Indexed
    /// </summery>
    public function getOffersByTargetItemID(targetItemID){

        global db;

        if(!is_numeric(targetItemID))
            return false;

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_OFFERS + " WHERE targetItemID=%d", targetItemID);

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// Filter the item list which are possible to make offer for this item. In a word, we are going to remove used items for this targetItem.
    /// 
    /// <typeparam name=""></typeparam> integer targetItemID
    /// <typeparam name=""></typeparam> array   itemList
    /// <returns></returns> array
    /// </summery>
    public function getAvailableItemList(targetItemID, itemList){

        offerList = this.getOffersByTargetItemID(targetItemID);

        offeredItemList = [];
        if(count(offerList) > 0){
            foreach(offerList as tmpData){
                offeredItemList[] = tmpData["offeredItemID"];
            }
        }

        if(count(offeredItemList) == 0)
            return itemList;

        newItemList = [];
        foreach(itemList as tmpItemData){
            if(!in_array(tmpItemData["itemID"], offeredItemList))
                newItemList[] = tmpItemData;
        }

        return newItemList;

    }

    /// <summery>
    /// Get offer received
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> Indexed
    /// </summery>
    public function getOfferReceived(userID, targetID = null){
        return this.getOfferByUserID(userID, targetID, "received", TradeOffer.STATUS_OFFER_ACTIVE);
    }

    /// <summery>
    /// Get offer made
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> Indexed
    /// </summery>
    public function getOfferMade(userID){
        return this.getOfferByUserID(userID, null, "made", TradeOffer.STATUS_OFFER_ACTIVE);
    }

    /// <summery>
    /// Get offer declined
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> Indexed
    /// </summery>
    public function getOfferDeclined(userID, declinedByThem = null){

        if(declinedByThem === null){
            return this.getOfferByUserID(userID, null, "declined", TradeOffer.STATUS_OFFER_DECLINED);
        }else if(declinedByThem === true){
            return this.getOfferByUserID(userID, null, "declined_bythem", TradeOffer.STATUS_OFFER_DECLINED);
        }else if(declinedByThem === false){
            return this.getOfferByUserID(userID, null, "declined_byme", TradeOffer.STATUS_OFFER_DECLINED);
        }

    }

    /// <summery>
    /// Get offer by userID
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer targetID : if it is set, then it will be used to get result
    /// <typeparam name=""></typeparam> string  type
    /// <typeparam name=""></typeparam> integer paramStatus
    /// <returns></returns> Indexed
    /// </summery>
    public function getOfferByUserID(userID, targetID, type, paramStatus){

        global db;

        if(!is_numeric(userID) || !is_numeric(paramStatus))
            return;

        //Get this user"s items
        tradeItemIns = new TradeItem();
        itemList = tradeItemIns.getItemList(userID, null, TradeItem.STATUS_ITEM_ACTIVE);

        itemIDList = [];
        if(count(itemList) > 0){
            foreach(itemList as itemData){
                itemIDList[] = itemData["itemID"];
            }
            if(is_numeric(targetID)){
                if(in_array(targetID, itemIDList)){
                    itemIDList = [targetID];
                }else{
                    return;
                }
            }

            query = sprintf("
                    SELECT tOffer.offerID, tOffer.targetItemID, tOffer.offeredItemID, tOffer.createdDate AS offerCreatedDate, tOffer.lastUpdateDate AS offerUpdatedDate, tOffer.status AS offerStatus, 
                        tItem.userID AS targetUserID, tItem.title AS targetTitle, tItem.subtitle AS targetSubtitle, tItem.description AS targetDescription, tItem.images AS targetImages, tItem.createdDate AS targetCreatedDate,tItem.expiryDate AS targetExpiryDate, tItem.locationID AS targetLocationID, 
                        oItem.userID AS offeredUserID, oItem.title AS offeredTitle, oItem.subtitle AS offeredSubtitle, oItem.description AS offeredDescription, oItem.images AS offeredImages, oItem.createdDate AS offeredCreatedDate, oItem.locationID AS offeredLocationID, 
                        oItem.expiryDate AS offeredExpiryDate,
                        LOC.country_title AS offeredLocationTitle, 
                        tUser.totalRating, tUser.positiveRating 
                    FROM %s AS tOffer 
                        LEFT JOIN %s AS tItem ON tItem.itemID = tOffer.targetItemID 
                        LEFT JOIN %s AS oItem ON oItem.itemID = tOffer.offeredItemID 
                        LEFT JOIN %s AS LOC ON LOC.countryID = oItem.locationID                          
            ", TABLE_TRADE_OFFERS, TABLE_TRADE_ITEMS, TABLE_TRADE_ITEMS, TABLE_COUNTRY);

            if(type == "made")
                query += sprintf(" LEFT JOIN %s AS tUser ON tUser.userID = tItem.userID", TABLE_USERS_RATING);else
                query += sprintf(" LEFT JOIN %s AS tUser ON tUser.userID = oItem.userID", TABLE_USERS_RATING);

            switch(type){
                case "received":
                    query = db.prepare(query + " WHERE tOffer.targetItemID IN " + sprintf("(%s)", implode(",", itemIDList)) + " AND tOffer.status=" + paramStatus);
                    break;
                case "made":
                    query = db.prepare(query + " WHERE tOffer.offeredItemID IN " + sprintf("(%s)", implode(",", itemIDList)) + " AND tOffer.status=" + paramStatus);
                    break;
                case "declined":
                    query = db.prepare(query + " WHERE tOffer.offeredItemID IN " + sprintf("(%s)", implode(",", itemIDList)) + " AND tOffer.status=" + paramStatus);
                    break;
                case "declined_bythem":
                    query = db.prepare(query + " WHERE tOffer.offeredItemID IN " + sprintf("(%s)", implode(",", itemIDList)) + " AND tOffer.status=" + paramStatus + " AND tOffer.offeredHideDeclined=0");
                    break;
                case "declined_byme":
                    query = db.prepare(query + " WHERE tOffer.targetItemID IN " + sprintf("(%s)", implode(",", itemIDList)) + " AND tOffer.status=" + paramStatus + " AND tOffer.targetHideDeclined=0");
                    break;
                default:
                    query = db.prepare(query + " WHERE tOffer.status=" + paramStatus);
                    break;
            }

            //items should be valid (active one)
            query += sprintf(" AND tItem.status=%d AND oItem.status=%d", TradeItem.STATUS_ITEM_ACTIVE, TradeItem.STATUS_ITEM_ACTIVE);

            data = db.getResultsArray(query);

            return data;

        }else{
            return;
        }

    }

    /// <summery>
    /// Check if it is acceptable by this user.
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer offerID
    /// <returns></returns> bool
    /// </summery>
    public function isAcceptableOffer(userID, offerID){

        global db;

        if(!is_numeric(userID) || !is_numeric(offerID)){
            return false;
        }

        query = sprintf("
                    SELECT * FROM %s AS tOffer
                        LEFT JOIN %s AS tItem ON tItem.itemID = tOffer.targetItemID 
                        LEFT JOIN %s AS oItem ON oItem.itemID = tOffer.offeredItemID 
                    WHERE tItem.userID=%d AND tOffer.offerID=%d AND tOffer.status=%d AND tItem.status=%d AND oItem.status=%d 
                ", TABLE_TRADE_OFFERS, TABLE_TRADE_ITEMS, TABLE_TRADE_ITEMS, userID, offerID, TradeOffer.STATUS_OFFER_ACTIVE, TradeItem.STATUS_ITEM_ACTIVE, TradeItem.STATUS_ITEM_ACTIVE);

        query = db.prepare(query);

        data = db.getRow(query);

        if(data)
            return true;else
            return false;

    }

    /// <summery>
    /// Accept offer
    /// 
    /// <typeparam name=""></typeparam> integer offerID
    /// <returns></returns> bool|int
    /// </summery>
    public function acceptOffer(offerID){

        if(!is_numeric(offerID))
            return false;

        //Get related info for this offer
        tradeItemIns = new TradeItem();
        offerInfo = this.getOfferByID(offerID);
        if(empty(offerInfo))
            return false;

        sellerData = tradeItemIns.getItemById(offerInfo["targetItemID"]);
        buyerData = tradeItemIns.getItemById(offerInfo["offeredItemID"]);
        if(empty(buyerData) || empty(sellerData))
            return false;

        offerInfo["sellerID"] = sellerData["userID"];
        offerInfo["buyerID"] = buyerData["userID"];

        //Add trade
        tradeIns = new Trade();
        newTradeID = tradeIns.addTrade(offerInfo["sellerID"], offerInfo["buyerID"], offerInfo["targetItemID"], offerInfo["offeredItemID"]);

        //Change trade Item status to "traded"
        tradeItemIns.updateItem(offerInfo["targetItemID"], ["status" => TradeItem.STATUS_ITEM_TRADED]);
        tradeItemIns.updateItem(offerInfo["offeredItemID"], ["status" => TradeItem.STATUS_ITEM_TRADED]);

        //Remove all offered made by these 2 items
        this.deleteOffersByItemID(offerInfo["targetItemID"]);
        this.deleteOffersByItemID(offerInfo["offeredItemID"]);

        //After processing accept (creating record in trade table), then you should remove the offer record from the table
        //this.deleteOffer(offerID); // Meaningless code since it should be deleted by deleteOffersByItemID (2 commands above this)

        //Create notification 
        tradeNotificationIns = new TradeNotification();
        tradeNotificationIns.createNotification(offerInfo["buyerID"], offerInfo["sellerID"], TradeNotification.ACTION_TYPE_OFFER_ACCEPTED, newTradeID);

        return newTradeID;

    }

    /// <summery>
    /// Delete Offer
    /// 
    /// <typeparam name=""></typeparam> integer offerID
    /// </summery>
    public function deleteOffer(offerID){

        global db;

        if(!is_numeric(offerID))
            return;

        query = sprintf("DELETE FROM %s WHERE offerID=%d", TABLE_TRADE_OFFERS, offerID);

        db.query(query); // delete record
    }

    /// <summery>
    /// Delete offer made
    /// 
    /// <typeparam name=""></typeparam> integer offerID
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> bool
    /// </summery>
    public function deleteOfferMade(offerID, userID){

        global db;

        if(!is_numeric(offerID) || !is_numeric(userID))
            return false;

        offerData = this.getOfferByID(offerID);
        if(!offerData)
            return false;

        tradeItemIns = new TradeItem();

        offeredItemData = tradeItemIns.getItemById(offerData["offeredItemID"]);

        if(!offeredItemData)
            return false;

        if(offeredItemData["userID"] == userID){
            this.deleteOffer(offerID);
            return true;
        }else{
            return false;
        }

    }

    /// <summery>
    /// Delete all offers with this item
    /// 
    /// <typeparam name=""></typeparam> integer itemID
    /// </summery>
    public function deleteOffersByItemID(itemID){
        global db;

        if(!is_numeric(itemID))
            return;

        query = sprintf("DELETE FROM %s WHERE targetItemID=%d OR offeredItemID=%d", TABLE_TRADE_OFFERS, itemID, itemID);

        db.query(query); // delete record
    }

    /// <summery>
    /// Decline offer
    /// 
    /// <typeparam name=""></typeparam> integer offerID
    /// </summery>
    public function declineOffer(offerID){
        global db;

        if(!is_numeric(offerID))
            return;

        dateTimeStamp = date("Y-m-d H:i:s");

        query = sprintf("UPDATE %s SET STATUS=%d, lastUpdateDate="%s" WHERE offerID=%d", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_DECLINED, dateTimeStamp, offerID);

        db.query(query);

        //Notification
        tradeItemIns = new TradeItem();
        offerData = this.getOfferByID(offerID);
        if(offerData){

            buyerItemData = tradeItemIns.getItemById(offerData["offeredItemID"]);
            sellerItemData = tradeItemIns.getItemById(offerData["targetItemID"]);

            if(buyerItemData){
                //Create notification 
                tradeNotificationIns = new TradeNotification();
                tradeNotificationIns.createNotification(buyerItemData["userID"], sellerItemData["userID"], TradeNotification.ACTION_TYPE_OFFER_DECLINED, offerID);
            }

        }

    }

    /// <summery>
    /// Remove Declined Offer List
    /// 
    /// <typeparam name=""></typeparam> array   offerIDList
    /// <typeparam name=""></typeparam> integer userID , check if the offers are belonged to this user
    /// <typeparam name=""></typeparam> integer type   , 0: declined by them (my offer declined) 1: I declined the offers which made by others.
    /// <returns></returns> integer, number of records removed
    /// </summery>
    public function removeDeclinedOffers(offerIDList, userID, type = 0){

        global db;

        if(isset(userID)){

            if(!is_numeric(userID))
                return false;
            tradeItemIns = new TradeItem();
            itemList = tradeItemIns.getItemList(userID, null, TradeItem.STATUS_ITEM_ACTIVE);

            itemIDList = [];
            if(count(itemList) > 0){
                foreach(itemList as itemData){
                    itemIDList[] = itemData["itemID"];
                }
            }

            if(count(itemIDList) == 0)
                return false;;

            if(type == 0){
                query = sprintf("SELECT * FROM %s WHERE offeredItemID IN (%s)", TABLE_TRADE_OFFERS, implode(",", itemIDList));
            }else{
                query = sprintf("SELECT * FROM %s WHERE targetItemID IN (%s)", TABLE_TRADE_OFFERS, implode(",", itemIDList));
            }

            thisUserOffers = db.getResultsArray(query);

            if(count(thisUserOffers) == 0)
                return false;;

            thisUserOfferIDList = [];
            foreach(thisUserOffers as offerData){
                thisUserOfferIDList[] = offerData["offerID"];
            }

            newOfferIDList = [];
            foreach(offerIDList as key => offerID){
                if(in_array(offerID, thisUserOfferIDList)){
                    newOfferIDList[] = offerID;
                }
            }

            offerIDList = newOfferIDList;

            //Hide displaying offers removed

            if(count(offerIDList) > 0){

                if(type == 0){
                    query = sprintf("UPDATE %s SET offeredHideDeclined=1 WHERE offerID IN (%s)", TABLE_TRADE_OFFERS, implode(",", offerIDList));
                }else{
                    query = sprintf("UPDATE %s SET targetHideDeclined=1 WHERE offerID IN (%s)", TABLE_TRADE_OFFERS, implode(",", offerIDList));
                }

                db.query(query);

            }
        }

    }

    /// <summery>
    /// Remove related offers with this item
    /// 
    /// <typeparam name=""></typeparam> array /integer itemIDList
    /// </summery>
    public function removeRelatedOffers(itemIDList){

        global db;

        if(is_numeric(itemIDList) && itemIDList > 0){
            itemIDList = [itemIDList];
        }

        if(is_array(itemIDList) && count(itemIDList) > 0){

            idCondStr = implode(",", itemIDList);
            query = sprintf("DELETE FROM %s WHERE targetItemID IN (%s) OR offeredItemID IN (%s)", TABLE_TRADE_OFFERS, idCondStr, idCondStr);
            db.query(query);

        }

        return;

    }

    /// <summery>
    /// Update isNew field
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed status
    /// </summery>
    public function markAsRead(userID, status){

        global db;

        if(!is_numeric(userID))
            return;

        tradeItemIns = new TradeItem();
        userTradeItemList = tradeItemIns.getItemList(userID, null, TradeItem.STATUS_ITEM_ACTIVE);

        if(count(userTradeItemList) > 0){

            idList = [];
            foreach(userTradeItemList as row){
                idList[] = row["itemID"];
            }
            idListStr = implode(",", idList);

            query = sprintf("UPDATE %s SET isNew=0 WHERE targetItemID IN (%s) AND STATUS=%d", TABLE_TRADE_OFFERS, idListStr, status);

            db.query(query);
        }

        return;
    }

    /// <summery>
    /// Get user"s offer count
    /// 
    /// <typeparam name=""></typeparam> mixed   userID
    /// <typeparam name=""></typeparam> integer status
    /// <returns></returns> int|one
    /// </summery>
    public function getNewOfferCount(userID, status = TradeOffer.STATUS_OFFER_ACTIVE){

        global db;

        if(!is_numeric(userID))
            return 0;

        tradeItemIns = new TradeItem();
        userTradeItemList = tradeItemIns.getItemList(userID, null, TradeItem.STATUS_ITEM_ACTIVE);

        if(count(userTradeItemList) > 0){

            idList = [];
            foreach(userTradeItemList as row){
                idList[] = row["itemID"];
            }
            idListStr = implode(",", idList);

            query = sprintf("SELECT count(*) FROM %s WHERE targetItemID IN (%s) AND STATUS=%d AND isNew=1", TABLE_TRADE_OFFERS, idListStr, status);

            return db.getVar(query);
        }

        return 0;

    }

    /// <summery>
    /// Change offer status 1) to Activate 2) to make inactive
    /// It will find all offers related to this user, and change status as the status parameter
    /// This function will be called when banning the user or unbanning the user
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer status (one of STATUS_OFFER_INACTIVE, STATUS_OFFER_ACTIVE)
    /// <returns></returns> bool|void
    /// </summery>
    public function massStatusChange(userID, status = TradeOffer.STATUS_OFFER_INACTIVE){

        global db;

        if(!is_numeric(userID))
            return;

        tradeItemIns = new TradeItem();

        itemList = tradeItemIns.getItemList(userID);

        itemIDList = [];
        if(count(itemList) > 0){
            foreach(itemList as itemData){
                itemIDList[] = itemData["itemID"];
            }
        }

        itemStr = "";
        if(count(itemIDList) > 0){

            itemStr = implode(",", itemIDList);

            if(status == TradeOffer.STATUS_OFFER_INACTIVE){
                //make pending offers to inactive status
                query = sprintf("UPDATE %s SET STATUS=%d WHERE (targetItemID IN (%s) OR offeredItemID IN (%s)) AND STATUS=%d", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_INACTIVE, itemStr, itemStr, TradeOffer.STATUS_OFFER_ACTIVE);

            }else if(status == TradeOffer.STATUS_OFFER_ACTIVE){
                //Make inactive offers to pending status
                query = sprintf("UPDATE %s SET STATUS=%d WHERE (targetItemID IN (%s) OR offeredItemID IN (%s)) AND STATUS=%d", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_ACTIVE, itemStr, itemStr, TradeOffer.STATUS_OFFER_INACTIVE);
            }else{
                //We don"t have this case
                return;
            }

            db.query(query);

        }

        return true;

    }

}