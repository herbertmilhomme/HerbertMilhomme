using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for TradeCategory
/// </summary>

class TradeCategory {

    /// <summery>
    /// Add category
    /// 
    /// <typeparam name=""></typeparam> mixed name
    /// <typeparam name=""></typeparam> mixed parentID
    /// <typeparam name=""></typeparam> mixed status
    /// <returns></returns> string
    /// </summery>
    public function addCategory(name, parentID = 0, status = 1){
        global db;

        name = trim(name);

        if(empty(name))
            return;

        newID = db.insertFromArray(TABLE_TRADE_CATEGORIES, ["name" => name, "parentID" => parentID, "status" => status]);

        return newID;
    }

    /// <summery>
    /// Get categories
    /// 
    /// <typeparam name=""></typeparam> mixed parentID
    /// <typeparam name=""></typeparam> mixed status
    /// <returns></returns> stdClass
    /// </summery>
    public function getCategoryList(parentID = null, status = 1){
        global db;

        whereCond = "";
        if(isset(parentID)){
            whereCond += " WHERE parentID=" + parentID;
        }

        if(isset(status)){
            if(whereCond != ""){
                whereCond += " AND ";
            }else{
                whereCond += " WHERE ";
            }

            whereCond += "status=" + status;
        }

        query = db.prepare("SELECT * FROM " + TABLE_TRADE_CATEGORIES + whereCond + " ORDER BY NAME");

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// Get Category By Name
    /// 
    /// <typeparam name=""></typeparam> String  catName
    /// <typeparam name=""></typeparam> integer status
    /// <returns></returns> stdClass
    /// </summery>
    public function getCategoryByName(catName, status = 1){
        global db;

        if(catName == "")
            return;
        if(isset(status))
            query = db.prepare("SELECT * FROM " + TABLE_TRADE_CATEGORIES + " WHERE lower(NAME)=lower(%s) AND STATUS=%d", catName, status);else
            query = db.prepare("SELECT * FROM " + TABLE_TRADE_CATEGORIES + " WHERE lower(NAME)=lower(%s)", catName);

        data = db.getRow(query);

        return data;
    }

    /// <summery>
    /// Get Category By ID
    /// 
    /// <typeparam name=""></typeparam> integer catID
    /// <typeparam name=""></typeparam> integer status
    /// <returns></returns> stdClass
    /// </summery>
    public function getCategoryByID(catID, status = 1){
        global db;

        if(!is_numeric(catID))
            return;

        if(isset(status))
            query = db.prepare("SELECT * FROM " + TABLE_TRADE_CATEGORIES + " WHERE catID=%d AND STATUS=%d", catID, status);else
            query = db.prepare("SELECT * FROM " + TABLE_TRADE_CATEGORIES + " WHERE catID=%d", catID);

        data = db.getRow(query);

        return data;
    }

    /// <summery>
    /// Remove Trade category
    /// 
    /// <typeparam name=""></typeparam> Int categoryID
    /// </summery>
    public function removeCategory(categoryID){
        global db;

        query = db.prepare("DELETE FROM " + TABLE_TRADE_CATEGORIES + " WHERE catID=%s", categoryID);
        db.query(query);

        return;
    }

}