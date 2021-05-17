$(function() {
	if (ShowHeatmap) {
		/** Main variables */
		var doc = (document.documentElement !== undefined && document.documentElement.clientHeight !== 0) ? document.documentElement : document.body;
		var browserWidth = doc.clientWidth !== undefined ? doc.clientWidth : window.innerWidth;
		var browserHeight = doc.clientHeight !== undefined ? doc.clientHeight : window.innerHeight;

		$('#HeatmapBox').show().prependTo('body');
		$(this).pngFix();

		var now = new Date();
		$('.start').datepicker({ mandatory: true });
		$('.start').val((now.getMonth()+1) + '/01/' + now.getFullYear());
		$('.end').datepicker({ mandatory: true });
		$('.end').val((now.getMonth()+1) + '/' + now.getDate() + '/' + now.getFullYear());
		
		$('.UpdateHeatmap').click(GenerateHeatmap);
		$('.RemoveHeatmap').click(function(){
			$('.ClickText').hide();
			$('.heatmap').remove();
			return false;
		});
	}
});

function GenerateHeatmap() {
	$('.ClickText').show();
	//$(this).npModal({containerTarget:'#LoadingPopup', width:'200px'});
	var sitewidth = $('#Container').width();
	if ($('.HeatmapContainer').length>0) sitewidth = $('.HeatmapContainer').width();
	
	params = '{ path:"' + $('.PathVal').text() + '", browser:"' + $('.Browsers').val() + '", start:"' + $('.start').val() + '",' +
			' end:"' + $('.end').val() + '", width:"' + $("body").width() + '", height:"' + $("body").height() + '",' +
			' sitewidth:"' + sitewidth + '", center:"' + $('.IsCenteredVal').text() + '", query:"' + $('.NoQuery input').is(':checked') + '" }';
	$.ajax({
		type: "POST",
		url: "NovelProjects.Web.HeatMap.SaveClicks.asmx/GenerateImage",
		data: params,
		dataType: "json",
		contentType: "application/json; charset=utf-8",
		success: function(msg) {
			var object = msg.d;
			if (object==undefined || object==null) object = msg;
			object = object.parseJSON();
			$('.ClickCount').text(object.Clicks);
			$('.UniqueCount').text(object.Unique);

			$('.npOverlay').remove();
			$('#LoadingPopup').hide();
			$('.heatmap').remove();
			$('#HeatmapBox').after("<img src='" + rootpath + "utils/heatmap.png?rand=" + new Date().getMilliseconds() + "' class='heatmap hmpng' />");
			$(this).pngFix();
		},
		error: function(xhr, msg) {
			alert("fail: " + msg + '\n ' + xhr.responseText);
		}
	});
	return false;
}

//-- This method is used to parse the returned server JSON to make sure it is safe to use --//
String.parseJSON = (function(s) {
	var m = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' };
	
	s.parseJSON = function(filter) {
		try {
			if (/^("(\\.|[^"\\\n\r])*?"|[,:{}\[\]0-9.\-+Eaeflnr-u \n\r\t])+?$/.test(this)) {
				var j = eval('(' + this + ')');
				if (typeof filter === 'function') {
					function walk(k, v) {
						if (v && typeof v === 'object') {
							for (var i in v) {
								if (v.hasOwnProperty(i)) {
									v[i] = walk(i, v[i]);
								}
							}
						}
						return filter(k, v);
					}
					j = walk('', j);
				}
				return j;
			}
		} catch (e) {
		}
		throw new SyntaxError("Trouble loading data please contact support@novelprojects.com");
	};
}
)(String.prototype);
function SyntaxError(e) {
	alert(e);
}