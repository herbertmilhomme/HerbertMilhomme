using System;
using System.Collections.Generic;
using System.Web;
using System.Web.WebPages;
using System.Net;
//using config;

/// <summary>
/// Summary description for accountApi
/// </summary>
public class accountApi
{
    public accountApi()
    {
        //
        // TODO: Add constructor logic here
        //
    }

	/// <summary>
	/// Process Login from api
	/// <para>@input userID, Email and Token</para>
	/// </summary>
	/// <returns>@returns Dictionary "STATUS(Success/Error), DATA(Message/Results)"</returns>
	/// <typeparam name="loginAction">Dictionary string "STATUS", and "DATA"</typeparam>
    private void loginAction()//public IDictionary<string, object> loginAction(string userID,string email,string TOKEN)
	{
        //The login request should be POST method
        //request = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
		request.Method = "POST";
		//request.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();
        string email = WebPageRenderingBase.Request["email"] ?? WebPageRenderingBase.Request["email"].Trim();
        string password = WebPageRenderingBase.Request["password"] ?? WebPageRenderingBase.Request["password"].Trim();

		config config = new config();
		Dictionary<string, object> loginResults = new Dictionary<string, object>();
		
        if(string.IsNullOrEmpty(token))
        {
			loginResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			loginResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return loginResults; //["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }
		
        if(token != config.PUBLIC_API_KEY){
            loginResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			loginResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return loginResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        Dictionary<string, object> info = new Dictionary<string, object>(get_user_by_email(email));

        if(not_null(info) && validate_password(password, info["password"])){
            if(info["status"] == 0){ //Account is not verified
                loginResults.Add("STATUS_CODE", STATUS_CODE_OK);
				loginResults.Add("DATA", config.api_get_error_result(MSG_ACCOUNT_NOT_VERIFIED));
				return loginResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => config.api_get_error_result(MSG_ACCOUNT_NOT_VERIFIED)];
            }else{
                //Remove Old Token
                UsersToken.removeUserToken(info["userID"], "api");

                //Create New Token
                token = UsersToken.createNewToken(info["userID"], "api");

				Dictionary<string, object> dataResults = new Dictionary<string, object>();
                dataResults.Add("STATUS", "SUCCESS");
                dataResults.Add("TOKEN", token);
                dataResults.Add("EMAIL", info["email"]);
                dataResults.Add("USERID", info["userID"]);

                loginResults.Add("STATUS_CODE", STATUS_CODE_OK);
				loginResults.Add("DATA", dataResults);
				return loginResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "TOKEN" => token, "EMAIL" => info["email"], "USERID" => info["userID"]]];
            }
        }else{
            loginResults.Add("STATUS_CODE", STATUS_CODE_OK);
			loginResults.Add("DATA", config.api_get_error_result("Email or password is not correct."));
            return loginResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => config.api_get_error_result("Email or password is not correct.")];
        }
    }

	/// <summary>
	/// email, firstName, lastName, email, password, password2
	/// </summary>
    public function registerAction(){
		
        //request = _POST; //email, firstName, lastName, email, password, password2 //Dictionary Object with keys and value from a post protocol
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
		request.Method = "POST";
		//request.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> registerResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            registerResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			registerResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
			return registerResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(token != config.PUBLIC_API_KEY){
            registerResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			registerResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
			return registerResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        //Validate Input Data
        newID = User.createNewAccount(request); //Not sure if BOOL value

        if(!newID){
            //Getting Error Message
            error = get_pure_messages();

            registerResults.Add("STATUS_CODE", STATUS_CODE_OK);
			registerResults.Add("DATA", config.api_get_error_result(error));
			return registerResults; //["STATUS_CODE" => STATUS_CODE_OK, "DATA" => config.api_get_error_result(error)];
        }else{
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
            dataResults.Add("STATUS", "SUCCESS");
            dataResults.Add("USERID", newID);
            dataResults.Add("MESSAGE", MSG_NEW_ACCOUNT_CREATED);

            registerResults.Add("STATUS_CODE", STATUS_CODE_OK);
			registerResults.Add("DATA", dataResults);
			return registerResults; //["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "USERID" => newID, "MESSAGE" => MSG_NEW_ACCOUNT_CREATED]];
        }
    }

	/// <summary>
	/// Get User Basic Info
	/// </summary>
    public function getBasicInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> basicInfoResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            basicInfoResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			basicInfoResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return basicInfoResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            basicInfoResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			basicInfoResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return basicInfoResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        Dictionary<string,string> userData = User.getUserData(userID);

        Dictionary<string,string> basicInfo = new Dictionary<string, object>();
		basicInfo.Add("firstName", userData["firstName"]);
		basicInfo.Add("lastName", userData["lastName"]);
		basicInfo.Add("gender", userData["gender"]);
		basicInfo.Add("gender_visibility", userData["gender_visibility"]);
		basicInfo.Add("relationship_status", userData["relationship_status"]);
		basicInfo.Add("relationship_status_visibility", userData["relationship_status_visibility"]);
		basicInfo.Add("birthdate_year", DateTime.Parse(userData["birthdate"]).ToString("Y")); //date("Y", strtotime(
		basicInfo.Add("birthdate_month", DateTime.Parse(userData["birthdate"]).ToString("n")); //date("n", strtotime(
		basicInfo.Add("birthdate_day", DateTime.Parse(userData["birthdate"]).ToString("j")); //date("j", strtotime(
		basicInfo.Add("birthdate", DateTime.Parse(userData["birthdate"]).ToString("M/d/YYYY"));
		basicInfo.Add("birthdate_visibility", userData["birthdate_visibility"]);
		basicInfo.Add("religion", userData["religion"]);
		basicInfo.Add("religion_visibility", userData["religion_visibility"]);
		basicInfo.Add("political_views", userData["political_views"]);
		basicInfo.Add("political_views_visibility", userData["political_views_visibility"]);
		basicInfo.Add("birthplace", userData["birthplace"]);
		basicInfo.Add("birthplace_visibility", userData["birthplace_visibility"]);
		basicInfo.Add("current_city", userData["current_city"]);
		basicInfo.Add("current_city_visibility", userData["current_city_visibility"]);
        
		Dictionary<string, object> dataResults = new Dictionary<string, object>();
        dataResults.Add("STATUS", "SUCCESS");
        dataResults.Add("USER_INFO", basicInfo);

        basicInfoResults.Add("STATUS_CODE", STATUS_CODE_OK);
		basicInfoResults.Add("DATA", dataResults);
        return basicInfoResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "USER_INFO" => basicInfo]];
    }

    public function saveBasicInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> basicInfoSaveResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            basicInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			basicInfoSaveResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return basicInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            basicInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			basicInfoSaveResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return basicInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        Dictionary<string,string> userData = new Dictionary<string, object>(User.getUserData(userID));

        if(data["birthdate_year"] == "-")
            data["birthdate_year"] = "";
        if(data["birthdate_month"] == "-")
            data["birthdate_month"] = "";
        if(data["birthdate_day"] == "-")
            data["birthdate_day"] = "";

        switch(data["relationship_status"]){
            case "Single":
                data["relationship_status"] = 1;
                break;
            case "In a Relationship":
                data["relationship_status"] = 2;
                break;
            case "-":
				break;
            default:
                data["relationship_status"] = 0;
                break;
        }

        data["timezone"] = userData["timezone"];

        if(User.saveUserBasicInfo(userID, data)){//Not sure if, "NullOrEmpty" or "bool"
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
            dataResults.Add("STATUS", "SUCCESS");
            
            basicInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_OK);
			basicInfoSaveResults.Add("DATA", dataResults);
            return basicInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
        }else{
            basicInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			basicInfoSaveResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
            return basicInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
        }

        break;
    }

    public function getEducationInfoAction(){
        //request = _GET; //Dictionary Object with keys and value from a GET protocol
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
		request.Method = "GET";
		//request.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> educationInfoResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            educationInfoResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			educationInfoResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return educationInfoResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            educationInfoResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			educationInfoResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return educationInfoResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        Dictionary<string, object> educationInfo = new Dictionary<string, object>(User.getUserEducations(userID)); //may be another dictionary

		Dictionary<string, object> dataResults = new Dictionary<string, object>();
        dataResults.Add("STATUS", "SUCCESS");
		dataResults.Add("RESULT", educationInfo);

        educationInfoResults.Add("STATUS_CODE", STATUS_CODE_OK);
		educationInfoResults.Add("DATA", dataResults);
        return educationInfoResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "RESULT" => educationInfo]];
    }

    public function saveEducationInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a POST protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> educationInfoSaveResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            educationInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			educationInfoSaveResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return educationInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

		string userID;
        if(string.IsNullOrEmpty(userID = (string)UsersToken.checkTokenValidity(token, "api"))){
            educationInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			educationInfoSaveResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return educationInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        int count = data["COUNT"] ??  0;

        Dictionary<string, object> info = new Dictionary<string, object>(); //info = [];
		
        for(i = 0; i < count; i++){
            Dictionary<string, object> row = new Dictionary<string, object>(); //row = [];

            row["name"] = data["NAME" + i];
            row["start"] = data["START" + i];
            row["end"] = data["END" + i];
            row["visibility"] = data["VISIBILITY" + i];

            info = row; //info[] = row;
        }

        if(User.updateUserEducationInfo(userID, info)){
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
			dataResults.Add("STATUS", "SUCCESS");

            educationInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_OK);
			educationInfoSaveResults.Add("DATA", dataResults);
            return educationInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
        }else{
            educationInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			educationInfoSaveResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
            return educationInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
        }

        break;
    }

    public function getEmployeeInfoAction(){
        //request = _GET; //Dictionary Object with keys and value from a GET protocol
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
		request.Method = "GET";
		//request.ContentType = @"application/json; charset=utf-8";//text/plain;

        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> employeeInfoResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            employeeInfoResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			employeeInfoResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return employeeInfoResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            employeeInfoResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			employeeInfoResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return employeeInfoResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        employeeInfo = User.getUserEmploymentHistory(userID);
		Dictionary<string, object> dataResults = new Dictionary<string, object>();
		dataResults.Add("STATUS", "SUCCESS");
		dataResults.Add("RESULT", employeeInfo);

        employeeInfoResults.Add("STATUS_CODE", STATUS_CODE_OK);
		employeeInfoResults.Add("DATA", dataResults);
        return employeeInfoResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "RESULT" => employeeInfo]];
    }

    public function saveEmployeeInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a POST protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> employeeInfoSaveResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            employeeInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			employeeInfoSaveResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return employeeInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            employeeInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			employeeInfoSaveResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return employeeInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        count = data["COUNT"] ?? 0;

        Dictionary<string, object> info = new Dictionary<string, object>(); //info = [];

        for(i = 0; i < count; i++){
            Dictionary<string, object> row = new Dictionary<string, object>(); //row = [];

            row["employer"] = data["NAME" + i];
            row["start"] = data["START" + i];
            row["end"] = data["END" + i];
            row["visibility"] = data["VISIBILITY" + i];

            info = row; //info[] = row;
        }

        if(User.updateUserEmploymentHistory(userID, info)){
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
            dataResults.Add("STATUS", "SUCCESS");

            employeeInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_OK);
			employeeInfoSaveResults.Add("DATA", dataResults);
            return employeeInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
        }else{
            employeeInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			employeeInfoSaveResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
            return employeeInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
        }

        break;
    }

    public function getLinkInfoAction(){
        //request = _GET; //Dictionary Object with keys and value from a GET protocol
		HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
		request.Method = "GET";
		//request.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> linkInfoResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            linkInfoResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			linkInfoResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return linkInfoResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            linkInfoResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			linkInfoResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return linkInfoResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        linkInfo = User.getUserLinks(userID);
		Dictionary<string, object> dataResults = new Dictionary<string, object>();
        dataResults.Add("STATUS", "SUCCESS");
        dataResults.Add("RESULT", linkInfo);

        linkInfoResults.Add("STATUS_CODE", STATUS_CODE_OK);
		linkInfoResults.Add("DATA", dataResults);
        return linkInfoResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "RESULT" => linkInfo]];
    }

    public function saveLinkInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> linkInfoSaveResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            linkInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			linkInfoSaveResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return linkInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            linkInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			linkInfoSaveResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return linkInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        count = data["COUNT"] ?? 0;

        Dictionary<string, object> info = new Dictionary<string, object>(); //info = [];

        for(i = 0; i < count; i++){
            Dictionary<string, object> row = new Dictionary<string, object>(); //row = [];

            row["title"] = data["TITLE" + i];
            row["url"] = data["URL" + i];
            row["visibility"] = data["VISIBILITY" + i];

            info = row; //info[] = row;
        }

        if(User.updateUserLinks(userID, info)){
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
            dataResults.Add("STATUS", "SUCCESS");

            linkInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_OK);
			linkInfoSaveResults.Add("DATA", dataResults);
            return linkInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
        }else{
            linkInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			linkInfoSaveResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
            return linkInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
        }

        break;
    }

    public function getContactInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> contactInfoResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            contactInfoResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			contactInfoResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return contactInfoResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            contactInfoResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			contactInfoResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return contactInfoResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        contactInfo = User.getUserContactInfo(userID);
		Dictionary<string, object> dataResults = new Dictionary<string, object>();
        dataResults.Add("STATUS", "SUCCESS");
        dataResults.Add("RESULT", contactInfo);

        contactInfoResults.Add("STATUS_CODE", STATUS_CODE_OK);
		contactInfoResults.Add("DATA", dataResults);
        return contactInfoResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS", "RESULT" => contactInfo]];
    }

    public function saveContactInfoAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> contactInfoSaveResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            contactInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			contactInfoSaveResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return contactInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            contactInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			contactInfoSaveResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return contactInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        Dictionary<string, object> header = new Dictionary<string, object>(); //header = [];
        header["email"] = data["email"];
        header["work_phone"] = data["work_phone"];
        header["home_phone"] = data["home_phone"];
        header["cell_phone"] = data["cell_phone"];
        header["email_visibility"] = data["email_visibility"];
        header["home_phone_visibility"] = data["home_phone_visibility"];
        header["work_phone_visibility"] = data["work_phone_visibility"];
        header["cell_phone_visibility"] = data["cell_phone_visibility"];

        count = data["COUNT"] ?? 0;

        Dictionary<string, object> info = new Dictionary<string, object>(); //info = [];

        for(i = 0; i < count; i++){
            Dictionary<string, object> row = new Dictionary<string, object>(); //row = [];

            row["name"] = data["CONTACT_NAME" + i];
            row["type"] = data["CONTACT_TYPE" + i];
            row["visibility"] = data["VISIBILITY" + i];

            info = row; //info[] = row;
        }

        if(User.updateUserFields(userID, header) && User.updateUserMessengerInfo(userID, info)){
			Dictionary<string, object> dataResults = new Dictionary<string, object>();
            dataResults.Add("STATUS", "SUCCESS"); 

            contactInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_OK);
			contactInfoSaveResults.Add("DATA", dataResults);
            return contactInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
        }else{
            contactInfoSaveResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			contactInfoSaveResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
            return contactInfoSaveResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
        }

        break;
    }

    public function changePasswordAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> passwordChangeResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            passwordChangeResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			passwordChangeResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return passwordChangeResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            passwordChangeResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			passwordChangeResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return passwordChangeResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        current = User.getUserData(userID);

        if(!validate_password(data["current_password"], current["password"])){
            passwordChangeResults.Add("STATUS_CODE", STATUS_CODE_OK);
			passwordChangeResults.Add("DATA", config.api_get_error_result("Current password is incorrect."));
            return passwordChangeResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => config.api_get_error_result("Current password is incorrect.")];
        }else{
            pwd = encrypt_password(data["new_password"]);
			Dictionary<string, object> pwdResults = new Dictionary<string, object>();
			pwdResults["password"] = pwd;

            if(User.updateUserFields(userID, pwdResults)){//["password" => pwd]
				Dictionary<string, object> dataResults = new Dictionary<string, object>();
				dataResults.Add("STATUS", "SUCCESS"); 

                passwordChangeResults.Add("STATUS_CODE", STATUS_CODE_OK);
				passwordChangeResults.Add("DATA", dataResults);
				return passwordChangeResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
            }else{
                passwordChangeResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
				passwordChangeResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
				return passwordChangeResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
            }
        }

        break;
    }

    public function deleteAccountAction(){
        //data = _POST; //Dictionary Object with keys and value from a post protocol
		HttpWebRequest data = (HttpWebRequest)HttpWebRequest.Create(uri);
		data.Method = "POST";
		//data.ContentType = @"application/json; charset=utf-8";//text/plain;
		
        string token = WebPageRenderingBase.Request["TOKEN"] ?? WebPageRenderingBase.Request["TOKEN"].Trim();

		config config = new config();
		Dictionary<string, object> accountDeleteResults = new Dictionary<string, object>();

        if(string.IsNullOrEmpty(token)){
            accountDeleteResults.Add("STATUS_CODE", STATUS_CODE_BAD_REQUEST);
			accountDeleteResults.Add("DATA", config.api_get_error_result("Api token should not be blank"));
            return accountDeleteResults; // ["STATUS_CODE" => STATUS_CODE_BAD_REQUEST, "DATA" => config.api_get_error_result("Api token should not be blank")];
        }

        if(!(userID = UsersToken.checkTokenValidity(token, "api"))){
            accountDeleteResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
			accountDeleteResults.Add("DATA", config.api_get_error_result("Api token is not valid."));
            return accountDeleteResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("Api token is not valid.")];
        }

        current = User.getUserData(userID);

        if(!validate_password(data["password"], current["password"])){
            accountDeleteResults.Add("STATUS_CODE", STATUS_CODE_OK);
			accountDeleteResults.Add("DATA", config.api_get_error_result("Current password is incorrect."));
            return accountDeleteResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => config.api_get_error_result("Current password is incorrect.")];
        }else{
            if(User.deleteUserAccount(userID)){
				Dictionary<string, object> dataResults = new Dictionary<string, object>();
				dataResults.Add("STATUS", "SUCCESS"); 

                accountDeleteResults.Add("STATUS_CODE", STATUS_CODE_OK);
				accountDeleteResults.Add("DATA", dataResults);
				return accountDeleteResults; // ["STATUS_CODE" => STATUS_CODE_OK, "DATA" => ["STATUS" => "SUCCESS"]];
            }else{
                accountDeleteResults.Add("STATUS_CODE", STATUS_CODE_UNAUTHORIZED);
				accountDeleteResults.Add("DATA", config.api_get_error_result("There was an error to saving your information."));
				return accountDeleteResults; // ["STATUS_CODE" => STATUS_CODE_UNAUTHORIZED, "DATA" => config.api_get_error_result("There was an error to saving your information.")];
            }
        }

        break;
    }
}
