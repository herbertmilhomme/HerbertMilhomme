using System;
using System.Collections.Generic;
using System.Web;

/// <summery>
/// Database Management Class Using mssqli
/// </summery>
class Database_MSSQL {

    //Database Link : Connection
    //private WebMatrix.Data.Database link; //.Connection.Database{get; set;}

	/// <summery>
	/// The query executed lastly
	/// </summery>
    private void last_query(string querystring){//public string last_query;
    	WebMatrix.Data.Database.Open("").Execute("INSERT INTO last_query VALUES @0", querystring);
    }

    //Last Query Error
    public string last_error;

    /// <summery>
    /// Construct
    /// </summery>
    /// <typeparam name="host">mixed</typeparam>  
    /// <typeparam name="username">mixed</typeparam>  
    /// <typeparam name="pass">mixed</typeparam>  
    /// <typeparam name="database">mixed</typeparam>  
    //public function __construct(string database){//string host,string username,string pass,
        //Init Class Member Variables
    //    this.link = null; //.Connection.Database
    //    this.last_query = null;
    //    this.last_error = null;

		//string dsn = "Server="+host+";Database="+database;

        //Establish connection to mssql using mssqli
    //    var link = WebMatrix.Data.Database.Open(database);//mssqli_connect(dsn, username, pass, database);
    //    if(mssqli_connect_errno()){
            //Console.Write("Failed to connect to mssql: (Error " + mssqli_connect_errno() + ") " + mssqli_connect_error());
    //        System.Web.WebPages.WebPageRenderingBase.Response.Write("Failed to connect to mssql: (Error " + mssqli_connect_errno() + ") " + mssqli_connect_error());
            //exit(0);
    //    }
        //if(!link.set_charset("utf8")){
        //    Console.Write("Error loading character set utf8: {0}\n", link.error);
            //exit(0);
        //}
     //   this.link = link;
     //}

    /// <summery>
    /// Fetch Rows by query
    /// </summery>
    /// <typeparam name="query">mixed</typeparam>  
    /// <typeparam name="database">string database name</typeparam>
    /// <typeparam name="key">mixed  : column name, or index number</typeparam>
    /// <typeparam name="type">mixed  : ASSOC, BOTH, NUM AND OBJ</typeparam> 
    /// <returns>Indexed Array</returns> 
    public function getResultsArray(string query, string database,dynamic key = null){//,string type = "ASSOC"
        //IEnumerable<dynamic> res = WebMatrix.Data.Database.Open(database).Query(query);//mssqli_query(this.link, query);

        this.last_query(query);// = query;
        this.last_error = null;

        //if(mssqli_errno(this.link)){
        //    this.setLastError();
        //    return null;
        //}

        IEnumerable<dynamic> rows = WebMatrix.Data.Database.Open(database).Query(query);//[];

  //      if(res != null)
		//{
  //          resultType = MSSQLI_BOTH;
  //          switch(type){
  //              case "ASSOC":
  //              case "OBJ":
  //                  resultType = MSSQLI_ASSOC;
  //                  break;
  //              case "NUM":
  //                  resultType = MSSQLI_NUM;
  //                  break;
  //              case "BOTH":
  //              default:
  //                  resultType = MSSQLI_BOTH;
  //                  break;
  //          }
  //          while(row = mssqli_fetch_array(res, resultType)){//row : col1 - colN
  //              if(type != "OBJ")
  //                  rows[] = row;else{
  //                  //Convert array to object
  //                  rowObj = new stdClass();
  //                  foreach(var pair in row){// as k => v
  //                      rowObj.k = v;
  //                  }
  //                  rows[] = rowObj;
  //              }
  //          }
  //          mssqli_free_result(res);
  //      }
        if(key != null && rows.Count > 0 && string.IsNullOrEmpty(rows[0][key])){
            //Change the array to key indexed array
            IEnumerable<dynamic> result = null;
            foreach(var row in rows){// as row
                result[key] = row[key];//result[row[key]]
            }
            rows = result;
        }
        return rows;
    }

    /// <summery>
    /// Get One Row
    /// </summery>
    /// <typeparam name="query">mixed</typeparam>  
    /// <typeparam name=""></typeparam> mixed type
    /// <returns></returns> array or null
    public function getRow(string query, string database){//, type = "ASSOC"
        //res = mssqli_query(this.link, query);

        this.last_query(query);// = query;
        this.last_error = null;

        //if(mssqli_errno(this.link)){
        //    this.setLastError();
        //    return null;
        //}

        IEnumerable<dynamic> row = WebMatrix.Data.Database.Open(database).QuerySingle(query);//[];

        //if(res){
        //    resultType = mssqlI_BOTH;
        //    switch(type){
        //        case "ASSOC":
        //        case "OBJ":
        //            resultType = mssqlI_ASSOC;
        //            break;
        //        case "NUM":
        //            resultType = mssqlI_NUM;
        //            break;
        //        case "BOTH":
        //        default:
        //            resultType = mssqlI_BOTH;
        //            break;
        //    }
        //    if(row = mssqli_fetch_array(res, resultType)){
        //        if(type == "OBJ"){
        //            //Convert array to object
        //            rowObj = new stdClass();
        //            foreach(row as k => v){
        //                rowObj.k = v;
        //            }
        //            row = rowObj;
        //        }
        //    }
        //    mssqli_free_result(res);
        //}
        return row;
    }

    /// <summery>
    /// Get One value of the query
    /// </summery>
    /// <typeparam name="query">String</typeparam>  
    /// <returns>one value</returns> 
    public object getVar(string query, string database){
        //res = mssqli_query(this.link, query);

        this.last_query(query);// = query;
        this.last_error = null;

        //if(mssqli_errno(this.link)){
        //    this.setLastError();
        //    return null;
        //}

        IEnumerable<dynamic> var = WebMatrix.Data.Database.Open(database).QueryValue(query);//null;

        //if(res){
        //    if(row = mssqli_fetch_array(res, mssqlI_NUM)){
        //        var = row[0];
        //    }
        //    mssqli_free_result(res);
        //}
        return var;
    }

    /// <summery>
    /// Set Last error if an error happened
    /// </summery>
    function setLastError(){
        this.last_error = "Query Error"  ;//mssqli_errno(this.link) + ": " + mssqli_error(this.link);
    }

    /// <summery>
    /// Get Last mssql Error
    /// </summery>
    public function getLastError(){
		IEnumerable<dynamic> error = WebMatrix.Data.Database.Open(database).QuerySingle(query);
        if(error != null){//mssqli_errno(this.link)
            return error;
        }else{
            return null;
        }
    }

    /// <summery>
    /// Escape input to prevent sql injection or error
    /// <typeparam name="input">string</typeparam>  
    /// <returns>escaped string</returns> 
    /// </summery>
    public function escapeInput(dynamic input, bool html_encode = true){
        //converts = ["<" => "&lt;", ">" => "&gt;", "'" => "&#039;", "\"" => "&quot;"];
		string escaped = string.Empty;
    //    if(){//is_array(input)
    //        //escaped = [];
    //        foreach(var pair in input){//input as k => v
    //            if(html_encode)
    //                v = str_replace(array_keys(converts), array_values(converts), v);
    //            escaped[k] = mssqli_real_escape_string(this.link, v);
    //        }

    //    }else{
    //        if(html_encode)
				//input = System.Web.WebPages.Html.HtmlHelper.Encode(input);//input.Replace("<","&lt;").Replace(">","&gt;").Replace("'","&#039;").Replace("\"","&quot;");
    //            //input = str_replace(array_keys(converts), array_values(converts), input);
    //        escaped = input;//mssqli_real_escape_string(this.link, input);
    //    }
        input = escaped;
        return escaped;

    }

    /// <summery>
    /// Prepare a SQL query for safe execution. Uses sprintf()-like syntax
    /// <para>%d (integer)</para>
    /// <para>%f (float)</para>
    /// <para>%s (string)</para>
    /// <para>%% (literal percentage sign - no argument needed)</para>
    /// </summery>
    /// <typeparam name="query">mixed</typeparam>  
    /// <returns>string|void</returns> 
    //public function prepare(string query = null) // { query, args}
    //{
    //    if(string.IsNullOrEmpty(query))
    //        return;

    //    args = func_get_args();
    //    array_shift(args);
    //    if(isset(args[0]) && is_array(args[0]))
    //        args = args[0];

    //    //query = str_replace("\"%s\"", "%s", query);
    //    //query = str_replace("'%s'", "%s", query);
    //    query = preg_replace("|(?<!%)%s|", "\"%s\"", query);

    //    array_walk(args, [&this, "escapeInput"]);

    //    return @vsprintf(query, args);
    //}

    /// <summery>
    /// Execute Insert Query
    /// </summery>
    /// <typeparam name="query">mixed</typeparam> 
    /// <typeparam name="database">database name</typeparam> 
    /// <returns>int|null|string</returns> 
    public function insert(string query, string database){
        this.last_query(query);// = query;
        this.last_error = null;

        //res = mssqli_query(this.link, query);
        try{//if(res){
            //newId = mssqli_insert_id(this.link);
			int newId = WebMatrix.Data.Database.Open(database).Execute(query);
            return newId;
        }catch{//else{
            this.setLastError();
            return null;
        }
    }

    /// <summery>
    /// <returns></returns> int|string
    /// </summery>
    public function getLastInsertId(){
        return WebMatrix.Data.Database;//mssqli_insert_id(this.link);
    }

    /// <summery>
    /// Insert Data From Array
    /// </summery>
    /// <typeparam name="table">string  : Table Name</typeparam> 
    /// <typeparam name="data">Dictionary|array    : key = Field Name, value = Field Value</typeparam> 
    /// <typeparam name="database">string  : Database Name</typeparam> 
    /// <returns>int|null|string</returns> 
    public function insertFromArray(string table,IDictionary<string,dynamic> data, string database){
        string[] query_k = new string[data.Keys];
        string[] query_v = new string[data.Values];//[];
        //foreach(var pair in data){//foreach(data as k => v){ k:key,v:value
        //    query_k += pair.Key;
        //    query_v += pair.Value;
        //}
        string query = "INSERT INTO " + table + "(" + string.Join(", ", query_k) + ") OUTPUT Inserted.ID VALUES(" + string.Join(", ", query_v) + ")";

        return this.insert(query, database);
    }

    /// <summery>
    /// Execute all kind of query
    /// </summery>
    /// <typeparam name=""></typeparam> mixed query
    /// <returns></returns> bool
    public function query( string query,  string database){// params object[]
        this.last_query(query);// = query;
        //res = mssqli_query(this.link, query);
        try{//if(!res){
			WebMatrix.Data.Database.Open(database).Execute(query);
            return true;
        }
		catch(Exception e){//else{
            this.setLastError();
			return false;
		}
    }

    /// <summery>
    /// Execute Update Query
    /// </summery>
    /// <typeparam name=""></typeparam> mixed query
    /// <returns>bool|null</returns> 
    public function update(string query, string database){
        this.last_query(query);// = query;
        this.last_error = null;

        //res = mssqli_query(this.link, query);
        try{//if(res){
			WebMatrix.Data.Database.Open(database).Execute(query);
            return true;
        }catch{//else{
            this.setLastError();
            return null;
        }
    }

    /// <summery>
    /// Update Query
    /// </summery>
    /// <typeparam name="table">String</typeparam>  
    /// <typeparam name="data">array</typeparam>   
    /// <typeparam name="where">array</typeparam>   
    /// <returns>bool|null</returns> 

    public function updateFromArray(string table,IDictionary<string,dynamic> data,IDictionary<string,dynamic> where, string database){
        string[] query_k = new string[data.Keys];
        string[] query_v = new string[data.Values];
        //query_w = [];
        //foreach(var pair in data){//foreach(data as k => v){ k:key,v:value
        //    query_v[] = "`" + k + "`="" + this.escapeInput(v) + """;
        //}
        //foreach(where as k => v){
        //    query_w[] = "`" + k + "`="" + this.escapeInput(v) + """;
        //}

		string query = "UPDATE " + table + "(" + string.Join(", ", query_k) + ")VALUES(" + string.Join(", ", query_v) + ")";
        //query = "UPDATE " + table + " SET " + implode(", ", query_v) + " WHERE 1=1 AND (" + implode(" AND ", query_w) + ")";

        return this.update(query);
    }
}



        //[];
        //foreach(
        //    query_k += pair.Key;
        //    query_v += pair.Value;
        //}
        