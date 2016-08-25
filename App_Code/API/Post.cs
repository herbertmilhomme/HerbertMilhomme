using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Post
/// </summary>

/// <summery>
/// Post Manage Class
/// </summery>
class Post{

	public static post_per_page = 5;
	public static images_per_page = 30;
	public static IMAGES_PER_PAGE_FOR_MANAGE_PHOTOS_PAGE = 15;
	public static COUNT_PER_PAGE_TEXT = 30;
	public static COUNT_PER_PAGE_IMAGE = 60;
	public static COUNT_PER_PAGE_VIDEO = 40;

	// When this post doesn"t belong to a page, then the post"s page ID will be 0;
	const INDEPENDENT_POST_PAGE_ID = 0;

	/// <summery>
	/// Getting all posts that were published by the user"s friends
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <typeparam name=""></typeparam> null lastDate
	/// <returns></returns> Array
	/// </summery>
	public static function getUserPostsStream(userID, lastDate = null){
		global db;

		userID = intval(userID);

		//Page Limit Query
		limit_query = " LIMIT " + Post.post_per_page;

		query = db.prepare("SELECT p.* FROM " + TABLE_POSTS + " AS p
             WHERE (
                p.poster = %d 
                OR
                p.poster IN (SELECT userFriendID FROM " + TABLE_FRIENDS + " WHERE userID=%d AND STATUS=1)
                OR
                p.profileID IN (SELECT userFriendID FROM " + TABLE_FRIENDS + " WHERE userID=%d AND STATUS=1)
             ) AND (
                p.pageID=" + Post.INDEPENDENT_POST_PAGE_ID + "
                OR 
                p.pageID IN (SELECT pageID FROM " + TABLE_PAGE_FOLLOWERS + " WHERE userID=%d)
             ) " + (lastDate != null ? " AND p.post_date < "" + lastDate + """ : "") + " ORDER BY p.post_date DESC " + limit_query, userID, userID, userID, userID);

		rows = db.getResultsArray(query);

		foreach(rows as idx => row){

			//Getting full name
			query = "SELECT firstName, lastName FROM " + TABLE_USERS + " WHERE userID=" + row["poster"];
			urow = db.getRow(query);
			rows[idx]["posterFullName"] = urow["firstName"] + " " + urow["lastName"];

			//Getting Liked
			query = "SELECT likeID FROM " + TABLE_POSTS_LIKES + " WHERE postID=" + row["postID"] + " AND userID=" + userID;
			liked = db.getVar(query);
			rows[idx]["likeID"] = liked;

			//Getting Reported
			query = db.prepare("SELECT reportID FROM " + TABLE_REPORTS + " WHERE reporterID=%d AND objectID=%d AND objectType="post"", userID, row["postID"]);
			reportID = db.getVar(query);
			rows[idx]["reportID"] = reportID;

		}
		return rows;
	}

	/// <summery>
	/// Get Posts Or Post
	/// 
	/// <typeparam name=""></typeparam> integer userID : Poster
	/// <typeparam name=""></typeparam> integer loggedUserID : viewer
	/// <typeparam name=""></typeparam> int pageID
	/// <typeparam name=""></typeparam> boolean canViewPrivate
	/// <typeparam name=""></typeparam> integer postID
	/// <typeparam name=""></typeparam> null lastDate
	/// <typeparam name=""></typeparam> string postType
	/// <returns></returns> Indexed
	/// </summery>
	public static function getPostsByUserID(userID, loggedUserID = null, pageID = Post.INDEPENDENT_POST_PAGE_ID, canViewPrivate = false, postID = null, lastDate = null, postType = "user"){
		global db;

		limit = Post.post_per_page;

		//Getting Page Parameter
		if(isset(_GET["page"]) && not_null(_GET["page"]))
			page = intval(_GET["page"]);else
			page = 1;

		//Page Limit Query
		limit_query = " LIMIT " + ((page - 1) * limit) + ", " + limit;

		if(not_null(loggedUserID)) //Get All posts that were posted by userID
		{
			query = db.prepare("SELECT p.*, CONCAT(u.firstName, " ", u.lastName) AS posterFullName, pl.likeID, r.reportID FROM " + TABLE_POSTS + " AS p
                                LEFT JOIN " + TABLE_USERS + " AS u ON p.poster = u.userID
                                LEFT JOIN " + TABLE_POSTS_LIKES + " AS pl ON pl.postID = p.postID AND pl.userID = %d
                                LEFT JOIN " + TABLE_REPORTS + " AS r ON r.objectType="post" AND r.objectID=p.postID AND r.reporterID= %d
                                WHERE p.pageID=%d", loggedUserID, loggedUserID, pageID);
			if(!canViewPrivate)
				query += " AND p.visibility=1 ";
		}else{ //Get Only Public Posts
			query = db.prepare("SELECT p.*, CONCAT(u.firstName, " ", u.lastName) AS posterFullName, 0 AS likedID, 0 AS reportID FROM " + TABLE_POSTS + " AS p
                                LEFT JOIN " + TABLE_USERS + " AS u ON p.poster = u.userID
                                WHERE p.visibility=1 AND p.pageID=%d", pageID);
		}

		if(!check_user_acl(USER_ACL_ADMINISTRATOR, loggedUserID))
			query += " AND p.post_status=1 ";

		//If Post ID is set, get only one post
		if(postID != null)
			query += db.prepare(" AND p.postID=%d", postID);

		if(lastDate != null){
			lastDate = date("Y-m-d H:i:s", strtotime(lastDate));
			query += " AND p.post_date < "" + lastDate + """;
		}

		if(postType == "user"){
			query += db.prepare(" AND p.poster=%d ", userID);
		}else if(postType == "friends"){
			query += db.prepare(" AND p.profileID=%d ", userID);
		}else{
			query += db.prepare(" AND (p.poster=%d OR p.profileID=%d)", userID, userID);
		}

		query += " ORDER BY p.post_date DESC " + limit_query;
		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Get photos
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <typeparam name=""></typeparam> int loggedUserID
	/// <typeparam name=""></typeparam> int pageID
	/// <typeparam name=""></typeparam> boolean canViewPrivate
	/// <typeparam name=""></typeparam> int postID
	/// <typeparam name=""></typeparam> int albumID
	/// <typeparam name=""></typeparam> int limit
	/// <typeparam name=""></typeparam> string lastDate
	/// <returns></returns> Indexed
	/// </summery>
	public static function getPhotosByUserID(userID, loggedUserID = null, pageID = Post.INDEPENDENT_POST_PAGE_ID, canViewPrivate = false, postID = null, albumID = null, limit = null, lastDate = null){
		global db;

		userID = intval(userID);

		//Getting Page Parameter
		if(isset(_GET["page"]) && not_null(_GET["page"]))
			page = intval(_GET["page"]);else
			page = 1;

		//Page Limit Query
		if(limit)
			limit_query = " LIMIT " + ((page - 1) * limit) + ", " + limit;

		if(not_null(loggedUserID) && canViewPrivate){
			//Get All posts that were posted by userID
			query = "SELECT p.*, CONCAT(u.firstName, " ", u.lastName) AS posterFullName, pl.likeID, pa.album_id FROM " + TABLE_POSTS + " AS p
                                LEFT JOIN " + TABLE_USERS + " AS u ON p.poster = u.userID
                                LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS pa ON pa.post_id = p.postID
                                LEFT JOIN " + TABLE_POSTS_LIKES + " AS pl ON pl.postID = p.postID AND pl.userID = " + userID + "
                                WHERE p.poster= " + userID + " AND p.pageID=" + pageID;
		}else{
			//Get Only Public Posts
			query = "SELECT p.*, CONCAT(u.firstName, " ", u.lastName) AS posterFullName, pl.likeID, pa.album_id FROM " + TABLE_POSTS + " AS p
                                LEFT JOIN " + TABLE_USERS + " AS u ON p.poster = u.userID
                                LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS pa ON pa.post_id = p.postID
                                LEFT JOIN " + TABLE_POSTS_LIKES + " AS pl ON pl.postID = p.postID AND pl.userID = " + userID + "
                                WHERE p.poster= " + userID + " AND p.visibility=1 " + " AND p.pageID=" + pageID;
		}
		query += " AND p.type="image" ";

		//If postID is set, get only one post
		if(postID != null)
			query += db.prepare(" AND p.postID=%d", postID);

		//AlbumID Query
		if(albumID != null){
			aPhotos = Album.getPhotos(albumID);

			apIds = [0];
			foreach(aPhotos as a)
				apIds[] = a["postID"];
			query += " AND p.postID in (" + implode(", ", apIds) + ")";
		}

		if(lastDate != null){
			lastDate = date("Y-m-d H:i:s", strtotime(lastDate));
			query += " AND p.post_date < "" + lastDate + """;
		}

		query += " ORDER BY p.post_date DESC " + limit_query;
		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Get Number of photos
	/// 
	/// <typeparam name=""></typeparam> integer profileID
	/// <typeparam name=""></typeparam> integer pageID
	/// <typeparam name=""></typeparam> integer albumID
	/// <returns></returns> one
	/// </summery>
	public static function getNumberOfPhotosByUserID(profileID, pageID = Post.INDEPENDENT_POST_PAGE_ID, albumID = null){
		global db;

		userID = is_logged_in();

		if(not_null(userID) && (userID == profileID || Friend.isFriend(profileID, userID))){
			query = db.prepare("SELECT count(DISTINCT(p.postID)) FROM " + TABLE_POSTS + " AS p LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS pa ON pa.post_id = p.postID WHERE p.type="image" AND p.poster=%d AND pageID=%d", profileID, pageID);
		}else{
			query = db.prepare("SELECT count(DISTINCT(p.postID)) FROM " + TABLE_POSTS + " AS p LEFT JOIN " + TABLE_ALBUMS_PHOTOS + " AS pa ON pa.post_id = p.postID WHERE p.type="image" AND p.poster=%d AND p.visibility=1 AND pageID=%d", profileID, pageID);
		}

		if(not_null(albumID))
			query += db.prepare(" AND pa.album_id=%d", albumID);

		count = db.getVar(query);

		return count;
	}

	/// <summery>
	/// Check that the postID and PosterID are correct
	/// 
	/// <typeparam name=""></typeparam> Int postID
	/// <typeparam name=""></typeparam> Int posterID
	/// <returns></returns> bool
	/// </summery>
	public static function checkPostID(postID, posterID = null){
		global db;

		if(posterID == null)
			query = db.prepare("SELECT postID FROM " + TABLE_POSTS + " WHERE postID=%s", postID);else
			query = db.prepare("SELECT postID FROM " + TABLE_POSTS + " WHERE postID=%s AND poster=%s", postID, posterID);

		rs = db.getVar(query);

		return rs ? true : false;
	}

	/// <summery>
	/// Save Post
	/// 
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> array data
	/// <returns></returns> bool|int|null|string
	/// </summery>
	public static function savePost(userID, data){
		global db;

		now = date("Y-m-d H:i:s");
		type = isset(data["type"]) ? data["type"] : "text";

		if(!in_array(type, ["text", "image", "video"])){
			type = "text";
		}

		data["pageID"] = isset(data["pageID"]) && is_numeric(data["pageID"]) ? data["pageID"] : Post.INDEPENDENT_POST_PAGE_ID;

		if(data["pageID"] != Post.INDEPENDENT_POST_PAGE_ID && !check_id_encrypted(data["pageID"], data["pageIDHash"])){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}

		if(isset(data["profileID"]) && !Friend.isFriend(userID, data["profileID"])){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}

		if(!isset(data["profileID"]))
			data["profileID"] = 0;

		if(!UsersDailyActivity.checkUserDailyLimit(userID, "posts")){
			add_message(sprintf(MSG_DAILY_POSTS_LIMIT_EXCEED_ERROR, USER_DAILY_LIMIT_POSTS), MSG_TYPE_ERROR);
			return false;
		}

		if(type == "text"){
			if(trim(data["content"]) == ""){
				add_message(MSG_CONTENT_IS_EMPTY, MSG_TYPE_ERROR);
				return false;
			}

			// Strip tags, and change url to clickable
			data["content"] = strip_tags(data["content"]);
			newId = db.insertFromArray(TABLE_POSTS, ["poster" => userID, "pageID" => data["pageID"], "profileID" => data["profileID"], "content" => data["content"], "type" => type, "youtube_url" => "", "post_date" => now, "visibility" => data["post_visibility"]]);

			if(!newId){
				add_message(db.getLastError(), MSG_TYPE_ERROR);
				return false;
			}
		}else if(type == "video"){
			//Check Youtube URL is Valid or Not
			if(!validate_youtube_url(data["youtube_url"])){
				add_message(MSG_INVALID_YOUTUBE_URL, MSG_TYPE_ERROR);
				return false;
			}
			newId = db.insertFromArray(TABLE_POSTS, ["poster" => userID, "pageID" => data["pageID"], "profileID" => data["profileID"], "content" => data["content"], "type" => type, "youtube_url" => data["youtube_url"], "post_date" => now, "visibility" => data["post_visibility"]]);
			if(!newId){
				add_message(db.getLastError(), MSG_TYPE_ERROR);
				return false;
			}
		}else if(type == "image"){
			newId = Post.savePhoto(userID, data);
		}

		if(!isset(newId)){
			return false;
		}

		switch(type){
			case "image":
				// No message
				break;
			case "video":
				add_message(MSG_NEW_VIDEO_CREATED, MSG_TYPE_SUCCESS);
				break;
			case "text":
				add_message(MSG_NEW_POST_CREATED, MSG_TYPE_SUCCESS);
				break;
			default:
				break;
		}

		UsersDailyActivity.addPost(userID);

		return newId;
	}

	/// <summery>
	/// Remove Post and Comment
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <typeparam name=""></typeparam> mixed postID
	/// <returns></returns> bool
	/// </summery>
	public static function deletePost(userID, postID){
		global db;

		query = db.prepare("SELECT postID, type, image, poster FROM " + TABLE_POSTS + " WHERE postID=%s AND poster=%s", postID, userID);
		row = db.getRow(query);

		if(row){
			//Getting Comments and Likes
			comments = db.getVar("SELECT count(*) FROM " + TABLE_POSTS_COMMENTS + " WHERE postID=" + row["postID"]);
			likes = db.getVar("SELECT count(*) FROM " + TABLE_POSTS_LIKES + " WHERE postID=" + row["postID"]);
			//Update Stats
			User.updateStats(row["poster"], "comments", -1 * comments);
			User.updateStats(row["poster"], "likes", -1 * likes);

			db.query("DELETE FROM " + TABLE_POSTS + " WHERE postID=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_COMMENTS + " WHERE postID=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE post_id=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_MAIN_ACTIVITIES + " WHERE objectID=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_REPORTS + " WHERE objectID=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_POSTS_LIKES + " WHERE postID=" + row["postID"]);
			db.query("DELETE FROM " + TABLE_POSTS_HITS + " WHERE postID=" + row["postID"]);

			//Remove Image
			if(row["type"] == "image"){
				@unlink(DIR_FS_PHOTO + "users/" + userID + "/resized/" + row["image"]);
				@unlink(DIR_FS_PHOTO + "users/" + userID + "/original/" + row["image"]);
				@unlink(DIR_FS_PHOTO + "users/" + userID + "/thumbnail/" + row["image"]);

				//Remove From Albums
				db.query("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE post_id=" + row["postID"]);
				user = User.getUserData(userID);

				//If current image is a profile image, remove it from the profile image
				if(user["thumbnail"] == row["image"]){
					User.updateUserFields(userID, ["thumbnail" => ""]);
				}
			}
			return true;
		}else{
			return false;
		}
	}

	/// <summery>
	/// Save Post
	/// 
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> mixed data
	/// <returns></returns> bool|int|null|string
	/// </summery>
	public static function savePhoto(userID, data){
		global db, GLOBALS;

		//Check the Photo File Name
		if(!isset(data["file"]) || strpos(data["file"], "../") !== false || !file_exists(DIR_FS_PHOTO_TMP + data["file"])){
			add_message(MSG_FILE_UPLOAD_ERROR, MSG_TYPE_ERROR);
			return false;
		}

		data["pageID"] = isset(data["pageID"]) && is_numeric(data["pageID"]) ? data["pageID"] : Post.INDEPENDENT_POST_PAGE_ID;

		// Validate the file type
		fileParts = pathinfo(data["file"]);
		if(!in_array(strtolower(fileParts["extension"]), GLOBALS["imageTypes"])){
			add_message(MSG_INVALID_PHOTO_TYPE, MSG_TYPE_ERROR);
			return false;
		}

		//Validate File Size
		list(width, height, type, attr) = getimagesize(DIR_FS_PHOTO_TMP + data["file"]);
		if(width * height > MAX_IMAGE_WIDTH * MAX_IMAGE_HEIGHT){
			add_message(MSG_PHOTO_MAX_SIZE_ERROR, MSG_TYPE_ERROR);
			return false;
		}

		//Checking File Size and move it from the tmp folder to the user photo folder and resize it.
		if(data["post_visibility"] == 2){
			//Calc Ratio using real image width
			ratio = floatval(width / data["width"]);
			sourceWidth = (data["x2"] - data["x1"]) * ratio;

			Post.moveFileFromTmpToUserFolder(userID, data["file"], PROFILE_IMAGE_WIDTH, PROFILE_IMAGE_HEIGHT, data["x1"] * ratio, data["y1"] * ratio, sourceWidth, sourceWidth);
			if(data["pageID"] == Post.INDEPENDENT_POST_PAGE_ID){
				//Update User Profile Field
				User.updateUserFields(userID, ["thumbnail" => data["file"]]);
				is_profile = 1;
			}else{
				//Update Page Profile field
				pageIns = new Page();
				pageIns.updateData(data["pageID"], ["logo" => data["file"]]);
				is_profile = 1;
			}

		}else{
			if(width > MAX_POST_IMAGE_WIDTH){
				height = height * (MAX_POST_IMAGE_WIDTH / width);
				width = MAX_POST_IMAGE_WIDTH;
			}
			if(height > MAX_POST_IMAGE_HEIGHT){
				width = width * (MAX_POST_IMAGE_HEIGHT / height);
				height = MAX_POST_IMAGE_HEIGHT;
			}

			//Create normal image
			Post.moveFileFromTmpToUserFolder(userID, data["file"], width, height, 0, 0);
			is_profile = 0;
		}

		now = date("Y-m-d H:i:s");

		newId = db.insertFromArray(TABLE_POSTS, ["poster" => userID, "pageID" => data["pageID"], "profileID" => data["profileID"], "content" => data["content"], "type" => "image", "post_date" => now, "image" => data["file"], "visibility" => data["post_visibility"] > 0 ? 1 : 0, "is_profile" => is_profile]);
		if(!newId){
			add_message(db.getLastError(), MSG_TYPE_ERROR);
			return false;
		}

		//Assign Photo to Album
		if(isset(data["album"]) && data["album"] != ""){
			if(!Album.checkAlbumOwner(data["album"], userID)){
				add_message(MSG_INVALID_ALBUM_ID, MSG_TYPE_ERROR);
			}else{
				Album.addPhotoToAlbum(data["album"], newId);
			}

		}
		add_message(MSG_PHOTO_UPLOADED_SUCCESSFULLY);

		return newId;
	}

	/// <summery>
	/// Create Profile image using already uploaded images
	/// 
	/// <typeparam name=""></typeparam> Array photo
	/// <typeparam name=""></typeparam> Array data
	/// </summery>
	public static function createProfileImage(photo, data){
		global db;

		orgFile = DIR_FS_PHOTO + "users/" + photo["poster"] + "/original/" + photo["image"];
		targetFile = DIR_FS_PHOTO + "users/" + photo["poster"] + "/resized/" + photo["image"];

		list(width, height, type, attr) = getimagesize(orgFile);

		//Calc Ratio using real image width
		ratio = floatval(width / 576);
		sourceWidth = (data["x2"] - data["x1"]) * ratio;

		Post.resizeImage(photo["poster"], photo["image"], PROFILE_IMAGE_WIDTH, PROFILE_IMAGE_HEIGHT, data["x1"] * ratio, data["y1"] * ratio, sourceWidth, sourceWidth);

		db.updateFromArray(TABLE_POSTS, ["is_profile" => 1], ["postID" => photo["postID"]]);

	}

	/// <summery>
	/// Move uploaded file to the user folder from the tmp folder
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> file
	/// <typeparam name=""></typeparam> targetWidth
	/// <typeparam name=""></typeparam> targetHeight
	/// <typeparam name=""></typeparam> null sourceX
	/// <typeparam name=""></typeparam> null sourceY
	/// <typeparam name=""></typeparam> null sourceWidth
	/// <typeparam name=""></typeparam> null sourceHeight
	/// </summery>
	public static function moveFileFromTmpToUserFolder(userID, file, targetWidth, targetHeight, sourceX = null, sourceY = null, sourceWidth = null, sourceHeight = null){
		dir = DIR_FS_PHOTO + "users";
		if(!is_dir(dir)){
			mkdir(dir, 0777);
			fp = fopen(dir + "/index.html", "w");
			fclose(fp);
		}

		dir = dir + "/" + userID;
		if(!is_dir(dir)){
			mkdir(dir, 0777);
			fp = fopen(dir + "/index.html", "w");
			fclose(fp);
		}

		dir_org = dir + "/original";
		if(!is_dir(dir_org)){
			mkdir(dir_org, 0777);
			fp = fopen(dir_org + "/index.html", "w");
			fclose(fp);
		}

		dir_resized = dir + "/resized";
		if(!is_dir(dir_resized)){
			mkdir(dir_resized, 0777);
			fp = fopen(dir_resized + "/index.html", "w");
			fclose(fp);
		}

		dir_thumbnail = dir + "/thumbnail";
		if(!is_dir(dir_thumbnail)){
			mkdir(dir_thumbnail, 0777);
			fp = fopen(dir_thumbnail + "/index.html", "w");
			fclose(fp);
		}

		// Move File to the original folder
		fp1 = fopen(DIR_FS_PHOTO_TMP + file, "r");
		fp2 = fopen(dir_org + "/" + file, "w");
		buff = fread(fp1, filesize(DIR_FS_PHOTO_TMP + file));
		fwrite(fp2, buff);
		fclose(fp1);
		fclose(fp2);

		// Remove Tmp File
		@unlink(DIR_FS_PHOTO_TMP + file);

		// Resize The Image
		Post.resizeImage(userID, file, targetWidth, targetHeight, sourceX, sourceY, sourceWidth, sourceHeight);
	}

	/// <summery>
	/// Resize Image and create the file on the resized folder
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> String file
	/// <typeparam name=""></typeparam>        destWidth
	/// <typeparam name=""></typeparam>        destHeight
	/// <typeparam name=""></typeparam> null sourceX
	/// <typeparam name=""></typeparam> null sourceY
	/// <typeparam name=""></typeparam> null sourceWidth
	/// <typeparam name=""></typeparam> null sourceHeight
	/// <returns></returns> Resized File name
	/// @internal param Int width
	/// @internal param Int height
	/// </summery>
	public function resizeImage(userID, file, destWidth, destHeight, sourceX = null, sourceY = null, sourceWidth = null, sourceHeight = null){

		// Get the image size for the current original photo
		list(currentWidth, currentHeight, destType) = getimagesize(DIR_FS_PHOTO + "users/" + userID + "/original/" + file);
		destType = image_type_to_mime_type(destType);

		// Find the correct x/y offset and source width/height. Crop the image squarely, at the center.
		if(!sourceWidth)
			sourceWidth = currentWidth;
		if(!sourceHeight)
			sourceHeight = currentHeight;

		//Create Thumbnail;
		Post.createThumbnail(userID, file);

		destPath = DIR_FS_PHOTO + "users/" + userID + "/resized/" + file;
		return resize_image(DIR_FS_PHOTO + "users/" + userID + "/original/" + file, destPath, destType, destWidth, destHeight, sourceX, sourceY, sourceWidth, sourceHeight);
	}

	/// <summery>
	/// Create thumbnail
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <typeparam name=""></typeparam> mixed file
	/// </summery>
	public function createThumbnail(userID, file){
		list(currentWidth, currentHeight, destType) = getimagesize(DIR_FS_PHOTO + "users/" + userID + "/original/" + file);
		destType = image_type_to_mime_type(destType);

		if(currentWidth == currentHeight){
			sourceX = 0;
			sourceY = 0;
		}else if(currentWidth > currentHeight){
			sourceX = intval((currentWidth - currentHeight) / 2);
			sourceY = 0;
			currentWidth = currentHeight;
		}else{
			sourceX = 0;
			sourceY = intval((currentHeight - currentWidth) / 2);
			currentHeight = currentWidth;
		}

		destPath = DIR_FS_PHOTO + "users/" + userID + "/thumbnail/" + file;
		resize_image(DIR_FS_PHOTO + "users/" + userID + "/original/" + file, destPath, destType, IMAGE_THUMBNAIL_WIDTH, IMAGE_THUMBNAIL_HEIGHT, sourceX, sourceY, currentWidth, currentHeight);
	}

	/// <summery>
	/// Get Post By Id
	/// 
	/// <typeparam name=""></typeparam>      id
	/// <typeparam name=""></typeparam> null pageID
	/// <returns></returns> array
	/// </summery>
	public static function getPostById(id, pageID = null){
		global db;

		if(pageID){
			query = db.prepare("SELECT * FROM " + TABLE_POSTS + " WHERE postID=%d AND pageID=%d", id, pageID);
		}else{
			query = db.prepare("SELECT * FROM " + TABLE_POSTS + " WHERE postID=%d", id);
		}

		row = db.getRow(query);

		return row;
	}

	/// <summery>
	/// Update Photo Data
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool
	/// </summery>
	public static function updatePhoto(userID, data){
		global db;

		photo = Post.getPostById(data["photoID"]);

		//Update Content And visibility
		db.updateFromArray(TABLE_POSTS, ["content" => data["content"], "visibility" => !data["photo_visibility"] ? 0 : 1], ["postID" => data["photoID"]]);

		return true;
	}

	/// <summery>
	/// Like Post
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <typeparam name=""></typeparam> int postID
	/// <typeparam name=""></typeparam> action
	/// <typeparam name=""></typeparam> bool checkToken
	/// <returns></returns> bool|int|null|string
	/// </summery>
	public static function likePost(userID, postID, action, checkToken = true){
		global db;

		post = Post.getPostById(postID);

		if(checkToken && !check_form_token("request")){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}

		if(!post || post["poster"] == userID){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}

		if(post["visibility"] == 0 && !Friend.isFriend(userID, post["poster"])){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}

		if(!UsersDailyActivity.checkUserDailyLimit(userID, "likes")){
			add_message(sprintf(MSG_DAILY_LIKES_LIMIT_EXCEED_ERROR, USER_DAILY_LIMIT_LIKES), MSG_TYPE_ERROR);
			return false;
		}

		//Check already like it or not
		query = db.prepare("SELECT likeID FROM " + TABLE_POSTS_LIKES + " WHERE userID=%s AND postID=%s", userID, postID);
		likeId = db.getVar(query);

		if(action == "likePost"){
			if(likeId){
				add_message(MSG_ALREADY_LIKED_POST, MSG_TYPE_ERROR);
				return false;
			}

			UsersDailyActivity.addLikes(userID);

			//Like This post
			rs = db.insertFromArray(TABLE_POSTS_LIKES, ["userID" => userID, "postID" => postID]);

			//Update likes on the posts table
			query = db.prepare("UPDATE " + TABLE_POSTS + " SET `likes`=`likes` + 1 WHERE postID=%d", postID);
			db.query(query);

			//Add Activity
			activityId = Activity.addActivity(userID, postID, "post", "like", rs);

			//Add Notification
			Activity.addNotification(post["poster"], activityId, Activity.NOTIFICATION_TYPE_LIKE_POST);

			//Increase Hits
			Hit.addHit(postID, userID);

			//Update User Stats
			User.updateStats(post["poster"], "likes", 1);
			return rs;
		}else if(action == "unlikePost"){
			if(!likeId){
				add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
				return false;
			}

			UsersDailyActivity.addLikes(userID);

			query = db.prepare("DELETE FROM " + TABLE_POSTS_LIKES + " WHERE userID=%s AND postID=%s", userID, postID);
			db.query(query);

			//Update likes on the posts table
			query = db.prepare("UPDATE " + TABLE_POSTS + " SET `likes`=`likes` - 1 WHERE postID=%d", postID);
			db.query(query);

			//Increase Hits
			Hit.removeHit(postID, userID);

			//Update User Stats
			User.updateStats(post["poster"], "likes", -1);

			return true;
		}
	}

	/// <summery>
	/// Post Likes count
	/// 
	/// <typeparam name=""></typeparam> integer postID
	/// <returns></returns> one
	/// </summery>
	public static function getPostLikesCount(postID){
		global db;

		query = db.prepare("SELECT likes FROM " + TABLE_POSTS + " WHERE postID=%d", postID);
		count = db.getVar(query);

		return count;
	}

	/// <summery>
	/// Get Top Posts or Images
	/// 
	/// <typeparam name=""></typeparam> String period
	/// <typeparam name=""></typeparam> String type
	/// <returns></returns> Indexed
	/// </summery>
	public static function getTopPosts(period = "today", type = "text", page = 1, limit = null){
		global db;

		if(limit == null)
			limit = Post.{COUNT_PER_PAGE + strtoupper("_type")};

		limit = (page - 1) * limit + ", " + limit;

		switch(period){
			case "today":
				query = "
                    SELECT DISTINCT(p.postID), p.pageID, p.content, p.image, p.comments, p.post_date, p.is_profile, page.title AS pageTitle, page.logo AS pageLogo, page.userID AS pageOwnerID, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
					LEFT JOIN " + TABLE_PAGES + " AS page ON page.pageID = p.pageID                    
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date > "" + date("Y-m-d 00:00:00") + ""                    
                    ORDER BY p.likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
			case "this-week":
				cw = date("w");
				sDate = date("Y-m-d 00:00:00", time() - cw * 60 * 60 * 24);
				query = "
                    SELECT p.postID, p.pageID, p.content, p.image, p.post_date, p.comments, p.is_profile, page.title AS pageTitle, page.logo AS pageLogo, page.userID AS pageOwnerID, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster   
					LEFT JOIN " + TABLE_PAGES + " AS page ON page.pageID = p.pageID
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date > "" + sDate + ""
                    GROUP BY p.postID
                    ORDER BY likes DESC, p.post_date
                    LIMIT limit
                ";

				break;
			case "this-month":
				sDate = date("Y-m-01 00:00:00");
				query = "
                    SELECT p.postID, p.pageID, p.content, p.image, p.post_date, p.comments, p.is_profile, page.title AS pageTitle, page.logo AS pageLogo, page.userID AS pageOwnerID, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
					LEFT JOIN " + TABLE_PAGES + " AS page ON page.pageID = p.pageID					
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date > "" + sDate + ""                    
                    GROUP BY p.postID
                    ORDER BY likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
			case "all":
				query = "
                    SELECT p.postID, p.pageID, p.content, p.image, p.post_date, p.comments, p.is_profile, page.title AS pageTitle, page.logo AS pageLogo, page.userID AS pageOwnerID, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
					LEFT JOIN " + TABLE_PAGES + " AS page ON page.pageID = p.pageID					
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1)
                    GROUP BY p.postID
                    ORDER BY likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
		}

		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Getting Top Posts for Homepage
	/// 
	/// <typeparam name=""></typeparam> mixed period
	/// <typeparam name=""></typeparam> mixed type
	/// <typeparam name=""></typeparam> mixed base
	/// <typeparam name=""></typeparam> mixed page
	/// <typeparam name=""></typeparam> Post limit
	/// <returns></returns> Indexed
	/// </summery>
	public static function getTopPostsForHomepage(period = "today", type = "text", base = 1.04, page = 1, limit = null){
		global db;

		if(limit == null)
			limit = Post.{COUNT_PER_PAGE + strtoupper("_type")};

		limit = (page - 1) * limit + ", " + limit;

		switch(period){
			case "today":
				query = "
                    SELECT DISTINCT(p.postID), p.content, p.image, p.comments, p.post_date, p.is_profile, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url
                    ,(p.`likes` + p.`comments`) - POW(" + base + ", TIMESTAMPDIFF(MINUTE, p.`post_date`, "" + date("Y-m-d H:i:s") + "")) AS rating
                    FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster                    
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date >= "" + date("Y-m-d 00:00:00") + ""                    
                    ORDER BY rating DESC, p.likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
			case "this-week":
				cw = date("w");
				sDate = date("Y-m-d 00:00:00", time() - cw * 60 * 60 * 24);
				query = "
                    SELECT p.postID, p.content, p.image, p.post_date, p.comments, p.is_profile, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url 
                    ,(p.`likes` + p.`comments`) - POW(" + base + ", TIMESTAMPDIFF(MINUTE, p.`post_date`, "" + date("Y-m-d H:i:s") + "")) AS rating
                    FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster   
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date < "" + date("Y-m-d 00:00:00") + "" AND p.post_date >= "" + sDate + ""
                    GROUP BY p.postID
                    ORDER BY rating DESC, likes DESC, p.post_date
                    LIMIT limit
                ";

				break;
			case "this-month":
				sDate = date("Y-m-01 00:00:00");
				cw = date("w");
				eDate = date("Y-m-d 00:00:00", time() - cw * 60 * 60 * 24);
				query = "
                    SELECT p.postID, p.content, p.image, p.post_date, p.comments, p.is_profile, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url 
                    ,(p.`likes` + p.`comments`) - POW(" + base + ", TIMESTAMPDIFF(MINUTE, p.`post_date`, "" + date("Y-m-d H:i:s") + "")) AS rating
                    FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND p.post_date < "" + eDate + "" AND p.post_date >= "" + sDate + ""                    
                    GROUP BY p.postID
                    ORDER BY rating DESC, likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
			case "all":
				query = "
                    SELECT p.postID, p.content, p.image, p.post_date, p.comments, p.is_profile, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) as userName, p.likes, p.youtube_url 
                    ,(p.`likes` + p.`comments`) - POW(" + base + ", TIMESTAMPDIFF(MINUTE, p.`post_date`, "" + date("Y-m-d H:i:s") + "")) AS rating
                    FROM " + TABLE_POSTS + " as p
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
                    WHERE p.post_status=1 AND p.type="" + type + "" AND (p.visibility=1) AND TIMESTAMPDIFF(MINUTE, p.`post_date`, "" + date("Y-m-d H:i:s") + "") < 450000
                    GROUP BY p.postID
                    ORDER BY rating DESC, likes DESC, p.post_date
                    LIMIT limit
                ";
				break;
		}

		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Get number of top posts, videos or images
	/// 
	/// <typeparam name=""></typeparam> string period
	/// <typeparam name=""></typeparam> string type
	/// <returns></returns> one
	/// </summery>
	public static function getNumberOfPosts(period = "all", type = "post"){
		global db;

		query = db.prepare("SELECT count(DISTINCT(p.postID)) FROM " + TABLE_POSTS + " AS p WHERE p.post_status=1 AND p.visibility=1 AND p.type=%s", type);

		switch(period){
			case "today":
				query += " AND p.post_date > "" + date("Y-m-d 00:00:00") + "" ";
				break;
			case "this-week":
				cw = date("w");
				sDate = date("Y-m-d 00:00:00", time() - cw * 60 * 60 * 24);
				query += " AND p.post_date > "" + sDate + "" ";
				break;
			case "this-month":
				sDate = date("Y-m-01 00:00:00");
				query += " AND p.post_date > "" + sDate + "" ";
				break;

		}
		count = db.getVar(query);

		return count;
	}

	/// <summery>
	/// Get Liked User Data
	/// 
	/// <typeparam name=""></typeparam> mixed postID
	/// <returns></returns> Indexed
	/// </summery>
	public static function getLikedUsers(postID){
		global db;

		query = db.prepare("SELECT u.userID, u.firstName, u.thumbnail, u.lastName FROM " + TABLE_POSTS_LIKES + " AS pl LEFT JOIN " + TABLE_USERS + " AS u ON u.userID=pl.userID WHERE pl.postID=%s ORDER BY pl.liked_date DESC LIMIT 30", postID);
		likes = db.getResultsArray(query);

		return likes;
	}

	/// <summery>
	/// Get posts By PageID
	/// 
	/// <typeparam name=""></typeparam> integer pageID
	/// <typeparam name=""></typeparam> integer status
	/// <returns></returns> Indexed|null
	/// </summery>
	public function getPostsByPageID(pageID, status = null){

		global db;

		if(!is_numeric(pageID))
			return null;

		query = sprintf("SELECT * FROM %s WHERE pageID=%d", TABLE_POSTS, pageID);

		if(is_numeric(status)){
			query += " post_status=" + status;
		}

		return db.getResultsArray(query);

	}

	/// <summery>
	/// Getting Posts From stats_post table
	/// 
	/// <typeparam name=""></typeparam> String type : text, image, video
	/// <returns></returns> Indexed
	/// </summery>
	public function getPostsFromStats(type){
		global db;

		query = db.prepare("SELECT DISTINCT(p.postID), p.content, p.pageID, p.image, p.comments, p.post_date, p.is_profile, u.thumbnail, u.userID, CONCAT(u.firstName, " ", u.lastName) AS userName, p.likes, p.youtube_url, page.title AS pageTitle, page.logo AS pageLogo, page.userID AS pageOwnerID FROM " + TABLE_STATS_POST + " AS s
                    LEFT JOIN " + TABLE_POSTS + " AS p ON p.postID = s.postID
                    LEFT JOIN " + TABLE_USERS + " AS u ON u.userID = p.poster
                    LEFT JOIN " + TABLE_PAGES + " AS PAGE ON PAGE.pageID = p.pageID
                    WHERE s.postType=%s ORDER BY s.sortOrder
                    ", type);

		rows = db.getResultsArray(query);

		return rows;
	}

}