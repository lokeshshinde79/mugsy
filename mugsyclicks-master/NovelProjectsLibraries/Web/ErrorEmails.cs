using System;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Web.Configuration;

namespace NovelProjects.Web
{
	#region ErrorEmails Class
	public static class SendErrorEmail
	{
		public static void Send()
		{
			ErrorEmails e = new ErrorEmails();
			e.SendEmail();
		}

		public static void Send(Exception error)
		{
			ErrorEmails e = new ErrorEmails(error);
			e.SendEmail();
		}

		public static void Send(Uri page, Exception error)
		{
			ErrorEmails e = new ErrorEmails(page, error);
			e.SendEmail();
		}
	}

	public class ErrorEmails
	{
		#region variables

		public string SiteName;
		public string FromEmail;
		public string FromEmailPassword;
		public string ToEmail;
		public bool UseSSL;
		public string[] IgnoreHosts;
		public string[] IgnoreErrors;
		public string[] SupressCodes;

		public Uri ErrorPage;
		public Exception SiteError;
		public Exception BaseError;

		private HttpContext context;

		#endregion

		#region Constructors

		public ErrorEmails()
		{
			DoInitialize();

			ErrorPage = context.Request.Url;
			SiteError = context.Server.GetLastError();
			BaseError = SiteError.GetBaseException();
		}

		public ErrorEmails(Exception error)
		{
			DoInitialize();

			ErrorPage = context.Request.Url;
			SiteError = error;
			BaseError = SiteError.GetBaseException();
		}

		public ErrorEmails(Uri page, Exception error)
		{
			DoInitialize();

			ErrorPage = page;
			SiteError = error;
			BaseError = SiteError.GetBaseException();
		}

		#endregion

		#region Load Variables
		private void DoInitialize()
		{
			context = HttpContext.Current;

			SiteName = WebConfigurationManager.AppSettings["SiteName"];
			FromEmail = WebConfigurationManager.AppSettings["FromEmail"];

			if (WebConfigurationManager.AppSettings["FromEmailPassword"] != null)
			{
				FromEmailPassword = WebConfigurationManager.AppSettings["FromEmailPassword"];
			}

			ToEmail = WebConfigurationManager.AppSettings["ToEmail"];
			UseSSL = Convert.ToBoolean(WebConfigurationManager.AppSettings["UseSSL"]);

			if (WebConfigurationManager.AppSettings["IgnoreHosts"] != null)
			{
				IgnoreHosts = WebConfigurationManager.AppSettings["IgnoreHosts"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < IgnoreHosts.Length; i++)
				{
					IgnoreHosts[i] = IgnoreHosts[i].Trim();
				}
			}
			else
			{
				IgnoreHosts = new string[] { };
			}

			if (WebConfigurationManager.AppSettings["IgnoreErrors"] != null)
			{
				IgnoreErrors = WebConfigurationManager.AppSettings["IgnoreErrors"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < IgnoreErrors.Length; i++)
				{
					IgnoreErrors[i] = IgnoreErrors[i].Trim();
				}
			}
			else
			{
				IgnoreErrors = new string[] { };
			}

			if (WebConfigurationManager.AppSettings["SupressCodes"] != null)
			{
				SupressCodes = WebConfigurationManager.AppSettings["SupressCodes"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				for (int i = 0; i < SupressCodes.Length; i++)
				{
					SupressCodes[i] = SupressCodes[i].Trim();
				}
			}
			else
			{
				SupressCodes = new string[] { };
			}
		}
		#endregion

		#region SendEmail

		public void SendEmail()
		{
			bool ignore = false;

			//
			foreach (string host in IgnoreHosts)
			{
				if (context.Request.Url.Host.Equals(host))
				{
					ignore = true;
					break;
				}
			}

			if (ignore)
			{
				return;
			}

			//
			foreach (string err in IgnoreErrors)
			{
				if (SiteError.ToString().Contains(err))
				{
					ignore = true;
					break;
				}
			}

			if (ignore)
			{
				return;
			}

			//
			foreach (string code in SupressCodes)
			{
				if (SiteError is HttpException)
				{
					HttpException cex = (HttpException)SiteError;
					if (cex.GetHttpCode().ToString().Equals(code))
					{
						ignore = true;
						break;
					}
				}
			}


			/*
			 * Ignore errors from bots
			 */
			if (context.Request.UserAgent != null)
			{
				if (context.Request.UserAgent.ToLower().Contains("bot") ||
				  context.Request.UserAgent.ToLower().Contains("spider") ||
				  context.Request.UserAgent.ToLower().Contains("google") ||
				  context.Request.UserAgent.ToLower().Contains("yahoo"))
				{
					ignore = true;
				}
			}


			/*
			 * Ignore errors from unknown/invalid browsers
			 */
			if (context.Request.Browser.Browser.ToLower().Contains("unknown"))
			{
				ignore = true;
			}


			if (ignore)
			{
				return;
			}


			try
			{
				MailAddress from = new MailAddress(FromEmail);
				MailAddress to = new MailAddress(ToEmail);

				MailMessage message = new MailMessage(from, to);

				message.Subject = "Exception: " + SiteName + " Website Error | " + context.Request.ServerVariables["HTTP_HOST"];

				message.Body = "<html><head>";
				message.Body += "<style type=\"text/css\">\n" +
				  "html, body, table, p { font-size: 11px; font-family:verdana; }" +
				  "label { font-weight: bold; width: 90px; padding-right: 4px; display: block; text-align: right; }" +
				  "h3 { font-size: 14px;  margin-bottom: 4px; margin-top: 10px; border-bottom: 1px solid black; }" +
				  "p { font-size: 16px; font-weight: bold; }" +
				  "pre { margin: 0px; background-color: #dcdcdc; width: 98%; padding: 8px;" +
				  "white-space: pre-wrap;" +
				  "white-space: -moz-pre-wrap;" +
				  "white-space: -pre-wrap;" +
				  "white-space: -o-pre-wrap;" +
				  "word-wrap: break-word;" +
				  "_white-space: pre;}" +
				  "</style>";
				message.Body += "</head><body>";

				message.Body += "<p>" + SiteError.GetType() + " - " + BaseError.Message + "</p>\n";

				message.Body += "<h3>Summary</h3>\n";
				message.Body += "<table>\n";
				message.Body += "<tr><td valign=\"top\"><label>Server: </label></td><td>" + Environment.MachineName + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Exception Time: </label></td><td>" + DateTime.Now.ToString("M/d/yyyy hh:mm:ss tt") + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Exception: </label></td><td>" + BaseError.Message + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Method: </label></td><td>" + context.Request.ServerVariables["REQUEST_METHOD"] + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>User Agent: </label></td><td>" + context.Request.UserAgent + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Browser: </label></td><td>" + context.Request.Browser.Browser + " " + context.Request.Browser.Version + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Client IP: </label></td><td>" + context.Request.ServerVariables["REMOTE_ADDR"] + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Authenticated User: </label></td><td>" + context.Request.ServerVariables["AUTH_USER"] + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Referrer: </label></td><td>" + context.Request.ServerVariables["HTTP_REFERER"] + "</td></tr>";
				message.Body += "<tr><td valign=\"top\"><label>Request: </label></td><td>" + ErrorPage + "</td></tr>";
				message.Body += "</table>\n";

				message.Body += "\n<h3>Exception</h3>\n";
				message.Body += "<table>\n";
				message.Body += "<tr><td valign=\"top\"><label>Source: </label></td><td>" + SiteError.Source + "</td></tr>";
				if (SiteError.TargetSite != null)
				{
					message.Body += "<tr><td valign=\"top\"><label>Target Site: </label></td><td>" + SiteError.TargetSite + "</td></tr>";
				}
				if (SiteError.InnerException != null)
				{
					message.Body += "<tr><td valign=\"top\"><label>Inner Exception: </label></td><td>" + SiteError.InnerException + "</td></tr>";
				}
				message.Body += "</table>\n";

				message.Body += "\n<h3>Stack Trace</h3>\n";
				message.Body += "<pre>\n";
				message.Body += SiteError.StackTrace;
				message.Body += "</pre>\n";
				message.Body += "</body></html>\n";

				message.IsBodyHtml = true;

				SmtpClient client = new SmtpClient();

				if (UseSSL)
				{
					client.EnableSsl = UseSSL;
				}

				if (FromEmailPassword != null)
				{
					client.Credentials = new NetworkCredential(FromEmail, FromEmailPassword);
				}

				client.Send(message);
			}
			catch (Exception e) { Console.Write(e.Message); }
		}

		#endregion
	}
	#endregion
}
