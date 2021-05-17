using System;
using System.Collections;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.IO;
using System.Web.Script.Services;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace NovelProjects.Web
{
	[WebService(Namespace = "http://www.novelprojects.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ScriptService]
	public class FormBuilderService : WebService
	{
		#region Question code
		#region Save Question
		[WebMethod]
		public String SaveQuestion(Question Ques)
		{
			DataTable FormQuestions = new DataTable();
			string retval = "";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				if (Ques.FormID == Guid.Empty)
				{
					Ques.FormID = Guid.NewGuid();
					SqlCommand insert = new SqlCommand("INSERT INTO FormBuilderForms (ID) VALUES (@FormID);", conn);
					insert.Parameters.AddWithValue("@FormID", Ques.FormID);
					insert.ExecuteNonQuery();
				}

				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM FormBuilderQuestions WHERE FormID=@FormID ORDER BY OrderNum", conn);
				sqlcmd.Parameters.AddWithValue("@FormID", Ques.FormID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(FormQuestions);

				DataRow Question = null;
				if (Ques.ID == Guid.Empty)
				{
					Ques.ID = Guid.NewGuid();
					Question = FormQuestions.NewRow();
					FormQuestions.Rows.Add(Question);
				}
				else
				{
					Question = FormQuestions.Select("ID='" + Ques.ID + "'")[0];
				}

				Question["ID"] = Ques.ID;
				Question["FormID"] = Ques.FormID;
				Question["Label"] = HttpUtility.HtmlEncode(Ques.Label);
				Question["Type"] = Ques.Type;
				Question["Values"] = HttpUtility.HtmlEncode(Ques.Values);
				Question["Required"] = Ques.Required;
				Question["ValidationType"] = Ques.ValidationType;
				Question["Rows"] = Ques.Rows;
				Question["MaxLength"] = Ques.MaxLength;
				Question["Width"] = Ques.Width;
				Question["RepeatDirection"] = Ques.RepeatDirection;
				Question["RepeatColumns"] = Ques.RepeatColumns;
				Question["OrderNum"] = FormQuestions.Rows.Count;

				SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
				adapter.Update(FormQuestions);
			}

			retval = "{ \"FormID\":\"" + Ques.FormID + "\", \"Questions\":[";
			foreach (DataRow Question in FormQuestions.Rows)
			{
				retval += SerializeObject(new Question(Question)) + ",";
			}
			retval = retval.Remove(retval.LastIndexOf(","));
			retval += "]}";

			return retval;
		}
		#endregion

		#region Load Question
		[WebMethod]
		public String LoadQuestion(Guid ID)
		{
			DataTable FormQuestions = new DataTable();
			Question Ques;

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();

				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM FormBuilderQuestions WHERE ID=@ID", conn);
				sqlcmd.Parameters.AddWithValue("@ID", ID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(FormQuestions);

				DataRow dr = FormQuestions.Rows[0];

				Ques = new Question(dr);
			}

			return SerializeObject(Ques);
		}
		#endregion

		#region Swap Questions
		[WebMethod]
		public String SwapQuestions(Guid ID, string MoveCommand)
		{
			DataTable FormQuestions = new DataTable();
			string retval = "";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				SqlCommand sqlcmd = new SqlCommand("SELECT FormID,OrderNum FROM FormBuilderQuestions WHERE ID=@ID;", conn);
				sqlcmd.Parameters.AddWithValue("@ID", ID);
				SqlDataReader reader = sqlcmd.ExecuteReader();
				reader.Read();
				Guid FormID = new Guid(reader.GetValue(0).ToString());
				int OrderNum = Convert.ToInt32(reader.GetValue(1).ToString());
				reader.Close();


				sqlcmd = new SqlCommand("SELECT COUNT(ID) FROM FormBuilderQuestions WHERE FormID=@FormID;", conn);
				sqlcmd.Parameters.AddWithValue("@FormID", FormID);
				int Rows = Convert.ToInt32(sqlcmd.ExecuteScalar());


				if (MoveCommand.Equals("MoveUp") && OrderNum != 1)
				{
					SqlCommand update = new SqlCommand("UPDATE FormBuilderQuestions SET OrderNum = OrderNum+1 WHERE OrderNum = @OrderNum;UPDATE FormBuilderQuestions SET OrderNum = OrderNum-1 WHERE ID = @ID", conn);
					update.Parameters.AddWithValue("@OrderNum", OrderNum - 1);
					update.Parameters.AddWithValue("@ID", ID);
					update.ExecuteNonQuery();
				}
				else if (MoveCommand.Equals("MoveDown") && OrderNum != Rows)
				{
					SqlCommand update = new SqlCommand("UPDATE FormBuilderQuestions SET OrderNum = OrderNum-1 WHERE OrderNum = @OrderNum;UPDATE FormBuilderQuestions SET OrderNum = OrderNum+1 WHERE ID = @ID", conn);
					update.Parameters.AddWithValue("@OrderNum", OrderNum + 1);
					update.Parameters.AddWithValue("@ID", ID);
					update.ExecuteNonQuery();
				}
				else if (MoveCommand.Equals("Delete"))
				{
					SqlCommand update = new SqlCommand("UPDATE FormBuilderQuestions SET OrderNum = OrderNum-1 WHERE OrderNum > @OrderNum;DELETE FROM FormBuilderQuestions WHERE ID = @ID", conn);
					update.Parameters.AddWithValue("@OrderNum", OrderNum);
					update.Parameters.AddWithValue("@ID", ID);
					update.ExecuteNonQuery();
				}


				sqlcmd = new SqlCommand("SELECT * FROM FormBuilderQuestions WHERE FormID=@FormID ORDER BY OrderNum;", conn);
				sqlcmd.Parameters.AddWithValue("@FormID", FormID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(FormQuestions);
			}

			retval = "{ \"Questions\":[";
			foreach (DataRow Question in FormQuestions.Rows)
			{
				retval += SerializeObject(new Question(Question)) + ",";
			}
			if (retval.LastIndexOf(",") > 0)
				retval = retval.Remove(retval.LastIndexOf(","));
			retval += "]}";

			return retval;
		}
		#endregion
		#endregion

		#region Form code
		#region Load Form
		[WebMethod]
		public String LoadForm(Guid FormID)
		{
			DataTable FormQuestions = new DataTable();
			string retval = "";
			string FormName, FormSendEmail, FormEmail;

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM FormBuilderQuestions WHERE FormID=@FormID ORDER BY OrderNum;", conn);
				sqlcmd.Parameters.AddWithValue("@FormID", FormID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(FormQuestions);

				sqlcmd.CommandText = "SELECT * FROM FormBuilderForms WHERE ID=@FormID;";
				SqlDataReader reader = sqlcmd.ExecuteReader();
				reader.Read();
				FormName = reader.GetValue(1).ToString();
				FormSendEmail = reader.GetValue(2).ToString();
				FormEmail = reader.GetValue(3).ToString();
				reader.Close();
			}

			retval = "{ \"FormName\":\"" + FormName + "\", \"FormSendEmail\":\"" + FormSendEmail + "\", \"FormEmail\":\"" + FormEmail + "\", \"Questions\":[";
			foreach (DataRow Question in FormQuestions.Rows)
			{
				retval += SerializeObject(new Question(Question)) + ",";
			}
			if (retval.LastIndexOf(",") > 0)
				retval = retval.Remove(retval.LastIndexOf(","));
			retval += "]}";

			return retval;
		}
		#endregion

		#region Save Form
		[WebMethod]
		public String SaveForm(Form Form)
		{
			DataTable Forms = new DataTable();
			string retval = "";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				if (Form.ID == Guid.Empty)
					Form.ID = Guid.NewGuid();

				SqlCommand sqlcmd = new SqlCommand("SELECT * FROM FormBuilderForms WHERE ID=@ID", conn);
				sqlcmd.Parameters.AddWithValue("@ID", Form.ID);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(Forms);

				DataRow dr = null;
				if (Forms.Rows.Count == 0)
				{
					dr = Forms.NewRow();
					Forms.Rows.Add(dr);
				}
				else
				{
					dr = Forms.Rows[0];
				}

				dr["ID"] = Form.ID;
				dr["Name"] = HttpUtility.HtmlEncode(Form.Name);
				dr["SendEmail"] = Form.SendEmail;
				dr["Email"] = HttpUtility.HtmlEncode(Form.Email);
				dr["Visible"] = Form.Visible;
				dr["Saved"] = Form.Saved;

				SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
				adapter.Update(Forms);
			}

			return retval;
		}
		#endregion

		#region Clear Form
		[WebMethod]
		public String ClearForm(Guid ID)
		{
			string retval = "";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				SqlCommand sqlcmd = new SqlCommand("DELETE FROM FormBuilderForms WHERE ID=@ID", conn);
				sqlcmd.Parameters.AddWithValue("@ID", ID);
				sqlcmd.ExecuteNonQuery();
			}

			return retval;
		}
		#endregion
		#endregion

		#region Save the user submission
		[WebMethod]
		public String SaveSubmission(SubmittedForm Form)
		{
			DataSet ds = new DataSet();
			DataTable Submissions = new DataTable();
			DataTable Forms = new DataTable();
			Guid SubmissionID = Guid.NewGuid();
			string retval = "";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["ControlsConn"]].ConnectionString))
			{
				conn.Open();
				if (Form.FormID == Guid.Empty)
					Form.FormID = Guid.NewGuid();

				SqlCommand sqlcmd = new SqlCommand("INSERT INTO FormBuilderSubmissions (ID,FormID) VALUES (@ID,@FormID);", conn);
				sqlcmd.Parameters.AddWithValue("@ID", SubmissionID);
				sqlcmd.Parameters.AddWithValue("@FormID", Form.FormID);
				sqlcmd.ExecuteNonQuery();

				sqlcmd.CommandText = "SELECT * FROM FormBuilderSubmissionValues WHERE SubmissionID=@ID;SELECT * FROM FormBuilderForms WHERE ID=@FormID;";
				SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
				adapter.Fill(ds);
				Submissions = ds.Tables[0];
				Forms = ds.Tables[1];

				//CODE TO COMBINE CHECK BOX LISTS INTO ONE QUESTION VALUE
				DataRow Submission = null;
				foreach (FormAnswer Answer in Form.Answers)
				{
					DataRow[] Subs = Submissions.Select("QuestionID='" + Answer.QuestionID + "'");
					if (Subs.Length == 0)
					{
						Submission = Submissions.NewRow();
						Submissions.Rows.Add(Submission);
					}
					else
					{
						Submission = Subs[0];
						if (Answer.Value != "")
							Answer.Value = Submission["Value"] + "," + Answer.Value;
					}

					Submission["SubmissionID"] = SubmissionID;
					Submission["QuestionID"] = Answer.QuestionID;
					Submission["Value"] = HttpUtility.HtmlEncode(Answer.Value);
				}

				SqlCommandBuilder cb = new SqlCommandBuilder(adapter);
				adapter.Update(Submissions);
			}

			try
			{
				if (Convert.ToBoolean(Forms.Rows[0]["SendEmail"]))
					SendEmail(Forms.Rows[0]["Email"].ToString(), Forms.Rows[0]["Name"].ToString());
			}
			catch { }

			retval = "Success";
			return retval;
		}

		#region Send Email
		void SendEmail(string Email, string Name)
		{
			HttpContext curr = HttpContext.Current;
			string emailMsg = "";

			emailMsg = Name + " has been submitted<br/>";
			//emailMsg += "First Name: " + FirstName.Text + "<br/>";
			//emailMsg += "Last Name: " + LastName.Text + "<br/>";
			//emailMsg += "Email Address: " + Email.Text + "<br/>";
			//emailMsg += "Member ID or Date of Birth: " + MemberID.Text + "<br/>";
			//emailMsg += "Questions/Comments: " + Comments.Text + "<br/>";
			string body = "";
			//HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create("http://" + curr.Request.Url.Host + ClientScript.GetWebResourceUrl(typeof(DynamicForm), "NovelProjects.Web.email.html"));
			//using (HttpWebResponse wresp = (HttpWebResponse)wreq.GetResponse())
			//{
			//  using (StreamReader sr = new StreamReader(wresp.GetResponseStream()))
			//  {
			//    body = sr.ReadToEnd();
			//    sr.Close();
			//  }
			//  wresp.Close();
			//}

			//body = body.Replace("##EmailMessage##", emailMsg);
			body += emailMsg;

			SmtpClient client = new SmtpClient();
			MailAddress From = new MailAddress(ConfigurationManager.AppSettings["ConfigFromEmail"].ToString());
			MailAddress To = new MailAddress(Email);
			MailMessage message = new MailMessage(From, To);

			message.Subject = "Form Has Been Submitted - " + System.DateTime.Now;
			message.IsBodyHtml = true;
			message.Body = body;

			client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSSL"].ToString());
			client.Send(message);
		}
		#endregion

		#endregion

		#region Serialize Question
		private string SerializeObject(Question Ques)
		{
			string retval = "";

			retval = "{";
			retval += " \"ID\":\"" + Ques.ID + "\",";
			retval += " \"FormID\":\"" + Ques.FormID + "\",";
			retval += " \"Label\":\"" + Ques.Label + "\",";
			retval += " \"Type\":\"" + Ques.Type + "\",";
			retval += " \"Values\":\"" + Ques.Values + "\",";
			retval += " \"Required\":\"" + Ques.Required + "\",";
			retval += " \"ValidationType\":\"" + Ques.ValidationType + "\",";
			retval += " \"Rows\":\"" + Ques.Rows + "\",";
			retval += " \"MaxLength\":\"" + Ques.MaxLength + "\",";
			retval += " \"Width\":\"" + Ques.Width + "\",";
			retval += " \"RepeatDirection\":\"" + Ques.RepeatDirection + "\",";
			retval += " \"RepeatColumns\":\"" + Ques.RepeatColumns + "\",";
			retval += " \"OrderNum\":\"" + Ques.OrderNum + "\"";
			retval += "}";

			return retval;
		}
		#endregion
	}

	#region Class to hold Form data
	public class Form
	{
		public Guid ID;
		public string Name;
		public bool SendEmail;
		public string Email;
		public DateTime Created;
		public bool Visible;
		public bool Saved;

		public Form()
		{
		}
	}
	#endregion

	#region Class to hold Question data
	public class Question
	{
		public Guid ID;
		public Guid FormID;
		public string Label;
		public string Type;
		public string Values;
		public bool Required;
		public string ValidationType;
		public int Rows;
		public int MaxLength;
		public int Width;
		public string RepeatDirection;
		public int RepeatColumns;
		public int OrderNum;

		public Question() { }

		public Question(DataRow dr)
		{
			ID = new Guid(dr["ID"].ToString());
			FormID = new Guid(dr["FormID"].ToString());
			Label = HttpUtility.HtmlDecode(dr["Label"].ToString());
			Type = dr["Type"].ToString();
			Values = HttpUtility.HtmlDecode(dr["Values"].ToString());
			Required = Convert.ToBoolean(dr["Required"]);
			ValidationType = dr["ValidationType"].ToString();
			Rows = Convert.ToInt32(dr["Rows"]);
			MaxLength = Convert.ToInt32(dr["MaxLength"]);
			Width = Convert.ToInt32(dr["Width"]);
			RepeatDirection = dr["RepeatDirection"].ToString();
			RepeatColumns = Convert.ToInt32(dr["RepeatColumns"]);
			OrderNum = Convert.ToInt32(dr["OrderNum"]);
		}
	}
	#endregion

	#region Class to hold Submitted Form
	public class SubmittedForm
	{
		public Guid ID;
		public Guid FormID;
		public DateTime DateSubmitted;
		public FormAnswer[] Answers;

		public SubmittedForm() { }
	}

	public class FormAnswer
	{
		public Guid QuestionID;
		public string Value;
	}
	#endregion
}