using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for ShopOrder
/// </summary>

/// <summery>
/// Shop Order management
/// </summery>
class ShopOrder {

    const STATUS_SOLD = 1;

    const ORDER_READ = 1;
    const ORDER_NEW = 0;

    const ORDER_NOT_ARCHIVED = 0;
    const ORDER_ARCHIVED = 1;

    const NOTIFICATION_OBJECT_TYPE = "shop";
    const NOTIFICATION_ACTIVITY_TYPE_SOLD = "sold";

    /// <summery>
    /// Get Order by ID
    /// 
    /// <typeparam name=""></typeparam> mixed orderID
    /// <typeparam name=""></typeparam> mixed status
    /// <returns></returns> stdClass
    /// </summery>
    public function getOrderByID(orderID, status = ShopOrder.STATUS_SOLD){

        global db;

        if(!is_numeric(orderID))
            return;

        query = sprintf("SELECT * FROM %s WHERE orderID=%d AND STATUS=%d", TABLE_SHOP_ORDERS, orderID, status);

        data = db.getRow(query);

        return data;

    }

    /// <summery>
    /// <typeparam name=""></typeparam> orderID
    /// <typeparam name=""></typeparam> data
    /// </summery>
    public function updateOrder(orderID, data){

        global db;

        if(isset(data["price"]))
            data["price"] = fn_get_btc_price_formated(data["price"]);

        res = db.updateFromArray(TABLE_SHOP_ORDERS, data, ["orderID" => orderID]);

        return;

    }

    /// <summery>
    /// Create Order by array
    /// 
    /// <typeparam name=""></typeparam> mixed data
    /// <returns></returns> bool|int|null|string
    /// </summery>
    public function createOrder(data){

        global db;

        newID = db.insertFromArray(TABLE_SHOP_ORDERS, data);

        if(newID){

            //Create bitcoin transaction
            BitcoinTransaction.addTransaction(data["sellerID"], data["buyerID"], BitcoinTransaction.ACTIVITY_TYPE_PRODUCT_PURCHASE, newID, data["totalPrice"]);

            shopProdIns = new ShopProduct();

            product = shopProdIns.getProductById(data["productID"]);

            if(!product["isDownloadable"])
                shopProdIns.updateProduct(data["productID"], ["status" => ShopProduct.STATUS_SOLD]);

            //Send notification if the seller wants to get notification

            notificationIns = new ShopNotification();
            notificationIns.createNotification(data["sellerID"], data["buyerID"], ShopNotification.ACTION_TYPE_PRODUCT_SOLD, newID);

            return newID;
        }

        return false;

    }

    /// <summery>
    /// make payment
    /// 
    /// <typeparam name=""></typeparam> mixed buyerID
    /// <typeparam name=""></typeparam> mixed sellerID
    /// <typeparam name=""></typeparam> mixed amount
    /// </summery>
    public function makePayment(buyerID, sellerID, amount){

        sellerBitcoinInfo = User.getUserBitcoinInfo(sellerID);

        if(amount <= 0 || !sellerBitcoinInfo){
            return false; //no payment
        }

        flag = Bitcoin.sendBitcoin(buyerID, sellerBitcoinInfo["bitcoin_address"], amount);
        get_messages(); // this will flash the messages

        return flag;

    }

    /// <summery>
    /// Get sold product count, not read, new one
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// </summery>
    public function getNewSoldItemCount(userID){

        global db;

        query = sprintf("SELECT COUNT(*) AS count FROM %s WHERE sellerID=%d AND isRead=%d", TABLE_SHOP_ORDERS, userID, ShopOrder.ORDER_NEW);

        result = db.getRow(query);

        return result["count"];

    }

    /// <summery>
    /// Get purchased order history
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> Array
    /// </summery>
    public function getPurchased(userID, isArchived = ShopOrder.ORDER_NOT_ARCHIVED){

        global db;

        if(!is_numeric(userID)){
            return null;
        }

        archivedStr = "";

        if(isArchived !== null){
            archivedStr = " AND o.isArchived=" + isArchived + " ";
        }

        query = sprintf("SELECT o.*, p.title, p.price, p.subtitle, p.images, f.score, p.isDownloadable
            FROM %s AS o 
             LEFT JOIN %s AS p ON p.productID=o.productID 
             LEFT JOIN %s AS f ON f.activityID=o.orderID AND f.activityType=%d 
             WHERE o.buyerID=%d AND o.status=%d %s ORDER BY o.createdDate DESC", TABLE_SHOP_ORDERS, TABLE_SHOP_PRODUCTS, TABLE_FEEDBACK, Feedback.ACTIVITY_TYPE_SHOP, userID, ShopOrder.STATUS_SOLD, archivedStr);

        return db.getResultsArray(query);

    }

    /// <summery>
    /// Get sold history
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> Array
    /// </summery>
    public function getSold(userID){

        global db;

        if(!is_numeric(userID)){
            return null;
        }

        query = sprintf("SELECT o.*, p.title, p.price, p.subtitle, p.images, f.score, p.isDownloadable,
                            CONCAT(u.firstName, " ", u.lastName) AS fullName,
                            tu.shippingAddress AS address,
                            tu.shippingAddress2 AS address2,
                            tu.shippingCity AS city,
                            tu.shippingState AS state,
                            tu.shippingZip AS zip,
                            tu.shippingCountryID AS countryID
            FROM %s AS o 
            LEFT JOIN %s AS tu ON tu.userID=o.buyerID
            LEFT JOIN %s AS u ON u.userID=o.buyerID
            LEFT JOIN %s AS p ON p.productID=o.productID 
            LEFT JOIN %s AS f ON f.activityID=o.orderID AND f.activityType=%d 
            WHERE o.sellerID=%d AND o.status=%d ORDER BY o.createdDate DESC", TABLE_SHOP_ORDERS, TABLE_TRADE_USERS, TABLE_USERS, TABLE_SHOP_PRODUCTS, TABLE_FEEDBACK, Feedback.ACTIVITY_TYPE_SHOP, userID, ShopOrder.STATUS_SOLD);

        return db.getResultsArray(query);

    }

    /// <summery>
    /// Archive order with order ID
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed orderID
    /// </summery>
    public function archiveOrder(userID, orderID){

        orderData = this.getOrderByID(orderID);

        if(orderData){
            if(orderData["buyerID"] == userID){

                data = ["isArchived" => ShopOrder.ORDER_ARCHIVED];
                this.updateOrder(orderID, data);

                return true;
            }
        }

        return false;

    }

    /// <summery>
    /// Update the sold item info as read
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// </summery>
    public function updateSoldAsRead(userID){
        global db;

        query = sprintf("UPDATE %s SET isRead=%d WHERE sellerID=%d", TABLE_SHOP_ORDERS, ShopOrder.ORDER_READ, userID);

        db.query(query);

    }

    /// <summery>
    /// Create shipping info
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// </summery>
    public function createShippingInfo(userID){

        global db;

        newID = null;

        shippingInfoIns = new TradeUser();
        myShippingData = shippingInfoIns.getUserByID(userID);

        if(!myShippingData){
            return;
        }

        param = [//            "fullName" => myShippingData["shippingFullName"],
            "address" => myShippingData["shippingAddress"], "address2" => myShippingData["shippingAddress2"], "city" => myShippingData["shippingCity"], "state" => myShippingData["shippingState"], "zip" => myShippingData["shippingZip"], "countryID" => myShippingData["shippingCountryID"]];

        newID = db.insertFromArray(TABLE_SHOP_ORDERS_SHIPPING, param);

        return newID;

    }

}