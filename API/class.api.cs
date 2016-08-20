using System;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Summary description for class
/// </summary>
public class API
{
	private static const int STATUS_CODE_OK = 200;
	/*if(!defined("STATUS_CODE_OK"))
		define("STATUS_CODE_OK", 200);*/

	private static const int STATUS_CODE_BAD_REQUEST = 400;
	/*if(!defined("STATUS_CODE_BAD_REQUEST"))
		define("STATUS_CODE_BAD_REQUEST", 400);*/

	private static const int STATUS_CODE_UNAUTHORIZED = 401;
	/*if(!defined("STATUS_CODE_UNAUTHORIZED"))
		define("STATUS_CODE_UNAUTHORIZED", 401);*/

	private static const int STATUS_CODE_NOT_FOUND = 404;
	/*if(!defined("STATUS_CODE_NOT_FOUND"))
		define("STATUS_CODE_NOT_FOUND", 404);*/

	private static const int STATUS_CODE_INVALID_METHOD = 405;
	/*if(!defined("STATUS_CODE_INVALID_METHOD"))
		define("STATUS_CODE_INVALID_METHOD", 405);*/

	private static const int STATUS_CODE_INTERNAL_SERVER_ERROR = 500;
	/*if(!defined("STATUS_CODE_INTERNAL_SERVER_ERROR"))
		define("STATUS_CODE_INTERNAL_SERVER_ERROR", 500);*/

    /// <summary>
    /// REQUEST METHOD: GET, POST, PUT, DELETE
    /// @var mixed
    /// </summary>
    protected string METHOD = "";

    /// <summary>
    /// API TYPE: Authentication, POST, PAGE, FRIEND, ...
    /// @var mixed
    /// </summary>
    protected string TYPE = "";

    /// <summary>
    /// What to do: login, create or get posts, ...
    /// @var mixed
    /// </summary>
    protected string ACTION = "";

    public function __construct(object request)
	{
        System.Web.WebPages.WebPageRenderingBase.Response.AppendHeader("Access-Control-Allow-Origin","*");//header("Access-Control-Allow-Origin: *");
        System.Web.WebPages.WebPageRenderingBase.Response.AppendHeader("Access-Control-Allow-Methods","*");//header("Access-Control-Allow-Methods: *");
        System.Web.WebPages.WebPageRenderingBase.Response.ContentType = "Content-Type: application/json";//header("Content-Type: application/json");

        this.METHOD = _SERVER["REQUEST_METHOD"];
        if(this.METHOD == "POST" && array_key_exists("HTTP_X_HTTP_METHOD", _SERVER)){
            if(_SERVER["HTTP_X_HTTP_METHOD"] == "DELETE"){
                this.METHOD = "DELETE";
            }else if(_SERVER["HTTP_X_HTTP_METHOD"] == "PUT"){
                this.METHOD = "PUT";
            }else{
                throw new Exception("Unexpected Header");
            }
        }
        switch(this.METHOD)
		{
            case "DELETE":
            case "POST":
                this.TYPE = escape_query_string(_POST["TYPE"]);
                this.ACTION = escape_query_string(_POST["ACTION"]);
                break;
            case "PUT":
            case "GET":
                this.TYPE = escape_query_string(_GET["TYPE"]);
                this.ACTION = escape_query_string(_GET["ACTION"]);
                break;
            default:
                this._response("Invalid Method", STATUS_CODE_INVALID_METHOD);
				break;
        }
    }

    private function _response(string responseData, int statusCode = STATUS_CODE_OK){
        header("HTTP/1.1 " + statusCode + " " + this._status(statusCode));
		System.Web.WebPages.WebPageRenderingBase.Response.StatusCode = statusCode;
		//HttpResponse.AppendHeader() 
		//System.Web.WebPages.WebPageRenderingBase.Response.Headers[];
		//System.Web.WebPages.WebPageRenderingBase.Response.AppendHeader();
        return json_encode(responseData);
    }

    private function _status(int code){
		Dictionary<int, string> status = new Dictionary<int, string>();
        status.Add(200, "OK");
		status.Add(401, "Unauthorized");
		status.Add(404, "Not Found");
		status.Add(405, "Method Not Allowed");
		status.Add(500, "Internal Server Error");

        return status[code] ?? status[500];
    }

    public function processAction(){
        if(!this.TYPE || !this.ACTION){
            return this._response("Not Found", STATUS_CODE_NOT_FOUND);
        }

        //string classFile = dirname(__FILE__) + "/" + this.TYPE + "Api.php";

        if(!file_exists(classFile)){
            return this._response("Invalid Request", STATUS_CODE_INVALID_METHOD);
        }

        //require_once(classFile);
        string className = "" + ucfirst(this.TYPE) + "API";

        string actionName = this.ACTION + "Action";

        classObj = new className();

        if(!method_exists(classObj, actionName)){
            return this._response("Invalid Request", STATUS_CODE_INVALID_METHOD);
        }

        result = classObj.actionName();

        int status = result["STATUS_CODE"];
        string data = result["DATA"];

        System.Web.WebPages.WebPageRenderingBase.Response.Write(this._response(data, status));

    }
}
