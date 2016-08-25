using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Report
/// </summary>

/// <summery>
/// Manage reported
/// </summery>
class Report {

    public static COUNT_PER_PAGE = 20;

    /// <summery>
    /// Report post, comment and message
    /// 
    /// <typeparam name=""></typeparam> Int    userID
    /// <typeparam name=""></typeparam> Int    objectID
    /// <typeparam name=""></typeparam> String objectType
    /// <returns></returns> Boolean or String
    /// </summery>
    public static function reportObject(userID, objectID, objectType){
        global db;

        //Check that the object has already been reported by the user
        query = db.prepare("SELECT reportID FROM " + TABLE_REPORTS + " WHERE reporterID=%d AND objectID=%d AND objectType=%s", userID, objectID, objectType);
        rID = db.getVar(query);

        if(rID){
            return MSG_ALREADY_REPORTED;
        }

        //Check that the object id is correct
        switch(objectType){
            case "post":
                query = db.prepare("SELECT postID AS objectID, poster AS reportedID FROM " + TABLE_POSTS + " WHERE postID=%d AND `post_status`=1", objectID);
                break;
            case "comment":
                query = db.prepare("SELECT commentID AS objectID, commenter AS reportedID FROM " + TABLE_POSTS_COMMENTS + " WHERE commentID=%d AND commentStatus=1", objectID);
                break;
            case "video_comment":
                query = db.prepare("SELECT commentID AS objectID, userID AS reportedID FROM " + TABLE_VIDEO_COMMENTS + " WHERE commentID=%d", objectID);
                break;
            case "message":
                query = db.prepare("SELECT messageID AS objectID, sender AS reportedID  FROM " + TABLE_MESSAGES + " WHERE userID=%d AND messageID=%d AND messageStatus=1", userID, objectID);
                break;
            case "topic":
                query = db.prepare("SELECT topicID AS objectID, creatorID AS reportedID FROM " + TABLE_FORUM_TOPICS + " WHERE topicID=%d AND `status`="publish"", objectID);
                break;
            case "reply":
                query = db.prepare("SELECT replyID AS objectID, creatorID AS reportedID FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=%d AND `status`="publish"", objectID);
                break;
            case "trade_item":
                query = db.prepare("SELECT itemID AS objectID, userID AS reportedID FROM " + TABLE_TRADE_ITEMS + " WHERE itemID=%d", objectID);
                break;
            case "shop_item":
                query = db.prepare("SELECT productID AS objectID, userID AS reportedID FROM " + TABLE_SHOP_PRODUCTS + " WHERE productID=%d", objectID);
                break;

        }
        oRow = db.getRow(query);

        if(!oRow){
            return MSG_INVALID_REQUEST;
        }

        //Report Object
        nId = db.insertFromArray(TABLE_REPORTS, ["reporterID" => userID, "objectID" => oRow["objectID"], "objectType" => objectType, "reportedID" => oRow["reportedID"], "reportStatus" => 1, "reportedDate" => date("Y-m-d H:i:s")]);
        if(!nId)
            return db.getLastError();else
            return true;
    }

    /// <summery>
    /// Get Reported Object Count
    /// 
    /// <returns></returns> Int
    /// </summery>
    public static function getReportedObjectCount(){
        global db;

        query = "SELECT count(objectID) FROM " + TABLE_REPORTS + " WHERE reportStatus=1";
        count = db.getVar(query);

        return count;
    }

    /// <summery>
    /// Get Reported Object Count
    /// 
    /// <typeparam name=""></typeparam> Int page
    /// <typeparam name=""></typeparam> int limit
    /// <returns></returns> Array
    /// </summery>
    public static function getReportedObject(page = 1, limit = null){
        global db;

        query = "SELECT DISTINCT(r.reportID), 
                          r.objectID,
                          r.objectType,
                          r.reportedDate,                          
                          CONCAT(ru.firstName, " ", ru.lastName) AS reporterName,
                          ru.userID AS reporterID,
                          ru.thumbnail AS reporterThumb,
                          CONCAT(ou.firstName, " ", ou.lastName) AS ownerName,
                          ou.user_acl_id,
                          ou.userID AS ownerID,
                          ou.thumbnail AS ownerThumb
                  FROM " + TABLE_REPORTS + " AS r " + "LEFT JOIN " + TABLE_USERS + " AS ru ON ru.userID=r.reporterID " + "LEFT JOIN " + TABLE_USERS + " AS ou ON ou.userID=r.reportedID " + "WHERE reportStatus=1 ORDER BY reportedDate ";

        if(limit == null)
            limit = Report.COUNT_PER_PAGE;

        query += " LIMIT " + (page - 1) * limit + ", " + limit;

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Delete Objects
    /// 
    /// <typeparam name=""></typeparam> Array ids
    /// </summery>
    public static function deleteObjects(ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        ids = db.escapeInput(ids);

        query = db.prepare("SELECT * FROM " + TABLE_REPORTS + " WHERE reportID IN (" + implode(", ", ids) + ")");
        rows = db.getResultsArray(query);

        foreach(rows as row){
            if(row["objectType"] == "post"){
                post = db.getRow("SELECT * FROM " + TABLE_POSTS + " WHERE postID=" + row["objectID"]);
                Post.deletePost(post["poster"], post["postID"]);
            }else if(row["objectType"] == "comment"){
                //Getting Data
                comment = db.getRow("SELECT * FROM " + TABLE_POSTS_COMMENTS + " WHERE commentID=" + row["objectID"]);
                Comment.deleteComment(comment["commenter"], comment["commentID"]);
            }else if(row["objectType"] == "video_comment"){
                //Getting Data
                comment = db.getRow("SELECT * FROM " + TABLE_VIDEO_COMMENTS + " WHERE commentID=" + row["objectID"]);
                Video.deleteVideoComment(comment["commentID"]);
            }else if(row["objectType"] == "message"){
                //Delete Message
                db.query("DELETE FROM " + TABLE_MESSAGES + " WHERE messageID=" + row["objectID"]);
            }else if(row["objectType"] == "topic"){
                //Delete Topic
                ForumTopic.deleteTopic(row["objectID"]);
            }else if(row["objectType"] == "reply"){
                //Delete Topic
                ForumReply.deleteReply(row["objectID"]);
            }else if(row["objectType"] == "shop_item"){
                //Delete Shop Product
                shopProdIns = new ShopProduct();
                shopProdIns.removeProductByUserID(row["objectID"], row["reportedID"]);
            }else if(row["objectType"] == "trade_item"){
                //Delete Trade Item
                tradeItemIns = new TradeItem();
                tradeItemIns.removeItemByUserID(row["objectID"], row["reportedID"]);
            }

            //Delete the row on the report table
            db.query("DELETE FROM " + TABLE_REPORTS + " WHERE reportID=" + row["reportID"]);
        }

        return;
    }

    /// <summery>
    /// Approve Reported Objects
    /// 
    /// <typeparam name=""></typeparam> Array ids
    /// </summery>
    public static function approveObjects(ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        ids = db.escapeInput(ids);

        query = "SELECT * FROM " + TABLE_REPORTS + " WHERE reportID IN (" + implode(", ", ids) + ")";
        rows = db.getResultsArray(query);

        foreach(rows as row){
            //Delete the row on the report table
            db.query("DELETE FROM " + TABLE_REPORTS + " WHERE reportID=" + row["reportID"]);
        }

        return;
    }

    /// <summery>
    /// Ban users
    /// 
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> int
    /// </summery>
    public static function banUsers(ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        query = "SELECT * FROM " + TABLE_REPORTS + " WHERE reportID IN (" + implode(", ", ids) + ")";
        rows = db.getResultsArray(query);

        bannedUsers = 0;
        adminUsers = 0;
        foreach(rows as row){
            //Getting User ID
            if(row["objectType"] == "post"){
                query = "SELECT poster FROM " + TABLE_POSTS + " WHERE postID=" + row["objectID"];
            }else if(row["objectType"] == "comment"){
                query = "SELECT commenter FROM " + TABLE_POSTS_COMMENTS + " WHERE commentID=" + row["objectID"];
            }else if(row["objectType"] == "video_comment"){
                query = "SELECT userID FROM " + TABLE_VIDEO_COMMENTS + " WHERE commentID=" + row["objectID"];
            }else if(row["objectType"] == "message"){
                query = "SELECT sender FROM " + TABLE_MESSAGES + " WHERE messageID=" + row["objectID"];
            }else if(row["objectType"] == "topic"){
                query = "SELECT creatorID FROM " + TABLE_FORUM_TOPICS + " WHERE topicID=" + row["objectID"];
            }else if(row["objectType"] == "reply"){
                query = "SELECT creatorID FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=" + row["objectID"];
            }
            userID = db.getVar(query);

            if(userID){
                if(!check_user_acl(USER_ACL_MODERATOR, userID)){
                    BanUser.banUser(userID);
                    bannedUsers++;
                }else{
                    adminUsers++;
                }
            }

        }
        if(adminUsers > 0)
            add_message(MSG_CAN_NOT_BAN_ADMIN, MSG_TYPE_NOTIFY);

        return bannedUsers;
    }

    /// <summery>
    /// Check the object is reported and return the report id if it is reported
    /// 
    /// <typeparam name=""></typeparam> mixed objectID
    /// <typeparam name=""></typeparam> mixed objectType
    /// <returns></returns> one
    /// </summery>
    public static function isReported(objectID, objectType){
        global db;

        query = db.prepare("SELECT reportID FROM " + TABLE_REPORTS + " WHERE objectID=%d AND objectType=%s", objectID, objectType);

        reportID = db.getVar(query);

        return reportID;
    }

}
