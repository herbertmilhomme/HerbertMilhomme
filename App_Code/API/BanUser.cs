using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for BanUser
/// </summary>

/// <summery>
/// Manage Album
/// </summery>
class Album {

    /// <summery>
    /// Getting Album Photos
    /// 
    /// <typeparam name=""></typeparam> mixed albumID
    /// <typeparam name=""></typeparam> mixed limit
    /// <returns></returns> Indexed
    /// </summery>
    public static function getPhotos(albumID, limit = null){
        global db;

        query = db.prepare("SELECT p.* FROM " + TABLE_POSTS + " AS p LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS op ON op.post_id=p.postID WHERE op.album_id=%d", albumID);

        rows = db.getResultsArray(query, "postID");

        return rows;

    }

    /// <summery>
    /// Create New Album
    /// 
    /// <typeparam name=""></typeparam> Int    userID
    /// <typeparam name=""></typeparam> String title
    /// <returns></returns> bool|int|null|string
    /// </summery>
    public static function createAlbum(userID, title, visibility){
        global db;

        now = date("Y-m-d H:i:s");
        newId = db.insertFromArray(TABLE_ALBUMS, ["owner" => userID, "name" => title, "created_date" => now, "visibility" => visibility]);

        if(!newId) //Error
        {
            add_message(db.getLastError(), MSG_TYPE_ERROR);
            return false;
        }else{  //Success
            add_message(MSG_NEW_ALBUM_CREATED, MSG_TYPE_SUCCESS);
            return newId;
        }

    }

    /// <summery>
    /// Getting User Albums
    /// 
    /// <typeparam name=""></typeparam> Int userID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getAlbumsByUserId(userID){
        global db;

        query = db.prepare("SELECT a.*, count(ap.id) AS photos FROM " + TABLE_ALBUMS + " AS a LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS ap ON a.albumID=ap.album_id WHERE OWNER=%s GROUP BY a.albumID ORDER BY `name`", userID);
        albums = db.getResultsArray(query, "albumID");

        return albums;
    }

    /// <summery>
    /// Check that userID is a owner of albumID
    /// 
    /// <typeparam name=""></typeparam> int albumID
    /// <typeparam name=""></typeparam> int userID
    /// <returns></returns> bool
    /// </summery>
    public static function checkAlbumOwner(albumID, userID){
        global db;

        query = db.prepare("SELECT albumID FROM " + TABLE_ALBUMS + " WHERE OWNER=%s AND albumID= %s", userID, albumID);
        rs = db.getVar(query);

        return !rs ? false : true;
    }

    /// <summery>
    /// Getting Photo Albums
    /// 
    /// <typeparam name=""></typeparam> Int photoID
    /// <returns></returns> Indexed
    /// </summery>
    public static function getAlbumsByPostId(photoID){
        global db;

        query = db.prepare("SELECT a.* FROM " + TABLE_ALBUMS_PHOTOS + " AS ap LEFT JOIN " + TABLE_ALBUMS + " AS a ON a.albumID=ap.album_id WHERE ap.post_id=%s ORDER BY `name`", photoID);
        albums = db.getResultsArray(query, "albumID");

        return albums;
    }

    /// <summery>
    /// Add photo to album
    /// 
    /// <typeparam name=""></typeparam> mixed albumID
    /// <typeparam name=""></typeparam> mixed photoID
    /// <returns></returns> int|null|string
    /// </summery>
    public static function addPhotoToAlbum(albumID, photoID){
        global db;

        //Remove Old Entries
        query = db.prepare("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE post_id=%s", photoID);
        db.query(query);

        //Insert New Entry
        query = db.prepare("INSERT INTO " + TABLE_ALBUMS_PHOTOS + "(album_id, post_id)VALUES(%s, %s)", albumID, photoID);
        newId = db.insert(query);

        return newId;
    }

    /// <summery>
    /// Remove photo from album
    /// 
    /// <typeparam name=""></typeparam> mixed albumID
    /// <typeparam name=""></typeparam> mixed photoID
    /// </summery>
    public static function removePhotoFromAlbum(albumID, photoID){
        global db;

        //Remove Old Entries
        query = db.prepare("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE album_id=%s AND post_id=%s", albumID, photoID);
        db.query(query);

        return newId;
    }

    /// <summery>
    /// Remove Album
    /// 
    /// <typeparam name=""></typeparam> mixed albumID
    /// <typeparam name=""></typeparam> mixed userID
    /// <returns></returns> bool
    /// </summery>
    public static function deleteAlbum(albumID, userID){
        global db;

        if(Album.checkAlbumOwner(albumID, userID)){
            //Remove Album
            query = db.prepare("DELETE FROM " + TABLE_ALBUMS + " WHERE albumID=%s AND OWNER=%s", albumID, userID);
            db.query(query);
            //Remove Assigned Photos
            query = db.prepare("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE albumID=%s", albumID);
            db.query(query);
            return true;
        }
        return false;
    }

    /// <summery>
    /// Get Album Detail
    /// 
    /// <typeparam name=""></typeparam> int albumID
    /// <returns></returns> array
    /// </summery>
    public static function getAlbum(albumID){
        global db;

        query = db.prepare("SELECT a.*, u.firstName, u.lastName FROM " + TABLE_ALBUMS + " AS a LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=a.owner WHERE a.albumID=%s", albumID);
        row = db.getRow(query);

        return row;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> albumID
    /// <typeparam name=""></typeparam> title
    /// <typeparam name=""></typeparam> visibility
    /// <typeparam name=""></typeparam> photos
    /// </summery>
    public static function updateAlbum(albumID, title, visibility, photos){
        global db;

        //Update Album Title
        query = db.prepare("UPDATE " + TABLE_ALBUMS + " SET name=%s, visibility=%s WHERE albumID=%s", title, visibility, albumID);
        db.query(query);

        return;
    }
}