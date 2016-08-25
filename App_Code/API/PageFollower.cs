using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for PageFollower
/// </summary>

/// <summery>
/// Page Followers management
/// </summery>
class PageFollower {

    const COUNT_PER_PAGE = 15;

    /// <summery>
    /// Add followers
    /// 
    /// <typeparam name=""></typeparam> integer pageID
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> int
    /// </summery>
    public function addFollower(pageID, userID){
        global db;

        if(!is_numeric(pageID) || !is_numeric(userID))
            return; // failed

        if(this.hasRelationInFollow(pageID, userID))
            return; // already exists

        pageIns = new Page();
        pageData = pageIns.getPageByID(pageID);

        if(isset(pageData)){
            data = [];
            data["pageID"] = pageID;
            data["userID"] = userID;
            data["createdDate"] = date("Y-m-d H:i:s");
            newID = db.insertFromArray(TABLE_PAGE_FOLLOWERS, data);

            //Update User Stats
            User.updateStats(pageData["userID"], "pageFollowers", 1);

            return newID;
        }else{
            return;
        }

    }

    /// <summery>
    /// Unfollow
    /// 
    /// <typeparam name=""></typeparam> integer pageID
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> int
    /// </summery>
    public function removeFollower(pageID, userID){
        global db;

        if(!is_numeric(pageID) || !is_numeric(userID))
            return; // failed

        if(this.hasRelationInFollow(pageID, userID)){

            query = sprintf("DELETE FROM %s WHERE pageID=%d AND userID=%d", TABLE_PAGE_FOLLOWERS, pageID, userID);
            db.query(query);

            pageData = Page.getPageByID(pageID);

            //Update User Stats
            User.updateStats(pageData["userID"], "pageFollowers", -1);

            return true;
        }

        return;
    }

    /// <summery>
    /// Check relations if it has already followed the page
    /// 
    /// <typeparam name=""></typeparam> integer pageID
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> bool
    /// </summery>
    public function hasRelationInFollow(pageID, userID){

        global db;
        pageIns = new Page();

        if(!is_numeric(pageID) || !is_numeric(userID))
            return false; // failed

        pageData = pageIns.getPageByID(pageID);
        if(pageData["userID"] == userID){
            //It means you are the owner of this page.
            //            return true;
        }

        query = sprintf("SELECT * FROM %s WHERE pageID=%d AND userID=%d", TABLE_PAGE_FOLLOWERS, pageID, userID);

        if(db.getRow(query)){
            return true;
        }else{
            return false;
        }

    }

    /// <summery>
    /// Get followers by PageID
    /// 
    /// <typeparam name=""></typeparam>         pageID
    /// <typeparam name=""></typeparam> integer page
    /// <typeparam name=""></typeparam> integer limit
    /// <typeparam name=""></typeparam> boolean isRand
    /// <returns></returns> Indexed
    /// </summery>
    public function getFollowers(pageID, page = 1, limit = 1, isRand = false){

        global db;

        if(!is_numeric(pageID))
            return;

        randStr = "";
        if(isRand){

            randStr = ", rand() ";
        }

        query = sprintf("SELECT DISTINCT(u.userID), u.*, CONCAT(u.firstName, " ", u.lastName) AS fullName, IF(u.thumbnail = "", 0, 1) AS hasThumbnail
                FROM %s AS pf 
                LEFT JOIN %s AS u ON u.userID = pf.userID 
                WHERE u.status=1 AND pf.pageID=%d ORDER BY hasTHumbnail DESC, pf.createdDate, fullName ASC %s
        ", TABLE_PAGE_FOLLOWERS, TABLE_USERS, pageID, randStr);

        query += " LIMIT " + (page - 1) * limit + ", " + limit;

        rows = db.getResultsArray(query);

        return rows;

    }

    /// <summery>
    /// Remove page followers when removing page
    /// 
    /// <typeparam name=""></typeparam> mixed pageID
    /// </summery>
    public function removeAllFollowersByPageID(pageID){

        global db;

        if(!is_numeric(pageID))
            return;

        //Getting Followers
        query = db.prepare("SELECT userID FROM " + TABLE_PAGES + " WHERE pageID=%d", pageID);
        pageCreatorId = db.getVar(query);

        //Getting Followers
        query = db.prepare("SELECT count(*) FROM " + TABLE_PAGE_FOLLOWERS + " WHERE pageID=%d", pageID);
        followers = db.getVar(query);

        if(followers > 0)
            User.updateStats(pageCreatorId, "pageFollowers", -1 * followers);

        query = sprintf("DELETE FROM %s WHERE pageID=%d", TABLE_PAGE_FOLLOWERS, pageID);
        db.query(query);

        return;
    }

    /// <summery>
    /// Get number of followers
    /// 
    /// <typeparam name=""></typeparam> integer pageID
    /// <returns></returns> int|one
    /// </summery>
    public function getNumberOfFollowers(pageID){

        global db;

        if(!is_numeric(pageID))
            return 0;

        query = sprintf("SELECT count(*) FROM %s WHERE pageID=%d", TABLE_PAGE_FOLLOWERS, pageID);
        return db.getVar(query);

    }

    /// <summery>
    /// Get page list with follower ID, in another words, return pages this user followed
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <typeparam name=""></typeparam> integer limit
    /// <returns></returns> Indexed|void
    /// </summery>
    public function getPagesByFollowerID(userID, page = 1, limit = null){

        global db;

        if(!is_numeric(userID))
            return;

        limitCond = "";
        if(isset(limit) && is_numeric(limit) && limit > 0 && page >= 1){
            limitCond += " LIMIT " + (page - 1) * limit + ", " + limit;
        }

        query = sprintf("SELECT pf.pageID, p.userID, p.title, p.logo, p.about, p.links, p.createdDate, (SELECT COUNT(*) FROM %s AS fcpf WHERE fcpf.pageID=pf.pageID) AS followerCount, IF(p.userID=%s, 0, 1) AS pageOwner FROM %s AS pf LEFT JOIN %s AS p ON p.pageID=pf.pageID WHERE pf.userID=%d AND p.status=%d ORDER BY pageOwner %s", TABLE_PAGE_FOLLOWERS, userID, TABLE_PAGE_FOLLOWERS, TABLE_PAGES, userID, Page.STATUS_ACTIVE, limitCond);

        return db.getResultsArray(query);

    }

    /// <summery>
    /// Get page count by follower ID
    /// 
    /// <typeparam name=""></typeparam> integer userID
    /// <returns></returns> int|one
    /// </summery>
    public function getPagesCountByFollowerID(userID){

        global db;

        if(!is_numeric(userID))
            return 0;

        query = sprintf("SELECT count(pf.pageID) FROM %s AS pf LEFT JOIN %s AS p ON p.pageID=pf.pageID WHERE pf.userID=%d AND p.status=%d %s", TABLE_PAGE_FOLLOWERS, TABLE_PAGES, userID, Page.STATUS_ACTIVE, limitCond);

        return db.getVar(query);

    }

    /// <summery>
    /// Check the user is following the page
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// <typeparam name=""></typeparam> mixed pageID
    /// <returns></returns> bool
    /// </summery>
    public static function isFollower(userID, pageID){
        global db;

        query = db.prepare("SELECT id FROM " + TABLE_PAGE_FOLLOWERS + " WHERE pageID=%d AND userID=%d", pageID, userID);
        fid = db.getVar(query);

        return fid ? true : false;
    }

}