using System;
using System.ComponentModel;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Hosting;
using System.Xml;

namespace NovelProjects.Web
{
	[ToolboxData("<{0}:SiteManager runat=server></{0}:SiteManager>")]
	public class SiteManager : WebControl
	{
		#region control properties
		[Bindable(true)]
		[Category("Appearance")]
		[DefaultValue("")]
		[Localizable(true)]
		#endregion

		#region private variables
		private PlaceHolder ph;
		private LiteralControl lt;
		#endregion

		#region public variables
		// Used to set the icons sizes
		public bool UseLargeIcons { get; set; }
		#endregion

		#region Renders contents
		protected override void RenderContents(HtmlTextWriter output)
		{
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
			if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["CMSTemplates"]))
				throw new Exception("Must specify the AppSetting \"CMSTemplates\" in the form \"TemplateDescription|templatePath.aspx,etc\"");

			//Adds Javascript code/files and CSS files to page header
			lt = new LiteralControl();

			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.SiteManager.SiteManager.css") + "' />\n";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.jquery-ui-1.7-core.css") + "' />\n";
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.jquery-ui-1.7-tabs.css") + "' />\n";
			if (UseLargeIcons)
				lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.tree_component_large_icons.css") + "' />\n";
			else
				lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.tree_component.css") + "' />\n";
			// Editable Content Styles
			lt.Text += "<link rel='stylesheet' type='text/css' href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.EditableContent.EditableContent.css") + "' />";
			
			lt.Text += "<script type='text/javascript'>var rootpath='" + HttpContext.Current.Application["ROOTPATH"] + "';</script>\n";
      
      //lt.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery-1.3.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";
      lt.Text += "<script type=\"text/javascript\">try { jQuery.support.boxModel != 'test' } catch (err) { document.write(unescape(\"%3Cscript src='" + Page.ClientScript.GetWebResourceUrl(typeof(EditableContent), "NovelProjects.Web.javascript.jquery-1.4.2.min.js") + "' type='text/javascript'%3E%3C/script%3E\")); }</script>\n";
      
      lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.jquery.tinysort.js") + "' ></script>\n";
			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.css.js") + "' ></script>\n";
			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.tree_component.min.js") + "' ></script>\n";
			lt.Text += "<script type='text/javascript' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.jquery.cookie.min.js") + "' ></script>\n";

			lt.Text += "<script type=\"text/javascript\">$(function(){\n";
			lt.Text += "try { $('.TestTooltip').tooltip(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.jquery.tooltip.min.js") + "'); }\n";
			lt.Text += "try { $(this).npModalDestroy(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.npmodal.min.js") + "'); }\n";
			lt.Text += "try { $('.TestTooltip').tabs();\n\t";
			lt.Text += " $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.SiteManager.SiteManager.min.js") + "');\n";
			lt.Text += "} catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.jquery-ui-1.7.min.js") + "', function(){\n" +
				"$.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.SiteManager.SiteManager.min.js") + "'); }); }\n";
			// Editable Content Imports
			lt.Text += "try { $(this).bgIframe(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.javascript.jquery.bgiframe.min.js") + "'); }\n";
			lt.Text += "try { EnableApprove(); } catch (err) { setTimeout(\"$.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.EditableContent.EditableContent.min.js") + "');\",100); }\n";
			lt.Text += "try { CheckSlider(); } catch (err) { $.getScript('" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.EditableContent.Slider.min.js") + "'); }\n";
			// End Editable Content Imports
			lt.Text += "});</script>\n";

			Page.Header.Controls.Add(lt);

			BuildForm();
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

		#region Load Control

		#region Build Form
		private void BuildForm()
		{
			ph = new PlaceHolder();

			lt = new LiteralControl();
			lt.Text += "<div id='SiteManagerPopup' class='npContainer'>";
			lt.Text += "<div class='npTitle'>";
			lt.Text += "<div class='npClose'><a class='modalClose'><span>close</span></a></div>";
			lt.Text += "<a href='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.SiteManager.Help.pdf") + "' target=_blank><img align='right' alt='Help' title='Help' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.help.gif") + "' border=0 /></a>";
			lt.Text += "<h1>Edit</h1></div>";
			lt.Text += "<div class='npContent'>";
			lt.Text += "<div class='SMTabs npTabs'>";
			lt.Text += "<ul>";
			lt.Text += "<li class='SMPropertiesTab'><a href='#SMProperties'><span>Properties</span></a></li>";
			lt.Text += "<li class='SMAccessTab'><a href='#SMAccess'><span>Access Levels</span></a></li>";
			lt.Text += "<li class='SMEditContentTab'><a href='#SMEditContent'><span>Content</span></a></li>";
			lt.Text += "</ul>";
			lt.Text += "<div id='SMProperties'><table cellpadding='2' cellspacing='0'>";
			ph.Controls.Add(lt);

			BuildPropertiesTab();

			lt = new LiteralControl();
			lt.Text += "</div>";//End Properties Tab
			lt.Text += "<div id='SMEditContent'>";
			ph.Controls.Add(lt);

			BuildEditTab();

			lt = new LiteralControl();
			lt.Text += "</div>";//End Edit Tab
			lt.Text += "<div id='SMAccess'>";
			ph.Controls.Add(lt);

			BuildAccessTab();

			lt = new LiteralControl();
			lt.Text += "</div>";//End Access Tab
			ph.Controls.Add(lt);

			lt = new LiteralControl();
			lt.Text += "<span class='BtnHolder'>";
			lt.Text += "<a href='javascript:void(0);' class='BtnSaveNode' style='margin-left:10px;'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.save.gif") + "' align='absmiddle' border='0' /></a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnSaveNode' style='margin-right:25px;'>Save</a> ";

			lt.Text += "<a href='javascript:void(0);' class='BtnAddFolder' style='display:none;'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.tree.folder-directory-closed.png") + "' align='absmiddle' border='0' /></a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnAddFolder' style='display:none; margin-right:25px;'>Add Folder</a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnAddFile' style='display:none;'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.tree.window-application.png") + "' align='absmiddle' border='0' /></a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnAddFile' style='display:none; margin-right:25px;'>Add File</a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnDeleteNode' style='display:none;'><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.bin.gif") + "' align='absmiddle' border='0' /></a> ";
			lt.Text += "<a href='javascript:void(0);' class='BtnDeleteNode' style='display:none;'>Delete</a> ";
			lt.Text += "</span>";
			ph.Controls.Add(lt);

			lt = new LiteralControl();
			lt.Text += "</div>";//End Tabs
			lt.Text += "</div></div>";
			ph.Controls.Add(lt);

			//Add treeview
			BuildTreeView();
		}
		#endregion

		#region Build Properties Tab
		void BuildPropertiesTab()
		{
			lt = new LiteralControl();
			lt.Text += "<tr class='TemplateRow'><td width='120' class='bold'>Template: </td><td>";
			ph.Controls.Add(lt);

			DropDownList ddl = new DropDownList
			{
				CssClass = "NodeTemplate",
				Width = 232
			};
			foreach (string item in ConfigurationManager.AppSettings["CMSTemplates"].Split(','))
			{
				ddl.Items.Add(new ListItem(item.Substring(0, item.IndexOf("|")).Trim(), item.Substring(item.IndexOf("|") + 1).Trim()));
			}
			ph.Controls.Add(ddl);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' align='absbottom' title='|The template to use when creating this page.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr><td width='120' class='bold'><span class='UrlName'>Location</span>: ";
			ph.Controls.Add(lt);

			RequiredFieldValidator rfv = new RequiredFieldValidator
			{
				ValidationGroup = "SiteManager",
				ControlToValidate = "NodeUrl",
				Display = ValidatorDisplay.Dynamic,
				ErrorMessage = "*"
			};
			ph.Controls.Add(rfv);

			lt = new LiteralControl();
			lt.Text += "</td>";
			lt.Text += "<td><span class='UrlText' style='display:none;'></span>";
			ph.Controls.Add(lt);

			TextBox tb = new TextBox
			{
				CssClass = "NodeUrl",
				ID = "NodeUrl",
				Width = 230
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' align='absbottom' title='|The physical location of the file or folder.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr><td class='bold PageTitleText'>Page Title: ";
			ph.Controls.Add(lt);

			rfv = new RequiredFieldValidator
			{
				ValidationGroup = "SiteManager",
				ControlToValidate = "NodeTitle",
				Display = ValidatorDisplay.Dynamic,
				ErrorMessage = "*"
			};
			ph.Controls.Add(rfv);

			lt = new LiteralControl();
			lt.Text += "</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox
			{
				CssClass = "NodeTitle",
				ID = "NodeTitle",
				Width = 230
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt=\"What's This\" align='absbottom' title=\"|The title of the page displayed to search engines and<br/>by the browser.<br/><br/><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.pagetitle.jpg") + "'/>\" src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='FolderViewRow'><td class='bold'>Page Heading:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox
			{
				CssClass = "NodeDescription",
				Width = 230
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' align='absbottom' title=\"|The heading text used in the page content.<br /><br /><img src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.pageheading.jpg") + "'/>\" src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr><td class='bold'>Approval E-mail:";
			ph.Controls.Add(lt);

			RegularExpressionValidator reg = new RegularExpressionValidator
			{
				ValidationGroup = "SiteManager",
				ControlToValidate = "NodeApprovalEmail",
				Display = ValidatorDisplay.Dynamic,
				ValidationExpression = @"^(([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)([,]|[;])?[\s]*)+$",
				ErrorMessage = "*"
			};
			ph.Controls.Add(reg);

			lt = new LiteralControl();
			lt.Text += "</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox
			{
				CssClass = "NodeApprovalEmail",
				ID = "NodeApprovalEmail",
				Width = 230
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' align='absbottom' title=\"|The e-mail addresses to use when sending approval e-mails. This list should be ',' or ';' separated.\" src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='FolderViewRow'><td class='bold'>SEO Keywords:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox
			{
				CssClass = "NodeSEOKeywords",
				Width = 230
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt=\"What's This\" align='absbottom' title=\"|The keywords used for SEO (Search Engine Optimization). This should be a comma delimited list of approximately 20 keywords or phrases.\" src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='FolderViewRow'><td class='bold' valign='top'>SEO Description:</td><td>";
			ph.Controls.Add(lt);

			tb = new TextBox
			{
				CssClass = "NodeSEODescription",
				Width = 230,
				TextMode = TextBoxMode.MultiLine,
				Rows = 3
			};
			tb.Attributes.Add("AutoComplete", "Off");
			ph.Controls.Add(tb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' align='absbottom' title=\"|The description used for SEO (Search Engine Optimization). The description should be no longer than 150 characters.\" src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr><td class='bold'>Is Nav Item?</td><td>";
			ph.Controls.Add(lt);

			CheckBox cb = new CheckBox {CssClass = "NodeNavItem"};
			ph.Controls.Add(cb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' title='|If checked, display this item in the navigation.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr><td class='bold'>Use SSL?</td><td>";
			ph.Controls.Add(lt);

			cb = new CheckBox {CssClass = "NodeUseSSL"};
			ph.Controls.Add(cb);

			lt = new LiteralControl();
			lt.Text += "<img class='SMtooltip' alt='What\\'s This' title='|If checked, use ssl on this page.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr style='display:none;'><td class='bold'>Is Hidden?</td><td>";
			ph.Controls.Add(lt);

			cb = new CheckBox {CssClass = "NodeHidden"};
			ph.Controls.Add(cb);

			lt = new LiteralControl();
			lt.Text += "</td></tr></table>";
			ph.Controls.Add(lt);

			cb = new CheckBox {CssClass = "NodeAllowChildren"};
			ph.Controls.Add(cb);
		}
		#endregion

		#region Build Access Tab
		void BuildAccessTab()
		{
			lt = new LiteralControl();
			lt.Text += "<table cellpadding='4' cellspacing='0'>";
			lt.Text += "<tr class='ViewRolesRow'><td align='center' valign='bottom' class='bold'>Unassigned roles</td>";
			lt.Text += "<td align='center' valign='bottom' class='bold'>All these roles to view";
			lt.Text += " <img class='SMtooltip' align='absmiddle' alt='What\'s This' title='|If no roles are selected, everyone has access to the page.<br/>Select roles to restrict the access to this page.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='ViewRolesRow'><td align='center'>";
			ph.Controls.Add(lt);

			ListBox lb = new ListBox
			{
				CssClass = "NodeAllRoles",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td><td align='center'>";
			ph.Controls.Add(lt);

			lb = new ListBox
			{
				CssClass = "NodeAccessRoles",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td></tr>";
			ph.Controls.Add(lt);

			lt = new LiteralControl();
			lt.Text += "<tr class='EditRolesRow'><td align='center' valign='bottom' class='bold'>Unassigned roles</td>";
			lt.Text += "<td align='center' valign='bottom' class='bold'>Allow these roles to edit";
			lt.Text += " <img class='SMtooltip' align='absmiddle' alt='What\'s This' title='|If no roles are selected, no one can edit this page.<br/>Select the roles to allow to edit this page.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='EditRolesRow'><td align='center'>";
			ph.Controls.Add(lt);

			lb = new ListBox
			{
				CssClass = "NodeAllRoles2",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td><td align='center'>";
			ph.Controls.Add(lt);

			lb = new ListBox
			{
				CssClass = "NodeEditRoles",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td></tr>";
			ph.Controls.Add(lt);

			lt = new LiteralControl();
			lt.Text += "<tr class='ApproveRolesRow'><td align='center' valign='bottom' class='bold'>Unassigned roles</td>";
			lt.Text += "<td align='center' valign='bottom' class='bold'>Allow these roles to approve";
			lt.Text += " <img class='SMtooltip' align='absmiddle' alt='What\'s This' title='|If no roles are selected, those in the edit role can approve this page.<br/>Select the roles to allow to approve this page.' src='" + Page.ClientScript.GetWebResourceUrl(typeof(SiteManager), "NovelProjects.Web.images.info.gif") + "' />";
			lt.Text += "</td></tr><tr class='ApproveRolesRow'><td align='center'>";
			ph.Controls.Add(lt);

			lb = new ListBox
			{
				CssClass = "NodeAllRoles3",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td><td align='center'>";
			ph.Controls.Add(lt);

			lb = new ListBox
			{
				CssClass = "NodeApproveRoles",
				Width = 180,
				Rows = 6
			};
			ph.Controls.Add(lb);

			lt = new LiteralControl();
			lt.Text += "</td></tr></table>";
			ph.Controls.Add(lt);
		}
		#endregion

		#region Build Edit Tab
		void BuildEditTab()
		{
			lt = new LiteralControl();
			lt.Text += "<div id='SMIframe' style='width:835px; height:449px; overflow:auto;'></div>";
			ph.Controls.Add(lt);
		}
		#endregion

		#region Build Treeview
		private void BuildTreeView()
		{
			HttpContext curr = HttpContext.Current;
			XmlDocument sitemap = new XmlDocument();
			sitemap.Load(curr.Application["PHYSICALPATH"] + "web.sitemap");
			XmlNamespaceManager xmlmanager = new XmlNamespaceManager(sitemap.NameTable);
			xmlmanager.AddNamespace("sm", "http://schemas.microsoft.com/AspNet/SiteMap-File-1.0");

			lt = new LiteralControl();
			lt.Text += "<div id='treestuff' class='tree tree-default'><ul id='sitetreeview' class='ltr'>\n";
			ph.Controls.Add(lt);

			if (sitemap.DocumentElement != null)
			{
				XmlNode node = sitemap.DocumentElement;

				foreach (XmlNode n in node.ChildNodes)
				{
					AddChildren(n, true);
				}
			}

			lt = new LiteralControl();
			lt.Text += "</ul></div>\n";
			ph.Controls.Add(lt);
		}

		//Recursive function for adding child items
		private void AddChildren(XmlNode node, bool root)
		{
			if (node.Attributes["hidden"] != null && Convert.ToBoolean(node.Attributes["hidden"].Value))
				return;

			bool AllowChildren = node.Attributes["allowchildren"] != null ? Convert.ToBoolean(node.Attributes["allowchildren"].Value) : node.Attributes["url"].Value.EndsWith("/");
			bool AppFile = node.Attributes["appfile"] != null && Convert.ToBoolean(node.Attributes["appfile"].Value);

			lt = new LiteralControl();
			lt.Text += "<li class='" + (!AllowChildren ? "file leaf" : (root ? "open root" : "closed")) + "' rel='" +
				(!AllowChildren ? "file" : "folder") + "' deletable='" + AppFile + "'" +
				"><a href='javascript:void(0);' " + (root ? "" : "class='treenode'") + " url='" +
				node.Attributes["url"].Value + "'>" + node.Attributes["title"].Value + "</a>";
			ph.Controls.Add(lt);

			if (node.HasChildNodes)
			{
				lt = new LiteralControl();
				lt.Text += "\n<ul>\n";
				ph.Controls.Add(lt);

				foreach (XmlNode n in node.ChildNodes) AddChildren(n, false);

				lt = new LiteralControl();
				lt.Text += "</ul>\n";
				ph.Controls.Add(lt);
			}
			lt = new LiteralControl();
			lt.Text += "</li>\n";
			ph.Controls.Add(lt);
		}
		#endregion

		#endregion
	}
}