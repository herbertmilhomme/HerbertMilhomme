﻿using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Search
/// </summary>

/// <summery>
/// Search user and Page
/// </summery>
class Search {

    const SEARCH_TYPE_USER = 0;
    const SEARCH_TYPE_PAGE = 1;
    const SEARCH_TYPE_USER_AND_PAGE = 2;

    const SEARCH_RESULT_PER_PAGE = 30;

    /// <summery>
    /// Search users and pages with keyword
    /// 
    /// <typeparam name=""></typeparam> mixed qStr
    /// <typeparam name=""></typeparam> mixed type
    /// <returns></returns> Indexed
    /// </summery>
    public function search(qStr, type = Search.SEARCH_TYPE_USER_AND_PAGE, sort, page = 1, limit = Search.SEARCH_RESULT_PER_PAGE){

        global db;

        query = this._getSearchQuery(qStr, type, sort);

        if(query == "")
            return;

        if(!is_numeric(page) || page < 1)
            page = 1;

        limitCond = sprintf(" LIMIT %d, %d", (page - 1) * limit, limit);

        query += limitCond;

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// get number of search result
    /// 
    /// <typeparam name=""></typeparam> mixed qStr
    /// <typeparam name=""></typeparam> mixed type
    /// <returns></returns> one
    /// </summery>
    public function getNumberOfSearchResult(qStr, type){

        global db;

        query = this._getSearchQuery(qStr, type, "", true);

        return db.getVar(query);

    }

    /// <summery>
    /// Get Search Query
    /// 
    /// <typeparam name=""></typeparam> mixed qStr
    /// <typeparam name=""></typeparam> mixed type
    /// <typeparam name=""></typeparam> mixed isCount
    /// <returns></returns> string
    /// </summery>
    private function _getSearchQuery(qStr, type = Search.SEARCH_TYPE_USER_AND_PAGE, sort, isCount = false){

        qStr = addslashes(qStr);
        type = type == "" ? Search.SEARCH_TYPE_USER_AND_PAGE : type;

        whereUserCondStr = "";
        relavanceUserStr = "";

        wherePageCondStr = "";
        relavancePageStr = "";

        if(qStr != ""){
            whereUserCondStr = sprintf("WHERE MATCH (u.firstName, u.lastName) AGAINST ("%s" IN BOOLEAN MODE) AND u.status=%d", qStr, User.STATUS_USER_ACTIVE);
            relavanceUserStr = sprintf("MATCH (u.firstName, u.lastName) AGAINST ("%s") * 10", qStr);

            wherePageCondStr = sprintf("WHERE MATCH (p.title, p.about) AGAINST ("%s" IN BOOLEAN MODE) AND p.status=%d AND p.title !="%s"", qStr, Page.STATUS_ACTIVE, Page.DEFAULT_PAGE_TITLE);
            relavancePageStr = sprintf("MATCH (p.title, p.about) AGAINST ("%s") * 10", qStr);
        }else{

            whereUserCondStr = sprintf("WHERE u.status=%d", User.STATUS_USER_ACTIVE);
            relavanceUserStr = "0";

            wherePageCondStr = sprintf("WHERE p.status=%d AND p.title !="%s"", Page.STATUS_ACTIVE, Page.DEFAULT_PAGE_TITLE);
            relavancePageStr = "0";

        }

        query = "";
        orderByStr = "";
        sort = strtolower(sort);
        switch(sort){
            case "asc":
                orderByStr = " ORDER BY PPName ASC";
                break;
            case "desc":
                orderByStr = " ORDER BY PPName DESC";
                break;
            case "reputation":
                orderByStr = " ORDER BY reputation DESC";
                break;
            case "pop":
            default:
                orderByStr = " ORDER BY PPFollowers DESC";
                break;
        }

        switch(type){
            case Search.SEARCH_TYPE_USER_AND_PAGE:

                //Search user and page
                if(isCount){
                    selectStr = " COUNT(*) AS Count ";
                }else{
                    selectStr = " up.* ";

                }

                query = sprintf("
                    SELECT %s 
                    FROM 
                        (
                        SELECT u.userID AS userID, "" AS pageID, "user" AS type, %s AS Relevance, CONCAT(u.firstName, " ", u.lastName) AS PPName, (SELECT COUNT(DISTINCT(fri.userFriendID)) FROM %s AS fri INNER JOIN %s AS friUT ON friUT.userID=fri.userFriendID WHERE fri.userID=u.userID AND fri.status=1 AND friUT.status=1) PPFollowers FROM %s AS u %s 
                        UNION ALL  
                        SELECT "", p.pageID, "page", %s, p.title, (SELECT COUNT(*) FROM %s AS pf WHERE pf.pageID=p.pageID) FROM %s AS p %s
                        ) AS up
                   %s
                        
                ", selectStr, relavanceUserStr, TABLE_FRIENDS, TABLE_USERS, TABLE_USERS, whereUserCondStr, relavancePageStr, TABLE_PAGE_FOLLOWERS, TABLE_PAGES, wherePageCondStr, orderByStr);

                break;

            case Search.SEARCH_TYPE_USER:

                //search user only

                if(isCount)
                    query = sprintf("SELECT COUNT(u.userID) AS Count FROM %s AS u %s;", TABLE_USERS, whereUserCondStr);else{

                    query = sprintf("SELECT u.userID AS userID, "" AS pageID, "user" AS type, %s AS Relevance, CONCAT(u.firstName, " ", u.lastName) AS PPName, (SELECT COUNT(DISTINCT(fri.userFriendID)) FROM %s AS fri INNER JOIN %s AS friUT ON friUT.userID=fri.userFriendID WHERE fri.userID=u.userID AND fri.status=1 AND friUT.status=1) PPFollowers, ustats.reputation AS rp FROM %s AS u 
                    LEFT JOIN %s AS ustats ON ustats.userID=u.userID %s %s", relavanceUserStr, TABLE_FRIENDS, TABLE_USERS, TABLE_USERS, TABLE_USERS_STATS, whereUserCondStr, orderByStr);

                }

                break;

            case Search.SEARCH_TYPE_PAGE:

                //search page only
                if(isCount)
                    query = sprintf("SELECT COUNT(p.pageID) AS Count FROM %s AS p %s;", TABLE_PAGES, wherePageCondStr);else{

                    query = sprintf("SELECT "" AS userID, p.pageID AS pageID, "page" AS type, %s AS Relevance, p.title AS PPName, 
                        (SELECT COUNT(*) FROM %s AS pf WHERE pf.pageID=p.pageID) AS PPFollowers 
                        FROM %s AS p 
                        %s 
                        %s", relavancePageStr, TABLE_PAGE_FOLLOWERS, TABLE_PAGES, wherePageCondStr, orderByStr);

                }

                break;

        }

        return query;

    }

}