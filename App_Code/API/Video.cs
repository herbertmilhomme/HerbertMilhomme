﻿using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Video
/// </summary>

/// <summery>
/// room Videos
/// </summery>
class Video {

    /// <summery>
    /// <typeparam name=""></typeparam> null subjectID
    /// <typeparam name=""></typeparam> null parentID
    /// <returns></returns> array|Indexed
    /// </summery>
    public function getVideoCategories(subjectID = null, parentID = null){
        global db;

        query = "SELECT * FROM " + TABLE_VIDEO_CATEGORIES;

        if(subjectID !== null)
            query += db.prepare(" WHERE subjectID=%d", subjectID);

        if(parentID)
            query += db.prepare(" WHERE parentID=%d", parentID);

        query += " ORDER BY parentID, categoryName";

        rows = db.getResultsArray(query);

        if(!parentID){
            //Convert results to two dimension
            results = [];
            foreach(rows as row){
                if(!row["parentID"]){
                    results[row["categoryID"]] = ["categoryName" => row["categoryName"], "categories" => []];

                }else{
                    results[row["parentID"]]["categories"][] = row;
                }
            }

            rows = results;
        }

        return rows;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> null categoryID
    /// <returns></returns> Indexed
    /// </summery>
    public function getVideos(categoryID = null){
        global db;

        query = "SELECT * FROM " + TABLE_VIDEOS;
        if(categoryID)
            query += db.prepare(" WHERE categoryID=%d", categoryID);

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> videoID
    /// <returns></returns> array
    /// </summery>
    public function getVideo(videoID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_VIDEOS + " WHERE videoID=%d", videoID);
        row = db.getRow(query);

        return row;

    }

    /// <summery>
    /// <typeparam name=""></typeparam> categoryID
    /// <returns></returns> array
    /// </summery>
    public function getCategory(categoryID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_VIDEO_CATEGORIES + " WHERE categoryID=%d", categoryID);
        row = db.getRow(query);

        return row;

    }

    /// <summery>
    /// <typeparam name=""></typeparam> youtubeID
    /// <returns></returns> mixed
    /// </summery>
    public function getVideoInfo(youtubeID){
        ch = curl_init();
        curl_setopt(ch, CURLOPT_URL, "https://gdata.youtube.com/feeds/api/videos/" + youtubeID + "?v=2&alt=json");
        curl_setopt(ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt(ch, CURLOPT_SSL_VERIFYPEER, false);

        return = curl_exec(ch);
        curl_close(ch);

        returnData = json_decode(return, true);

        return returnData;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> content
    /// <returns></returns> mixed
    /// </summery>
    public function convertVideoDescription(content){
        content = preg_replace("/(https?:\/\/\S*)/i", "<a href="0" targe="_blank">0</a>", content);
        content = str_replace("\n\r", "<br />", content);
        content = str_replace("\n", "<br />", content);

        return content;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> videoID
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> comment
    /// <returns></returns> int|null|string
    /// </summery>
    public function addVideoComment(videoID, userID, comment){
        global db;

        return db.insertFromArray(TABLE_VIDEO_COMMENTS, ["videoID" => videoID, "userID" => userID, "content" => comment, "createdDate" => date("Y-m-d H:i:s")]);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> videoID
    /// <returns></returns> Indexed
    /// </summery>
    public function getVideoComments(videoID){
        global db;

        query = db.prepare("SELECT c.*, u.firstName, u.lastName, r.reportID FROM " + TABLE_VIDEO_COMMENTS + " AS c 
                               LEFT JOIN " + TABLE_USERS + " AS u ON c.userID=u.userID 
                               LEFT JOIN " + TABLE_REPORTS + " AS r ON r.objectID=c.commentID AND r.objectType="video_comment"
                               WHERE c.videoID=%d ORDER BY createdDate DESC", videoID);

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> commentID
    /// <returns></returns> array
    /// </summery>
    public function getVideoComment(commentID){
        global db;

        query = db.prepare("SELECT c.*, u.firstName, u.lastName FROM " + TABLE_VIDEO_COMMENTS + " AS c LEFT JOIN " + TABLE_USERS + " AS u ON c.userID=u.userID WHERE c.commentID=%d", commentID);

        row = db.getRow(query);

        return row;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> commentID
    /// </summery>
    public static function deleteVideoComment(commentID){
        global db;

        query = db.prepare("DELETE FROM " + TABLE_VIDEO_COMMENTS + " WHERE commentID=%d", commentID);

        db.query(query);

    }

    /// <summery>
    /// <typeparam name=""></typeparam> commentID
    /// <returns></returns> one
    /// </summery>
    public static function getVideoIDByCommentID(commentID){
        global db;

        query = db.prepare("SELECT videoID FROM " + TABLE_VIDEO_COMMENTS + " WHERE commentID=%d", commentID);

        return db.getVar(query);
    }

    /// <summery>
    /// <typeparam name=""></typeparam> subjectID
    /// <returns></returns> array
    /// </summery>
    public function getSubject(subjectID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_VIDEO_SUBJECTS + " WHERE subjectID=%d", subjectID);
        row = db.getRow(query);

        return row;

    }

    /// <summery>
    /// <typeparam name=""></typeparam> subjectName
    /// <returns></returns> array
    /// </summery>
    public function getSubjectByName(subjectName){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_VIDEO_SUBJECTS + " WHERE subjectName=%d", subjectName);
        row = db.getRow(query);

        return row;

    }

}