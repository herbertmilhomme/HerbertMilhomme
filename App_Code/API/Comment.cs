using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Comment
/// </summary>

/// <summery>
/// Manage Post Comments
/// </summery>
class Comment {

    public static COMMENT_LIMIT = 5;

    /// <summery>
    /// Getting Post Comments
    /// 
    /// <typeparam name=""></typeparam>      postID
    /// <typeparam name=""></typeparam> null last_date
    /// <returns></returns> Indexed
    /// </summery>
    public static function getPostComments(postID, last_date = null){
        global db;

        userID = is_logged_in();

        if(!last_date)
            last_date = date("Y-m-d H:i:s");
        query = db.prepare("SELECT c.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, p.poster, r.reportID FROM " + TABLE_POSTS_COMMENTS + " AS c " + "LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=c.commenter " + "LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID=c.postID " + "LEFT JOIN " + TABLE_REPORTS + " AS r ON r.objectID=c.commentID AND r.objectType="comment" AND r.reporterID=%d " + "WHERE c.commentStatus=1 AND c.postID=%s AND c.posted_date < %s ORDER BY c.posted_date DESC LIMIT 5 ", !userID ? 0 : userID, postID, last_date);

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Getting All Post Comments
    /// 
    /// <typeparam name=""></typeparam> postID
    /// <returns></returns> Indexed
    /// </summery>
    public function getPostAllComments(postID){
        global db;

        userID = is_logged_in();

        query = db.prepare("SELECT c.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, u.thumbnail AS commenterThumbnail, p.poster, r.reportID FROM " + TABLE_POSTS_COMMENTS + " AS c " + "LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=c.commenter " + "LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID=c.postID " + "LEFT JOIN " + TABLE_REPORTS + " AS r ON r.objectID=c.commentID AND r.objectType="comment" AND r.reporterID=%d " + "WHERE c.commentStatus=1 AND c.postID=%s ORDER BY c.posted_date DESC ", !userID ? 0 : userID, postID);

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Get Post Comments Count
    /// 
    /// <typeparam name=""></typeparam> mixed postID
    /// <returns></returns> Int
    /// </summery>
    public static function getPostCommentsCount(postID){
        global db;

        query = db.prepare("SELECT comments FROM " + TABLE_POSTS + " WHERE postID=%d", postID);
        c = db.getVar(query);

        return c;
    }

    /// <summery>
    /// Save Comment
    /// 
    /// <typeparam name=""></typeparam> Int    userID
    /// <typeparam name=""></typeparam> Int    postID
    /// <typeparam name=""></typeparam> String comment
    /// <returns></returns> int|null|string
    /// </summery>
    public static function saveComments(userID, postID, comment, image = null){
        global db;

        now = date("Y-m-d H:i:s");

        if(image != null){

            if(file_exists(DIR_FS_PHOTO_TMP + image)){
                list(width, height, type, attr) = getimagesize(DIR_FS_PHOTO_TMP + image);

                if(width > MAX_COMMENT_IMAGE_WIDTH){
                    height = height * (MAX_COMMENT_IMAGE_WIDTH / width);
                    width = MAX_COMMENT_IMAGE_WIDTH;
                }
                if(height > MAX_COMMENT_IMAGE_HEIGHT){
                    width = width * (MAX_COMMENT_IMAGE_HEIGHT / height);
                    height = MAX_COMMENT_IMAGE_HEIGHT;
                }

                Post.moveFileFromTmpToUserFolder(userID, image, width, height, 0, 0);
            }else{
                image = null;
            }
        }

        newId = db.insertFromArray(TABLE_COMMENTS, ["postID" => postID, "commenter" => userID, "content" => comment, "image" => image, "posted_date" => now]);

        if(not_null(newId)){
            postData = Post.getPostById(postID);
            UsersDailyActivity.addComment(userID);
            //Update comments on the posts table
            query = db.prepare("UPDATE " + TABLE_POSTS + " SET `comments`=`comments` + 1 WHERE postID=%d", postID);
            db.query(query);
            
            //Add Activity
            activityID = Activity.addActivity(userID, postID, "post", "comment", newId);
            
            //Add Notification
            if(postData["poster"] != userID)
                Activity.addNotification(postData["poster"], activityID, Activity.NOTIFICATION_TYPE_COMMENT_TO_POST);
            
            //Get Already Commented users which commentToComment is 1
            query = db.prepare("SELECT DISTINCT(pc.commenter), IFNULL(un.notifyCommentToMyComment, 1) AS notifyCommentToMyComment FROM " + TABLE_POSTS_COMMENTS + " AS pc LEFT JOIN " + TABLE_USERS_NOTIFY_SETTINGS + " AS un ON pc.commenter = un.userID WHERE pc.postID=%d AND pc.commenter != %d AND IFNULL(un.notifyCommentToMyComment, 1) > 0", postID, userID);
            rows = db.getResultsArray(query);
            
            foreach(rows as row){
                Activity.addNotification(row["commenter"], activityID, Activity.NOTIFICATION_TYPE_COMMENT_TO_COMMENT);
            }
            
            //Increase Hits
            Hit.addHit(postID, userID);

            //Update User Stats
            User.updateStats(postData["poster"], "comments", 1);

        }
        return newId;
    }

    /// <summery>
    /// Get Comment By ID
    /// 
    /// <typeparam name=""></typeparam> commentID
    /// <returns></returns> array
    /// </summery>
    public static function getComment(commentID){
        global db;

        userID = is_logged_in();

        query = db.prepare("SELECT c.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, p.poster, r.reportID FROM " + TABLE_POSTS_COMMENTS + " AS c
                                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=c.commenter
                                    LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID=c.postID
                                    LEFT JOIN " + TABLE_REPORTS + " AS r ON r.objectID=c.commentID AND r.objectType="comment" AND r.reporterID=%d
                                    WHERE c.commentID=%s
                                    ", userID, commentID);
        row = db.getRow(query);

        return row;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>      postID
    /// <typeparam name=""></typeparam> null last_date
    /// <returns></returns> one
    /// </summery>
    public static function hasMoreComments(postID, last_date = null){
        global db;

        if(!last_date)
            last_date = date("Y-m-d H:i:s");
        query = db.prepare("SELECT count(1) FROM " + TABLE_POSTS_COMMENTS + " WHERE postID=%s AND posted_date < %s ", postID, last_date);

        c = db.getVar(query);

        return c;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> commentID
    /// <returns></returns> bool
    /// </summery>
    public static function deleteComment(userID, commentID){
        global db;

        query = db.prepare("SELECT c.commentID, c.postID FROM " + TABLE_COMMENTS + " AS c LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID=c.postID WHERE c.commentID=%s AND (c.commenter=%s OR p.poster=%s)", commentID, userID, userID);
        row = db.getRow(query);

        if(!row){
            return false;
        }else{
            cID = row["commentID"];
            postID = row["postID"];

            db.query("DELETE FROM " + TABLE_COMMENTS + " WHERE commentID=" + cID);
            //Remove Activity
            db.query( "DELETE FROM " + TABLE_MAIN_ACTIVITIES + " WHERE actionID=" + cID );
            //Remove From Report
            db.query("DELETE FROM " + TABLE_REPORTS + " WHERE objectType="comment" AND objectID=" + cID);

            //Update comments on the posts table
            query = db.prepare("UPDATE " + TABLE_POSTS + " SET `comments`=`comments` - 1 WHERE postID=%d", postID);
            db.query(query);

            postData = Post.getPostById(postID);
            //Update User Stats
            User.updateStats(postData["poster"], "comments", -1);

            return true;
        }
    }

    /// <summery>
    /// <typeparam name=""></typeparam> commendID
    /// <returns></returns> one
    /// </summery>
    public function getPostID(commendID){
        global db;

        query = db.prepare("SELECT postID FROM " + TABLE_POSTS_COMMENTS + " WHERE commentID=%d", commendID);

        return db.getVar(query);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> commendID
    /// <returns></returns> array
    /// </summery>
    public static function getPost(commendID){
        global db;

        query = db.prepare("SELECT p.* FROM " + TABLE_POSTS_COMMENTS + " AS c LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID=c.postID WHERE c.commentID=%d", commendID);

        return db.getRow(query);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> bool
    /// </summery>
    public function checkUserDailyCommentsLimits(userID){
        global db;

        if(check_user_acl(USER_ACL_MODERATOR) || check_user_acl(USER_ACL_ADMINISTRATOR)){
            return true;
        }

        //Get created posts on today
        query = db.prepare("SELECT count(*) FROM " + TABLE_POSTS_COMMENTS + " WHERE commenter=%d AND DATE(`posted_date`) = %s", userID, date("Y-m-d"));
        comments = db.getVar(query);

        if(comments > USER_DAILY_LIMIT_COMMENTS){
            return false;
        }

        return true;
    }

}
