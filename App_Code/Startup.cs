using Microsoft.Owin;
using Owin;
using SignalRChat;

/// <summary>
/// Mapping the Hubs connection
/// ----------------------------
/// To enable SignalR in your application, create a class called Startup with the following:
/// </summary>
[assembly: OwinStartup(typeof(SignalRChat.Startup))]
namespace SignalRChat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			// Any connection or hub wire up and configuration should go here
            //this.ConfigureAuth(app);
			app.MapSignalR();
        }
    }
} 