/**
* Theme: CH Template
* Author: Herb
* Chat App 
*/
	function SubmitOnEnter() {
		document.getElementById("SendMsg").addEventListener("keydown", function (e) {
			if (!e) { var e = window.event; }
			e.preventDefault(); // sometimes useful

			// Enter is pressed
			if (e.keyCode == 13) { document.getElementById("PostMsg").submit(); }
		}, false);
	}
	//-----------------------------------------------------------------
	/*
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
	*/
	function submitForm(oFormElement)//
	{
		//var oFormElement = this.$chatInput.val();
		var xhr = new XMLHttpRequest();
		xhr.timeout = 4000;
		//xhr.onload = function(){ alert (xhr.responseText); }
		if (oFormElement !== "") {
			xhr.open(oFormElement.method, oFormElement.action, true);
			xhr.send(new FormData(oFormElement));
		} else { 
			alert("Oops... You forgot to enter your chat message"); 
			sweetAlert("Oops...", "You forgot to enter your chat message", "error");
		}
		document.getElementById("SendMsg").value = '';
		return false;
	}
	//-----------------------------------------------------------------
	//while mouse is up, feed msgs into div
	function showLiveMsgs() { //(str)
		//var msgTAGcount = document.getElementsByClassName('conversation-list li').length - 1;
		var msgTAGcount = document.getElementById("conversation-list").getElementsByTagName('li').length;
		var msgTAG; // = 0;
		//if(document.getElementsByClassName('conversation-msg')[msgTAGcount-1].hasAttribute('id')){
		if (msgTAGcount !== 0) {
			msgTAGcount = msgTAGcount - 1;
			//var Selectmsg = document.getElementsByClassName('conversation-msg');
			var Selectmsg = document.getElementById("conversation-list").getElementsByTagName('li');
			msgTAG = Selectmsg[msgTAGcount].getAttribute('id').substring(4);
		} else {
			msgTAG = 0;
		}; //.getAttribute();//.id;//getAttribute('id');
		var msgSer = document.getElementById('servername').getAttribute('value'); //.attr('value');
		//var docfrag = document.createDocumentFragment();//1
		var xhttp; /*
	                if (str == "") {
	                    document.getElementById("display-text").innerHTML = "";
	                    return;
	                }*/
		xhttp = new XMLHttpRequest();
		xhttp.timeout = 4000;
		xhttp.onreadystatechange = function () {
			if (xhttp.readyState == 4 && xhttp.status == 200) {
				//document.getElementById("loading").innerHTML = xhttp.responseText;
				//document.getElementById("conversation-list").appendChild(xhttp.responseText);
				//docfrag.innerHTML = xhttp.responseText;//2
				//docfrag.appendChild(xhttp.responseText);//2
				//document.getElementById("conversation-list").appendChild(docfrag);//3
				if (xhttp.responseText !== "") {
					//console.log("empty");
					document.getElementById("conversation-list").innerHTML += xhttp.responseText;
					if (xhttp.responseText.indexOf('onload="javascript:newMsgAlert()"') > -1) {
						newMsgAlert();
						var title = document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByClassName("conversation-text")[0].getElementsByTagName('i').getAttribute('value'); //$("#txtTitle").val()// "SERVERNAME : USERNAME"
						var message = document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByTagName('p').getAttribute('value'); //$("#txtMessage").val()
						var icon = document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByTagName('img').src;
						msgNotification(title,message,icon,msgSer);
						/*if (Notification.permission === "granted") {
						// If it's okay let's create a notification
							Handler.displayNotification(
								document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByClassName("conversation-text")[0].getElementsByTagName('i').getAttribute('value'), //$("#txtTitle").val()// "SERVERNAME : USERNAME"
								document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByTagName('p').getAttribute('value'), //$("#txtMessage").val()
								document.getElementById("conversation-list").getElementsByTagName('li')[msgTAGcount].getElementsByTagName('img').src, //$("#selIcon").val() === "1" ? $("#txtUrl").val() : ($("#selIcon").val() === "2" ? $("#hidFile").val() : "")
								0, //$("#txtSeconds").val()
								{tag: msgSer }
							);
						}*/
					}
				}
			}
		}
		//document.getElementById("loading").appendChild(docfrag);
		xhttp.open("GET", "/Layouts/LiveChat/LC2/msgs?server=" + msgSer + "&tag=" + msgTAG, true);
		xhttp.send();
		updateScroll(); /*
	            this.$chatList.scrollTo('100%', '100%', {
	                easing: 'swing'
	            });*/
	}
	function msgNotification(title,message,image,idKey) {
            notify.createNotification(title, {body:message, icon: image, tag: "LC2_"+idKey})
        }
	/* --------------------------------------------------------- */
	var scrolled = false
	function updateScroll() {
		if (!scrolled) {
			//var element = document.getElementsByClassName("conversation-list");
			var element = document.getElementById("conversation-list");
			element.scrollTop = element.scrollHeight;
		}
	}
	document.getElementById("conversation-list").onscroll = function () { myScroll(); };
	//document.getElementsByClassName("conversation-list").onscroll=function(){myScroll};
	function myScroll() {
		scrolled = true;
		//console.log("myScroll");
	}
	/* --------------------------------------------------------- */
	function newMsgAlert() {
		scrolled = false;
		console.log("newMsgAlert");
		if(!has_focus){newExcitingAlerts();}
		beep();
	}
	function updateDraft(draftTxt) {

	}
	/* --------------------------------------------------------- */
	//var variable = (function() {}());
	function newExcitingAlerts() {
		var oldTitle = document.title;
		var msg = "New Message"; //<p>.substring
		var timeoutId;
		var blink = function() { document.title = document.title == msg ? /*' '*/ oldTitle : msg; };//oldtitle
		var clear = function() {
		    clearInterval(timeoutId);
		    document.title = oldTitle;
		    window.onfocus = null;
		    timeoutId = null;
		};
		return function () {
		    if (!timeoutId) {
		        timeoutId = setInterval(blink, 1000);
		        window.onfocus = clear;
		    }
		};
	}
	/* --------------------------------------------------------- */
	function beep() {
    var snd = new Audio("data:audio/wav;base64,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qJrx4KOQjahZxWbcZgztj2c49nKmkId44S71j0c8eV9yDK6uPRzx5X18eDvjvQ6yKo9ZSS6l//8elePK/Lf//IInrOF/FvDoADYAGBMGb7FtErm5MXMlmPAJQVgWta7Zx2go+8xJ0UiCb8LHHdftWyLJE0QIAIsI+UbXu67dZMjmgDGCGl1H+vpF4NSDckSIkk7Vd+sxEhBQMRU8j/12UIRhzSaUdQ+rQU5kGeFxm+hb1oh6pWWmv3uvmReDl0UnvtapVaIzo1jZbf/pD6ElLqSX+rUmOQNpJFa/r+sa4e/pBlAABoAAAAA3CUgShLdGIxsY7AUABPRrgCABdDuQ5GC7DqPQCgbbJUAoRSUj+NIEig0YfyWUho1VBBBA//uQZB4ABZx5zfMakeAAAAmwAAAAF5F3P0w9GtAAACfAAAAAwLhMDmAYWMgVEG1U0FIGCBgXBXAtfMH10000EEEEEECUBYln03TTTdNBDZopopYvrTTdNa325mImNg3TTPV9q3pmY0xoO6bv3r00y+IDGid/9aaaZTGMuj9mpu9Mpio1dXrr5HERTZSmqU36A3CumzN/9Robv/Xx4v9ijkSRSNLQhAWumap82WRSBUqXStV/YcS+XVLnSS+WLDroqArFkMEsAS+eWmrUzrO0oEmE40RlMZ5+ODIkAyKAGUwZ3mVKmcamcJnMW26MRPgUw6j+LkhyHGVGYjSUUKNpuJUQoOIAyDvEyG8S5yfK6dhZc0Tx1KI/gviKL6qvvFs1+bWtaz58uUNnryq6kt5RzOCkPWlVqVX2a/EEBUdU1KrXLf40GoiiFXK///qpoiDXrOgqDR38JB0bw7SoL+ZB9o1RCkQjQ2CBYZKd/+VJxZRRZlqSkKiws0WFxUyCwsKiMy7hUVFhIaCrNQsKkTIsLivwKKigsj8XYlwt/WKi2N4d//uQRCSAAjURNIHpMZBGYiaQPSYyAAABLAAAAAAAACWAAAAApUF/Mg+0aohSIRobBAsMlO//Kk4soosy1JSFRYWaLC4qZBYWFRGZdwqKiwkNBVmoWFSJkWFxX4FFRQWR+LsS4W/rFRb/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////VEFHAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAU291bmRib3kuZGUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMjAwNGh0dHA6Ly93d3cuc291bmRib3kuZGUAAAAAAAAAACU=");  
    snd.play();
}
//beep(); if radioSound is ON play when out of focus and new msg
	/* --------------------------------------------------------- *
	var mousedownID = -1;  //Global ID of mouse down interval
	function mousedown(event) {
	if (mousedownID != -1) {  //Only stop if exists
	clearInterval(mousedownID);
	mousedownID = -1;
	}
	}
	function mouseup(event) {
	if (mousedownID == -1) {  //Prevent multiple loops!
	mousedownID = setInterval(showLiveMsgs, 500 /*execute every 500ms*);
	}
	}
	//Assign events
	document.addEventListener("mousedown", mousedown);
	document.addEventListener("mouseup", mouseup);
	//Also clear the interval when user leaves the window with mouse
	document.addEventListener("mouseout", mousedown);
	document.addEventListener("mousein", mouseup);
	//document.addEventListener("pageshow", mouseup);
	//document.addEventListener("onload", mouseup);
	//window.onload = mouseup;
	var evt = document.createEvent("MouseEvents");
	evt.initEvent("mouseup", true, true);
	document.dispatchEvent(evt);

	/* Click to Join 
	var timeout, clicker = $('#chat-window');
	//var count = 0;

	clicker.mouseup(function () {
	timeout = setInterval(function () {
	//clicker.text(count++);
	showLiveMsgs();
	}, 100); //500

	return false;
	});

	$(document).mousedown(function () {
	clearInterval(timeout);
	return false;
	});*/
	/*********************************************************************/
	var refresh_rate = 1200; //<-- In seconds, change to your needs
	var last_user_action = 0;
	var has_focus = false;
	var lost_focus_count = 0;
	var focus_margin = 300; // If we lose focus more then the margin we want to refresh


	function reset() {
		last_user_action = 0;
		console.log("Reset");
	}

	function windowHasFocus() {
		has_focus = true;
	}

	function windowLostFocus() {
		has_focus = false;
		lost_focus_count++;
		console.log(lost_focus_count + " <~ Lost Focus");
	}

	setInterval(function () {
		last_user_action++;
		refreshCheck();
	}, 500); //1000

	function refreshCheck() {
		var focus = window.onfocus;
		if ((last_user_action <= refresh_rate && has_focus && document.readyState == "complete") || lost_focus_count < focus_margin) {
			//window.location.reload(); // If this is called no reset is needed
			showLiveMsgs();
			reset(); // We want to reset just to make sure the location reload is not called.
		}

	}
	window.addEventListener("focus", windowHasFocus, false);
	window.addEventListener("blur", windowLostFocus, false);
	window.addEventListener("click", reset, false);
	//window.addEventListener("mousemove", reset, false);
	window.addEventListener("keypress", reset, false);