using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for ForumFollower
/// </summary>
public class ForumFollower
{
    public ForumFollower()
    {
        //
        // TODO: Add constructor logic here
        //
    }
}


/// <summery>
/// Manage Forum Follower
/// </summery>
class ForumFollower {

    /// <summery>
    /// Make user follow all categories
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> bool
    /// </summery>
    public function followAllForum(userID){
        global db;

        all_categories = db.getResultsArray("SELECT categoryID FROM " + TABLE_FORUM_CATEGORIES + " ORDER BY parentID, sortOrder");

        foreach(all_categories as c){
            query = db.prepare("INSERT INTO " + TABLE_FORUM_FOLLOWERS + "(`userID`, `categoryID`)VALUES(%d, %d)", userID, c["categoryID"]);
            db.query(query);
            query = db.prepare("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `followers`=`followers` + 1 WHERE categoryID=%d", c["categoryID"]);
            db.query(query);
        }

        return true;
    }

    /// <summery>
    /// Make user follow the basic forums
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> bool
    /// </summery>
    public static function followBasicForums(userID){
        global db;

        all_categories = db.getResultsArray("SELECT categoryID FROM " + TABLE_FORUM_CATEGORIES + " WHERE parentID IN (1,2,3,4,5) ORDER BY parentID, sortOrder");

        foreach(all_categories as c){
            query = db.prepare("INSERT INTO " + TABLE_FORUM_FOLLOWERS + "(`userID`, `categoryID`)VALUES(%d, %d)", userID, c["categoryID"]);
            db.query(query);
            query = db.prepare("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `followers`=`followers` + 1 WHERE categoryID=%d", c["categoryID"]);
            db.query(query);
        }

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> bool
    /// </summery>
    public static function followForum(userID, categoryID){
        global db;

        query = db.prepare("INSERT INTO " + TABLE_FORUM_FOLLOWERS + "(`userID`, `categoryID`)VALUES(%d, %d)", userID, categoryID);
        db.query(query);

        //Update Category Followers
        query = db.prepare("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `followers`=`followers` + 1 WHERE categoryID=%d", categoryID);
        db.query(query);

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> bool
    /// </summery>
    public static function unfollowForum(userID, categoryID){
        global db;

        query = db.prepare("DELETE FROM " + TABLE_FORUM_FOLLOWERS + " WHERE `userID`=%d AND `categoryID`=%d", userID, categoryID);
        db.query(query);

        //Update Category Followers
        query = db.prepare("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `followers`=`followers` - 1 WHERE categoryID=%d", categoryID);
        db.query(query);

        //Remove Moderator
        query = db.prepare("DELETE FROM " + TABLE_FORUM_MODERATORS + " WHERE `userID`=%d AND `categoryID`=%d", userID, categoryID);
        db.query(query);

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>      categoryID
    /// <typeparam name=""></typeparam> null userID
    /// <returns></returns> one
    /// </summery>
    public static function isFollow(categoryID, userID = null){
        global db;

        if(!userID)
            userID = is_logged_in();

        query = db.prepare("SELECT id FROM " + TABLE_FORUM_FOLLOWERS + " WHERE categoryID=%d AND userID=%d", categoryID, userID);
        id = db.getVar(query);

        return id;
    }

}