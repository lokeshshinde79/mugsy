<%@ Page Language="C#"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>Expandable Text</title>
</head>
<body>
	<form id="form1" runat="server">
	
		<div style="padding:0px;">
			Heading: <input type="text" id="Heading"><br/>
			Content: <textarea type="text" cols="50" rows="9" id="Content"></textarea><br/>
			<input type="button" onclick="javascript:insertLink();" value="Insert Link"/>
		</div>

		<script type="text/javascript">
		if (window.attachEvent)
		{
			window.attachEvent("onload", initDialog);
		}
		else if (window.addEventListener)
		{
			window.addEventListener("load", initDialog, false);
		}

		var Heading = document.getElementById("Heading");
		var Content = document.getElementById("Content");

		var workLink = null;

		function getRadWindow()
		{
			if (window.radWindow)
			{
				return window.radWindow;
			}
			if (window.frameElement && window.frameElement.radWindow)
			{
				return window.frameElement.radWindow;
			}
			return null;
		}

		function initDialog()
		{
			var clientParameters = getRadWindow().ClientParameters; //return the arguments supplied from the parent page

			workLink = clientParameters;
		}

		function insertLink() //fires when the Insert Link button is clicked
		{
			//create an object and set some custom properties to it
			workLink.heading = Heading.value;
			workLink.content = Content.value;
			     
			getRadWindow().close(workLink); //use the close function of the getRadWindow to close the dialog and pass the arguments from the dialog to the callback function on the main page.
		}
		</script>

	</form>
</body>
</html>