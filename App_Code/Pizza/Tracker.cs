using System;
using System.Collections.Generic;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
namespace PizzaTemplate
{
	/// <summary>
	/// Summary description for Tracker
	/// </summary>
	[Authorize]
	public class Tracker : Hub
	{
		private readonly static SignalRChat.ConnectionMapping<string> _connections = 
            new SignalRChat.ConnectionMapping<string>();
		/*public Tracker()
		{
			//
			// TODO: Add constructor logic here
			//
		}*/
		public void SubmitOrder(string name, string pizza, string store, int userid)
		{
			/*var db = Database.Open("Template_pizza");
			
			var sql = @"INSERT INTO template_pizza (name, pizza, store) VALUES (@0, @1, @2)";
			
			db.Execute(sql, name, pizza, store);

            /*foreach (var connectionId in _connections.GetConnections(name))
            {
                Clients.Client(connectionId).addChatMessage(name, message);
            }*/
			//Clients.All.broadcastMessage(name, message);
			//Clients.Group(server).updateOrder(name, pizza);
		}

		public void UpdatePizza(string name, string pizza, string store)
		{
			/*var db = Database.Open("ColonielHeights");
			
			var sql = @"UPDATE template_pizza SET status=@0 WHERE name=@1 pizza=@2 store=@3";
			
			db.Execute(sql, name, pizza, store);*/
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