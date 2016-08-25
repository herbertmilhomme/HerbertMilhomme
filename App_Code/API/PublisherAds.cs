﻿using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for PublisherAds
/// </summary>

//Ad typs for publisher 
if(!defined("AD_TYPE_CUSTOM"))
    define("AD_TYPE_CUSTOM", 1);

if(!defined("AD_TYPE_PROFILE"))
    define("AD_TYPE_PROFILE", 2);

if(!defined("AD_TYPE_FORUM"))
    define("AD_TYPE_FORUM", 3);

if(!defined("PUBLISHER_AD_STATUS_ACTIVE"))
    define("PUBLISHER_AD_STATUS_ACTIVE", 1);

if(!defined("PUBLISHER_AD_STATUS_DELETED"))
    define("PUBLISHER_AD_STATUS_DELETED", 0);

class PublisherAds {

    public last_message;

    public static COUNT_PER_PAGE = 20;

    /// <summery>
    /// Create Publisher Ad
    /// 
    /// <typeparam name=""></typeparam> mixed data
    /// <returns></returns> bool
    /// </summery>
    public function savePublisherAd(userID, data){
        global db;

        if(!data["name"]){
            this.last_message = MSG_INVALID_REQUEST;
            return false;
        }

        sizeId = data["size"];

        classAds = new Ads();

        sizeDetail = classAds.getAdSizeById(sizeId);

        if(!sizeDetail){
            this.last_message = MSG_INVALID_REQUEST;
            return false;
        }

        borderColor = !data["border-color"] ? "006699" : data["border-color"];
        bgColor = !data["bg-color"] ? "006699" : data["bg-color"];
        titleColor = !data["title-color"] ? "006699" : data["title-color"];
        descriptionColor = !data["description-color"] ? "006699" : data["description-color"];
        urlColor = !data["url-color"] ? "006699" : data["url-color"];
        adType = data["adType"];
        //Create token

        token = sha1(userID + session_id() + data["name"] + time() + generate_random_string(20));

        insertData = ["publisherID" => userID, "size" => sizeDetail["id"], "name" => trim(data["name"]), "borderColor" => borderColor, "bgColor" => bgColor, "titleColor" => titleColor, "textColor" => descriptionColor, "urlColor" => urlColor, "createdDate" => date("Y-m-d H:i:s"), "impressions" => 0, "earnings" => 0.0, "status" => PUBLISHER_AD_STATUS_ACTIVE, "adType" => adType, "token" => token];

        newId = db.insertFromArray(TABLE_PUBLISHER_ADS, insertData);
        if(!newId){
            this.last_message = db.getLastError();
            return false;
        }

        this.last_message = MSG_AD_NEW_AD_CREATED;
        return true;
    }

    /// <summery>
    /// Create room Ads for user
    /// 
    /// <typeparam name=""></typeparam> mixed userID
    /// </summery>
    public function createDefaultPublisherAds(userID){
        profileAd = ["name" => SITE_NAME + " Pages & Profile", "size" => 9, "border-color" => "006699", "bg-color" => "FFFFFF", "title-color" => "006699", "description-color" => "999999", "url-color" => "CC0000", "adType" => AD_TYPE_PROFILE];
        this.savePublisherAd(userID, profileAd);

        //Forum Ad
        forumAd = ["name" => SITE_NAME + " Forum", "size" => 8, "border-color" => "006699", "bg-color" => "FFFFFF", "title-color" => "006699", "description-color" => "999999", "url-color" => "CC0000", "adType" => AD_TYPE_FORUM];
        this.savePublisherAd(userID, forumAd);

    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <typeparam name=""></typeparam> status
    /// <returns></returns> one
    /// </summery>
    public function getPublisherAdsCount(userID, status){
        global db;

        query = db.prepare("SELECT count(*) FROM " + TABLE_PUBLISHER_ADS + " WHERE publisherID=%d", userID);

        switch(status){
            case "active":
                query += " AND `status`=" + PUBLISHER_AD_STATUS_ACTIVE;
                break;
            case "deleted":
                query += " AND `status`=" + PUBLISHER_AD_STATUS_DELETED;
                break;
        }

        c = db.getVar(query);

        return c;
    }

    /// <summery>
    /// <typeparam name=""></typeparam>      userID
    /// <typeparam name=""></typeparam>      status
    /// <typeparam name=""></typeparam> null page
    /// <typeparam name=""></typeparam> null limit
    /// <returns></returns> Indexed
    /// </summery>
    public function getPublisherAds(userID, status, page = null, limit = null){
        global db;

        query = db.prepare("SELECT AD.*, AD_SIZE.size AS size_name FROM " + TABLE_PUBLISHER_ADS + " AS AD LEFT JOIN " + TABLE_AD_SIZES + " AS AD_SIZE ON AD.size=AD_SIZE.id WHERE AD.publisherID=%d", userID);

        switch(status){
            case "active":
                query += " AND `status`=" + PUBLISHER_AD_STATUS_ACTIVE;
                break;
            case "deleted":
                query += " AND `status`=" + PUBLISHER_AD_STATUS_DELETED;
                break;
        }

        if(page){
            query += " LIMIT " + (page - 1) * limit + ", " + limit;
        }

        rows = db.getResultsArray(query);

        return rows;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> id
    /// <returns></returns> array
    /// </summery>
    public function getAdById(id){
        global db;

        query = db.prepare("SELECT AD.*, s.size AS size_name
                    FROM " + TABLE_PUBLISHER_ADS + " AS AD
                    LEFT JOIN " + TABLE_AD_SIZES + " AS s ON s.id=AD.size
                  WHERE AD.`id`=%d", id);

        detail = db.getRow(query);

        return detail;
    }

    /// <summery>
    /// Delete Ad
    /// 
    /// <typeparam name=""></typeparam> Int id
    /// <returns></returns> bool
    /// </summery>
    public function deleteAd(id){
        global db;

        //do not delete the ad, just change the status to 0
        query = db.prepare("UPDATE " + TABLE_PUBLISHER_ADS + " SET `status`=%d WHERE id=%d", PUBLISHER_AD_STATUS_DELETED, id);

        db.query(query);

        return true;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> token
    /// <returns></returns> bool|string
    /// </summery>
    public function renderAd(token){
        global db;

        //Getting Ad details by token
        query = db.prepare("SELECT * FROM " + TABLE_PUBLISHER_ADS + " WHERE token=%s", token);
        adDetail = db.getRow(query);

        if(!adDetail)
            return false;

        classAds = new Ads();

        sizeDetail = classAds.getAdSizeById(adDetail["size"]);

        //fixes a display issues with vertical ads
        if(sizeDetail["type"] == "vertical"){
            spaceToSubtract = sizeDetail["width"] - 18; //16px for padding, 2px for border
            displayWidth = "width:" + spaceToSubtract + "px;";
        }
        //changes padding for horizontal ads
        if(sizeDetail["type"] == "horizontal" && sizeDetail["ads"] > 1){
            newHorizontalPadding = "padding: 0px 20px;";
        }

        query1 = "SELECT AD.* FROM " + TABLE_ADS + " AS AD WHERE AD.defaultAd=0 AND AD.status="" + AD_STATUS_ACTIVE + "" AND AD.ownerID != "" + adDetail["publisherID"] + "" AND AD.type = "Text" ORDER BY rand() LIMIT " + sizeDetail["ads"];
        query2 = "SELECT AD.* FROM " + TABLE_ADS + " AS AD WHERE AD.defaultAd=0 AND AD.status="" + AD_STATUS_ACTIVE + "" AND AD.ownerID != "" + adDetail["publisherID"] + "" AND AD.type = "Image" AND AD.adSize="" + sizeDetail["id"] + "" ORDER BY rand() LIMIT 1";

        if(mt_rand(0, 10) > 5) //Getting Text Ads
        {
            results = db.getResultsArray(query1);
            if(!results)
                results = db.getResultsArray(query2);
        }else//Getting Image Ads
        {
            results = db.getResultsArray(query2);
            if(!results)
                results = db.getResultsArray(query1);
        }

        //Do not display borders on Image ads
        if(results[0]["type"] == "Image"){
            displayBorder = ";border:none;";
        }

        if(count(results) < sizeDetail["ads"]){
            //Getting room Default Ads
            query3 = "SELECT AD.* FROM " + TABLE_ADS + " AS AD WHERE AD.defaultAd=1 AND AD.status="" + AD_STATUS_ACTIVE + "" AND AD.type = "Text" ORDER BY rand() LIMIT " + (sizeDetail["ads"] - count(results));
            results2 = db.getResultsArray(query3);
            results = array_merge(results, results2);
        }

        counts = count(results);

        formToken = get_form_token();

        bannerHTML = "<div class="room-ad-banner" id="room-ads-preview">";
        bannerHTML += "<table cellpadding="0" cellspacing="0" style="width: " + sizeDetail["width"] + "px; height: " + sizeDetail["height"] + "px; border: solid 1px #" + adDetail["borderColor"] + ";  background-color: #" + adDetail["bgColor"] + displayBorder + "">";
        for(i = 1; i <= counts; i++){
            if(sizeDetail["type"] == "vertical" || i == 1)
                bannerHTML += "<tr>";

            bannerHTML += "<td>";
            if(results[i - 1]["type"] == "Text"){
                bannerHTML += "<div class="room-ad " + sizeDetail["class"] + " " style=" " + displayWidth + newHorizontalPadding + " ">
                                    <a href="//" + DOMAIN + "/goto-ad-url.php?key=" + results[i - 1]["adKey"] + "&" + formToken + "=1&url=" + base64_encode(results[i - 1]["url"]) + "" class="bsroom-ad-title" style="color: #" + adDetail["titleColor"] + "" target="_blank">" + results[i - 1]["title"] + "</a>
									<br />
                                    <p class="bsroom-ad-desc" style="color: #" + adDetail["textColor"] + "">" + results[i - 1]["description"] + "</p>
                                    <div style=" " + displayWidth + "overflow:hidden;">
									<a style="color: #" + adDetail["urlColor"] + "" href="//" + DOMAIN + "/goto-ad-url.php?key=" + results[i - 1]["adKey"] + "&" + formToken + "=1&url=" + base64_encode(results[i - 1]["url"]) + "" class="bsroom-ad-link" target="_blank">" + results[i - 1]["display_url"] + "</a>
									</div>
                                </div>";
            }else{
                bannerHTML += "<div class="room-ad room-ad-image"  style="padding: 0; margin: 0; line-height: 0; overflow: hidden"><a href="//" + DOMAIN + "/goto-ad-url.php?key=" + results[i - 1]["adKey"] + "&" + formToken + "=1&url=" + base64_encode(results[i - 1]["url"]) + "" target="_blank"><img src="" + DIR_WS_IMAGE + "user_ads/" + results[i - 1]["fileName"] + "" width="" + (sizeDetail["width"]) + "" height="" + (sizeDetail["height"]) + "" /></a></div>";
            }

            bannerHTML += "</td>";

            if(sizeDetail["type"] == "vertical" || i == counts)
                bannerHTML += "</tr>";

            if(results[i - 1]["defaultAd"])
                continue;

            db.query("UPDATE " + TABLE_PUBLISHER_ADS + " SET `impressions` = `impressions` + 1 WHERE id=" + adDetail["id"]);
            db.query("UPDATE " + TABLE_ADS + " SET `receivedImpressions` = `receivedImpressions` + 1 WHERE id=" + results[i - 1]["id"]);

            //Make it to expired if all expressions are received
            db.query("UPDATE " + TABLE_ADS + " SET `status` = " + AD_STATUS_EXPIRED + " WHERE  id=" + results[i - 1]["id"] + " AND `receivedImpressions` >= `impressions` ");

            //Image ads were creating multiple table rows
            if(results[i - 1]["type"] == "Image")
                break;

        }
        bannerHTML += "</table>";
        bannerHTML += "</div>";

        return bannerHTML;
    }

    /// <summery>
    /// <typeparam name=""></typeparam> userID
    /// <returns></returns> string
    /// </summery>
    public function getUserBalance(userID){
        global db;

        query = db.prepare("SELECT SUM(`impressions` - `paidImpressions`) FROM " + TABLE_PUBLISHER_ADS + " WHERE publisherID=%d", userID);
        totalImpressions = db.getVar(query);

        price_per_impression = ADS_PRICE_FOR_THOUSAND_IMPRESSIONS * ADS_PUBLISHER_PERCENTAGE / 1000;

        totalAmount = number_format(totalImpressions * price_per_impression, 8);

        return totalAmount;
    }
}