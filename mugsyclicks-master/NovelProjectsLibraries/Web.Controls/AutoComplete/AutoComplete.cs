using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;
using System.Configuration;

namespace NovelProjects.Web
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:AutoComplete runat=server></{0}:AutoComplete>")]
	public class AutoComplete : TextBox
	{
		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		Literal lit;
		#endregion

		#region public variables
		public string DisplayField
		{
			get { return (ViewState["ACDisplayField"] ?? ViewState["ACSearchField"]).ToString(); }
			set { ViewState["ACDisplayField"] = value; }
		}
		public string SearchField
		{
			get { return (ViewState["ACSearchField"] ?? ViewState["ACDisplayField"]).ToString(); }
			set { ViewState["ACSearchField"] = value; }
		}
		public int Delay
		{
			get
			{
				return Convert.ToInt32(ViewState["ACDelay"]) == 0 ? 200 : Convert.ToInt32(ViewState["ACDelay"]);
			}
			set { ViewState["ACDelay"] = value; }
		}
		public int MinChars
		{
			get
			{
				return Convert.ToInt32(ViewState["ACMinChars"]) == 0 ? 1 : Convert.ToInt32(ViewState["ACMinChars"]);
			}
			set { ViewState["ACMinChars"] = value; }
		}
		public string SearchTable { get; set; }
		public bool AutoFill { get; set; }
		public string Filters { get; set; }
		public Guid ApplicationId { get; set; }
		public string ConnString { get; set; }

    public string NoResultsMessage { get; set; }

		#endregion

		#region Initializes all of the controls
		protected override void OnInit(EventArgs args)
		{
			HostingEnvironment.RegisterVirtualPathProvider(new AssemblyResourceProvider());

			base.OnInit(args);
		}

		protected override void OnLoad(EventArgs e)
		{
			if (string.IsNullOrEmpty(DisplayField) && string.IsNullOrEmpty(SearchField))
			{
				throw new Exception("DisplayField and/or SearchField cannot be null.");
			}
			if (string.IsNullOrEmpty(SearchTable))
			{
				throw new Exception("SearchTable cannot be null.");
			}
			if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["AutoCompleteConnectionString"]) && string.IsNullOrEmpty(ConnString))
			{
				throw new Exception("AutoCompleteConnectionString cannot be null.");
			}

			//Adds Javascript code/files and CSS files to page
			lit = new Literal();
			lit.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(AutoComplete), "NovelProjects.Web.AutoComplete.AutoComplete.css") + "' />\n";

			lit.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(AutoComplete), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\\n\")); }</script>\n";
			lit.Text += "<script type=\"text/javascript\">\n";
			lit.Text += "try { $('test').autocomplete('test.aspx'); } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(AutoComplete), "NovelProjects.Web.javascript.jquery.autocomplete.min.js") + "' type='text/javascript'%3E%3C/script%3E\\n\")); }\n";
			lit.Text += "try { AutoComplete(); } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(AutoComplete), "NovelProjects.Web.AutoComplete.AutoComplete.min.js") + "' type='text/javascript'%3E%3C/script%3E\\n\"));\n";
			lit.Text += "document.write(unescape(\"%3Cscript type='text/javascript'%3E try { var prm = Sys.WebForms.PageRequestManager.getInstance(); " +
					"prm.add_endRequest(function() { AutoComplete(); });" +
					"} catch (err) {} %3C/script%3E\")); }\n";
			lit.Text += "</script>\n";

			Controls.Add(lit);
			AddAttributes();

			base.OnLoad(e);
		}
		#endregion

		#region Renders all of the controls in the page
		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();

			lit.RenderControl(writer);

			base.Render(writer);
		}
		#endregion

		#region Set the attributes on the autocomplete textbox
		private void AddAttributes()
		{
			CssClass += " TxtAutoComplete";
			Attributes.Add("applicationid", ApplicationId.ToString());
			Attributes.Add("connstring", ConnString);

			Attributes.Add("table", SearchTable);
			Attributes.Add("search", SearchField);
			Attributes.Add("display", DisplayField);
			Attributes.Add("delay", Delay.ToString());
			Attributes.Add("minchars", MinChars.ToString());
			Attributes.Add("autofill", AutoFill.ToString());
      Attributes.Add("filters", Filters);
      Attributes.Add("noresultsmessage", NoResultsMessage);
		}
		#endregion
	}
}