/* Version 3.0 of the NovelProjects Modal Popup
* Author: Chris Keenan
* Author: Nathan Wilkinson
* Website: http://www.novelprojects.com/ */

(function($) {

	var ie = $.browser.msie;
	var ie6 = ($.browser.msie && parseInt($.browser.version.substr(0, 1)) < 7);
	var c, c0, currentSettings;

	$.fn.npModal = function(params) {
		//-- Default Settings --//
		var settings = {
			containerTarget: '#modalTarget',
			targetClass: 'div.npContent',
			overlayClass: '.npOverlay',
			closeClass: 'div.npClose',
			dragClass: 'div.npTitle',
			enableOverlayClose: false,
			enableEscape: true,
			title: false, // set to a string value to be the modal windows title
			enableDrag: false, // if true, you must include jquery ui core, and jquery ui drag
			isModal: true, // if false, no overlay is displayed and you can still interact with the page
			animate: false, // when set to true, animations will fire
			ajax: false, // set to a path as a string of your requested page
			toTop: false, // always positions modal 15% from top of inner browser height
			destroyOverlay: true, // set to false if using parent/child modals so child doesn't remove overlay on it's close
			toggleHide: true, // used for parent/child modals whether to hide the parent on opening child
			sender: null, // used for parent/child modals as the parent sender
			overlay: 50, // percentage of opacity for overlay
			autoPosition: true, // if already positioned in css, set to false
			width: '620px',
			height: 'auto',
			closeCallback: null
		};
		currentSettings = $.extend(settings, params);
		var o = new Object();

		return this.each(function() {
			o.containerTarget = $(this);
			currentSettings.containerTarget = (o.containerTarget.attr('id') != null ? '#' + o.containerTarget.attr('id') : settings.containerTarget);
			o.targetClass = $(currentSettings.targetClass);
			o.overlayClass = $(currentSettings.overlayClass);
			o.closeClass = o.containerTarget.find(currentSettings.closeClass);
			o.dragClass = o.containerTarget.find(currentSettings.dragClass);
			if (currentSettings.sender != null)
				o.sender = $(currentSettings.sender);
			doModal(o);
		});
	};

	//-- Remove the modal from DOM --//
	$.fn.npModalDestroy = function(params) {
		if (currentSettings == undefined)
			currentSettings = new Object();

		if (params != undefined || params != null) {
			currentSettings.destroyOverlay = (params.destroyOverlay != null) ? params.destroyOverlay : true;
			currentSettings.animate = (params.animate != null) ? params.animate : false;
		}
		else {
			currentSettings.destroyOverlay = true;
			currentSettings.animate = false;
		}
		currentSettings.containerTarget = this;

		destroyModal();
	};

	function doModal(o) {
		c0 = "";
		c = "";
		c = o.containerTarget;

		var zIndex = 3000;
		var ov = $('<div></div>').css({ height: '100%', width: '100%', position: 'fixed', left: 0, top: 0, 'z-index': zIndex - 1, opacity: currentSettings.overlay / 100 });

		if (o.overlayClass.length == 0) {
			ov.addClass(currentSettings.overlayClass.substring(1, currentSettings.overlayClass.length)).appendTo('body');
			o.overlayClass = $(currentSettings.overlayClass);
		}

		if (ie6) {
			$('html,body').css({ height: '100%', width: '100%', 'z-index': zIndex - 1 });
			ov = ov.css({ position: 'absolute' });
			var pt = parseInt(c.css('padding-top').replace('px', ''), 10);
			var pb = parseInt(c.css('padding-bottom').replace('px', ''), 10);
			var pl = parseInt(c.css('padding-left').replace('px', ''), 10);
			var pr = parseInt(c.css('padding-right').replace('px', ''), 10);

			var ifr = $('<iframe></iframe>').css({ opacity: 0, width: cw + pl + pr + 'px', height: ch + pt + pb + 'px', left: 0, top: 0, 'z-index': '-1', position: 'absolute' });
			c.prepend(ifr);
		}

		if (currentSettings.autoPosition) {

			c.css({ width: currentSettings.width, height: currentSettings.height, 'z-index': zIndex }).appendTo('form');

			var cw = parseInt(c.width(), 10);
			var ch = parseInt(c.height(), 10);

			//-- Get Window Height and position in middle --//
			var top = 0;
			var h = getHeight();
			var s = getScroll();

			if (currentSettings.toTop) top = '15%';
			else top = (s == 0) ? ((h - ch) / 2) : (((h - ch) / 2) + s);
			c.css("top", top);

			//-- Get Window Width and position in middle --//
			var left = 0;
			var w = getWidth();
			left = ((w - cw) / 2);
			c.css("left", left);

			if (currentSettings.animate && !ie)
				c.fadeIn();
			else
				c.show();
		}

		if (currentSettings.isModal) o.overlayClass.css("display", "block");
		else o.overlayClass.css("display", "none");

		if (currentSettings.enableDrag) { try { c.draggable({ handle: currentSettings.dragClass }); o.dragClass.css({ cursor: "move" }) } catch (err) { /*alert('Draggable Import Needed')*/ } }

		if (currentSettings.title != false)
			o.dragClass.find("h1").text(currentSettings.title);

		if (currentSettings.ajax != false) {
			$.ajax({
				type: "GET",
				url: currentSettings.ajax,
				success: function(data, textStatus) {
					o.targetClass.html(data);
					//-- adjust height --//
					ch = parseInt(c.height(), 10);
					if (!currentSettings.toTop) {
						top = (s == 0) ? ((h - ch) / 2) : (((h - ch) / 2) + s);
						if (top < 0) top = 0;
						c.css("top", top);
					}
					//-- show --//
					if (currentSettings.animate && !ie) c.slideDown();
					else c.show();
				},
				error: function(XMLHttpRequest, textStatus, errorThrown) {
					alert('Error Loading \'' + currentSettings.ajax + '\'');
				}
			});
		}
		else {
			if (currentSettings.animate && !ie)
				c.fadeIn();
			else
				c.show();
		}

		if (currentSettings.sender != null) {
			c0 = o.sender;
			if (currentSettings.toggleHide) c0.hide();
		}
		else
			c0 = c;

		//-- Adds container target id to the close class item and binds the destroy function --//
		o.closeClass.attr('cid', currentSettings.containerTarget)
			.attr('d', currentSettings.destroyOverlay)
			.attr('a', currentSettings.animate)
			.click(destroyModal);

		//-- Keydown of escape key closes the modal --//
		if (currentSettings.enableEscape) $(document).keydown(keyHandler);
		if (currentSettings.isModal && currentSettings.enableOverlayClose) o.overlayClass.click(function() { destroyModal(); });
	}

	function destroyModal(e) {
		var cbtn = $(this);
		var containerTarget = currentSettings.containerTarget;
		var d = currentSettings.destroyOverlay;
		var a = currentSettings.animate;

		if (cbtn != null) {
			containerTarget = (cbtn.attr('cid') != null) ? $(cbtn.attr('cid')) : $(containerTarget);
			d = ((cbtn.attr('d') != null) && (cbtn.attr('d') == "false")) ? false : currentSettings.destroyOverlay;
			a = ((cbtn.attr('a') != null) && (cbtn.attr('a') == "false")) ? false : currentSettings.animate;
		}

		var o = currentSettings;

		var hasCallback = (o.closeCallback != null) ? true : false;
		if (o.isModal && d) $(o.overlayClass).css("display", "none");

		if (a && !ie) {
			if (hasCallback) containerTarget.fadeOut("normal", o.closeCallback);
			else containerTarget.fadeOut();
		}
		else {
			if (hasCallback) containerTarget.hide(1, o.closeCallback);
			else containerTarget.hide();
		}

		if (c0 != c && o.toggleHide) {
			c = containerTarget = c0;
			d = true;
			o.sender = null;
			currentSettings.destroyOverlay = d;

			if (o.animate && !ie)
				c0.fadeIn();
			else
				c0.show();
		}
	}

	function keyHandler(e) {
		if (e.keyCode == 27) destroyModal();
	}

	function getHeight() {
		var h = 0;
		if (typeof (window.innerHeight) == 'number') {
			h = window.innerHeight;
		} else if (document.documentElement && document.documentElement.clientHeight) {
			h = document.documentElement.clientHeight;
		} else if (document.body && document.body.clientHeight) {
			h = document.body.clientHeight;
		}
		return h;
	}

	function getWidth() {
		var w = 0;
		if (typeof (window.innerWidth) == 'number') {
			w = window.innerWidth;
		} else if (document.documentElement && document.documentElement.clientWidth) {
			w = document.documentElement.clientWidth;
		} else if (document.body && document.body.clientWidth) {
			w = document.body.clientWidth;
		}
		return w;
	}

	function getScroll() {
		var s = 0;
		if (typeof (window.pageYOffset) == 'number') {
			s = window.pageYOffset;
		} else if (document.body && (document.body.scrollLeft || document.body.scrollTop)) {
			s = document.body.scrollTop;
		} else if (document.documentElement && (document.documentElement.scrollLeft || document.documentElement.scrollTop)) {
			s = document.documentElement.scrollTop;
		}
		return s;
	}

})(jQuery);