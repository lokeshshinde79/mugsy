using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Xml;
using FredCK.FCKeditorV2;

namespace NovelProjects.Web
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:EditableContent runat=server></{0}:EditableContent>")]
	public class EditableContent : WebControl
	{
		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		private Boolean CanEdit, CanApprove, IsApproved;
		private Guid EditedUser;
		private DateTime LastModified;
		private LiteralControl lt;
		private FCKeditor Editor = new FCKeditor();
		private PlaceHolder ph = new PlaceHolder();

		private string ConnString;
		private string Url;
		private string VersionId;
		#endregion

		#region public variables
		//If jquery is not being imported to the page
		public bool NotFirstInstance { get; set; }
		//The text value for the content area
		public string Text { get; set; }
		//The content id to associate text with
		public string ContentID { get; set; }
		// Used for demo account
		public bool DemoMode { get; set; }
		#endregion

		#region Loads the text value for this content area
		void LoadContent()
		{
			DataTable dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnString].ConnectionString))
			{
				conn.Open();
				string sql = "SELECT TOP 1 ID,ContentText,LastModified,IsApproved,LastModifiedUserId FROM EditableContent WHERE Url=@Url AND ContentID=@ContentID AND ApplicationId=@ApplicationId AND IsPublished=1;";
				if (CanEdit)
					sql = "SELECT TOP 1 ID,ContentText,LastModified,IsApproved,LastModifiedUserId FROM EditableContent WHERE ID=(SELECT TOP 1 EditableContentId FROM EditableContent WHERE Url=@Url AND ContentID=@ContentID AND ApplicationId=@ApplicationId AND IsPublished=1);";
				SqlCommand sqlcmd = new SqlCommand(sql, conn);
				sqlcmd.Parameters.Add("@Url", SqlDbType.VarChar);
				sqlcmd.Parameters.Add("@ContentID", SqlDbType.VarChar);
				sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);

				sqlcmd.Parameters["@Url"].Value = Url;
				sqlcmd.Parameters["@ContentID"].Value = ContentID;
				sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(dt);

				if (dt.Rows.Count == 1)
				{
					DataRow dr = dt.Rows[0];
					Text = HttpContext.Current.Server.HtmlDecode(dr["ContentText"].ToString());
					if (CanEdit && (Text.Trim().Length == 0 || Text.Trim().Equals("<br />")))
						Text = "<br/><br/>";
					LastModified = Convert.ToDateTime(dr["LastModified"]);
					IsApproved = Convert.ToBoolean(dr["IsApproved"]);
					VersionId = dr["ID"].ToString();

					if (!string.IsNullOrEmpty(dr["LastModifiedUserId"].ToString())) EditedUser = (Guid)dr["LastModifiedUserId"];
				}
				else
				{
					Text = "<p>Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Donec diam nulla, hendrerit et, lacinia et, ornare vel, ante. " +
						"Nam sed erat non augue nonummy " +
						"congue. Vestibulum ullamcorper. Donec sagittis massa eu neque. Etiam magna. " +
						"Sed elementum bibendum mi.</p>" +
						"<ul> <li>Bulleted list</li> <li>list item</li> </ul>";

					//Insert the unpublished version
					Guid EditableContentId = Guid.NewGuid();
					sqlcmd.CommandText = "INSERT INTO EditableContent (ID,Url,ContentID,ContentText,ApplicationId,IsApproved) VALUES (@ID,@Url,@ContentID,@ContentText,@ApplicationId,1);";
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ContentText", SqlDbType.VarChar);

					sqlcmd.Parameters["@ID"].Value = EditableContentId;
					sqlcmd.Parameters["@ContentText"].Value = HttpContext.Current.Server.HtmlEncode(Text);
					sqlcmd.ExecuteNonQuery();

					//Insert the published version
					sqlcmd.CommandText = "INSERT INTO EditableContent (Url,ContentID,ContentText,ApplicationId,IsApproved,IsPublished,EditableContentId) VALUES (@Url,@ContentID,@ContentText,@ApplicationId,1,1,@ID);";
					sqlcmd.ExecuteNonQuery();

					IsApproved = true;
					LastModified = DateTime.Now;
					VersionId = EditableContentId.ToString();
				}
			}
		}
		#endregion

		#region Writes out the editable div or standard text
		protected override void RenderContents(HtmlTextWriter output)
		{
			//Code to spam protect emails.
			Regex reg = new Regex(@"<a[^>]*href=[""']?mailto:(\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)['""]?>([^<]+)</a>");
			foreach (Match m in reg.Matches(Text))
			{
				string email = reg.Replace(m.Value, "$1");
				string name = reg.Replace(m.Value, "$5");
				if (email == name)
					Text = Text.Replace(m.Value, email);
				else
					Text = Text.Replace(m.Value, "<script>decodeWithCustomDisplay('" + Encode(email) + "', '" + name + "')</script>");
			}
			reg = new Regex(@"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
			foreach (Match m in reg.Matches(Text))
			{
			  Text = Text.Replace(m.Value, "<script>decode(" + Encode(m.Value) + ")</script>");
			}

			if (!CanEdit)
				output.Write("<div>" + HttpContext.Current.Server.HtmlDecode(Text) + "</div>");
		}
		#endregion

		#region Obfuscate e-mail addresses to block spam bots
		public static String Encode(String address)
		{
			String encoded = "";
			int offset = 7;
			for (int i = 0; i < address.Length; i++)
			{
				encoded += Convert.ToInt32(address[i] + offset);
				if (i < address.Length - 1) encoded += ",";
			}
			return encoded;
		}
		#endregion

		#region Initializes all of the controls
		protected override void OnInit(EventArgs e)
		{
			HostingEnvironment.RegisterVirtualPathProvider(new AssemblyResourceProvider());

			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			LoadSettings();

			//Loads the text value
			LoadContent();

			lt = new LiteralControl();

			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.EditableContent.EditableContent.css") + "' />";
			if (CanEdit)
			{
				lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.jquery-ui-1.7-core.css") + "' />";
				lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.jquery-ui-1.7-tabs.css") + "' />";
			}

			lt.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";
			lt.Text += "<script type=\"text/javascript\">$(function(){\n";
			lt.Text += "try { $('TestTooltip').tooltip(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery.tooltip.min.js") + "'); }\n";
			lt.Text += "$.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.npmodal.min.js") + "');\n";
			lt.Text += "try { $(this).bgIframe(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery.bgiframe.min.js") + "'); }\n";
			lt.Text += "try { $('.TestTooltip').tabs();\n\t";
			lt.Text += " $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.EditableContent.EditableContent.min.js") + "');\n";
			lt.Text += "} catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery-ui-1.7.min.js") + "', function(){\n" +
				"$.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.EditableContent.EditableContent.min.js") + "'); }); }\n";
			lt.Text += "try { CheckSlider(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.EditableContent.Slider.min.js") + "'); }\n";
			lt.Text += " });</script>\n";

			Controls.Add(ph);
			ph.Controls.Add(lt);
			if (CanEdit)
			{
				BuildEditor();
			}

			base.OnLoad(e);
		}
		#endregion

		#region Renders all of the controls in the page
		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();

			//Render the main controls on the page
			ph.RenderControl(writer);

			base.Render(writer);
		}
		#endregion

		#region Build content popup
		private void BuildEditor()
		{
			Literal lit = new Literal();
			// Have to load the editor so we can get the client id later.
			LoadEditor();

			string css = "red";
			if (IsApproved)
				css = "green";
			lit.Text += "<div title='";
			// Add the username if exists
			if (Membership.GetUser(EditedUser) != null) lit.Text += "Edited By: " + Membership.GetUser(EditedUser).UserName + "<br/>";
			lit.Text += 
				"Last Modified: " + LastModified + "<br/>" +
				"<span class=" + css + ">" + (IsApproved ? "Approved" : "Not Approved") +
				"</span><br/>" +
				"Double-click to edit this content' id='" + ContentID + "' class='EditableTooltip EditBoxOff'>" +
				HttpContext.Current.Server.HtmlDecode(Text) +
				"<span class='ContentID hide'>" + ContentID + "</span>" +
				"<div id='Popup_" + ContentID + "' class='npContainer EditableContent'>" +
				"<div id='CustomEditorContent' class='npContent'>";

			lit.Text += "<div class='npTabs'>" +
				"<ul>" + // Tab navigation
				"<li><a href='#EditContent" + ContentID + "'><span>Edit Content</span></a></li>" +
				"<li><a href='#ContentHistory" + ContentID + "'><span>Versions</span></a></li>" +
				"</ul>";
			lit.Text += "<div id='EditContent" + ContentID + "'>";
			ph.Controls.Add(lit);

			ph.Controls.Add(Editor);

			lit = new Literal();
			lit.Text += "<span class='VersionId' style='display:none;'>" + VersionId + "</span><p class='npSpace'>&nbsp;</p>";
			lit.Text += "<span class='modifyapprove'>" + "<strong>Status:</strong> " +
									"<span class=" + css + ">" + (IsApproved ? "Approved" : "Not Approved") + "</span>&nbsp;&nbsp;&nbsp;" +
									"<strong>Last Modified:</strong> " + LastModified;
			if (Membership.GetUser(EditedUser) != null)
				lit.Text += "&nbsp;&nbsp;&nbsp;<strong>Modified By:</strong> " + Membership.GetUser(EditedUser).UserName;
			lit.Text += "</span><p class='npSpace'>&nbsp;</p><p class='npSpace'>&nbsp;</p>";
			ph.Controls.Add(lit);

			Button btn = new Button
			{
				CssClass = "CancelEditableContent",
				Text = DemoMode ? "Close" : "Cancel",
				CausesValidation = false
			};
			btn.Style["float"] = "right";
			ph.Controls.Add(btn);

			btn = new Button
			{
				CssClass = "SaveEditableContent",
				Enabled = !DemoMode,
				Text = "Save",
				CausesValidation = false
			};
			ph.Controls.Add(btn);

			btn = new Button
			{
				CssClass = "DisableSaveEditableContent",
				Enabled = false,
				Text = "processing...",
				CausesValidation = false
			};
			ph.Controls.Add(btn);

			if (!DemoMode)
			{
				CheckBox cb;
				if (CanApprove)
				{
					cb = new CheckBox
					{
						CssClass = "ApproveEditableContent",
						Text = "Approve on save"
					};
					ph.Controls.Add(cb);

					lit = new Literal();
					lit.Text += "&nbsp;&nbsp;";
					ph.Controls.Add(lit);
				}

				cb = new CheckBox
				{
					CssClass = "SaveNewContent",
					Text = "Save as new"
				};
				ph.Controls.Add(cb);
			}

			//<asp:TextBox ID="PageTitle" runat="server" style="display:none;" />

			lit = new Literal();
			if (DemoMode)
				lit.Text += "Some features are disabled in demo mode.";
			lit.Text += "<span class='Editor_" + ContentID +" hide'>" + Editor.ClientID + "</span></div>";//Close Content Editor Tab
			lit.Text += "<div id='ContentHistory" + ContentID + "'>";
			ph.Controls.Add(lit);

			LoadVersions();

			lit = new Literal();
			lit.Text += "</div></div>";//End Tabs
			lit.Text += "</div>";
			lit.Text += "</div>";//End EditableContentPopup
			lit.Text += "</div>";// Close EditableTooltip
			ph.Controls.Add(lit);
		}
		#endregion

		#region Load Editor
		void LoadEditor()
		{
			Editor.ID = "Editor_" + ContentID;
			Editor.Value = Text;
			Editor.Height = 435;
		}
		#endregion

		#region Load Versions
		void LoadVersions()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConnString].ConnectionString))
			{
				conn.Open();

				SqlCommand cmd = new SqlCommand("SELECT ID,Name,LastModified,LastModifiedUserId,IsApproved,IsNull((SELECT 1 FROM EditableContent ec WHERE EditableContentId=EditableContent.ID AND IsPublished=1), 0) AS IsPublished FROM EditableContent WHERE Url=@Url AND ContentID=@ContentID AND ApplicationId=@ApplicationId AND IsPublished=0 ORDER BY IsPublished DESC, Created;", conn);
				cmd.Parameters.AddWithValue("@Url", Url);
				cmd.Parameters.AddWithValue("@ContentID", ContentID);
				cmd.Parameters.AddWithValue("@ApplicationId", ConfigurationManager.AppSettings["AppId"]);
				SqlDataAdapter adapter = new SqlDataAdapter(cmd);
				adapter.Fill(dt);
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("<table class='HistoryPages' editorid='" + Editor.ClientID + "' cellpadding='4' cellspacing='0' width='100%'>");
			sb.Append("<tr><th align=left>Name</th>");
			sb.Append("<th>Approve</th><th>Publish</th><th align=left>Last Modified</th><th align=left>Last Modified By</th>");
			sb.Append("<th width='16'/><th width='16'/><th width='16'/>");
			sb.Append("</tr>");
			for (int i = 0; i < 10; i++)
			{
				sb.Append("<tr class='" + (i % 2 == 0 ? "" : "alt") + "'>");

				if (dt.Rows.Count > i)
				{
					dr = dt.Rows[i];

					sb.Append("<td><input id='" + dr["ID"] + "' " + (!DemoMode ? "class='EditableName'" : "disabled=disabled") + " maxlength='20' style='width:120px;' value='" + dr["Name"] + "' /></td>");

					sb.Append("<td align=center>");
					if (Convert.ToBoolean(dr["IsApproved"]))
						sb.Append("<img src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.accept.png") + "' title='This item is approved.' />");
					else if (CanApprove && !DemoMode)
						sb.Append("<input id='" + dr["ID"] + "' class='btnApproveContent' type='checkbox' />");
					sb.Append("</td>");

					sb.Append("<td align=center>");
					if (Convert.ToBoolean(dr["IsPublished"]))
						sb.Append("<img src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.accept.png") + "' title='This item is published.' />");
					else if (CanApprove && !DemoMode)
						sb.Append("<input id='" + dr["ID"] + "' class='btnPublishContent' type='checkbox' />");
					sb.Append("</td>");

					sb.Append("<td>" + dr["LastModified"] + "</td>");
					sb.Append("<td>");
					if (!string.IsNullOrEmpty(dr["LastModifiedUserId"].ToString()) && Membership.GetUser(dr["LastModifiedUserId"]) != null)
						sb.Append(Membership.GetUser(dr["LastModifiedUserId"]).UserName);
					sb.Append("</td>");

					sb.Append("<td>");
					if (dt.Rows.Count < 10 && !DemoMode)
						sb.Append("<img id='" + dr["ID"] + "' class='btnCopyContent' src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.copy.gif") + "' title='Copy' alt='Copy' />");
					sb.Append("</td>");

					sb.Append("<td>");
					sb.Append("<img id='" + dr["ID"] + "' " + (!DemoMode ? "class='btnEditContent'" : "") + " src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.pencil.gif") + "' title='Edit' alt='Edit' />");
					sb.Append("</td>");

					sb.Append("<td>");
					if (!Convert.ToBoolean(dr["IsPublished"]))
						sb.Append("<img id='" + dr["ID"] + "' " + (!DemoMode ? "class='btnDeleteContent'" : "") + " src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.bin.gif") + "' title='Delete' alt='Delete' />");
					sb.Append("</td>");
				}
				else
				{
					sb.Append("<td><a href='javascript:void(0);' " + (!DemoMode ? "class='btnCreateNewVersion'" : "") + ">Create New</a></td><td/><td/><td/><td/><td/><td/><td/>");
				}

				sb.Append("</tr>");
			}

			sb.Append("</table>");

			sb.Append("<br /><br />");
			sb.Append("<strong>Approve:</strong> Marks the content as approved and able to be published.<br/>");
			sb.Append("<strong>Publish:</strong> Marks the content as approved and publishes it.<br/>");
			sb.Append("<img align='absmiddle' src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.copy.gif") + "'/> <strong>Copy:</strong> Copy this version into a new entry.<br/>");
			sb.Append("<img align='absmiddle' src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.pencil.gif") + "'/> <strong>Edit:</strong> Allows you to edit the content for this version.<br/>");
			sb.Append("<img align='absmiddle' src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.images.bin.gif") + "'/> <strong>Delete:</strong> Deletes this version from the system.");

			Literal lit = new Literal();
			lit.Text += sb.ToString();
			ph.Controls.Add(lit);
		}
		#endregion

		#region Load Settings
		private void LoadSettings()
		{
			if (HttpContext.Current.Application["ROOTPATH"] == null)
				throw new Exception("Application[\"ROOTPATH\"] is not set.");
			Url = HttpContext.Current.Request.Url.AbsolutePath.Substring(HttpContext.Current.Application["ROOTPATH"].ToString().Length);
			if (string.IsNullOrEmpty(ContentID))
				ContentID = ID;
			ConnString = LoadConnectionString();

			if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CMSApprovalFromEmail"]))
				throw new Exception("AppSettings CMSApprovalFromEmail has not been set.");

			//If user is in the correct role allow them to modify the text
			CanEdit = LoadCanEdit();
			CanApprove = LoadCanApprove();
		}
		#endregion

		#region Load Connection String
		private string LoadConnectionString()
		{
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EditableConnectionString"]))
				return ConfigurationManager.AppSettings["EditableConnectionString"];
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ControlsConnectionString"]))
				return ConfigurationManager.AppSettings["ControlsConnectionString"];
			
      throw new Exception("Connection string for EditableContent is not specificed. Use either EditableConnectionString or ControlsConnectionString in the AppSettings.");
		}
		#endregion

		#region Can edit this content
		private bool LoadCanEdit()
		{
			try
			{
				HttpContext curr = HttpContext.Current;
				XmlDocument sitemap = new XmlDocument();
				sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
				XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
				xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
				XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);

				if (n.Attributes["editroles"] != null && n.Attributes["editroles"].Value != "")
				{
					foreach (string role in n.Attributes["editroles"].Value.Split(','))
					{
						if (role == "") continue;

						if (curr.User.IsInRole(role)) return true;
					}
				}
			}
			catch { }

			return false;
		}
		#endregion

		#region Can approve this content
		private bool LoadCanApprove()
		{
			try
			{
				HttpContext curr = HttpContext.Current;
				XmlDocument sitemap = new XmlDocument();
				sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
				XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
				xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
				XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);

				if (n.Attributes["approveroles"] != null && n.Attributes["approveroles"].Value != "")
				{
					foreach (string role in n.Attributes["approveroles"].Value.Split(','))
					{
						if (role == "") continue;

						if (curr.User.IsInRole(role)) return true;
					}
				}
				else
					return CanEdit;
			}
			catch { }

			return false;
		}
		#endregion
	}
}