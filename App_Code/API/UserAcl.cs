using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for UserAcl
/// </summary>

/// <summery>
/// Manage User ACL
/// </summery>
class UserAcl {

    static USER_ACL = null;

    /// <summery>
    /// Define User Acl Constants
    /// It will be called on the bootstrap file

    /// </summery>
    public static function defineAclConstants(){
        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(!defined("USER_ACL_" + strtoupper(row["Name"])))
                define("USER_ACL_" + strtoupper(row["Name"]), row["Level"]);
        }

    }

    /// <summery>
    /// Get ACL data from database and store it to USER_ACL

    /// </summery>
    public static function loadAcl(){
        global db;

        query = "SELECT * FROM " + TABLE_USER_ACL + " ORDER BY LEVEL";
        rows = db.getResultsArray(query);

        UserAcl.USER_ACL = rows;

        return;
    }

    /// <summery>
    /// Get id from level
    /// 
    /// <typeparam name=""></typeparam> level
    /// <returns></returns>
    /// @internal param Int acl
    /// </summery>
    public function getIdFromLevel(level){
        global db;

        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(row["Level"] == level)
                return row["aclID"];
        }

    }

    /// <summery>
    /// Get id from Name
    /// 
    /// <typeparam name=""></typeparam> name
    /// <returns></returns>
    /// @internal param Int acl
    /// </summery>
    public static function getIdFromName(name){
        global db;

        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(strtolower(row["Name"]) == strtolower(name))
                return row["aclID"];
        }

    }

    /// <summery>
    /// Get level from id
    /// 
    /// <typeparam name=""></typeparam> ac_id
    /// <returns></returns>
    /// @internal param Int acl
    /// </summery>
    public function getLevelFromId(ac_id){
        global db;

        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(row["aclID"] == ac_id)
                return row["Level"];
        }

    }

    /// <summery>
    /// Get Level from level
    /// 
    /// <typeparam name=""></typeparam> name
    /// <returns></returns>
    /// @internal param Int acl
    /// </summery>
    public function getLevelFromName(name){
        global db;

        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(strtolower(row["Name"]) == strtolower(name))
                return row["Level"];
        }

    }

    /// <summery>
    /// Get name from level
    /// 
    /// <typeparam name=""></typeparam> level
    /// <returns></returns>
    /// @internal param Int acl
    /// </summery>
    public function getNameFromLevel(level){
        global db;

        if(UserAcl.USER_ACL == null)
            UserAcl.loadAcl();

        foreach(UserAcl.USER_ACL as row){
            if(row["Level"] == level)
                return row["Name"];
        }

    }

}