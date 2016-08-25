using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Tracker
/// </summary>

/// <summery>
/// Manage Tracker Table
/// </summery>
class Tracker {

    //Add current track
    /// <summery>
    /// <typeparam name=""></typeparam> string action
    /// </summery>
    public static function addTrack(action = "login"){
        global db;

        userID = is_logged_in();
        ip = _SERVER["REMOTE_ADDR"];
        time = time();

        if(!empty(_SERVER["HTTP_CLIENT_IP"])){
            ip = _SERVER["HTTP_CLIENT_IP"];
        }elseif(!empty(_SERVER["HTTP_X_FORWARDED_FOR"])){
            ip = _SERVER["HTTP_X_FORWARDED_FOR"];
        }

        if(ip != "127.0.0.1"){
            db.insertFromArray(TABLE_TRACKER, ["userID" => !userID ? 0 : userID, "ipAddr" => ip, "trackedTime" => time, "action" => action]);
        }

        return;
    }

    /// <summery>
    /// <returns></returns> one
    /// </summery>
    public static function getLoginAttemps(){
        global db;

        ip = _SERVER["REMOTE_ADDR"];

        if(!empty(_SERVER["HTTP_CLIENT_IP"])){
            ip = _SERVER["HTTP_CLIENT_IP"];
        }elseif(!empty(_SERVER["HTTP_X_FORWARDED_FOR"])){
            ip = _SERVER["HTTP_X_FORWARDED_FOR"];
        }

        time = time() - MAX_LOGIN_ATTEMPT_PERIOD;
        query = "SELECT COUNT(1) FROM " + TABLE_TRACKER + " WHERE ipAddr="ip" AND trackedTime > "time"";
        count = db.getVar(query);

        return count;
    }

    /// <summery>
    /// 
    /// </summery>
    public static function clearLoginAttemps(){
        global db;

        ip = _SERVER["REMOTE_ADDR"];

        if(!empty(_SERVER["HTTP_CLIENT_IP"])){
            ip = _SERVER["HTTP_CLIENT_IP"];
        }elseif(!empty(_SERVER["HTTP_X_FORWARDED_FOR"])){
            ip = _SERVER["HTTP_X_FORWARDED_FOR"];
        }

        time = time() - MAX_LOGIN_ATTEMPT_PERIOD;
        query = "DELETE FROM " + TABLE_TRACKER + " WHERE ipAddr="ip"";
        //query = "DELETE FROM " + TABLE_TRACKER + " WHERE ipAddr="ip" AND trackedTime > "time"";

        db.query(query);

        return;
    }
}
