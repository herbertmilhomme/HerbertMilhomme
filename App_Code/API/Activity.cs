﻿using System;
using System.Collections.Generic;
using System.Web;
using db = Database_MSSQL;

/// <summary>
/// Manage user Activities
/// </summary>
class Activity {
   /// <summery>
   ///   1: CommentToPost, 
   ///   2: CommentedToMyComment, 
   ///   3: ReplyToMyTopic, 
   ///   4: ReplyToMyReply, 
   ///   5: PostApproved, 
   ///   6: Like Post
   ///   21: Product Sold
   ///   22: Offer Accepted
   ///   23: Offer Received
   ///   24: Offer Declined
   ///   25: Offer Feedback
   /// </summery>
    
    const int NOTIFICATION_TYPE_COMMENT_TO_POST    = 1;
    const int NOTIFICATION_TYPE_COMMENT_TO_COMMENT = 2;
    const int NOTIFICATION_TYPE_LIKE_POST          = 3;
    
    const int NOTIFICATION_TYPE_PRODUCT_SOLD       = 21;
    const int NOTIFICATION_TYPE_OFFER_ACCEPTED     = 22;
    const int NOTIFICATION_TYPE_OFFER_RECEIVED     = 23;
    const int NOTIFICATION_TYPE_OFFER_DECLINED     = 24;
    const int NOTIFICATION_TYPE_OFFER_FEEDBACK     = 25;
    

	public static int COUNT_PER_PAGE = 20;
	
    /// <summery>  
    /// </summery>
    /// <typeparam name="userID"></typeparam>     
    /// <typeparam name="objectID"></typeparam>     
    /// <typeparam name="objectType"></typeparam>     
    /// <typeparam name="activityType"></typeparam>     
    /// <typeparam name="actionID">int</typeparam>
    public static function addActivity(int userID,int objectID,int objectType,int activityType,int actionID = 0){
        //global db;
		//var db = Database.Open("HerbsSocialNet");
        
        //Remove Duplicated Like Action Activity
        if(activityType == "like"){
            string query = string.Format("DELETE FROM " + TABLE_MAIN_ACTIVITIES + " WHERE userID={0} AND objectID={1} AND objectType={2} AND activityType={3}", userID, objectID, objectType, activityType); //query = 
            db::Database_MSSQL.query(query);//db.Execute(query);
        }
        
        activityID = db.insertFromArray(TABLE_MAIN_ACTIVITIES, array(
            "userID" => userID,
            "objectID" => objectID,
            "objectType" => objectType,
            "activityType" => activityType,            
            "createdDate" => date("Y-m-d H:i:s"),            
            "isNew" => 1,            
            "activityStatus" => 1,            
            "actionID" => actionID           
        ));
        
        return activityID;
        
    }
    
    public static function addNotification(userID, activityID, notificationType, isNew = 1)
    {
        global db;
        
        db.insertFromArray(TABLE_MAIN_NOTIFICATIONS, array("userID" => userID, "activityID" => activityID, "notificationType" => notificationType, "isNew" => isNew, "createdDate" => time())); 
    }    
    
    public static function addForumActivity(userID, objectID, objectType, activityType, actionID = 0)
    {
        global db;
        
        activityID = db.insertFromArray(TABLE_FORUM_ACTIVITIES, array(
            "userID" => userID,
            "objectID" => objectID,
            "objectType" => objectType,
            "activityType" => activityType,            
            "createdDate" => date("Y-m-d H:i:s"),            
            "isNew" => 1,            
            "activityStatus" => 1,            
            "actionID" => actionID           
        ));
        
        return activityID;
        
    }
    
    
    
    /// <summery>
    /// <typeparam name="userID"></typeparam>     
    /// <typeparam name="limit">int</typeparam>  
    /// <returns>Indexed</returns> 
    /// </summery>
    public static function getActivities(userID, limit = 15){    
        global db;
        
        query = db.prepare("SELECT a.*,p.*, pc.content as comment_content FROM " + TABLE_MAIN_ACTIVITIES + " AS a "+
                    "INNER JOIN " + TABLE_FRIENDS + " as f ON a.userID=f.userFriendID  AND f.userID=%d AND f.status=1 "+
                    "LEFT JOIN " + TABLE_POSTS + " as p ON a.objectID=p.postID "+
                    "LEFT JOIN " + TABLE_POSTS_COMMENTS + " as pc ON a.activityType="comment" AND pc.commentID=a.actionID "+
                    "WHERE a.userID != %d AND p.poster != %d ORDER BY a.createdDate desc LIMIT %d", userID, userID, userID, limit);
        
        rows = db.getResultsArray(query);
        
        return rows;
    }
    
    /// <summery>
    /// <typeparam name=""></typeparam> row
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> string
    /// </summery>
    public static function getActivityHTML(row, userID){
        ob_start();
        user = User.getUserBasicInfo(row["userID"]);
        owner = User.getUserBasicInfo(row["poster"]);
        
        pagePostFlag = false;
        
        if (row["pageID"] != Post.INDEPENDENT_POST_PAGE_ID) {            
            pageIns = new Page();
            pageData = pageIns.getPageByID(row["pageID"]);            
        }
        
        if (isset(pageData)) {
            pagePostFlag = true;
        }
        
        if(pagePostFlag){
            objectLink = "/page.php?pid=" + row["pageID"] + "&post=" + row["objectID"];
            authorLink = "/page.php?pid=" + row["pageID"];
        }else{
            objectLink = "/posts.php?user=" + row["poster"] + "&post=" + row["objectID"];
            authorLink = "/profile.php?user=" + row["poster"];
        }
        if(row["activityType"] == "like"){
            
        ?>
            <div class="activityComment">
                 render_profile_link(user, "replyToPostIcons"); ?>
                <span>
                    <a href="/profile.php?user= echo row["userID"]?>"
                        class="userName"> echo user["firstName"] + " " + user["lastName"]?></a>
                    liked  echo row["poster"] == userID ? "your" : ("<a href="/profile.php?user=" + row["poster"] + "" class=\"userName\">" + owner["firstName"] + " " + owner["lastName"] + ""s</a>") ?>
                     
                    switch(row["type"]){
                            case "image":   
                                echo "<a href="" + objectLink + "">photo</a>";
                                break;
                            case "video":   
                                echo "<a href="" + objectLink + "">video</a>";
                                break;
                            case "text":
                            default:
                                echo "<a href="" + objectLink + "">post</a> ";
                            if(strlen(trunc_content(row["content"], 60)) > 0){
                                    echo "&#8220;" + trunc_content(row["content"], 60) + "&#8221;" ;
                                }
                                break;
                            
                        }
                    ?>
                </span>
            </div>
            
            
        }else if(row["activityType"] == "comment"){
            ?>
            <div class="activityComment">                
                 render_profile_link(user, "replyToPostIcons"); ?>
                <span>
                    <a href="/profile.php?user= echo row["userID"] ?>"
                        class="userName"> echo user["firstName"] + " " + user["lastName"] ?></a>
                    left a comment on 
                     
                    if(row["poster"] == userID){
                            echo "your";
                        } else if( row["poster"] == row["userID"] ){
                            //Getting User Data
                            tUinfo = User.getUserBasicInfo( row["userID"] );
                        switch(strtolower(tUinfo["gender"])){
                                case "male":
                                    echo "his";
                                    break;
                                case "female":
                                    echo "her";
                                    break;
                                break;
                                    echo "their";
                                    break;
                            }
                        } else {
                            echo "<a href="/profile.php?user=" + row["poster"] + "" class=\"userName\">" + owner["firstName"] + " " + owner["lastName"] + ""s</a>";
                        }
                    ?> 
                     
                    switch(row["type"]){
                            case "image":   
                                echo "<a href="" + objectLink + "">photo</a>";
                                break;
                            case "video":   
                                echo "<a href="" + objectLink + "">video</a>";
                                break;
                            case "text":
                            default:
                                echo "<a href="" + objectLink + "">post</a> ";                                
                                break;
                            
                        }
                    if(strlen(trunc_content(row["comment_content"], 25)) > 0){
                            echo ": &#8220;" + trunc_content(row["comment_content"], 25) + "&#8221;" ;
                        }
                    ?>                    
                </span>
            </div>
            
        }
        html = ob_get_contents();
        ob_end_clean();
        return html;
    }
    
    /// <summery>
    /// <typeparam name=""></typeparam>      userID
    /// <typeparam name=""></typeparam> int  page
    /// <typeparam name=""></typeparam> null status
    /// <returns></returns> Indexed
    /// </summery>
    public function getAppNotifications(userID, page = 1, status = null){
        global db;
        query = db.prepare("SELECT activityID FROM " + TABLE_MAIN_NOTIFICATIONS + " WHERE userID = %d " + (status != null ? " AND isNew=" + status : "") + " ORDER BY createdDate DESC ", userID);        
        query += " LIMIT " + (page - 1) * Activity.COUNT_PER_PAGE + ", " + Activity.COUNT_PER_PAGE;
        
        arows = db.getResultsArray(query);
        rows = array();
        foreach(arows as aid){
            query = db.prepare("SELECT a.*,p.*, pc.content AS comment_content FROM " + TABLE_MAIN_ACTIVITIES + " AS a " + " LEFT JOIN " + TABLE_POSTS + " AS p ON a.objectID=p.postID " + " LEFT JOIN " + TABLE_POSTS_COMMENTS + " AS pc ON a.activityType="comment" AND pc.commentID=a.actionID " + " WHERE a.activityID=%d", aid["activityID"]);
            row = db.getRow(query);
            rows[] = row;
        }
        
        return rows;
    }
	
    /// <summery>
    /// <typeparam name=""></typeparam>      userID
    /// <typeparam name=""></typeparam> int  limit
    /// <typeparam name=""></typeparam> null status
    /// <returns></returns> Indexed
    /// </summery>
    public static function getNotifications(userID, limit = 15, status = null){
        global db;
        query = db.prepare("SELECT activityID FROM " + TABLE_MAIN_NOTIFICATIONS + " WHERE userID = %d " + (status != null ? " AND isNew=" + status : "") + " ORDER BY createdDate DESC LIMIT %d", userID, limit);        
        
        arows = db.getResultsArray(query);
        rows = array();
        foreach(arows as aid){
            query = db.prepare("SELECT a.*,p.*, pc.content AS comment_content FROM " + TABLE_MAIN_ACTIVITIES + " AS a " + " LEFT JOIN " + TABLE_POSTS + " AS p ON a.objectID=p.postID " + " LEFT JOIN " + TABLE_POSTS_COMMENTS + " AS pc ON a.activityType="comment" AND pc.commentID=a.actionID " + " WHERE a.activityID=%d", aid["activityID"]);
            row = db.getRow(query);
            rows[] = row;
        }
        
        return rows;
    }
    
    /// <summery>
   /// Get the number of notifications
   /// 
   /// <typeparam name=""></typeparam> Int userID
   /// <returns></returns> Int
   /// </summery>
    public static function getNumberOfNotifications(userID, isNew = 1){
        global db;
        
        query = db.prepare("SELECT count(*) FROM " + TABLE_MAIN_NOTIFICATIONS + "            
                    WHERE userID = %d AND notificationType IN (%d, %d, %d) AND isNew = %d", 
                        userID, 
                        Activity.NOTIFICATION_TYPE_COMMENT_TO_POST, Activity.NOTIFICATION_TYPE_COMMENT_TO_COMMENT, Activity.NOTIFICATION_TYPE_LIKE_POST,
                        isNew);
        
        count = db.getVar(query);
        
        return count;
    }
    
    /// <summery>
   /// Make notifications to read
   /// 
   /// <typeparam name=""></typeparam> mixed userID
   /// <typeparam name=""></typeparam> mixed postID
   /// </summery>
    public static function markReadNotifications(userID, postID = null){
        global db;
        
        if(postID){
            query = db.prepare("UPDATE " + TABLE_MAIN_NOTIFICATIONS + " SET isNew=0 WHERE userID=%d AND activityID IN (SELECT activityID FROM " + TABLE_MAIN_ACTIVITIES + " WHERE objectID=%d AND isNew=1)", userID, postID);            
            db.query(query);
        }else{
            query = db.prepare("UPDATE " + TABLE_MAIN_NOTIFICATIONS + " SET isNew=0 WHERE userID=%d AND isNew=1", userID);            
            db.query(query);
        }
        return ;
    }
    
}