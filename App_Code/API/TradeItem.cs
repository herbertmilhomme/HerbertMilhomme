using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for TradeItem
/// </summary>

class TradeItem {

    const STATUS_ITEM_INACTIVE = 0;     // Item has been inactivated. When user banned, all items (status=new) will be changed to this inactive
    const STATUS_ITEM_ACTIVE = 1;       // Item is available for trade.
    const STATUS_ITEM_TRADED = 2;       // Item has been traded
    const LIST_FEE_PAYMENT_TYPE_CREDIT = 0;
    const LIST_FEE_PAYMENT_TYPE_BTC = 1;

    /// <summery>
    /// Check if you have money or credits
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed paymentType
    /// <returns></returns> bool
    /// </summery>
    public function hasMoneyToListTradeItem(userID, paymentType = TradeItem.LIST_FEE_PAYMENT_TYPE_BTC){

        if(paymentType == TradeItem.LIST_FEE_PAYMENT_TYPE_CREDIT){
            tradeUserIns = new TradeUser();
            return tradeUserIns.hasCredits(userID, TRADE_ITEM_LISTING_FEE_IN_CREDIT);
        }else if(paymentType == TradeItem.LIST_FEE_PAYMENT_TYPE_BTC){
            balance = Bitcoin.getUserWalletBalance(userID);
            return balance >= TRADE_ITEM_LISTING_FEE_IN_BTC;

        }else{
            return false;
        }

    }

    /// <summery>
    /// Make payment for listing trade items.
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed itemID
    /// <typeparam name=""></typeparam> mixed paymentType
    /// <returns></returns> bool|int|null|string|void
    /// </summery>
    public function payListingFee(userID, itemID, paymentType = TradeItem.LIST_FEE_PAYMENT_TYPE_BTC){

        flag = false;

        if(paymentType == TradeItem.LIST_FEE_PAYMENT_TYPE_CREDIT){
            transactionIns = new Transaction();
            flag = transactionIns.useCreditsInTrade(userID, TRADE_ITEM_LISTING_FEE_IN_CREDIT);

        }else if(paymentType == TradeItem.LIST_FEE_PAYMENT_TYPE_BTC){

            flag = Bitcoin.sendBitcoin(userID, TRADE_LISTING_FEE_RECEIVER_BITCOIN_ADDRESS, TRADE_ITEM_LISTING_FEE_IN_BTC);
            get_messages(); // this will flash the messages

            if(flag){
                //Create bitcoin transaction
                BitcoinTransaction.addTransaction(BitcoinTransaction.BITCOIN_RECEIVER_ID, userID, BitcoinTransaction.ACTIVITY_TYPE_LISTING_TRADE_ITEM, itemID, TRADE_ITEM_LISTING_FEE_IN_BTC);
            }

        }

        return flag;
    }

    /// <summery>
    /// Add Trade Item
    /// 
    /// <typeparam name=""></typeparam> array data
    /// <returns></returns> int|null|string|void
    /// </summery>
    public function addItem(data, paymentType = TradeItem.LIST_FEE_PAYMENT_TYPE_BTC){

        tradeUserIns = new TradeUser();

        /* FreeTradeListings - uncomment to enable listing fees
        if (!this.hasMoneyToListTradeItem(data["userID"], paymentType)) {
            //You don"t have money to list this product
            return;
        }
       */

        global db;

        if(empty(data["userID"]) || empty(data["title"]) || empty(data["subtitle"]) || empty(data["catID"])
        )
            return;

        newID = db.insertFromArray(TABLE_TRADE_ITEMS, data);

        //Trade User has been created?
        tradeUserIns.addUser(data["userID"]);

        /* FreeTradeListings - uncomment to enable listing fees
        //Use one credits
        if (newID) {
            //tradeUserIns.useCredit(data["userID"]);
            
            flag = this.payListingFee(data["userID"], newID, paymentType);
            
            if (!flag) {
                this.removeItems(newID);
                return; //failed since we can"t charge you.
            }
            
        }
       */

        return newID;
    }

    /// <summery>
    /// get Item List Of Personal User"s
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> bool    isExpired
    /// <typeparam name=""></typeparam> integer status
    /// <typeparam name=""></typeparam> integer catID
    /// <typeparam name=""></typeparam> string  searchStr
    /// <typeparam name=""></typeparam> string  sortField
    /// <typeparam name=""></typeparam> string  sortDir
    /// <returns></returns> Array
    /// </summery>
    public function getItemList(userID = null, isExpired = null, status = null, catID = null, searchStr = null, sortField = "title", sortDir = "ASC"){
        global db;

        whereCondList = [];
        if(isset(userID)){
            whereCondList[] = "i.userID=" + userID;
        }

        if(isset(catID)){
            whereCondList[] = "i.catID=" + catID;
        }

        if(isset(searchStr) && searchStr != ""){
            searchStr = addslashes(searchStr);
            whereCondList[] = sprintf(" MATCH (i.title, i.subtitle, i.description) AGAINST ("%s" IN BOOLEAN MODE)", searchStr);
        }

        avaiableTime = date("Y-m-d H:i:s");
        if(isExpired === false){

            whereCondList[] = "i.expiryDate >="" + avaiableTime + """;
        }else if(isExpired === true){

            whereCondList[] = "i.expiryDate <"" + avaiableTime + """;
        }

        if(isset(status)){
            whereCondList[] = "i.status=" + status;
        }

        if(count(whereCondList) > 0)
            whereCond = " WHERE " + implode(" AND ", whereCondList);else
            whereCond = " WHERE 1 ";

        whereCond += " GROUP BY i.itemID ";

        if(isset(sortField)){
            whereCond += sprintf(" ORDER BY %s %s", sortField, sortDir);
        }

        query = sprintf("SELECT i.*, (SELECT COUNT(*) FROM %s AS tOffer WHERE i.itemID=tOffer.targetItemID AND tOffer.status=%d) AS offer FROM %s AS i ", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_ACTIVE, TABLE_TRADE_ITEMS);

        query = db.prepare(query + whereCond);

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// Return item data by ID
    /// 
    /// <typeparam name=""></typeparam> mixed itemID
    /// <returns></returns> array
    /// </summery>
    public function getItemById(itemID, isExpired = null){

        if(empty(itemID) || !is_numeric(itemID) || itemID <= 0)
            return null;

        global db;

        availableQueryStr = "";

        avaiableTime = date("Y-m-d H:i:s");
        if(isExpired === false){
            availableQueryStr = " AND i.expiryDate >="" + avaiableTime + "" AND i.status=" + TradeItem.STATUS_ITEM_ACTIVE;
        }else if(isExpired === true){
            availableQueryStr = " AND i.expiryDate <"" + avaiableTime + "" AND i.status=" + TradeItem.STATUS_ITEM_ACTIVE;
        }

        query = sprintf("SELECT i.*, r.reportID,
                (SELECT COUNT(*) FROM %s AS tOffer WHERE i.itemID=tOffer.targetItemID AND tOffer.status=%d) AS offer, 
                tu.totalRating, tu.positiveRating 
                FROM %s AS i 
                    LEFT JOIN %s AS r ON r.objectID=i.itemID AND r.objectType="trade_item"
                    LEFT JOIN %s AS tu ON i.userID=tu.userID", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_ACTIVE, TABLE_TRADE_ITEMS, TABLE_REPORTS, TABLE_USERS_RATING);

        query += db.prepare(" WHERE i.itemID=%d ", itemID) + availableQueryStr + " GROUP BY i.itemID ";

        data = db.getRow(query);

        if(data){
            if(strtotime(avaiableTime) < strtotime(data["createdDate"]))
                data["isExpired"] = false;else
                data["isExpired"] = true;
        }

        return data;

    }

    /// <summery>
    /// Update Item data
    /// 
    /// <typeparam name=""></typeparam> integer itemID
    /// <typeparam name=""></typeparam> array   data
    /// </summery>
    public function updateItem(itemID, data){

        global db;

        res = db.updateFromArray(TABLE_TRADE_ITEMS, data, ["itemID" => itemID]);

        return;

    }

    /// <summery>
    /// Search items
    /// 
    /// <typeparam name=""></typeparam> string qStr   : Query String
    /// <typeparam name=""></typeparam> string catStr : Category Name/ Category ID
    /// <typeparam name=""></typeparam> string locStr : Location / Location ID
    /// <returns></returns> array
    /// </summery>
    public function search(qStr, catStr, locStr, userID){

        global db;

        tradeCatIns = new TradeCategory();
        tradeLocationIns = new Country();

        //Get category data
        catData = null;
        if(is_numeric(catStr))
            catData = tradeCatIns.getCategoryByID(catStr);else
            catData = tradeCatIns.getCategoryByName(catStr);

        //Get Location data
        locationData = null;
        if(is_numeric(locStr))
            locationData = tradeLocationIns.getCountryById(locStr);else
            locationData = tradeLocationIns.getCountryByName(locStr);

        //Make Where condition
        whereCondList = [];

        if(isset(qStr) && qStr != ""){
            //            qStr = addslashes(qStr);
            whereCondList[] = db.prepare(" MATCH (i.title, i.subtitle, i.description) AGAINST (%s IN BOOLEAN MODE)", qStr);
        }

        if(isset(catData)){
            whereCondList[] = "i.catID=" + catData["catID"];
        }else if(catStr != ""){
            return null;
        }

        if(isset(locationData)){
            whereCondList[] = "i.locationID=" + locationData["countryID"];
        }

        if(isset(userID) && is_numeric(userID)){
            whereCondList[] = db.prepare("i.userID=%d", userID);
        }

        //Valid items
        avaiableTime = date("Y-m-d H:i:s");
        whereCondList[] = "i.expiryDate >="" + avaiableTime + """;

        whereCondList[] = "i.status=" + TradeItem.STATUS_ITEM_ACTIVE;

        whereCond = " WHERE " + implode(" AND ", whereCondList);

        whereCond += " GROUP BY i.itemID ";

        query = sprintf("SELECT i.*, (SELECT COUNT(*) FROM %s AS o WHERE i.itemID=o.targetItemID AND o.status=%d) AS offer, u.firstName, u.lastName, tu.totalRating, tu.positiveRating 
                            FROM %s AS i 
                            LEFT JOIN %s AS tu ON i.userID=tu.userID 
                            LEFT JOIN %s AS u ON i.userID=u.userID 
                            ", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_ACTIVE, TABLE_TRADE_ITEMS, TABLE_USERS_RATING, TABLE_USERS);

        query = query + whereCond;

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// Remove Trade Items
    /// This function will be used by Super admin. Others will use removeItemByUserID
    /// 
    /// <typeparam name=""></typeparam> integer /array itemIDList
    /// </summery>
    public function removeItems(itemIDList){
        global db;

        if(is_numeric(itemIDList) && itemIDList > 0){
            itemIDList = [itemIDList];
        }

        if(is_array(itemIDList) && count(itemIDList) > 0){
            idCondStr = implode(",", itemIDList);

            query = sprintf("SELECT * FROM %s WHERE itemID IN (%s) AND STATUS=%d", TABLE_TRADE_ITEMS, idCondStr, TradeItem.STATUS_ITEM_ACTIVE);
            itemList = db.getResultsArray(query);

            if(count(itemList) > 0){

                //remove item images first
                foreach(itemList as itemData){
                    if(itemData["images"] != ""){
                        imageList = explode("|", itemData["images"]);

                        if(count(imageList) > 0){
                            foreach(imageList as key => val){
                                if(val != ""){
                                    val = ltrim(val, "/");
                                    thumb = fn_get_item_first_image_thumb(val);

                                    @unlink(DIR_FS_ROOT + val);
                                    @unlink(DIR_FS_ROOT + thumb);
                                }
                            }
                        }

                    }
                }

                //Delete items
                query = sprintf("DELETE FROM %s WHERE itemID IN (%s) AND STATUS=%d", TABLE_TRADE_ITEMS, idCondStr, TradeItem.STATUS_ITEM_ACTIVE);
                db.query(query);
            }

        }

        return;
    }

    /// <summery>
    /// Remove item id by userID & itemID : the Item should be belonged to the user
    /// 
    /// <typeparam name=""></typeparam> integer itemID
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> bool|void
    /// </summery>
    public function removeItemByUserID(itemID, userID){

        global db;

        if(is_numeric(userID) && is_numeric(itemID)){

            //Check if this item is new (not traded). If it has been traded already, then it couldn"t be deleted
            itemData = this.getItemById(itemID);

            if(itemData["status"] == TradeItem.STATUS_ITEM_ACTIVE && itemData["userID"] == userID){
                this.removeItems([itemID]);

                //After deleting the items, it will remove related offers which are related to this item.

                tradeOfferIns = new TradeOffer();
                tradeOfferIns.removeRelatedOffers(itemID);

                add_message("An item has been removed successfully.");
                return true;

            }

        }

        add_message("Something goes wrong. Please contact customer support!");

        return;

    }

    /// <summery>
    /// Sort Items
    /// 
    /// <typeparam name=""></typeparam> array itemList
    /// <returns></returns> array
    /// </summery>
    public function sortItems(itemList, sortMod){

        if(!is_array(itemList) || count(itemList) == 0){
            return [];
        }

        nowTimeVal = time();
        foreach(itemList as &tmpItem){
            tmpItem["leftSec"] = strtotime(tmpItem["expiryDate"]) - nowTimeVal;
        }

        switch(sortMod){

            case "endsoon" :
                usort(itemList, [this, "_compareEndSoonFirst"]);
                break;

            case "newly" :
                usort(itemList, [this, "_compareEndSoonLast"]);
                break;

            case "offersmost" :
                usort(itemList, [this, "_compareOfferMostFirst"]);

                break;

            case "offersleast" :
                usort(itemList, [this, "_compareOfferMostLast"]);

                break;

            case "best" :
            default:
                //already sorted
                break;

        }

        return itemList;

    }

    /// <summery>
    /// <typeparam name=""></typeparam> a
    /// <typeparam name=""></typeparam> b
    /// <returns></returns> int
    /// </summery>
    private function _compareEndSoonFirst(a, b){
        if(a["leftSec"] == b["leftSec"])
            return 0;

        return (a["leftSec"] > b["leftSec"]) ? 1 : -1;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> a
    /// <typeparam name=""></typeparam> b
    /// <returns></returns> int
    /// </summery>
    private function _compareEndSoonLast(a, b){
        if(a["leftSec"] == b["leftSec"])
            return 0;

        return (a["leftSec"] < b["leftSec"]) ? 1 : -1;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> a
    /// <typeparam name=""></typeparam> b
    /// <returns></returns> int
    /// </summery>
    private function _compareOfferMostFirst(a, b){
        if(a["offer"] == b["offer"])
            return 0;

        return (a["offer"] < b["offer"]) ? 1 : -1;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> a
    /// <typeparam name=""></typeparam> b
    /// <returns></returns> int
    /// </summery>
    private function _compareOfferMostLast(a, b){
        if(a["offer"] == b["offer"])
            return 0;

        return (a["offer"] > b["offer"]) ? 1 : -1;
    }

    /// <summery>
    /// Count items according to the category
    /// 
    /// <typeparam name=""></typeparam> array itemList
    /// <returns></returns> stdClass
    /// </summery>
    public function countItemInCategory(itemList){

        tradeCatIns = new TradeCategory();
        categoryList = tradeCatIns.getCategoryList();

        catItemCountList = [];
        if(count(itemList) > 0){

            foreach(itemList as itemData){
                if(isset(catItemCountList[itemData["catID"]])){
                    catItemCountList[itemData["catID"]]++;
                }else{
                    catItemCountList[itemData["catID"]] = 1;
                }
            }
        }

        if(count(catItemCountList) > 0 && count(categoryList) > 0){
            foreach(categoryList as &tmpCatData){
                isset(catItemCountList[tmpCatData["catID"]]) ? tmpCatData["count"] = catItemCountList[tmpCatData["catID"]] : tmpCatData["count"] = 0;
            }
        }

        return categoryList;
    }

    /// <summery>
    /// Most wanted item list by offer received
    /// 
    /// <typeparam name=""></typeparam> integer limit
    /// <returns></returns> Indexed|void
    /// </summery>
    public function getItemsTopByOffers(limit = 10){

        if(!is_numeric(limit))
            return;

        global db;

        avaiableTime = date("Y-m-d H:i:s");

        query = sprintf("
                        SELECT tItem.*, user.firstName, user.lastName, (SELECT COUNT(*) FROM %s AS tOffer WHERE tOffer.targetItemID=tItem.itemID AND tOffer.status=%d) AS offerCount, r.reportID
                        FROM %s AS tItem 
                            LEFT JOIN %s AS USER ON tItem.userID=USER.userID
                            LEFT JOIN %s AS r ON tItem.itemID=r.objectID AND r.objectType="trade_item"
                            WHERE tItem.status=%d AND tItem.expiryDate >="%s" ORDER BY offerCount DESC LIMIT %d 
                            
                    ", TABLE_TRADE_OFFERS, TradeOffer.STATUS_OFFER_ACTIVE, TABLE_TRADE_ITEMS, TABLE_USERS, TABLE_REPORTS, TradeItem.STATUS_ITEM_ACTIVE, avaiableTime, limit);

        result = db.getResultsArray(query);

        return result;
    }

    /// <summery>
    /// Get recent 10 items
    /// 
    /// <typeparam name=""></typeparam> integer limit
    /// <returns></returns> Indexed|void
    /// </summery>
    public function getRecentItems(limit = 10){

        if(!is_numeric(limit))
            return;

        global db;

        avaiableTime = date("Y-m-d H:i:s");

        query = sprintf("
                        SELECT tItem.*, user.firstName, user.lastName , r.reportID
                        FROM %s AS tItem 
                            LEFT JOIN %s AS USER ON tItem.userID=USER.userID
                            LEFT JOIN %s AS r ON tItem.itemID=r.objectID AND r.objectType="trade_item"
                            WHERE tItem.status=%d AND tItem.expiryDate >="%s" ORDER BY tItem.createdDate DESC LIMIT %d 
                            
                    ", TABLE_TRADE_ITEMS, TABLE_USERS, TABLE_REPORTS, TradeItem.STATUS_ITEM_ACTIVE, avaiableTime, limit);

        result = db.getResultsArray(query);

        return result;
    }

    /// <summery>
    /// Remove expired items & related trade offers

    /// </summery>
    public function removeExpiredItems(){

        global db;

        limitDate = date("Y-m-d H:i:s");

        query = sprintf("SELECT itemID FROM %s WHERE STATUS=%d AND expiryDate < "%s"", TABLE_TRADE_ITEMS, TradeItem.STATUS_ITEM_ACTIVE, limitDate);

        oldItemList = db.getResultsArray(query);
        idList = [];

        if(count(oldItemList) > 0){

            foreach(oldItemList as data){
                idList[] = data["itemID"];
            }

        }

        if(count(idList) > 0){

            //Remove items
            //this.removeItems(idList);

            //Remove related trade offers which made with this item
            tradeOfferIns = new TradeOffer();
            tradeOfferIns.removeRelatedOffers(idList);

        }

        return;

    }

    /// <summery>
    /// Delete whole items and related offers when deleting user
    /// Please note that we will delete items which has not been traded yet.

    /// </summery>
    public function deleteItemsByUserID(userID){

        global db;

        if(!is_numeric(userID))
            return;

        query = sprintf("SELECT itemID FROM %s WHERE STATUS!=%d AND userID=%d", TABLE_TRADE_ITEMS, TradeItem.STATUS_ITEM_TRADED, userID);

        oldItemList = db.getResultsArray(query);
        idList = [];

        if(count(oldItemList) > 0){

            foreach(oldItemList as data){
                idList[] = data["itemID"];
            }
        }

        if(count(idList) > 0){

            //Delete items
            this.removeItems(idList);

            //Remove related trade offers which made with this item
            tradeOfferIns = new TradeOffer();
            tradeOfferIns.removeRelatedOffers(idList);

        }

        return;

    }

    /// <summery>
    /// Change item status 1) to Activate 2) to make inactive
    /// It will find all items belonged to this user, and change status as the status parameter
    /// This function will be called when banning the user or unbanning the user
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer status : value will be one of (STATUS_ITEM_INACTIVE, STATUS_ITEM_ACTIVE)
    /// <returns></returns> bool|void
    /// </summery>
    public function massStatusChange(userID, status = TradeItem.STATUS_ITEM_INACTIVE){

        global db;

        if(!is_numeric(userID))
            return;

        query = "";
        if(status == TradeItem.STATUS_ITEM_INACTIVE){

            // To make inactive from active
            query = sprintf("UPDATE %s SET STATUS=%d WHERE STATUS=%d AND userID=%d", TABLE_TRADE_ITEMS, TradeItem.STATUS_ITEM_INACTIVE, TradeItem.STATUS_ITEM_ACTIVE, userID);
        }else if(status == TradeItem.STATUS_ITEM_ACTIVE){

            // To make active from inactive
            query = sprintf("UPDATE %s SET STATUS=%d WHERE STATUS=%d AND userID=%d", TABLE_TRADE_ITEMS, TradeItem.STATUS_ITEM_ACTIVE, TradeItem.STATUS_ITEM_INACTIVE, userID);
        }else{
            //Error
            return;
        }

        db.query(query);

        return true;

    }

}