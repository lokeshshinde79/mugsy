(function($) {

jQuery.fn.pngFix = function() {

	var ie55 = (navigator.appName == "Microsoft Internet Explorer" && parseInt(navigator.appVersion) == 4 && navigator.appVersion.indexOf("MSIE 5.5") != -1);
	var ie6 = (navigator.appName == "Microsoft Internet Explorer" && parseInt(navigator.appVersion) == 4 && navigator.appVersion.indexOf("MSIE 6.0") != -1);

	if (jQuery.browser.msie && (ie55 || ie6)) {

		//fix images with png-source
		$(".hmpng").each(function() {

			$(this).attr('width',$(this).width());
			$(this).attr('height',$(this).height());
			
			var strNewHTML = '';
			var imgId = ($(this).attr('id')) ? 'id="' + $(this).attr('id') + '" ' : '';
			var imgClass = ($(this).attr('class')) ? 'class="' + $(this).attr('class') + '" ' : '';
			var imgTitle = ($(this).attr('title')) ? 'title="' + $(this).attr('title') + '" ' : '';
			var imgAlt = ($(this).attr('alt')) ? 'alt="' + $(this).attr('alt') + '" ' : '';
			var imgAlign = ($(this).attr('align')) ? 'float:' + $(this).attr('align') + ';' : '';
			var imgHand = ($(this).parent().attr('href')) ? 'cursor:hand;' : '';
			var imgStyle = (this.style.cssText);

			strNewHTML += '<span '+imgId+imgClass+imgTitle+imgAlt;
			strNewHTML += 'style="white-space:pre-line;display:inline-block;background:transparent;'+imgAlign+imgHand;
			strNewHTML += 'width:' + $(this).width() + 'px;' + 'height:' + $(this).height() + 'px;';
			strNewHTML += 'filter:progid:DXImageTransform.Microsoft.AlphaImageLoader' + '(src=\'' + $(this).attr('src') + '\', sizingMethod=\'scale\');';
			strNewHTML += imgStyle+'"></span>';
			
			$(this).hide();
			$(this).after(strNewHTML);

		});
	
	}
	
	return jQuery;

};

})(jQuery);
