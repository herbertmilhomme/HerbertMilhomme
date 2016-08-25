using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Friend
/// </summary>

class Friend {

    public static COUNT_PER_PAGE = 15;

    /// <summery>
    /// Getting Friend Request
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> one
    /// </summery>
    public static function getNewFriendRequests(userID){
        global db;

        query = db.prepare("SELECT COUNT(DISTINCT(userID)) FROM " + TABLE_FRIENDS + " WHERE userFriendID=%d  AND `status`=0", userID);
        num = db.getVar(query);

        return num;
    }

    /// <summery>
    /// Getting User Friends
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> array
    /// </summery>
    public function getFriendIDs(userID){
        global db;

        query = db.prepare("SELECT userFriendID FROM " + TABLE_FRIENDS + " WHERE userID=%d AND `status`=1", userID);
        rows = db.getResultsArray(query);

        ids = [];
        foreach(rows as row){
            if(row["user1"] == userID)
                ids[] = row["user2"];else
                ids[] = row["user1"];
        }
        return ids;
    }

    /// <summery>
    /// Getting User Friends
    /// 
    /// <typeparam name=""></typeparam> Int     userID
    /// <typeparam name=""></typeparam> int     limit
    /// <typeparam name=""></typeparam> Boolean isRand
    /// <returns></returns> Indexed
    /// </summery>
    public static function getAllFriends(userID, page = 1, limit = 1, isRand = false){
        global db;

        if(isRand)
            query = db.prepare("SELECT u.*, f.friendID, IF(u.thumbnail = "", 0, 1) AS hasThumbnail FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = f.userFriendID WHERE u.status=1 AND f.userID=%d AND f.status="1" GROUP BY u.userID ORDER BY hasThumbnail DESC, rand() ", userID);else
            query = db.prepare("SELECT u.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, f.friendID, IF(u.thumbnail = "", 0, 1) AS hasThumbnail FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = f.userFriendID WHERE u.status=1 AND f.userID=%d AND f.status=1 GROUP BY u.userID ORDER BY hasTHumbnail DESC, fullName ASC ", userID);

        query += " LIMIT " + (page - 1) * limit + ", " + limit;

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Get Total Count of friends
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> Int
    /// </summery>
    public static function getNumberOfFriends(userID){
        global db;

        query = db.prepare("SELECT count(DISTINCT(f.userFriendID)) FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=f.userFriendID WHERE u.status=1 AND f.userID=%d AND f.status=1", userID);
        count = db.getVar(query);

        return count;
    }

    /// <summery>
    /// Search User Friends
    /// 
    /// <typeparam name=""></typeparam> mixed  userID
    /// <typeparam name=""></typeparam> String term
    /// <returns></returns> Indexed
    /// </summery>
    public static function searchFriends(userID, term){
        global db;

        query = "SELECT DISTINCT(u.userID), u.*, CONCAT(u.firstName, " ", u.lastName) AS fullName FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = f.userFriendID WHERE u.status=1 AND f.userID=" + userID + " AND f.status=1 AND (CONCAT(u.firstName, " ", u.lastName) LIKE "%" + db.escapeInput(term) + "%") ORDER BY fullName";

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Check that the two users is friend
    /// 
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> userFriendID
    /// <returns></returns> one
    /// </summery>
    public static function isFriend(userID, userFriendID){
        global db;

        query = db.prepare("SELECT friendID FROM " + TABLE_FRIENDS + " WHERE userID=%d AND userFriendID=%d AND `status`="1"", userID, userFriendID);
        fid = db.getVar(query);

        return fid;
    }

    /// <summery>
    /// Return True if the userID1 sent a friend request to the userID2
    /// 
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> userFriendID
    /// <returns></returns> one
    /// </summery>
    public static function isSentFriendRequest(userID, userFriendID){
        global db;

        query = db.prepare("SELECT friendID FROM " + TABLE_FRIENDS + " WHERE userID=%d AND userFriendID=%s AND `status`="0"", userID, userFriendID);
        fid = db.getVar(query);

        return fid;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> userFriendID
    /// <returns></returns> string
    /// </summery>
    public function getRelationType(userID, userFriendID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_FRIENDS + " WHERE (userID=%d AND userFriendID=%s) OR (userID=%d AND userFriendID=%s) ", userID, userFriendID, userFriendID, userID);

        rows = db.getResultsArray(query);

        foreach(rows as row){
            if(row["status"] == 1){
                return "friend";
            }else if(row["userID"] == userID){
                return "sent";
            }else if(row["userFriendID"] == userID){
                return "received";
            }
        }

        return "none";

    }

    /// <summery>
    /// Get Pending Request
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <typeparam name=""></typeparam> int page
    /// <returns></returns> Array
    /// </summery>
    public static function getPendingRequests(userID, page = 1){
        global db;

        query = db.prepare("SELECT u.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, f.friendID FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=f.userFriendID WHERE u.status=1 AND f.userID=%d AND f.status="0" ORDER BY fullName ASC", userID);

        query += " LIMIT " + (page - 1) * Friend.COUNT_PER_PAGE + ", " + Message.COUNT_PER_PAGE;

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Get Total Number Of Friends
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> one
    /// </summery>
    public static function getNumberOfPendingRequests(userID){
        global db;

        query = db.prepare("SELECT count(f.friendID) FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=f.userFriendID WHERE u.status=1 AND f.userID=%d AND f.status="0" ", userID);

        count = db.getVar(query);

        return count;
    }

    /// <summery>
    /// Get Received Request
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <typeparam name=""></typeparam> int page
    /// <returns></returns> Array
    /// </summery>
    public static function getReceivedRequests(userID, page = 1){
        global db;

        query = db.prepare("SELECT u.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, f.friendID FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=f.userID WHERE u.status=1 AND f.userFriendID=%d AND f.status="0" ORDER BY fullName ", userID);

        query += " LIMIT " + (page - 1) * Friend.COUNT_PER_PAGE + ", " + Friend.COUNT_PER_PAGE;

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// Get Number of Friend Requests
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> Int
    /// </summery>
    public static function getNumberOfReceivedRequests(userID){
        global db;

        query = db.prepare("SELECT count(f.friendID) FROM " + TABLE_FRIENDS + " AS f LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=f.userID WHERE u.status=1  AND f.userFriendID=%d AND f.status="0"", userID);

        row = db.getVar(query);

        return row;
    }

    /// <summery>
    /// Unfriend
    /// 
    /// <typeparam name=""></typeparam> Int   userID
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool
    /// </summery>
    public static function unfriend(userID, ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        foreach(ids as id){
            query = db.prepare("DELETE FROM " + TABLE_FRIENDS + " WHERE userID=%d AND userFriendID=%d", userID, id);
            db.query(query);
            query = db.prepare("DELETE FROM " + TABLE_FRIENDS + " WHERE userFriendID=%d AND userID=%d", userID, id);
            db.query(query);
        }

        return true;
    }

    /// <summery>
    /// Delete
    /// 
    /// <typeparam name=""></typeparam> Int   userID
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool|SQLite3Result
    /// </summery>
    public static function delete(userID, ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        //Add userID times
        array_push(ids, userID);

        query = db.prepare("DELETE FROM " + TABLE_FRIENDS + " WHERE userFriendID IN (" + implode(", ", array_fill(0, count(ids) - 1, "%d")) + ") AND userID=%d AND STATUS=0", ids);

        return db.query(query);
    }

    /// <summery>
    /// Decline
    /// 
    /// <typeparam name=""></typeparam> Int   userID
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool|SQLite3Result
    /// </summery>
    public static function decline(userID, ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        //Add userID times
        array_push(ids, userID);

        query = db.prepare("DELETE FROM " + TABLE_FRIENDS + "  WHERE userID IN (" + implode(", ", array_fill(0, count(ids) - 1, "%d")) + ") AND userFriendID=%d", ids);
        //        query = db.prepare("UPDATE " + TABLE_FRIENDS + " SET `status`="-1" WHERE friendID IN (" + implode(", ", array_fill(0, count(ids) - 1, "%d")) + ") AND userFriendID=%d", ids);

        return db.query(query);
    }

    /// <summery>
    /// Decline
    /// 
    /// <typeparam name=""></typeparam> Int   userID
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool
    /// </summery>
    public static function accept(userID, ids){
        global db;

        if(!is_array(ids))
            ids = [ids];

        //Add userID times
        array_push(ids, userID);

        //Getting Friend IDs
        query = db.prepare("SELECT friendID, userID FROM " + TABLE_FRIENDS + " WHERE userID IN (" + implode(", ", array_fill(0, count(ids) - 1, "%d")) + ") AND STATUS=0 AND userFriendID=%d", ids);
        frows = db.getResultsArray(query);

        foreach(frows as row){
            query = db.prepare("DELETE FROM " + TABLE_FRIENDS + " WHERE (userID=%d AND userFriendID=%d) OR (userID=%d AND userFriendID=%d)", userID, row["userID"], row["userID"], userID);
            db.query(query);

            db.insertFromArray(TABLE_FRIENDS, ["userID" => row["userID"], "userFriendID" => userID, "status" => 1]);
            db.insertFromArray(TABLE_FRIENDS, ["userID" => userID, "userFriendID" => row["userID"], "status" => 1]);
        }

        return true;
    }

    /// <summery>
    /// Decline
    /// 
    /// <typeparam name=""></typeparam> Int   userID
    /// <typeparam name=""></typeparam> Array ids
    /// <returns></returns> bool
    /// </summery>
    public static function sendFriendRequest(userID, id){
        global db;

        query = db.prepare("INSERT INTO " + TABLE_FRIENDS + "(userID, userFriendID, `status`)VALUES(%d, %d, "0")", userID, id);
        db.query(query);

        UsersDailyActivity.addFrendRequest(userID);

        return true;
    }

    /// <summery>
    /// Get Friend Row Details
    /// 
    /// <typeparam name=""></typeparam> mixed friendID
    /// <returns></returns> stdClass
    /// </summery>
    public function getFriendRow(friendID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_FRIENDS + " WHERE friendID=%d", friendID);
        return db.getRow(query);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> bool
    /// </summery>
    public function checkUserDailyFriendRequestsLimits(userID){
        global db;

        if(check_user_acl(USER_ACL_MODERATOR) || check_user_acl(USER_ACL_ADMINISTRATOR)){
            return true;
        }

        //Get created posts on today
        query = db.prepare("SELECT count(*) FROM " + TABLE_FRIENDS + " WHERE userID=%d AND `status`=0 AND DATE(`created_date`) = %s", userID, date("Y-m-d"));
        comments = db.getVar(query);

        if(comments > USER_DAILY_LIMIT_COMMENTS){
            return false;
        }

        return true;
    }
}