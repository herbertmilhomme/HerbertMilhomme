using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for ForumModerator
/// </summary>

/// <summery>
/// Forum Category Moderator
/// </summery>
class ForumModerator {

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> bool
    /// </summery>
    public static function isModerator(categoryID, userID){
        global db;

        query = db.prepare("SELECT id, status FROM " + TABLE_FORUM_MODERATORS + " WHERE userID=%d AND categoryID=%d", userID, categoryID);
        row = db.getRow(query);

        if(!row || row["status"] != "Approved")
            return false;else
            return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>      categoryID
    /// <typeparam name=""></typeparam> null userID
    /// <returns></returns> bool
    /// </summery>
    public static function isAppliedToModerate(categoryID, userID = null){
        global db;

        if(!userID)
            userID = is_logged_in();

        query = db.prepare("SELECT id FROM " + TABLE_FORUM_MODERATORS + " WHERE userID=%d AND categoryID=%d", userID, categoryID);
        mid = db.getVar(query);

        return mid ? true : false;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>      categoryID
    /// <typeparam name=""></typeparam> null userID
    /// <returns></returns> int|null|string
    /// </summery>
    public static function applyToModerate(categoryID, userID = null){
        global db;

        if(!userID)
            userID = is_logged_in();

        query = db.prepare("INSERT INTO " + TABLE_FORUM_MODERATORS + "(`userID`, `categoryID`, `status`, `createdDate`)VALUES(%d, %d, "Pending", %s)", userID, categoryID, date("Y-m-d H:i:s"));
        mid = db.insert(query);

        return mid;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getReportedArticles(categoryID){
        global db;

        query = db.prepare("(SELECT DISTINCT(r.reportID), 
                          r.objectID,
                          r.objectType,
                          r.objectID AS topicID,
                          r.reportedDate,                          
                          CONCAT(ru.firstName, " ", ru.lastName) as reporterName, 
                          ru.userID as reporterID, 
                          ru.thumbnail as reporterThumb,
                          CONCAT(ou.firstName, " ", ou.lastName) as ownerName, 
                          ou.user_acl_id,
                          ou.userID as ownerID, 
                          ou.thumbnail as ownerThumb                          
                  FROM " + TABLE_REPORTS + " AS r " + "LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=r.objectID " + "LEFT JOIN " + TABLE_USERS + " AS ru ON ru.userID=r.reporterID " + "LEFT JOIN " + TABLE_USERS + " AS ou ON ou.userID=r.reportedID " + "WHERE r.objectType="topic" AND r.reportStatus=1 AND t.categoryID=%d )" + " UNION " + "(SELECT DISTINCT(r.reportID),
                          r.objectID,
                          r.objectType,
                          fr.topicID,
                          r.reportedDate,                          
                          CONCAT(ru.firstName, " ", ru.lastName) as reporterName, 
                          ru.userID as reporterID, 
                          ru.thumbnail as reporterThumb,
                          CONCAT(ou.firstName, " ", ou.lastName) as ownerName, 
                          ou.user_acl_id,
                          ou.userID as ownerID, 
                          ou.thumbnail as ownerThumb                          
                  FROM " + TABLE_REPORTS + " AS r " + "LEFT JOIN " + TABLE_FORUM_REPLIES + " AS fr ON fr.replyID=r.objectID " + "LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=fr.topicID " + "LEFT JOIN " + TABLE_USERS + " AS ru ON ru.userID=r.reporterID " + "LEFT JOIN " + TABLE_USERS + " AS ou ON ou.userID=r.reportedID " + "WHERE r.objectType="reply" AND r.reportStatus=1 AND t.categoryID=%d) ORDER By reportedDate ", categoryID, categoryID);

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// <returns></returns> array
    /// </summery>
    public static function getReportedItemsCount(){
        global db;

        userID = is_logged_in();

        if(!userID)
            return [];

        if(is_admin() || is_moderator()){
            categoryWhere = "";
        }else{
            //Getting the categories that user can moderate
            query = db.prepare("SELECT categoryID FROM " + TABLE_FORUM_MODERATORS + " WHERE userID=%d", userID);
            categories = db.getResultsArray(query);
            if(!categories)
                return [];
            categoryWhere = [];
            foreach(categories as c){
                categoryWhere[] = c["categoryID"];
            }

            categoryWhere = " AND t.categoryID IN (" + implode(",", categories) + ")";
        }

        query = "SELECT count(DISTINCT(r.reportID)) AS c, t.categoryID
                  FROM " + TABLE_REPORTS + " AS r " + "LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=r.objectID " + "WHERE r.objectType="topic" AND r.reportStatus=1 " + categoryWhere + " GROUP BY categoryID ";
        topics = db.getResultsArray(query);

        query = "SELECT count(DISTINCT(r.reportID)) AS c, t.categoryID
                  FROM " + TABLE_REPORTS + " AS r " + "LEFT JOIN " + TABLE_FORUM_REPLIES + " AS fr ON fr.replyID=r.objectID " + "LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=fr.topicID " + "WHERE r.objectType="reply" AND r.reportStatus=1 " + categoryWhere + " GROUP BY categoryID ";
        replies = db.getResultsArray(query);

        results = [];
        foreach(topics as row){
            if(!isset(results[row["categoryID"]]))
                results[row["categoryID"]] = 0;
            results[row["categoryID"]] += row["c"];
        }
        foreach(replies as row){
            if(!isset(results[row["categoryID"]]))
                results[row["categoryID"]] = 0;
            results[row["categoryID"]] += row["c"];
        }

        return results;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getApplicants(categoryID){
        global db;

        query = db.prepare("SELECT m.id, m.userID, m.categoryID, CONCAT(u.firstName, " ", u.lastName) AS applicantName, u.thumbnail 
                                FROM " + TABLE_FORUM_MODERATORS + " AS m LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=m.userID 
                                WHERE m.categoryID=%d AND m.status="Pending"", categoryID);
        users = db.getResultsArray(query);

        return users;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <typeparam name=""></typeparam> applicants
    /// </summery>
    public static function approveApplicants(categoryID, applicants){
        global db;
        foreach(applicants as aid){
            query = db.prepare("UPDATE " + TABLE_FORUM_MODERATORS + " SET `status`="Approved" WHERE categoryID=%d AND userID=%d", categoryID, aid);
            db.query(query);
            //Make the user follow the forum
            if(!ForumFollower.isFollow(categoryID, aid)){
                ForumFollower.followForum(aid, categoryID);
            }
        }
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <typeparam name=""></typeparam> applicants
    /// </summery>
    public static function declineApplicants(categoryID, applicants){
        global db;

        foreach(applicants as aid){
            query = db.prepare("DELETE FROM " + TABLE_FORUM_MODERATORS + " WHERE categoryID=%d AND userID=%d AND `status`="Pending"", categoryID, aid);
            db.query(query);
        }
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <typeparam name=""></typeparam> userID
    /// </summery>
    public static function deleteModerator(categoryID, userID){
        global db;

        query = db.prepare("DELETE FROM " + TABLE_FORUM_MODERATORS + " WHERE categoryID=%d AND userID=%d AND `status`="Approved"", categoryID, userID);
        db.query(query);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getForumModerators(categoryID){
        global db;

        query = db.prepare("SELECT m.id, m.userID, m.categoryID, CONCAT(u.firstName, " ", u.lastName) AS applicantName, u.thumbnail 
                                FROM " + TABLE_FORUM_MODERATORS + " AS m LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=m.userID 
                                WHERE m.categoryID=%d AND m.status="Approved" ORDER BY applicantName", categoryID);
        users = db.getResultsArray(query);

        return users;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> categoryID
    /// </summery>
    public static function blockUser(userID, categoryID){
        global db;

        //Getting Users Topics and Replies
        query = db.prepare("SELECT * FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=%d AND categoryID=%d", userID, categoryID);
        topics = db.getResultsArray(query);

        foreach(topics as row){
            ForumTopic.deleteTopic(row["topicID"]);
        }

        query = db.prepare("SELECT r.replyID FROM " + TABLE_FORUM_REPLIES + " AS r LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=r.topicID WHERE r.creatorID=%d AND t.categoryID=%d", userID, categoryID);
        replies = db.getResultsArray(query);

        foreach(replies as row){
            ForumReply.deleteReply(row["replyID"]);
        }

        //Block User
        query = db.prepare("INSERT INTO " + TABLE_FORUM_BLOCKED_USRES + "(userID, categoryID, blockedDate)VALUES(%d, %d, %s)", userID, categoryID, date("Y-m-d H:i:s"));
        db.query(query);

    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> categoryID
    /// </summery>
    public static function unBlockUser(userID, categoryID){
        global db;

        query = db.prepare("DELETE FROM " + TABLE_FORUM_BLOCKED_USRES + " WHERE categoryID=%d AND userID=%d", categoryID, userID);
        db.query(query);

        return;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> one
    /// </summery>
    public static function isBlocked(userID, categoryID){
        global db;

        query = db.prepare("SELECT id FROM " + TABLE_FORUM_BLOCKED_USRES + " WHERE categoryID=%d AND userID=%d", categoryID, userID);
        id = db.getVar(query);

        return id;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getBlockedUsers(categoryID){
        global db;

        query = db.prepare("SELECT u.*, b.id AS blockedID, b.categoryID FROM " + TABLE_FORUM_BLOCKED_USRES + " AS b LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=b.userID WHERE b.categoryID=%d", categoryID);

        users = db.getResultsArray(query);

        return users;
    }
}