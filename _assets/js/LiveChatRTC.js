 $(function () {
 
            var WebServer = $.connection.webRTC;

            WebServer.client.broadcastMessage = function (name, message/*, userid, image, time*/) {
				//WebServer.state.msgid = "";//connectionid+userid+int
                $('#conversation-list').append('<li id="msg_"  class="clearfix conversation-msg'+ //name="'+WebServer.state.msgid+'"//'+WebServer.state.msgid+'
				//@if(msg.userid==WebSecurity.CurrentUserId || msg.ipaddress==PageData["ipAddress"]){<text>odd</text>} 
				//if((bool)msg.sent==false){<text>typing</text>}
				//'" onload="javascript:newMsgAlert()">'+
                                                '><div class="chat-avatar">'+
													/*if(msg.userid==0){<img src="/_assets/images/users/avatar-guest.jpg" alt="Anonymous Guest" />}
                                                    else{
														if(File.Exists(Href("/_assets/images/users/avatar-"+Request.Form["UserMemberID"]+".jpg"))){<text>
														<img src="@Href("/_assets/images/users/avatar-"+Request.Form["UserMemberID"]+".jpg")" alt="@Request.Form["UserAlias"].ToString()'s avatar image - @Request.Form["UserGender"].ToString()" />*/
														//--'<img src="'+image+'" alt="'+name+'\'s avatar image" />'+
														/*</text>}
														else{<img src="/_assets/images/users/avatar-guest.jpg" alt="No avatar image set" />}
													}*/
                                                    //--'<i><time datetime="'+ time +'">'+
													//--time/*.ToString("h':'mmt")*/+'</time></i>'+
                                                '</div>'+
                                                '<div class="conversation-text">'+
                                                    '<div class="ctext-wrap">'+
                                                        '<i>'+name+'</i>'+
														/*@if((bool)msg.removemsg){<p class="removed">This message has been removed.</p>}
														else{*/'<p>'+message+'</p>'+//}
                                                    '</div>'+
													//<!--<time datetime="@DateTime.SpecifyKind(msg.postdate, DateTimeKind.Utc).ToLocalTime().ToString()">@DateTime.SpecifyKind(msg.postdate, DateTimeKind.Utc).ToLocalTime().ToString("dddd', 'MMMM d', 'yyyy")</time>-->
                                                '</div>'+
                                            '</li>'
				/*'<li><strong>' + name
                    + '</strong>:&nbsp;&nbsp;' + message + '</li>'*/);
            };


			function ImageExist(url) 
			{
			   var img = new Image();
			   img.src = url;
			   return img.height != 0;
			}
            
            //$('#displayname').val(prompt('Enter your name:', '')); 
            $('#SendMsg').focus();
            $.connection.hub.start().done(function () {
                var encodedName = 'herb';//$('#UserAlias').val();//$('<div />').text($('#displayname').val()).html();
                var encodedMsg = $('#SendMsg').val();//$('<div />').text($('#SendMsg').val()).html();
				var ServerChannel = $('#ServerChannel').val();
				var encodedUserid = $('#UserId').val();
                $('#sendbtn').click(function () {
					//if(ImageExist()){}else{}
					if (encodedMsg !== "") {
						WebServer.server.send(encodedName, encodedMsg, ServerChannel, encodedUserid);
						$('#SendMsg').val('').focus();
					} else { 
						alert("Oops... You forgot to enter your chat message"); 
						sweetAlert("Oops...", "You forgot to enter your chat message", "error");
					}
                });
				$('#SendMsg').keypress(function (e) {
					if (!e) { var e = window.event; }
					e.preventDefault(); // sometimes useful

					// Enter is pressed
					if (e.keyCode == 13) { 
						//document.getElementById("PostMsg").submit();
						if (encodedMsg !== "") {
							WebServer.server.send(encodedName, encodedMsg);
							$('#SendMsg').val('').focus();
						} else { 
							alert("Oops... You forgot to enter your chat message"); 
							sweetAlert("Oops...", "You forgot to enter your chat message", "error");
						} 
					}
				});
				/*function SubmitOnEnter() {
					document.getElementById("SendMsg").addEventListener("keydown", function (e) {
						if (!e) { var e = window.event; }
						e.preventDefault(); // sometimes useful

						// Enter is pressed
						if (e.keyCode == 13) { 
							//document.getElementById("PostMsg").submit(); 
						}
					}, false);
				}*/
            });


//----------------------------
$('#message').keypress(function () {
    var encodedName = 'herb';//$('<div />').text($('#displayname').val()).html();$('#displayname').val()
    WebServer.server.isTyping(encodedName);
});

WebServer.client.sayWhoIsTyping = function (name) {
    $('#isTyping').html('<em id="'+$('#UserAlias').val()+'_typing">' + name + ' is typing</em>');
    setTimeout(function () { 
        $('#isTyping').html('&nbsp;');
    }, 3000);
};

//----------------------------
$.connection.hub.connectionSlow(function() {
    //notifyUserOfConnectionProblem(); // Your function to notify user.
});

$.connection.hub.reconnecting(function() {
    //notifyUserOfTryingToReconnect(); // Your function to notify user.
});

//Add Song from client, and post to server
/*WebServer.client.sayWhoIsTyping = function (name) {
    $('#isTyping').html('<em>' + name + ' is typing</em>');
    setTimeout(function () { 
        $('#isTyping').html('&nbsp;');
    }, 3000);
};*/

//Track song on Seeking from client, and broadcast to server

//Pause/Play toggle

//Volume Control / Mute

//Nightbot 

        });