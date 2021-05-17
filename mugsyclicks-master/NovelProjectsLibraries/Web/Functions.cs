using System;
using System.Web;
using System.Web.Configuration;

namespace NovelProjects.Web
{
	public static class Functions
	{
		public static string cookieName = WebConfigurationManager.AppSettings["CookieName"].ToString();
		public static string domain = WebConfigurationManager.AppSettings["MainDomain"].ToString();

		#region Redirect to a Domain
		public static void DoDomainRedirect()
		{
			HttpContext context = HttpContext.Current;
			string[] IgnoreRedirects;
			bool ignore = false;

			IgnoreRedirects = WebConfigurationManager.AppSettings["IgnoreRedirects"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < IgnoreRedirects.Length; i++)
			{
				IgnoreRedirects[i] = IgnoreRedirects[i].Trim();
			}

			foreach (string host in IgnoreRedirects)
			{
				if (context.Request.Url.Host.Equals(host))
				{
					ignore = true;
					break;
				}
			}

			if (!ignore)
			{
				context.Session.Abandon();

				context.Response.StatusCode = 301;// = "301 Moved Permanently";
				context.Response.AddHeader("Location", context.Request.Url.AbsoluteUri.Replace(context.Request.Url.Host, domain).Replace("index.aspx", ""));
				context.Response.End();

				//context.Response.Redirect(context.Request.Url.AbsoluteUri.Replace(context.Request.Url.Host, domain).Replace("index.aspx",""), true);
			}
		}
		#endregion

		#region Switch the connection in and out of SSL
		public static void DoSslSwitch(bool secureTheConnection)
		{
			HttpContext context = HttpContext.Current;
			string[] IgnoreHosts;
			bool ignore = false;

			IgnoreHosts = WebConfigurationManager.AppSettings["IgnoreHosts"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < IgnoreHosts.Length; i++)
			{
				IgnoreHosts[i] = IgnoreHosts[i].Trim();
			}


			foreach (string host in IgnoreHosts)
			{
				if (context.Request.Url.Host.Equals(host))
				{
					ignore = true;
					break;
				}
			}


			if (secureTheConnection)
			{
				if (context.Request.IsSecureConnection)
				{
					return;
				}
				else
				{
					if (!ignore)
					{
						context.Response.Redirect(context.Request.Url.AbsoluteUri.Replace("http://", "https://"));
					}
				}
			}
			else
			{
				if (!context.Request.IsSecureConnection)
				{
					return;
				}
				else
				{
					if (!ignore)
					{
						context.Response.Redirect(context.Request.Url.AbsoluteUri.Replace("https://", "http://"));
					}
				}
			}
		}
		#endregion

		#region Application Global Methods
		public static void DoApplicationStart()
		{
			HttpContext.Current.Application["PHYSICALPATH"] = HttpContext.Current.Server.MapPath("~/");

			string rootPath = HttpContext.Current.Request.ApplicationPath;
			if (rootPath != "/") rootPath += "/";
			HttpContext.Current.Application["ROOTPATH"] = rootPath;
		}

		public static void DoApplicationError()
		{
			SendErrorEmail.Send();
		}
		#endregion
	}
}
