$(function(){
	$(".slider").unbind('click').click( function() {
		$(".slideOut").slideUp(400);
		if ($(this).nextAll(".slideOut:first").css("display")=="none")
			$(this).nextAll(".slideOut:first").slideDown(400);
		if ($(this).parent("p").nextAll(".slideOut:first").css("display")=="none")
		{
			$(this).parent("p").nextAll(".slideOut:first").slideDown(400);
		}
	});
});

function CheckSlider(){ };

/*
* Address Scrabmler
*/

var offset = 7;

function encode(str) {
	var encoded = "";
	for (var i = 0; i < str.length; i++) {
		var c = str.charCodeAt(i) + offset;
		encoded += c;
		if (i < str.length - 1) encoded += ",";
	}
	document.write(encoded);
}

function decode() {
	var str = "";
	for (var i = 0; i < arguments.length; i++) {
		var p = arguments[i] - offset;
		str += String.fromCharCode(p)
	}
	document.write('<a href="mailto:' + str + '">' + str + '</a>');
}

function decodewithclass(email, className) {
	var str = "";
	email = email.split(",");

	for (var i = 0; i < email.length; i++) {
		var p = email[i] - offset;
		str += String.fromCharCode(p)
	}
	document.write('<a href="mailto:' + str + '" class="' + className + '">' + str + '</a>');
}

function decodeWithCustomDisplay(email, display) {
	var str = "";
	email = email.split(",");

	for (var i = 0; i < email.length; i++) {
		var p = email[i] - offset;
		str += String.fromCharCode(p)
	}
	document.write('<a href="mailto:' + str + '">' + display + '</a>');
}

function decodeWithCustomDisplayClass(email, display, className) {
	var str = "";
	email = email.split(",");

	for (var i = 0; i < email.length; i++) {
		var p = email[i] - offset;
		str += String.fromCharCode(p)
	}
	document.write('<a href="mailto:' + str + '" class="' + className + '">' + display + '</a>');
}