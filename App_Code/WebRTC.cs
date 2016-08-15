using System;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using WebMatrix.Data;
namespace SignalRChat
{
	/// <summary>
	/// Summary description for WebRTC
	/// </summary>
	[Authorize]
	public class WebRTC : Hub
	{
		private readonly static ConnectionMapping<string> _connections = 
            new ConnectionMapping<string>();
		/*public WebRTC()
		{
			//
			// TODO: Add constructor logic here
			//
		}*/
		public void Send(string name, string message, string server, int userid)
		{
			var db = Database.Open("ColonielHeights");
			//var messages = db.QuerySingle("SELECT * FROM website_livechat WHERE date>'@0' ORDER BY date DESC", date);	//date.ToString("d")
			//string SendMsg = string.Empty;

			var sql = @"INSERT INTO website_livechat (userid, userName, ipaddress, postmsg, server, alias, sent, postdate) VALUES (@0, @1, @2, @3, @4, @5, @6, CONVERT(varchar(50),@7,101))";
			//var alias = WebSecurity.IsAuthenticated ? db.QueryValue("SELECT Alias FROM UserProfile WHERE userid=@0",WebSecurity.CurrentUserId) : Request.Cookies["alias"].Value.ToString();
			/*int userid = WebSecurity.IsAuthenticated ? WebSecurity.CurrentUserId : 0;
			var username = WebSecurity.IsAuthenticated ? db.QueryValue("SELECT Username FROM UserProfile WHERE userid=@0",WebSecurity.CurrentUserId) : "Guest";

			System.Web.HttpContext context = System.Web.HttpContext.Current; 
			string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
			if (!string.IsNullOrEmpty(ipAddress))
			{
				string[] addresses = ipAddress.Split(',');
				if (addresses.Length != 0)
				{
					ipAddress = addresses[0];
				}
			}
			else{ipAddress = context.Request.ServerVariables["REMOTE_ADDR"];}*/

			//db.Execute(sql, userid/*(int)Page.userid*/, (string)Page.username, (string)Page.ipAddress /*//Context.ConnectionId*/, message/*(string)Request.Form["SendMsg"]*/, server/*(int)int.Parse(Request.QueryString["server"])*/, name/*(string)alias*/, true, (string)DateTimeOffset.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss' 'zzz"));
		    //string name = WebMatrix.WebData.WebSecurity.IsAuthenticated? WebMatrix.WebData.WebSecurity.CurrentUserName : Context.User.Identity.Name;

            /*foreach (var connectionId in _connections.GetConnections(name))
            {
                Clients.Client(connectionId).addChatMessage(name, message);
            }*/
			//Clients.All.broadcastMessage(name, message);
			Clients.Group(server).addChatMessage(name, message);
		}
		/*public async Task JoinGroup(string groupName)
		{
			await Groups.Add(Context.ConnectionId, groupName);
			//Clients.Group(groupname).addContosoChatMessageToPage(Context.ConnectionId + " added to group");
		}*/
		public Task JoinRoom(string roomName)
		{
			return Groups.Add(Context.ConnectionId, roomName);
		}

		public Task LeaveGroup(string groupName)
		{
			return Groups.Remove(Context.ConnectionId, groupName);
		}
		public void IsTyping(string name){
			Clients.All.sayWhoIsTyping(name);
		}

		//-------
		public override Task OnConnected()
        {
			// Add your own code here.
			// For example: in a chat application, record the association between
			// the current connection ID and user name, and mark the user as online.
			// After the code in this method completes, the client is informed that
			// the connection is established; for example, in a JavaScript client,
			// the start().done callback is executed.
			string name = Context.User.Identity.Name;

            _connections.Add(name, Context.ConnectionId);

            // Retrieve user.
            var user = 0;

            // If user does not exist in database, must add.
            if (WebMatrix.WebData.WebSecurity.IsAuthenticated)
            {
                user = WebMatrix.WebData.WebSecurity.CurrentUserId;
            }
            else
            {
                // Add to each assigned group.
                /*foreach (var item in user.Rooms)
                {
                    Groups.Add(Context.ConnectionId, item.RoomName);
                }*/
            }
            return base.OnConnected();
        }

		/*public override Task OnDisconnected()
		{
			// Add your own code here.
			// For example: in a chat application, mark the user as offline, 
			// delete the association between the current connection id and user name.
			return base.OnDisconnected();
		}
		public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
		{
			//string name = Context.User.Identity.Name;

            //_connections.Remove(name, Context.ConnectionId);

			if (stopCalled)
			{
				Console.WriteLine(String.Format("Client {0} explicitly closed the connection.", Context.ConnectionId));
			}
			else
			{
				Console.WriteLine(String.Format("Client {0} timed out .", Context.ConnectionId));
			}
            
			return base.OnDisconnected();
		}*/

		public override Task OnReconnected()
		{
			// Add your own code here.
			// For example: in a chat application, you might have marked the
			// user as offline after a period of inactivity; in that case 
			// mark the user as online again.
			/*string name = Context.User.Identity.Name;

            if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                _connections.Add(name, Context.ConnectionId);
            }*/

            return base.OnReconnected();
		}
	}
}