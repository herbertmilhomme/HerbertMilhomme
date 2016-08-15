using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for config
/// </summary>
public class config
{
    public config()
    {
        //
        // TODO: Add constructor logic here
        //
    }
	/***
	 * Rest API Configuration File
	 */

	/*
	if(!defined("PUBLIC_API_KEY")){
		define("PUBLIC_API_KEY", "api-key-201619920223");
	}

	if(!defined("SITE_URL"))
		define("SITE_URL", "http://www.herbertmilhomme.com");
	}
	*/

	IDictionary<string, string> api_get_error_result(string errorMessage){
		Dictionary<string, string> errorResults =
	    new Dictionary<string, string>();

		errorResults.Add("STATUS", "ERROR");
		errorResults.Add("ERROR", errorMessage);

		//return ["STATUS" => 'ERROR', 'ERROR' => errorMessage];
		return errorResults;
	}

	function void api_format_date(string userID, string date, string format = 'F j, Y'){
		public TNB_GLOBALS;

		timeOffset = 0;

		userInfo = BuckysUser::getUserBasicInfo(userID);

		timeOffset = TNB_GLOBALS['timezone'][userInfo['timezone']];

		strDate = "";

		now = time();
		today = date("Y-m-d");
		cToday = date("Y-m-d", strtotime(date));

		if(cToday == today){
			h = floor((now - strtotime(date)) / 3600);
			m = floor(((now - strtotime(date)) % 3600) / 60);
			s = floor(((now - strtotime(date)) % 3600) % 60);
			if(s > 40)
				m++;
			if(h > 0)
				strDate = h . " hour" . (h > 1 ? "s " : " ");
			if(m > 0)
				strDate .= m . " minute" . (m > 1 ? "s " : " ");

			if(strDate == ""){
				if(s == 0)
					s = 1;
				strDate .= s . " second" . (s > 1 ? "s " : " ");
			}

			strDate .= "ago";
		}else{
			strDate = date(format, strtotime(date) + timeOffset * 60 * 60);
			//        strDate = date("F j, Y h:i A", strtotime(date));
		}

		return strDate;
	}
}
