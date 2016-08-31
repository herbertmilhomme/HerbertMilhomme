<%@ Application Language="C#" %>   
  
<script runat="server">  
  
    void Application_OnStart(object sender, EventArgs e)   
    {  
        // Code that runs on application startup  
        Application["TotalOnlineUsers"] = 0;
		Application["TotalOnlineMembers"] = 0;  
		Application["TotalVisitors"] = 0;
		// Make long polling connections wait a maximum of 110 seconds for a
		// response. When that time expires, trigger a timeout command and
		// make the client reconnect.
		//GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);
    
		// Wait a maximum of 30 seconds after a transport connection is lost
		// before raising the Disconnected event to terminate the SignalR connection.
		//GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);
    
		// For transports other than long polling, send a keepalive packet every
		// 10 seconds. 
		// This value must be no more than 1/3 of the DisconnectTimeout value.
		//GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);
    
		//RouteTable.Routes.MapHubs();
    }  

	void Application_BeginRequest(object sender, EventArgs e)
	{
        //  Code that runs when application begins  
  
	}
      
    void Application_End(object sender, EventArgs e)   
    {  
        //  Code that runs on application shutdown  
  
    }  
          
    void Application_Error(object sender, EventArgs e)   
    {   
        // Code that runs when an unhandled error occurs  
  
    }  
  
    void Session_OnStart(object sender, EventArgs e)   
    {  
        // Code that runs when a new session is started  
        Application.Lock(); 
		if ((HttpContext.Current != null) && (HttpContext.Current.Session != null)) {
			Session["ASP.NET_SessionId"] = HttpContext.Current.Session.SessionID;
		}else{
			Session["ASP.NET_SessionId"] = System.Guid.NewGuid().ToString();
		}
		//if(HttpContext.Request.QueryString["AffliateID"]!=null){HttpContext.Response.Cookies["AffliateID"] = HttpContext.Request.QueryString["AffliateID"];}//	Session["ASP.NET_SessionId"] = }
		//if(HttpContext.Request.QueryString["ReferralID"]!=null){HttpContext.Response.Cookies["ReferralID"] = HttpContext.Request.QueryString["ReferralID"];}
        Application["TotalOnlineUsers"] = (int)Application["TotalOnlineUsers"] + 1;  //count users on page
        Application["TotalVisitors"] = (int)Application["TotalVisitors"] + 1;  //count visitors to website
        Application.UnLock();  
		string connectionString = "Data Source=mssql.webmatrix-appliedi.net;Initial Catalog=HerbServer1;User ID=HMServer;Password=HMadmin1";//@"Data Source=|DataDirectory|\ColonielHeights.sdf";
		//"Data Source=" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\;Initial Catalog=ColonielHeights.sdf; Integrated Security=SSPI;";
		//"data source=C:\Users\topho\Dropbox\Apps\Azure\colonielheights\App_Data\ColonielHeights.sdf";
		//ConsoleApplication1.Properties.Settings.Default.ConnectionString;
		using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
		//using (System.Data.SqlServerCe.SqlCeConnection connection = new  System.Data.SqlServerCe.SqlCeConnection(connectionString))
		{
			connection.Open();
			//insert into database new session key
			//on login update database
			try{
				using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand("INSERT INTO website_whoisonline (SessionID, LoginTime) VALUES (@SessionID, CONVERT(varchar(50),@LoginTime,101))", connection))
				//using (System.Data.SqlServerCe.SqlCeCommand command = new System.Data.SqlServerCe.SqlCeCommand("INSERT INTO website_whoisonline (SessionID) VALUES (@SessionID)", connection))
				{
					command.Parameters.Add(new System.Data.SqlClient.SqlParameter("SessionID", (string)Session["ASP.NET_SessionId"]));
					command.Parameters.Add(new System.Data.SqlClient.SqlParameter("LoginTime", (string)DateTimeOffset.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss' 'zzz")));
					//command.Parameters.Add(new System.Data.SqlServerCe.SqlCeParameter("SessionID", System.Web.WebPages.WebPageRenderingBase.Session["SESSION_KEY"]));
					command.ExecuteNonQuery();
				}
			}
			catch{}
		}
    }  
  
    void Session_OnEnd(object sender, EventArgs e)   
    {  
        // Code that runs when a session ends.   
        // Note: The Session_End event is raised only when the sessionstate mode  
        // is set to InProc in the Web.config file. If session mode is set to StateServer   
        // or SQLServer, the event is not raised.  
        Application.Lock();  
        Application["TotalOnlineUsers"] = (int)Application["TotalOnlineUsers"] - 1;  
        Application["TotalOnlineMembers"] = (int)Application["TotalOnlineMembers"] - 1; 
        Application.UnLock();  
		string connectionString = "Data Source=mssql.webmatrix-appliedi.net;Initial Catalog=HerbServer1;User ID=HMServer;Password=HMadmin1";//@"Data Source=|DataDirectory|\ColonielHeights.sdf";//"Data Source=" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\;Initial Catalog=ColonielHeights.sdf; Integrated Security=SSPI;";//"data source=C:\Users\topho\Dropbox\Apps\Azure\colonielheights\App_Data\ColonielHeights.sdf";//ConsoleApplication1.Properties.Settings.Default.ConnectionString;
		using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
		//using (System.Data.SqlServerCe.SqlCeConnection connection = new  System.Data.SqlServerCe.SqlCeConnection(connectionString))
		{
			connection.Open();
			//insert into database new session key
			//on login update database
			try{
				using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand("UPDATE website_whoisonline SET LogoutTime=getdate(), IsOnline='false' WHERE SessionID='@SessionID'", connection))
				//using (System.Data.SqlServerCe.SqlCeCommand command = new System.Data.SqlServerCe.SqlCeCommand("UPDATE website_whoisonline SET LogoutTime=getdate(), IsOnline='false' WHERE [SessionID]='@SessionID'", connection))
				{
					command.Parameters.Add(new System.Data.SqlClient.SqlParameter("SessionID", (string)Session["ASP.NET_SessionId"]));
					//command.Parameters.Add(new System.Data.SqlServerCe.SqlCeParameter("SessionID", System.Web.WebPages.WebPageRenderingBase.Session["SESSION_KEY"]));
					//command.Parameters.Add(new SqlParameter("Logout", now()));
					command.ExecuteNonQuery();
				}
			}
			catch{}
		}
    }           
</script>  