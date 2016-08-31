/**
* Theme: Velonic Admin Template
* Author: Coderthemes
* Modified by: Herb.
* Chat application 
*/

!function ($) {
	"use strict";

	var ChatApp = function () {
		this.$body = $("body"),
        this.$chatInput = $('.chat-input'),
        this.$chatList = $('.conversation-list'),
        this.$chatSendBtn = $('.chat-send .btn')
	};
	
	function showLiveMsgs() {//(str)
		var msgTAG = $('.conversation-list li:last').attr('id');
		var msgSer = $('#servername').val();//.attr('value');
		//var msgDate = $('#ChatStart').val();//.attr('value');
		var xhttp; /*
	            if (str == "") {
	                document.getElementById("display-text").innerHTML = "";
	                return;
	            }*/
		xhttp = new XMLHttpRequest();
		xhttp.onreadystatechange = function () {
			if (xhttp.readyState == 4 && xhttp.status == 200) {
				//document.getElementById("display-text").innerHTML = xhttp.responseText;
				$(xhttp.responseText).appendTo('.conversation-list');
			}
		}
		xhttp.open("GET", "/Layouts/LiveChat/LC2/msgs?server="+msgSer+"&tag="+msgTAG, true);
		xhttp.send();
			this.$chatList.scrollTo('100%', '100%', {
				easing: 'swing'
			});
	}

	//saves chat entry - You should send ajax call to server in order to save chat enrty
	ChatApp.prototype.save = function () {
		var chatText = this.$chatInput.val();
		//var oFormElement = this.$chatInput.val();
		var chatTime = moment().format("h:mm");
		if (chatText == "") {
			sweetAlert("Oops...", "You forgot to enter your chat message", "error");
			this.$chatInput.focus();
		} else {
			$('<li class="clearfix"><div class="chat-avatar"><img src="/_assets/images/users/avatar-2.jpg" alt="male"><i>' + chatTime + '</i></div><div class="conversation-text"><div class="ctext-wrap"><i>John Deo</i><p>' + chatText + '</p></div></div></li>').appendTo('.conversation-list');
			/*function submitForm()//oFormElement
			{
				var oFormElement = this.$chatInput.val();
				var xhr = new XMLHttpRequest();
				//xhr.onload = function(){ alert (xhr.responseText); }
				if (oFormElement != "") {
					xhr.open(oFormElement.method, oFormElement.action, true);
					xhr.send(new FormData(oFormElement));
				}
				return false;
				//document.getElementById("SendMsg").value = "";
			} /*
	var frm = $('#PostMsg');
    frm.submit(function (ev) {
        $.ajax({
            type: frm.attr('method'),
            url: frm.attr('action'),
            data: frm.serialize(),
            success: function (data) {
                //alert('ok');
				document.getElementById("SendMsg").value = "";
            }
        });

        ev.preventDefault();
    });
	*//*
			function purchaseCD() 
			{ 
				new Ajax.Updater( 
					{ success: 'CD Count', failure: 'errors' }, 
					'LongPolling.php', 
					{ 
						method:     'get', 
						parameters: { num: $('txtQty').getValue() } 
				}); 
			} */
			this.$chatInput.val('');
			this.$chatInput.focus();/* //remove scroll from save
			this.$chatList.scrollTo('100%', '100%', {
				easing: 'swing'
			});*/
		}
	},
    ChatApp.prototype.init = function () {
    	var $this = this;
    	//binding keypress event on chat input box - on enter we are adding the chat into chat list - 
    	$this.$chatInput.keypress(function (ev) {
    		var p = ev.which;
    		if (p == 13) {
    			$this.save();
    			return false;
    		}
    	});


    	//binding send button click
    	$this.$chatSendBtn.click(function (ev) {
    		$this.save();
    		return false;
    	});
    },
	//init ChatApp
    $.ChatApp = new ChatApp, $.ChatApp.Constructor = ChatApp

} (window.jQuery);