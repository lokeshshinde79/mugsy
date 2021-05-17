using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml;
using System.Web.Security;

namespace NovelProjects.Web
{
	[WebService(Namespace = "http://www.novelprojects.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.Web.Script.Services.ScriptService]
	public class SiteManagerService : WebService
	{
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

		#region Load Node Data
		[WebMethod]
		public string LoadNode(string Url, string TimeStamp)
		{
			HttpContext curr = HttpContext.Current;
			XmlDocument sitemap = new XmlDocument();
			sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
			XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
			xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
			XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);

			Node node = new Node
			{
				NavItem = (n.Attributes["navitem"] != null) ? Convert.ToBoolean(n.Attributes["navitem"].Value) : false,
				UseSSL = (n.Attributes["usessl"] != null) ? Convert.ToBoolean(n.Attributes["usessl"].Value) : false,
				Hidden = (n.Attributes["hidden"] != null) ? Convert.ToBoolean(n.Attributes["hidden"].Value) : false,
				AllowChildren = (n.Attributes["allowchildren"] != null) ? Convert.ToBoolean(n.Attributes["allowchildren"].Value) : n.Attributes["url"].Value.EndsWith("/"),
				IsAppFile = (n.Attributes["appfile"] != null) ? Convert.ToBoolean(n.Attributes["appfile"].Value) : n.Attributes["url"].Value.Contains("admin/"),
				Url = n.Attributes["url"].Value,
				Title = (n.Attributes["title"] != null) ? n.Attributes["title"].Value : "",
				Description = (n.Attributes["description"] != null) ? n.Attributes["description"].Value : "",
				ApprovalEmail = (n.Attributes["approvalemail"] != null) ? n.Attributes["approvalemail"].Value : "",
				SEOKeywords = (n.Attributes["seokeywords"] != null) ? curr.Server.HtmlDecode(n.Attributes["seokeywords"].Value) : "",
				SEODescription = (n.Attributes["seodescription"] != null) ? curr.Server.HtmlDecode(n.Attributes["seodescription"].Value) : "",
				AccessRoles = (n.Attributes["roles"] != null) ? n.Attributes["roles"].Value : "",
				EditRoles = (n.Attributes["editroles"] != null) ? n.Attributes["editroles"].Value : "",
				ApproveRoles = (n.Attributes["approveroles"] != null) ? n.Attributes["approveroles"].Value : ""
			};
			node.AllRoles = LoadRoles(node.AccessRoles);
			node.AllRoles2 = LoadRoles(node.EditRoles);
			node.AllRoles3 = LoadRoles(node.ApproveRoles);

			return SerializeObject(node);
		}
		#endregion

		#region Load Roles
		private static string LoadRoles(string roles)
		{
			string retval = "";

      List<string> AllRoles = new List<string>(Roles.GetAllRoles());

      if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["AccessRolesToExclude"]))
      {
        List<string> RolesToExclude =
          new List<string>(ConfigurationManager.AppSettings["AccessRolesToExclude"].Split(new[] { ',' },
                                                                                 StringSplitOptions.RemoveEmptyEntries));

        foreach (string excluderole in RolesToExclude)
        {
          AllRoles.Remove(excluderole.Trim());
        }
      }
			
      bool[] RemoveRoles = new bool[AllRoles.Count];
			string[] PageRoles = roles.Split(',');
			
      for (int i = 0; i < AllRoles.Count; i++)
			{
				foreach (string prole in PageRoles)
				{
					if (prole.Equals(AllRoles[i]))
						RemoveRoles[i] = true;
				}
			}

			for (int i = 0; i < AllRoles.Count; i++)
			{
				if (!RemoveRoles[i])
					retval += AllRoles[i] + ",";
			}

			if (retval.Length > 0)
				retval = retval.Remove(retval.Length - 1);

			return retval;
		}
		#endregion

		#region Save Node
		[WebMethod]
		public string SaveNode(Node node, bool IsAdd, string TemplatePath)
		{
			node.Url = node.Url.ToLower().Replace(" ", "-");
			HttpContext curr = HttpContext.Current;
			XmlDocument sitemap = new XmlDocument();
			sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
			XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
			xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
		
			//Strip out last comma
			if (node.AccessRoles.Length > 0) node.AccessRoles = node.AccessRoles.Remove(node.AccessRoles.Length - 1);
			if (node.EditRoles.Length > 0) node.EditRoles = node.EditRoles.Remove(node.EditRoles.Length - 1);
			if (node.ApproveRoles.Length > 0) node.ApproveRoles = node.ApproveRoles.Remove(node.ApproveRoles.Length - 1);
			if (node.AllowChildren && !node.Url.EndsWith("/")) node.Url = node.Url + "/";
			if (!node.AllowChildren && !node.Url.EndsWith(".aspx")) node.Url = node.Url + ".aspx";

            //get after rendering url
            XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + node.Url + "']", xmlmanager);

            //if (IsAdd && n != null && node.ParentUrl == "")
			if (IsAdd && n != null)
			{
				throw new Exception("Error: Item Already Exists");
			}
			if (IsAdd)
			{
				XmlNode Parent = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + node.ParentUrl.ToLower() + "']", xmlmanager);
				if (node.ParentUrl == "" && sitemap.DocumentElement != null)
					Parent = sitemap.DocumentElement.FirstChild;
				n = sitemap.CreateNode(XmlNodeType.Element, Parent.Name, Parent.NamespaceURI);
				Parent.AppendChild(n);

				if (node.AllowChildren)
					Directory.CreateDirectory(curr.Application["PHYSICALPATH"] + node.Url);
				else
				{
					try
					{
						string text = File.ReadAllText(curr.Application["PHYSICALPATH"] + TemplatePath);
						text = text.Replace("##Name##", node.Description);
						File.WriteAllText(curr.Application["PHYSICALPATH"] + node.Url, text);
					}
					catch
					{
						return "Trouble saving file.";
					}
				}
			}

			UpdateAttribute(sitemap, n, "url", node.Url);
			UpdateAttribute(sitemap, n, "title", node.Title);
			UpdateAttribute(sitemap, n, "navitem", node.NavItem.ToString().ToLower());
			UpdateAttribute(sitemap, n, "usessl", node.UseSSL.ToString().ToLower());
			UpdateAttribute(sitemap, n, "hidden", node.Hidden.ToString().ToLower());
			UpdateAttribute(sitemap, n, "allowchildren", node.AllowChildren.ToString().ToLower());
			UpdateAttribute(sitemap, n, "description", node.Description);
			UpdateAttribute(sitemap, n, "approvalemail", node.ApprovalEmail);
			UpdateAttribute(sitemap, n, "seokeywords", curr.Server.HtmlEncode(node.SEOKeywords.Trim()));
			UpdateAttribute(sitemap, n, "seodescription", curr.Server.HtmlEncode(node.SEODescription.Trim()));
			UpdateAttribute(sitemap, n, "roles", node.AccessRoles);
			UpdateAttribute(sitemap, n, "editroles", node.EditRoles);
			UpdateAttribute(sitemap, n, "approveroles", node.ApproveRoles);

			sitemap.Save(curr.Application["PHYSICALPATH"] + "web.sitemap");

			return SerializeObject(node);
		}

		private static void UpdateAttribute(XmlDocument sitemap, XmlNode n, string attr, string value)
		{
			if (n.Attributes[attr] == null)
			{
				XmlAttribute newAttr = sitemap.CreateAttribute(attr);
				newAttr.Value = value;
				n.Attributes.Append(newAttr);
			}
			else n.Attributes[attr].Value = value;
		}
		#endregion

		#region Add New Node
		[WebMethod]
		public string AddNode()
		{
			string retval = "{";
			retval += " \"AccessRoles\":\"\",";
			retval += " \"EditRoles\":\"Content Admin\",";
			retval += " \"ApproveRoles\":\"Content Admin\",";
			retval += " \"AllRoles\":\"" + LoadRoles("") + "\",";
			retval += " \"AllRoles2\":\"" + LoadRoles("Content Admin") + "\",";
			retval += " \"AllRoles3\":\"" + LoadRoles("Content Admin") + "\"";
			retval += "}";
			return retval;
		}
		#endregion

		#region Delete Node
		[WebMethod]
		public string DeleteNode(string Url)
		{
			HttpContext curr = HttpContext.Current;
			XmlDocument sitemap = new XmlDocument();
			sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
			XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
			xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
			XmlNode n = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);
			n.ParentNode.RemoveChild(n);

			if (Convert.ToBoolean(n.Attributes["allowchildren"].Value))
			{
				if (Directory.Exists(curr.Application["PHYSICALPATH"] + n.Attributes["url"].Value))
					Directory.Delete(curr.Application["PHYSICALPATH"] + n.Attributes["url"].Value, true);
			}
			else
			{
				if (File.Exists(curr.Application["PHYSICALPATH"] + n.Attributes["url"].Value))
					File.Delete(curr.Application["PHYSICALPATH"] + n.Attributes["url"].Value);
			}
			sitemap.Save(curr.Application["PHYSICALPATH"] + "web.sitemap");

			using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings[LoadConnString()].ConnectionString))
			{
				conn.Open();

				SqlCommand sqlcmd = new SqlCommand("DELETE FROM EditableContent WHERE Url=@Url AND ApplicationId=@ApplicationId;", conn);
				sqlcmd.Parameters.AddWithValue("@Url", Url);
				sqlcmd.Parameters.AddWithValue("@ApplicationId", ConfigurationManager.AppSettings["AppId"]);
				sqlcmd.ExecuteNonQuery();
			}

			return "";
		}
		#endregion

		#region Move Node
		[WebMethod]
		public string MoveNode(string Url, string RefUrl, string Type)
		{
			HttpContext curr = HttpContext.Current;
			XmlDocument sitemap = new XmlDocument();
			sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
			XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
			xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");
			XmlNode Node = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + Url.ToLower() + "']", xmlmanager);
			XmlNode RefNode = sitemap.SelectSingleNode("//sm:siteMapNode[@url='" + RefUrl.ToLower() + "']", xmlmanager);

			if (Type == "before")
				RefNode.ParentNode.InsertBefore(Node, RefNode);
			else if (Type == "after")
				RefNode.ParentNode.InsertAfter(Node, RefNode);

			sitemap.Save(curr.Application["PHYSICALPATH"] + "web.sitemap");

			return "Success";
		}
		#endregion

		#region Serialize Node
		private static string SerializeObject(Node node)
		{
			string retval = "{";
			retval += " \"ParentUrl\":\"" + node.ParentUrl + "\",";
			retval += " \"NavItem\":\"" + node.NavItem + "\",";
			retval += " \"UseSSL\":\"" + node.UseSSL + "\",";
			retval += " \"Hidden\":\"" + node.Hidden + "\",";
			retval += " \"AllowChildren\":\"" + node.AllowChildren + "\",";
			retval += " \"IsAppFile\":\"" + node.IsAppFile + "\",";
			retval += " \"Url\":\"" + node.Url + "\",";
			retval += " \"Title\":\"" + node.Title + "\",";
			retval += " \"Description\":\"" + node.Description + "\",";
			retval += " \"ApprovalEmail\":\"" + node.ApprovalEmail + "\",";
			retval += " \"SEOKeywords\":\"" + node.SEOKeywords + "\",";
			retval += " \"SEODescription\":\"" + node.SEODescription + "\",";
			retval += " \"AccessRoles\":\"" + node.AccessRoles + "\",";
			retval += " \"EditRoles\":\"" + node.EditRoles + "\",";
			retval += " \"ApproveRoles\":\"" + node.ApproveRoles + "\",";
			retval += " \"AllRoles\":\"" + node.AllRoles + "\",";
			retval += " \"AllRoles2\":\"" + node.AllRoles2 + "\",";
			retval += " \"AllRoles3\":\"" + node.AllRoles3 + "\"";
			retval += "}";

			return retval;
		}
		#endregion

		#region Node class
		public class Node
		{
			public string ParentUrl;
			public string Url;
			public string Title;
			public string Description;
			public string ApprovalEmail;
			public string SEOKeywords;
			public string SEODescription;
			public bool NavItem;
			public bool UseSSL;
			public bool Hidden;
			public bool AllowChildren;
			public bool IsAppFile;
			public string AccessRoles;
			public string EditRoles;
			public string ApproveRoles;
			public string AllRoles;
			public string AllRoles2;
			public string AllRoles3;
		}
		#endregion
	}
}