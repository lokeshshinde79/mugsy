using System;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.IO;
using System.Web.Script.Services;
using System.Net.Mail;
using System.Xml;

namespace NovelProjects.Web
{
	[WebService(Namespace = "http://www.novelprojects.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[ScriptService]
	public class EditableContentService : WebService
	{
		string Url = HttpContext.Current.Request.UrlReferrer.AbsolutePath.Substring(
			HttpContext.Current.Application["ROOTPATH"].ToString().Length) + 
			(HttpContext.Current.Request.UrlReferrer.AbsolutePath.EndsWith("/") ? "index.aspx" : "");

		#region Load Connection String
		private static string LoadConnString()
		{
			string ConnString = "";
			if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["EditableConnectionString"]))
				ConnString = ConfigurationManager.AppSettings["EditableConnectionString"];
			else if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ControlsConnectionString"]))
				ConnString = ConfigurationManager.AppSettings["ControlsConnectionString"];

			return ConnString;
		}
		#endregion

		#region Save/Approve Content
		[WebMethod]
		public string SaveApprove(Guid ID, string ContentID, string Content, bool Approve, bool IsNew, string PageUrl)
		{
			try
			{
				if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

				if (!CanEdit() && !CanApprove()) return "Invalid Permssions.";
				if (IsNew && CheckMaxVersions(ContentID)) return "Maximum versions exceeded.";

				string status = SaveContent(ID, ContentID, Content, Approve, IsNew);
				if (status != "Success") return status;

				if (!Approve) return SendApproval(Content);
			}
			catch (Exception e)
			{
				SendErrorEmail.Send(new Uri(Url), e);
			}

			return "Success";
		}
		#endregion

		#region Save Content
		private string SaveContent(Guid ID, string ContentID, string Content, bool Approved, bool IsNew)
		{
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				string text = HttpUtility.UrlDecode(Content, Encoding.ASCII);

				try
				{
					SqlCommand sqlcmd = new SqlCommand("UPDATE EditableContent SET ContentText=@ContentText, LastModified=GetDate(), LastModifiedUserId=@UserId, IsApproved=@IsApproved WHERE ID=@ID AND ApplicationId=@ApplicationId;", conn, trans);
					if (IsNew)
						sqlcmd.CommandText = "INSERT INTO EditableContent (ApplicationId,Url,ContentId,ContentText,IsApproved,LastModifiedUserId) VALUES (@ApplicationId,@Url,@ContentId,@ContentText,@IsApproved,@UserId);";

					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ContentText", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@Url", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@ContentId", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@IsApproved", SqlDbType.Bit);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.Parameters["@ContentText"].Value = HttpContext.Current.Server.HtmlEncode(text);
					sqlcmd.Parameters["@UserId"].Value = Membership.GetUser().ProviderUserKey;
					sqlcmd.Parameters["@Url"].Value = Url;
					sqlcmd.Parameters["@ContentId"].Value = ContentID;
					sqlcmd.Parameters["@IsApproved"].Value = (Approved && CanApprove());
					sqlcmd.ExecuteNonQuery();

					// Delete versions if there are more than 10 versions
					sqlcmd.CommandText = "DELETE FROM EditableContent WHERE ApplicationId=@ApplicationId AND ContentId=(SELECT ContentId FROM EditableContent WHERE ApplicationId=@ApplicationId AND ID=@ID) AND Url=@Url AND IsPublished=0 AND ID NOT IN (SELECT TOP 10 ID FROM EditableContent WHERE ApplicationId=@ApplicationId AND Url=@Url AND ContentId=(SELECT ContentId FROM EditableContent WHERE ApplicationId=@ApplicationId AND ID=@ID) AND IsPublished=0 ORDER BY LastModified DESC);";
					sqlcmd.ExecuteNonQuery();

					trans.Commit();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);
					trans.Rollback();
					return "Error saving changes.";
				}
			}

			if (Approved && Approve(ID, false, "", ContentID, null) != "Success")
				return "Error approving content.";

			return "Success";
		}
		#endregion

		#region Save Name
		[WebMethod]
		public string SaveName(Guid ID, string Name)
		{
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				try
				{
					string text = HttpUtility.UrlDecode(Name, Encoding.ASCII);
					SqlCommand sqlcmd = new SqlCommand("UPDATE EditableContent SET Name=@Name WHERE ApplicationId=@ApplicationId AND ID=@ID;", conn);

					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@Name", SqlDbType.VarChar);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.Parameters["@Name"].Value = HttpContext.Current.Server.HtmlEncode(text);
					sqlcmd.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);
					return "Error saving name.";
				}
			}

			return "Success";
		}
		#endregion

		#region Create New Version
		[WebMethod]
		public string CreateNew(string ContentID, string PageUrl)
		{
			if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

			if (!CanEdit() && !CanApprove()) return "Invalid Permissions.";
			if (CheckMaxVersions(ContentID)) return "Maximum versions exceeded.";

			Guid VersionID = Guid.NewGuid();
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				try
				{
					SqlCommand sqlcmd = new SqlCommand("INSERT INTO EditableContent (ID,Url,ContentID,ApplicationId,LastModifiedUserId) VALUES (@ID,@Url,@ContentID,@ApplicationId,@UserId);", conn);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@Url", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@ContentID", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = VersionID;
					sqlcmd.Parameters["@Url"].Value = Url;
					sqlcmd.Parameters["@ContentID"].Value = ContentID;
					sqlcmd.Parameters["@UserId"].Value = (Guid)Membership.GetUser().ProviderUserKey;
					sqlcmd.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);

					return "Error creating new version.";
				}
			}

			return VersionID.ToString();
		}
		#endregion

		#region Copy Content
		[WebMethod]
		public String CopyContent(Guid ID, string ContentID, string PageUrl)
		{
			if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

			if (!CanEdit()) return "Invalid Permissions.";
			if (CheckMaxVersions(ContentID)) return "Maximum versions exceeded.";

			Guid NewID = Guid.NewGuid();
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				try
				{
					SqlCommand sqlcmd = new SqlCommand("INSERT INTO EditableContent (ID,ApplicationId,Url,ContentId,ContentText,Name,LastModified,LastModifiedUserId)" +
						" SELECT @NewID,ApplicationId,Url,ContentId,ContentText,Name,LastModified,LastModifiedUserId FROM EditableContent" +
						" WHERE ApplicationId=@ApplicationId AND ID=@ID;", conn);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@NewID", SqlDbType.UniqueIdentifier);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.Parameters["@NewID"].Value = NewID;
					sqlcmd.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);

					return "Error copying version.";
				}
			}

			return NewID.ToString();
		}
		#endregion

		#region Load Content
		[WebMethod]
		public string LoadContent(Guid ID)
		{
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				try
				{
					SqlCommand sqlcmd = new SqlCommand("SELECT @ContentText=ContentText FROM EditableContent WHERE ApplicationId=@ApplicationId AND ID=@ID;", conn);
					sqlcmd.Parameters.Add("@ContentText", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);

					sqlcmd.Parameters["@ContentText"].Direction = ParameterDirection.Output;
					sqlcmd.Parameters["@ContentText"].Size = Int32.MaxValue;

					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.ExecuteNonQuery();

					return HttpContext.Current.Server.HtmlDecode(sqlcmd.Parameters["@ContentText"].Value.ToString());
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);
					return "Error loading content.";
				}
			}
		}
		#endregion

		#region Approve Content
		[WebMethod]
		public String Approve(Guid ID, bool NewSave, string Content, string ContentID, string PageUrl)
		{
			if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

			if (!CanApprove()) return "Invalid Permissions.";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					SqlCommand sqlcmd = new SqlCommand("UPDATE EditableContent SET IsApproved=1 WHERE ApplicationId=@ApplicationId AND ID=@ID;", conn, trans);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.ExecuteNonQuery();

					// If content has changed then resave
					if (NewSave && !string.IsNullOrEmpty(Content))
					{
						string text = HttpUtility.UrlDecode(Content, Encoding.ASCII);
						sqlcmd.CommandText = "UPDATE EditableContent SET ContentText=@ContentText, LastModified=GetDate(), LastModifiedUserId=@UserId" +
							" WHERE ApplicationId=@ApplicationId AND ID=@ID AND ContentText!=@ContentText;";
						sqlcmd.Parameters.Add("@ContentText", SqlDbType.VarChar);
						sqlcmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier);
						sqlcmd.Parameters["@ContentText"].Value = HttpContext.Current.Server.HtmlEncode(text);
						sqlcmd.Parameters["@UserId"].Value = (Guid)Membership.GetUser().ProviderUserKey;
						sqlcmd.ExecuteNonQuery();
					}
					// If this is a published version, update the published version
					sqlcmd.CommandText = "DELETE FROM EditableContent WHERE ApplicationId=@ApplicationId AND EditableContentId=@ID;";
					sqlcmd.ExecuteNonQuery();

					sqlcmd.CommandText = "IF NOT EXISTS (SELECT * FROM EditableContent WHERE ApplicationId=@ApplicationId" +
						" AND Url=@Url AND ContentId=@ContentId AND IsPublished=1)" +
						" BEGIN " +
						" INSERT INTO EditableContent (ApplicationId,Url,ContentId,ContentText,IsPublished,IsApproved,EditableContentId,LastModified,LastModifiedUserId)" +
						" SELECT ApplicationId,Url,ContentId,ContentText,1,1,ID,LastModified,LastModifiedUserId FROM EditableContent" +
						" WHERE ApplicationId=@ApplicationId AND ID=@ID" +
						" END";
					sqlcmd.Parameters.Add("@Url", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@ContentId", SqlDbType.VarChar);

					sqlcmd.Parameters["@Url"].Value = Url;
					sqlcmd.Parameters["@ContentId"].Value = ContentID;
					sqlcmd.ExecuteNonQuery();

					trans.Commit();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);
					trans.Rollback();
					return "Error approving content.";
				}
			}

			return "Success";
		}
		#endregion

		#region Publish Content
		[WebMethod]
		public String Publish(Guid ID, string PageUrl)
		{
			if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

			if (!CanApprove()) return "Invalid Permissions.";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction("Publish");

				try
				{
					SqlCommand sqlcmd = new SqlCommand("DELETE FROM EditableContent WHERE ApplicationId=@ApplicationId AND" +
						" Url=(SELECT Url FROM EditableContent WHERE ApplicationId=@ApplicationId AND ID=@ID) AND" +
						" ContentId=(SELECT ContentId FROM EditableContent WHERE ApplicationId=@ApplicationId AND ID=@ID) AND IsPublished=1;", conn, trans);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@ID", SqlDbType.UniqueIdentifier);

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@ID"].Value = ID;
					sqlcmd.ExecuteNonQuery();

					sqlcmd.CommandText = "UPDATE EditableContent SET IsApproved=1 WHERE ApplicationId=@ApplicationId AND ID=@ID;" +
						" INSERT INTO EditableContent (ApplicationId,Url,ContentId,ContentText,Name,LastModified,LastModifiedUserId,IsPublished,IsApproved,EditableContentId)" +
						" SELECT ApplicationId,Url,ContentId,ContentText,Name,LastModified,LastModifiedUserId,1,1,ID FROM EditableContent" +
						" WHERE ApplicationId=@ApplicationId AND ID=@ID;";
					sqlcmd.ExecuteNonQuery();

					trans.Commit();
				}
				catch (Exception e)
				{
					SendErrorEmail.Send(new Uri(Url), e);
					trans.Rollback();
				}
			}

			return "Success";
		}
		#endregion

		#region Delete Content
		[WebMethod]
		public String Delete(Guid ID, string PageUrl)
		{
			if (!string.IsNullOrEmpty(PageUrl) && PageUrl != "undefined") Url = PageUrl;

			if (!CanEdit() && !CanApprove()) return "Invalid Permissions.";

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				SqlCommand sqlcmd = new SqlCommand("DELETE FROM EditableContent WHERE ID=@ID;", conn);
				sqlcmd.Parameters.AddWithValue("@ID", ID);
				sqlcmd.ExecuteNonQuery();
			}

			return "Success";
		}
		#endregion

		#region Send Approval Email
		private string SendApproval(string Content)
		{
			HttpContext curr = HttpContext.Current;
			string emailMsg = "";
			string approvalEmails = "";
			string pageTitle = "";
			string text = HttpUtility.UrlDecode(Content, Encoding.ASCII);

			try
			{
				XmlDocument sitemap = new XmlDocument();
				sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
				XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
				xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
				XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);
				XmlNode parent = n.ParentNode;

				if (n.Attributes["approvalemail"] != null && n.Attributes["approvalemail"].Value != "")
				{
					approvalEmails = n.Attributes["approvalemail"].Value.Replace(";", ",");
				}
				else if (parent != null && parent.Attributes["approvalemail"] != null && parent.Attributes["approvalemail"].Value != "")
				{
					approvalEmails = parent.Attributes["approvalemail"].Value.Replace(";", ",");
				}

				if (n.Attributes["title"] != null && n.Attributes["title"].Value != "")
				{
					pageTitle = n.Attributes["title"].Value;
				}
			}
			catch { return "Success"; }

			if (approvalEmails.Length > 0 && !File.Exists(HttpContext.Current.Application["PHYSICALPATH"] + "emails/cms/approval.html"))
			{
				return "E-mail template missing at 'emails/cms/approval.html'. The body of the e-mail must have a ##EmailMessage## and ##BaseUrl## tag.";
			}

			try
			{
				string url = "http://" + curr.Request.Url.Host + curr.Application["ROOTPATH"] + Url;
				emailMsg += "<strong>Page:</strong> " + pageTitle + "<br />";
				emailMsg += "<strong>Url:</strong> <a href='" + url + "' target='_blank'>" + url + "</a><br/>";
				emailMsg += "<strong>Edited By:</strong> " + Membership.GetUser().UserName + "<br/>";
				emailMsg += "<strong>Modified:</strong> " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "<br/>";
				emailMsg += text;

				string body = File.ReadAllText(HttpContext.Current.Application["PHYSICALPATH"] + "emails/cms/approval.html", Encoding.UTF8);
				body = body.Replace("##BaseUrl##", curr.Request.Url.Host + curr.Application["ROOTPATH"]);
				body = body.Replace("##EmailMessage##", emailMsg);

				SmtpClient client = new SmtpClient();

				MailMessage message = new MailMessage
				{
					From = new MailAddress(ConfigurationManager.AppSettings["CMSApprovalFromEmail"]),
					Subject = ConfigurationManager.AppSettings["MainDomain"] + " | CMS Content Approval | " + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"),
					IsBodyHtml = true,
					Body = body
				};
				message.To.Add(approvalEmails);

				client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["UseSSL"]);
				client.Send(message);
			}
			catch { }

			return "Success";
		}
		#endregion

		#region Check Max Versions
		private bool CheckMaxVersions(string ContentID)
		{
			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				try
				{
					SqlCommand sqlcmd = new SqlCommand("SELECT @COUNT=COUNT(*) FROM EditableContent WHERE ApplicationId=@ApplicationId AND Url=@Url AND ContentId=@ContentId AND IsPublished=0;", conn);
					sqlcmd.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier);
					sqlcmd.Parameters.Add("@Url", SqlDbType.VarChar);
					sqlcmd.Parameters.Add("@ContentID", SqlDbType.VarChar);

					sqlcmd.Parameters.Add("@COUNT", SqlDbType.Int);
					sqlcmd.Parameters["@COUNT"].Direction = ParameterDirection.Output;

					sqlcmd.Parameters["@ApplicationId"].Value = new Guid(ConfigurationManager.AppSettings["AppId"]);
					sqlcmd.Parameters["@Url"].Value = Url;
					sqlcmd.Parameters["@ContentID"].Value = ContentID;
					sqlcmd.ExecuteNonQuery();

					if (Convert.ToInt32(sqlcmd.Parameters["@COUNT"].Value) > 9)
						return true;
				}
				catch
				{
					return false;
				}
			}

			return false;
		}
		#endregion

		#region Can edit this content
		private bool CanEdit()
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

						if (User.IsInRole(role)) return true;
					}
				}
			}
			catch { }

			return false;
		}
		#endregion

		#region Can approve this content
		private bool CanApprove()
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

						if (User.IsInRole(role)) return true;
					}
				}
				else
					return CanEdit();
			}
			catch { }

			return false;
		}
		#endregion
	}
}