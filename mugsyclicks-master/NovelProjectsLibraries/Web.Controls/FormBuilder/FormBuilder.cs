using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;

namespace NovelProjects.Web
{
	[DefaultProperty("Text")]
	[ToolboxData("<{0}:FormBuilder runat=server></{0}:FormBuilder>")]
	public class FormBuilder : WebControl
	{

		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		private PlaceHolder ph;
		#endregion

		#region public variables
		//If jquery is not being imported to the page
		public bool JQuery { get; set; }
		#endregion

		#region Writes out the editable div or standard text
		protected override void RenderContents(HtmlTextWriter output)
		{
			
		}
		#endregion

		#region Initializes all of the controls
		protected override void OnInit(EventArgs args)
		{
			HostingEnvironment.RegisterVirtualPathProvider(new AssemblyResourceProvider());

			//Adds Javascript code/files and CSS files to page header
			LiteralControl lt = new LiteralControl();
			lt.ID = "FormBuilderJS";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.FormBuilder.FormBuilder.css") + "' />";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.Tabs.css") + "' />";
			
			if (JQuery)
				lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' ></script>";

			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.javascript.jquery-ui.1.7.min.js") + "' ></script>\n";
			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.FormBuilder.FormBuilder.min.js") + "' ></script>";
			
			Page.Header.Controls.Add(lt);

			BuildFormBuilder();
			Controls.Add(ph);

			base.OnInit(args);
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

		#region Build the form builder

		#region Add the controls to the page
		private void BuildFormBuilder()
		{
			ph = new PlaceHolder();
			Literal lt = new Literal();

			//Used to reload jquery when there is an updatepanel
			lt.Text += "<script>" +
					"try {" +
					"var prm = Sys.WebForms.PageRequestManager.getInstance();" +
					"prm.add_endRequest(function() { " +
					"LoadJquery();" +
					"});" +
					"} catch (err) {}" +
					"</script>";
			ph.Controls.Add(lt);

			//Form stuff goes here
			lt = new Literal();
			lt.Text += "<ul class='ConfigTabs'>";
			lt.Text += "<li><a href='#AddForm'><span>Add</span></a></li>";
			lt.Text += "<li><a href='#EditForm'><span>Edit</span></a></li>";
			lt.Text += "<li><a href='#OptionsForm'><span>Options</span></a></li>";
			lt.Text += "</ul>";
			lt.Text += "<div id='AddForm'><strong>Add a New Question:</strong><br/><br/>";
			ph.Controls.Add(lt);

			BuildQuestionForm();

			//Edit form control
			lt = new Literal();
			lt.Text += "</div><div id='EditForm'><strong>Edit</strong><br /><br /><div id='EditQuestionsList'>";
			ph.Controls.Add(lt);

			//Form Options
			lt = new Literal();
			lt.Text += "</div></div><div id='OptionsForm'><strong>Options</strong><br /><br />";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<strong>Form Name:</strong><br/>";
			ph.Controls.Add(lt);

			TextBox tb = new TextBox();
			tb.CssClass = "FormName";
			tb.Text = "New Form";
			ph.Controls.Add(tb);

			tb = new TextBox();
			tb.CssClass = "FormID";
			tb.Text = Guid.Empty.ToString();
			tb.Style["display"] = "none";
			ph.Controls.Add(tb);

			lt = new Literal();
			lt.Text += "<br/><br/>";
			ph.Controls.Add(lt);

			CheckBox cb = new CheckBox();
			cb.CssClass = "FormSendEmail";
			cb.Text = "Notify me by email when people submit this form:";
			cb.TextAlign = TextAlign.Left;
			ph.Controls.Add(cb);

			lt = new Literal();
			lt.Text += "<span class='FormSendEmailRow'><br/><br/>Recipient Email: ";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "FormEmail";
			ph.Controls.Add(tb);

			lt = new Literal();
			lt.Text += "</span></div>";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<a href='javascript:void(0);' class='SaveForm' title='Save Form'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.images.saveform.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<a href='javascript:void(0);' class='SaveForm' title='Save For Later'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.images.saveformlater.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<a href='javascript:void(0);' class='ClearForm' title='Clear Form'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.images.clearform.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(lt);
		}
		#endregion

		#region Build Form To Add/Edit Questions
		void BuildQuestionForm()
		{
			Literal lt = new Literal();
			lt.Text += "<table cellspacing=0 cellpadding=2><tr><td align=right>Question:</td><td>";
			ph.Controls.Add(lt);

			TextBox tb = new TextBox();
			tb.CssClass = "QLabel";
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);

			tb = new TextBox();
			tb.CssClass = "QID";
			tb.Text = Guid.Empty.ToString();
			tb.Style["display"] = "none";
			ph.Controls.Add(tb);

			lt = new Literal();
			lt.Text += "</td></tr><tr><td align=right valign=top>Answer Type:</td><td>";
			ph.Controls.Add(lt);

			RadioButtonList rbl = new RadioButtonList();
			rbl.CssClass = "QType";
			rbl.ValidationGroup = "AddQuestion";
			rbl.RepeatColumns = 2;
			rbl.Items.Add("Text Field");
			rbl.Items.Add("Check Box");
			rbl.Items.Add("Drop Down List");
			rbl.Items.Add("Radio Button List");
			rbl.Items.Add("Check Box List");
			rbl.Items.Add("List Box");
			rbl.Items[0].Selected = true;
			ph.Controls.Add(rbl);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QValuesRow'><td valign=top align=right>Values:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "QValues";
			tb.TextMode = TextBoxMode.MultiLine;
			tb.Rows = 4;
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QRequiredRow'><td align=right>Required Field:</td><td>";
			ph.Controls.Add(lt);

			rbl = new RadioButtonList();
			rbl.CssClass = "QRequired";
			rbl.ValidationGroup = "AddQuestion";
			rbl.RepeatColumns = 2;
			rbl.Items.Add(new ListItem("No", "False"));
			rbl.Items.Add(new ListItem("Yes", "True"));
			rbl.Items[0].Selected = true;
			ph.Controls.Add(rbl);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QRequiredType'><td align=right>Validation Type:</td><td>";
			ph.Controls.Add(lt);

			DropDownList ddl = new DropDownList();
			ddl.CssClass = "QValidationType";
			ddl.Items.Add("--");
			ddl.Items.Add("Email");
			ddl.Items.Add("Integer");
			ddl.Items.Add("Decimal");
			ddl.Items.Add("Date");
			ddl.ValidationGroup = "AddQuestion";
			ph.Controls.Add(ddl);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QText'><td align=right>Rows:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "QRows";
			tb.Text = "1";
			tb.Width = 25;
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QText'><td align=right>Max Length:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "QMaxLength";
			tb.Text = "100";
			tb.Width = 25;
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QText'><td align=right>Width:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "QWidth";
			tb.Text = "140";
			tb.Width = 25;
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QRadio'><td align=right>Repeat Direction:</td><td>";
			ph.Controls.Add(lt);

			rbl = new RadioButtonList();
			rbl.CssClass = "QRepeatDirection";
			rbl.ValidationGroup = "AddQuestion";
			rbl.RepeatColumns = 2;
			rbl.Items.Add("Vertical");
			rbl.Items.Add("Horizontal");
			rbl.Items[0].Selected = true;
			ph.Controls.Add(rbl);
			lt = new Literal();
			lt.Text += "</td></tr><tr class='QRadio'><td align=right>Repeat Columns:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox();
			tb.CssClass = "QRepeatColumns";
			tb.Text = "1";
			tb.Width = 15;
			tb.ValidationGroup = "AddQuestion";
			ph.Controls.Add(tb);
			lt = new Literal();
			lt.Text += "</td></tr><tr><td></td><td>";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<a href='javascript:void(0);' class='AddQuestion' title='Save Question'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.images.savequestion.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "<a href='javascript:void(0);' class='CancelQuestion' title='Cancel Question'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(FormBuilder), "NovelProjects.Web.images.cancel.gif") + "' border=0 align=absmiddle /></a>";
			ph.Controls.Add(lt);

			lt = new Literal();
			lt.Text += "</td></tr></table>";
			ph.Controls.Add(lt);
		}
		#endregion

		#endregion
	}
}
