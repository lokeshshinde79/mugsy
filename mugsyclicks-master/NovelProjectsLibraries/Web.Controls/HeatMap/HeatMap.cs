using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Hosting;
using System.Web.Security;

namespace NovelProjects.Web
{
	[ToolboxData("<{0}:HeatMap runat=server></{0}:HeatMap>")]
	public class HeatMap : WebControl
	{

		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		private PlaceHolder ph = new PlaceHolder();
		private LiteralControl lt;
		#endregion

		#region public variables
		public string RoleName { get; set; }
		public bool IsCentered { get; set; }
		public string Path { get; set; }
		#endregion

		#region Writes out the editable div or standard text
		protected override void RenderContents(HtmlTextWriter output)
		{
			output.Write("<span style='display:none' class='IsCenteredVal'>" + IsCentered + "</span>");
			output.Write("<span style='display:none' class='PathVal'>" + Path + "</span>");
		}
		#endregion

		#region Initializes all of the controls
		protected override void OnInit(EventArgs args)
		{
			HostingEnvironment.RegisterVirtualPathProvider(new AssemblyResourceProvider());

			base.OnInit(args);
		}

		protected override void OnLoad(EventArgs e)
		{
			//Adds Javascript code/files and CSS files to page
			lt = new LiteralControl();

			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.HeatMap.HeatMap.css") + "' />\n";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.jquery-ui-1.7-core.css") + "' />\n";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.jquery-ui-1.7-icons.css") + "' />\n";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.jquery-ui-1.7-datepicker.css") + "' />\n";
			
			lt.Text += "<script type='text/javascript'>\n";
			lt.Text += "var rootpath='" + HttpContext.Current.Application["ROOTPATH"] + "';\n";
			lt.Text += "var ShowHeatmap = '" + Roles.IsUserInRole(RoleName) + "'=='True';\n";
			lt.Text += "</script>\n";
			lt.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";

			//Code to show heatmap or log clicks based on logged into website
			if (Roles.IsUserInRole(RoleName))
			{
				lt.Text += "<script type=\"text/javascript\">try { $('.DatepickerTest').datepicker(); } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.javascript.jquery-ui-1.7.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";
				lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.javascript.jquery.pngFix.min.js") + "' ></script>\n";
				lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.HeatMap.HeatMap.min.js") + "' ></script>\n";
			}
			else
				lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.HeatMap.SaveClick.min.js") + "' ></script>\n";


			Page.Header.Controls.Add(lt);

			if (Roles.IsUserInRole(RoleName))
				BuildHeatmap();
			Controls.Add(ph);

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

		#region Build Heatmap control
		private void BuildHeatmap()
		{
			lt = new LiteralControl();
			lt.Text += "<div id='LoadingPopup' class='npContainer'>";
			lt.Text += "<div class='npContent' align='center'><br/>";
			lt.Text += "<img src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.images.loading.gif") + "' /><br/>";
			lt.Text += "</div></div>";

			lt.Text += "<div id='HeatmapBox'><div id='Content'>";
			lt.Text += "<div class='ClickText'><strong>Number of Clicks:</strong> <span class='ClickCount'></span><br/>";
			lt.Text += "<strong>Unique Visitors:</strong> <span class='UniqueCount'></span></div>";
			lt.Text += "<h1>Heatmap Stats</h1>";
			lt.Text += "<img class='hmpng' width='180' src='" + Page.ClientScript.GetWebResourceUrl(typeof(HeatMap), "NovelProjects.Web.images.legend.png") + "' align='right'/>";
			lt.Text += "<input type='text' style='position:relative; z-index:501; width:77px;' class='start'/> - ";
			lt.Text += "<input type='text' style='position:relative; z-index:501; width:77px;' class='end'/> ";
			ph.Controls.Add(lt);

			DropDownList ddl = new DropDownList
			{
				CssClass = "Browsers",
				Width = 180,
				AppendDataBoundItems = true,
				DataTextField = "Browser"
			};
			ddl.Items.Add("Show All");
			ddl.DataSource = GetBrowsers();
			ddl.DataBind();
			ph.Controls.Add(ddl);

			CheckBox cb = new CheckBox
			{
				CssClass = "NoQuery",
				Text = "All Queries"
			};
			ph.Controls.Add(cb);

			Button btn = new Button
			{
				CssClass = "UpdateHeatmap",
				Text = "Show",
				CausesValidation = false
			};
			ph.Controls.Add(btn);

			btn = new Button
			{
				CssClass = "RemoveHeatmap",
				Text = "Hide"
			};
			ph.Controls.Add(btn);

			lt = new LiteralControl();
			lt.Text += "</div></div>";
			ph.Controls.Add(lt);
		}
		#endregion

		#region Get Browser List
		private static DataTable GetBrowsers()
		{
			if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["HeatmapConn"]) && string.IsNullOrEmpty(ConfigurationManager.AppSettings["ControlsConn"]))
				throw new Exception("Must specify the AppSetting \"HeatmapConn\" for the connection string to be used.");

			DataTable Browsers = new DataTable();
			string conns = ConfigurationManager.AppSettings["HeatmapConn"] ?? ConfigurationManager.AppSettings["ControlsConn"];

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[conns].ConnectionString))
			{
				conn.Open();
				SqlCommand sqlcmd = new SqlCommand("SELECT DISTINCT Browser FROM ClickLog WHERE ApplicationID=@ApplicationID ORDER BY Browser ASC;", conn);
				sqlcmd.Parameters.AddWithValue("@ApplicationID", ConfigurationManager.AppSettings["AppId"]);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(Browsers);

				sqlcmd = new SqlCommand("SELECT DISTINCT OS AS Browser FROM ClickLog WHERE ApplicationID=@ApplicationID AND OS IS NOT NULL ORDER BY OS ASC;", conn);
				sqlcmd.Parameters.AddWithValue("@ApplicationID", ConfigurationManager.AppSettings["AppId"]);
				adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(Browsers);
			}

			return Browsers;
		}
		#endregion
	}
}