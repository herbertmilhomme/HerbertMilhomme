using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Rest API Configuration File
/// </summary>
public class config
{
    public config()
    {
        //
        // TODO: Add constructor logic here
        //
    }

	
	private static string PUBLIC_API_KEY = "api-key-201619920223";
	/*if(!defined("PUBLIC_API_KEY")){
		define("PUBLIC_API_KEY", "api-key-201619920223");
	}
	*/

	private static const string SITE_URL = "http://www.herbertmilhomme.com";
	/*if(!defined("SITE_URL"))
		define("SITE_URL", "http://www.herbertmilhomme.com");
	*/

	public IDictionary<string, string> api_get_error_result(string errorMessage){
		Dictionary<string, string> errorResults =
	    new Dictionary<string, string>();

		errorResults.Add("STATUS", "ERROR");
		errorResults.Add("ERROR", errorMessage);

		//return ["STATUS" => 'ERROR', 'ERROR' => errorMessage];
		return errorResults;
	}

	void api_format_date(int userID, string date, string format = "F j, Y"){
		//public HM_GLOBALS;

		int timeOffset = 0;

		Dictionary<string, string> userInfo = WebUser.getUserBasicInfo(userID);

		int timeOffset = 0; //HM_GLOBALS['timezone'][userInfo['timezone']];

		string strDate = "";

		TimeSpan now = DateTime.Now.TimeOfDay;
		DateTime today = DateTime.Today.ToString("Y-m-d");
		DateTime cToday = DateTime.Parse(date).ToString("Y-m-d");//date("Y-m-d", /*strtotime*/ DateTime.Parse(date).TimeOfDay);

		if(cToday == today){
			double h = floor((now - DateTime.Parse(date).TimeOfDay) / 3600);
			double m = floor(((now - DateTime.Parse(date).TimeOfDay) % 3600) / 60);
			double s = floor(((now - DateTime.Parse(date).TimeOfDay) % 3600) % 60);
			if(s > 40)
				m++;
			if(h > 0)
				strDate = h + " hour" + (h > 1 ? "s " : " ");
			if(m > 0)
				strDate += m + " minute" + (m > 1 ? "s " : " ");

			if(strDate == ""){
				if(s == 0)
					s = 1;
				strDate += s + " second" + (s > 1 ? "s " : " ");
			}

			strDate += "ago";
		}else{
			strDate = date(format, DateTime.Parse(date).TimeOfDay + timeOffset * 60 * 60);
			//        strDate = date("F j, Y h:i A", strtotime(date));
		}

		return strDate;
	}
}
