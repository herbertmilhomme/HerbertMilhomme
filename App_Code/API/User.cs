using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for User
/// </summary>

/// <summery>
/// User Class
/// </summery>
class User{

	const STATUS_USER_ACTIVE = 1; // User Active
	const STATUS_USER_BANNED = 0; // User Banned
	const STATUS_USER_DELETED = -1; // User Deleted

	/// <summery>
	/// <typeparam name=""></typeparam> user
	/// <returns></returns> string
	/// </summery>
	public static function getProfileIcon(user){
		global db;

		//Getting From DB
		if(gettype(user) != "array"){
			query = db.prepare("SELECT thumbnail FROM " + TABLE_USERS + " WHERE userID=%s", user);
			icon = db.getVar(query);

			if(not_null(icon)){
				return DIR_WS_PHOTO + "users/" + user + "/resized/" + icon;
			}else{
				return DIR_WS_IMAGE + "defaultProfileImage.png";
			}
		}else if(gettype(user) == "array"){ //Getting From Array
			if(not_null(user["thumbnail"])){
				return DIR_WS_PHOTO + "users/" + user["userID"] + "/resized/" + user["thumbnail"];
			}else{
				return DIR_WS_IMAGE + "defaultProfileImage.png";
			}
		}
	}

	/// <summery>
	/// <typeparam name=""></typeparam> userID
	/// <returns></returns> array|null
	/// </summery>
	public static function getUserData(userID){
		global db;

		query = db.prepare("SELECT u.*, us.reputation, us.pageFollowers, us.likes, us.comments, us.voteUps, us.replies FROM " + TABLE_USERS + " AS u LEFT JOIN " + TABLE_USERS_STATS + " AS us ON us.userID=u.userID WHERE u.userID=%d", userID);
		data = db.getRow(query);

		if(!data)
			return null;

		//Getting Education
		query = db.prepare("SELECT * FROM " + TABLE_USERS_EDUCATIONS + " WHERE userID=%d ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);
		data["educations"] = rows;

		//Getting Employments
		query = db.prepare("SELECT * FROM " + TABLE_USERS_EMPLOYMENTS + " WHERE userID=%d ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);
		data["employments"] = rows;

		//Getting Links
		query = db.prepare("SELECT * FROM " + TABLE_USERS_LINKS + " WHERE userID=%d ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);
		data["links"] = rows;

		//Getting Contact
		query = db.prepare("SELECT * FROM " + TABLE_USERS_CONTACT + " WHERE userID=%d ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);
		data["contact"] = rows;

		return data;
	}

	/// <summery>
	/// Get User Basic Information by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserBasicInfo(userID){
		global db;

		query = db.prepare("SELECT
            " + TABLE_USERS + ".userID, 
            firstName, 
            lastName, 
            thumbnail,
            email,
            gender, gender_visibility, 
            birthdate, birthdate_visibility, 
            relationship_status, relationship_status_visibility, 
            religion, religion_visibility ,
            political_views, political_views_visibility,
            birthplace, birthplace_visibility,
            current_city, current_city_visibility,
            messenger_privacy, show_messenger, timezone, timezone_visibility,
            user_type, user_acl_id, ua.Name as aclName, ua.Level as aclLevel, credits,
            us.reputation
            FROM " + TABLE_USERS + " LEFT JOIN " + TABLE_USER_ACL + " as ua ON " + TABLE_USERS + ".user_acl_id=ua.aclID" + " LEFT JOIN " + TABLE_USERS_STATS + " as us ON " + TABLE_USERS + ".userID=us.userID" + " WHERE " + TABLE_USERS + ".userID=%s", userID);
		data = db.getRow(query);

		return data;
	}

	/// <summery>
	/// Getting User Bitcoin Details
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserBitcoinInfo(userID){
		global db;

		query = db.prepare("SELECT * FROM " + TABLE_USERS_BITCOIN + " WHERE userID=%d", userID);
		row = db.getRow(query);

		return row;
	}

	/// <summery>
	/// Get User ACL by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public function getUserACL(userID){
		global db;

		query = db.prepare("SELECT user_type, user_acl_id, ua.Name AS aclName, ua.Level AS aclLevel FROM " + TABLE_USERS + " LEFT JOIN " + TABLE_USER_ACL + " AS ua ON " + TABLE_USERS + ".user_acl_id=ua.aclID" + " WHERE userID=%s", userID);
		data = db.getRow(query);

		return data;
	}

	/// <summery>
	/// Save User Basic Information
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool|null
	/// </summery>
	public static function saveUserBasicInfo(userID, data){
		global db;

		if(data["birthdate_month"] == "" || data["birthdate_year"] == "" || data["birthdate_day"] == "")
			birthdate = "0000-00-00";else
			birthdate = date("Y-m-d", strtotime(data["birthdate_year"] + "-" + data["birthdate_month"] + "-" + data["birthdate_day"]));

		rs = db.updateFromArray(TABLE_USERS, ["firstName" => trim(data["firstName"]), "lastName" => trim(data["lastName"]), "gender" => data["gender"], "gender_visibility" => data["gender_visibility"], "birthdate" => birthdate, "birthdate_visibility" => data["birthdate_visibility"], "relationship_status" => data["relationship_status"], "relationship_status_visibility" => data["relationship_status_visibility"], "religion" => data["religion"], "religion_visibility" => data["religion_visibility"], "political_views" => data["political_views"], "political_views_visibility" => data["political_views_visibility"], "birthplace" => data["birthplace"], "birthplace_visibility" => data["birthplace_visibility"], "current_city" => data["current_city"], "current_city_visibility" => data["current_city_visibility"], "timezone" => data["timezone"], "timezone_visibility" => 0 //data["timezone_visibility"]

		], ["userID" => userID]);

		return rs;
	}

	/// <summery>
	/// Get User Basic Information by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserContactInfo(userID){
		global db;

		query = db.prepare("SELECT
            userID, 
            email, email_visibility,
            home_phone, home_phone_visibility, 
            work_phone, work_phone_visibility, 
            cell_phone, cell_phone_visibility, 
            address1, address2,
            city,
            state,
            country,
            zip,
            address_visibility
            FROM " + TABLE_USERS + " WHERE userID=%s", userID);
		data = db.getRow(query);

		//Getting Contact
		query = db.prepare("SELECT * FROM " + TABLE_USERS_CONTACT + " WHERE userID=%s ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);
		data["contact"] = rows;

		return data;
	}

	/// <summery>
	/// Get User Messenger Ids
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <returns></returns> Indexed
	/// </summery>
	public function getUserMessengerNames(userID){
		global db;

		query = db.prepare("SELECT * FROM " + TABLE_USERS_CONTACT + " WHERE userID=%s ORDER BY `order`", userID);

		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Check if the email address exists or not
	/// 
	/// <typeparam name=""></typeparam> mixed email
	/// <typeparam name=""></typeparam> mixed userID
	/// <returns></returns> bool
	/// </summery>
	public static function checkEmailDuplication(email, userID = null){
		global db;

		if(!userID)
			query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE `email`=%s", email);else
			query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE `email`=%s AND userID != %s", email, userID);

		res = db.getVar(query);

		return res ? true : false;
	}

	/// <summery>
	/// Update User Fields
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool|null
	/// </summery>
	public static function updateUserFields(userID, data){
		global db;

		res = db.updateFromArray(TABLE_USERS, data, ["userID" => userID]);

		return res;
	}

	/// <summery>
	/// Update User Messenger Names
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool
	/// </summery>
	public static function updateUserMessengerInfo(userID, data){
		global db;

		//Remove old Data
		query = "DELETE FROM " + TABLE_USERS_CONTACT + " WHERE userID="" + userID + """;
		db.query(query);

		//Insert New Values
		for(i = 0; i < count(data); i++){
			row = data[i];
			db.insertFromArray(TABLE_USERS_CONTACT, ["userID" => userID, "contact_name" => row["name"], "contact_type" => row["type"], "visibility" => row["visibility"], "order" => i + 1]);
		}
		return true;
	}

	/// <summery>
	/// Get User Education by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserEducations(userID){
		global db;

		//Getting Contact
		query = db.prepare("SELECT * FROM " + TABLE_USERS_EDUCATIONS + " WHERE userID=%s ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Update User Education Information
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool
	/// </summery>
	public static function updateUserEducationInfo(userID, data){
		global db;

		//Remove old Data
		query = "DELETE FROM " + TABLE_USERS_EDUCATIONS + " WHERE userID="" + userID + """;
		db.query(query);

		//Insert New Values
		for(i = 0; i < count(data); i++){
			row = data[i];
			db.insertFromArray(TABLE_USERS_EDUCATIONS, ["userID" => userID, "school" => row["name"], "start" => row["start"], "end" => row["end"], "visibility" => row["visibility"], "order" => i + 1]);
		}
		return true;
	}

	/// <summery>
	/// Get User Employment History by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserEmploymentHistory(userID){
		global db;

		//Getting Contact
		query = db.prepare("SELECT * FROM " + TABLE_USERS_EMPLOYMENTS + " WHERE userID=%s ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Update User Employment History
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool
	/// </summery>
	public static function updateUserEmploymentHistory(userID, data){
		global db;

		//Remove old Data
		query = db.prepare("DELETE FROM " + TABLE_USERS_EMPLOYMENTS + " WHERE userID=%d", userID);
		db.query(query);

		//Insert New Values
		for(i = 0; i < count(data); i++){
			row = data[i];
			db.insertFromArray(TABLE_USERS_EMPLOYMENTS, ["userID" => userID, "employer" => row["employer"], "start" => row["start"], "end" => row["end"], "visibility" => row["visibility"], "order" => i + 1]);
		}
		return true;
	}

	/// <summery>
	/// Get User Links by ID
	/// 
	/// <typeparam name=""></typeparam> int userID
	/// <returns></returns> array
	/// </summery>
	public static function getUserLinks(userID){
		global db;

		//Getting Contact
		query = db.prepare("SELECT * FROM " + TABLE_USERS_LINKS + " WHERE userID=%s ORDER BY `order` ASC", userID);
		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Update User Links
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool
	/// </summery>
	public static function updateUserLinks(userID, data){
		global db;

		//Remove old Data
		query = db.prepare("DELETE FROM " + TABLE_USERS_LINKS + " WHERE userID=%d", userID);
		db.query(query);

		//Insert New Values
		for(i = 0; i < count(data); i++){
			row = data[i];
			db.insertFromArray(TABLE_USERS_LINKS, ["userID" => userID, "title" => row["title"], "url" => row["url"], "visibility" => row["visibility"], "order" => i + 1]);
		}
		return true;
	}

	/// <summery>
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> photoID
	/// <returns></returns> bool
	/// </summery>
	public static function updateUserProfilePhoto(userID, photoID){
		global db;

		query = db.prepare("SELECT image, is_profile FROM " + TABLE_POSTS + " WHERE postID=%s AND poster=%s", photoID, userID);
		row = db.getRow(query);

		if(!row){
			add_message(MSG_INVALID_REQUEST, MSG_TYPE_ERROR);
			return false;
		}
		if(!row["is_profile"]){
			redirect("/photo_edit.php?photoID=" + photoID + "&set_profile=1");
			exit;
		}

		query = db.updateFromArray(TABLE_USERS, ["thumbnail" => row["image"]], ["userID" => userID]);

		add_message(MSG_PROFILE_PHOTO_CHANGED, MSG_TYPE_SUCCESS);
		return true;
	}

	/// <summery>
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> photoID
	/// <returns></returns> bool
	/// </summery>
	public function updateUserProfileThumbnail(userID, photoID){
		global db;

		query = db.updateFromArray(TABLE_USERS, ["thumbnail" => photoID], ["userID" => userID]);

		add_message(MSG_PROFILE_PHOTO_CHANGED, MSG_TYPE_SUCCESS);

		return true;
	}

	/// <summery>
	/// Create New Account
	/// 
	/// <typeparam name=""></typeparam> Array data
	/// <returns></returns> bool|int|null|string
	/// </summery>
	public static function createNewAccount(data){
		global db;

		data = array_map("trim", data);

		if(data["firstName"] == "" || data["lastName"] == ""){
			add_message(MSG_USERNAME_EMPTY_ERROR, MSG_TYPE_ERROR);
			return false;
		}

		//Check Email Address
		if(!preg_match("/^([a-zA-Z0-9])+([a-zA-Z0-9\._-])*@([a-zA-Z0-9_-])+([a-zA-Z0-9\._-]+)+/", data["email"])){
			add_message(MSG_INVALID_EMAIL, MSG_TYPE_ERROR);
			return false;
		}

		//Check Email Duplication
		if(User.checkEmailDuplication(data["email"])){

			//If this one is banned?
			if(User.getUserStatus(data["email"]) == User.STATUS_USER_DELETED){
				add_message(MSG_EMAIL_BANNED, MSG_TYPE_ERROR);
			}else{
				add_message(MSG_EMAIL_EXIST, MSG_TYPE_ERROR);
			}

			return false;
		}

		if(!data["password"] || !data["password2"]){
			add_message(MSG_EMPTY_PASSWORD, MSG_TYPE_ERROR);
			return false;
		}
		if(data["password"] != data["password2"]){
			add_message(MSG_NOT_MATCH_PASSWORD, MSG_TYPE_ERROR);
			return false;
		}
		if(!check_password_strength(data["password"])){
			add_message(MSG_PASSWORD_STRENGTH_ERROR, MSG_TYPE_ERROR);
			return false;
		}

		//Create Token
		token = md5(mt_rand(0, 99999) + time() + data["email"] + mt_rand(0, 99999));
		password = encrypt_password(data["password"]);

		//Create New Account
		newId = db.insertFromArray(TABLE_USERS, ["firstName" => data["firstName"], "lastName" => data["lastName"], "email" => data["email"], "email_visibility" => -1, "password" => password, "thumbnail" => "", "user_type" => "Registered", "user_acl_id" => 2, "ip_addr" => _SERVER["REMOTE_ADDR"], "created_date" => date("Y-m-d H:i:s"), "token" => token]);

		if(!newId){
			add_message(db.getLastError(), MSG_TYPE_ERROR);
			return false;
		}

		//Create New Record on the users_stats table
		db.insertFromArray(TABLE_USERS_STATS, ["userID" => newId, "pageFollowers" => 0, "likes" => 0, "comments" => 0, "voteUps" => 0, "replies" => 0, "reputation" => 0]);

		//Make new user to follow all categories
		ForumFollower.followBasicForums(newId);

		url_protocol = "http://";
		if(SITE_USING_SSL == true)
			url_protocol = "https://";

		//Send an email to new user with a validation link
		link = url_protocol + _SERVER["HTTP_HOST"] + "/register.php?action=verify&email=" + data["email"] + "&token=" + token;

		title = "Please verify your account.";
		body = "Dear " + data["firstName"] + " " + data["lastName"] + "\n\n" + "Thanks for your registration. \n" + "To complete your registration, please verify your email address by clicking the below link:. \n" + link + "\n\n" + DOMAIN;

		sendmail(data["email"], data["firstName"] + " " + data["lastName"], title, body);

		return newId;

	}

	/// <summery>
	/// <typeparam name=""></typeparam> email
	/// <typeparam name=""></typeparam> token
	/// <returns></returns> bool
	/// </summery>
	public static function verifyAccount(email, token){
		global db;

		query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE token=%s AND email=%s AND STATUS=0", token, email);
		userID = db.getVar(query);
		if(!userID){
			add_message(MSG_INVALID_TOKEN, MSG_TYPE_ERROR);
			return false;
		}

		//Verify links
		query = db.prepare("UPDATE " + TABLE_USERS + " SET status=1, token="" WHERE userID=%d", userID);
		db.query(query);
		add_message(MSG_ACCOUNT_VERIFIED, MSG_TYPE_SUCCESS);

		//Make this user to friend with bucky
		query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE email=%s", ADMIN_EMAIL);
		ID = db.getVar(query);

		//ID = db.getVar("Select userID FROM " + TABLE_USERS + " WHERE email="admin@thenewboston.com"");
		db.insertFromArray(TABLE_FRIENDS, ["userID" => ID, "userFriendID" => userID, "status" => "1"]);
		db.insertFromArray(TABLE_FRIENDS, ["userID" => userID, "userFriendID" => ID, "status" => "1"]);

		//Create Bitcoin account
		Bitcoin.createWallet(userID, email);

		//Create Default Ads for the users
		classPublisherAds = new PublisherAds();
		classPublisherAds.createDefaultPublisherAds(userID);

		return true;
	}

	/// <summery>
	/// Create new password and send it to user
	/// 
	/// <typeparam name=""></typeparam> String email
	/// <returns></returns> bool|void
	/// </summery>
	public static function resetPassword(email){
		global db;

		email = trim(email);
		if(!email){
			redirect("/register.php?forgotpwd=1", MSG_EMPTY_EMAIL, MSG_TYPE_ERROR);
			return;
		}

		//Check Email Address
		if(!preg_match("/^([a-zA-Z0-9])+([a-zA-Z0-9\._-])*@([a-zA-Z0-9_-])+([a-zA-Z0-9\._-]+)+/", email)){
			redirect("/register.php?forgotpwd=1", MSG_INVALID_EMAIL, MSG_TYPE_ERROR);
			return false;
		}

		query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE email=%s", email);
		userID = db.getVar(query);

		if(!userID){
			redirect("/register.php", MSG_RESET_PASSWORD_EMAIL_SENT);
			//            redirect("/register.php?forgotpwd=1", MSG_EMAIL_NOT_FOUND, MSG_TYPE_ERROR);
			return false;
		}

		data = User.getUserData(userID);

		//Remove Old Token
		UsersToken.removeUserToken(userID, "password");

		//Create New Token
		token = UsersToken.createNewToken(userID, "password");

		link = "https://" + _SERVER["HTTP_HOST"] + "/reset_password.php?token=" + token;

		//Send an email to user with the link
		title = "Reset your password.";
		body = "Dear " + data["firstName"] + " " + data["lastName"] + "\n\n" + "Please reset your password by using the below link:\n" + link + "\n\nroom.com";
		require_once(DIR_FS_INCLUDES + "phpMailer/class.phpmailer.php");

		sendmail(data["email"], data["firstName"] + " " + data["lastName"], title, body);

		redirect("/register.php", MSG_RESET_PASSWORD_EMAIL_SENT, MSG_TYPE_SUCCESS);

		return;
	}

	/// <summery>
	/// Check UserID is correct or not
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> Boolean onlyActived
	/// <returns></returns> bool
	/// </summery>
	public static function checkUserID(userID, onlyActived = true){
		global db;

		if(onlyActived)
			query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE userID=%d AND `status`=1", userID);else
			query = db.prepare("SELECT userID FROM " + TABLE_USERS + " WHERE userID=%d AND (`status`=1 OR `status`=0)", userID);

		id = db.getVar(query);

		return not_null(id) ? true : false;
	}

	/// <summery>
	/// Get a value from user attributes
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <typeparam name=""></typeparam> String key
	/// <typeparam name=""></typeparam> Mixed default
	/// <returns></returns> Mixed
	/// </summery>
	public function getAttribute(userID, key, default = null){
		global db;

		query = db.query("SELECT attributes FROM " + TABLE_USERS + " WHERE userID=%d", userID);
		attr = db.getVar(query);

		if(!attr)
			return default;

		attr = unserialize(attr);
		if(!isset(attr[key]))
			return default;

		return attr[key];
	}

	/// <summery>
	/// Save Attribute
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <typeparam name=""></typeparam> mixed key
	/// <typeparam name=""></typeparam> mixed value
	/// <returns></returns> bool|null
	/// </summery>
	public function setAttribute(userID, key, value){
		global db;

		query = db.query("SELECT attributes FROM " + TABLE_USERS + " WHERE userID=%d", userID);
		attr = db.getVar(query);

		if(!attr)
			attr = [];else
			attr = unserialize(attr);

		attr[key] = value;

		//Save Attribute
		return db.update("UPDATE " + TABLE_USERS + " SET attributes="" + serialize(attr) + "" WHERE userID=" + userID);

	}

	/// <summery>
	/// Remove Account
	/// </summery>
	public static function deleteUserAccount(userID){
		global db;

		userID = intval(userID);

		//Fix Comments Count
		query = db.prepare("SELECT count(commentID) AS c, postID FROM " + TABLE_POSTS_COMMENTS + " WHERE commenter=%d AND commentStatus=1 GROUP BY postID", userID);
		pcRows = db.getResultsArray(query);
		foreach(pcRows as row){
			db.query("UPDATE " + TABLE_POSTS + " SET `comments` = `comments` - " + row["c"] + " WHERE postID=" + row["postID"]);

		}

		//Fix Likes Count
		query = db.prepare("SELECT count(likeID) AS c, postID FROM " + TABLE_POSTS_LIKES + " WHERE userID=%d AND likeStatus=1 GROUP BY postID", userID);
		plRows = db.getResultsArray(query);
		foreach(plRows as row){
			db.query("UPDATE " + TABLE_POSTS + " SET `likes` = `likes` - " + row["c"] + " WHERE postID=" + row["postID"]);
		}

		//Block Votes for Moderator
		query = db.prepare("SELECT count(voteID) AS c, candidateID FROM " + TABLE_MODERATOR_VOTES + " WHERE voterID=%d AND voteStatus=1 GROUP BY candidateID", userID);
		vRows = db.getResultsArray(query);
		foreach(vRows as row){
			db.query("UPDATE " + TABLE_MODERATOR_CANDIDATES + " SET `votes` = `votes` - " + row["c"] + " WHERE candidateID=" + row["candidateID"]);
		}

		//Block Replies
		query = db.prepare("SELECT count(r.replyID), r.topicID, t.categoryID FROM " + TABLE_FORUM_REPLIES + " AS r LEFT JOIN " + TABLE_FORUM_TOPICS + " AS t ON t.topicID=r.topicID WHERE r.status="publish" AND r.creatorID=%d GROUP BY r.topicID", userID);
		rRows = db.getResultsArray(query);
		db.query("UPDATE " + TABLE_FORUM_REPLIES + " SET `status`="suspended" WHERE creatorID=" + userID + " AND `status`="publish"");
		foreach(rRows as row){
			db.query("UPDATE " + TABLE_FORUM_TOPICS + " SET `replies` = `replies` - " + row["c"] + " WHERE topicID=" + row["topicID"]);
			db.query("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `replies` = `replies` - " + row["c"] + " WHERE categoryID=" + row["categoryID"]);
			ForumTopic.updateTopicLastReplyID(row["topicID"]);
		}

		//Block Topics
		query = db.prepare("SELECT count(topicID) AS tc, SUM(replies) AS rc, categoryID FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=%d AND `status`="publish" GROUP BY categoryID", userID);
		tRows = db.getResultsArray(query);
		db.query("UPDATE " + TABLE_FORUM_TOPICS + " SET `status`="suspended" WHERE creatorID=" + userID + " AND `status`="publish"");
		foreach(tRows as row){
			db.query("UPDATE " + TABLE_FORUM_CATEGORIES + " SET `replies` = `replies` - " + row["rc"] + ", `topics` = `topics` - " + row["tc"] + " WHERE categoryID=" + row["categoryID"]);
			ForumCategory.updateCategoryLastTopicID(row["categoryID"]);
		}

		//Block Reply Votes
		query = db.prepare("SELECT count(voteID) AS c, objectID FROM " + TABLE_FORUM_VOTES + " WHERE voterID=%d AND voteStatus=1 GROUP BY objectID", userID);
		vRows = db.getResultsArray(query);
		foreach(vRows as row){
			db.query("UPDATE " + TABLE_FORUM_REPLIES + " SET `votes` = `votes` - " + row["c"] + " WHERE replyID=" + row["objectID"]);
		}

		//Delete Reported Objects
		db.query("DELETE FROM " + TABLE_REPORTS + " WHERE objectID IN (SELECT postID FROM " + TABLE_POSTS + " WHERE poster=" + userID + ")");
		db.query("DELETE FROM " + TABLE_REPORTS + " WHERE objectID IN (SELECT topicID FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=" + userID + ")");
		db.query("DELETE FROM " + TABLE_REPORTS + " WHERE objectID IN (SELECT replyID FROM " + TABLE_FORUM_REPLIES + " WHERE creatorID=" + userID + ")");

		//Delete From banned Users
		db.query("DELETE FROM " + TABLE_BANNED_USERS + "  WHERE bannedUserID=" + userID);

		//Delete Activities
		db.query("DELETE FROM " + TABLE_MAIN_ACTIVITIES + " WHERE userID=" + userID);

		//Delete Album Photos
		db.query("DELETE FROM " + TABLE_ALBUMS_PHOTOS + " WHERE album_id IN (SELECT albumID FROM " + TABLE_ALBUMS + " WHERE OWNER=" + userID + ")");
		//Delete ALbums
		db.query("DELETE FROM " + TABLE_ALBUMS + " WHERE OWNER=" + userID);

		//Delete Friends
		db.query("DELETE FROM " + TABLE_FRIENDS + " WHERE userID=" + userID + " OR userFriendID=" + userID);

		//Delete Messages
		db.query("DELETE FROM " + TABLE_MESSAGES + " WHERE userID=" + userID + " OR sender=" + userID);

		//Delete Private Messengers
		db.query("DELETE FROM " + TABLE_MESSENGER_BLOCKLIST + " WHERE userID=" + userID + " OR blockedID=" + userID);
		db.query("DELETE FROM " + TABLE_MESSENGER_BUDDYLIST + " WHERE userID=" + userID + " OR buddyID=" + userID);
		db.query("DELETE FROM " + TABLE_MESSENGER_MESSAGES + " WHERE userID=" + userID + " OR buddyID=" + userID);

		//Delete Posts
		posts = db.getResultsArray("SELECT * FROM " + TABLE_POSTS + " WHERE poster=" + userID);
		foreach(posts as post){
			//Delete Comments
			db.query("DELETE FROM " + TABLE_POSTS_COMMENTS + " WHERE postID=" + post["postID"]);
			//Delete Likes
			db.query("DELETE FROM " + TABLE_POSTS_LIKES + " WHERE postID=" + post["postID"]);
			//Delete hits
			db.query("DELETE FROM " + TABLE_POSTS_HITS + " WHERE postID=" + post["postID"]);
		}
		db.query("DELETE FROM " + TABLE_POSTS + " WHERE poster=" + userID);

		//Delete Pages
		pageIns = new Page();
		pageIns.deletePageByUserID(userID);

		//Delete Trade Section which are related to this user.
		tradeIns = new TradeItem();
		tradeIns.deleteItemsByUserID(userID);

		//Delete Shop Section which are related to this user
		shopIns = new ShopProduct();
		shopIns.deleteProductsByUserID(userID);

		//Delete Comments
		db.query("DELETE FROM " + TABLE_POSTS_COMMENTS + " WHERE commenter=" + userID);

		//Delete Likes
		db.query("DELETE FROM " + TABLE_POSTS_LIKES + " WHERE userID=" + userID);

		//Delete Page Followers
		db.query("DELETE FROM " + TABLE_PAGE_FOLLOWERS + " WHERE userID=" + userID);

		//Getting Removed Topics
		topicIDs = db.getResultsArray("SELECT topicID FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=" + userID);
		if(!topicIDs)
			topicIDs = [0];

		//Delete Reply Votes
		db.query("DELETE FROM " + TABLE_FORUM_VOTES + " WHERE voterID=" + userID);
		db.query("DELETE FROM " + TABLE_FORUM_VOTES + " WHERE objectID IN ( SELECT replyID FROM " + TABLE_FORUM_REPLIES + " WHERE creatorID=" + userID + " OR topicID IN (" + implode(", ", topicIDs) + ") )");

		//Delete Replies
		db.query("DELETE FROM " + TABLE_FORUM_REPLIES + " WHERE creatorID=" + userID + " OR topicID IN (" + implode(", ", topicIDs) + ")");

		//Delete Topics
		db.query("DELETE FROM " + TABLE_FORUM_TOPICS + " WHERE creatorID=" + userID);

		//Delete Users
		/*db.query("DELETE FROM " + TABLE_USERS + " WHERE userID=" + userID);
		db.query("DELETE FROM " + TABLE_USERS_CONTACT + " WHERE userID=" + userID);
		db.query("DELETE FROM " + TABLE_USERS_EDUCATIONS + " WHERE userID=" + userID);
		db.query("DELETE FROM " + TABLE_USERS_EMPLOYMENTS + " WHERE userID=" + userID);
		db.query("DELETE FROM " + TABLE_USERS_LINKS + " WHERE userID=" + userID);
		db.query("DELETE FROM " + TABLE_USERS_TOKEN + " WHERE userID=" + userID);*/
		//Don"t delete user from the database, just update the user"s status
		db.query("UPDATE " + TABLE_USERS + " SET `status`=" + User.STATUS_USER_DELETED + " WHERE userID=" + userID);

		//Send
		bitCoinInfo = User.getUserBitcoinInfo(userID);
		if(bitCoinInfo){
			userInfo = User.getUserBasicInfo(userID);

			content = "Your " + SITE_NAME + " account has been deleted. However, you may still access your Bitcoin wallet at:\n" + "https://blockchain.info/wallet/login\n" + "Identifier: " + bitCoinInfo["bitcoin_guid"] + "\n" + "Password: " + decrypt(bitCoinInfo["bitcoin_password"]) + "\n";

			//Send Email to User
			sendmail(userInfo["email"], userInfo["firstName"] + " " + userInfo["lastName"], SITE_NAME + " Account has been Deleted", content);
		}

	}

	/// <summery>
	/// Search Users
	/// 
	/// <typeparam name=""></typeparam> Int term
	/// <typeparam name=""></typeparam> array exclude
	/// <returns></returns> Array
	/// @internal param Int userID
	/// </summery>
	public static function searchUsers(term, exclude = []){
		global db;

		if(not_null(exclude) && !is_array(exclude))
			exclude = [exclude];

		if(not_null(exclude))
			query = "SELECT DISTINCT(u.userID), CONCAT(u.firstName, " ", u.lastName) AS fullName FROM " + TABLE_USERS + " AS u WHERE u.status = 1 AND u.userID NOT IN(" + implode(", ", db.escapeInput(exclude)) + ") AND (CONCAT(u.firstName, " ", u.lastName) LIKE "%" + db.escapeInput(term) + "%") ORDER BY fullName";else
			query = "SELECT DISTINCT(u.userID), CONCAT(u.firstName, " ", u.lastName) AS fullName FROM " + TABLE_USERS + " AS u WHERE u.status = 1 AND (CONCAT(u.firstName, " ", u.lastName) LIKE "%" + db.escapeInput(term) + "%") ORDER BY fullName";

		rows = db.getResultsArray(query);

		return rows;
	}

	/// <summery>
	/// Get User Forum Settings
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <returns></returns> Array
	/// </summery>
	public static function getUserNotificationSettings(userID){
		global db, GLOBALS;

		query = db.prepare("SELECT * FROM " + TABLE_USERS_NOTIFY_SETTINGS + " WHERE userID=%s", userID);
		row = db.getRow(query);

		if(!row)
			row = [];

		row = array_merge(GLOBALS["notify_settings"], row);

		return row;
	}

	/// <summery>
	/// Save User Notification Settings
	/// 
	/// <typeparam name=""></typeparam> mixed userID
	/// <typeparam name=""></typeparam> mixed data
	/// <returns></returns> bool|null|string
	/// </summery>
	public static function saveUserNotificationSettings(userID, data){
		global db;

		userID = intval(userID);

		paramData = ["optOfferReceived" => isset(data["optOfferReceived"]) ? 1 : 0, "optOfferAccepted" => isset(data["optOfferAccepted"]) ? 1 : 0, "optOfferDeclined" => isset(data["optOfferDeclined"]) ? 1 : 0, "optFeedbackReceived" => isset(data["optFeedbackReceived"]) ? 1 : 0, "optProductSoldOnShop" => isset(data["optProductSoldOnShop"]) ? 1 : 0,];

		res = db.updateFromArray(TABLE_TRADE_USERS, paramData, ["userID" => userID]);

		notifyData = ["userID" => userID, "notifyRepliedToMyTopic" => isset(data["notifyRepliedToMyTopic"]) ? 1 : 0, "notifyRepliedToMyReply" => isset(data["notifyRepliedToMyReply"]) ? 1 : 0, "notifyMyPostApproved" => isset(data["notifyMyPostApproved"]) ? 1 : 0];

		//Check if the forum notification exists or not
		query = db.prepare("SELECT settingID FROM " + TABLE_USERS_NOTIFY_SETTINGS + " WHERE userID=%d", userID);
		sID = db.getVar(query);

		if(!sID)
			fr = db.insertFromArray(TABLE_USERS_NOTIFY_SETTINGS, notifyData);
		else
			fr = db.updateFromArray(TABLE_USERS_NOTIFY_SETTINGS, notifyData, ["settingID" => sID]);

		if(fr && res)
			return true;else
			return db.getLastError();

	}

	/// <summery>
	/// <typeparam name=""></typeparam> userID
	/// <typeparam name=""></typeparam> type
	/// <typeparam name=""></typeparam> value
	/// </summery>
	public static function updateStats(userID, type, value){
		global db;

		query = db.prepare("UPDATE " + TABLE_USERS_STATS + " SET `type` = `type` + %d, `reputation` = `reputation` + %d  WHERE userID=%d", value, value, userID);
		db.query(query);

	}

	/// <summery>
	/// Get Users Invitation Code
	/// 
	/// <typeparam name=""></typeparam> Int userID
	/// <returns></returns> string
	/// </summery>
	public function getUserInviteCode(userID){
		positon = userID;
		if(positon > 1000){
			positon = userID % 1000;
		}
		invite_key = substr(INVITE_MASTER_KEY, positon, INVITE_LENGTH);

		if(strpos(INVITE_MASTER_KEY, invite_key) !== false && strlen(invite_key) == INVITE_LENGTH){
			return invite_key;
		}else{
			return "";
		}
	}

	/// <summery>
	/// Validate Code for New Users
	/// 
	/// <typeparam name=""></typeparam> invite_code
	/// <returns></returns> bool
	/// @internal param Int userID
	/// </summery>
	public function is_invite_code_valid(invite_code){
		invite_code = trim(invite_code);
		if(strpos(INVITE_MASTER_KEY, invite_code) !== false && strlen(invite_code) == INVITE_LENGTH){
			return true;
		}else{
			return false;
		}
	}

	/// <summery>
	/// User Status By Email / UserID
	/// 
	/// <typeparam name=""></typeparam> mixed param
	/// </summery>
	public function getUserStatus(param){

		global db;

		query = "";
		if(is_numeric(param)){
			query = db.prepare("SELECT status FROM " + TABLE_USERS + " WHERE userID=%d", param);
		}else{
			query = db.prepare("SELECT status FROM " + TABLE_USERS + " WHERE email="%s"", param);
		}

		data = db.getRow(query);

		return data["status"];
	}

	/// <summery>
	/// <returns></returns> bool
	/// </summery>
	public static function checkDailyUserLimit(){
		global db;

		date = date("Y-m-d");

		query = db.prepare("SELECT count(*) FROM " + TABLE_USERS + " WHERE DATE(`created_date`) = %s AND `ip_addr` = %s", date, _SERVER["REMOTE_ADDR"]);
		counts = db.getVar(query);

		return counts < USER_DAILY_LIMIT_ACCOUNTS;
	}

}