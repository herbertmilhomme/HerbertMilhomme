using System;
using System.Collections.Generic;
//using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for WhoIsOnline
/// Grabs current user id and stores in Database with time-stamp and page id
/// </summary>
public class WhoIsOnline
{
	public WhoIsOnline(int userid, string localpath, string sessionkey)
	{
        var db = Database.Open("ColonielHeights");
        //int userid = WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserId : 0;
        string sql = @"UPDATE website_whoisonline SET UserID=@0, TimeVisitPage=getdate(), PageUrl='@1' WHERE SessionID='@2'";
        //var username = WebSecurity.IsAuthenticated ? db.QueryValue("SELECT Username FROM UserProfile WHERE userid=@0",WebSecurity.CurrentUserId) : "Guest";
        /*if(){//new
            sql = @"INSERT INTO website_livechat (userid, userName, ipaddress, postmsg) VALUES (@0, @1, @2, @3)";
        }else{
            sql = @"UPDATE website_whoisonline SET UserID=@0,TimeVisitPage=@1,PageUrl=@2 WHERE SessionID=@3";
        }*/
        //db.Execute(sql, userid,Request.Url.LocalPath,Session["SESSION_KEY"]);
        //HttpContext.Current.Session["yourSessionKey"] = value;
        //AppState["TotalOnlineChatters"] = (int)AppState["TotalOnlineChatters"] + 1; 
        //Request.Url.LocalPath on this page

        //db.Execute(sql, userid, Request.Url.LocalPath, Session["SESSION_KEY"]);
        db.Execute(sql, userid, localpath, sessionkey);
    }
}