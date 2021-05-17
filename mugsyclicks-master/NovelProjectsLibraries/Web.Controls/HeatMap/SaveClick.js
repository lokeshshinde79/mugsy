$(function() {
	if (!ShowHeatmap) {
		/** Main variables */
		var WaitTime = 0;
		var doc = (document.documentElement !== undefined && document.documentElement.clientHeight !== 0) ? document.documentElement : document.body;
		var browserWidth = doc.clientWidth !== undefined ? doc.clientWidth : window.innerWidth;
		var browserHeight = doc.clientHeight !== undefined ? doc.clientHeight : window.innerHeight;

		/** Add onmousedown event using listeners */
		$("html").mousedown(function(e) {
			LogClick(e);
		});

		/** Main function */
		function LogClick(e) {
			try {
				var x = e.clientX;
				var y = e.clientY;
				var scrollx = window.pageXOffset === undefined ? doc.scrollLeft : window.pageXOffset;
				var scrolly = window.pageYOffset === undefined ? doc.scrollTop : window.pageYOffset;
				/** Is the click in the viewing area? Not on scrollbars. The problem still exists for FF on the horizontal scrollbar */
				if (x > browserWidth || y > browserHeight) {
					return true;
				}
				/** Check if last click was at least 1 second ago */
				if (new Date().getTime() - WaitTime < 1000) {
					return true;
				}
				WaitTime = new Date().getTime();

				var 
				params = '{ x:"' + (x + scrollx) + '", y:"' + (y + scrolly) + '", width:"' + browserWidth + '", ' +
					' height:"' + browserHeight + '", path:"' + $('.PathVal').text() + '" }';
				$.ajax({
					type: "POST",
					url: "NovelProjects.Web.HeatMap.SaveClicks.asmx/SaveClick",
					data: params,
					dataType: "json",
					contentType: "application/json; charset=utf-8",
					error: function(xhr, msg) {
						//alert("fail: " + msg + '\n ' + xhr.responseText);
					}
				});
			}
			catch (err) { }
		}
	}
});