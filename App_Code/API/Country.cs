using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for Country
/// </summary>

class Country {

    /// <summery>
    /// Get country list
    /// 
    /// <typeparam name=""></typeparam> mixed status
    /// <returns></returns> Indexed
    /// </summery>
    public function getCountryList(status = 1){
        global db;

        if(isset(status)){
            query = db.prepare("SELECT * FROM " + TABLE_COUNTRY + " WHERE STATUS=%d", status);
        }else{
            query = db.prepare("SELECT * FROM " + TABLE_COUNTRY + "");
        }

        data = db.getResultsArray(query);

        return data;
    }

    /// <summery>
    /// Get country data by id
    /// 
    /// <typeparam name=""></typeparam> mixed countryID
    /// <returns></returns> stdClass
    /// </summery>
    public function getCountryById(countryID){
        global db;

        query = db.prepare("SELECT * FROM " + TABLE_COUNTRY + " WHERE countryID=%d", countryID);

        data = db.getRow(query);

        return data;
    }

    /// <summery>
    /// Get Country By Name
    /// 
    /// <typeparam name=""></typeparam> string  countryName
    /// <typeparam name=""></typeparam> integer status
    /// <returns></returns> stdClass
    /// </summery>
    public function getCountryByName(countryName, status = 1){
        global db;

        countryName = trim(countryName);

        if(countryName == "")
            return;

        if(isset(status))
            query = db.prepare("SELECT * FROM " + TABLE_COUNTRY + " WHERE lower(country_title)=lower(%s) AND STATUS=%d", countryName, status);else
            query = db.prepare("SELECT * FROM " + TABLE_COUNTRY + " WHERE lower(country_title)=lower(%s)", countryName);

        data = db.getRow(query);

        return data;
    }

}