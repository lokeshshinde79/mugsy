using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Hosting;

namespace NovelProjects.Web
{
	[ToolboxData("<{0}:DynamicForm runat=server></{0}:DynamicForm>")]
	public class DynamicForm : WebControl
	{

		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		private string FormName;
		private bool FormSendEmail;
		private string FormEmail;
		private PlaceHolder ph;
		private LiteralControl lt;
		private DataTable Questions;
		#endregion

		#region public variables
		//If jquery is not being imported to the page
		public bool JQuery { get; set; }
		public string FormID;
		#endregion

		#region Loads the questions for the form
		private void LoadQuestions()
		{
			Questions = new DataTable();
			DataTable FormInfo = new DataTable();
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ConfigConn"]].ConnectionString))
			{
				conn.Open();
				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM FormBuilderQuestions WHERE FormID=@FormID ORDER BY OrderNum;", conn);
				sqlcmd.Parameters.AddWithValue("@FormID", FormID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(Questions);

				sqlcmd.CommandText = "SELECT * FROM FormBuilderForms WHERE ID=@FormID;";
				adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(FormInfo);

				DataRow dr = FormInfo.Rows[0];
				FormName = HttpUtility.HtmlDecode(dr["Name"].ToString());
				FormSendEmail = Convert.ToBoolean(dr["SendEmail"]);
				FormEmail = HttpUtility.HtmlDecode(dr["Email"].ToString());
			}
		}
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

			if (string.IsNullOrEmpty(FormID))
				return;

			LoadQuestions();
			//Adds Javascript code/files and CSS files to page header
			lt = new LiteralControl();
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.FormBuilder.FormBuilder.css") + "' />";

			if (JQuery)
				lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' ></script>";

			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.FormBuilder.DynamicForm.min.js") + "' ></script>";
			

			Page.Header.Controls.Add(lt);

			BuildForm();
			Controls.Add(ph);

			base.OnInit(args);
		}
		#endregion

		#region Renders all of the controls in the page
		protected override void Render(HtmlTextWriter writer)
		{
			if (string.IsNullOrEmpty(FormID))
				return;

			EnsureChildControls();

			//Render the main controls on the page
			ph.RenderControl(writer);

			base.Render(writer);
		}
		#endregion

		#region Build Form
		private void BuildForm()
		{
			string ValidationGroup = "DynamicForm";
			ph = new PlaceHolder();

			ValidationSummary vs = new ValidationSummary();
			vs.DisplayMode = ValidationSummaryDisplayMode.List;
			vs.ValidationGroup = ValidationGroup;
			ph.Controls.Add(vs);

			Literal text = new Literal();
			text.Text += "<span style='display:none' id='FormID'>" + FormID + "</span><div id='ThankYou' style='display:none;'>Thank you for submitting the form.</div>";
			ph.Controls.Add(text);

			text = new Literal();
			text.Text += "<table class=DynamicForm cellpadding=2 cellspacing=2><tr><td colspan=2><strong>" + FormName + "</strong></td></tr>";
			ph.Controls.Add(text);

			foreach (DataRow Question in Questions.Rows)
			{
				text = new Literal();
				text.Text += "<tr><td valign=top>" + Question["Label"] + "</td><td class='FormBuilderQuestion'>";
				ph.Controls.Add(text);

				string QID = Question["ID"].ToString();
				bool Required = Convert.ToBoolean(Question["Required"]);
				string ValidationType = Question["ValidationType"].ToString();
				string[] Values = Question["Values"].ToString().Split('|');
				RepeatDirection RepeatDirection = (RepeatDirection)Enum.Parse(typeof(RepeatDirection), Question["RepeatDirection"].ToString());
				int RepeatColumns = Convert.ToInt32(Question["RepeatColumns"]);

				switch (Question["Type"].ToString())
				{
					case "Text Field":
						{
							int Rows = Convert.ToInt32(Question["Rows"]);
							int MaxLength = Convert.ToInt32(Question["MaxLength"]);
							int Width = Convert.ToInt32(Question["Width"]);
							TextBox ques = new TextBox();
							if (Rows > 1) ques.TextMode = TextBoxMode.MultiLine;
							ques.ID = QID;
							ques.MaxLength = MaxLength;
							ques.Width = Width;
							ph.Controls.Add(ques);
						} break;
					case "Radio Button List":
						{
							RadioButtonList ques = new RadioButtonList();
							ques.ID = QID;
							ques.RepeatDirection = RepeatDirection;
							ques.RepeatColumns = RepeatColumns;
							foreach (string val in Values)
								ques.Items.Add(val);
							ph.Controls.Add(ques);
						} break;
					case "Check Box":
						{
							CheckBox ques = new CheckBox();
							ques.ID = QID;
							ph.Controls.Add(ques);
						} break;
					case "Check Box List":
						{
							CheckBoxList ques = new CheckBoxList();
							ques.ID = QID;
							ques.RepeatDirection = RepeatDirection;
							ques.RepeatColumns = RepeatColumns;
							foreach (string val in Values)
								ques.Items.Add(val);
							ph.Controls.Add(ques);
						} break;
					case "Drop Down List":
						{
							DropDownList ques = new DropDownList();
							ques.ID = QID;
							foreach (string val in Values)
								ques.Items.Add(val);
							ph.Controls.Add(ques);
						} break;
					case "List Box":
						{
							ListBox ques = new ListBox();
							ques.ID = QID;
							foreach (string val in Values)
								ques.Items.Add(val);
							ph.Controls.Add(ques);
						} break;
				}
				if (Required)
				{
					RequiredFieldValidator rfv = new RequiredFieldValidator();
					rfv.ValidationGroup = ValidationGroup;
					rfv.ControlToValidate = QID;
					rfv.Display = ValidatorDisplay.None;
					rfv.ErrorMessage = Question["Label"] + " required";
					ph.Controls.Add(rfv);

					if (ValidationType == "Email")
					{
						RegularExpressionValidator rev = new RegularExpressionValidator();
						rev.ValidationGroup = ValidationGroup;
						rev.ControlToValidate = QID;
						rev.ValidationExpression = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
						rev.Display = ValidatorDisplay.None;
						rev.ErrorMessage = "Invalid Email";
						ph.Controls.Add(rev);
					}
					else if (ValidationType == "Integer")
					{
						CompareValidator cv = new CompareValidator();
						cv.ValidationGroup = ValidationGroup;
						cv.ControlToValidate = QID;
						cv.Operator = ValidationCompareOperator.DataTypeCheck;
						cv.Type = ValidationDataType.Integer;
						cv.Display = ValidatorDisplay.None;
						cv.ErrorMessage = Question["Label"] + " must be a number";
						ph.Controls.Add(cv);
					}
					else if (ValidationType == "Decimal")
					{
						CompareValidator cv = new CompareValidator();
						cv.ValidationGroup = ValidationGroup;
						cv.ControlToValidate = QID;
						cv.Operator = ValidationCompareOperator.DataTypeCheck;
						cv.Type = ValidationDataType.Double;
						cv.Display = ValidatorDisplay.None;
						cv.ErrorMessage = Question["Label"] + " must be a number";
						ph.Controls.Add(cv);
					}
					else if (ValidationType == "Date")
					{
						CompareValidator cv = new CompareValidator();
						cv.ValidationGroup = ValidationGroup;
						cv.ControlToValidate = QID;
						cv.Operator = ValidationCompareOperator.DataTypeCheck;
						cv.Type = ValidationDataType.Date;
						cv.Display = ValidatorDisplay.None;
						cv.ErrorMessage = Question["Label"] + " invalid date";
						ph.Controls.Add(cv);
					}
				}

				text = new Literal();
				text.Text += "</td></tr>";
				ph.Controls.Add(text);
			}
			text = new Literal();
			text.Text += "<tr><td></td><td>";
			ph.Controls.Add(text);

			text = new Literal();
			text.Text += "<a href='javascript:void(0);' class='SaveSubmission' title='Submit Form'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.images.saveform.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(text);

			text = new Literal();
			text.Text += "<a href='javascript:void(0);' class='ResetForm' title='Reset Form'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.images.clearform.gif") + "' border=0 align=absmiddle /></a> ";
			ph.Controls.Add(text);

			//Button btn = new Button();
			//btn.Text = "Submit";
			//btn.ValidationGroup = ValidationGroup;
			//btn.Click += new EventHandler(SaveSubmission);
			//ph.Controls.Add(btn);

			//btn = new Button();
			//btn.Text = "Reset Form";
			//btn.Click += new EventHandler(ClearForm);
			//ph.Controls.Add(btn);

			text = new Literal();
			text.Text += "</td></tr></table>";
			ph.Controls.Add(text);
		}
		#endregion
	}
}
