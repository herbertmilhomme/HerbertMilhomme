using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for ForumReply
/// </summary>
/// <summery>
* Manage Forum Reply
* 
*/

class ForumReply
{
    static COUNT_PER_PAGE = 10;
    
    /// <summery>
   /// Get Replies by Topic ID
   /// 
   /// <typeparam name=""></typeparam> Int topicID
   /// <typeparam name=""></typeparam> Int page
   /// <typeparam name=""></typeparam> String orderBy : oldest, newest, toprated
   /// <returns></returns> Array
   /// </summery>
    public static function getReplies(topicID = null, status=null, page = 1, orderBy = "oldest")
    {
        global db, GLOBALS;
        
        if(!GLOBALS["user"]["userID"])
            query = "SELECT r.*, CONCAT(u.firstName, " ", u.lastName) AS creatorName, u.thumbnail,u.posts_count, u.posts_rating, 0 AS voteID, 0 AS reportID, 0 AS voteStatus, us.reputation FROM " + TABLE_FORUM_REPLIES + " AS r " .
                     "LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=r.creatorID " .
                     "LEFT JOIN " + TABLE_USERS_STATS + " AS us ON us.userID=r.creatorID ";
        else
            query = "SELECT r.*, CONCAT(u.firstName, " ", u.lastName) AS creatorName, u.thumbnail,u.posts_count, u.posts_rating, v.voteID, v.voteStatus, rp.reportID, us.reputation FROM " + TABLE_FORUM_REPLIES + " AS r " .
                     " LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=r.creatorID " .
                     " LEFT JOIN " + TABLE_USERS_STATS + " AS us ON us.userID=r.creatorID " .
                     " LEFT JOIN " + TABLE_FORUM_VOTES + " AS v ON v.objectID=r.replyID AND v.voterID=" + GLOBALS["user"]["userID"] .
                     " LEFT JOIN " + TABLE_REPORTS + " AS rp ON rp.objectType="reply" AND rp.objectID=r.replyID AND rp.reporterID=" + GLOBALS["user"]["userID"];
        
        where = array();
        if(status != null)
            where[] = db.prepare(" r.status=%s ", status);
        
        if(topicID != null)
            where[] = db.prepare(" r.topicID=%d", topicID);
        
        if(count(where) > 0)
            query += " WHERE " + implode(" AND ", where);
            
        switch(strtolower(orderBy))         
        {
            case "highrated":
                query += " ORDER BY r.votes DESC ";
                break;
            case "newest":
                query += " ORDER BY r.createdDate DESC ";
                break;            
            case "oldest":
            default:
                query += " ORDER BY r.createdDate ASC ";
                break;            
        }
        
        query += " LIMIT " + (page - 1) * ForumReply.COUNT_PER_PAGE .", " + ForumReply.COUNT_PER_PAGE;
        
        rows = db.getResultsArray(query);
        
        return rows;
    }

    /// <summery>
    /// get reply by ID
    /// 
    /// <typeparam name=""></typeparam> mixed replyID
    /// <returns></returns> array|void
    /// </summery>
    public function getReplyByID(replyID) {
        
        
        global db;
        
        if (!is_numeric(replyID))
            return;
        query = sprintf("SELECT * FROM %s WHERE replyID=%d", TABLE_FORUM_REPLIES, replyID);
        
        return db.getRow(query);        
        
    }
    
    /// <summery>
   /// Getting Total Number Of Replies
   /// 
   /// <typeparam name=""></typeparam> Int topicID
   /// <returns></returns> Int
   /// </summery>
    public static function getTotalNumOfReplies(topicID = null, status = null)
    {
        global db;
        
        query = "SELECT count(1) FROM " + TABLE_FORUM_REPLIES;
                 
        where = array();
        if(status != null)
            where[] = db.prepare(" status=%s ", status);
        
        if(topicID != null)
            where[] = db.prepare(" topicID=%d", topicID);
        
        if(count(where) > 0)
            query += " WHERE " + implode(" AND ", where);
            
        count = db.getVar(query);
        
        return count;
    }

    /// <summery>
    /// Create Post Reply
    /// 
    /// <typeparam name=""></typeparam> mixed data
    /// <returns></returns> null|string
    /// </summery>
    public static function createReply(data)
    {
        global db, GLOBALS;
        
        content = trim(data["content"]);
        
        if(!content)
        {
            return MSG_ALL_FIELDS_REQUIRED;
        }
        
        content = remove_invalid_image_urls(content);
        content = remove_tags_inside_code(content);
        
        //Check Category ID is valid or not
        query = db.prepare("SELECT topicID, categoryID, creatorID FROM " + TABLE_FORUM_TOPICS + " WHERE topicID=%d AND status="publish"", data["topicID"]);
        topic = db.getRow(query);
        if(!topic)
        {
            return MSG_INVALID_REQUEST;
        }
        
        query = "INSERT INTO " + TABLE_FORUM_REPLIES + "(
                    `topicID`,
                    `replyContent`,
                    `creatorID`,
                    `createdDate`,
                    `votes`,
                    `status`
                )VALUES(
                    "" + topic["topicID"] + "",
                    "" + db.escapeInput(content, false) + "",
                    "" + GLOBALS["user"]["userID"] + "",
                    "" + date("Y-m-d H:i:s") + "",
                    "0",
                    "pending"
                )";
        db.query(query);
        
        newID = db.getLastInsertId();
        
        if(!newID)
            return db.getLastError();
            
        //If the user has more than 5 actived topics, update the topic status to 1
        count1 = db.getVar("SELECT count(1) FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=" + GLOBALS["user"]["userID"] + " AND `status`="publish"");
        count2 = db.getVar("SELECT count(1) FROM " + TABLE_FORUM_REPLIES + " WHERE creatorID=" + GLOBALS["user"]["userID"] + " AND `status`="publish"");
        if(count1 + count2 >= 5){
            //Publish  Reply
            db.updateFromArray(TABLE_FORUM_REPLIES, array("status" => "publish"), array("replyID" => newID));
            
            //Update Topic Table
            db.query("UPDATE " + TABLE_FORUM_TOPICS + " SET lastReplyID=" + newID + ", `replies`=`replies` + 1, lastReplyDate="" + date("Y-m-d H:i:s") + "", lastReplierID=" + GLOBALS["user"]["userID"] + " WHERE topicID=" + topic["topicID"]);
            
            db.query("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `replies`=`replies` + 1, lastTopicID="" + topic["topicID"] + "" WHERE categoryID=" + topic["categoryID"]);
            
            //Increase user posts count
            db.query("UPDATE " + TABLE_USERS + " SET `posts_count`=`posts_count` + 1 WHERE userID=" + GLOBALS["user"]["userID"]);
            
            //Add Notifications
            forumNotification = new ForumNotification();
            forumNotification.addNotificationsForReplies(topic["creatorID"], topic["topicID"], newID);
            if(topic["creatorID"] != GLOBALS["user"]["userID"])
                forumNotification.addNotificationsForTopic(topic["creatorID"], topic["topicID"], newID);
                
            //Update User Stats
            User.updateStats(topic["creatorID"], "replies", 1);
            
            return "publish";
        }
        
        return "pending";
    }


    /// <summery>
    /// Edit Post Reply
    /// 
    /// <typeparam name=""></typeparam> mixed data
    /// <returns></returns> bool|string
    /// </summery>
    public function editReply(data)
    {
        global db, GLOBALS;
        
        content = trim(data["content"]);
        
        if(!content)
        {
            return MSG_ALL_FIELDS_REQUIRED;
        }
        
        content = remove_invalid_image_urls(content);
        
        //Check Category ID is valid or not
        query = db.prepare("SELECT topicID, categoryID, creatorID FROM " + TABLE_FORUM_TOPICS + " WHERE topicID=%d AND status="publish"", data["topicID"]);
        topic = db.getRow(query);
        if(!topic)
        {
            return MSG_INVALID_REQUEST;
        }
        
        query = "UPDATE " + TABLE_FORUM_REPLIES + " SET `replyContent`="" + db.escapeInput(content, false) + "" WHERE `replyID`=" + db.escapeInput(data["replyID"]) ;
        
        db.query(query);
        
        return true;
        
    }


    /// <summery>
    /// Approve Pending Replies
    /// 
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool|string
    /// </summery>
    public static function approvePendingReplies(ids)
    {
        global db;
        
        if(!is_array(ids))
            ids = array(ids);
        
        ids = db.escapeInput(ids);
        
        //Getting Topics for confirmation
        query = "SELECT r.topicID, r.replyID, t.categoryID, r.creatorID, r.createdDate, t.creatorID AS topicCreatorID FROM " + TABLE_FORUM_REPLIES + " AS r LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=r.topicID WHERE r.status="pending" AND r.replyID IN (" + implode(", ", ids) + ")";
        rows = db.getResultsArray(query);
        
        if(!rows)
            return MSG_INVALID_REQUEST;
        
        forumNotification = new ForumNotification();
                
        foreach(rows as row)
        {
            //Update Topic Status
            db.updateFromArray(TABLE_FORUM_REPLIES, array("status" => "publish", "createdDate" => date("Y-m-d H:i:s")), array("replyID" => row["replyID"]));            
            
            //Update Category Table
            db.query("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `replies`=`replies` + 1, lastTopicID="" + row["topicID"] + "" WHERE categoryID=" + row["categoryID"]);            
            
            db.query("UPDATE " + TABLE_FORUM_TOPICS + " SET `replies`=`replies` + 1 WHERE topicID=" + row["topicID"]);            
            db.query("UPDATE " + TABLE_FORUM_TOPICS + " SET lastReplyID=" + row["replyID"] + ", lastReplyDate="" + date("Y-m-d H:i:s") + "", lastReplierID=" + row["creatorID"] + " WHERE topicID=" + row["topicID"] + " AND lastReplyID < " + row["replyID"]);  
            
            //Increase user posts count
            db.query("UPDATE " + TABLE_USERS + " SET `posts_count`=`posts_count` + 1 WHERE userID=" + row["creatorID"]);
            
            //Add Notifications
            
            forumNotification.addNotificationsForPendingPost(row["creatorID"], row["topicID"], row["replyID"]);
            forumNotification.addNotificationsForReplies(row["creatorID"], row["topicID"], row["replyID"]);
            if(row["topicCreatorID"] != row["creatorID"])
                forumNotification.addNotificationsForTopic(row["topicCreatorID"], row["topicID"], row["replyID"]);
            
            //Update User Stats
            User.updateStats(row["topicCreatorID"], "replies", 1);
        }
        
        return true;
    }

    /// <summery>
    /// Delete Pending Replies
    /// 
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool|string
    /// </summery>
    public static function deletePendingReplies(ids)
    {
        global db;
        
        if(!is_array(ids))
            ids = array(ids);
        
        ids = db.escapeInput(ids);
        
        //Getting Topics for confirmation
        query = "SELECT r.topicID, r.replyID FROM " + TABLE_FORUM_REPLIES + " AS r WHERE r.status="pending" AND r.replyID IN (" + implode(", ", ids) + ")";
        rows = db.getResultsArray(query);
        
        if(!rows)
            return MSG_INVALID_REQUEST;
                        
        foreach(rows as row)
        {
            db.query("DELETE FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=" + row["replyID"]);            
            
        }
        
        return true;
    }

    /// <summery>
    /// Cast a vote on a reply
    /// 
    /// <typeparam name=""></typeparam> Int userID : voterID
    /// <typeparam name=""></typeparam> replyID
    /// <typeparam name=""></typeparam> Int voteType : 1: Thumb up, -1: Thumb Down
    /// <returns></returns> Int|null|string
    /// </summery>
    public static function voteReply(userID, replyID, voteType)
    {
        global db, GLOBALS;
        
        //Check Reply ID        
        query = db.prepare("SELECT replyID, votes, creatorID, topicID FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=%d AND STATUS="publish"", replyID);
        reply = db.getRow(query);
        
        if(!reply)
            return MSG_INVALID_REQUEST;
        
        replyID = reply["replyID"];
        votes = reply["votes"];
        
        //Check the user already casted his vote or not
        query = db.prepare("SELECT voteID FROM " + TABLE_FORUM_VOTES + " WHERE objectID=%d AND voterID=%d AND objectType="reply"", replyID, userID);
        voteID = db.getVar(query);
        if(voteID)
            return MSG_ALREADY_CASTED_A_VOTE;
            
        //Add Vote
        voteID = db.insertFromArray(TABLE_FORUM_VOTES, array("objectID" => replyID, "voterID" => userID, "objectType" => "reply", "voteStatus" => voteType, "voteDate" => date("Y-m-d H:i:s")));
        if(!voteID)
            return db.getLastError();
        
        votes += voteType;
        db.update("UPDATE " + TABLE_FORUM_REPLIES + " SET `votes` = " + votes + " WHERE replyID=" + replyID);
        
        //Update user ragings        
        db.query("UPDATE " + TABLE_USERS + " SET `posts_rating`=`posts_rating` " + (voteType > 0 ? "+" : "-") + " 1 WHERE userID=" + reply["creatorID"]);
        
        if (voteType > 0) {
            //Update User Stats
            User.updateStats(reply["creatorID"], "voteUps", 1);
        }
        
        return votes;
    }

    /// <summery>
    /// Delete Reply
    /// 
    /// <typeparam name=""></typeparam> Int replyID
    /// <returns></returns> bool
    /// </summery>
    public static function deleteReply(replyID)
    {
        global db;
        
        query = db.prepare("SELECT * FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=%d", replyID);
        reply = db.getRow(query);
        if(reply)
        {
            if(reply["status"] == "publish")
            {
                //Getting Topic
                query = db.prepare("SELECT * FROM " + TABLE_FORUM_TOPICS + " WHERE topicID=%d", reply["topicID"]);
                topic = db.getRow(query);
                
                //Update Replies Count For Topic
                query = "UPDATE " + TABLE_FORUM_TOPICS + " SET `replies`=`replies` - 1 WHERE topicID=" + reply["topicID"];
                db.query(query);
                //Update Replies Count For Category
                query = "UPDATE " + TABLE_FORUM_CATEGORIES + " SET `replies`=`replies` - 1 WHERE categoryID=" + topic["categoryID"];
                db.query(query);
                
                db.query("UPDATE " + TABLE_USERS + " SET `posts_count`=`posts_count` - 1 WHERE userID=" + reply["creatorID"]);
                db.query("UPDATE " + TABLE_USERS + " SET `posts_rating`=`posts_rating`" + (reply["votes"] > 0 ? "-" : "+" ) + abs(reply["votes"])  + " WHERE userID=" + reply["creatorID"]);
                
                //Update Stats
                User.updateStats(topic["creatorID"], "replies", -1);
                User.updateStats(reply["creatorID"], "voteUps", -1 * reply["votes"]);
            }            
            //Remove Reply Votes
            query = "DELETE FROM " + TABLE_FORUM_VOTES + " WHERE objectID=" + reply["replyID"];
            db.query(query);
            
            //Delete Frome Reports Table
            query = "DELETE FROM " + TABLE_REPORTS + " WHERE objectType="reply" AND objectID=" + reply["replyID"];
            db.query(query);
            
            //Remove Reply
            query = "DELETE FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=" + reply["replyID"];
            db.query(query);
            
            ForumTopic.updateTopicLastReplyID(reply["topicID"]);
            ForumCategory.updateCategoryLastTopicID(topic["categoryID"]);
            
            return true;
        }
        
        return false;
    }

    /// <summery>
    /// Get Forum ID
    /// 
    /// <typeparam name=""></typeparam> mixed replyID
    /// <returns></returns> one
    /// </summery>
    public static function getForumID(replyID)
    {
        global db;
        
        query = db.prepare("SELECT topicID FROM " + TABLE_FORUM_REPLIES + " WHERE replyID=%d", replyID);
        
        return db.getVar(query);
    }
}
